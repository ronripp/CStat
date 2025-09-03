using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Common;
using CStat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using MinAddressPerson;

namespace CStat.Models
{
    public partial class Person
    {
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();

        public enum PersonStatus
        {
            NotBaptized = 0x0000000000000010,
            Baptized = 0x0000000000000020,
            Deceased = 0x0000000000000040
        };

        //public object PersonStatus { get; private set; }

        public enum TitleRoles
        {
            Undefined = 0, Blds_NGnds = 0x1, Dean = 0x2, Counselor = 0x4, SWAT = 0x8, Worship = 0x10, Health_Dir = 0x20, nurse = 0x40, Manager = 0x80,
            Cook = 0x100, FFE = 0x200, Trustee = 0x400, President = 0x800, Treasurer = 0x1000, Secretary = 0x2000, Vice_Pres = 0x4000, Memb_at_Lg = 0x8000,
            Security = 0x10000, Unavailable = 0x20000, Admin = 0x40000, POC = 0x80000, Book_Keeper = 0x100000
        };

        public enum eGender
        {
            M = (int)(uint)(byte)'M',
            F = (int)(uint)(byte)'F'
        };

        public enum eSkills
        {
            Carpentry  = 0x1,
            Plumbing   = 0x2,
            Roofing    = 0x4,
            Cook       = 0x8,
            Nurse      = 0x10,
            SWAT       = 0x20,
            Manager    = 0x40,
            Painter    = 0x80,
            Minister   = 0x100,
            Dean       = 0x200,
            Accountant = 0x400,
            Electricn  = 0x800,
            Computer   = 0x1000,
            Kitch_Help = 0x2000,
            Counselor  = 0x4000,
            Gardening  = 0x8000,
            Tree_Cut   = 0x10000,
            Worship    = 0x20000,
            Worker     = 0x40000,
            Mason      = 0x80000,
            Constructn = 0x100000,
            Septic     = 0x200000,
            Donor      = 0x400000,
            Book_Keep  = 0x800000,
            Staff_Mgr  = 0x1000000,
            Legal      = 0x2000000
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
            SPT = 0x200000,
            DNR = 0x400000,
            BKP = 0x800000,
            SMG = 0x1000000,
            LGL = 0x2000000
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
            WebSite,
            AdrStatus
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
                    int PGInfo1 = 0, PGInfo2 = 0;

                    Person p1 = entities.Person.FirstOrDefault(p => p.Id == m.PG1_Person_id);
                    if (p1 != null)
                    {
                        p1.FirstName = PersonMgr.MakeFirstName(p1);
                        p1.LastName = PersonMgr.MakeLastName(p1);
                        PG1Str = p1.FirstName + " " + p1.LastName;
                        PGInfo1 = p1.Id;
                    }

                    Person p2 = entities.Person.FirstOrDefault(p => p.Id == m.PG2_Person_id);
                    if (p2 != null)
                    {
                        p2.FirstName = PersonMgr.MakeFirstName(p2);
                        p2.LastName = PersonMgr.MakeLastName(p2);
                        PG2Str = p2.FirstName + " " + p2.LastName;
                        PGInfo2 = p2.Id;
                    }

                    if ((PG1Str != null) || (PG2Str != null))
                    {
                        if (PG1Str != null)
                        {
                            pgObj.PG1Name = PG1Str;
                            pgObj.PGInfo1 = PGInfo1;
                        }
                        if (PG2Str != null)
                        {
                            pgObj.PG2Name = PG2Str;
                            pgObj.PGInfo2 = PGInfo2;
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

            String sel = "Select * from ronripp_CStat.Person FULL OUTER JOIN ronripp_CStat.Address ON (ronripp_CStat.Person.Address_id = ronripp_CStat.Address.id)";
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

            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.FirstName", fp.FirstName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.LastName", fp.LastName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Address.Street", fa.Street, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Address.Town", fa.Town, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.CellPhone", fp.CellPhone, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.EMail", fp.Email, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.Church_id", fp.ChurchId, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.Ssnum", fp.Ssnum, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Address.State", fa.State, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.Gender", fp.Gender.GetValueOrDefault(0), ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.SkillSets", fp.SkillSets, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.Roles", fp.Roles.GetValueOrDefault(0), ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.Status", fp.Status, ref bFiltered);
            if (!fp.Dob.GetValueOrDefault().Equals(new DateTime(1900, 1, 1)))
                PersonMgr.AddToSelect(ref sel, "ronripp_CStat.Person.DOB", fp.Dob.GetValueOrDefault(), true, ref bFiltered);

            if (sel.Length == orgSelLen)
                sel += "order by ronripp_CStat.Address.Street ASC";

            //                Also, it is recommended to use ISO8601 format YYYY - MM - DDThh:mm: ss.nnn[Z], as this one will not depend on your server's local culture.
            //SELECT*
            //FROM TABLENAME
            //WHERE
            //    DateTime >= '2011-04-12T00:00:00.000' AND
            //    DateTime <= '2011-05-25T03:53:04.000'

            int rowCnt = 0;
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
                            ++rowCnt;

                            if (row.IsDBNull((int)Person.ePA.pid))
                                continue; // has happened. TBD : Remove garbage records. Find how they got in.

                            if (rowCnt == 237)
                               rowCnt *= 1;

                            var m = new MinPerson();
                            bool hasFName = false;
                            bool hasLName = false;
                            m.id = (int)row.GetValue((int)Person.ePA.pid);
                            if (!row.IsDBNull((int)Person.ePA.FirstName))
                            {
                                m.FirstName = (string)row.GetValue((int)Person.ePA.FirstName);
                                hasFName = true;
                            }
                            else
                                m.FirstName = "(none)";
                            if (!row.IsDBNull((int)Person.ePA.LastName))
                            {
                                m.LastName = (string)row.GetValue((int)Person.ePA.LastName);
                                hasLName = true;
                            }
                            else
                                m.LastName = "(none)";

                            if (!hasFName && !hasLName)
                                continue; // has happened. TBD : Remove garbage records. Find how they got in.

                            if (!row.IsDBNull((int)Person.ePA.Alias))
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
                            if (!DBNull.Value.Equals(row.GetValue((int)Person.ePA.Roles)))
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
                                m.Address.Status = (int)row.GetValue((int)Person.ePA.AdrStatus);
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
                                m.Address.Status = 0;
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
            catch (Exception e)
            {
                return "Error RowCnt=" + rowCnt + " DB:" + e.Message;
            }

            return "ERROR~:No one found with Name";
        }

        public static string FixPhone(string raw, bool bAnchor=false)
        {
            if (String.IsNullOrEmpty(raw))
                return "---";
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

        public static string GetPOCMinister(Church church)
        {
            if (church.YouthMinister != null)
                return church.YouthMinister.FirstName + " " + church.YouthMinister.LastName;
            if (church.SeniorMinister != null)
                return church.SeniorMinister.FirstName + " " + church.SeniorMinister.LastName;
            return "???";
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
                kvp.Add(new KeyValuePair<string, string>("AdrStatus", GetVS(Address.Status.ToString())));
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
            if (String.IsNullOrEmpty(pg1)) kvp.Add(new KeyValuePair<string, string>("PGInfo1", pg1));
            if (String.IsNullOrEmpty(pg1)) kvp.Add(new KeyValuePair<string, string>("PGInfo2", pg2));
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
                        Address.UpdateAddress(ce, ResAddress);
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
            Address newAdr = new Address();

            Person person = null;
            bool bPersonFound = false;

            Address oldAdr = null;
            bool bFoundOldAdr = false;

            Address matchingOldAdr = null;
            bool bMatchingOldAdr = false;

            bool bNeedToAddPG1 = false;
            bool bNeedToModPG1 = false;
            Person PG1Person = new Person();
            bool bNeedToAddPG2 = false;
            bool bNeedToModPG2 = false;
            Person PG2Person = new Person();
            bool bValidNewAddress = false;

            //Person person = new Person();
            //Address newAdr = new Address();
            //bool bPersonFound = false;
            //bool bFoundAdr = false;
            //bool bNeedToAddPG1 = false;
            //bool bNeedToModPG1 = false;
            //Person PG1Person = new Person();
            //bool bNeedToAddPG2 = false;
            //bool bNeedToModPG2 = false;
            //Person PG2Person = new Person();
            //bool bValidAddress = false;
            //Address foundAdr = null;

            KeyValuePair<string, string> pv;

            pv = props.Find(prop => prop.Key == "id");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                Int32 pid = Int32.Parse(pv.Value);
                if (pid > 0)
                {
                    person = ce.Person.AsNoTracking().FirstOrDefault(p => p.Id == pid);
                    if (person != null)
                    {
                        bPersonFound = true;
                        person.Id = pid;
                        if (person.AddressId != null)
                        {
                            oldAdr = ce.Address.AsNoTracking().FirstOrDefault(a => a.Id == person.AddressId);
                            bFoundOldAdr = (oldAdr != null);
                        }
                    }
                }
            }
            if (person == null)
                person = new Person();

            pv = props.Find(prop => prop.Key == "Adr_id"); // overrides person.AddressId
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.All(c => Char.IsDigit(c)))
                {
                    int aid = Convert.ToInt32(pv.Value);
                    oldAdr = ce.Address.AsNoTracking().FirstOrDefault(a => a.Id == aid);
                    if (oldAdr != null)
                    {
                        person.AddressId = Convert.ToInt32(pv.Value);
                        bFoundOldAdr = true;
                    }
                }
            }

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
                        //OLD Church ch = ce.Church.AsNoTracking().FirstOrDefault(c => c.Name == churchName);
                        ChurchMgr chMgr = new ChurchMgr(ce);
                        Church ch = chMgr.FindChurch(pv.Value);
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

