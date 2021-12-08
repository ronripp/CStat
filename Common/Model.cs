using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using CTask = CStat.Models.Task;
using CStat.Common;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Runtime.Serialization.Json;

namespace CStat.Models
{
    public static class CCommon
    {
        public static string EncodeQuotes(string str)
        {
            return (str.Length > 0) ? str.Replace("\"", "^^").Replace("'", "^") : "";
        }
        public static string UnencodeQuotes(string str)
        {
            return (str.Length > 0) ? str.Replace("^^", "\"").Replace("^", "'") : "";
        }
    }

    public class DateRange
    {
        public DateRange() { }
        public DateRange(DateTime startDate, DateTime endDate, int? eventID = null)
        {
            Start = startDate;
            End = endDate;
            EventID = eventID;
        }

        public bool In (DateRange range)
        {
            return !((this.Start > range.End) || (this.End < range.Start));
        }

        public bool In(DateTime sDate, DateTime eDate)
        {
            return !((this.Start > eDate) || (this.End < sDate));
        }

        public bool In(DateTime date)
        {
            return !((this.Start > date) || (this.End < date));
        }
        public double TotalHours()
        {
            TimeSpan diff = End - Start;
            return diff.TotalHours;
        }
        public int NumUniqueDates()
        {
            int NumDates = 1;
            var fd = new DateTime(Start.Year, Start.Month, Start.Day, 1, 0, 0);
            var ld = new DateTime(End.Year, End.Month, End.Day, 1, 0, 0);
            for (var d = fd; (d.Year != ld.Year) || (d.Month != ld.Month) || (d.Day != ld.Day); d = d.AddDays(1), ++NumDates);
            return NumDates;
        }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int? EventID { get; set; } = null;
    }
    public class DateTimeEv
    {
        public DateTimeEv(DateTime _dt, int? _eventID = null)
        {
            dt = _dt;
            eventID = _eventID;
        }

        public DateTime dt { get; set; }
        public int? eventID { get; set; } = null;
    }

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

    public partial class Tasks
    {
    }

