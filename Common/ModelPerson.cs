using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CStat.Common;
using CStat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace CStat.Models
{
    public partial class Person
    {
        enum PersonStatus
        {
            NotBaptized = 0x0000000000000010, Baptized =0x0000000000000020
        }

        //public object PersonStatus { get; private set; }

        public enum TitleRoles
        {
            Undefined = 0, Blds_NGnds = 0x1, Dean = 0x2, Counselor = 0x4, SWAT = 0x8, Worship = 0x10, Health_Dir = 0x20, nurse = 0x40, Manager = 0x80,
            Cook = 0x100, FFE = 0x200, Trustee = 0x400, President = 0x800, Treasurer = 0x1000, Secretary = 0x2000, Vice_Pres = 0x4000, Memb_at_Lg = 0x8000, Security = 0x10000, Unavailable = 0x20000, Admin = 0x40000, POC = 0x80000
        };

        public enum eGender
        {
            M = (int)(uint)(byte)'M',
            F = (int)(uint)(byte)'F'
        };

        public enum eSkills
        {
            Carpentry = 0x1,
            Plumbing = 0x2,
            Roofing = 0x4,
            Cook = 0x8,
            Nurse = 0x10,
            SWAT = 0x20,
            Manager = 0x40,
            Painter = 0x80,
            Minister = 0x100,
            Dean = 0x200,
            Accountant = 0x400,
            Electricn = 0x800,
            Computer = 0x1000,
            Kitch_Help = 0x2000,
            Counselor = 0x4000,
            Gardening = 0x8000,
            Tree_Cut = 0x10000,
            Worship = 0x20000,
            Worker = 0x40000,
            Mason = 0x80000,
            Constructn = 0x100000,
            Septic = 0x200000
        };

        public enum eSkillsAbbr
        {
            CRP = 0x1,
            PLB = 0x2,
            ROF = 0x4,
            COK = 0x8,
            HLD = 0x10,
            SWT = 0x20,
            CMG = 0x40,
            PNT = 0x80,
            MNS = 0x100,
            DEN = 0x200,
            ACC = 0x400,
            ELE = 0x800,
            CMP = 0x1000,
            KTH = 0x2000,
            CNS = 0x4000,
            GAR = 0x8000,
            TRR = 0x10000,
            WRS = 0x20000,
            WRK = 0x40000,
            MSN = 0x80000,
            CON = 0x100000,
            SPT = 0x200000
        };

        public enum ePA
        {
            pid = 0,
            FirstName,
            LastName,
            Alias,
            DOB,
            Gender,
            Status,
            SSNum,
            Address_id,
            PG1_Person_id,
            PG2_Person_id,
            Church_id,
            SkillSets,
            CellPhone,
            EMail,
            ContactPref,
            Notes,
            Roles,
            aid,
            Street,
            Town,
            State,
            ZipCode,
            Phone,
            Fax,
            Country,
            WebSite
        };

        public Person ShallowCopy()
        {
            return (Person)this.MemberwiseClone();
        }

        public string GetShortName()
        {
            if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            {
                return FirstName + " " + LastName.Substring(0, 1).ToUpper() + ".";
            }
            if (!string.IsNullOrEmpty(Email))
            {
                var idx = Email.IndexOf("@");
                return (idx != -1) ? Email.Substring(0, idx) : Email;
            }
            return "Person #" + Id.ToString();
        }

        public static int? PersonIdFromExactName(CStatContext context, string name, string defaultLast="<unknown>")
        {
            if (string.IsNullOrEmpty(name))
                return null;

            string fn, ln;

            var parts = name.Trim().Split(" ");
            switch (parts.Length)
            {
                case 0:
                    return null;
                case 1:
                    {
                        fn = parts[0].ToLower();
                        var person = context.Person.Where(p => ((p.FirstName.ToLower() == fn) || (p.Alias.ToLower() == fn)) && (p.LastName.ToLower() == defaultLast)).FirstOrDefault();
                        if (person == default(Person))
                        {
                            Person pers = new Person();
                            pers.FirstName = parts[0];
                            pers.LastName = defaultLast;
                            try
                            {
                                context.Person.Add(pers);
                                context.SaveChanges();
                            }
                            catch
                            {
                                return null;
                            }
                            return pers.Id;
                        }
                        else
                            return person.Id;
                    }
                case 2:
                default:
                    {
                        fn = parts[0].ToLower();
                        ln = parts[parts.Length-1].ToLower();
                        var person = context.Person.Where(p => ((p.FirstName.ToLower() == fn) || (p.Alias.ToLower() == fn)) && (p.LastName.ToLower() == ln)).FirstOrDefault();
                        if (person == default(Person))
                        {
                            Person pers = new Person();
                            pers.FirstName = parts[0];
                            pers.LastName = parts[parts.Length - 1];
                            try
                            {
                                context.Person.Add(pers);
                                context.SaveChanges();
                            }
                            catch
                            {
                                return null;
                            }
                            return pers.Id;
                        }
                        else
                            return person.Id;
                    }
            }       
        }

        public static List<Person> PeopleFromNameHint(CStatContext context, string hint)
        {
            string fn, ln;

            var parts = hint.Trim().Split(" ");
            switch (parts.Length)
            {
                case 0:
                    return new List<Person>();
                case 1:
                    fn = parts[0];
                    var fnList = context.Person.Where(p => p.FirstName.StartsWith(fn) || p.Alias.StartsWith(fn)).ToList();
                    var lnList = context.Person.Where(p => p.LastName.StartsWith(fn)).ToList();
                    return fnList.Concat(lnList).ToList();
                case 2:
                default:
                    fn = parts[0];
                    ln = parts[parts.Length-1];
                    return context.Person.Where(p => (p.FirstName.StartsWith(fn) || p.Alias.StartsWith(fn)) && p.LastName.StartsWith(ln)).ToList();
            }                
        }

        public static int AddPersonFromNameHint(CStatContext context, string hint)
        {
            if (string.IsNullOrEmpty(hint))
                return -1;

            var names = hint.Split(" ");
            var NumStrs = names.Length;
            if (NumStrs < 2)
                return -1;
            try
            {
                Person pers = new Person();
                pers.FirstName = names[0].Trim();
                pers.LastName = names[NumStrs-1].Trim();
                context.Person.Add(pers);
                context.SaveChanges();
                return pers.Id;
            }
            catch
            {
                return -1;
            }
        }

        public static string FindPeople(CStatContext entities, string id)
        {
            String target = "Lookup Person:";
            int lookup_index = id.IndexOf(target);
            if (lookup_index >= 0)
            {
                String pidStr = id.Substring(lookup_index + target.Length);
                int pid = Int32.Parse(pidStr);

                //var PList = from p in entities.People
                //            where p.id == pid
                //            join a in entities.Addresses on
                //            p.Address_id equals a.id
                //            join c in entities.Churches on
                //            p.Church_id equals c.id into pc
                //            from pcn in pc.DefaultIfEmpty()
                //            select p;

                //var PList = from p in entities.Person
                //            where p.Id == pid
                //            join a in entities.Address on
                //            p.AddressId equals a.Id into pa
                //            from a in pa.DefaultIfEmpty()
                //            join c in entities.Church on
                //            p.ChurchId equals c.Id into pc
                //            from c in pc.DefaultIfEmpty()
                //            from pcn in pc.DefaultIfEmpty()
                //            select p;

                var PList = from _p in entities.Person
                            where _p.Id == pid
                            join _a in entities.Address on
                            _p.AddressId equals _a.Id into pa
                            from _a in pa.DefaultIfEmpty()
                            select new { p = _p, a = _a };

                var pgObj = new PG();
                var MPList = new List<MinPerson>();

                foreach (var pa in PList)
                {
                    pa.p.FirstName = PersonMgr.MakeFirstName(pa.p);
                    pa.p.LastName = PersonMgr.MakeLastName(pa.p);

                    // TEST pa.p.PG1_Person_id = 3;
                    // TEST pa.p.PG2_Person_id = 4;

                    if (pa.p.AddressId != null)
                        pgObj.Adr_id = (int)pa.p.AddressId;

                    var m = new MinPerson();
                    m.FirstName = pa.p.FirstName;
                    m.LastName = pa.p.LastName;
                    m.id = pa.p.Id;
                    m.Alias = pa.p.Alias;
                    m.DOB = pa.p.Dob;
                    m.Gender = pa.p.Gender;
                    m.Status = pa.p.Status;
                    m.SSNum = pa.p.Ssnum;
                    m.Address_id = pa.p.AddressId;
                    m.PG1_Person_id = pa.p.Pg1PersonId;
                    m.PG2_Person_id = pa.p.Pg2PersonId;
                    m.Church_id = pa.p.ChurchId;
                    m.SkillSets = pa.p.SkillSets;
                    m.Roles = pa.p.Roles.HasValue ? pa.p.Roles.Value : 0;
                    m.CellPhone = pa.p.CellPhone;
                    m.EMail = pa.p.Email;
                    m.ContactPref = pa.p.ContactPref;
                    m.Notes = pa.p.Notes;
                    if (pa.p.Address != null)
                    {
                        m.Address.id = pa.a.Id;
                        m.Address.Street = pa.a.Street;
                        m.Address.Town = pa.a.Town;
                        m.Address.State = pa.a.State;
                        m.Address.ZipCode = pa.a.ZipCode;
                        m.Address.Phone = pa.a.Phone;
                        m.Address.Fax = pa.a.Fax;
                        m.Address.Country = pa.a.Country;
                        m.Address.WebSite = pa.a.WebSite;
                    }
                    MPList.Add(m);
                }

                // Resolve parents
                foreach (var m in MPList)
                {
                    String PG1Str = null, PG2Str = null;
                    int PG1_id = 0, PG2_id = 0;

                    Person p1 = entities.Person.FirstOrDefault(p => p.Id == m.PG1_Person_id);
                    if (p1 != null)
                    {
                        p1.FirstName = PersonMgr.MakeFirstName(p1);
                        p1.LastName = PersonMgr.MakeLastName(p1);
                        PG1Str = p1.FirstName + " " + p1.LastName;
                        PG1_id = p1.Id;
                    }

                    Person p2 = entities.Person.FirstOrDefault(p => p.Id == m.PG2_Person_id);
                    if (p2 != null)
                    {
                        p2.FirstName = PersonMgr.MakeFirstName(p2);
                        p2.LastName = PersonMgr.MakeLastName(p2);
                        PG2Str = p2.FirstName + " " + p2.LastName;
                        PG2_id = p2.Id;
                    }

                    if ((PG1Str != null) || (PG2Str != null))
                    {
                        if (PG1Str != null)
                        {
                            pgObj.PG1Name = PG1Str;
                            pgObj.PG1_id = PG1_id;
                        }
                        if (PG2Str != null)
                        {
                            pgObj.PG2Name = PG2Str;
                            pgObj.PG2_id = PG2_id;
                        }

                        var jsonPG = JsonConvert.SerializeObject(pgObj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        m.ContactPref = jsonPG.ToString();
                    }
                    else
                        m.ContactPref = null;
                }

                if (MPList != null)
                {
                    var json = JsonConvert.SerializeObject(MPList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    string jstr = json.ToString();
                    return jstr;
                }
                else
                    return ("Lookup Failed with id:" + pid);
            }

            string FPTarget = "Find People:";
            if (id.StartsWith(FPTarget))
                id = id.Substring(FPTarget.Length);

            id = id.Replace("{", "").Replace("}", ""); // get rid of curly braces

            //******************************************************
            // MULTI_FIELD FIND PEOPLE SEARCH
            //******************************************************

            // Generate Key/Value Pairs to update person
            char[] outerDelims = { ';', '~', '&' };
            char[] innerDelims = { ':', '=' };

            string[] pairs = id.Split(outerDelims);

            List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
            foreach (string nvs in pairs)
            {
                string[] nvp = nvs.Split(innerDelims);
                if ((nvp.Length == 2) && (nvp[1].Length > 0))
                {
                    props.Add(new KeyValuePair<string, string>(nvp[0], nvp[1]));
                }
            }

            // SQL Based Query

            String sel = "Select * from Person FULL OUTER JOIN Address ON (Person.Address_id = Address.id)";
            int orgSelLen = sel.Length;
            bool bFiltered = false;

            PersonMgr pmgr = new PersonMgr(entities);
            Person fp;
            Address fa;
            if (pmgr.Update(ref props, true, true, out fp, out fa) != MgrStatus.Add_Update_Succeeded)
                return "Invalid Search Criteria";

            //  "&DOB="
            //  "&Street="
            //  "&City="
            //  "&State="
            //  "&Zip="
            //  "&HPhone="
            //  "&Cell="
            //  "&EMail="
            //  "&Comments="
            //  "&SSNum="
            //  "&PG1="
            //  "&PG2="
            //  "&SkillSets=" ...OR...  "&multicheckbox[]="

            PersonMgr.AddToSelect(ref sel, "Person.FirstName", fp.FirstName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.LastName", fp.LastName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.Street", fa.Street, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.Town", fa.Town, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.CellPhone", fp.CellPhone, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.EMail", fp.Email, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Church_id", fp.ChurchId, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Ssnum", fp.Ssnum, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.State", fa.State, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Gender", fp.Gender.GetValueOrDefault(0), ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.SkillSets", fp.SkillSets, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Roles", fp.Roles.GetValueOrDefault(0), ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Status", fp.Status, ref bFiltered);
            if (!fp.Dob.GetValueOrDefault().Equals(new DateTime(1900, 1, 1)))
                PersonMgr.AddToSelect(ref sel, "Person.DOB", fp.Dob.GetValueOrDefault(), true, ref bFiltered);

            if (sel.Length == orgSelLen)
                sel += "order by Address.Street ASC";

            //                Also, it is recommended to use ISO8601 format YYYY - MM - DDThh:mm: ss.nnn[Z], as this one will not depend on your server's local culture.
            //SELECT*
            //FROM TABLENAME
            //WHERE
            //    DateTime >= '2011-04-12T00:00:00.000' AND
            //    DateTime <= '2011-05-25T03:53:04.000'

            try
            {
                //List<Person> PList = new List<Person>();
                //var PList = entities.Person.ExecuteSqlRaw(sel).ToList<Person>();
                //var PList = entities.Person.SqlQuery(sel).ToList<Person>();
                //var PList = entities.SqlQuery<Person>(sel).ToList<Person>();
                var MPList = new List<MinPerson>();

                using (var command = entities.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = sel;
                    entities.Database.OpenConnection();
                    using (var row = command.ExecuteReader())
                    {
                        while (row.Read())
                        {
                            var m = new MinPerson();
                            m.FirstName = (string)row.GetValue((int)Person.ePA.FirstName);
                            m.LastName = (string)row.GetValue((int)Person.ePA.LastName);
                            m.id = (int)row.GetValue((int)Person.ePA.pid);
                            m.Alias = (string)row.GetValue((int)Person.ePA.Alias).ToString();
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.DOB)))
                                m.DOB = (DateTime)row.GetValue((int)Person.ePA.DOB);
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.Gender)))
                                m.Gender = (byte)row.GetValue((int)Person.ePA.Gender);
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.Status)))
                                m.Status = (long)row.GetValue((int)Person.ePA.Status);
                            m.SSNum = (string)row.GetValue((int)Person.ePA.SSNum).ToString();

                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.Address_id)))
                                m.Address_id = (int)row.GetValue((int)Person.ePA.Address_id);
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.PG1_Person_id)))
                                m.PG1_Person_id = (int)row.GetValue((int)Person.ePA.PG1_Person_id);
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.PG2_Person_id)))
                                m.PG2_Person_id = (int?)row.GetValue((int)Person.ePA.PG2_Person_id);
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.Church_id)))
                                m.Church_id = (int?)row.GetValue((int)Person.ePA.Church_id);
                            m.SkillSets = (long)row.GetValue((int)Person.ePA.SkillSets);
                            m.Roles = (long)row.GetValue((int)Person.ePA.Roles);
                            m.CellPhone = (string)row.GetValue((int)Person.ePA.CellPhone).ToString();
                            m.EMail = (string)row.GetValue((int)Person.ePA.EMail).ToString();
                            m.ContactPref = (string)row.GetValue((int)Person.ePA.ContactPref).ToString();
                            m.Notes = (string)row.GetValue((int)Person.ePA.Notes).ToString();
                            if (m.Address_id.HasValue)
                            {
                                m.Address.id = (int)row.GetValue((int)Person.ePA.aid);
                                m.Address.Street = (string)row.GetValue((int)Person.ePA.Street).ToString();
                                m.Address.Town = (string)row.GetValue((int)Person.ePA.Town).ToString();
                                m.Address.State = (string)row.GetValue((int)Person.ePA.State).ToString();
                                m.Address.ZipCode = (string)row.GetValue((int)Person.ePA.ZipCode).ToString();
                                m.Address.Phone = (string)row.GetValue((int)Person.ePA.Phone).ToString();
                                m.Address.Fax = (string)row.GetValue((int)Person.ePA.Fax).ToString();
                                m.Address.Country = (string)row.GetValue((int)Person.ePA.Country).ToString();
                                m.Address.WebSite = (string)row.GetValue((int)Person.ePA.WebSite).ToString();
                            }
                            else
                            {
                                m.Address.Street = "";
                                m.Address.Town = "";
                                m.Address.State = "";
                                m.Address.ZipCode = "";
                                m.Address.Phone = "";
                                m.Address.Fax = "";
                                m.Address.Country = "";
                                m.Address.WebSite = "";
                            }
                            MPList.Add(m);
                        }
                    }
                }

                if (MPList.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(MPList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    string jstr = json.ToString();
                    return jstr;
                }
            }
            catch (Exception)
            {
            }

            return "ERROR~:No one found with Name";
        }

        public static string FixPhone(string raw, bool bAnchor=false)
        {
            var numStr = raw.Replace("(", "").Replace(")", "").Replace("-", "").Replace(".", "").Replace(" ", "");
            if ((numStr.Length > 10) && numStr.StartsWith("1"))
                numStr = numStr[1..];
            if (numStr.Length == 10)
            {
                var pNum = numStr.Substring(0, 3) + "-" + numStr.Substring(3, 3) + "-" + numStr.Substring(6, 4);
                if (bAnchor)
                    return "<a href=\"tel:" + numStr + "\">" + pNum + "</a>"; 
                return pNum;
            }
            return raw;
        }

        public static string GetVS(string str)
        {
            return !string.IsNullOrEmpty(str) ? str : "";
        }
        public async Task<bool> AddPerson(CStatContext ce, string baptized= "", string church="", string pg1="", string pg2="")
        {
            List<KeyValuePair<String, String>> kvp = new List<KeyValuePair<String, String>>();
            kvp.Add(new KeyValuePair<string, string>("FName", GetVS(FirstName)));
            kvp.Add(new KeyValuePair<string, string>("LName", GetVS(LastName)));
            if (string.IsNullOrEmpty(church))
            {
                if (ChurchId.HasValue) kvp.Add(new KeyValuePair<string, string>("Church", ChurchId.ToString()));
            }
            else
                kvp.Add(new KeyValuePair<string, string>("Church", church)); // Potential new church

            if (AddressId.HasValue) kvp.Add(new KeyValuePair<string, string>("Adr_id", AddressId.ToString()));
            if (Address != null)
            {
                kvp.Add(new KeyValuePair<string, string>("Street", GetVS(Address.Street)));
                kvp.Add(new KeyValuePair<string, string>("City", GetVS(Address.Town)));
                kvp.Add(new KeyValuePair<string, string>("State", GetVS(Address.State)));
                kvp.Add(new KeyValuePair<string, string>("Zip", GetVS(Address.ZipCode)));
                kvp.Add(new KeyValuePair<string, string>("HPhone", GetVS(Address.Phone)));
            }

            kvp.Add(new KeyValuePair<string, string>("Cell", GetVS(CellPhone)));
            kvp.Add(new KeyValuePair<string, string>("SSNum", GetVS(this.Ssnum)));
            kvp.Add(new KeyValuePair<string, string>("EMail", GetVS(this.Email)));
            if (Dob.HasValue) kvp.Add(new KeyValuePair<string, string>("DOB", Dob.Value.Date.ToString()));
            kvp.Add(new KeyValuePair<string, string>("SkillSets", SkillSets.ToString()));
            kvp.Add(new KeyValuePair<string, string>("Roles", Roles.ToString()));
            if (Id > 0) kvp.Add(new KeyValuePair<string, string>("id", Id.ToString()));
            if (Pg1PersonId.HasValue) kvp.Add(new KeyValuePair<string, string>("PG1_pid", Pg1PersonId.Value.ToString()));
            if (Pg2PersonId.HasValue) kvp.Add(new KeyValuePair<string, string>("PG2_pid", Pg1PersonId.Value.ToString()));
            if (String.IsNullOrEmpty(pg1)) kvp.Add(new KeyValuePair<string, string>("PG1_id", pg1));
            if (String.IsNullOrEmpty(pg1)) kvp.Add(new KeyValuePair<string, string>("PG2_id", pg2));
            kvp.Add(new KeyValuePair<string, string>("Comments", GetVS(this.Notes)));
            if (!String.IsNullOrEmpty(baptized))
            {
                string LBapStr = baptized.ToLower().Trim();
                kvp.Add(new KeyValuePair<string, string>("Baptized", LBapStr));
            }
            if (Gender.HasValue) kvp.Add(new KeyValuePair<string, string>("Gender", Gender.Value.ToString()));

            var res = Person.UpdatePerson(ce, kvp, false, true, out Person ResPerson, out Address ResAddress);

            switch (res)
            {
                case MgrStatus.Add_Update_Succeeded:
                    return true;
                case MgrStatus.Add_Person_Failed:
                case MgrStatus.Save_Person_Failed:
                case MgrStatus.Invalid_DOB:
                case MgrStatus.No_Gender:
                case MgrStatus.Save_Event_Failed:
                case MgrStatus.Bad_Event_Type:
                case MgrStatus.Add_Address_Failed:
                case MgrStatus.Invalid_Church:
                case MgrStatus.Add_Church_Failed:
                case MgrStatus.Save_Attendance_Failed:
                case MgrStatus.Add_Attendance_Failed:
                case MgrStatus.Save_Reg_Failed:
                case MgrStatus.Add_Reg_Failed:
                case MgrStatus.Save_Med_Failed:
                case MgrStatus.Add_Med_Failed:
                    return false;

                case MgrStatus.Invalid_Address:
                case MgrStatus.Save_Address_Failed:
                case MgrStatus.Save_PG_Failed:
                    {
                    try
                    {
                        ce.Person.Add(ResPerson);
                        int sres = await ce.SaveChangesAsync();
                        return (sres > 0);
                    }
                    catch (Exception e)
                    {
                        _ = e;
                        return false;
                    }
                }
            }
            return false;
        }

        public static MgrStatus UpdatePerson(CStatContext ce, List<KeyValuePair<String, String>> props, bool bJustGetPerson, bool bAllowNoAddress, out Person ResPerson, out Address ResAddress)
        {
            Person person = new Person();
            Address newAdr = new Address();
            bool bPersonFound = false;
            bool bFoundAdr = false;
            bool bNeedToAddPG1 = false;
            bool bNeedToModPG1 = false;
            Person PG1Person = new Person();
            bool bNeedToAddPG2 = false;
            bool bNeedToModPG2 = false;
            Person PG2Person = new Person();
            bool bValidAddress = false;
            Address foundAdr = null;

            KeyValuePair<string, string> pv;

            pv = props.Find(prop => prop.Key == "FName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.FirstName = pv.Value.Trim();
                int spar = person.FirstName.IndexOf('(');
                if (spar > 0)
                {
                    int epar = person.FirstName.IndexOf(')');
                    if (epar > spar)
                    {
                        person.Alias = person.FirstName.Substring(spar + 1, (epar - spar) - 1);
                        person.Alias = "F:" + person.Alias.Trim();
                        person.FirstName = person.FirstName.Substring(0, spar);
                        person.FirstName = person.FirstName.Trim();
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "LName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.LastName = pv.Value.Trim();
                int spar = person.LastName.IndexOf('(');
                if (spar > 0)
                {
                    int epar = person.LastName.IndexOf(')');
                    if (epar > spar)
                    {
                        person.Alias = person.LastName.Substring(spar + 1, (epar - spar) - 1);
                        person.Alias = "L:" + person.Alias.Trim();
                        person.LastName = person.LastName.Substring(0, spar);
                        person.LastName = person.LastName.Trim();
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "Church");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.All(c => Char.IsDigit(c)))
                {
                    person.ChurchId = Convert.ToInt32(pv.Value);
                }
                else
                {
                    String churchName = pv.Value.Trim();
                    if (churchName != "-1")
                    {
                        Church ch = ce.Church.FirstOrDefault(c => c.Name == churchName);
                        if (ch != null)
                        {
                            person.ChurchId = ch.Id;
                        }
                        else
                        {
                            if (!bJustGetPerson)
                            {
                                // Add new Church
                                Church nc = new Church();
                                nc.Name = churchName;
                                nc.Affiliation = "?";
                                nc.StatusDetails = "Home church of " + person.FirstName + " " + person.LastName;
                                ChurchMgr cmgr = new ChurchMgr(ce);
                                if (cmgr.Validate(ref nc))
                                {
                                    try
                                    {
                                        ce.Church.Add(nc);
                                        ce.SaveChanges();
                                        person.ChurchId = nc.Id;
                                    }
                                    catch
                                    {
                                        person.ChurchId = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            KeyValuePair<string, string> street, zip, city, state;
            zip = props.Find(prop => prop.Key == "Zip");
            city = props.Find(prop => prop.Key == "City");
            state = props.Find(prop => prop.Key == "State");
            street = props.Find(prop => prop.Key == "Street");
            bool bHasSSNum = false;
            bool bHasDOB = false;
            bool bHasZip = !zip.Equals(default(KeyValuePair<String, String>)) && (zip.Value.Length > 0);
            if (bHasZip)
                zip = new KeyValuePair<string, string>("Zip", zip.Value.Replace("-", string.Empty));
            bool bHasStreet = !street.Equals(default(KeyValuePair<String, String>)) && (street.Value.Length > 0);
            bool bHasCS = (!city.Equals(default(KeyValuePair<String, String>)) && (city.Value.Length > 0)) && (!state.Equals(default(KeyValuePair<String, String>)) && (state.Value.Length > 0));
            String streetValue;
            if (bHasStreet)
                streetValue = CleanStreet(street.Value);
            else
                streetValue = "";
            if (((bHasStreet != bHasCS) || (bHasCS != bHasZip)) && !bJustGetPerson && !bAllowNoAddress)
            {
                ResPerson = null;
                ResAddress = null;
                return MgrStatus.Invalid_Address;
            }

            pv = props.Find(prop => prop.Key == "Adr_id");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.All(c => Char.IsDigit(c)))
                {
                    person.AddressId = Convert.ToInt32(pv.Value);
                    bFoundAdr = true;
                }
            }

            if (!bFoundAdr && bHasStreet && (bHasZip || bHasCS))
            {
                bool bTrySCS = false;

                // Try to find an existing address by looping here up to 2 times.
                do
                {
                    int ld, MinLD = 10000;
                    List<Address> adrList = new List<Address>();
                    if (bHasZip && !bTrySCS)
                    {
                        if (zip.Value != "11111")
                        {
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.ZipCode == zip.Value)
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                adrList.Add(a);
                            }
                        }
                    }
                    else
                    {
                        if (!city.Value.Contains("<missing>"))
                        {
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.Town == city.Value) && (adr.State == state.Value)
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                adrList.Add(a);
                            }
                        }
                    }

                    if (adrList.Count() > 0)
                    {
                        Address MinAdr = new Address();
                        foreach (Address a in adrList)
                        {
                            ld = a.Street.Contains("<missing>") ? 10000 : LevenshteinDistance.Compute(a.Street, streetValue);
                            if (ld < MinLD)
                            {
                                MinAdr = a;
                                MinLD = ld;
                            }
                        }

                        int si = MinAdr.Street.IndexOf(' ');
                        if ((MinLD < 2) && ((si > 1) && ((si + 3) < MinAdr.Street.Length) && streetValue.StartsWith(MinAdr.Street.Substring(0, si + 3))))
                        {
                            // Found matching address. Just set address id
                            person.AddressId = newAdr.Id = MinAdr.Id;
                            foundAdr = MinAdr;
                            bFoundAdr = true;
                        }
                    }

                    if (bTrySCS)
                        break; // We already looped. Break here.

                    if (!bFoundAdr && bHasZip)
                    {
                        // Zip may be bad or changed try to match by street, city, state
                        bTrySCS = true;
                    }
                    else
                        break; // Do not loop because there is nothing to retry.
                }
                while (!bFoundAdr);
            }

            //***************************
            //*** Add Address fields ****
            //***************************
            if (bHasStreet)
                newAdr.Street = streetValue;
            if (bHasCS)
            {
                newAdr.Town = city.Value;
                newAdr.State = state.Value;
            }
            else
            {
                if (bJustGetPerson)
                {
                    if (!city.Equals(default(KeyValuePair<String, String>)) && (city.Value.Length > 0))
                        newAdr.Town = city.Value;

                    if (!state.Equals(default(KeyValuePair<String, String>)) && (state.Value.Length > 0))
                        newAdr.State = state.Value;
                }
            }

            if (bHasZip)
                newAdr.ZipCode = zip.Value;

            KeyValuePair<string, string> hphone = props.Find(prop => prop.Key == "HPhone");
            if (!hphone.Equals(default(KeyValuePair<String, String>)) && (hphone.Value.Length > 0))
                newAdr.Phone = hphone.Value;

            AddressMgr amgr = new AddressMgr(ce);
            if (!(bValidAddress = AddressMgr.Validate(ref newAdr)) && !bJustGetPerson && !bAllowNoAddress)
            {
                ResPerson = null;
                ResAddress = null;
                return MgrStatus.Invalid_Address;
            }

            pv = props.Find(prop => prop.Key == "EMail");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Email = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "SSNum");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (bJustGetPerson)
                {
                    person.Ssnum = pv.Value;
                    bHasSSNum = person.Ssnum.Length > 1;
                }
                else
                {
                    person.Ssnum = new string(pv.Value.Where(c => char.IsDigit(c)).ToArray()); // strip out non-numbers
                    bHasSSNum = person.Ssnum.Length > 3;
                }
            }

            pv = props.Find(prop => prop.Key == "Cell");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.CellPhone = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "DOB");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                String dob = pv.Value.Trim();
                int month, day, year;
                string[] tokens = dob.Split('/', '-', '.');
                if (tokens.Count() != 3)
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Invalid_DOB;
                }

                if ((tokens[0].Length < 1) || (tokens[1].Length < 1) || (tokens[2].Length < 1))
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Invalid_DOB;
                }

                bool T0_Num = ((tokens[0][0] >= '0') && (tokens[0][0] <= '9'));
                bool T1_Num = ((tokens[1][0] >= '0') && (tokens[1][0] <= '9'));

                if (T0_Num && T1_Num)
                {
                    month = Convert.ToInt32(tokens[0]);
                    day = Convert.ToInt32(tokens[1]);
                    year = Convert.ToInt32(tokens[2].Split(" ")[0]);
                }
                else
                {
                    if (T0_Num)
                    {
                        month = DateTime.ParseExact(tokens[1], "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
                        day = Convert.ToInt32(tokens[0]);
                        year = Convert.ToInt32(tokens[2]);
                    }
                    else
                    {
                        month = DateTime.ParseExact(tokens[0], "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
                        day = Convert.ToInt32(tokens[1]);
                        year = Convert.ToInt32(tokens[2]);
                    }
                }

                if (year < 100)
                {
                    int YNow = PropMgr.ESTNow.Year;
                    if (year > (YNow - 2000))
                        year += 1900;
                    else
                        year += 2000;
                }
                person.Dob = new DateTime(year, month, day);
                if (person.Dob == null)
                {
                    //ResPerson = null;
                    //ResAddress = null;
                    //return MgrStatus.Invalid_DOB;
                    person.Dob = new DateTime(1900, 1, 1); // To handle poor records
                }
                bHasDOB = true;
            }
            else
            {
                if (!bJustGetPerson)
                {
                    //ResPerson = null;
                    //ResAddress = null;
                    //return MgrStatus.Invalid_DOB;
                    person.Dob = new DateTime(1900, 1, 1);  // To handle poor records
                }
            }

            pv = props.Find(prop => prop.Key == "Gender");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                String gender = pv.Value.Trim().ToUpper();
                if ((gender == "F") || (gender == "70") || (gender == "FEMALE"))
                    person.Gender = (byte)'F';
                else if ((gender == "M") || (gender == "77") || (gender == "MALE"))
                    person.Gender = (byte)'M';
                else
                {
                    person.Gender = 0;
                    if (!bJustGetPerson && (gender.Length > 0))
                    {
                        ResPerson = null;
                        ResAddress = null;
                        return MgrStatus.No_Gender;
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "SkillSets");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.SkillSets = long.Parse(pv.Value.Trim());

            }

            pv = props.Find(prop => prop.Key == "Roles");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Roles = long.Parse(pv.Value.Trim());

            }

            var pva = from pair in props
                      where (pair.Key == "multicheckbox[]")
                      select pair;

            int NumPairs = pva.Count();
            if (NumPairs > 0)
            {
                person.SkillSets = 0;
                foreach (KeyValuePair<string, string> pair in pva)
                {
                    String sval = pair.Value.Trim();
                    long lval;
                    if (eSkills.TryParse(sval, out lval))
                        person.SkillSets |= lval;
                }
            }

            pv = props.Find(prop => prop.Key == "Comments");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Notes = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "Baptized");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.ToLower().StartsWith("y"))
                {
                    person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.NotBaptized;
                    person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.Baptized;
                }
                else
                {
                    if (pv.Value.ToLower().StartsWith("n"))
                    {
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.Baptized;
                        person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.NotBaptized;
                    }
                    else
                    {
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.Baptized;
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.NotBaptized;
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "id");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                Int32 pid = Int32.Parse(pv.Value);
                if (pid > 0)
                {
                    bPersonFound = true;
                    person.Id = pid;
                }
            }

            pv = props.Find(prop => prop.Key == "PG1_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg1PersonId = int.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PG1_id");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    bool bParent = true;

                    if (pga.Count() > 1)
                    {
                        String rel = pga[1].Trim().ToUpper();
                        // {TBD : deal with all pgs. Assume pg for now} if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                        //    bParent = false;
                    }

                    if (bParent)
                    {
                        bool bAbort = false;
                        for (int i = 0; i < pga.Count(); ++i)
                        {
                            if (bAbort)
                                break;
                            switch (i)
                            {
                                case 0: // "First Last" name
                                    {
                                        if (SplitName(pga[0], ref PG1Person))
                                        {
                                            int adr_id;
                                            int id = FindPersonIDByName(ce, ref PG1Person, bAllowNoAddress, out adr_id);
                                            if (id != -1)
                                            {
                                                person.Pg1PersonId = PG1Person.Id = id;
                                                if (adr_id != -1)
                                                    PG1Person.AddressId = adr_id;
                                            }
                                            else
                                            {
                                                if (!bJustGetPerson)
                                                    bNeedToAddPG1 = true;
                                            }
                                        }
                                        else
                                            bAbort = true;
                                    }
                                    break;

                                case 2: // Phone
                                    {
                                        if (PG1Person != null)
                                        {
                                            PG1Person.CellPhone = pga[2].Trim();
                                            bNeedToModPG1 = true;
                                        }
                                    }
                                    break;

                                case 3: // EMail
                                    {
                                        if (PG1Person != null)
                                        {
                                            PG1Person.Email = pga[3].Trim();
                                            bNeedToModPG1 = true;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "PG2_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg2PersonId = Int32.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PG2_id");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    bool bParent = true;

                    if (pga.Count() > 1)
                    {
                        String rel = pga[1].Trim().ToUpper();
                        if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                            bParent = false;
                    }

                    if (bParent)
                    {
                        bool bAbort = false;
                        for (int i = 0; i < pga.Count(); ++i)
                        {
                            if (bAbort)
                                break;

                            switch (i)
                            {
                                case 0: // "First Last" name
                                    {
                                        if (SplitName(pga[0], ref PG2Person))
                                        {
                                            int adr_id;
                                            int id = FindPersonIDByName(ce, ref PG2Person, bAllowNoAddress, out adr_id);
                                            if (id != -1)
                                            {
                                                person.Pg2PersonId = PG2Person.Id = id;
                                                if (adr_id != -1)
                                                    PG2Person.AddressId = adr_id;
                                            }
                                            else
                                            {
                                                if (!bJustGetPerson)
                                                    bNeedToAddPG2 = true;
                                            }
                                        }
                                        else
                                            bAbort = true;
                                    }
                                    break;

                                case 2: // Phone
                                    {
                                        if (PG2Person != null)
                                        {
                                            PG2Person.CellPhone = pga[2].Trim();
                                            bNeedToModPG2 = true;
                                        }
                                    }
                                    break;

                                case 3: // EMail
                                    {
                                        {
                                            if (PG2Person != null)
                                            {
                                                PG2Person.Email = pga[3].Trim();
                                                bNeedToModPG2 = true;
                                            }
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            Person JGPerson = (bJustGetPerson) ? person.ShallowCopy() : person;
            Address JGnewAdr = (bJustGetPerson) ? newAdr.ShallowCopy() : newAdr;

            //**************************************************************************
            // Find Person #1 : Find person by SS# First
            //**************************************************************************
            if (bHasSSNum)
            {
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.Ssnum == person.Ssnum)
                           select psn;

                // Match first name only due to marriage changing last name
                int adr_id = -1;
                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id, false);
                if (id != -1)
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (adr_id != -1)
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (!bPersonFound && bHasDOB)
            {
                //**************************************************************************
                // Find Person #2 : Find person by DOB and close name match
                //**************************************************************************
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.Dob == person.Dob)
                           select psn;

                int adr_id = -1;
                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id);
                if (id != -1)
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (adr_id != -1)
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (!bPersonFound && (person.LastName != null) && (person.LastName.Length > 0))
            {
                //**************************************************************************
                // Find Person #3 : Find person by Last Name and close first name match
                //**************************************************************************
                int adr_id;
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.LastName.ToUpper() == person.LastName.ToUpper())
                           select psn;

                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id, false); // already checking last name in query
                if (id != -1)
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (adr_id != -1)
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (bJustGetPerson)
            {
                ResPerson = JGPerson;
                ResAddress = JGnewAdr;
                return MgrStatus.Add_Update_Succeeded;
            }

            int tryState = 1;
            try
            {
                if (bValidAddress)
                {
                    if (!bFoundAdr)
                    {
                        // Add new, valid address and set Person.Address_id
                        try
                        { 
                            ce.Address.Add(newAdr);
                            if (ce.SaveChanges() < 1)
                            {
                                ResPerson = person;
                                ResAddress = null;
                                return MgrStatus.Save_Address_Failed;
                            }
                            if (newAdr.Id != 0)
                                person.AddressId = newAdr.Id;  // addedAdr.id;
                        }
                        catch
                        {
                            ResPerson = person;
                            ResAddress = null;
                            return MgrStatus.Save_Address_Failed;
                        }
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
                            }
                            catch
                            {
                                ResPerson = person;
                                ResAddress = null;
                                return MgrStatus.Save_Address_Failed;
                            }
                        }
                    }
                }

                if (bNeedToAddPG1)
                {
                    tryState = 2;
                    if (newAdr.Id != 0)
                        PG1Person.AddressId = newAdr.Id; // Assume PG lives with child.
                    try
                    {
                        if (ce.Person.Add(PG1Person) != null)
                        {
                            ce.SaveChanges();
                            if (PG1Person.Id != 0)
                                person.Pg1PersonId = PG1Person.Id;
                            else
                                person.Pg1PersonId = null;
                        }
                    }
                    catch
                    {
                        ResPerson = person;
                        ResAddress = null;
                        return MgrStatus.Save_PG_Failed;
                    }
                }
                else
                {
                    if (bNeedToModPG1)
                    {
                        try
                        {
                            ce.Person.Attach(PG1Person);
                            ce.Entry(PG1Person).State = EntityState.Modified;
                            ce.SaveChanges();
                        }
                        catch
                        {
                            ResPerson = person;
                            ResAddress = null;
                            return MgrStatus.Save_PG_Failed;
                        }
                    }
                }

                if (bNeedToAddPG2)
                {
                    tryState = 3;
                    if (newAdr.Id != 0)
                        PG2Person.AddressId = newAdr.Id; // Assume PG lives with child.
                    try
                    {
                        if (ce.Person.Add(PG2Person) != null)
                        {
                            ce.SaveChanges();
                            if (PG2Person.Id != 0)
                                person.Pg2PersonId = PG2Person.Id;
                            else
                                person.Pg2PersonId = null;
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        if (bNeedToModPG2)
                        {
                            ce.Person.Attach(PG2Person);
                            ce.Entry(PG2Person).State = EntityState.Modified;
                            ce.SaveChanges();
                        }
                    }
                    catch
                    {

                    }
                }

                if (!bPersonFound)
                {
                    // Add a new Person
                    tryState = 4;
                    if (ce.Person.Add(person) == null)
                    {
                        ResPerson = null;
                        ResAddress = null;
                        return MgrStatus.Add_Person_Failed;
                    }
                }
                else
                {
                    // Update an existing Person
                    ce.Person.Attach(person);
                    ce.Entry(person).State = EntityState.Modified;
                }

                if (ce.SaveChanges() < 1)
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Save_Person_Failed;
                }
            }
            catch (Exception e)
            {
                _ = e;
                ResPerson = null;
                ResAddress = null;
                if (tryState > 1)
                    return MgrStatus.Add_Person_Failed;
                else
                    return MgrStatus.Add_Address_Failed;
            }

            ResPerson = person;
            ResAddress = newAdr;
            return MgrStatus.Add_Update_Succeeded;

        }

        public static int FindPersonIDByName(CStatContext ce, ref Person person, bool bMerge, out int address_id, bool bCheckLastName = true)
        {
            String LName = person.LastName;

            var psnL = from psn in ce.Person
                       where (psn.LastName == LName)
                       select psn;

            return FindPersonIDByName(psnL, ref person, bMerge, out address_id, bCheckLastName);
        }

        public static int FindPersonIDByName(IQueryable<Person> psnL, ref Person person, bool bMerge, out int address_id, bool bCheckLastName = true)
        {
            address_id = -1;
            if (psnL.Count() > 0)
            {
                int ld, MinLD = 1000000;
                Person MinP = null;

                int FStartLen3 = 0;
                String FStart3 = "";
                int FStartLen4 = 0;
                String FStart4 = "";
                if (person.FirstName != null)
                {
                    FStartLen3 = person.FirstName.Length < 3 ? person.FirstName.Length : 3;
                    FStart3 = person.FirstName.Substring(0, FStartLen3).ToUpper();
                    FStartLen4 = person.FirstName.Length < 4 ? person.FirstName.Length : 4;
                    FStart4 = person.FirstName.Substring(0, FStartLen4).ToUpper();
                }

                foreach (Person p in psnL)
                {
                    if (person.Gender.HasValue && (person.Gender != 0))
                    {
                        if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                            continue;
                    }
                    if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                    {
                        if (String.Compare(p.LastName, person.LastName, true) != 0)
                            continue;
                    }

                    if ((p.Alias != null) && (person.Alias != null) && (p.Alias.Length > 0) && (String.Compare(p.Alias, person.Alias, true) == 0))
                    {
                        if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                            address_id = MinP.AddressId.Value;

                        // Merge existing info onto referenced person
                        if (bMerge)
                            MergePersonAttrs(p, ref person);

                        return p.Id;
                    }

                    if (FStartLen3 == 0)
                    {
                        MinP = p;
                        MinLD = 0;
                    }
                    else
                    {
                        ld = LevenshteinDistance.Compute(p.FirstName, person.FirstName);
                        if ((ld < MinLD) && (p.FirstName.ToUpper().StartsWith(FStart4)))
                        {
                            MinP = p;
                            MinLD = ld;
                        }
                        else
                        {
                            if ((ld < MinLD) && (p.FirstName.ToUpper().StartsWith(FStart3)))
                            {
                                MinP = p;
                                MinLD = ld;
                            }
                        }
                    }
                }

                if (MinLD < 3)
                {
                    if (MinP.AddressId.HasValue && (MinP.AddressId.Value > 0))
                        address_id = MinP.AddressId.Value;

                    // Merge existing info onto referenced person
                    if (bMerge)
                        MergePersonAttrs(MinP, ref person);

                    return MinP.Id;
                }

                if (FStartLen4 > 0)
                {
                    foreach (Person p in psnL)
                    {
                        if (person.Gender.HasValue && (person.Gender != 0))
                        {
                            if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                                continue;
                        }
                        if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                        {
                            if (String.Compare(p.LastName, person.LastName, true) != 0)
                                continue;
                        }

                        if (p.FirstName.ToUpper().StartsWith(FStart4))
                        {
                            if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                                address_id = p.AddressId.Value;

                            // Merge existing info onto referenced person
                            if (bMerge)
                                MergePersonAttrs(p, ref person);

                            return p.Id;
                        }
                    }
                }

                if (FStartLen3 > 0)
                {
                    foreach (Person p in psnL)
                    {
                        if (person.Gender.HasValue && (person.Gender != 0))
                        {
                            if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                                continue;
                        }
                        if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                        {
                            if (String.Compare(p.LastName, person.LastName, true) != 0)
                                continue;
                        }

                        if (p.FirstName.ToUpper().StartsWith(FStart3))
                        {
                            if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                                address_id = p.AddressId.Value;

                            // Merge existing info onto referenced person
                            if (bMerge)
                                MergePersonAttrs(p, ref person);

                            return p.Id;
                        }
                    }
                }
            }
            return -1;
        }

        public static void MergePersonAttrs(Person src, ref Person dst)
        {
            if (src.Gender.HasValue && !dst.Gender.HasValue)
                dst.Gender = src.Gender;

            if ((src.Dob.HasValue && (src.Dob.Value.Year > 1900)) && (!dst.Dob.HasValue || (dst.Dob.Value.Year == 1900)))
                dst.Dob = src.Dob;

            if (((src.Email != null) && (src.Email.Length > 0)) && ((dst.Email == null) || (dst.Email.Length == 0)))
                dst.Email = src.Email;

            if (((src.Ssnum != null) && (src.Ssnum.Length > 0)) && ((dst.Ssnum == null) || (dst.Ssnum.Length == 0)))
                dst.Ssnum = src.Ssnum;

            if (((src.CellPhone != null) && (src.CellPhone.Length > 0)) && ((dst.CellPhone == null) || (dst.CellPhone.Length == 0)))
                dst.CellPhone = src.CellPhone;

            if ((src.SkillSets != 0) && (dst.SkillSets == 0))
                dst.SkillSets = src.SkillSets;

            if ((src.ChurchId.HasValue) && (!dst.ChurchId.HasValue))
                dst.ChurchId = src.ChurchId;
        }

        public static bool SplitName(String name, ref Person p) // Currently assumes no possible last name alias
        {
            String raw = name.Trim();
            int isp = raw.LastIndexOf(" ");
            if (isp <= 0)
                return false;
            int iop = raw.IndexOf('(');
            if (iop > isp)
                return false;
            if (iop > 0)
            {
                int icp = p.FirstName.IndexOf(')');
                if (icp > iop)
                {
                    p.Alias = p.FirstName.Substring(iop + 1, (icp - iop) - 1);
                    p.Alias = "F:" + p.Alias.Trim();
                    p.FirstName = raw.Substring(0, iop);
                    p.FirstName = p.FirstName.Trim();
                }
            }
            else
            {
                p.FirstName = raw.Substring(0, isp);
                p.FirstName = p.FirstName.Trim();
            }
            p.LastName = raw.Substring(isp + 1);
            p.LastName = p.LastName.Trim();
            return true;
        }
        public static String CleanStreet(String oldStr)
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
            else if (upStr.EndsWith(" Boulevard"))
                newStr = newStr.Remove(len - 9) + "Blvd.";
            else if (upStr.EndsWith(" Blvd"))
                newStr = newStr.Remove(len - 4) + "Blvd.";
            else if (upStr.EndsWith(" Blv"))
                newStr = newStr.Remove(len - 3) + "Blvd.";
            else if (upStr.EndsWith(" Bl"))
                newStr = newStr.Remove(len - 2) + "Blvd.";
            else if (upStr.EndsWith(" HIGHWAY"))
                newStr = newStr.Remove(len - 7) + "Hwy.";
            else if (upStr.EndsWith(" WAY"))
                newStr = newStr.Remove(len - 3) + "Way";

            return newStr;
        }
        public static String DecorateSS(String rawSS)
        {
            var ss = rawSS.Trim();
            if (ss.Length == 9)
            {
                return ss.Substring(0, 3) + "-" + ss.Substring(3, 2) + "-" + ss.Substring(5, 4);
            }
            return ss;
        }
        public static String UndecorateSS(String ss)
        {
            return ss.Trim().Replace("-", "");
        }

        public static DateTime? InitDOB(DateTime? dt)
        {
            if (dt.HasValue && (dt.Value.Year == 1900) && (dt.Value.Month == 1) && (dt.Value.Day == 1))
                return null;
            return dt;
        }

        public static DateTime? UpdateDOB(DateTime? dt)
        {
            DateTime? retDT;
            if (!dt.HasValue)
            {
                retDT = new DateTime(1900, 1, 1);
                return retDT;
            }
            return dt;
        }

        public int GetTermExpire()
        {
            if ((Roles & (long)(Person.TitleRoles.President | Person.TitleRoles.Vice_Pres | Person.TitleRoles.Secretary | Person.TitleRoles.Treasurer | Person.TitleRoles.Memb_at_Lg)) == 0)
                return 0;

            if (!string.IsNullOrEmpty(Notes))
            {
                var sidx = Notes.IndexOf("{ExpiresNov:");
                if (sidx != -1)
                {
                    sidx += 12;
                    var eidx = this.Notes.Substring(sidx).IndexOf("}");
                    if ((eidx != -1) && (eidx > 0))
                    {
                        try
                        {
                            if (int.TryParse(Notes.Substring(sidx, eidx), out int year))
                                return year;
                        }
                        catch { }
                    }
                }
            }
            return 0;
        }

        // {FName=&LName=&Gender=0&AgeRange=&Church=-1&SkillSets=0&Roles=1024}
        // return new JsonResult(Person.FindPeople(_context, "Find People:" + jsonQS));
    }
}