            KeyValuePair<string, string> street, zip, city, rawState, state;
            zip = props.Find(prop => prop.Key == "Zip");
            city = props.Find(prop => prop.Key == "City");
            rawState = props.Find(prop => prop.Key == "State");
            street = props.Find(prop => prop.Key == "Street");
            bool bHasSSNum = false;
            bool bHasDOB = false;
            bool bHasZip = !zip.Equals(default(KeyValuePair<String, String>)) && (zip.Value.Length > 0);
            if (bHasZip)
                zip = new KeyValuePair<string, string>("Zip", zip.Value.Replace("-", string.Empty));
            bool bHasStreet = !street.Equals(default(KeyValuePair<String, String>)) && (street.Value.Length > 0);
            bool bHasState = (!rawState.Equals(default(KeyValuePair<String, String>)) && (rawState.Value.Length > 0));
            state = (bHasState) ? new KeyValuePair<string, string>("State", Address.GetStateAbbr(rawState.Value)) : rawState;
            bool bHasCS = (!city.Equals(default(KeyValuePair<String, String>)) && (city.Value.Length > 0)) && bHasState;
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


            if (bHasStreet && (bHasZip || bHasCS))
            {
                bool bTrySCS = false;

                // Try to find an existing MATCHING address by looping here up to 2 times.
                do
                {
                    int ld;
                    List<Address> adrList = new List<Address>();
                    if (bHasZip && !bTrySCS)
                    {
                        if (zip.Value != "11111")
                        {
                            bool IsTrim = zip.Value.Length > 5;
                            var TrimZip = IsTrim ? zip.Value.Substring(0, 5) : zip.Value;

                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.ZipCode.StartsWith(TrimZip))
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                if (IsTrim && (a.ZipCode.Length > 5) && (a.ZipCode != zip.Value))
                                    continue;
                                if (!bHasCS || cm.EQ(a.State, state.Value))
                                    adrList.Add(a);
                            }
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(city.Value) && !city.Value.Contains("<missing>"))
                        {
                            var lcity = city.Value.ToLower();
                            var lstate = state.Value.ToLower();
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.Town.ToLower() == lcity) && (adr.State.ToLower() == lstate)
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
                            ld = (a.Street.Contains("<missing>") || a.Town.Contains("<missing>")) ? 10000 : LevenshteinDistance.Compute(a.Street, streetValue);
                            if (ld < 2)
                            {
                                int si = a.Street.IndexOf(' ');
                                if (String.Equals(a.Street, streetValue, StringComparison.OrdinalIgnoreCase) || ((si > 1) && ((si + 4) < a.Street.Length) && streetValue.StartsWith(a.Street.Substring(0, si + 4))))
                                {
                                    // Found matching address. Just set address id
                                    person.AddressId = newAdr.Id = a.Id;
                                    matchingOldAdr = a;
                                    bMatchingOldAdr = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (bTrySCS)
                        break; // We already looped. Break here.

                    if (!bMatchingOldAdr && bHasZip)
                    {
                        // Zip may be bad or changed try to match by street, city, state
                        bTrySCS = true;
                    }
                    else
                        break; // Do not loop because there is nothing to retry.
                }
                while (!bMatchingOldAdr);
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
            if (!(bValidNewAddress = AddressMgr.Validate(ref newAdr)) && !bJustGetPerson && !bAllowNoAddress)
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

            pv = props.Find(prop => prop.Key == "Baptized");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                // Do not reset Baptism because that is the default (unknown) and some subsequent records may be incomplete and if set prior, then keep that setting and don't reset it to unknown
                var BapVal = pv.Value.Trim().ToLower();
                if (BapVal.StartsWith("y") || BapVal.StartsWith("t"))
                {
                    person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.NotBaptized;
                    person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.Baptized;
                }
                else
                {
                    if (BapVal.StartsWith("n") || BapVal.StartsWith("f"))
                    {
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.Baptized;
                        person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.NotBaptized;
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "PG1_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg1PersonId = int.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PGInfo1");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    //bool bParent = true;

                    //if (pga.Count() > 1)
                    //{
                        //String rel = pga[1].Trim().ToUpper();
                        //// {TBD : deal with all pgs. Assume pg for now} if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                        ////    bParent = false;
                    //}

                    // DO NOT LIMIT TO PARENTS   if (bParent)
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
                                        if (SplitName(pga[0], ref PG1Person, person.LastName))
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
                                            if (!string.IsNullOrEmpty(PG1Person.Email))
                                                bNeedToModPG1 = true;
                                        }
                                    }
                                    break;

                                case 4: // Address
                                    {
                                        if (PG1Person != null)
                                        {
                                            var PG1Adr = pga[4].Trim();
                                            if (Address.IsStreetSimilar(streetValue, PG1Adr))
                                            {
                                                if (!PG1Person.AddressId.HasValue && person.AddressId.HasValue)
                                                    PG1Person.AddressId = person.AddressId.Value;
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

            pv = props.Find(prop => prop.Key == "PG2_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg2PersonId = Int32.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PGInfo2");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    //bool bParent = true;

                    //if (pga.Count() > 1)
                    //{
                        //String rel = pga[1].Trim().ToUpper();
                        //if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                            //bParent = false;
                    //}

                    //DO NOT LIMIT TO PARENTS if (bParent)
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
                                        if (SplitName(pga[0], ref PG2Person, person.LastName))
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
                                        if (PG2Person != null)
                                        {
                                            PG2Person.Email = pga[3].Trim();
                                            if (!string.IsNullOrEmpty(PG2Person.Email))
                                                bNeedToModPG2 = true;
                                        }
                                    }
                                    break;

                                case 4: // Address
                                    {
                                        if (PG2Person != null)
                                        {
                                            var PG2Adr = pga[4].Trim();
                                            if (Address.IsStreetSimilar(streetValue, PG2Adr))
                                            {
                                                if (!PG2Person.AddressId.HasValue && person.AddressId.HasValue)
                                                    PG2Person.AddressId = person.AddressId.Value;
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

            pv = props.Find(prop => prop.Key == "CMT");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Notes = pv.Value.Trim();
            }

            Person JGPerson = (bJustGetPerson) ? person.ShallowCopy() : person;
            Address JGnewAdr = (bJustGetPerson) ? newAdr.ShallowCopy() : newAdr;

            //**************************************************************************
            // Find Person #1 : Find person by SS# First
            //**************************************************************************
            if (!bPersonFound && bHasSSNum)
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
                    if (!bFoundOldAdr && !bMatchingOldAdr && (adr_id != -1))
                    {
                        oldAdr = ce.Address.AsNoTracking().FirstOrDefault(a => a.Id == adr_id);
                        bFoundOldAdr = (oldAdr != null);
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
                    if (!bFoundOldAdr && !bMatchingOldAdr && (adr_id != -1))
                    {
                        oldAdr = ce.Address.AsNoTracking().FirstOrDefault(a => a.Id == adr_id);
                        bFoundOldAdr = (oldAdr != null);
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
                    if (!bFoundOldAdr && !bMatchingOldAdr && (adr_id != -1))
                    {
                        oldAdr = ce.Address.AsNoTracking().FirstOrDefault(a => a.Id == adr_id);
                        bFoundOldAdr = (oldAdr != null);
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
                if (bValidNewAddress)
                {
                    // Set Status on new Adr 
                    var adrStatus = props.Find(prop => prop.Key == "AdrStatus");
                    if (!adrStatus.Equals(default(KeyValuePair<String, String>)) && (adrStatus.Value.Length > 0))
                        newAdr.Status = int.Parse(adrStatus.Value);

                    if (bMatchingOldAdr)
                    {
                        // Make sure all address fields are updated.
                        newAdr = Address.Merge(matchingOldAdr, newAdr);
                        try
                        {
                            ce.Address.Attach(newAdr);
                            ce.Entry(newAdr).State = EntityState.Modified;
                            ce.SaveChanges();
                            ce.Entry(newAdr).State = EntityState.Detached;
                        }
                        catch
                        {
                            ResPerson = person;
                            ResAddress = null;
                            return MgrStatus.Save_Address_Failed;
                        }
                        if (newAdr.Id != 0)
                            person.AddressId = newAdr.Id;

                    }
                    else
                    {
                        // Add new, valid address and set Person.Address_id
                        ce.Address.Add(newAdr);
                        if (ce.SaveChanges() < 1)
                        {
                            ResPerson = null;
                            ResAddress = null;
                            return MgrStatus.Save_Address_Failed;
                        }
                        if (newAdr.Id != 0)
                            person.AddressId = newAdr.Id;  // addedAdr.id;
                    }
                }
                else
                {
                    // Not Valid Address, use matching or old address
                    if (bMatchingOldAdr)
                    {
                        newAdr = matchingOldAdr;
                        person.AddressId = newAdr.Id;
                    }
                    else if (bFoundOldAdr)
                    {
                        newAdr = oldAdr;
                        person.AddressId = oldAdr.Id;
                    }
                    else
                    {
                        // No address
                        newAdr.Id = 0;
                        person.AddressId = null;
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
                            ce.Entry(PG1Person).State = EntityState.Detached;
                        }
                        catch
                        {
                            ResPerson = person;
                            ResAddress = null;
                            return MgrStatus.Save_PG_Failed;
                        }
                    }
                }

                // Filter out case where PG1Person is the same as PG2Person
                if ((PG1Person != null) && (PG2Person != null) && (PG1Person.FirstName != null) && (PG2Person.FirstName != null) && (PG1Person.LastName != null) && (PG2Person.LastName != null) &&
                    (!PG1Person.FirstName.Equals(PG2Person.FirstName, StringComparison.OrdinalIgnoreCase) || !PG1Person.LastName.Equals(PG2Person.LastName, StringComparison.OrdinalIgnoreCase)))
                {
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
                                ce.Entry(PG2Person).State = EntityState.Detached;
                            }
                        }
                        catch
                        {

                        }
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

        public static int FindPersonIDByName(CStatContext ce, ref Person person, bool bMerge, out int address_id, bool bCheckLastName = true, Person child = null)
        {
            String LName = person.LastName;

            var psnL = from psn in ce.Person.AsNoTracking()
                       where (psn.LastName == LName)
                       select psn;

            return FindPersonIDByName(psnL, ref person, bMerge, out address_id, bCheckLastName, child);
        }

        public static int FindPersonIDByName(IQueryable<Person> psnL, ref Person person, bool bMerge, out int address_id, bool bCheckLastName = true, Person child = null)
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
                        if ((p.Gender.HasValue) && (person.Gender.HasValue) && (person.Gender != p.Gender))
                            continue;
                    }
                    if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                    {
                        if (String.Compare(p.LastName, person.LastName, true) != 0)
                            continue;
                    }

                    if ((p.Alias != null) && (person.Alias != null) && (p.Alias.Length > 0) && (String.Compare(p.Alias, person.Alias, true) == 0) && ((child == null) || isAValidPG(MinP, child)))
                    {
                        if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                            address_id = p.AddressId.Value;

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

                if ((MinP != null) && (MinLD < 3) && ((child == null) || isAValidPG(MinP, child)))
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
                        if (person.Gender.HasValue && (person.Gender.HasValue) && (person.Gender != 0))
                        {
                            if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                                continue;
                        }
                        if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                        {
                            if (String.Compare(p.LastName, person.LastName, true) != 0)
                                continue;
                        }

                        if (p.FirstName.ToUpper().StartsWith(FStart4) && ((child == null) || isAValidPG(MinP, child)))
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

                        if (p.FirstName.ToUpper().StartsWith(FStart3) && ((child == null) || isAValidPG(MinP, child)))
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

        private static bool isAValidPG(Person p, Person child)
        {
            // Assumed person is a Parent/Guardian if child defined. Check to see if this is a child or the same as child with child expected to have a DOB
            if (p.Dob.HasValue && child.Dob.HasValue)
            {
                if ((p.Dob.Value - child.Dob.Value).TotalDays < 10 * 365)
                    return false;
            }

            if (String.Equals(child.FirstName, p.FirstName, StringComparison.InvariantCultureIgnoreCase) &&
                String.Equals(child.LastName, p.LastName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (p.Dob.HasValue && child.Dob.HasValue)
                {
                    if ((p.Dob.Value - child.Dob.Value).TotalDays < 14 * 365)
                        return false;
                }
            }
            return true;
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

        public static bool SplitName(String name, ref Person p, string defLast="") // Currently assumes no possible last name alias
        {
            String raw = name.Trim();
            if (string.IsNullOrEmpty(raw))
                return false;
            int isp = raw.LastIndexOf(" ");
            if ((isp <= 0) && !string.IsNullOrEmpty(defLast))
            {
                p.FirstName = raw;
                p.LastName = defLast;
                return true;
            }
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
        public string GetECRole()
        {
            long RoleMask = (long)TitleRoles.President | (long)Person.TitleRoles.Treasurer | (long)Person.TitleRoles.Secretary | (long)Person.TitleRoles.Vice_Pres | (long)Person.TitleRoles.Memb_at_Lg;
            if (!this.Roles.HasValue)
                return "";
            Person.TitleRoles role = (Person.TitleRoles)(this.Roles.Value & RoleMask);
            switch (role)
            {
                case Person.TitleRoles.President:
                    return "President";
                case Person.TitleRoles.Treasurer:
                    return "Treasurer";
                case Person.TitleRoles.Secretary:
                    return "Secretary";
                 case Person.TitleRoles.Vice_Pres:
                    return "VicePres.";
                case Person.TitleRoles.Memb_at_Lg:
                    return "MemAtLarge";
                default:
                    return "";
            }
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
        public static string ExportPeople (CStatContext ctx, bool fullInfo)
        {
            if (ctx == null)
                return "FAILED: No CStat DB Context";

            var pList = ctx.Person.Include(p => p.Address).Include(p => p.Church).AsNoTracking().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
            if (pList.Count == 0)
                return "FAILED: No People found. Valid CStat DB Context?";

            string LogPath = Path.Combine(CSLogger.WebRootPath, "Log");
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            DateTime now = PropMgr.ESTNow; 
            var ExpFile = Path.Combine(LogPath, (fullInfo ? "CSFullPeopleExport" : "CSPeopleList") + (now.Year-2000).ToString() + now.Month.ToString().PadLeft(2,'0') + now.Day.ToString().PadLeft(2, '0') + now.Hour.ToString().PadLeft(2, '0') + now.Minute.ToString().PadLeft(2, '0') + now.Second.ToString().PadLeft(2, '0') + ".csv");

            if (Person.fLock.TryEnterWriteLock(250))
            {
                string resStr = "FAILED: Unknown"; 
                try
                {
                    using (StreamWriter sw = new StreamWriter(ExpFile, false))
                    {
                        //"Id,First Name,Last Name,Alias,DOB,Gender,Status,Cell Phone,Email,Street,Town,State,Zip,Phone,Church,SkillSets,Notes,Roles,ContactPref,SS,Pg1PersonId,Pg2PersonId,ChurchId,AddressId,Fax,Country,WebSite"
                        sw.WriteLine(fullInfo ?
                            "Id,First Name,Last Name,Alias,DOB,Gender,Status,Cell Phone,Email,Street,Town,State,Zip,Phone,Church,SkillSets,Notes,Roles,ContactPref,SS,Pg1PersonId,Pg2PersonId,ChurchId,AddressId,Fax,Country,WebSite"
                          :
                            "First Name,Last Name,DOB,Gender,Cell Phone,Email,Street,Town,State,Zip,Phone,Church,Notes");

                        // use 166 a6 ¦ for separator and replace it lastly with ,
                        foreach (var p in pList)
                        {
                            //string line = GetStr(p.Id) + "¦" + GetStr(p.FirstName) + "¦" + GetStr(p.LastName) + "¦" + GetStr(p.Alias) + "¦" + GetStr(p.Dob) + "¦" + GetStr(p.Gender) + "¦" +
                            //    GetStr(p.Status) + "¦" + GetStr(p.CellPhone) + "¦" + GetStr(p.Email) + "¦" + GetStr(p.SkillSets) + "¦" + GetStr(p.Notes) + "¦" + GetStr(p.Roles) + "¦" +
                            //    GetStr(p.ContactPref) + "¦" + GetStr(p.Ssnum) + "¦" + GetStr(p.Pg1PersonId) + "¦" + GetStr(p.Pg2PersonId) + "¦" + GetStr(p.ChurchId) + "¦" + GetStr(p.AddressId) + "¦";
                            //line += (p.Church != null) ? GetStr(p.Church.Name) + "¦" : "¦";
                            //if (p.Address != null)
                            //{
                            //    line += GetStr(p.Address.Street) + "¦" +
                            //    GetStr(p.Address.Town) + "¦" +
                            //    GetStr(p.Address.State) + "¦" +
                            //    GetStr(p.Address.ZipCode) + "¦" +
                            //    GetStr(p.Address.Phone) + "¦" +
                            //    GetStr(p.Address.Fax) + "¦" +
                            //    GetStr(p.Address.Country) + "¦" +
                            //    GetStr(p.Address.WebSite);
                            //}
                            //else
                            //{
                            //    line += "¦¦¦¦¦¦¦";
                            //}

                            string line =
                                (fullInfo ? GetStr(p.Id) + "¦" : "") +
                                GetStr(p.FirstName) + "¦" +
                                GetStr(p.LastName) + "¦" +
                                (fullInfo ? GetStr(p.Alias) + "¦" : "") +
                                GetStr(p.Dob) + "¦" +
                                GetStr(p.Gender) + "¦" +
                                (fullInfo ? GetStr(p.Status) + "¦" : "") +
                                GetStr(p.CellPhone) + "¦" +
                                GetStr(p.Email) + "¦";

                            if ((p.Address != null) && ((p.Address.Status & (int)Address.AddressStatus.AdrStat_RTS) == 0))
                                {
                                    line += GetStr(p.Address.Street) + "¦" +
                                    GetStr(p.Address.Town) + "¦" +
                                    GetStr(p.Address.State) + "¦" +
                                    GetStr(p.Address.ZipCode) + "¦" +
                                    GetStr(p.Address.Phone) + "¦";
                                }
                                else
                                {
                                    line += "¦¦¦¦¦";
                                }
                                line += (p.Church != null) ? GetStr(p.Church.Name) + "¦" : "¦";
                                line += (fullInfo ? GetStr(p.SkillSets) + "¦" : "") +
                                GetStr(p.Notes) + "¦" +
                                (fullInfo ? GetStr(p.Roles) + "¦" : "") +
                                (fullInfo ? GetStr(p.ContactPref) + "¦" : "") +
                                (fullInfo ? GetStr(p.Ssnum) + "¦" : "") +
                                (fullInfo ? GetStr(p.Pg1PersonId) + "¦" : "") +
                                (fullInfo ? GetStr(p.Pg2PersonId) + "¦" : "") +
                                (fullInfo ? GetStr(p.ChurchId) + "¦" : "") +
                                (fullInfo ? GetStr(p.AddressId) + "¦" : "");

                                if (fullInfo)
                                {
                                    if (p.Address != null)
                                    {
                                        line += GetStr(p.Address.Fax) + "¦" +
                                        GetStr(p.Address.Country) + "¦" +
                                        GetStr(p.Address.WebSite);
                                    }
                                    else
                                    {
                                        line += "¦¦";
                                    }
                                }

                            line = line.Replace(",", ";").Replace("¦", ",");
                                sw.WriteLine(line);
                        }
                        sw.Close();
                    }
                    resStr = "SUCCESS: Created " + ExpFile;
                }
                catch (Exception e)
                {
                    resStr = "FAILED: " + e.Message;
                }
                finally
                {
                    Person.fLock.ExitWriteLock();
                }
                return resStr;
            }
            else
            {
                return "FAILED: Currently Busy";
            }

        }
        static string GetStr (string str)
        {
            return !string.IsNullOrEmpty(str) ? str : "";
        }
        static string GetStr(int val)
        {
            return val.ToString();
        }
        static string GetStr(int? val)
        {
            if (val.HasValue)
                return val.Value.ToString();
            return val.ToString();
        }
        static string GetStr(byte? bval)
        {
            if (bval.HasValue)
            {
                if (bval.Value == 70)
                    return "F";
                if (bval.Value == 77)
                    return "M";
            }
            return "";
        }
        static string GetStr(long lval)
        {
            return lval.ToString();
        }
        static string GetStr(long? lval)
        {
            if (lval.HasValue)
                return lval.Value.ToString();
            return "";
        }
        static string GetStr(DateTime? dt, bool full=false)
        {
            if (dt.HasValue && (dt.Value.Year != 1900))
                return full ? dt.Value.ToShortDateString() : dt.Value.ToShortDateString();
            return "";
        }

        private static readonly List<List<string>> SameNames = new List<List<string>>()
        {
            new List<string>{"Aaron","Erin","Ron","Ronnie"},
            new List<string>{"Abel","Ab","Abe","Eb","Ebbie"},
            new List<string>{"Abiah","Abijah","A.B.","Ab","Biah"},
            new List<string>{"Abiel","Biel","Ab"},
            new List<string>{"Abigail","Abby","Abbie","Abie","Nabby","Gail"},
            new List<string>{"Abner","Ab"},
            new List<string>{"Abraham","Abram","Abe"},
            new List<string>{"Adaline","Adeline","Ada","Addy","Dell","Delia","Lena","Adelle"},
            new List<string>{"Adam","Ad","Ade"},
            new List<string>{"Adelaide","Addy","Adele","Dell","Della","Heidi"},
            new List<string>{"Adelbert","Ad","Ade","Albert","Bert","Del","Delbert","Elbert"},
            new List<string>{"Adelphia","Adele","Addy","Dell","Delphia","Philly"},
            new List<string>{"Adolph","Adolphus","Ad","Dolph","Olph"},
            new List<string>{"Agatha","Aggy"},
            new List<string>{"Agnes","Aggy","Inez","Nessa"},
            new List<string>{"Aileen","Allie","Lena"},
            new List<string>{"Alanson","Alan","Al","Lonson"},
            new List<string>{"Albert","Al","Bert","Elbert","Adelbert"},
            new List<string>{"Alberta","Allie","Bert","Bertie"},
            new List<string>{"Aldrich","Al","Rich","Richie"},
            new List<string>{"Alexander","Al","Alex","Xander","Sandy","Eleck","Alec","Alek"},
            new List<string>{"Alexandra","Alex","Alla","Sandy"},
            new List<string>{"Alfred","Al","Fred"},
            new List<string>{"Alfred","Fred","Freddy","Alf","Alfie"},
            new List<string>{"Alfreda","Alfy","Freda","Freddy","Frieda"},
            new List<string>{"Alice","Alicia","Allie","Elsie","Lisa"},
            new List<string>{"Alison","Ali","Ally","Allie"},
            new List<string>{"Almena","Allie","Mena"},
            new List<string>{"Alonzo","Al","Lon","Lonzo"},
            new List<string>{"Amanda","Manda","Mandy","Mindy"},
            new List<string>{"Amelia","Emily","Mel","Millie","Amy"},
            new List<string>{"Anderson","Ander","Andy","Sonny"},
            new List<string>{"Andrew","Andy","Drew"},
            new List<string>{"Ann","Anne","Annie","Nan","Nanny","Nana","Nancy","Antoinette","Christiana","Roseanne"},
            new List<string>{"Anthony","Tony"},
            new List<string>{"Antoinette","Antonia","Ann","Tony","Netta"},
            new List<string>{"Arabella","Arabelle","Ara","Arry","Belle","Bella"},
            new List<string>{"Archibald","Archie","Baldo"},
            new List<string>{"Arlene","Arly","Lena"},
            new List<string>{"Armena","Arry","Mena"},
            new List<string>{"Arthur","Art"},
            new List<string>{"Asahel","Asa","Asaph"},
            new List<string>{"Asenath","Assene","Natty","Sene"},
            new List<string>{"Augusta","Augustina","Aggy","Gatsy","Gussie","Tina"},
            new List<string>{"Augustus","Augustine","August","Austin","Gus"},
            new List<string>{"Azariah","Aze","Riah"},
            new List<string>{"Barbara","Barb","Barbie","Bab","Babs","Barby"},
            new List<string>{"Barnabas","Barney"},
            new List<string>{"Bartholomew","Bart","Bartel","Bat","Mees","Meus"},
            new List<string>{"Beatrice","Bea","Trisha","Trix","Trixie"},
            new List<string>{"Belinda","Belle","Linda"},
            new List<string>{"Benedict","Ben","Bennie"},
            new List<string>{"Benjamin","Ben","Bennie","Benjy","Benny"},
            new List<string>{"Bernard","Barney","Berney","Bernie"},
            new List<string>{"Bernice","Bunny"},
            new List<string>{"Bertha","Birdie","Bert","Bertie"},
            new List<string>{"Beverly","Beverley","Bev"},
            new List<string>{"Bradford","Brad","Ford"},
            new List<string>{"Bradley","Brad"},
            new List<string>{"Bridget","Biddie","Biddy","Bridgie","Bridie"},
            new List<string>{"Broderick","Ricky","Brady","Brody"},
            new List<string>{"Calvin","Cal","Vin","Vinny"},
            new List<string>{"Cameron","Cam","Ronny","Ron"},
            new List<string>{"Camille","Cammie","Millie"},
            new List<string>{"Carol","Caroline","Carolyn","Carrie","Cassie","Lynn"},
            new List<string>{"Casey","Kasey","KC","K.C."},
            new List<string>{"Cassandra","Cassie","Sandra","Sandy"},
            new List<string>{"Catherine","Cathleen","Katherine","Kathleen","Cathy","Kathy","Katy","Cassie","Kay","Kit","Kittie","Trina","Lena"},
            new List<string>{"Cecilia","Celia","Cissy"},
            new List<string>{"Cedric","Ced","Rick","Ricky"},
            new List<string>{"Charles","Carl","Charlie","Chick","Chuck"},
            new List<string>{"Charles","Charlie","Charley","Chuck","Charley"},
            new List<string>{"Charlotte","Lotte","Lottie","Char","Charlie"},
            new List<string>{"Chauncey","Chan"},
            new List<string>{"Chester","Chet"},
            new List<string>{"Christine","Christina","Christiana","Kristine","etc.","Chris","Kris","Crissy","Christy","Kristy","Tina"},
            new List<string>{"Christopher","Christian","Chris","Kit"},
            new List<string>{"Clarence","Clair","Clare"},
            new List<string>{"Clarinda","Clara"},
            new List<string>{"Clarissa","Clara","Cissy"},
            new List<string>{"Clement","Clem"},
            new List<string>{"Clifford","Cliff","Ford","Clifton"},
            new List<string>{"Columbus","Clum"},
            new List<string>{"Conrad","Con","Conny"},
            new List<string>{"Constance","Connie"},
            new List<string>{"Cordelia","Cordy","Delia"},
            new List<string>{"Cornelia","Corny","Nelle","Nelly","Nelia","Cornie"},
            new List<string>{"Cornelius","Con","Conny","Corny","Niel"},
            new List<string>{"Courtney","Court","Curt"},
            new List<string>{"Curtis","Curt"},
            new List<string>{"Cynthia","Cindy","Cintha"},
            new List<string>{"Cyrenius","Cene","Cy","Renius","Serene","Swene"},
            new List<string>{"Dalton","Dahl","Dal"},
            new List<string>{"Daniel","Dan","Danny"},
            new List<string>{"Darlene","Lena","Darry"},
            new List<string>{"David","Dave","Davey","Day"},
            new List<string>{"Deborah","Debora","Debby","Debbie","Deb"},
            new List<string>{"Delbert","Bert","Del"},
            new List<string>{"Deliverance","Della","Delly","Dilly"},
            new List<string>{"Delores","Dell","Lola","Lolly","Della","Dee"},
            new List<string>{"Dennis","Denny","maybeshortforDennison"},
            new List<string>{"Derrick","Eric","Rick","Ricky"},
            new List<string>{"Dicey","Dicie"},
            new List<string>{"Donald","Don","Donny"},
            new List<string>{"Dorcus","Darkey"},
            new List<string>{"Dorothy","Dolly","Dot","Dortha","Dotty"},
            new List<string>{"Duncan","Dunc"},
            new List<string>{"Ebenezer","Eben","Eb","Ebbie"},
            new List<string>{"Edith","Edie"},
            new List<string>{"Edmund","Ed","Ned","Ted"},
            new List<string>{"Edward","Ed","Eddie","Eddy","Ned"},
            new List<string>{"Edwin","Ed","Ned","Win"},
            new List<string>{"Elaine","Eleanor"},
            new List<string>{"Elbert","seeAdelbert","Albert"},
            new List<string>{"Eleanor","Elaine","Ellen","Ellie","Lanna","Lenora","Nelly","Nora"},
            new List<string>{"Eleazer","Lazar"},
            new List<string>{"Elias","Eli","Lee","Lias"},
            new List<string>{"Elijah","Eli","Lige"},
            new List<string>{"Eliphalet","Left"},
            new List<string>{"Elisha","Eli","Lish"},
            new List<string>{"Elizabeth","Eliza","Bess","Bessie","Beth","Betsy","Betty","Lib","Libby","Liza","Lisa","Liz","Lizzie"},
            new List<string>{"Elizabeth","Liz","Lizzie","Liza","Libb","Lisbeth","Beth","Bessie","Bess"},
            new List<string>{"Ellen","Eleanor","Helen"},
            new List<string>{"Ellswood","Elswood","Elsey","Elze","Elsie"},
            new List<string>{"Elmira","Elly","Ellie","Mira"},
            new List<string>{"Elouise","Louise","seeHeloise"},
            new List<string>{"Elwood","Woody"},
            new List<string>{"Emanuel","Manny","Manuel"},
            new List<string>{"Emeline","Em","Emily","Emma","Emmy","Milly"},
            new List<string>{"Emily","Em","Emy"},
            new List<string>{"Emily","Emmy","Millie","Emma","Ameliand","Emeline"},
            new List<string>{"Epaphroditius","Dite","Ditus","Dyce","Dyche","Eppa"},
            new List<string>{"Ephraim","Eph"},
            new List<string>{"Eric","Rick","Ricky","Derrick"},
            new List<string>{"Ernest","Ernie"},
            new List<string>{"Estelle","Estella","Essy","Stella"},
            new List<string>{"Esther","seeHester"},
            new List<string>{"Eugene","Gene"},
            new List<string>{"Evaline","Eva","Eve","Lena"},
            new List<string>{"Ezekiel","Ez","Zeke"},
            new List<string>{"Ezra","Ez"},
            new List<string>{"Faith","Fay"},
            new List<string>{"Ferdinand","Ferdie"},
            new List<string>{"Fidelia","Delia"},
            new List<string>{"Florence","Flo","Flora","Flossy"},
            new List<string>{"Frances","Fanny","Fran","Cissy","Frankie","Sis"},
            new List<string>{"Francis","Frank","Fran"},
            new List<string>{"Franklin","Frank","Fran"},
            new List<string>{"Frederick","Fred","Freddy","Fritz"},
            new List<string>{"Fredericka","Freda","Freddy","Ricka","Frieda"},
            new List<string>{"Gabriel","Gabe","Gabby"},
            new List<string>{"Gabrielle","Gabriella","Gabby","Ella"},
            new List<string>{"Gail","Abigail"},
            new List<string>{"Genevieve","Eve","Jean","Jenny"},
            new List<string>{"Geoffrey","Jeffrey","Geoff","Jeff"},
            new List<string>{"Georgia","Georgie","Georgy"},
            new List<string>{"Gerald","Jerry","Gerry"},
            new List<string>{"Geraldine","Dina","Gerry","Gerrie","Jerry"},
            new List<string>{"Gertrude","Trudy","Gert","Gertie"},
            new List<string>{"Gilbert","Gil","Bert","Wilber"},
            new List<string>{"Gwendolyn","Gwen","Wendy"},
            new List<string>{"Hannah","Nan","Nanny","Anna"},
            new List<string>{"Harold","Hal","Harry"},
            new List<string>{"Harriet","Hattie"},
            new List<string>{"Heidi","Adelaide"},
            new List<string>{"Helen","Helene","Ella","Ellen","Ellie","Lena"},
            new List<string>{"Heloise","Eloise","Lois"},
            new List<string>{"Henrietta","Etta","Etty","Hank","Nettie","Retta"},
            new List<string>{"Henry","Hal","Hank","Harry"},
            new List<string>{"Hepsibah","Hephsibah","Hipsie"},
            new List<string>{"Herbert","Herb","Bert"},
            new List<string>{"Hermione","Hermie"},
            new List<string>{"Hester","Hessy","Esther","Hetty"},
            new List<string>{"Hezekiah","Hez","Hy","Kiah"},
            new List<string>{"Hilary","Hil"},
            new List<string>{"Hiram","Hy"},
            new List<string>{"Hopkins","Hop","Hopp"},
            new List<string>{"Horace","Horry"},
            new List<string>{"Hubert","Hugh","Bert","Hub"},
            new List<string>{"Ignatius","Iggy","Nace","Nate","Natius"},
            new List<string>{"Ina","seeLavina"},
            new List<string>{"Inez","seeAgnes"},
            new List<string>{"Irene","Rena"},
            new List<string>{"Isaac","Ike","Zeke"},
            new List<string>{"Isabel","Isabelle","Isabella","Bella","Belle","Ib","Issy","Nib","Nibby","Tibbie"},
            new List<string>{"Isadora","Dora","Issy"},
            new List<string>{"Isidore","Izzy"},
            new List<string>{"Jacob","Jaap","Jake","Jay","Jacobus","Jacob"},
            new List<string>{"James","Jamie","Jim","Jimmy","Jimbo"},
            new List<string>{"Jane","Janie","Jean","Jennie","Jessie"},
            new List<string>{"Janet","Jessie","Jan"},
            new List<string>{"Jay","Jacob"},
            new List<string>{"Jean","Jeanne","Jane","Jeannie"},
            new List<string>{"Jeanette","Janet","Jean","Jessie","Nettie"},
            new List<string>{"Jedidiah","Jed"},
            new List<string>{"Jefferson","Jeff","Sonny"},
            new List<string>{"Jehiel","Hiel"},
            new List<string>{"Jemima","Mima"},
            new List<string>{"Jennifer","Jenny","Jen","Jennie"},
            new List<string>{"Jeremiah","Jereme","Jerry"},
            new List<string>{"Jessica","Jessie"},
            new List<string>{"Joan","Nonie","Joanna","Johannah"},
            new List<string>{"Joanna","Johannah","Joan","Jody","Hannah"},
            new List<string>{"John","Jack","Jock","Johnny","Jonathan","Nathan","Jon"},
            new List<string>{"John","Johnny","Jack","Jake"},
            new List<string>{"Jonathan","Jon"},
            new List<string>{"Joseph","Joe","Joey","Jos","Jody"},
            new List<string>{"Joseph","Joe","Joey"},
            new List<string>{"Josephine","Jody","Jo","Joey","Josey","Fina"},
            new List<string>{"Joshua","Josh","Jos"},
            new List<string>{"Josiah","Jos"},
            new List<string>{"Joyce","Joy"},
            new List<string>{"Juanita","Nettie","Nita"},
            new List<string>{"Judson","Jud","Sonny"},
            new List<string>{"Julia","Julie","Jill"},
            new List<string>{"Julias","Julian","Jule"},
            new List<string>{"Junior","JR","June","Junie"},
            new List<string>{"K.C.","KC","Kasey","Casey"},
            new List<string>{"Kathryn","Kate","Katie"},
            new List<string>{"Kenneth","Ken","Kenny","Kendall","Kendrick",""},
            new List<string>{"Kent","etc."},
            new List<string>{"Keziah","Kizza","Kizzie"},
            new List<string>{"King","Kingston","Kingsley"},
            new List<string>{"Lafayette","Fate","Laffie"},
            new List<string>{"Lamont","Monty"},
            new List<string>{"Lavina","Lavinia","Ina","Vina","Viney"},
            new List<string>{"Lawrence","Laurence","Larry","Lon","Lonny","Lorne","Lorry"},
            new List<string>{"Lemuel","Lem"},
            new List<string>{"Lenora","Nora","Lee","seeEleanor"},
            new List<string>{"Leonard","Leo","Leon","Len","Lenny","Lineau"},
            new List<string>{"LeRoy","L.R.","Lee","Roy"},
            new List<string>{"Leslie","Les"},
            new List<string>{"Lester","Les"},
            new List<string>{"Letitia","Lettie","Lettice","Titia","Tish"},
            new List<string>{"Levi","Lee"},
            new List<string>{"Lillah","Lily","Lilly","Lil","Lolly"},
            new List<string>{"Lillian","Lil","Lilly","Lolly"},
            new List<string>{"Lincoln","Link"},
            new List<string>{"Linda","Lindy","Lynn","Belinda","Melinda","Philinda"},
            new List<string>{"Lisa","Alice","Melissa"},
            new List<string>{"Lois","Heloise","Louise"},
            new List<string>{"Lorenzo","Loren"},
            new List<string>{"Loretta","Etta","Lorrie","Retta"},
            new List<string>{"Lorraine","Lorrie"},
            new List<string>{"Louis","Lou","Louie"},
            new List<string>{"Louise","Louisa","Lou","Eliza","Lois"},
            new List<string>{"Lucias","Lucas","Luke"},
            new List<string>{"Lucille","Lu","Lou","Cille","Lucy"},
            new List<string>{"Lucinda","Cindy","Lu","Lou","Lucy"},
            new List<string>{"Luella","Ella","Lu","Lula"},
            new List<string>{"Luther","Luke"},
            new List<string>{"Lydia","Lidia","Lyddy"},
            new List<string>{"Lyndon","Lynn","Lindy"},
            new List<string>{"Lynn","Carolyn","Madeline","Linda"},
            new List<string>{"Madeline","Lena","Maddy","Madge","Magda","Maggie","Maida","Maud"},
            new List<string>{"Magdelina","Lena","Madge","Magda"},
            new List<string>{"Mahala","Hallie"},
            new List<string>{"Marcus","Mark"},
            new List<string>{"Margaret","Maggie","Meg","Maisie"},
            new List<string>{"Margaret","Margaretta","Daisy","Gretta","Madge","Maggie","Meg","Midge","Peg","Peggy","Rita","Margery","Marge","Margie"},
            new List<string>{"Martha","Marty","Mat","Mattie","Patsy","Patty"},
            new List<string>{"Martin","Marty"},
            new List<string>{"Marvin","Marv"},
            new List<string>{"Mary","Molly","Polly","Mae","Mamie","Mitzi"},
            new List<string>{"Matilda","Tilly","Matty","Maud"},
            new List<string>{"Matthew","Matthias","Matt","Thias","Thys"},
            new List<string>{"Maurice","Morris","Morey"},
            new List<string>{"Mehitabel","Hetty","Hitty","Mabel","Mitty"},
            new List<string>{"Melinda","Linda","Lindy","Mel","Mindy"},
            new List<string>{"Melissa","Lisa","Lissa","Mel","Milly","Missy"},
            new List<string>{"Mervyn","Merv"},
            new List<string>{"Micah","Michael"},
            new List<string>{"Michael","Mike","Micah","Mick","Micky"},
            new List<string>{"Michael","Mike","Mick","Micky"},
            new List<string>{"Michelle","Mickey"},
            new List<string>{"Mildred","Milly"},
            new List<string>{"Millicent","Milly","Missy"},
            new List<string>{"Minerva","Minnie"},
            new List<string>{"Miranda","Mandy","Mira","Randy"},
            new List<string>{"Miriam","Mimi","Mitzi"},
            new List<string>{"Mitchell","Mitch"},
            new List<string>{"Montgomery","Monty","Gum"},
            new List<string>{"Morris","Morry","Morrie"},
            new List<string>{"Morton","Mort","Morty"},
            new List<string>{"Nancy","Nan","seealsoAnn"},
            new List<string>{"Napoleon","Nap","Nappy","Leon"},
            new List<string>{"Natalie","Natty","Nettie"},
            new List<string>{"Natasha","Tash","Tasha"},
            new List<string>{"Nathaniel","Nathan","Nate","Nat","Natty","Than"},
            new List<string>{"Nicholas","Nick","Claas","Claes","Nicky"},
            new List<string>{"Nicola","Nicky"},
            new List<string>{"Norbert","Bert","Norby"},
            new List<string>{"Obadiah","Diah","Dyer","Obed","Obie"},
            new List<string>{"Olive","Olivia","Ollie","Nollie","Livia"},
            new List<string>{"Oliver","Ollie"},
            new List<string>{"Oswald","Ossy","Ozzy","Waldo"},
            new List<string>{"Pamela","Pam","Pammy"},
            new List<string>{"Parmelia","Amelia","Melia","Milly"},
            new List<string>{"Patience","Pat","Patty"},
            new List<string>{"Patricia","Pat","Patty","Patsy","Tricia"},
            new List<string>{"Patricia","Pat","Patty","Patsy","Trish","Trisha"},
            new List<string>{"Patrick","Paddy","Pat","Patsy","Pate"},
            new List<string>{"Paul","Pauly","Pauley"},
            new List<string>{"Paula","Paulina","Polly","Lina"},
            new List<string>{"Pelegrine","Perry"},
            new List<string>{"Penelope","Penny"},
            new List<string>{"Percival","Percy"},
            new List<string>{"Peter","Pete","Pate","Petey","Pita"},
            new List<string>{"Philetus","Leet","Phil"},
            new List<string>{"Philinda","Linda"},
            new List<string>{"Philip","Phil"},
            new List<string>{"Philippa","Pippa"},
            new List<string>{"Phillip","Phil"},
            new List<string>{"Prescott","Scott","Scotty","Pres"},
            new List<string>{"Priscilla","Cissy","Cilla","Prissy"},
            new List<string>{"Prudence","Prudy","Prue"},
            new List<string>{"Rachel","Rae"},
            new List<string>{"Rachel","Shelly"},
            new List<string>{"Ralph","Raphael"},
            new List<string>{"Randolph","Randy","Dolph"},
            new List<string>{"Raymond","Ray"},
            new List<string>{"Rebecca","Reba","Becca","Becky","Beck"},
            new List<string>{"Regina","Reggie","Gina"},
            new List<string>{"Reginald","Reg","Reggie","Naldo","Renny"},
            new List<string>{"Relief","Leafa"},
            new List<string>{"Reuben","Rube"},
            new List<string>{"Richard","Dick","Dickon","Rich","Rick","Ricky"},
            new List<string>{"Richard","Richie","Rick","Dick"},
            new List<string>{"Robert","Bob","Dob","Dobbin","Hob","Hobkin","Rob","Robby","Bobby","Robin","Rupert","Robbie"},
            new List<string>{"Roberta","Bobbie","Robbie","Bert","BertieorBirdie"},
            new List<string>{"Roger","Rodger","Roge","Hodge","Rod"},
            new List<string>{"Roland","Lanny","Rollo","Rolly","Orlando","Ron","Ronnie"},
            new List<string>{"Ronald","Ron","Ronny","Naldo","Ronnie"},
            new List<string>{"Rosabel","Rosabella","Belle","Rosa","Rose","Roz"},
            new List<string>{"Rosalyn","Rosalinda","Rosa","Rose","Linda","Roz"},
            new List<string>{"Roseann","Roseanna","Rose","Ann","Roz","Rosie"},
            new List<string>{"Roxanne","Roxanna","Ann","Rose","Roxie"},
            new List<string>{"Rudolph","Rudolphus","Dolph","Olph","Rolf","Rudy"},
            new List<string>{"Rupert","Robert"},
            new List<string>{"Russell","Russ","Rusty"},
            new List<string>{"Sabrina","Brina"},
            new List<string>{"Sally","Sal"},
            new List<string>{"Salvatore","Sal"},
            new List<string>{"Samuel","Sam","Sammy"},
            new List<string>{"Sandra","Sandy","Cassandra","Alexandra"},
            new List<string>{"Sarah","Sally","Sadie"},
            new List<string>{"Selina","Celina","Lena"},
            new List<string>{"Serena","Rena"},
            new List<string>{"Seymour","Morey","See"},
            new List<string>{"Shelton","Shelly","Shel","Tony"},
            new List<string>{"Sheridan","Dan","Danny","Sher"},
            new List<string>{"Shirley","Lee","Sherry","Shirl"},
            new List<string>{"Sidney","Sid","Syd"},
            new List<string>{"Silas","Si"},
            new List<string>{"Simon","Si"},
            new List<string>{"Simon","Simeon","Si","Sion"},
            new List<string>{"Smith","Smitty"},
            new List<string>{"Solomon","Sal","Salmon","Saul","Sol","Solly","Zolly"},
            new List<string>{"Sonia","Sonie"},
            new List<string>{"Stephen","Steven","Steve","Ste","Steph"},
            new List<string>{"Stephen","Steven","Steve","Steph"},
            new List<string>{"Stewart","Stu"},
            new List<string>{"Submit","Mitty"},
            new List<string>{"Sullivan","Sully","Van"},
            new List<string>{"Susan","Susannah","Sue","Sukey","Susie","Hannah"},
            new List<string>{"Susan","Suzie","Sue","Suze"},
            new List<string>{"Sylvester","Si","Sly","Sy","Syl","Vester","Vet","Vessie"},
            new List<string>{"Tabitha","Tabby"},
            new List<string>{"Terence","Terry"},
            new List<string>{"Thaddeus","Thad"},
            new List<string>{"Theodore","Ted","Teddy","Theo"},
            new List<string>{"Theresa","Terry","Tess","Tessie","Tessa","Thirza","Thursa","Tracy"},
            new List<string>{"Thomas","Tom","Thom","Tommy"},
            new List<string>{"Timothy","Tim","Timmy"},
            new List<string>{"Tobias","Toby","Bias"},
            new List<string>{"Tryphena","Phena"},
            new List<string>{"Uriah","Riah"},
            new List<string>{"Valentine","Felty","Feltie"},
            new List<string>{"Valerie","Val"},
            new List<string>{"Vanessa","Nessa","Essa","Vanna"},
            new List<string>{"Veronica","Franky","Frony","Ron","Ronnie","Ronna","Vonnie"},
            new List<string>{"Victoria","Vicky","Torry","Tori","Torrie","Torri","Vic"},
            new List<string>{"Vincent","Vinson","Vince","Vin","Vinny"},
            new List<string>{"Virginia","Ginger","Ginny","Jane","Jennie","Virgy"},
            new List<string>{"Wallace","Wally"},
            new List<string>{"Washington","Wash"},
            new List<string>{"Wilber","Wilbur","Will","Gilbert"},
            new List<string>{"Wilfred","Will","Willie","Fred"},
            new List<string>{"Wilhelmina","Mina","Willie","Wilma","Minnie"},
            new List<string>{"William","Bill","Billy","Will","Willy","Willie"},
            new List<string>{"Winfield","Win","Winny","Field"},
            new List<string>{"Winifred","Winnie"},
            new List<string>{"Woodrow","Wood","Woody","Drew"},
            new List<string>{"Zachariah","Zach","Zachy","Zeke","Zak","Zack"},
            new List<string>{"Zebedee","Zeb"},
            new List<string>{"Zedediah","Zed","Diah","Dyer"},
            new List<string>{"Zephaniah","Zeph"}
        };

        public static string[] FindSimilarNames(string fn)
        {
            if (String.IsNullOrEmpty(fn))
                return new string[] { };

            var found = SameNames.FirstOrDefault((list => list.Any(n => n.Equals(fn, StringComparison.OrdinalIgnoreCase))));
            if ((found?.Count() ?? 0) > 0)
                return found.ToArray().Where(n => !n.Equals(fn, StringComparison.OrdinalIgnoreCase)).ToArray();
            return new string[] { };
        }

        public static bool SameFirstName(string fn1, string fn2)
        {
            if (String.IsNullOrEmpty(fn1) || String.IsNullOrEmpty(fn2))
                return false;

            if (fn1.Equals(fn2, StringComparison.OrdinalIgnoreCase))
                return true;
            return SameNames.Any(list => list.Any(n => n.Equals(fn1, StringComparison.OrdinalIgnoreCase) && list.Any(n => n.Equals(fn2, StringComparison.OrdinalIgnoreCase))));
        }

        public static string GetBestPhone(Person p, string emptyStr = "unknown #")
        {
            return !string.IsNullOrEmpty(p.CellPhone) ? Person.FixPhone(p.CellPhone) : (((p.Address != null) && !string.IsNullOrEmpty(p.Address.Phone)) ? Person.FixPhone(p.Address.Phone) : emptyStr);
        }

        public static string GetAllPhones(Person p, string emptyStr = "unknown #")
        {
            string phone = emptyStr;
            string cell = !string.IsNullOrEmpty(p.CellPhone) ? Person.FixPhone(p.CellPhone) : "";
            string home = ((p.Address != null) && !string.IsNullOrEmpty(p.Address.Phone)) ? Person.FixPhone(p.Address.Phone) : "";
            bool hasCell = !String.IsNullOrEmpty(cell);
            bool hasHome = !String.IsNullOrEmpty(home);
            bool hasBoth = hasCell && hasHome && (cell != home);
            if (hasBoth)
                return "C:" + cell + " H:" + home;
            if (hasCell)
                return cell;
            if (hasHome)
                return home;
            return phone;
        }

        public static string GetGender(Person p)
        {
           return p.Gender.HasValue ? ((p.Gender.Value == (byte)'M') ? "M" : ((p.Gender.Value == (byte)'F') ? "F" : "")) : "";
        }

        public static string GetDOB(Person p)
        {
            return (p.Dob.HasValue && (p.Dob.Value.Year != 1900)) ? p.Dob.Value.Date.ToString("MM/dd/yy") : "---";
        }

        public static string GetRoleStr(Person p)
        {
            var ecRole = p.GetECRole();
            string rstr = (!string.IsNullOrEmpty(ecRole) ? ecRole : "").Trim();
            var exp = p.GetTermExpire();
            if (exp != 0)
                rstr += " Exp.Nov." + exp.ToString();
            return rstr;
        }

        public static string GetEMailStr(Person p)
        {
            return (!string.IsNullOrEmpty(p.Email) ? p.Email : "");
        }

        public static string GetSkillStr(Person p)
        {
            string sstr = "";
            if (p.SkillSets == 0)
                return "";
            foreach (int sval in Enum.GetValues(typeof(eSkills)))
            {
                if ((p.SkillSets & (long)sval) != 0)
                    sstr += ((sstr.Length > 0) ? "," : "") + ((eSkills)sval).ToString();               
            }
            return "[" + sstr.Trim() + "]";
        }

    }
}