    public partial class InventoryItem
    {
        public enum States { InStock = 0, OpenNeed = 1, TakenNeed = 2, InInv = 3 };
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
            Basement_Heater = 9,
            Hall_Wash_Closet = 10
        };
    }

    public partial class Person
    {
        public enum TitleRoles { Undefined = 0, Blds_Gnds = 0x1, Dean = 0x2, Counselor = 0x4, SWAT = 0x8, Worship = 0x10, HealthDir = 0x20, nurse = 0x40, Manager = 0x80,
                            Cook = 0x100, FFE = 0x200, Trustee = 0x400, President = 0x800, Treasurer = 0x1000, Secretary = 0x2000, VicePres = 0x4000, ECMember = 0x8000, Security = 0x10000, Unavailable = 0x20000, Admin = 0x40000 };

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
            catch (Exception)
            {
            }

            return "ERROR~:No one found with Name";
        }
    }

    public partial class Address
    {
        public Address ShallowCopy()
        {
            return (Address)this.MemberwiseClone();
        }
    }

    public partial class Task
    {
        public enum eTaskType
        {
            // Amount  MASK = 0x00000FFF

            // Area :  MASK = 0x0001F000     
            PlanCampEvent   = 0x00001000,
            CampEventTask   = 0x00002000,
            BuildingFix     = 0x00003000,
            GroundsFix      = 0x00004000,
            NewAdditon      = 0x00005000,
            RequiredTask    = 0x00006000,
            CampEnhancement = 0x00007000,
            Promotion       = 0x00008000,
            CampMantTask    = 0x00009000,
            // MAX          = 0x0001F000

            // Flags   MASK = 0x00180000
            Template        = 0x00020000,
            AutoPersonID    = 0x00040000,
            unk2            = 0x00080000,
            unk3            = 0x00100000,

            // Due     MASK = 0x7C000000 
            Before_Start        = 0x00000001 << 26, //  400 0000
            At_Start            = 0x00000002 << 26, //  800 0000
            Before_End          = 0x00000003 << 26, //  C00 0000
            At_End              = 0x00000004 << 26, // 1000 0000
            Hours_Before_Start  = 0x00000005 << 26,
            Hours_After_Start   = 0x00000006 << 26,
            Hours_Before_End    = 0x00000007 << 26,
            Hours_After_End     = 0x00000008 << 26,
            Days_Before_Start   = 0x00000009 << 26,
            Days_After_Start    = 0x0000000A << 26,
            Days_Before_End     = 0x0000000B << 26,
            Days_After_End      = 0x0000000C << 26,
            Weeks_Before_Start  = 0x0000000D << 26,
            Weeks_After_Start   = 0x0000000E << 26,
            Weeks_Before_End    = 0x0000000F << 26,
            Weeks_After_End     = 0x00000010 << 26,
            Months_Before_Start = 0x00000011 << 26,
            Months_After_Start  = 0x00000012 << 26,
            Months_Before_End   = 0x00000013 << 26,
            Months_After_End    = 0x00000014 << 26,
            Day_Of_Week_SunMon  = 0x00000015 << 26,
            Every_Num_Years     = 0x00000016 << 26,
            // MAX              = 0x0000001F << 26, 

            // Each    MASK   = 0x03E00000 
            Retreat_Event     = 0x00000001 << 21, // 20 0000
            Week_Event        = 0x00000002 << 21,
            Event_Day         = 0x00000003 << 21,
            Work_Day          = 0x00000004 << 21,
            Work_Week         = 0x00000005 << 21,
            Work_Event        = 0x00000006 << 21,
            ChCamp_Month      = 0x00000007 << 21,
            Quarter           = 0x00000008 << 21,
            First_Quarter     = 0x00000009 << 21,
            Second_Quarter    = 0x0000000A << 21,
            Third_Quarter     = 0x0000000B << 21,
            Fourth_Quarter    = 0x0000000C << 21,
            Event             = 0x0000000D << 21,
            Month             = 0x0000000E << 21,
            Year              = 0x0000000F << 21,
            Due_Day           = 0x00000010 << 21,
            Day_from_Due_till = 0x00000011 << 21,
            Num_Start_Date    = 0x00000012 << 21,
            // MAX            = 0x0000001F << 21 
        }

        public enum ePriority
        {
            Required  = 0x00000001,
            Urgent    = 0x00000002,
            Very_High = 0x00000004,
            High      = 0x00000008,
            Medium    = 0x00000010,
            Low       = 0x00000020
        }

        public enum eTaskStatus
        {
            // Percent Complete 0x7F

            // State
            Not_Started    = 0x00000080,
            Planning       = 0x00000100,
            Ready          = 0x00000200,
            Active         = 0x00000400,
            Paused         = 0x00000800,
            Completed      = 0x00001000,

            //Reason
            Need_Funds      = 0x00100000,
            Need_Labor      = 0x00200000,
            Need_Material   = 0x00400000,
            Need_Inspection = 0x00800000,
            Need_Planning   = 0x01000000,
            Job_Done        = 0x02000000,
            Unknown         = 0x04000000,
        }
        public enum eAGFrequency
        {

        }

        public String ClassName(int i)
        {
            string name;

            if (IsTemplate())
                name = "TemplateStyle";
            else
            {
                if (IsDone())
                    name = "DoneStyle";
                else
                    name = IsDue() ? "DueStyle" : "NotDueStyle";
            }
            if ((i % 2) != 0)
                name = name + "Odd";
            return name;
        }

        public bool IsDue()
        {
            if ((Type & (int)eTaskType.Template) != 0)
                return false;
            DateTime dtTol = PropMgr.ESTNow - TimeSpan.FromDays(1);
            return (((Status & (int)CTask.eTaskStatus.Completed) == 0) && DueDate.HasValue && DueDate.Value < PropMgr.ESTNow);
        }
        public bool IsDone()
        {
            if (IsTemplate())
                return false;
            return ((Status & (int)CTask.eTaskStatus.Completed) != 0);
        }
        public bool IsTemplate()
        {
            return (Type & (int)eTaskType.Template) != 0;
        }
        public int DueOrder()
        {
            return (DueDate.HasValue && DueDate.Value < PropMgr.ESTNow) ? 1 : 2;
        }

        public static List<CTask> GetDueTasks(CStat.Models.CStatContext context, double hoursEarly=0)
        {
            DateTime dtThreshold = (hoursEarly == 0) ? dtThreshold = PropMgr.ESTNow : PropMgr.ESTNow + TimeSpan.FromHours(hoursEarly);
            return _GetDueTasks(context, dtThreshold).Result;
        }

        public static async System.Threading.Tasks.Task<List<CTask>> _GetDueTasks(CStat.Models.CStatContext context, DateTime threshold)
        {
            var dTasks = await context.Task
              .Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) == 0) && ((t.Type & (int)CTask.eTaskType.Template) == 0) && (t.DueDate.HasValue && t.DueDate <= threshold)).ToListAsync();
            return dTasks;
        }

        public static bool NotifyUserTaskDue (IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CStatContext context, double hoursEarly, bool forceClean=false)
        {
            var sms = new CSSMS(hostEnv, config, userManager);
            var tasks = GetDueTasks(context, hoursEarly);
            foreach (var t in tasks)
            {
                if ((t.Person != null) && !String.IsNullOrEmpty(t.Person.Email))
                    sms.NotifyUser(t.Person.Email, CSSMS.NotifyType.TaskNT, "CStat: Task " + t.Id + "> " + t.Description + " is due " + t.DueDate.ToString(), true, forceClean); // Allow Resend
            }
            return true;
        }

        public void GetTaskStatus (out CTask.eTaskStatus state, out CTask.eTaskStatus reason, out int pctComp)
        {
            state = (CTask.eTaskStatus)(this.Status & 0x000FFF80);
            reason = (CTask.eTaskStatus)(this.Status & 0xFFF00000);
            pctComp = (int)(this.Status & 0x0000007F);
        }
        public void SetTaskStatus(CTask.eTaskStatus state, CTask.eTaskStatus reason, int pctComp)
        {
            Status = ((int)state & 0x000FFF80) | ((int)reason & 0x7FF00000) | (pctComp & 0x0000007F);
        }

        public void GetTaskType(out CTask.eTaskType dueType, out CTask.eTaskType eachType, out int dueVal)
        {
            dueType = (CTask.eTaskType)(this.Type & 0x7C000000);
            eachType = (CTask.eTaskType)(this.Type & 0x03E00000);
            dueVal = (int)(this.Type & 0x00000FFF);
        }
        public void SetTaskType(CTask.eTaskType dueType, CTask.eTaskType eachType, int dueVal)
        {
            Type = ((int)dueType & 0x7C000000) | ((int)eachType & 0x03E00000) | (dueVal & 0x00000FFF);
            if (Type != 0)
                Type |= (int)CTask.eTaskType.Template;
        }

        public void SetType(CTask.eTaskType ttype)
        {
            Type |= (int)ttype;
        }

        public void ClearType(CTask.eTaskType ttype)
        {
            Type &= ~(int)ttype;
        }

        public static int? ResolvePersonId (CStat.Models.CStatContext context, long role1, long role2, long role3)
        {
            Person pers = null;
            if (role1 > 0)
                pers = context.Person.FirstOrDefault(p => (p.Roles & role1) != 0);
            if (pers != null)
                return pers.Id;
            if (role2 > 0)
                pers = context.Person.FirstOrDefault(p => (p.Roles & role2) != 0);
            if (pers != null)
                return pers.Id;
            if (role3 > 0)
                pers = context.Person.FirstOrDefault(p => (p.Roles & role3) != 0);
            if (pers != null)
                return pers.Id;
            else
                pers = context.Person.FirstOrDefault(p => (p.Roles & (long)Person.TitleRoles.Admin) != 0); // default to Admin to ensure assignment.
            if (pers != null)
                return pers.Id;
            return null;
        }

        public String TaskString (CTask.eTaskStatus ts)
        {
            String resStr = "";
            foreach (String s in Enum.GetValues(typeof(eTaskStatus)))
            {
                if (((eTaskStatus)Enum.Parse(typeof(eTaskStatus), s) & ts) != 0)
                {
                    if (resStr.Length > 0)
                        resStr += ", ";
                    if ((resStr.IndexOf("Need") != -1) && (s.IndexOf("Need_") == 0))
                        resStr += s.Substring(5);
                    else
                        resStr += s;
                }
            }
            return resStr.Replace("_", " ");
        }
        public bool GetDueDates(List<Event> events, out List<DateTimeEv> dueDates, DateRange lim)
        {
            GetTaskType(out CTask.eTaskType dueType, out CTask.eTaskType eachType, out int dueVal);
            dueDates = ApplyDueType(dueType, dueVal, GetEachTypeRange(eachType, events, lim), lim);
            return dueDates.Count > 0;
        }
        public List<DateRange> GetEachTypeRange(CTask.eTaskType eachType, List<Event> events, DateRange lim)
        {
            List<DateRange> ranges = new List<DateRange>();

            // TBD Lim In() needs to be applied AFTER ApplyDueType adjustment for some/all?.

            switch (eachType)
            {
                case eTaskType.Retreat_Event:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (er.TotalHours() <= 72))
                            {
                                ranges.Add(er);
                            }
                        }
                    }
                    break;
                case eTaskType.Week_Event:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (er.TotalHours() > 72))
                            {
                                ranges.Add(er);
                            }
                        }
                    }
                    break;

                case eTaskType.Event_Day:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er))
                            {
                                int totalDays = er.NumUniqueDates();
                                DateTime s = ev.StartTime;
                                DateTime e = new DateTime(s.Year, s.Month, s.Day, 23, 59, 59), t;
                                for (int d = 0; d < totalDays; ++d)
                                {
                                    if (e > ev.EndTime)
                                        e = ev.EndTime;

                                    ranges.Add(new DateRange(s, e, ev.Id));

                                    t = s.AddDays(1);
                                    s = new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
                                    e = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59);
                                }
                                
                            }
                        }
                    }
                    break;
                case eTaskType.Work_Day:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (ev.Type == (int)Event.EventType.Work_Event))
                            {
                                int totalDays = er.NumUniqueDates();
                                DateTime s = ev.StartTime;
                                DateTime e = new DateTime(s.Year, s.Month, s.Day, 23, 59, 59), t;
                                for (int d = 0; d < totalDays; ++d)
                                {
                                    if (e > ev.EndTime)
                                        e = ev.EndTime;

                                    ranges.Add(new DateRange(s, e, ev.Id));

                                    t = s.AddDays(1);
                                    s = new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
                                    e = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59);
                                }

                            }
                        }
                    }
                    break;
                case eTaskType.Work_Week:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (ev.Type == (int)Event.EventType.Work_Event))
                            {
                                int totalWeeks = er.NumUniqueDates();
                                DateTime s = ev.StartTime;
                                DateTime e = new DateTime(s.Year, s.Month, s.Day, 23, 59, 59), t;
                                for (int d = 0; d < totalWeeks; ++d)
                                {
                                    if (e > ev.EndTime)
                                        e = ev.EndTime;

                                    ranges.Add(new DateRange(s, e, ev.Id));

                                    t = s.AddDays(7);
                                    s = new DateTime(t.Year, t.Month, t.Day, 0, 0, 0);
                                    e = new DateTime(t.Year, t.Month, t.Day, 23, 59, 59);
                                }
                            }
                        }
                    }
                    break;
                case eTaskType.Work_Event:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (ev.Type == (int)Event.EventType.Work_Event))
                            {
                                ranges.Add(er);
                            }
                        }
                    }
                    break;
                case eTaskType.ChCamp_Month:
                    {
                        for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                        {
                            for (int m = 7; m < 9; ++m) // July & August
                            {
                                DateTime s = new DateTime(y, m, 1, 0, 0, 0);
                                DateTime e = new DateTime(y, m, 31, 23, 59, 59);
                                DateRange range = new DateRange(s, e);
                                if (lim.In(range))
                                    ranges.Add(range);
                            }
                        }
                    }
                    break;
                case eTaskType.First_Quarter:
                    {
                        for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                        {
                            DateTime s = new DateTime(y, 1, 1, 0, 0, 0);
                            DateTime e = new DateTime(y, 3, 31, 23, 59, 59);
                            DateRange range = new DateRange(s, e);
                            if (lim.In(range))
                                ranges.Add(range);
                        }
                    }
                    break;
                case eTaskType.Second_Quarter:
                    {
                        for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                        {
                            DateTime s = new DateTime(y, 4, 1, 0, 0, 0);
                            DateTime e = new DateTime(y, 6, 30, 23, 59, 59);
                            DateRange range = new DateRange(s, e);
                            if (lim.In(range))
                                ranges.Add(range);
                        }
                    }
                    break;
                case eTaskType.Third_Quarter:
                    {
                        for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                        {
                            DateTime s = new DateTime(y, 7, 1, 0, 0, 0);
                            DateTime e = new DateTime(y, 9, 30, 23, 59, 59);
                            DateRange range = new DateRange(s, e);
                            if (lim.In(range))
                                ranges.Add(range);
                        }
                    }
                    break;
                case eTaskType.Fourth_Quarter:
                    {
                        for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                        {
                            DateTime s = new DateTime(y, 10, 1, 0, 0, 0);
                            DateTime e = new DateTime(y, 12, 31, 23, 59, 59);
                            DateRange range = new DateRange(s, e);
                            if (lim.In(range))
                                ranges.Add(range);
                        }
                    }
                    break;

                case eTaskType.Event:
                    {
                        foreach (var ev in events)
                        {
                            DateRange er = new DateRange(ev.StartTime, ev.EndTime, ev.Id);
                            if (lim.In(er) && (er.TotalHours() > 72))
                            {
                                ranges.Add(er);
                            }
                        }
                    }
                    break;
                case eTaskType.Month:
                    for (int y = lim.Start.Year, m = lim.Start.Month; y <= lim.End.Year;)
                    {
                        DateTime s = new DateTime(y, m, 1, 0, 0, 0);
                        DateTime e = s.AddMonths(1).AddSeconds(-1);
                        DateRange range = new DateRange(s, e);
                        if (lim.In(range))
                            ranges.Add(range);
                        else
                            break;
                        if (++m > 12)
                        {
                            ++y;
                            m = 1;
                        }
                    }
                    break;
                case eTaskType.Year:
                    for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                    {
                        DateTime s = new DateTime(y, 1, 1, 0, 0, 0);
                        DateTime e = new DateTime(y, 12, 31, 23, 59, 59);
                        DateRange range = new DateRange(s, e);
                        if (lim.In(range))
                            ranges.Add(range);
                    }
                    break;
                case eTaskType.Due_Day:
                    {
                        if (DueDate.HasValue)
                        {
                            for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                            {
                                DateTime s = new DateTime(y, DueDate.Value.Month, DueDate.Value.Day, 0, 0, 0);
                                DateTime e = new DateTime(y, DueDate.Value.Month, DueDate.Value.Day, 23, 59, 59);
                                DateRange range = new DateRange(s, e);
                                if (lim.In(range))
                                    ranges.Add(range);
                            }
                        }
                    }
                    break;
                case eTaskType.Day_from_Due_till:
                    {
                        if (DueDate.HasValue)
                        {
                            for (int y = lim.Start.Year; y <= lim.End.Year; ++y)
                            {
                                DateTime curDate = new DateTime (y, DueDate.Value.Month, DueDate.Value.Day);
                                DateTime endDate = new DateTime(y, EstimatedDoneDate.Value.Month, EstimatedDoneDate.Value.Day);
                                for (; curDate <= endDate; curDate = curDate.AddDays(1))
                                {
                                    DateTime s = new DateTime(y, curDate.Month, curDate.Day, 0, 0, 0);
                                    DateTime e = new DateTime(y, curDate.Month, curDate.Day, 23, 59, 59);
                                    DateRange range = new DateRange(s, e);
                                    if (lim.In(range))
                                        ranges.Add(range);
                                }
                            }
                        }
                    }
                    break;

                case eTaskType.Num_Start_Date:
                   {
                        if (DueDate.HasValue)
                        {
                            DateTime s = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 0, 0, 0);
                            DateTime e = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 23, 59, 59);
                            ranges.Add(new DateRange(s, e));
                        }
                    }
                    break;
            }
            return ranges;
        }
        public List<DateTimeEv> ApplyDueType(CTask.eTaskType dueType, int dueVal, List<DateRange> eachTypeRanges, DateRange lim)
        {
            List<DateTimeEv> dtList = new List<DateTimeEv>();

            foreach(var dr in eachTypeRanges)
            {
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddMinutes(-1), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.At_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start, dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Before_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddMinutes(-1).AddSeconds(1), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.At_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End, dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Hours_Before_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddHours(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Hours_After_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddHours(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Hours_Before_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddHours(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Hours_After_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddHours(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Days_Before_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddDays(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Days_After_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddDays(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Days_Before_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddDays(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Days_After_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddDays(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Weeks_Before_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddDays(-dueVal*7), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Weeks_After_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddDays(dueVal * 7), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Weeks_Before_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddDays(-dueVal * 7), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Weeks_After_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddDays(dueVal * 7), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Months_Before_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddMonths(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Months_After_Start:
                        {
                            dtList.Add(new DateTimeEv(dr.Start.AddMonths(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Months_Before_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddMonths(-dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Months_After_End:
                        {
                            dtList.Add(new DateTimeEv(dr.End.AddMonths(dueVal), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Day_Of_Week_SunMon:
                        {
                            int sdow = (int)dr.Start.DayOfWeek + 1;
                            if (dueVal < sdow)
                                dtList.Add(new DateTimeEv(dr.End.AddDays((7-sdow) + dueVal), dr.EventID));
                            else
                                dtList.Add(new DateTimeEv(dr.End.AddDays(dueVal - sdow), dr.EventID));
                        }
                        break;
                    case CTask.eTaskType.Every_Num_Years:
                        {
                            for (int mult = 1; true; ++mult)
                            {
                                DateTime dt = dr.Start.AddYears(mult*dueVal);
                                dtList.Add(new DateTimeEv(dt, dr.EventID));
                                if (!lim.In(new DateRange(dt, dt)))
                                    break;
                            }
                        }
                        break;
                }
            }
            return dtList;
        }
    }

    [DataContract]
    public class Pic
    {
        public Pic(int taskID, int picID, string desc, string urL = "")
        {
            taskId = taskID;
            picId = picID;
            title = desc;
            url = urL;
        }
        [DataMember]
        public int taskId { get; set; }
        [DataMember]
        public int picId { get; set; }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string url { get; set; }
        public string GetFileName(string srcFile)
        {
            string ext = Path.GetExtension(srcFile);
            return "Img_" + taskId + "_" + picId + ext;
        }
    }

    [DataContract]
    public class BOMLine
    {
        public BOMLine(int id_, string desc_, string ucost_, string units_, string qty_, string tcost_)
        {
            id = id_;
            desc = desc_;
            ucost = ucost_;
            units = units_;
            qty = qty_;
            tcost = tcost_;
        }
        [DataMember]
        public int id { get; set; }
        [DataMember]
        public string desc { get; set; }
        [DataMember]
        public string ucost { get; set; }
        [DataMember]
        public string units { get; set; }
        [DataMember]
        public string qty { get; set; }
        [DataMember]
        public string tcost { get; set; }
    }

    [DataContract]
    public class TaskData
    {
        [DataMember]
        public int tid { get; set; }
        [DataMember]
        public int state { get; set; }
        [DataMember]
        public int reason { get; set; }
        [DataMember]
        public int CreateTaskDue { get; set; }
        [DataMember]
        public string CreateTaskDueVal { get; set; }
        [DataMember]
        public int CreateTaskEach { get; set; }
        public int PercentComplete { get; set; }
        [DataMember]
        public string Title { get; set; }
        [DataMember]
        public string Detail { get; set; }
        [DataMember]
        public bool FixedDueDate { get; set; }
        [DataMember]
        public List<Pic> pics { get; set; }
        [DataMember]
        public List<BOMLine> bom { get; set; }
        [DataMember]
        public string comments { get; set; }
        [DataMember]
        public long Role1 { get; set; } = 0;
        [DataMember]
        public long Role2 { get; set; } = 0;
        [DataMember]
        public long Role3 { get; set; } = 0;
        [DataMember]
        public string TillDay { get; set; }
        public TaskData(int taskId = -1)
        {
            tid = taskId;                               // These top 4 memmbers are redundant but they must match DB Record
            state = (int)CTask.eTaskStatus.Not_Started;
            reason = (int)CTask.eTaskStatus.Unknown;
            CreateTaskDue = 0;
            CreateTaskDueVal = "";
            CreateTaskEach = 0;
            PercentComplete = 0;
            Title = "";
            Detail = "";
            pics = new List<Pic>();
            bom = new List<BOMLine>();
            comments = "";
            FixedDueDate = false;
            TillDay = "";
        }
        public static TaskData ReadTaskData(IWebHostEnvironment hstEnv, int taskId, int parentTaskId)
        {
            TaskData taskData = null;
            string folderName = @"Tasks";
            string webRootPath = hstEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            for (int i = 0, tid = taskId; i < 2; ++i, tid = parentTaskId)
            {
                string fullPath = Path.Combine(newPath, "Task_" + tid.ToString() + ".json");
                if (File.Exists(fullPath))
                {
                    using (StreamReader r = new StreamReader(fullPath))
                    {
                        string json = r.ReadToEnd();
                        var ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TaskData));
                        taskData = (TaskData)deserializer.ReadObject(ms);
                        taskData.tid = taskId;
                    }
                    break;
                }
            }

            if (taskData == null)
                taskData = new TaskData();
            return taskData;
        }

        public bool Write(IWebHostEnvironment hstEnv)
        {
            if (tid == -1)
                return false;
            string folderName = @"Tasks";
            string webRootPath = hstEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            string fullPath = Path.Combine(newPath, "Task_" + tid.ToString() + ".json");
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(TaskData));
                js.WriteObject(ms, this);
                StreamWriter writer = new StreamWriter(ms);
                writer.Flush();

                //You have to rewind the MemoryStream before copying
                ms.Seek(0, SeekOrigin.Begin);

                using (FileStream fs = new FileStream(fullPath, FileMode.OpenOrCreate))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }
            }
            return File.Exists(fullPath);
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
            Church_Event,
            Private_Event,
            Private_Retreat,
            VOE_Retreat,
            Work_Event,
            Elementary_Retreat,
            Intermediate_Retreat,
            Senior_Retreat,
            Leadership_Retreat,
            SWAT_Training,
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