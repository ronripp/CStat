using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CStat.Models
{
    public partial class Address
    {
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
    }
}