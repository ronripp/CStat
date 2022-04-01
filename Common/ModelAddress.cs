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
                return 0;

            if (!bFoundAdrId && bHasStreet && (bHasZip || bHasCS))
            {
                bool bTrySCS = false;

                // Try to find an existing address by looping here up to 2 times.
                do
                {
                    int ld, MinLD = 10000;
                    List<Address> adrList = new List<Address>();
                    if (bHasZip && !bTrySCS)
                    {
                        var adrL = from adr in ce.Address.AsNoTracking()
                                   where (adr.ZipCode == inAdr.ZipCode)
                                   select adr;
                        foreach (Address a in adrL)
                        {
                            adrList.Add(a);
                        }
                    }
                    else
                    {
                        var adrL = from adr in ce.Address.AsNoTracking()
                                   where (adr.Town == inAdr.Town) && (adr.State == inAdr.State)
                                   select adr;
                        foreach (Address a in adrL)
                        {
                            adrList.Add(a);
                        }
                    }

                    if (adrList.Count() > 0)
                    {
                        Address MinAdr = new Address();
                        foreach (Address a in adrList)
                        {
                            ld = LevenshteinDistance.Compute(a.Street, inAdr.Street);
                            if (ld < MinLD)
                            {
                                MinAdr = a;
                                MinLD = ld;
                            }
                        }

                        int si = MinAdr.Street.IndexOf(' ');
                        if ((MinLD < 2) && ((si > 1) && ((si + 3) < MinAdr.Street.Length) && inAdr.Street.StartsWith(MinAdr.Street.Substring(0, si + 3))))
                        {
                            // Found matching address. Just set address id
                            FoundAdrId = newAdr.Id = MinAdr.Id;
                            foundAdr = MinAdr;
                            bFoundAdrId = true;
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

            AddressMgr amgr = new AddressMgr(ce);
            bValidAddress = amgr.Validate(ref newAdr);
            if (!bFoundAdrId && !bValidAddress)
                return 0;

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
                        {
                            newAdr = Address.Merge(foundAdr, newAdr);
                            try
                            {
                                ce.Address.Attach(newAdr);
                                ce.Entry(newAdr).State = EntityState.Modified;
                                ce.SaveChanges();
                                return newAdr.Id;
                            }
                            catch
                            {
                                return 0;
                            }
                        }
                        return FoundAdrId;
                    }
                }
            }
            catch
            {
            }
            return 0;
        }
    }
}