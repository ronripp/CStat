using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;

namespace CStat.Models
{
    public partial class Address
    {
        public enum AddressStatus
        {
            AdrStat_RTS = 0x10000000,
            AdrStat_LastMMYY = 0x00001000,
            AdrStat_MM_MASK = 0x00000F00,
            AdrStat_YY_MASK = 0x000000FF
        }

        public static int UpdateAddress(Models.CStatContext ce, Address inAdr)
        {
            if (inAdr == null)
                return 0;

            Address newAdr = new Address();
            Address foundAdr = null;
            bool bValidAddress = false;
            bool bHasZip = !string.IsNullOrEmpty(inAdr.ZipCode);
            if (bHasZip)
                inAdr.ZipCode = inAdr.ZipCode.Replace("-", string.Empty);
            bool bHasStreet = !string.IsNullOrEmpty(inAdr.Street);
            bool bHasCS = !string.IsNullOrEmpty(inAdr.Town) && !string.IsNullOrEmpty(inAdr.State);
            if (bHasStreet)
                inAdr.Street = Models.Person.CleanStreet(inAdr.Street);
            else
                inAdr.Street = "";

            bool bFoundAdrId = inAdr.Id > 0;
            int FoundAdrId = bFoundAdrId ? inAdr.Id : 0;
            if (!bFoundAdrId && (!bHasStreet || !bHasCS))
                return 0; // nothing can be done.

            if (bFoundAdrId && (!bHasStreet || !bHasCS))
                return FoundAdrId; // not going to attempt to update address

            if (!bFoundAdrId && bHasStreet && (bHasZip || bHasCS))
            {
                bool bTrySCS = false;

                // Try to find an existing MATCHING address by looping here up to 2 times.
                do
                {
                    int ld;
                    List<Address> adrList = new List<Address>();
                    if (bHasZip && !bTrySCS)
                    {
                        if (inAdr.ZipCode != "11111")
                        {
                            bool IsTrim = inAdr.ZipCode.Length > 5;
                            var TrimZip = IsTrim ? inAdr.ZipCode.Substring(0, 5) : inAdr.ZipCode;
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.ZipCode.StartsWith(TrimZip))
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                if (IsTrim && (a.ZipCode.Length > 5) && (a.ZipCode != inAdr.ZipCode))
                                    continue;
                                if (!bHasCS || cm.EQ(a.State, inAdr.State))
                                    adrList.Add(a);
                            }
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(inAdr.Town) && !inAdr.Town.Contains("<missing>"))
                        {
                            var ltown = inAdr.Town.ToLower();
                            var lstate = inAdr.State.ToLower();
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.Town.ToLower() == ltown) && (adr.State.ToLower() == lstate)
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                adrList.Add(a);
                            }
                        }
                    }

                    if (adrList.Count() > 0)
                    {
                        foreach (Address a in adrList)
                        {
                            ld = (a.Street.Contains("<missing>") || a.Town.Contains("<missing>")) ? 10000 : LevenshteinDistance.Compute(a.Street, inAdr.Street);
                            if (ld < 2)
                            {
                                int si = a.Street.IndexOf(' ');
                                if (String.Equals(a.Street, inAdr.Street, StringComparison.OrdinalIgnoreCase) || ((si > 1) && ((si + 4) < a.Street.Length) && inAdr.Street.StartsWith(a.Street.Substring(0, si + 4))))
                                {
                                    // Found matching address. Just set address id
                                    FoundAdrId = newAdr.Id = a.Id;
                                    foundAdr = a;
                                    bFoundAdrId = true;
                                }
                            }
                        }
                    }

                    if (bTrySCS)
                        break; // We already looped. Break here.

