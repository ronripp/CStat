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
using System.Web.WebPages.Html;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Drawing;

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

        public static CSUser GetCurUser (CStat.Models.CStatContext context, IConfiguration config, IHttpContextAccessor httpCA, UserManager<CStatUser> userManager)
        {
            var csSettings = CSSettings.GetCSSettings(config, userManager);
            string email = httpCA.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
            if (email == null)
                email = httpCA.HttpContext.User.Identity.Name;
            var userSettings = csSettings.GetUser(email);
            if (userSettings != null)
                userSettings.SetPersonIDByEmailPhoneAlias(context);
            return userSettings;
        }
        public static bool SamePhone(string p1, string p2)
        {
            if (String.IsNullOrEmpty(p1) || String.IsNullOrEmpty(p2))
                return false;
            p1 = string.Join("", p1.ToCharArray(0, p1.Length).Where(c => (c >= '0') && (c <= '9')));
            p2 = string.Join("", p2.ToCharArray(0, p2.Length).Where(c => (c >= '0') && (c <= '9')));
            return (p1 == p2);
        }

        public static void ExifOrientJPEGFile(string jpgFile)
        {
            FileInfo fi = new FileInfo(jpgFile);
            if (!fi.Extension.ToLower().ToLower().StartsWith(".jp"))
                return;

            using (var image = System.Drawing.Image.FromFile(jpgFile))
            {
                if (image == null)
                    return;
                if (ExifOrient(image))
                {
                    image.Save(jpgFile);
                }
            }
        }

        private const int exifOrientationID = 0x112; //274

        // Extends Image Class to Orient Image as user (e.g. taken by Smart Phone by incorporating JPEG MetaTag exifOrientation
        public static bool ExifOrient(this Image img)
        {
            if (!img.PropertyIdList.Contains(exifOrientationID))
                return false;

            var prop = img.GetPropertyItem(exifOrientationID);
            int val = BitConverter.ToUInt16(prop.Value, 0);
            var rot = RotateFlipType.RotateNoneFlipNone;

            if (val == 3 || val == 4)
                rot = RotateFlipType.Rotate180FlipNone;
            else if (val == 5 || val == 6)
                rot = RotateFlipType.Rotate90FlipNone;
            else if (val == 7 || val == 8)
                rot = RotateFlipType.Rotate270FlipNone;

            if (val == 2 || val == 4 || val == 5 || val == 7)
                rot |= RotateFlipType.RotateNoneFlipX;

            if (rot != RotateFlipType.RotateNoneFlipNone)
            {
                img.RotateFlip(rot);
                img.RemovePropertyItem(exifOrientationID);
                return true;
            }
            return false;
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
    public class InventoryState
    {
        public int ItemID;
        public string Name;
        public string State;
        public double InStock;
        public string Units;
        public DateTime Date;
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
            tablets = 14,
            LBS = 15
        };

        public enum ItemZoneM
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
            Hall_Wash_Closet = 10,
        };

        public enum ItemZoneF
        {
            unknown = 0,
            Front_Shelves = 101,
            Front_Refrigs = 102,
            Mid_Shelves = 103,
            Walk_In_Refrig = 104,
            Walk_In_Freezer = 105,
            Back_Refrigs = 106,
            Rack_Near_Oven = 107,
            Basement_Shelves = 108,
            Other_1 = 109,
            Other_2 = 110
        };


        public static string GetInventoryReport(CStatContext context, IConfiguration config, bool justNeeded, out string subject, bool withHdr)
        {
            var InventoryItems = context.InventoryItem.Include(i => i.Inventory).Include(i => i.Item).Include(i => i.Person).OrderByDescending(i => (i.State != null) ? i.State % 3 : 0).ToList();
            string[] StateStr = { "ok", "NEEDED: unassigned", "BUYING", "Not Checked" };

            string report = "";
            DateTime Now = PropMgr.ESTNow;
            subject = "CCA Inventory Report as of " + Now.ToShortDateString() + " " + Now.ToShortTimeString();
            report = (withHdr) ? subject + "\n--------------------------------.\n" : "";
            foreach (var i in InventoryItems)
            {
                var stateIdx = i.State.HasValue ? i.State.Value : 0;
                if (justNeeded && (stateIdx != 1) && (stateIdx != 2))
                    continue;

                var stateStr = ((stateIdx == 2) && (i.Person != null)) ? "BUYING: " + i.Person.GetShortName() : StateStr[stateIdx];

                 InventoryItem.ItemUnits units = (InventoryItem.ItemUnits)(i.Units.HasValue ? i.Units : 0);
                report += "[" + stateStr + "] " + i.Item.Name.Trim() + " CUR: " + i.CurrentStock + " " + units.ToString() + ".\n";
            }
            return report;
        }
        public static List<InventoryState> GetInventoryList(CStatContext context, IConfiguration config, bool justNeeded)
        {
            var InventoryItems = context.InventoryItem.Include(i => i.Inventory).Include(i => i.Item).Include(i => i.Person).OrderByDescending(i => (i.State != null) ? i.State % 3 : 0).ToList();
            string[] StateStr = { "ok", "NEEDED: unassigned", "BUYING", "Not Checked" };
            List<InventoryState> invList = new List<InventoryState>();
    
            DateTime Now = PropMgr.ESTNow;
            foreach (var i in InventoryItems)
            {
                var stateIdx = i.State.HasValue ? i.State.Value : 0;
                if (justNeeded && (stateIdx != 1) && (stateIdx != 2))
                    continue;
    
                var stateStr = ((stateIdx == 2) && (i.Person != null)) ? "BUYING: " + i.Person.GetShortName() : StateStr[stateIdx];
                InventoryItem.ItemUnits units = (InventoryItem.ItemUnits)(i.Units.HasValue ? i.Units : 0);
                invList.Add(new InventoryState { ItemID = i.Item.Id, Name = i.Item.Name.Trim(), State = stateStr, InStock = (double)i.CurrentStock, Units = units.ToString(), Date = i.Date.Value });
            }
            return invList;
        }
    }

    public partial class Address
    {
        public static readonly Dictionary<string, string> StateDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"AL", "Alabama"},
            {"AK", "Alaska"},
            {"AZ", "Arizona"},
            {"AR", "Arkansas"},
            {"CT", "Connecticut"},
            {"CA", "California"},
            {"CO", "Colorado"},
            {"DE", "Delaware"},
            {"DC", "DC"},
            {"FL", "Florida"},
            {"GA", "Georgia"},
            {"HI", "Hawaii"},
            {"ID", "Idaho"},
            {"IL", "Illinois"},
            {"IN", "Indiana"},
            {"IA", "Iowa"},
            {"KS","Kansas"},
            {"KY", "Kentucky"},
            {"LA", "Louisiana"},
            {"ME", "Maine"},
            {"MD", "Maryland"},
            {"MA", "Massachusetts"},
            {"MI", "Michigan"},
            {"MN", "Minnesota"},
            {"MS", "Mississippi"},
            {"MO", "Missouri"},
            {"MT", "Montana"},
            {"NY", "New York"},
            {"NJ", "New Jersey"},
            {"NE", "Nebraska"},
            {"NV", "Nevada"},
            {"NH", "New Hampshire"},
            {"NM", "New Mexico"},
            {"NC", "North Carolina"},
            {"ND", "North Dakota"},
            {"OH", "Ohio"},
            {"OK", "Oklahoma"},
            {"OR", "Oregon"},
            {"PA", "Pennsylvania"},
            {"RI", "Rhode Island"},
            {"SC", "South Carolina"},
            {"SD", "South Dakota"},
            {"TN", "Tennessee"},
            {"TX", "Texas"},
            {"UT", "Utah"},
            {"VT", "Vermont"},
            {"VA", "Virginia"},
            {"WA", "Washington"},
            {"WV", "West Virginia"},
            {"WI", "Wisconsin"},
            {"WY", "Wyoming"},
            {"AS","American Samoa"},
            {"GU", "Guam"},
            {"MP", "N Mariana Isls."},
            {"PR", "Puerto Rico"},
            {"UM", "US Minor Outl Isls."},
            {"VI", "Virgin Islands"},
            {"AA", "Armed Forces Am."},
            {"AP", "Armed Forces Pac."},
            {"AE", "Armed Forces Others"}
        };

        public static string GetStateOptions(string defState="NY")
        {
            if (defState == "")
                defState = "NY";

            string ops = "";
            foreach (var kvp in StateDict)
            {
                //<option value="AL" id="Alabama">Alabama</option>
                if (String.Equals(kvp.Key, defState, StringComparison.OrdinalIgnoreCase) || String.Equals(kvp.Value, defState, StringComparison.OrdinalIgnoreCase))
                    ops += "<option selected=\"selected\" value=\"" + kvp.Key + "\" id=\"" + kvp.Value + "\">" + kvp.Value + "</option>\n";
                else
                    ops += "<option value=\"" + kvp.Key + "\" id=\"" + kvp.Value + "\">" + kvp.Value + "</option>\n";

                if ((kvp.Key == "WY") || (kvp.Key == "VI"))
                    ops += "<option value=\"\" disabled=\"disabled\" class=\"SelectSeparator\">--------</option>\n";
            }
            return ops;
        }

        public static string GetStateAbbr(string state)
        {
            if (string.IsNullOrEmpty(state))
                return "";
            string st = state.Trim();
            var abbr = StateDict.FirstOrDefault(s => String.Equals(s.Value, st, StringComparison.InvariantCultureIgnoreCase)).Key;
            return (abbr != null) ? abbr : st;
        }

        public Address ShallowCopy()
        {
            return (Address)this.MemberwiseClone();
        }
        public static Address Merge (Address oldAdr, Address newAdr)
        {
            if (oldAdr == null)
                return newAdr;
            if (newAdr == null)
                return oldAdr;
            newAdr.Street = MergeField(oldAdr.Street, newAdr.Street);
            newAdr.Town = MergeField(oldAdr.Town, newAdr.Town);
            newAdr.State = MergeField(oldAdr.State, newAdr.State);
            newAdr.ZipCode = MergeField(oldAdr.ZipCode, newAdr.ZipCode);
            newAdr.Phone = MergeField(oldAdr.Phone, newAdr.Phone);
            newAdr.Fax = MergeField(oldAdr.Fax, newAdr.Fax);
            newAdr.Country = MergeField(oldAdr.Country, newAdr.Country);
            newAdr.WebSite = MergeField(oldAdr.WebSite, newAdr.WebSite);
            newAdr.Status = MergeField(oldAdr.Status, newAdr.Status);
            return newAdr;
        }
        public static string MergeField(string oldFld, string newFld)
        {
            return string.IsNullOrEmpty(newFld) ? oldFld : newFld;
        }

        public static int MergeField(int oldFld, int newFld)
        {
            return oldFld | newFld;
        }

        public static string FormatAddress(Address a, bool addPhone = false)
        {
            string astr = "";
            if (a == null)
                return astr;

            if (!string.IsNullOrEmpty(a.Street))
                astr += " " + a.Street;
            if (!string.IsNullOrEmpty(a.Town))
                astr += ", " + a.Town;
            if (!string.IsNullOrEmpty(a.State))
                astr += ", " + GetStateAbbr(a.State);
            if (!string.IsNullOrEmpty(a.ZipCode))
                astr += " " + FixZip(a.ZipCode);
            if (addPhone)
            {
                if (!string.IsNullOrEmpty(a.Phone))
                    astr += " H#: " + Models.Person.FixPhone(a.Phone);
            }
            return astr;
        }

        public string GetStreet() { return !string.IsNullOrEmpty(Street) ? Street : ""; }
        public string GetTown() { return !string.IsNullOrEmpty(Town) ? Town : ""; }
        public string GetState() { return !string.IsNullOrEmpty(State) ? GetStateAbbr(State) : ""; }
        public string GetZip() { return !string.IsNullOrEmpty(ZipCode) ? FixZip(ZipCode) : ""; }

        public static string GetStreet(Address a) { return !string.IsNullOrEmpty(a?.Street) ? a.Street : ""; }
        public static string GetTown(Address a)  { return !string.IsNullOrEmpty(a?.Town) ? a.Town : ""; }
        public static string GetState(Address a) { return !string.IsNullOrEmpty(a?.State) ? GetStateAbbr(a.State) : ""; }
        public static string GetZip(Address a) { return !string.IsNullOrEmpty(a?.ZipCode) ? FixZip(a.ZipCode) : ""; }
    }

    //static class LevenshteinDistance
    //{
    //    public static int Compute(string s, string t)
    //    {
    //        if (string.IsNullOrEmpty(s))
    //        {
    //            if (string.IsNullOrEmpty(t))
    //                return 0;
    //            return t.Length;
    //        }

    //        if (string.IsNullOrEmpty(t))
    //        {
    //            return s.Length;
    //        }

    //        int n = s.Length;
    //        int m = t.Length;
    //        int[,] d = new int[n + 1, m + 1];

    //        // initialize the top and right of the table to 0, 1, 2, ...
    //        for (int i = 0; i <= n; d[i, 0] = i++) ;
    //        for (int j = 1; j <= m; d[0, j] = j++) ;

    //        for (int i = 1; i <= n; i++)
    //        {
    //            for (int j = 1; j <= m; j++)
    //            {
    //                int cost = (char.ToUpperInvariant(t[j - 1]) == char.ToUpperInvariant(s[i - 1])) ? 0 : 1;
    //                int min1 = d[i - 1, j] + 1;
    //                int min2 = d[i, j - 1] + 1;
    //                int min3 = d[i - 1, j - 1] + cost;
    //                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
    //            }
    //        }
    //        return d[n, m];
    //    }
    //}

    public partial class Task
    {
        public enum eTaskType
        {
            // Amount  MASK = 0x00000FFF

            // Area :  MASK = 0x0001F000     
            PlanCampEvent = 0x00001000,
            CampEventTask = 0x00002000,
            BuildingFix = 0x00003000,
            GroundsFix = 0x00004000,
            NewAdditon = 0x00005000,
            RequiredTask = 0x00006000,
            CampEnhancement = 0x00007000,
            Promotion = 0x00008000,
            CampMantTask = 0x00009000,
            // MAX          = 0x0001F000

            // Flags   MASK = 0x00180000
            Template = 0x00020000,
            AutoPersonID = 0x00040000,
            unk2 = 0x00080000,
            unk3 = 0x00100000,

            // Due     MASK = 0x7C000000 
            Before_Start = 0x00000001 << 26, //  400 0000
            At_Start = 0x00000002 << 26, //  800 0000
            Before_End = 0x00000003 << 26, //  C00 0000
            At_End = 0x00000004 << 26, // 1000 0000
            Hours_Before_Start = 0x00000005 << 26,
            Hours_After_Start = 0x00000006 << 26,
            Hours_Before_End = 0x00000007 << 26,
            Hours_After_End = 0x00000008 << 26,
            Days_Before_Start = 0x00000009 << 26,
            Days_After_Start = 0x0000000A << 26,
            Days_Before_End = 0x0000000B << 26,
            Days_After_End = 0x0000000C << 26,
            Weeks_Before_Start = 0x0000000D << 26,
            Weeks_After_Start = 0x0000000E << 26,
            Weeks_Before_End = 0x0000000F << 26,
            Weeks_After_End = 0x00000010 << 26,
            Months_Before_Start = 0x00000011 << 26,
            Months_After_Start = 0x00000012 << 26,
            Months_Before_End = 0x00000013 << 26,
            Months_After_End = 0x00000014 << 26,
            Day_Of_Week_SunMon = 0x00000015 << 26,
            //At_Time = 0x00000017 << 26,
            // MAX              = 0x0000001F << 26, 

            // Each    MASK   = 0x03E00000 
            Retreat_Event = 0x00000001 << 21, // 20 0000
            Week_Event = 0x00000002 << 21,
            Event_Day = 0x00000003 << 21,
            Work_Day = 0x00000004 << 21,
            Work_Week = 0x00000005 << 21,
            Work_Event = 0x00000006 << 21,
            ChCamp_Month = 0x00000007 << 21,
            Quarter = 0x00000008 << 21,
            First_Quarter = 0x00000009 << 21,
            Second_Quarter = 0x0000000A << 21,
            Third_Quarter = 0x0000000B << 21,
            Fourth_Quarter = 0x0000000C << 21,
            Event = 0x0000000D << 21,
            Month = 0x0000000E << 21,
            Year = 0x0000000F << 21,
            Due_Day = 0x00000010 << 21,
            Day_from_Due_till = 0x00000011 << 21,
            Months_Period_from_Date = 0x00000012 << 21,
            Years_Period_from_Date = 0x00000013 << 21,
            // MAX            = 0x0000001F << 21 
        }

        public enum ePriority
        {
            Required = 0x00000001,
            Urgent = 0x00000002,
            Very_High = 0x00000004,
            High = 0x00000008,
            Medium = 0x00000010,
            Low = 0x00000020
        }

        public enum eTaskStatus
        {
            // Percent Complete 0x7F

            // State
            Not_Started = 0x00000080,
            Planning = 0x00000100,
            Ready = 0x00000200,
            Active = 0x00000400,
            Paused = 0x00000800,
            Completed = 0x00001000,

            //Reason
            Need_Funds = 0x00100000,
            Need_Labor = 0x00200000,
            Need_Material = 0x00400000,
            Need_Inspection = 0x00800000,
            Need_Planning = 0x01000000,
            Job_Done = 0x02000000,
            Unknown = 0x04000000,
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
                    name = IsDue() ? "DueStyle" : (IsDueToday() ? "DueTodayStyle" : "NotDueStyle");
            }
            if ((i % 2) != 0)
                name = name + "Odd";
            return name;
        }

        public bool IsDue()
        {
            if ((Type & (int)eTaskType.Template) != 0)
                return false;
            //DateTime dtTol = PropMgr.ESTNow - TimeSpan.FromDays(1);
            return (((Status & (int)CTask.eTaskStatus.Completed) == 0) && DueDate.HasValue && DueDate.Value < PropMgr.ESTNow);
        }

        public bool IsDueToday()
        {
            if ((Type & (int)eTaskType.Template) != 0)
                return false;
            //DateTime dtTol = PropMgr.ESTNow - TimeSpan.FromDays(1);
            return (((Status & (int)CTask.eTaskStatus.Completed) == 0) && DueDate.HasValue && (DueDate.Value.Date == PropMgr.ESTNow.Date));
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

        public static List<CTask> GetDueTasks(CStat.Models.CStatContext context, int? pid, double hoursEarly)
        {
            DateTime dtThreshold = (hoursEarly == 0) ? dtThreshold = PropMgr.ESTNow : PropMgr.ESTNow + TimeSpan.FromHours(hoursEarly);
            return _GetDueTasks(context, pid, dtThreshold).Result;
        }

        public static async System.Threading.Tasks.Task<List<CTask>> _GetDueTasks(CStat.Models.CStatContext context, int? pid, DateTime threshold)
        {
            return (pid.HasValue) ?
                await context.Task.Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) == 0) &&
                ((t.Type & (int)CTask.eTaskType.Template) == 0) &&
                t.DueDate.HasValue && (t.DueDate <= threshold) &&
                t.PersonId.HasValue && (t.PersonId.Value == pid.Value)
                ).OrderBy(t => t.Id).ToListAsync()
            :
                await context.Task.Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) == 0) &&
                ((t.Type & (int)CTask.eTaskType.Template) == 0) && (t.DueDate.HasValue && t.DueDate <= threshold)).OrderBy(t => t.Id).ToListAsync();
        }

        public static List<CTask> GetAllTasks(CStat.Models.CStatContext context, DateRange dr, int? pid, bool hasDoneOnly)
        {
            return _GetAllTasks(context, dr, pid, hasDoneOnly).Result;
        }

        public static async System.Threading.Tasks.Task<List<CTask>> _GetAllTasks(CStat.Models.CStatContext context, DateRange dr, int? pid, bool hasDoneOnly)
        {

            //Status & (int)CTask.eTaskStatus.Completed
            if (hasDoneOnly)
            {
                return (dr != null) ? await context.Task.Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) != 0) && ((t.Type & (int)CTask.eTaskType.Template) == 0) && (!pid.HasValue || (t.PersonId.HasValue && (t.PersonId.Value == pid.Value))) &&
                            (t.DueDate.HasValue && (t.DueDate.Value.Date >= dr.Start.Date) && (t.DueDate.Value.Date <= dr.End.Date))).OrderBy(t => t.Id).ToListAsync() :
                      await context.Task.Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) != 0) && ((t.Type & (int)CTask.eTaskType.Template) == 0) && (!pid.HasValue || (t.PersonId.HasValue && (t.PersonId.Value == pid.Value)))).OrderBy(t => t.Id).ToListAsync();
            }
            else
            {
                return (dr != null) ? await context.Task.Include(t => t.Person).Where(t => ((t.Type & (int)CTask.eTaskType.Template) == 0) && (!pid.HasValue || (t.PersonId.HasValue && (t.PersonId.Value == pid.Value))) &&
                                            (t.DueDate.HasValue && (t.DueDate.Value.Date >= dr.Start.Date) && (t.DueDate.Value.Date <= dr.End.Date))).OrderBy(t => t.Id).ToListAsync() :
                                      await context.Task.Include(t => t.Person).Where(t => ((t.Type & (int)CTask.eTaskType.Template) == 0) && (!pid.HasValue || (t.PersonId.HasValue && (t.PersonId.Value == pid.Value)))).OrderBy(t => t.Id).ToListAsync();
            }
        }

        public static bool NotifyUserTaskDue (IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CStatContext context, double hoursEarly, bool forceClean=false)
        {
            var sms = new CSSMS(hostEnv, config, userManager);
            var tasks = GetDueTasks(context, null, hoursEarly);
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
            int eachVal = ((eachType == eTaskType.Months_Period_from_Date) || (eachType == eTaskType.Years_Period_from_Date)) ? this.Status : 0;
            dueDates = ApplyDueType(dueType, dueVal, GetEachTypeRange(eachType, eachVal, events, lim), lim);
            return dueDates.Count > 0;
        }
        public List<DateRange> GetEachTypeRange(CTask.eTaskType eachType, int eachVal, List<Event> events, DateRange lim)
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

                case eTaskType.Months_Period_from_Date:
                   {
                        if (DueDate.HasValue)
                        {
                            DateTime s = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 0, 0, 0).AddMonths(eachVal);
                            DateTime e = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 23, 59, 59).AddMonths(eachVal);
                            ranges.Add(new DateRange(s, e));
                        }
                    }
                    break;

                case eTaskType.Years_Period_from_Date:
                    {
                        if (DueDate.HasValue)
                        {
                            DateTime s = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 0, 0, 0).AddYears(eachVal);
                            DateTime e = new DateTime(DueDate.Value.Year, DueDate.Value.Month, DueDate.Value.Day, 23, 59, 59).AddYears(eachVal);
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
        public string NumMOYs { get; set; } = "";

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
            UNK,
            ICCOC,
            RCATH,
            CATH,
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
            public String Fax { get; set; }
            public String EMail { get; set; }
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
                new ChurchAtt("Unknown",              AffiliationType.UNK),
                new ChurchAtt("Indep. Christian / COC",   AffiliationType.ICCOC),
                new ChurchAtt("Roman Catholic",       AffiliationType.RCATH),
                new ChurchAtt("Catholic",       AffiliationType.CATH),
                new ChurchAtt("Lutheran",             AffiliationType.LUTH),
                new ChurchAtt("Anglican",             AffiliationType.ANG),
                new ChurchAtt("Episcopal",            AffiliationType.EPIS),
                new ChurchAtt("Southern Baptist",     AffiliationType.SOBAP),
                new ChurchAtt("Methodist",            AffiliationType.METH),
                new ChurchAtt("American Baptist",     AffiliationType.AMBAP),
                new ChurchAtt("Indep. Fund. Baptist",     AffiliationType.IFBAP),
                new ChurchAtt("Assembly of God",      AffiliationType.AOG),
                new ChurchAtt("Presbyterian",         AffiliationType.PRESB),
                new ChurchAtt("Non-Instrumental COC", AffiliationType.NICOC),
                new ChurchAtt("Congregational UCOC",    AffiliationType.CONG),
                new ChurchAtt("Non Denominational",   AffiliationType.NOND),
                new ChurchAtt("Other",                AffiliationType.OTHER)
                
            };

            public static ChurchAtt[] MemberStatList = new ChurchAtt[]
            {
                new ChurchAtt("n/a", MemberStatType.UNK_M),
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
        public enum DateRangeType
        {
            Before_Date,
            On_or_Before_Date,
            On_Date,
            On_or_After_Date,
            After_Date,
            All,
            For_a_Year_From_Date
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

        public static List<Event> GetEvents(CStatContext context, DateTime date, DateRangeType drType)
        {
            switch (drType)
            {
                case DateRangeType.Before_Date:
                    return context.Event.Where(e => e.StartTime.Date < date.Date).OrderBy(ev => ev.StartTime).ToList();
                case DateRangeType.On_or_Before_Date:
                    return context.Event.Where(e => e.StartTime.Date <= date.Date).OrderBy(ev => ev.StartTime).ToList();
                case DateRangeType.On_Date:
                    return context.Event.Where(e => e.StartTime.Date == date.Date).OrderBy(ev => ev.StartTime).ToList();
                case DateRangeType.On_or_After_Date:
                    return context.Event.Where(e => e.StartTime.Date >= date.Date).OrderBy(ev => ev.StartTime).ToList();
                case DateRangeType.After_Date:
                    return context.Event.Where(e => e.StartTime.Date > date.Date).OrderBy(ev => ev.StartTime).ToList();
                case DateRangeType.For_a_Year_From_Date:
                    DateTime yearLater = date.Date.AddYears(1);
                    return context.Event.Where(e => (e.StartTime.Date >= date.Date) && (e.StartTime.Date < yearLater.Date)).OrderBy(ev => ev.StartTime).ToList();
                default:
                case DateRangeType.All:
                    return context.Event.OrderBy(ev => ev.StartTime).ToList();
            }
        }

        public static List<Event> GetEvents(CStatContext context, int type1, int type2)
        {
            return context.Event.Where(e => (e.Type == type1) || (e.Type == type2)).ToList();
        }

        public static string GetEventNeeds(CStatContext context)
        {
            string needsStr = "";
            var eList = Event.GetEvents(context, PropMgr.ESTNow, DateRangeType.On_or_After_Date);

            var startRole = (int)CStat.Models.Attendance.AttendanceRoles.Sch_Host;
            eList.ForEach(e =>
            {
                int eRoleNeeds = e.Staff ?? 0;
                var aList = context.Attendance.Where(a => (a.EventId == e.Id) && (a.RoleType >= startRole)).ToList();
                aList.ForEach(a =>
                {
                    eRoleNeeds &= ~a.RoleType;
                });
                List<int> RoleList = Enum.GetValues(typeof(Attendance.AttendanceRoles)).Cast<int>().Where(r => (int)r >= (int)CStat.Models.Attendance.AttendanceRoles.Sch_Host).Select(x => x).ToList();

                RoleList.ForEach(r =>
                {
                    if ((r & eRoleNeeds) != 0)
                    {
                        string roleStr = ((Attendance.AttendanceRoles)Enum.ToObject(typeof(Attendance.AttendanceRoles), r)).ToString().Replace("Sch_", "").Replace("_", " ");
                        needsStr += roleStr + " needed: " + e.Description + "(" + PropMgr.DTStr(e.StartTime) + "-" + PropMgr.DTStr(e.EndTime) +  ").\n";
                    }
                });
            });

            return needsStr;
        }

        public static bool IsEventDay(CStatContext context, bool IncludeBanquet=true, int beforeHours=0, int aheadHours=0)
        {
            if ((beforeHours == 0) && (aheadHours == 0))
            {
                var today = PropMgr.ESTNow.Date;

                IQueryable<Event> events;
                if (IncludeBanquet)
                    events = context.Event.Where(e => (e.StartTime.Date <= today) && (e.EndTime.Date >= today));
                else
                    events = context.Event.Where(e => (e.Type != (int)EventType.Banquet) && (e.StartTime.Date <= today) && (e.EndTime.Date >= today));

                return events.Count() > 0;
            }
            else
            {
                var now = PropMgr.ESTNow;
                var stime = now.AddHours(-beforeHours);
                var etime = now.AddHours(-aheadHours);
                IQueryable<Event> sEvents;
                IQueryable<Event> eEvents;
                if (IncludeBanquet)
                {
                    sEvents = context.Event.Where(e => (e.StartTime <= stime) && (e.EndTime >= stime));
                    eEvents = context.Event.Where(e => (e.StartTime <= etime) && (e.EndTime >= etime));
                }
                else
                {
                    sEvents = context.Event.Where(e => (e.Type != (int)EventType.Banquet) && (e.StartTime <= stime) && (e.EndTime >= stime));
                    eEvents = context.Event.Where(e => (e.Type != (int)EventType.Banquet) && (e.StartTime <= etime) && (e.EndTime >= etime));
                }

                return (sEvents.Count() > 0) || (eEvents.Count() > 0);
            }
        }
    }

    public partial class Attendance
    {
        public enum AttendanceRoles
        {
            Unknown = 0, Visitor, Camper, Staff, Dean, Counselor, SWAT, Missionary, Speaker, Worship, HealthDir, nurse, Manager, Cook, kitchen, Worker, Trustee, ChurchRep, DropOff, PickUp, Other = 101,
            Sch_Host            = 0x000080,
            Sch_Dean            = 0x000100,
            Sch_Dean_Helper     = 0x000200,
            Sch_Boys_Counselor  = 0x000400,
            Sch_Girls_Counselor = 0x000800,
            Sch_Nurse           = 0x001000,
            Sch_Cook            = 0x002000,
            Sch_Kitchen_Helper  = 0x004000,
            Sch_SWAT1           = 0x008000,
            Sch_SWAT2           = 0x010000,
            Sch_SWAT3           = 0x020000,
            Sch_SWAT4           = 0x040000,
            Sch_Boys_Counselor2 = 0x080000,
            Sch_Girls_Counselr2 = 0x100000,
            Sch_Dean2           = 0x200000,
            Sch_Dean_Helper2    = 0x400000,
            Sch_Kitchen_Helper2 = 0x800000
        }
    }
    public partial class Business
    {
        public enum EType { Unknown = 0, NYS = 1, USGov, Propane, Electric, Phone, Internet, Refuse, Rentals, Hardware, Accounting, Bank_CC, Food, Water, Septic, Kitchen_Supply, Department, Office_Supply, Inspection, Insurance, Water_Test, Law, Recreation, Ministry, Auto_Repair, Tires_Repair }
        public enum EStatus { Vendor = 0, Required_Gov = 1, Preferred, Secondary, Suspended, Excluded, Out_of_Bus }

        public static string GetBizTypeOptions(EType selVal)
        {
            var selStr = ((int)selVal).ToString();

            IList<SelectListItem> BizTypeList = Enum.GetValues(typeof(Business.EType)).Cast<Business.EType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();

            string ops = "";
            foreach (var bt in BizTypeList)
            {
                if (bt.Value == selStr)
                    ops += "<option selected=\"selected\" value=\"" + bt.Value + "\" id=\"" + bt.Text + "\">" + bt.Text + "</option>\n";
                else
                    ops += "<option value=\"" + bt.Value + "\" id=\"" + bt.Text + "\">" + bt.Text + "</option>\n";
            }
            return ops;
        }
        public static string GetBizStatusOptions(EStatus selVal)
        {
            var selStr = ((int)selVal).ToString();

            IList<SelectListItem> BizStatList = Enum.GetValues(typeof(Business.EStatus)).Cast<Business.EStatus>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();

            string ops = "";
            foreach (var bt in BizStatList)
            {
                if (bt.Value == selStr)
                    ops += "<option selected=\"selected\" value=\"" + bt.Value + "\" id=\"" + bt.Text + "\">" + bt.Text + "</option>\n";
                else
                    ops += "<option value=\"" + bt.Value + "\" id=\"" + bt.Text + "\">" + bt.Text + "</option>\n";
            }
            return ops;
        }
    }

    public class UserInput
    {
        public string Pre { get; set; } = "";
        public string Input { get; set; } = "";
        public string Range { get; set; } = "";
        public string Post { get; set; } = "";
    }
    
}