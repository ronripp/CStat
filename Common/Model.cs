using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace CStat.Models
{
    public class _CCADataEntities
    {
        private readonly CStat.Models.CStatContext _context;

        public _CCADataEntities(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public DbContext Get()
        {
            return _context;
        }
    }
    public partial class InventoryItem
    {
        public enum States { InStock = 0, OpenNeed = 1, TakenNeed = 2 };
        public enum ItemUnits
        {
            unknown = 0,
            bags = 1,
            bladders = 2,
            bottles = 3,
            boxes = 4,
            bulbs = 5,
            drums = 6,
            jugs = 7,
            ounces = 8,
            pairs = 9,
            pieces = 10,
            reams = 11,
            rolls = 12,
            sheets = 13,
            tablets = 14
        };

        public enum ItemZone
        {
            unknown = 0,
            Office = 1,
            Kitchen_Front = 2,
            Kitchen_Mid = 3,
            Kitchen_Back = 4,
            Basement_Shelves = 5,
            Basement_Closet = 6,
            Laundry_Room = 7,
            Basement_Tanks = 8,
            Basement_Heater = 9
        };
    }

    public partial class Person
    {
        public enum eGender
        {
            M = (int)(uint)(byte)'M',
            F = (int)(uint)(byte)'F'
        };

        public enum eSkills
        { 
            Carpentry    = 0x1,
            Plumbing     = 0x2,   
            Roofing      = 0x4,   
            Cook         = 0x8,   
            Nurse        = 0x10,  
            SWAT         = 0x20,  
            Manager      = 0x40,  
            Painter      = 0x80,  
            Minister     = 0x100, 
            Dean         = 0x200, 
            Accountant   = 0x400, 
            Electricn    = 0x800, 
            Computer     = 0x1000,
            Kitch_Help   = 0x2000,
            Counselor    = 0x4000,
            Gardening    = 0x8000,
            Tree_Cut     = 0x10000,
            Worship      = 0x20000, 
            Worker       = 0x40000, 
            Mason        = 0x80000,
            Constructn   = 0x100000,
            Septic       = 0x200000
        };

        public enum eSkillsAbbr
        {
            CRP = 1,
            PLB = 2,
            ROF = 4,
            COK = 8,
            HLD = 10,
            SWT = 20,
            CMG = 40,
            PNT = 80,
            MNS = 100,
            DEN = 200,
            ACC = 400,
            ELE = 800,
            CMP = 1000,
            KTH = 2000,
            CNS = 4000,
            GAR = 8000,
            TRR = 10000,
            WRS = 20000,
            WRK = 40000,
            MSN = 80000,
            CON = 100000,
            SPT = 200000
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
            catch (Exception e)
            {
            }

            return "No one found with Name";
        }
    }

    public partial class Church
    {
        public enum MemberStatType
        {
            UNK_M = 0x0,
            MIGS, // Member in Good Standing
            PRIOR_M,
            EXP_M,
            SUSP_M,
            NOT_M,
            NumMemberStats
        }
        public enum AffiliationType
        {
            ICCOC = 0,
            RC,
            LUTH,
            ANG,
            EPIS,
            SOBAP,
            METH,
            AMBAP,
            IFBAP,
            AOG,
            PRESB,
            NICOC,
            CONG,
            NOND,
            OTHER,
            UNK,
            NumAffiliations
        }

        public class ChurchData
        {
            public int id { get; set; }
            public String Name { get; set; }
            public String Affiliation { get; set; }
            public String Status { get; set; }

            public int MembershipStatus { get; set; }
            public String Street { get; set; }
            public String City { get; set; }
            public String State { get; set; }
            public String Zip { get; set; }
            public String Phone { get; set; }
            public String Minister_id { get; set; }
            public String YouthPastor_id { get; set; }
            public String Trustee1_id { get; set; }
            public String Trustee2_id { get; set; }
            public String Trustee3_id { get; set; }
            public String Alt1_id { get; set; }
            public String Alt2_id { get; set; }
            public String Alt3_id { get; set; }
        }

        public class ChurchAtt
        {
            public ChurchAtt(String n, AffiliationType t) { name = n; aff_type = t; bAffiliation = true; }
            public ChurchAtt(String n, MemberStatType t) { name = n; ms_type = t; bAffiliation = false; }
            public String name { get; set; }
            public AffiliationType aff_type { get; set; }
            public MemberStatType ms_type { get; set; }
            private bool bAffiliation;
            public int GetEnumType()
            {
                if (bAffiliation)
                    return (int)aff_type;
                else
                    return (int)ms_type;
            }
            public override string ToString()
            {
                return name;
            }
            public string ToAbbrString()
            {
                if (bAffiliation)
                    return aff_type.ToString();
                else
                    return ms_type.ToString();
            }

        }

        public class ChurchLists
        {
            public static ChurchAtt[] AffiliationList = new ChurchAtt[]
            {
                new ChurchAtt("Indep. Christian / COC",   AffiliationType.ICCOC),
                new ChurchAtt("Roman Catholic",       AffiliationType.RC),
                new ChurchAtt("Lutheran",             AffiliationType.LUTH),
                new ChurchAtt("Anglican",             AffiliationType.ANG),
                new ChurchAtt("Episcopal",            AffiliationType.EPIS),
                new ChurchAtt("Southern Baptist",     AffiliationType.SOBAP),
                new ChurchAtt("Methodist",            AffiliationType.METH),
                new ChurchAtt("American Baptist",     AffiliationType.AMBAP),
                new ChurchAtt("Indep. Fund. Baptist",     AffiliationType.IFBAP),
                new ChurchAtt("Assembly of God",      AffiliationType.AOG),
                new ChurchAtt("Presbyterian",         AffiliationType.PRESB),
                new ChurchAtt("Non Instrumental COC", AffiliationType.NICOC),
                new ChurchAtt("Congregational UCOC",    AffiliationType.CONG),
                new ChurchAtt("Non Denominational",   AffiliationType.NOND),
                new ChurchAtt("Other",                AffiliationType.OTHER),
                new ChurchAtt("Unknown",              AffiliationType.UNK)
            };

            public static ChurchAtt[] MemberStatList = new ChurchAtt[]
            {
                new ChurchAtt("Membership Unknown", MemberStatType.UNK_M),
                new ChurchAtt("Mem. Good Standing", MemberStatType.MIGS),
                new ChurchAtt("Prior Member",       MemberStatType.PRIOR_M),
                new ChurchAtt("Expelled Member",    MemberStatType.EXP_M),
                new ChurchAtt("Suspended Member",   MemberStatType.SUSP_M),
                new ChurchAtt("Not a Member",       MemberStatType.NOT_M)
            };
        };

    }

    public class CCAChurch : Church
    {
        public Church.MemberStatType Membership_Status = 0;
    }

    public partial class Event
    {
        public enum EventType
        {
            Winter_Retreat = 0x101,
            Womans_Retreat,
            Mens_Retreat,
            Church_Retreat,
            Spring_Work_Retreat,
            First_Chance_Retreat,
            Elementary_Week,
            Intermediate_Week,
            Senior_Week,
            VOE_Week,
            Young_Adult_Retreat,
            BWAC_Fall_Retreat,
            Banquet,
            Fund_Raiser,
            Family_Retreat,
            Annual_Meeting,
            SignIn_Sheet,
            Other
        }

        public class EventData
        {
            public int id { get; set; }
            public String Description { get; set; }
            public String Type { get; set; }
            public DateTime StartDT { get; set; }
            public DateTime EndDT { get; set; }
            public decimal costChild { get; set; }
            public decimal costAdult { get; set; }
            public decimal costFamily { get; set; }
            public int churchID { get; set; }
            public decimal costLodge { get; set; }
            public decimal costCabin { get; set; }
            public decimal costTent { get; set; }
            public String contractLink { get; set; }
        }

        public class Raw
        {
            public string type { get; set; }
            public string data { get; set; }
        }

        public class CInfo
        {
            public CInfo()
            {
                id = -1;
                Name = null;
                Affiliation = null;
            }
            public int id { get; set; }
            public String Name { get; set; }
            public String Affiliation { get; set; }
        }

        public class EInfo
        {
            public EInfo()
            {
                id = -1;
                Description = null;
                Type = null;
            }
            public int id { get; set; }
            public String Description { get; set; }
            public String Type { get; set; }
            public String StartDate { get; set; }
        }
    }

    public partial class Attendance
    {
        public enum AttendanceRoles { Unknown = 0, Visitor, Camper, Staff, Dean, Counselor, SWAT, Missionary, Speaker, Worship, HealthDir, nurse, Manager, Cook, kitchen, Worker, Trustee, ChurchRep, DropOff, PickUp, Other = 101 };

        public class PFullInfo
        {
            public PFullInfo()
            {
                LFName = "";
                FLName = "";
                FName = "";
                LName = "";
                _Gender = "";
                Church = "";
                DOB = "";
                Street = "";
                State = "";
                City = "";
                Zip = "";
                HPhone = "";
                Cell = "";
                SSNum = "";
                EMail = "";
                PG1_id = "";
                PG2_id = "";
                CMT = "";
                EventType = "";
                Baptized = "";
                Skills = "";
                PGFirst1 = "";
                PGLast1 = "";
                PGFirst2 = "";
                PGLast2 = "";
                PGFirst12 = "";
                PGFirstLast1 = "";
                AttStart = "";
                AttEnd = "";
                AttGrade = "";
                AttSRole = "";
                PG1_Info = "";
                PG2_Info = "";
            }

            public String LFName
            {
                set
                {
                    if (value.Contains(','))
                    {
                        String[] names = value.Split(',');
                        if (names.Count() > 0)
                        {
                            LName = names[0].Trim();
                            if (names.Count() > 1)
                                FName = names[1].Trim();
                        }
                    }
                    else
                    {
                        if (value.Contains(' '))
                        {
                            String[] nms = value.Split(' ');
                            if (nms.Count() > 0)
                            {
                                FName = nms[0].Trim();
                                if (nms.Count() > 1)
                                    LName = nms[1].Trim();
                            }
                        }
                        //FName = value ZZZ;
                        //LName = value ZZZ;
                    }
                }
            }
            public String FLName
            {
                set
                {
                    LFName = value;
                }
            }
            public String FName { get; set; }
            public String LName { get; set; }
            private String _Gender;
            public String Gender
            {
                get { return _Gender; }
                set
                {
                    string gstr = value.Trim().ToUpper();
                    if (gstr.StartsWith("M") || gstr.Contains("BOY"))
                        _Gender = "M";
                    else if (gstr.StartsWith("F") || gstr.Contains("GIRL"))
                        _Gender = "F";
                }
            }

            public String Church { get; set; }
            public String DOB { get; set; }
            public String Street { get; set; }
            public String State { get; set; }
            public String City { get; set; }
            public String Zip { get; set; }
            public String HPhone { get; set; }
            public String Cell { get; set; }
            public String SSNum { get; set; }
            public String EMail { get; set; }
            public String PG1_id { get; set; }
            public String PG2_id { get; set; }
            public String CMT { get; set; }
            public String EventType
            {
                get
                {
                    return _EventType;
                }
                set
                {
                    String uvalue = value.ToUpper().Trim();
                    if (uvalue.Contains("BWAC") || uvalue.StartsWith("FALL"))
                    {
                        _EventType = Event.EventType.BWAC_Fall_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("MEN"))
                    {
                        _EventType = Event.EventType.Mens_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("WINT"))
                    {
                        _EventType = Event.EventType.Winter_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("WOM"))
                    {
                        _EventType = Event.EventType.Womans_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("FAM"))
                    {
                        _EventType = Event.EventType.Family_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("Y"))
                    {
                        _EventType = Event.EventType.Young_Adult_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("V"))
                    {
                        _EventType = Event.EventType.VOE_Week.ToString();
                    }
                    else if (uvalue.StartsWith("SE") || value.StartsWith("SN"))
                    {
                        _EventType = Event.EventType.Senior_Week.ToString();
                    }
                    else if (uvalue.StartsWith("SP") || value.StartsWith("SR"))
                    {
                        _EventType = Event.EventType.Spring_Work_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("I"))
                    {
                        _EventType = Event.EventType.Intermediate_Week.ToString();
                    }
                    else if (uvalue.StartsWith("E"))
                    {
                        _EventType = Event.EventType.Elementary_Week.ToString();
                    }
                    else if (uvalue.StartsWith("FI") ||
                             uvalue.StartsWith("F") ||
                             uvalue.StartsWith("FC") ||
                             uvalue.StartsWith("CHA") ||
                             uvalue.StartsWith("CB"))
                    {
                        _EventType = Event.EventType.First_Chance_Retreat.ToString();
                    }
                    else if (uvalue.StartsWith("B"))
                    {
                        _EventType = Event.EventType.Banquet.ToString();
                    }
                    else if (uvalue.StartsWith("FUND"))
                    {
                        _EventType = Event.EventType.Fund_Raiser.ToString();
                    }
                    else
                        _EventType = Event.EventType.Other.ToString();
                }
            }
            public String Baptized { get; set; }
            public String Skills
            {
                get
                {
                    return _Skills;
                }
                set
                {
                    char[] arr;
                    arr = value.ToCharArray();
                    foreach (char c in arr)
                    {
                        switch (c)
                        {
                            case 'K':
                                if (_Skills.Length > 0) _Skills += ";";
                                _Skills += "multicheckbox[]:COK";
                                break;
                            case 'M':
                            case 'S':
                                if (_Skills.Length > 0) _Skills += ";";
                                _Skills += "multicheckbox[]:WRK;multicheckbox[]:KTH";
                                break;
                            case 'D':
                                if (_Skills.Length > 0) _Skills += ";";
                                _Skills += "multicheckbox[]:DEN";
                                break;
                            case 'T':
                            case 'C':
                            case 'R':
                                if (_Skills.Length > 0) _Skills += ";";
                                _Skills += "multicheckbox[]:CNS";
                                break;
                            case 'H':
                                if (_Skills.Length > 0) _Skills += ";";
                                _Skills += "multicheckbox[]:HLD";
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            private String _PGLast = "";
            private String _Skills = "";
            public String _EventType = "";
            public override string ToString()
            {
                Type t = this.GetType();
                PropertyInfo[] props = t.GetProperties();
                String full = "";

                if ((FName == null) || (FName.Length == 0))
                    return full;

                foreach (var prop in props)
                {
                    if ((prop.GetMethod != null) && prop.CanRead && (prop.GetValue(this) != null) && (prop.GetValue(this).ToString().Length > 0))
                    {
                        full = full + prop.Name + ":" + prop.GetValue(this) + ";";
                    }
                }
                return full;
            }
            public String PGFirst1
            {
                set
                {
                    if ((PG1_id != null) && (PG1_id.Length > 0))
                        PG1_id = value.Trim() + " " + PG1_id;
                    else
                        PG1_id = value.Trim();

                }
            }
            public String PGLast1
            {
                set
                {
                    _PGLast = value.Trim();

                    if ((PG1_id != null) && (PG1_id.Length > 0))
                        PG1_id = PG1_id + " " + value.Trim();
                    else
                        PG1_id = value.Trim();
                }
            }

            public String PGFirst2
            {
                set
                {
                    if ((PG2_id != null) && (PG2_id.Length > 0))
                        PG2_id = value.Trim() + " " + PG2_id;
                    else
                        PG2_id = value.Trim();

                }
            }
            public String PGLast2
            {
                set
                {
                    _PGLast = value.Trim();

                    if ((PG2_id != null) && (PG2_id.Length > 0))
                        PG2_id = PG2_id + " " + value.Trim();
                    else
                        PG2_id = value.Trim();
                }
            }
            public String PGFirst12
            {
                set
                {
                    char[] delims = { '&', ',' };
                    String[] fnames = value.Split(delims);
                    if (fnames.Count() > 0)
                    {
                        if ((PG1_id != null) && PG1_id.Length > 0)
                            PG1_id = fnames[0].Trim() + " " + PG1_id;
                        else
                            PG1_id = fnames[0].Trim() + " " + _PGLast;
                        if (fnames.Count() > 1)
                        {
                            if ((PG2_id != null) && (PG2_id.Length > 0))
                                PG2_id = fnames[1].Trim() + " " + PG2_id;
                            else
                                PG2_id = fnames[1].Trim() + " " + _PGLast;
                        }
                    }
                }
            }
            public String PGFirstLast1
            {
                set
                {
                    if (value.Contains(','))
                    {
                        String[] names = value.Split(',');
                        if (names.Count() > 0)
                        {
                            PG1_id = names[0].Trim();
                            if (names.Count() > 1)
                                PG1_id = names[1].Trim() + " " + PG1_id;
                        }
                    }
                    else
                    {
                        if (value.Contains(' '))
                        {
                            String[] nms = value.Split(' ');
                            if (nms.Count() > 0)
                            {
                                PG1_id = nms[0].Trim();
                                if (nms.Count() > 1)
                                    PG1_id = PG1_id + nms[1].Trim();
                            }
                        }
                        //FName = value ZZZ;
                        //LName = value ZZZ;
                    }
                }
            }

            public String AttStart { get; set; }

            public String AttEnd { get; set; }

            private String _AttGrade = null;
            public String AttGrade
            {
                get { return _AttGrade; }
                set
                {
                    String gstr = value.Trim().ToUpper();
                    if (gstr.StartsWith("K"))
                        _AttGrade = "0";
                    else if (gstr.Contains("C"))
                        _AttGrade = "13";
                    else
                        _AttGrade = gstr;
                }
            }

            public int AttRole = (int)AttendanceRoles.Unknown;
            public String AttSRole
            {
                get { return ((AttendanceRoles)AttRole).ToString(); }
                set
                {
                    String role = value.Trim();
                    AttendanceRoles ar;
                    if (Enum.TryParse<AttendanceRoles>(role, true, out ar))
                        AttRole = (int)ar;
                    else
                        AttRole = (int)AttendanceRoles.Unknown;
                }
            }

            public String PG1_Info
            {
                set
                {
                    PG1_id = value;
                }
            }

            public String PG2_Info
            {
                set
                {
                    PG2_id = value;
                }
            }
        }
        public class AttData
        {
            public AttData()
            {
                id = pid = rid = eid = mid = tid = -1;
                PersonName = "";
                EventName = "";
                role = 0;
                CurGrade = -1;
                gender = '?';
            }
            public int id { get; set; }
            public String PersonName { get; set; }
            public int AgeAtEvent { get; set; }
            public String EventName { get; set; }
            public int role { get; set; }
            public DateTime? StartDT { get; set; }
            public DateTime? EndDT { get; set; }
            public String MedForm { get; set; }
            public String RegForm { get; set; }
            public int CurGrade { get; set; }
            public String TSize { get; set; }
            public int pid { get; set; }
            public int rid { get; set; }
            public int eid { get; set; }
            public int mid { get; set; }
            public int tid { get; set; }
            public String note { get; set; }
            public char gender { get; set; }

            public override string ToString()
            {
                return String.Format("{0,20} {1,2} {2,20} {3,10} {4,15} {5,15}", PersonName, AgeAtEvent, EventName, role.ToString(), StartDT.ToString(), EndDT.ToString());
            }
        }

    }
}