                    if (!bFoundAdrId && bHasZip)
                    {
                        // Zip may be bad or changed try to match by street, city, state
                        bTrySCS = true;
                    }
                    else
                        break; // Do not loop because there is nothing to retry.
                }
                while (!bFoundAdrId);
            }

            //***************************
            //*** Add Address fields ****
            //***************************
            if (bHasStreet)
                newAdr.Street = inAdr.Street;
            if (bHasCS)
            {
                newAdr.Town = inAdr.Town;
                newAdr.State = inAdr.State;
            }

            if (bHasZip)
                newAdr.ZipCode = inAdr.ZipCode;

            if (!string.IsNullOrEmpty(inAdr.Phone))
                newAdr.Phone = inAdr.Phone;

            if (!string.IsNullOrEmpty(inAdr.Fax))
                newAdr.Fax = inAdr.Fax;

            newAdr.Status = inAdr.Status; // TBD : May want to add more logic for status.

            //AddressMgr amgr = new AddressMgr(ce);
            bValidAddress = AddressMgr.Validate(ref newAdr);
            if (!bFoundAdrId && !bValidAddress)
                return 0; // nothing can be done

            // Add / Update Address
            try
            {
                if (bValidAddress)
                {
                    if (!bFoundAdrId)
                    {
                        // Add new, valid address and set Person.Address_id
                        try
                        {
                            ce.Address.Add(newAdr);
                            if (ce.SaveChanges() < 1)
                            {
                                return 0;
                            }
                            if (newAdr.Id != 0)
                                return newAdr.Id;
                        }
                        catch
                        {
                        }
                        return 0;
                    }
                    else
                    {
                        // Make sure address fields are updated.
                        if (foundAdr != null)
                            newAdr = Address.Merge(foundAdr, newAdr);
                        else
                            newAdr.Id = FoundAdrId;
                        try
                        {
                            ce.Address.Attach(newAdr);
                            ce.Entry(newAdr).State = EntityState.Modified;
                            ce.SaveChanges();
                            ce.Entry(newAdr).State = EntityState.Detached;
                            return newAdr.Id;
                        }
                        catch (Exception e)
                        {
                            _ = e;
                            return 0;
                        }
                    }
                }
            }
            catch
            {
            }
            return 0;
        }

        public static string StripZip(string raw)
        {
            var numStr = raw.Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace(" ", "");
            return numStr;
        }

        public static string FixZip(string raw)
        {
            var numStr = raw.Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace(" ", "");
            if ((numStr.Length > 5))
                return numStr.Substring(0, 5) + "-" + numStr.Substring(5);
            return numStr;
        }
        public static void DetachAllEntities(CStatContext ctx)
        {
            var undetachedEntriesCopy = ctx.ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in undetachedEntriesCopy)
                entry.State = EntityState.Detached;
        }

        public static string GetAddressStr(Address adr, bool enclose)
        {
            string astr = "";
            if ((adr != null) && (adr.Status & (int)AddressStatus.AdrStat_RTS) == 0)
            {
                if (adr == null)
                    return astr;
                if (!String.IsNullOrEmpty(adr.Street))
                    astr += (adr.Street + " ");
                if (!String.IsNullOrEmpty(adr.Town))
                    astr += (adr.Town + ", ");
                if (!String.IsNullOrEmpty(adr.State))
                    astr += (adr.State + " ");
                if (!String.IsNullOrEmpty(adr.ZipCode))
                    astr += (FixZip(adr.ZipCode));
                return enclose ? "{" +  astr.Trim() + "}" : astr.Trim();
            }
            return astr;
        }

        public static bool SameAddress(Address adr1, Address adr2)
        {
            if ((adr1 == null) || (adr2 == null))
                return false;

            //Debug.WriteLine("*** SA : [{0}]-[{1}]  [{2}]-[{3}]", adr1.Town, adr2.Town, GetStateAbbr(adr1.State), GetStateAbbr(adr2.State));

            if (!string.Equals(GetStateAbbr(adr1.State), GetStateAbbr(adr2.State), StringComparison.OrdinalIgnoreCase))
                return false;

            if (!string.Equals(adr1.Town, adr2.Town, StringComparison.OrdinalIgnoreCase))
                return false;

            //if ((adr1?.Street.IndexOf("1134") ?? -1) != -1)
            //    Debug.WriteLine("1134");
            //if ((adr2?.Street.IndexOf("1134") ?? -1) != -1)
            //    Debug.WriteLine("1134");
            var res = IsStreetSimilar(adr1.Street, adr2.Street);

            //Debug.WriteLine("CA : [{0}] [{1}] = {2}", adr1.Street, adr2.Street, res);
            return res;
        }

    public static string CleanStreet(string s)
    {
        var str = CleanStreetEnd(s.Replace("Mt", "Mount", StringComparison.OrdinalIgnoreCase)
                                      .Replace("Mt.", "Mount", StringComparison.OrdinalIgnoreCase)
                                      .Replace("PO Box", "~@~", StringComparison.OrdinalIgnoreCase)
                                      .Replace("PO", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("P O", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("P.O. Box", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("PO. Box", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("PO.Box", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("P.O.Box", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("POBox", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("P.O.", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("~@~", "PO Box ", StringComparison.OrdinalIgnoreCase)
                                      .Replace(" S ", " South ", StringComparison.OrdinalIgnoreCase).Replace(" S.", " South ", StringComparison.OrdinalIgnoreCase)
                                      .Replace(" N ", " North ", StringComparison.OrdinalIgnoreCase).Replace(" N.", " North ", StringComparison.OrdinalIgnoreCase)
                                      .Replace(" E ", " East ", StringComparison.OrdinalIgnoreCase).Replace(" E.", " East ", StringComparison.OrdinalIgnoreCase)
                                      .Replace(" W ", " West ", StringComparison.OrdinalIgnoreCase).Replace(" W.", " West ", StringComparison.OrdinalIgnoreCase)
                                      .Replace("  ", " ").Replace("  ", " ").Trim());

        if (str.IndexOf("PO Box") != -1)
        {
            str = str.Replace("#", " ");
        }
        else
        {
            str = str.Replace("Apt.", "~@~", StringComparison.OrdinalIgnoreCase)
                     .Replace("Apt", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("Apt #", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("Apt#", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("Suite #", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("Suite#", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("Suite", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("#", " Apt. ", StringComparison.OrdinalIgnoreCase)
                     .Replace("~@~", "Apt.", StringComparison.OrdinalIgnoreCase);
        }
        return str.Replace("  ", " ").Replace("  ", " ").Trim();
    }

    static public String CleanStreetEnd(String oldStr)
    {
        String newStr = oldStr.Trim();
        newStr = newStr.Replace("  ", " ");
        String upStr = newStr.ToUpper();
        int len = newStr.Length;
        if (upStr.EndsWith(" DRIVE"))
            newStr = newStr.Remove(len - 5) + "Dr.";
        else if (upStr.EndsWith(" DR"))
            newStr = newStr.Remove(len - 2) + "Dr.";
        else if (upStr.EndsWith(" PLACE"))
            newStr = newStr.Remove(len - 5) + "Pl.";
        else if (upStr.EndsWith(" PL"))
            newStr = newStr.Remove(len - 2) + "Pl.";
        else if (upStr.EndsWith(" AVENUE"))
            newStr = newStr.Remove(len - 6) + "Ave.";
        else if (upStr.EndsWith(" AVE"))
            newStr = newStr.Remove(len - 3) + "Ave.";
        else if (upStr.EndsWith(" AV"))
            newStr = newStr.Remove(len - 2) + "Ave.";
        else if (upStr.EndsWith(" STREET"))
            newStr = newStr.Remove(len - 6) + "St.";
        else if (upStr.EndsWith(" ST"))
            newStr = newStr.Remove(len - 2) + "St.";
        else if (upStr.EndsWith(" ROAD"))
            newStr = newStr.Remove(len - 4) + "Rd.";
        else if (upStr.EndsWith(" RD"))
            newStr = newStr.Remove(len - 2) + "Rd.";
        else if (upStr.EndsWith(" LANE"))
            newStr = newStr.Remove(len - 4) + "Ln.";
        else if (upStr.EndsWith(" LN"))
            newStr = newStr.Remove(len - 2) + "Ln.";
        else if (upStr.EndsWith(" COURT"))
            newStr = newStr.Remove(len - 5) + "Ct.";
        else if (upStr.EndsWith(" CT"))
            newStr = newStr.Remove(len - 2) + "Ct.";
        else if (upStr.EndsWith(" TRAIL"))
            newStr = newStr.Remove(len - 5) + "Tr.";
        else if (upStr.EndsWith(" TR"))
            newStr = newStr.Remove(len - 2) + "Tr.";
        else if (upStr.EndsWith(" TERRACE"))
            newStr = newStr.Remove(len - 7) + "Ter.";
        else if (upStr.EndsWith(" TER"))
            newStr = newStr.Remove(len - 3) + "Ter.";
        else if (upStr.EndsWith(" TERR"))
            newStr = newStr.Remove(len - 4) + "Ter.";
        else if (upStr.EndsWith(" BOULEVARD"))
            newStr = newStr.Remove(len - 9) + "Blvd.";
        else if (upStr.EndsWith(" BLVD"))
            newStr = newStr.Remove(len - 4) + "Blvd.";
        else if (upStr.EndsWith(" BLV"))
            newStr = newStr.Remove(len - 3) + "Blvd.";
        else if (upStr.EndsWith(" BL"))
            newStr = newStr.Remove(len - 2) + "Blvd.";
        else if (upStr.EndsWith(" HIGHWAY"))
            newStr = newStr.Remove(len - 7) + "Hwy.";
        else if (upStr.EndsWith(" WY"))
            newStr = newStr.Remove(len - 3) + "Way";
        else if (upStr.EndsWith(" COMMONS"))
            newStr = newStr.Remove(len - 7) + "Cmns.";
        else if (upStr.EndsWith(" CMNS"))
            newStr = newStr.Remove(len - 7) + "Cmns.";
        return newStr;
    }

    public static string AddNumberHyphen(string s)
        {
            if ((s?.Length ?? 0) == 0)
                return "";

            char[] sa = s.Trim().ToCharArray();

            bool InNum1 = false;
            bool InNum2 = false;
            int spaceIdx = -1;
            for (int i=0; i < sa.Length; ++i)
            {
                var c = sa[i];
                if ((c >= '0') && (c <= '9'))
                {
                    if (!InNum1 && (spaceIdx == -1))
                        InNum1 = true;
                    else if (!InNum2 && (spaceIdx != -1))
                        InNum2 = true;
                }
                else
                {
                    if (c != ' ')
                        return s;
                    
                    if (InNum1 && (spaceIdx == -1))
                        spaceIdx = i;

                    if (InNum2)
                    {
                        sa[spaceIdx] = '-';
                        return new string(sa);
                    }
                }
            }
            return s;
        }

        public static bool IsStreetSimilar(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return false;

            if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
                return true;

            s1 = CleanStreet(s1);
            s2 = CleanStreet(s2);
            s1 = AddNumberHyphen(s1);
            s2 = AddNumberHyphen(s2);

            if (s1.Equals(s2, StringComparison.OrdinalIgnoreCase))
                return true;

            var flds1 = s1.Split(' ');
            var flds2 = s2.Split(' ');
            var f1Len = flds1.Length;
            var f2Len = flds2.Length;

            if ((f1Len < 2) || (f2Len < 2))
                return false;

            if (!flds1[0].Equals(flds2[0], StringComparison.OrdinalIgnoreCase))
                return false;

            var s1Match = (flds1[1].Equals(flds2[1], StringComparison.OrdinalIgnoreCase) ||
                            (flds1[1].StartsWith(flds2[1], StringComparison.OrdinalIgnoreCase) ||
                              flds2[1].StartsWith(flds1[1], StringComparison.OrdinalIgnoreCase)) && (LevenshteinDistance.Compute(flds1[1], flds2[1]) < 2));

            if (!s1Match)
                return false;


            int Apt1Idx = flds1.ToList().FindIndex(s => s.ToLower().StartsWith("Apt."));
            int Apt2Idx = flds2.ToList().FindIndex(s => s.ToLower().StartsWith("Apt."));
            bool AptMatch = (Apt1Idx == -1) && (Apt2Idx == -1);
            if ((Apt1Idx > 1) && (Apt2Idx > 1))
            {
                var a1 = (flds1[Apt1Idx] + ((Apt1Idx + 1 < f1Len) ? flds1[Apt1Idx + 1] : "")).Replace(".", "").Replace("#", "");
                var a2 = (flds2[Apt2Idx] + ((Apt2Idx + 1 < f2Len) ? flds2[Apt2Idx + 1] : "")).Replace(".", "").Replace("#", "");
                AptMatch = a1 == a2;
            }

            if ((f1Len < 3) || (f2Len < 3))
                return s1Match && AptMatch;

            var s12 = flds1[2];
            var s22 = flds2[2];

            return s12.Equals(s22, StringComparison.OrdinalIgnoreCase) ||
                   ((s12.StartsWith(s22, StringComparison.OrdinalIgnoreCase) || s22.StartsWith(s12, StringComparison.OrdinalIgnoreCase)) && (LevenshteinDistance.Compute(s12, s22) < 2));
        }

    }
}