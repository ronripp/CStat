using CStat.Areas.Identity.Data;
using CStat.Data;
using CStat.Models;
using CStat.Pages;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CStat.Common.PtzCamera;
using static CStat.Models.Event;

namespace CStat.Common
{
    public class CmdMgr
    {
        // default values recommended by http://isthe.com/chongo/tech/comp/fnv/
        static UInt32 Prime = 0x01000193; //   16777619
        static UInt32 Seed = 0x811C9DC5; // 2166136261

        public delegate string HandleSrcDel(List<string> words);

        public class PCh
        {
            public Person person { get; set; }
            public Church church { get; set; }
        }

        public enum CmdAction
        {
            FIND         = 0x00000000,
            COMPUTE      = 0x00000001,
            ADD          = 0x00000002,
            DELETE       = 0x00000004,
            UPDATE       = 0x00000008,
            CALL         = 0x00000010,
            ASK          = 0x00000020,
            CLEAN        = 0x00000040,
            BUY          = 0x00000080,
            USE          = 0x00000100,
            TURN_ON      = 0x00000200,
            TURN_OFF     = 0x00000400,
            RESET        = 0x00000800
        };

        public enum CmdFormat
        {
            NONE  = 0x00000000,
            TEXT  = 0x00000001,
            PDF   = 0x00000020,
            MSDOC = 0x00000040,
            EXCEL = 0x00000080,
            CSV   = 0x00000100
        };

        //private static readonly string[] StockHelper = 
        //{
        //    "tin foil", 
        //    "dish soap",
        //    "pot soap",
        //    "pan soap",
        //    "baking sheets",
        //    "chlorine tablets",
        //    "test strip",
        //    "dishwasher soap",
        //    "dishwasher barrel",
        //    "dishwasher barrels",
        //    "bleach",
        //    "nsf bleach",
        //    "kitchen filter",
        //    "kitchen water filter",
        //    "pre filter",
        //    "tube filter",
        //    "cone cups",
        //    "cooler cups",
        //    "grill screens",
        //    "griddle fluid",
        //    "griddle scrubbers",
        //    "battery",
        //    "bathoom rolls",
        //    "light bulbs",
        //    "dpd-1",
        //    "garbage bags",
        //    "hand soap",
        //    "laundry soap",
        //    "pots and pans soap",
        //    "tp",
        //    "toilet paper",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //    "",
        //};

        private static readonly Dictionary<string, CmdAction> CmdActionDict = new Dictionary<string, CmdAction>(StringComparer.OrdinalIgnoreCase)
        {
            {"Phone", CmdAction.CALL},
            {"Phones", CmdAction.CALL},
            {"get", CmdAction.FIND},
            {"send", CmdAction.FIND},
            {"show", CmdAction.FIND},
            {"report", CmdAction.FIND},
            {"list", CmdAction.FIND},
            {"locate", CmdAction.FIND},
            {"Info", CmdAction.FIND},
            {"Information", CmdAction.FIND},
            {"modify", CmdAction.UPDATE},
            {"change", CmdAction.UPDATE},
            {"append", CmdAction.ADD},
            {"we_need", CmdAction.BUY},
            {"order", CmdAction.BUY},
            {"buy", CmdAction.BUY},
            {"purchase", CmdAction.BUY},
            {"vendor", CmdAction.FIND},
            {"call", CmdAction.FIND},
            {"clean", CmdAction.CLEAN},
            {"wash", CmdAction.CLEAN}
        };

        private static readonly Dictionary<string, CmdFormat> CmdFormatDict = new Dictionary<string, CmdFormat>(StringComparer.OrdinalIgnoreCase)
        {
            {"text", CmdFormat.TEXT},
            {"text_file", CmdFormat.TEXT},
            {"pdf", CmdFormat.PDF},
            {"pdf_file", CmdFormat.PDF},
            {"excel", CmdFormat.EXCEL},
            {"spread-sheet", CmdFormat.EXCEL},
            {"spreadsheet", CmdFormat.EXCEL},
            {"spreadlist", CmdFormat.EXCEL},
            {"sheet", CmdFormat.EXCEL},
            {"sheets", CmdFormat.EXCEL},
            {"ss", CmdFormat.EXCEL},
            {"ms_word", CmdFormat.MSDOC},
            {"microsoft_word", CmdFormat.MSDOC},
            {"word_doc", CmdFormat.MSDOC},
            {"comma_delimited", CmdFormat.CSV},
            {"csv", CmdFormat.CSV},
        };

        //=================================================================
        public enum CmdSource
        {
            NONE       = 0x00000000,
            QUESTION   = 0x00000001,
            INVENTORY  = 0x00000002,
            PERSON     = 0x00000004,
            BUSINESS   = 0x00000008,
            TASK       = 0x00000010,
            REQ        = 0x00000020,
            DOC        = 0x00000040,
            MMS        = 0x00000041,
            EQUIP      = 0x00000080,
            MENU       = 0x00000100,
            PROPANE    = 0x00000200,
            FRIDGE     = 0x00000400,
            FREEZER    = 0x00000800,
            PRESSURE   = 0x00001000,
            ELECTRIC   = 0x00002000,
            CAMERA     = 0x00004000,
            BYLAWS     = 0x00008000,
            EVENT      = 0x00010000,
            ATTENDANCE = 0x00020000,
            URGENCY    = 0x00040000,
            CORP       = 0x00080000,
            TRUSTEE    = 0x00100000,
            EC         = 0x00200000,
            CHURCH     = 0x00400000,
            TRASH      = 0x00800000,
            INTERNET   = 0x01000000,
            NYSDOH     = 0x02000000,
            FOOD       = 0x04000000,
            HARDWARE   = 0x08000000,
            OFFICE     = 0x10000000,
            POWER_CO   = 0x20000000,
            KITCH_TEMP = 0x40000000,

        }

        public enum CmdSpecific
        {
            // phone_number of
            // email of
            // email_address of
            // names of
            // manual of
            // address of
            // manual of
            // mailing_list of
            // location of
            // Where is/are the/a
            // Where can we/I/someone
            // How do/does we/I/someone/one/ <use/operate/perform/do>
            // How to <use/operate/perform/do>
            // What is/are
            // Who is/can/has a/the
            // What is are

            NONE         = 0x00000000,
            EMAIL_OF     = 0x00000001,
            PHONE_OF     = 0x00000002,
            HOW_TO       = 0x00000004,
            LOCATION_OF  = 0x00000008,
            DATE_TIME_OF = 0x00000010,
            NAME_OF      = 0x00000020,
            OWNER_OF     = 0x00000040,
            INFO_ON      = 0x00000080,
            AMOUNT_OF    = 0x00000100,
            STATE_OF     = 0x00000200,
            REASON_OF    = 0x00000400,
            NEED_OF      = 0x00000800,
            EXECUTE      = 0x00001000
        }

        private static readonly Dictionary<string, Tuple<CmdSource, bool>> CmdSrcDict = new Dictionary<string, Tuple<CmdSource, bool>>(StringComparer.OrdinalIgnoreCase)
        {
            {"stock", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, false) },
            {"items", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, true) },
            {"supply", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, false) },
            {"supplies", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, true) },
            {"tank", new Tuple<CmdSource, bool>(CmdSource.PROPANE, false) },
            {"fuel", new Tuple<CmdSource, bool>(CmdSource.PROPANE, false) },
            {"gas", new Tuple<CmdSource, bool>(CmdSource.PROPANE, false) },
            {"lpg", new Tuple<CmdSource, bool>(CmdSource.PROPANE, false) },
            {"lp", new Tuple<CmdSource, bool>(CmdSource.PROPANE, false) },
            {"propanes", new Tuple<CmdSource, bool>(CmdSource.PROPANE, true) },
            {"power", new Tuple<CmdSource, bool>(CmdSource.ELECTRIC, false) },
            {"electricity", new Tuple<CmdSource, bool>(CmdSource.ELECTRIC, false) },
            {"meter", new Tuple<CmdSource, bool>(CmdSource.ELECTRIC, false) },
            {"insurance", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"tax", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"990", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"1023", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"tax-exempt", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"ST-119.1", new Tuple<CmdSource, bool>(CmdSource.CORP, false) },
            {"inv", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, false) },
            {"?", new Tuple<CmdSource, bool>(CmdSource.QUESTION, false) },
            {"pic", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"photo", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"shot", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"snapshot", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) }, // C -
            {"image", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"cam", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"refrigerator", new Tuple<CmdSource, bool>(CmdSource.FRIDGE, false) },
            {"freezer", new Tuple<CmdSource, bool>(CmdSource.FREEZER, false) },
            {"doc", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"manual", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"document", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"file", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"meeting", new Tuple<CmdSource, bool>(CmdSource.MMS, false) },
            {"minutes", new Tuple<CmdSource, bool>(CmdSource.MMS, false) },
            {"meeting-minutes", new Tuple<CmdSource, bool>(CmdSource.MMS, false) },
            {"mms", new Tuple<CmdSource, bool>(CmdSource.MMS, false) },
            {"mm", new Tuple<CmdSource, bool>(CmdSource.MMS, false) },
            {"bylaws", new Tuple<CmdSource, bool>(CmdSource.BYLAWS, false) },
            {"by-laws", new Tuple<CmdSource, bool>(CmdSource.BYLAWS, false) },
            {"pics", new Tuple<CmdSource, bool>(CmdSource.CAMERA, true) },
            {"photos", new Tuple<CmdSource, bool>(CmdSource.CAMERA, true) },
            {"shots", new Tuple<CmdSource, bool>(CmdSource.CAMERA, true) },
            {"snapshots", new Tuple<CmdSource, bool>(CmdSource.CAMERA, true) }, // C -
            {"docs", new Tuple<CmdSource, bool>(CmdSource.DOC, true) },
            {"manuals", new Tuple<CmdSource, bool>(CmdSource.DOC, true) },
            {"documents", new Tuple<CmdSource, bool>(CmdSource.DOC, true) },
            {"infos", new Tuple<CmdSource, bool>(CmdSource.DOC, true) },
            {"procedure", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"procedures", new Tuple<CmdSource, bool>(CmdSource.DOC, true) },
            {"instructions", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"requirements", new Tuple<CmdSource, bool>(CmdSource.REQ, true) },
            {"required", new Tuple<CmdSource, bool>(CmdSource.REQ, false) },
            {"trustees", new Tuple<CmdSource, bool>(CmdSource.TRUSTEE, true) },
            {"delegate", new Tuple<CmdSource, bool>(CmdSource.TRUSTEE, false) },
            {"delegates", new Tuple<CmdSource, bool>(CmdSource.TRUSTEE, true) },
            {"director", new Tuple<CmdSource, bool>(CmdSource.TRUSTEE, false) },
            {"directors", new Tuple<CmdSource, bool>(CmdSource.TRUSTEE, true) },
            {"ec", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"term", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"terms", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"executive committee", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"executive board", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"committee", new Tuple<CmdSource, bool>(CmdSource.EC, false) },
            {"church", new Tuple<CmdSource, bool>(CmdSource.CHURCH, false) },
            {"churches", new Tuple<CmdSource, bool>(CmdSource.CHURCH, true) },
            {"garbage", new Tuple<CmdSource, bool>(CmdSource.TRASH, false) },
            {"rubbish", new Tuple<CmdSource, bool>(CmdSource.TRASH, false) },
            {"cable", new Tuple<CmdSource, bool>(CmdSource.INTERNET, false) },
            {"mtc", new Tuple<CmdSource, bool>(CmdSource.INTERNET, false) },
            {"new york", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"ny", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"nys", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"state", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"doh", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"health", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"department", new Tuple<CmdSource, bool>(CmdSource.NYSDOH, false) },
            {"name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"names", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"last_name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"first_name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"phone_list", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"people_list", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"mailing_list", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"mailing", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },

            {"email_list", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"email_address", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"emails", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },

            {"contact", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"contacts", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"campers", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },

            {"kitchen_supply", new Tuple<CmdSource, bool>(CmdSource.FOOD, false) },
            {"grocery", new Tuple<CmdSource, bool>(CmdSource.FOOD, false) },
            {"groceries", new Tuple<CmdSource, bool>(CmdSource.FOOD, true) },
            {"produce", new Tuple<CmdSource, bool>(CmdSource.FOOD, false) },

            {"hardware", new Tuple<CmdSource, bool>(CmdSource.HARDWARE, false) },
            {"lumber", new Tuple<CmdSource, bool>(CmdSource.HARDWARE, false) },

            {"office", new Tuple<CmdSource, bool>(CmdSource.OFFICE, false) },
            {"office supply", new Tuple<CmdSource, bool>(CmdSource.OFFICE, false) },

            {"power_company", new Tuple<CmdSource, bool>(CmdSource.POWER_CO, false) },

            //{pots_and_pans}
            //{sink}
            //{bathroom}
            //dishwasher
            //lawnmover
            //tools
            //water
            //tanks

            {"vendors", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, false) },
            {"business", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, false) },
            {"biz", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, false) },
            {"store", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, false) },
            {"stores", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, true) },
            {"businesses", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, true) },
            {"company", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, false) },
            {"companies", new Tuple<CmdSource, bool>(CmdSource.BUSINESS, true) },

            {"req", new Tuple<CmdSource, bool>(CmdSource.REQ, false) },
            {"reqs", new Tuple<CmdSource, bool>(CmdSource.REQ, true) },
            {"list", new Tuple<CmdSource, bool>(CmdSource.URGENCY, false) },
            {"todo", new Tuple<CmdSource, bool>(CmdSource.URGENCY, false) },
            {"tasks", new Tuple<CmdSource, bool>(CmdSource.TASK, true) },
            {"alert", new Tuple<CmdSource, bool>(CmdSource.URGENCY, false) },
            {"alerts", new Tuple<CmdSource, bool>(CmdSource.URGENCY, true) },
            {"alarm", new Tuple<CmdSource, bool>(CmdSource.URGENCY, false) },
            {"alarms", new Tuple<CmdSource, bool>(CmdSource.URGENCY, true) },
            {"urgent", new Tuple<CmdSource, bool>(CmdSource.URGENCY, false) },
            {"urgencies", new Tuple<CmdSource, bool>(CmdSource.URGENCY, true) },
            {"events", new Tuple<CmdSource, bool>(CmdSource.EVENT, true) },
            {"phone#", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"phonenumber", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"phone_number", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"phone_numbers", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"phone", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"people", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) }
        };

        //===============================================================
        public enum CmdInsts
        {
            CURRENT = 0x00000000,
            FIRST   = 0x00000001,
            LAST    = 0x00000002,
            PRIOR   = 0x00000004,
            NEXT    = 0x00000008,
            YEAR    = 0x00000010,
            YEAR_MONTH = 0x00000020,
            DATE_RANGE = 0x00000040,
            OVERDUE    = 0x00000080,
            MY         = 0x00000100,
            DONE       = 0x00000200,
            SPECIFIC   = 0x00000400,
            YESTERDAY  = 0x0000080,
            LAST_WEEK  = 0x00001000,
            LAST_MONTH = 0x00002000,
            LAST_YEAR  = 0x00004000,
            ALL =        0x00008000
        }

        private static readonly Dictionary<string, CmdInsts> CmdInstsDict = new Dictionary<string, CmdInsts>(StringComparer.OrdinalIgnoreCase)
        {
            {"every", CmdInsts.ALL},
            {"full", CmdInsts.ALL},
            {"present", CmdInsts.CURRENT},
            {"todays", CmdInsts.CURRENT},
            {"today", CmdInsts.CURRENT},
            {"yesterday", CmdInsts.YESTERDAY},
            {"yesterdays", CmdInsts.YESTERDAY},
            {"new", CmdInsts.NEXT},
            {"old", CmdInsts.PRIOR},
            {"past", CmdInsts.PRIOR},
            {"future", CmdInsts.NEXT},
            {"last_week", CmdInsts.LAST_WEEK},
            {"last_month", CmdInsts.LAST_MONTH},
            {"last_year", CmdInsts.LAST_YEAR},
            {"coming", CmdInsts.NEXT},
            {"upcoming", CmdInsts.NEXT},    // c -
            {"overdue", CmdInsts.OVERDUE},  // c -
            {"delinquent", CmdInsts.OVERDUE},
            {"late", CmdInsts.OVERDUE},
            {"mine", CmdInsts.MY},
            {"me", CmdInsts.MY}
        };

        public enum CmdOutput
        {
            REPLY = 0x00000000,
            EMAIL = 0x00000001
        }

        private readonly CStat.Models.CStatContext _context;
        private readonly CSSettings _csSettings;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private readonly CSUser _curUser;
        private readonly bool _isTextMsg;

        public CmdMgr(CStat.Models.CStatContext context, CSSettings cset, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CSUser curUser, bool isTextMsg)
        {
            _context = context;
            _csSettings = cset;
            _hostEnv = hostEnv;
            _config = config;
            _userManager = userManager;
            _curUser = curUser;
            _isTextMsg = isTextMsg;
            _isQuestion = false;
            _isUrgent = false;

            _srcDelegateDict.Add(CmdSource.MENU, HandleMenu);
            _srcDelegateDict.Add(CmdSource.INVENTORY, HandleInventory);
            _srcDelegateDict.Add(CmdSource.TASK, HandleTasks);
            _srcDelegateDict.Add(CmdSource.PERSON, HandlePeople);
            _srcDelegateDict.Add(CmdSource.TRUSTEE, HandlePeople);
            _srcDelegateDict.Add(CmdSource.EC, HandlePeople);

            _srcDelegateDict.Add(CmdSource.MMS, HandleMMs);
            _srcDelegateDict.Add(CmdSource.DOC, HandleDocs);
            _srcDelegateDict.Add(CmdSource.CORP, HandleCorp);
            _srcDelegateDict.Add(CmdSource.REQ, HandleDocs);
            _srcDelegateDict.Add(CmdSource.BYLAWS, HandleDocs);

            _srcDelegateDict.Add(CmdSource.EQUIP, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PROPANE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FRIDGE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FREEZER, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PRESSURE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.ELECTRIC, HandleEquip);
            _srcDelegateDict.Add(CmdSource.KITCH_TEMP, HandleEquip);

            _srcDelegateDict.Add(CmdSource.INTERNET, HandleBiz);
            _srcDelegateDict.Add(CmdSource.TRASH, HandleBiz);
            _srcDelegateDict.Add(CmdSource.NYSDOH, HandleBiz);
            _srcDelegateDict.Add(CmdSource.BUSINESS, HandleBiz);
            _srcDelegateDict.Add(CmdSource.FOOD, HandleBiz);
            _srcDelegateDict.Add(CmdSource.HARDWARE, HandleBiz);
            _srcDelegateDict.Add(CmdSource.OFFICE, HandleBiz);
            _srcDelegateDict.Add(CmdSource.POWER_CO, HandleBiz);

            _srcDelegateDict.Add(CmdSource.EVENT, HandleEvents);
            _srcDelegateDict.Add(CmdSource.ATTENDANCE, HandleAttendance);
            _srcDelegateDict.Add(CmdSource.URGENCY, HandleUrgency);
            _srcDelegateDict.Add(CmdSource.CHURCH, HandleChurch);

            _srcDelegateDict.Add(CmdSource.CAMERA, HandleCamera);

            //_srcDelegateDict.Add(CmdSource.CSTEST, HandleCSTest);
        }

        public static List<string> GetWords(string cmdStr)
        {
            cmdStr = cmdStr.ToLower()
                           .Replace("  ", " ")
                           .Replace("sop ", "sop~")
                           .Replace("'s", "s")
                           .Replace("young adults", "young adult")
                           .Replace("yar", "young adult retreat")
                           .Replace("sop-", "sop~")
                           .Replace("-", "")
                           .Replace("microsoft word", "microsoft_word")
                           .Replace("ms word", "microsoft_word")
                           .Replace("word doc", "word_doc")
                           .Replace("pdf file", "pdf_file")
                           .Replace("text file", "text_file")
                           .Replace("comma delimited", "comma_delimited")
                           .Replace("dock", "doc")
                           .Replace("e-mail", "email")
                           .Replace("a email", "via_email")
                           .Replace("an email", "via_email")
                           .Replace("as email", "via_email")
                           .Replace("via email", "via_email")
                           .Replace("the email", "email_address")
                           .Replace("email addresses", "email_list")
                           .Replace("email address", "email_address")
                           .Replace("last names", "last_name")
                           .Replace("last name", "last_name")
                           .Replace("first names", "first_name")
                           .Replace("first name", "first_name")
                           .Replace("phone #", "phone_number")
                           .Replace("phone number", "phone_number")
                           .Replace("phone #s", "phone_list")
                           .Replace("phone numbers", "phone_list")
                           .Replace("phone directory", "phone_list")
                           .Replace("listing", "list")
                           .Replace("mailing addresses", "mailing_list")
                           .Replace("mailing address", "mailing")
                           .Replace("list of contacts", "people_list")
                           .Replace("list of people", "people_list")
                           .Replace("people list", "people_list")
                           .Replace("people directory", "people_list")
                           .Replace("contact list", "people_list")
                           .Replace("contact directory", "people_list")
                           .Replace("camper list", "people_list")
                           .Replace("camper directory", "people_list")
                           .Replace("list of people", "people_list")
                           .Replace("camper list", "people_list")
                           .Replace("list of campers", "people_list")
                           .Replace("mailing list", "mailing_list")
                           .Replace("list of contacts", "people_list")
                           .Replace("building supplies", "hardware")
                           .Replace("building supply", "hardware")
                           .Replace("kitchen supply", "kitchen_supply")
                           .Replace("kitchen supplies", "kitchen_supply")
                           .Replace("office supply", "office_supply")
                           .Replace("office supplies", "office_supply")
                           .Replace("email list", "email_list")
                           .Replace("list of email addresses", "email_list")
                           .Replace("list of emails", "email_list")
                           .Replace("turn on", "turn_on")
                           .Replace("turn off", "turn_off")
                           .Replace("electric power company", "power_company")
                           .Replace("power company", "power_company")
                           .Replace("electric company", "power_company")
                           .Replace("last week", "last_week")
                           .Replace("last weeks", "last_week")
                           .Replace("last month", "last_month")
                           .Replace("last months", "last_month")
                           .Replace("last year", "last_year")
                           .Replace("last years", "last_year")
                           .Replace("kitchen temperature", "kitch_temp")
                           .Replace("kitchen temp", "kitch_temp")
                           .Replace("temperature in kitchen", "kitch_temp")
                           .Replace("walk-in freezer", "freezer")
                           .Replace("walk-in fridge", "refrigerator")
                           .Replace("walk-in refrigerator", "refrigerator")
                           .Replace("walk in freezer", "freezer")
                           .Replace("walk in fridge", "refrigerator")
                           .Replace("walk in refrigerator", "refrigerator")
                           .Replace("spread sheet", "spread-sheet")
                           .Replace("tax exempt", "tax-exempt")
                           .Replace("tax exemption", "tax-exempt")
                           .Replace("we need", "we_need");
           
            return new List<string>(cmdStr.Split(new char[] { ' ', '\n', ',', ';' })).Select(w => w.Trim()).ToList();
        }

        public bool ParseCmd(List<string> stWords)
        {
            var NumWords = stWords.Count;
            var words = new List<string>();

            // Create Clean List in words
            List<string> rawWords = new List<string>(stWords);
            for (int i = 0; i < NumWords; ++i)
            {
                var word = rawWords[i];

                if (i == NumWords - 1)
                {
                    // Interpret and clear out ending punctuation
                    if (word.EndsWith("?"))
                    {
                        _isQuestion = true;
                        word = word.Replace("?", "");
                    }
                    if (word.EndsWith("."))
                    {
                        word = word.Replace(".", "");
                    }
                    if (word.EndsWith("!"))
                    {
                        _isUrgent = true;
                        word = word.Replace("!", "");
                    }
                }

                if ((word == "camp") || (word == "please"))
                    continue; // skip because it is implied / gets in the way

                int j = i + 1;
                int k = j + 1;

                if (k < NumWords)
                {
                    var word2 = rawWords[j];
                    var word3 = rawWords[k];
                    // Combine certain words
                    if ((word == "new") && (word2 == "york") && (word3 == "state"))
                    {
                        words.Add("nys");
                        i = k;
                        continue;
                    }
                }

                if (j < NumWords)
                {
                    // Combine certain words
                    var word2 = rawWords[j];
                    if (((word == "young") && (word2.StartsWith("adult"))) ||
                        ((word == "spring") && (word2.StartsWith("work"))) ||
                        ((word == "first") && (word2.StartsWith("chance"))))
                    {
                        word = word + " " + word2; // Combine with space
                        i = j;
                    }
                    else if (((word == "up") && (word2.StartsWith("coming"))) ||
                             ((word == "to") && (word2.StartsWith("do"))) ||
                             ((word == "phone") && (word2.StartsWith("number"))) ||
                             ((word == "phone") && (word2.StartsWith("#"))) ||
                             ((word == "over") && (word2.StartsWith("due"))))
                    {
                        word = word + word2; // Combine with NO space
                        i = j;
                    }
                    else if (((word == "meeting") && (word2.StartsWith("minutes"))) ||
                             ((word == "by") && (word2.StartsWith("laws"))))
                    {
                        word = word + "-" + word2; // Combine with hyphen
                        i = j;
                    }
                }

                words.Add(word);
            }

            _cmdDescList.Clear(); // Clear first as any of the Finds below may add to it.

            FindCmdSpecific(words);

            FindCmdSource(words); // Look for command source first
            FindCmdOutput(words); // Look for command output second
            FindCmdAction(words);
            FindInsts(words);
            FindNumber(words);
            FindDates(words);
            FindEvent(words);
            FindDesc(words); // Check for descriptive hints
            FindCmdFormat(words);

            if ((_cmdSrc == CmdSource.NONE) && (_cmdDescList.Count > 0))
            {
                // Check events, inventory or equipment match
                if (!FindCmdOther(words))
                {
                    _cmdSrc = CmdSource.PERSON; // default
                }
            }

            if (_cmdFormat != CmdFormat.TEXT)
                _cmdOutput = CmdOutput.EMAIL; // Send files via EMail

            if ((words.Count == 1) && (words[0].Length == 0))
            {
                _cmdSrc = CmdSource.MENU;
                return true;
            }

            if ((_cmdSrc == default) && ((_cmdAction == CmdAction.CALL) || (_cmdDescList.Count == 1) || (_cmdDescList.Count == 2)))
                _cmdSrc = CmdSource.PERSON;

            if ( ((_cmdAction == CmdAction.BUY) || (_cmdAction == CmdAction.ADD)) && ((_cmdSrc == default) || (_cmdSrc == CmdSource.MENU)))
                _cmdSrc = CmdSource.URGENCY;

            if (_hasMy && !_curUser.pid.HasValue)
                return false; // My with no person id;

            return true;
        }

        public string ExecuteCmd(string cmdStr, bool noEMail=false)
        {
            _rawCmd = cmdStr;
            var words = CmdMgr.GetWords(cmdStr);
            if (!ParseCmd(words))
                return ("Try later.");
            var result = _srcDelegateDict.TryGetValue(_cmdSrc, out HandleSrcDel cmdDel) ? cmdDel(words) : "Huh?";

            if (!_alreadySentEMail && !noEMail && (IsEMailResult(words) || (_isTextMsg && (result.Length > (TwilioReceive.Controllers.SmsController.MaxSendChars - 500)))) ) // TBD Determine better limit besides - 500
            {
                CSEMail csEMail = new CSEMail(_config, _userManager);
                return SrcTitle(_cmdSrc) + (csEMail.Send(_curUser.EMail, _curUser.EMail, "RE: " + _rawCmd, result) ? " successfully sent to " : " FAILED to be sent to ") + _curUser.EMail;
            }
            return (result.Length > 0) ? result : "No Results.";
        }

        private bool IsEMailResult(List<string> words)
        {
            if (_cmdSpecific == CmdSpecific.EMAIL_OF)
                return false;

            return ( ((words[0] == "email") && ((words.Count == 1) || (words[1] != "of"))) ||
                     ((words.Count > 2) && (words[0] == "send" && words[1] == "email")));
        }

        private string SrcTitle (CmdSource cs)
        {
            try
            {
                var raw = cs.ToString();
                return raw.Substring(0, 1) + raw.Substring(1).ToLower();
            }
            catch
            {
                return "Results";
            }
        }

        static bool ew(List<string> words, int index, string target)
        {
            return (index < words.Count) && string.Equals(words[index], target, StringComparison.OrdinalIgnoreCase);
        }

        static string gw(List<string> words, int index)
        {
            return (index < words.Count) ? words[index] : "";
        }

        public bool FindCmdOther(List<string> words)
        {
            string sentence = "";
            int NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
                sentence += (words[i] + " ");
            }
            sentence = sentence.Trim();

            if (sentence.Contains("temporary residen"))
            {
                _cmdSrc = CmdSource.CORP;
                _cmdDescList.Add("trp");
                _cmdDescIdxList.Add(_cmdDescList.Count-1);
                return true;
            }

            if ((_cmdSrc == CmdSource.NONE) && (words.Count == 1) && words[0].StartsWith("sop"))
            {
                if (_cmdDescList.Count == 0)
                {
                    _cmdDescList.Add(words[0]);
                    _cmdDescIdxList.Add(0);
                }
                _cmdSrc = CmdSource.DOC;
                return true;
            }

            if ((_cmdSrc == CmdSource.NONE) && (words.Count >= 1))
            {
                // Check for Event match
                if (FindEvent(words, true))
                {
                    _cmdSrc = CmdSource.EVENT;
                    return true;
                }

                if (words.Count <= 2)
                {

                    // Check for a match with first name 
                    if (Person.SameFirstName(words[0], words[0]))
                    {
                        _cmdSrc = CmdSource.PERSON;
                        return true;
                    }
                }
            }

            CSSettings cset = CSSettings.GetCSSettings(_config, _userManager);
            var InvList = InventoryItem.GetInventoryList(_context, _config, false);

            int BestEquipMatch = 0;
            EquipProp BestEquipProp = null;
            foreach (var e in cset.EquipProps)
            {
                int curMatch = 0;
                foreach (var w in _cmdDescList)
                {
                    var equipTitle = e.Title.ToLower();
                    if (w.Equals(equipTitle, StringComparison.OrdinalIgnoreCase))
                        curMatch += 10;
                    else if (equipTitle.Contains(w))
                         curMatch += 8;
                    else
                        curMatch += LevenshteinDistance.Compute(w, equipTitle) switch { 0 => 10, 1 => 7, 2 => 5, _ => 0 };
                }
                if (curMatch > BestEquipMatch)
                {
                    BestEquipMatch = curMatch;
                    BestEquipProp = e;
                }
            }

            int BestItemMatch = 0;
            InventoryState BestInvState = null;
            foreach (var i in InvList)
            {
                int curMatch = 0;
                foreach (var w in _cmdDescList)
                {
                    var itemName = i.Name.ToLower();
                    if (w.Equals(itemName, StringComparison.OrdinalIgnoreCase))
                        curMatch += 10;
                    else if (itemName.Contains(w))
                        curMatch += 8;
                    else
                        curMatch += LevenshteinDistance.Compute(w, itemName) switch { 0 => 10, 1 => 7, 2 => 5, _ => 0 };
                }
                if (curMatch > BestItemMatch)
                {
                    BestItemMatch = curMatch;
                    BestInvState = i;
                }
            }

            if ((_cmdAction == CmdAction.BUY) && (BestItemMatch > 0))
            {
                if (_cmdSrc == CmdSource.NONE)
                    _cmdSrc = CmdSource.INVENTORY;
                _inventoryStateMatch = BestInvState;
                return true;
            }

            if (BestEquipMatch > BestItemMatch)
            {
                if (_cmdSrc == CmdSource.NONE)
                    _cmdSrc = CmdSource.EQUIP;
                _equipPropMatch = BestEquipProp;
                if (BestItemMatch > 0)
                    _inventoryStateMatch = BestInvState;
                return true;
            }
            if (BestItemMatch > BestEquipMatch)
            {
                if (_cmdSrc == CmdSource.NONE)
                    _cmdSrc = CmdSource.INVENTORY;
                _inventoryStateMatch = BestInvState;
                if (BestEquipMatch > 0)
                    _equipPropMatch = BestEquipProp;
                return true;
            }
            return false;
        }
        public bool FindCmdSpecific(List<string> words)
        {
            var NumWords = words.Count;
            // phone_number of
            // email of
            // email_address of
            // names of
            // manual of
            // address of
            // manual of
            // mailing_list of
            // location of
            // Where is/are the/a
            // Where can we/I/someone
            // How do/does we/I/someone/one/ <use/operate/perform/do>
            // How to <use/operate/perform/do>
            // What is/are
            // Who is/can/has a/the
            // What is are
            // Why did/does/is/are the 

            //EMAIL_OF = 0x00000001,
            //PHONE_OF = 0x00000002,
            //HOW_TO = 0x00000004,
            //LOCATION_OF = 0x00000008,
            //DATE_TIME_OF = 0x00000010,
            //NAME_OF = 0x00000020,
            //OWNER_OF = 0x00000040,
            //INFO_ON = 0x00000080,
            //AMOUNT_OF = 0x00000100,
            //STATE_OF = 0x00000200,
            //REASON_OF = 0x00000400,
            //EXECUTE = 0x00000800

            if (NumWords == 0) return false;

            int idx = words.IndexOf("please");
            if (idx != -1)
                _cmdIgnoreIdxList.Add(idx);

            _cmdSpecific = CmdSpecific.NONE;

            for (int curIdx = 0; curIdx < NumWords;)
            {

                // TBD : Handle preceding tell me, show me, please give me.
                // TBD ending with to m

                string w = "";
                var startIdx = curIdx;
                switch (w = words[curIdx])
                {
                    case "are":
                    case "am":
                        if (curIdx == 0)
                        {
                            w = gw(words, curIdx + 1);
                            if ((w == "we") || (_isPlural = (w == "I")))
                            {
                                _isQuestion = true;
                                _cmdIgnoreIdxList.Add(curIdx);
                                _cmdIgnoreIdxList.Add(++curIdx);
                            }
                        }
                        break;

                    case "who":
                        _cmdSpecific = CmdSpecific.OWNER_OF;
                        _cmdSpecificIdxList.Add(curIdx);
                        _isQuestion = true;
                        w = gw(words, curIdx + 1);
                        if ((w == "is") || (_isPlural = (w == "are")))
                        {
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx + 1);
                            if ((w == "the") || (w == "a") || (w == "an"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }
                        return true;

                    case "needed":
                    case "needs":
                        if (_isQuestion)
                        {
                            _cmdAction = CmdAction.ASK;
                            _cmdActionIdx = curIdx;
                            _cmdSpecific = CmdSpecific.STATE_OF;
                            _cmdSpecificIdxList.Add(curIdx);
                        }

                        w = gw(words, curIdx+1);
                        if ((w == "is") || (_isPlural = (w == "are")))
                        {
                            _isQuestion = true;
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx+1);
                            if ((w == "the") || (w == "a") || (w == "an"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }
                        break;

                    case "what":
                        _cmdIgnoreIdxList.Add(curIdx);
                        w = gw(words, curIdx+1);
                        if ((w == "is") || (_isPlural = (w == "are")))
                        {
                            _isQuestion = true;
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx + 1);
                            if ((w == "the") || (w == "a") || (w == "an"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }

                        if (w == "do")
                        {
                            _isQuestion = true;
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx + 1);
                            if ((w == "the") || (w == "a") || (w == "an") || (w == "we") || (w == "I"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }

                        if ((w == "location") || (w == "place") || (w == "address") || (w == "street"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.LOCATION_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }
                        if ((w == "time") || (w == "date"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.DATE_TIME_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                            {
                                _cmdIgnoreIdxList.Add(++curIdx);
                                w = gw(words, curIdx + 1);
                                if (w == "the")
                                    _cmdIgnoreIdxList.Add(++curIdx);
                            }
                            return true;
                        }
                        if ((w == "cost") || (w == "amount") | (w == "quantity"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.AMOUNT_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }
                        if ((w == "phone") || (w == "phone_number") || (w == "number"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.PHONE_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }
                        if ((w == "email") || (w == "email_address"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.EMAIL_OF;
                            w = gw(words, curIdx+1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }
                        if ((w == "state") || (w == "status") || (w == "current"))
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.STATE_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }

                        if (w == "name")
                        {
                            _cmdSpecificIdxList.Add(curIdx);
                            _cmdSpecific = CmdSpecific.NAME_OF;
                            w = gw(words, curIdx + 1);
                            if ((w == "of") || (w == "for"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                            return true;
                        }
                        break;

                    case "where":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.LOCATION_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "is") || (_isPlural = (w == "are")))
                        {
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx + 1);
                            if (w == "the")
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }
                        return true;

                    case "when":
                        _isWhen = true;
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.DATE_TIME_OF;
                        w = gw(words, curIdx+1);
                        if ((w == "is") || (_isPlural = (w == "are")))
                        {
                            _cmdIgnoreIdxList.Add(++curIdx);
                            w = gw(words, curIdx + 1);
                            if ((w == "the") || (w == "a") || (w == "an"))
                                _cmdIgnoreIdxList.Add(++curIdx);
                        }
                        return true;

                    case "need":
                    case "needing":
                    case "run":
                    case "running":
                    case "out":
                    case "low":
                        {
                            bool isNeed = w.StartsWith("need");
                            if (w.StartsWith("run"))
                            {
                                _cmdSpecificIdxList.Add(curIdx);
                                w = gw(words, curIdx + 1);
                            }
                            else if (isNeed)
                            {
                                _cmdSpecificIdxList.Add(curIdx);
                                w = gw(words, curIdx + 1);
                                if (w == "more")
                                    _cmdIgnoreIdxList.Add(++curIdx);
                            }

                            if ((w == "out") || (w == "low") || isNeed)
                            {
                                w = gw(words, curIdx + 1);
                                bool isOfOn = (w == "of") || (w == "on");
                                if (isOfOn || isNeed)
                                {
                                    if (_isQuestion)
                                    {
                                        _cmdSpecificIdxList.Add(curIdx);
                                        _cmdSpecific = CmdSpecific.NEED_OF;
                                        _cmdActionIdx = curIdx;
                                        _cmdAction = CmdAction.ASK;
                                    }
                                    else
                                    {
                                        _cmdSpecificIdxList.Add(curIdx);
                                        _cmdSpecific = CmdSpecific.NEED_OF;
                                        _cmdActionIdx = curIdx;
                                        _cmdAction = CmdAction.BUY;
                                    }
                                    if (isOfOn)
                                        _cmdIgnoreIdxList.Add(++curIdx); // For "of" or "on"

                                    // Add 1 to more Descriptive Details of what is needed or being asked about
                                    AddDescDetails(words, ref curIdx);
                                }
                                return true;
                            }
                            break;
                        }

                    case "how":
                        _cmdSpecificIdxList.Add(curIdx);
                        w = gw(words, curIdx + 1);
                        if ((w == "much") || (w == "many") )
                        {
                            _isQuestion = true;
                            _cmdSpecificIdxList.Add(++curIdx);
                            _cmdSpecific = CmdSpecific.AMOUNT_OF;
                            w = gw(words, curIdx + 1);
                            if (w == "is")
                               _cmdIgnoreIdxList.Add(++curIdx);
                            AddDescDetails(words, ref curIdx);
                            return true;
                        }
                        break;

                    case "why":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.REASON_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "is") || (w == "are"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "start":
                    case "stop":
                    case "turn_on":
                    case "turn_off":
                    case "reset":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.EXECUTE;
                        w = gw(words, curIdx + 1);
                        if (w == "the")
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "location":
                    case "place":
                    case "address":
                    case "street":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.LOCATION_OF;
                        w = gw(words, curIdx+1);
                        if ((w == "of") || (w == "for"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "time":
                    case "date":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.DATE_TIME_OF;
                        w = gw(words, curIdx + 1);
                        if (w == "the")
                          _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "cost":
                    case "amount":
                    case "quantity":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.AMOUNT_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "of") || (w == "for"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "phone":
                    case "phone_number":
                    case "number":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.PHONE_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "of") || (w == "for"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "state":
                    case "status":
                    case "current":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.STATE_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "of") || (w == "for"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "name":
                        _cmdSpecificIdxList.Add(curIdx);
                        _cmdSpecific = CmdSpecific.NAME_OF;
                        w = gw(words, curIdx + 1);
                        if ((w == "of") || (w == "for"))
                            _cmdIgnoreIdxList.Add(++curIdx);
                        return true;

                    case "email":
                    case "email_address":
                        w = gw(words, curIdx + 1);
                        if ((w == "of") || (w == "for"))
                        {
                            // EMAIL can be be a form of output so check specifically if "of"
                            _cmdSpecificIdxList.Add(curIdx++);
                            _cmdSpecific = CmdSpecific.EMAIL_OF;
                            _cmdIgnoreIdxList.Add(curIdx++);
                        }
                        return true;

                    default:
                        break;
                }

                if (_cmdSpecific != CmdSpecific.NONE)
                    return true;

                if (startIdx == curIdx)
                    ++curIdx;
            }

            return false;
        }

        private void AddDescDetails(List<string> words, ref int curIdx)
        {
            // Add 1 to 3 Descriptive Details of what is needed or being asked about
            string w = gw(words, curIdx + 1);
            if (!ValidDesc(w))
            {
                _cmdDescList.Add(w);
                _cmdDescIdxList.Add(++curIdx);
                w = gw(words, curIdx + 1);
                if (!ValidDesc(w))
                {
                    _cmdDescList.Add(w);
                    _cmdDescIdxList.Add(++curIdx);
                    w = gw(words, curIdx + 1);
                    if (!ValidDesc(w))
                    {
                        _cmdDescList.Add(w);
                        _cmdDescIdxList.Add(++curIdx);
                    }
                }
            }
        }

        public bool FindCmdSource(List<string> words)
        {
            var NumWords = words.Count;

            for (int i = NumWords - 1; i >= 0; --i)
            {
                if (AlreadyHandledExceptDesc(i)) continue;

                if (GetCmdSrc(words[i], out CmdSource cmdSrc1, out bool? isPlural1))
                {
                    int j = i - 1;
                    if ((j >= 0) && GetCmdSrc(words[j], out CmdSource cmdSrc2, out bool? isPlural2))
                    {
                        if ((int)cmdSrc2 > (int)cmdSrc1)
                        {
                            _cmdSrc = cmdSrc2;
                            _cmdSrcIdx = j;
                            if (isPlural2.HasValue)
                                _isPlural = isPlural2.Value;
                            return true;
                        }
                    }
                    _cmdSrc = cmdSrc1;
                    _cmdSrcIdx = i;
                    if (isPlural1.HasValue)
                        _isPlural = isPlural1.Value;
                    return true;
                }
            }
            return false;
        }

        public bool GetCmdSrc (string raw, out CmdSource cmdSrc, out bool? isPlural)
        {
            cmdSrc = CmdSource.NONE;
            isPlural = null;
            var CmdSrcList = Enum.GetNames(typeof(CmdSource)).Cast<string>().ToList();
            var match = CmdSrcList.Find(c => c.Equals(raw, StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(match))
            {
                cmdSrc = (CmdSource)Enum.Parse(typeof(CmdSource), match);
                return true;
            }

            if (CmdSrcDict.TryGetValue(raw, out Tuple<CmdSource, bool> TCmdSrc))
            {
                cmdSrc = TCmdSrc.Item1;
                isPlural = TCmdSrc.Item2;
                return true;
            }
            return false;
        }

        public bool FindNumber(List<string> words)
        {
            _cmdNumber = -1;

            var NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
                if (words[i] == "two")
                    words[i] = "2";
                else if (words[i] == "three")
                    words[i] = "3";
                else if (words[i] == "four")
                    words[i] = "4";
                else if (words[i] == "five")
                    words[i] = "5";
                else if (words[i] == "six")
                    words[i] = "6";
                else if (words[i] == "seven")
                    words[i] = "7";
                else if (words[i] == "eight")
                    words[i] = "8";
                else if (words[i] == "nine")
                    words[i] = "9";
                else if (words[i] == "ten")
                    words[i] = "10";
                else if (words[i] == "eleven")
                    words[i] = "11";
                else if (words[i] == "twelve")
                    words[i] = "12";
                else if (words[i] == "thirteen")
                    words[i] = "13";
                else if (words[i] == "fourteen")
                    words[i] = "14";
                else if (words[i] == "fifteen")
                    words[i] = "15";
                else if (words[i] == "sixteen")
                    words[i] = "16";
                else if (words[i] == "seventeen")
                    words[i] = "17";
                else if (words[i] == "eighteen")
                    words[i] = "18";
                else if (words[i] == "nineteen")
                    words[i] = "19";
                else if (words[i] == "twenty")
                    words[i] = "20";
                else if (words[i] == "thirty")
                    words[i] = "30";

                if (int.TryParse(words[i], out int num))
                {
                    _cmdNumber = num;
                    _cmdNumberIdx = i;
                    return true;
                }
            }
            return false;
        }

        public bool FindDates(List<string> words)
        {
            DateTime? cmdDateTime = null;
            _cmdDateTimeStartIdx = -1;
            _cmdDateTimeEndIdx = -1;
            _cmdDateRange = null;

            var NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
                if (DateTime.TryParse(words[i], out DateTime dt))
                {
                    if (!cmdDateTime.HasValue)
                    {
                        cmdDateTime = dt;
                        _cmdDateTimeStartIdx = i;
                    }
                    else
                    {
                        _cmdDateRange = new DateRange(cmdDateTime.Value, dt);
                        _cmdDateTimeEndIdx = i;
                        return true;
                    }
                }
            }
            if ((_cmdDateTimeStartIdx != -1) && (_cmdDateTimeEndIdx == -1))
                _cmdDateRange = new DateRange(cmdDateTime.Value, cmdDateTime.Value);

            return cmdDateTime.HasValue;
        }

        public bool FindCmdAction(List<string> words)
        {
            var NumWords = words.Count;
            if (_cmdAction == CmdAction.BUY) // From FindSpecific
            {
                for (int i = 0; i < NumWords; ++i)
                {
                    var w = words[i];
                    if (w.StartsWith("level") || w.StartsWith("measurement") || w.StartsWith("reading") || w.StartsWith("value") ||
                        w.StartsWith("value") || w.StartsWith("pressure") || w.StartsWith("temperature") || w.StartsWith("temp") || w.StartsWith("kitch_temp"))
                    {
                        _cmdSpecificIdxList.Add(i);
                        _cmdSpecific = CmdSpecific.STATE_OF;
                        _cmdAction = CmdAction.FIND;
                        _cmdActionIdx = i;
                        _isPlural = w.EndsWith("s");
                        return true;
                    }
                }
                return true;
            }

            _cmdAction = CmdAction.FIND; // default to FIND
            var CmdActionList = Enum.GetNames(typeof(CmdAction)).Cast<string>().ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                var word = words[i];
                var match = CmdActionList.Find(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));

                if (!string.IsNullOrEmpty(match))
                {
                    _cmdAction = (CmdAction)Enum.Parse(typeof(CmdAction), match);
                    _cmdActionIdx = i;
                    break;
                }

                if (CmdActionDict.TryGetValue(word, out CmdAction cmdAction))
                {
                    _cmdAction = cmdAction;
                    _cmdActionIdx = i;
                    break;
                }
            }

            if ((_cmdSrc == CmdSource.QUESTION) && (_cmdAction == CmdAction.ASK))
            {
                _cmdSrc = CmdSource.URGENCY; // Set Cmd Source when a generic need, needs, needed is requested.
                _cmdSrcIdx = _cmdActionIdx;
            }

            return true;
        }

        public bool FindCmdFormat(List<string> words)
        {
            _cmdFormat = CmdFormat.TEXT; // default to FIND
            var NumWords = words.Count;
            var CmdFormatList = Enum.GetNames(typeof(CmdFormat)).Cast<string>().ToList();
            bool lastTrySheets = false;
            int lastTrySheetsIndex = -1;
            for (int i = 0; i < NumWords; ++i)
            {
                var word = words[i];
                if ((_cmdSrc != CmdSource.INVENTORY) && ((word == "sheet") || (word == "sheets")))
                {
                    lastTrySheets = true;
                    lastTrySheetsIndex = i;
                }

                var match = CmdFormatList.Find(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(match))
                {
                    _cmdFormat = (CmdFormat)Enum.Parse(typeof(CmdFormat), match);
                    _cmdFormatIdx = i;
                    break;
                }

                if (CmdFormatDict.TryGetValue(word, out CmdFormat CmdFormat))
                {
                    _cmdFormat = CmdFormat;
                    _cmdFormatIdx = i;
                    break;
                }
            }
            if ((_cmdFormat == CmdFormat.TEXT) && lastTrySheets)
            {
                _cmdFormat = CmdFormat.EXCEL;
                _cmdFormatIdx = lastTrySheetsIndex;
            }

            return true;
        }

        public bool FindCmdOutput(List<string> words)
        {
            if ((words.Count > 0) && (words[0] == "send"))
                _cmdIgnoreIdxList.Add(0);

            var idx = words.IndexOf("via_email");
            if (idx != -1)
            {
                _cmdOutputIdx = idx;
                _cmdOutput = CmdOutput.EMAIL;
            }
            else if(words?[0] == "email")
            {
                _cmdOutputIdx = 0;
                _cmdOutput = CmdOutput.EMAIL;
            }
            else
            {
                idx = words.IndexOf("email");
                if ((idx != -1) && (_cmdSrc != CmdSource.PERSON) && 
                    (_cmdSrc != CmdSource.TRUSTEE) &&
                    (_cmdSrc != CmdSource.EC) &&
                    (_cmdSrc != CmdSource.CHURCH) &&
                    (_cmdSrc != CmdSource.TRASH) &&
                    (_cmdSrc != CmdSource.INTERNET) &&
                    (_cmdSrc != CmdSource.NYSDOH))
                {
                    _cmdOutputIdx = idx;
                    _cmdOutput = CmdOutput.EMAIL;
                }
            }
            return true;
        }

        public bool FindInsts(List<string> words)
        {
            var NumWords = words.Count;
            _cmdInstsList.Clear();
            _cmdInstsIdxList.Clear();

            var CmdInstsList = Enum.GetNames(typeof(CmdInsts)).Cast<string>().ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                if (AlreadyHandled(i)) continue;

                var word = words[i];
                CmdInsts cmdInsts;
                var match = CmdInstsList.Find(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(match))
                {
                    cmdInsts = (CmdInsts)Enum.Parse(typeof(CmdInsts), match);
                    if (cmdInsts == CmdInsts.MY)
                    {
                        _hasMy = true;
                        _hasMyIdx = i;
                    }
                    else
                    {
                        _cmdInstsList.Add(cmdInsts);
                        _cmdInstsIdxList.Add(i);
                    }
                }

                if (CmdInstsDict.TryGetValue(word, out cmdInsts))
                {
                    if (cmdInsts == CmdInsts.MY)
                    {
                        _hasMy = true;
                        _hasMyIdx = i;
                    }
                    else
                    {
                        _cmdInstsList.Add(cmdInsts);
                        _cmdInstsIdxList.Add(i);
                    }
                }
            }

            if (_cmdInstsList.Any(s => s == CmdInsts.ALL))
                _isPlural = true;

            if (words.Any(w => (w == "done") || (w == "complete") || (w == "completed") || (w == "finished") || (w == "closed")))
                _hasDoneOnly = true;

            return true;
        }

        public bool AlreadyHandled (int i)
        {
            return (i == _cmdActionIdx) || (i == _cmdEventIdx) || (i == _cmdEvent2Idx) || (i == _cmdFormatIdx) || (i == _cmdOutputIdx) || (_cmdInstsIdxList.IndexOf(i) != -1) || (i == _hasMyIdx) || (i == _cmdNumberIdx) ||
                (_cmdIgnoreIdxList.IndexOf(i) != -1) || (_cmdSpecificIdxList.IndexOf(i) != -1) || (_cmdDescIdxList.IndexOf(i) != -1) || (i == _cmdSrcIdx) || ((_cmdDateTimeStartIdx != -1) && (i >= _cmdDateTimeStartIdx) && (i <= _cmdDateTimeEndIdx));
        }

        public bool AlreadyHandledExceptDesc(int i)
        {
            return (i == _cmdActionIdx) || (i == _cmdEventIdx) || (i == _cmdEvent2Idx) || (i == _cmdFormatIdx) || (i == _cmdOutputIdx) || (_cmdInstsIdxList.IndexOf(i) != -1) || (i == _hasMyIdx) || (i == _cmdNumberIdx) ||
                (_cmdIgnoreIdxList.IndexOf(i) != -1) || (_cmdSpecificIdxList.IndexOf(i) != -1) || (i == _cmdSrcIdx) || ((_cmdDateTimeStartIdx != -1) && (i >= _cmdDateTimeStartIdx) && (i <= _cmdDateTimeEndIdx));
        }

        public bool FindEvent(List<string> words, bool forceCheck = false)
        {
            if (!forceCheck)
            {
                // Filter out Sources and actions not associated with Event
                if (((_cmdSrc != CmdSource.QUESTION) && (_cmdSrc != CmdSource.ATTENDANCE) && (_cmdSrc != CmdSource.MENU) && (_cmdSrc != CmdSource.EVENT)) ||
                    (_cmdAction == CmdAction.CALL))
                {
                    return false;
                }
            }

            var NumWords = words.Count;
            var eventList = Enum.GetNames(typeof(EventType)).Cast<string>().Select(e => { return e.Replace('_', ' ').ToLower(); }).Where(s => s != "other").ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                //if (AlreadyHandled(i)) continue;
                if (AlreadyHandledExceptDesc(i)) continue;

                // Make sure word match is a whole Event word
                var eventStrs = eventList.Where(es => (es.StartsWith(words[i]) && words[i].Length >= es.Length) || es.StartsWith(words[i] + " ")).ToList();
                if (eventStrs.Count > 0)
                {
                    string str = eventStrs[0];
                    int eidx = -1;
                    if (eventStrs.Count > 1)
                    {
                        if (words.FindIndex(w => w == "retreat") != -1)
                        {
                            eidx = eventStrs.FindIndex(e => e.IndexOf("retreat") != -1);
                        }
                        if (eidx == -1)
                        {
                            if (words.FindIndex(w => w == "week") != -1)
                            {
                                eidx = eventStrs.FindIndex(e => e.IndexOf("week") != -1);
                            }
                        }
                        if (eidx != -1)
                            str = eventStrs[eidx];
                    }

                    if (Enum.TryParse(str.Replace(' ', '_'), true, out EventType et))
                    {
                        if (_cmdEventIdx == -1)
                        {
                            _cmdEvent = et;
                            _cmdEventIdx = i;

                            if ((eventStrs.Count == 1) || (eidx != -1))
                               return true;

                            if (Enum.TryParse(eventStrs[0].Replace(' ', '_'), true, out EventType et2))
                            {
                                _cmdEvent2 = et2;
                                _cmdEvent2Idx = i;
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool ValidDesc(string w)
        {
            if (string.IsNullOrEmpty(w) || (w == "as") || (w == "with") || (w == "a") || (w == "the") || (w == "in") || (w == "from") || (w == "do") || (w == "we") || (w == "have") || (w == "an") || (w == "i") || (w == "be"))
                return false;
            return true;
        }

        public bool FindDesc(List<string> words)
        {
            var NumWords = words.Count;
            int LastDescIdx = -1;
            for (int i = 0; i < NumWords; ++i)
            {
                if (AlreadyHandled(i)) continue;

                if ((LastDescIdx != -1) && (LastDescIdx != (i - 1)) && (_cmdSrcIdx != (i-1)))
                    break;

                LastDescIdx = i;
                string word = words[i];
                if (!ValidDesc(word))
                    continue;

                _cmdDescList.Add(words[i]);
                _cmdDescIdxList.Add(i);
            }

            if (_cmdSrc == CmdSource.QUESTION)
            {
                int DescCount = _cmdDescList.Count;
                _cmdSrc = (DescCount == 0) ? CmdSource.MENU : (DescCount > 2) ? CmdSource.DOC : CmdSource.PERSON;
            }

            return _cmdDescList.Count > 0;
        }
        private string HandleMenu(List<string> words)
        {
            return "Ask about Inventory, propane, specific measuments, people, camera, Requirements, By-Laws, docs, Events, urgencies, churches, trustees, EC and much more...";
        }
        private string HandleInventory(List<string> words)
        {
            string report; 
            var justAsk = _cmdAction == CmdAction.ASK;
            if (_inventoryStateMatch != null)
            {
                if (_cmdAction == CmdAction.BUY)
                {
                    report = "BUY Inventory Item : " + _inventoryStateMatch.Name + " TBD!!!\n";
                }
                else
                {
                    report = "STATUS Inventory Item : " + _inventoryStateMatch.Name + "[" + _inventoryStateMatch.State + "] current : " + _inventoryStateMatch.InStock + " " + _inventoryStateMatch.Units + " as of " + _inventoryStateMatch.Date.ToString("g") + "\n";
                }
                return report;
            }
            return InventoryItem.GetInventoryReport(_context, _config, false, out string subject, true);
        }
        private string HandleUrgency(List<string> raw)
        {
            var words = new List<string>(raw);

            var justAsk = _cmdAction == CmdAction.ASK;
            var report = InventoryItem.GetInventoryReport(_context, _config, true, out string subject, false);
            int? pid = _hasMy ? _curUser.pid : null;
            subject = subject.Replace("Inventory", "Needed Urgencies");

            // Get Needed Tasks due up tp 15 days in future
            var TaskList = CStat.Models.Task.GetDueTasks(_context, pid, 24 * 15);
            report += "\n";
            foreach (var t in TaskList)
            {
                var DateStr = (t.DueDate.HasValue ? t.DueDate.Value.Date.ToShortDateString() : "???");
                report += "* Task " + t.Id + " " + t.Description + " [" + ((t.Person != null) ? t.Person.GetShortName() : "???") + "] Due:" + DateStr + ".\n";
            }
            report += "\n";
            ArdMgr am = new ArdMgr(_hostEnv, _config, _userManager);
            var ar = am.ReportLastestValues(ref report, true); // Ard only : No propane
            PropaneMgr pm = new PropaneMgr(_hostEnv, _config, _userManager);
            pm.ReportLatestValue(ref report); // propane only

            report += GetEventNeeds(_context);

            return report;
        }
        private string HandlePeople(List<string> words)
        {
            string result = "";
            List<Person> people = null;
            bool showEMail, showAdr, showPhone, showAttr;
            showEMail = showAdr = showPhone = showAttr = true;
            bool EMailOnly, MailingOnly, PhoneOnly;
            EMailOnly = MailingOnly = PhoneOnly = false;
            string sshFileBody = "ContactList";

            string EMailTitle = "Requested Contact Info";

            if (_cmdSrc == CmdSource.TRUSTEE)
            {
                EMailTitle = "CCA Trustees";
                sshFileBody = "CCA_Trustees";
                people = _context.Person.Where(p => p.Roles.HasValue && ((p.Roles.Value & (long)Person.TitleRoles.Trustee) != 0)).Include(p => p.Address).Include(p => p.Church).ToList();
            }
            else if (_cmdSrc == CmdSource.EC)
            {
                sshFileBody = "CCA_Exec_Com";
                //President = 0x800, Treasurer = 0x1000, Secretary = 0x2000, Vice_Pres = 0x4000, Memb_at_Lg = 0x8000
                EMailTitle = "CCA Executive Committee";
                long ECRoles = (long)(Person.TitleRoles.President | Person.TitleRoles.Treasurer | Person.TitleRoles.Secretary | Person.TitleRoles.Vice_Pres | Person.TitleRoles.Memb_at_Lg);
                people = _context.Person.Where(p => p.Roles.HasValue && (p.Roles.Value & ECRoles) != 0).Include(p => p.Address).Include(p => p.Church).ToList();
            }

            if (people == null)
            {
                if (words.IndexOf("cca") != -1)
                    return "Catskill Christian Assembly ; 185 Falke Rd., Prattsville, NY 12468 ; PH# 518-299-3611 ; EMAIL: catskillchristianassembly@gmail.com ; NYS Water ID : 1902270 ; EIN : 22-2148820 ; Physically in Lexington, NY (Greene County)" ;

                if (FindSpecificName(words, out string fName, out string lName, out string name))
                {
                    people = (!string.IsNullOrEmpty(name))
                             ? _context.Person.Where(p => (p.FirstName.StartsWith(name) || (p.LastName.StartsWith(name)))).Include(p => p.Address).Include(p => p.Church).ToList()
                             : (!string.IsNullOrEmpty(fName) && !string.IsNullOrEmpty(lName))
                                ? _context.Person.Where(p => (p.FirstName.StartsWith(fName)) && (p.LastName.StartsWith(lName))).Include(p => p.Address).Include(p => p.Church).ToList()
                                : (!string.IsNullOrEmpty(fName))
                                  ? _context.Person.Where(p => p.FirstName.StartsWith(fName)).Include(p => p.Address).Include(p => p.Church).ToList()
                                  : _context.Person.Where(p => p.LastName.StartsWith(lName)).Include(p => p.Address).Include(p => p.Church).ToList();
                }

                if ((_cmdSrcIdx != -1) || (_cmdSrc == CmdSource.PERSON))
                {
                    string srcDetail = (_cmdSrcIdx != -1) ? words[_cmdSrcIdx] : "unknown";
                    if ((srcDetail == "people_list") || (srcDetail == "contacts") || (srcDetail == "campers") || (srcDetail == "people"))
                    {
                        // Check for more detail
                        if (words.IndexOf("email_address") != -1) srcDetail = "email_address";
                        if (words.IndexOf("phone_number") != -1) srcDetail = "phone_number";
                        if (words.IndexOf("phone_list") != -1) srcDetail = "phone_list";
                        if (words.IndexOf("mailing_list") != -1) srcDetail = "mailing_list";
                        if (words.IndexOf("mailing") != -1) srcDetail = "mailing";
                        if (words.IndexOf("email_list") != -1) srcDetail = "email_list";
                    }

                    switch (srcDetail)
                    {
                        case "email_address":
                            EMailTitle = "Contact EMail Addresses";
                            showAdr = showPhone = showAttr = false;
                            break;
                        case "phone_number":
                            EMailTitle = "Contact Phone #s";
                            showEMail = showAdr = showAttr = false;
                            break;

                        case "phone_list":
                            PhoneOnly = true;
                            EMailTitle = "Contact Phone #s";
                            showEMail = showAdr = showAttr = false;
                            people = _context.Person.Include(p => p.Address).ToList();
                            break;
                        case "people_list":
                        case "contacts":
                        case "campers":
                        case "people":
                            EMailTitle = "Contact Full List";
                            people = _context.Person.Include(p => p.Address).ToList();
                            break;
                        case "mailing_list":
                        case "mailing":
                            MailingOnly = true;
                            EMailTitle = "Contact Mailing List";
                            showEMail = showPhone = showAttr = false;
                            people = _context.Person.Include(p => p.Address).ToList();
                            break;
                        case "email_list":
                            EMailOnly = true;
                            EMailTitle = "Contact EMail Addresses";
                            showAdr = showPhone = showAttr = false;
                            people = _context.Person.Include(p => p.Address).ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            // Last try
            if ((people?.Count ?? 0) == 0)
            {
                if (_cmdDescList.Count == 1)
                {
                    people = _context.Person.Where(p => p.FirstName.StartsWith(_cmdDescList[0])).Include(p => p.Address).ToList();
                    if (people.Count == 0)
                        people = _context.Person.Where(p => p.LastName.StartsWith(_cmdDescList[0])).Include(p => p.Address).ToList();
                }
                else if (_cmdDescList.Count == 2)
                {
                    people = _context.Person.Where(p => (p.FirstName.StartsWith(_cmdDescList[0])) && (p.LastName.StartsWith(_cmdDescList[1]))).Include(p => p.Address).ToList();
                    if ((people?.Count ?? 0) == 0)
                    {
                        string[] otherFNs = Person.FindSimilarNames(_cmdDescList[0]);
                        foreach (var fn in otherFNs)
                        {
                            people = _context.Person.Where(p => (p.FirstName.StartsWith(fn)) && (p.LastName.StartsWith(_cmdDescList[1]))).Include(p => p.Address).ToList();
                            if ((people?.Count ?? 0) > 0)
                                break;
                        }
                    }
                }
            }

            if ((people?.Count ?? 0) > 0)
            {
                if (MailingOnly)
                    people = GetMailingListPeople(people);

                int NumPeople = people.Count;
                var ssType = CSSpreadSheet.GetSSType((int)_cmdFormat);
                if ((NumPeople > 10) || (ssType == CSSpreadSheet.SSType.CSV) || (ssType == CSSpreadSheet.SSType.EXCEL))
                {
                    try
                    {
                        bool isOnly = EMailOnly || PhoneOnly || MailingOnly;
                        string sshFileName = "";
                        using (var ssh = new CSSpreadSheet(sshFileBody, ssType))
                        {
                            sshFileName = ssh.FileName;
                            if (!isOnly)
                            {
                                ssh.SetPad(16, 16, 1, 10, 30, 20, 15, 4, 11, 13, 25, 25);
                                ssh.AddRow("First Name", "Last Name", "Gender", "DOB", "Serve", "Street", "Town", "State", "Zip", "Phone", "EMail", "Church");
                            }
                            else if (MailingOnly)
                            {
                                ssh.SetPad(16, 16, 30, 20, 4, 10);
                                ssh.AddRow("First Name", "Last Name", "Street", "Town", "State", "Zip");
                            }
                            else if (EMailOnly)
                            {
                                ssh.SetPad(16, 16, 25);
                                ssh.AddRow("First Name", "Last Name", "EMail");
                            }
                            else // if (PhoneOnly)
                            {
                                ssh.SetPad(16, 16, 13);
                                ssh.AddRow("First Name", "Last Name", "Phone");
                            }

                            // Handle person.Church is not a valid property for LINQ (get fails)
                            List<PCh> pchs = new List<PCh>();
                            foreach (var p in people)
                            {
                                Church c;
                                c = (p.Church == null) ?
                                      new Church { Name = "", Affiliation = "ICCOC", Id = 0 }
                                    : new Church { Name = p.Church.Name, Affiliation = p.Church.Affiliation, Id = p.Church.Id, MembershipStatus=p.Church.MembershipStatus };
                                pchs.Add(new PCh { person = p, church = c });
                            }
                            var apeople = pchs.OrderBy(p => p.person.LastName).ThenBy(p => p.person.FirstName);

                            foreach (var pc in apeople)
                            {
                                var p = pc.person;
                                var c = pc.church;
                                var fn = p.FirstName;
                                var ln = p.LastName;
                                var a = p?.Address;

                                string cn = (a.Id != 0) ? c.Name : "???";

                                if (MailingOnly)
                                {
                                    if (!String.IsNullOrEmpty(a?.Street))
                                    {
                                        if (fn == "Family")
                                            ssh.AddRow(ln, fn, a.GetStreet(), a.GetTown(), a.GetState(), a.GetZip());
                                        else
                                            ssh.AddRow(fn, ln, a.GetStreet(), a.GetTown(), a.GetState(), a.GetZip());
                                    }
                                    continue;
                                }

                                var eMail = !string.IsNullOrEmpty(p.Email) ? p.Email : "";
                                if (EMailOnly)
                                {
                                    if (!String.IsNullOrEmpty(eMail.Trim()))
                                        ssh.AddRow(fn, ln, eMail);
                                    continue;
                                }

                                var phone = Person.GetAllPhones(p, "");
                                if (PhoneOnly)
                                {
                                    if (!String.IsNullOrEmpty(phone.Trim()))
                                        ssh.AddRow(fn, ln, phone);
                                    continue;
                                }

                                var gender = Person.GetGender(p);
                                var dob = Person.GetDOB(p);
                                var serve = Person.GetRoleStr(p) + Person.GetSkillStr(p);
                                ssh.AddRow(fn, ln, gender, dob, serve, Address.GetStreet(a), Address.GetTown(a), Address.GetState(a), Address.GetZip(a), phone, eMail, c.Name);
                            }
                        }

                        _alreadySentEMail = true;
                        var email = new CSEMail(_config, _userManager);
                            result = email.Send(_curUser.EMail, _curUser.EMail, EMailTitle, "Hi\nAttached, please find " + EMailTitle + ".\nThanks!\nCee Stat", new string[] { sshFileName })
                                ? "E-Mail sent with " + EMailTitle
                                : "Failed to E-Mail : " + EMailTitle;
                    }
                    catch (Exception e)
                    {
                        var cl = new CSLogger();
                        cl.Log("CmdMgr: EMail with " + EMailTitle + " Spreadsheet failed : " + ((e == null) ? "???" : e.Message));
                    }
                }
                else
                {
                    foreach (var p in people.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
                    {
                        if (_cmdSrc == CmdSource.EC)
                        {
                            result += ("*" + p.FirstName + " " + p.LastName + " : " +
                            Person.GetAllPhones(p) + " " +
                            Person.GetEMailStr(p) + " " +
                            Person.GetRoleStr(p) + $"\n\n");
                        }
                        else
                        {
                            result += ((!MailingOnly ? "*" : "") + ((p.FirstName == "Family") ? p.LastName + " " + p.FirstName : p.FirstName + " " + p.LastName) + " : " +
                            (showPhone ? Person.GetAllPhones(p) + " " : "") +
                            (showAdr ? Address.GetAddressStr(p.Address, !MailingOnly) + " " : "") +
                            (showEMail ? Person.GetEMailStr(p) + " " : "") +
                            (showAttr ? Person.GetRoleStr(p) + " " : "") +
                            (showAttr ? Person.GetSkillStr(p) : "") + $"\n" + (!MailingOnly ? "\n" : ""));
                        }
                    }
                }
            }
            return result.Trim();
        }

        private static List<Person> GetMailingListPeople(List<Person> raw)
        {
            string curln = "";
            List<List<Person>> LALists = null;
            List<Person> LAPeople = new List<Person>();

            foreach (var p in raw.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
            {
                var ln = p.LastName.Trim();
                if (string.IsNullOrEmpty(ln)
                    || (p.Address == null)
                    || string.IsNullOrEmpty(p.Address.Street)
                    || p.Address.Street.StartsWith("<missing>", StringComparison.OrdinalIgnoreCase)
                    || ((p.Address.Status & (int)Address.AddressStatus.AdrStat_RTS) != 0)
                    || (p.Status.HasValue && ((p.Status.Value & (int)Person.PersonStatus.Deceased) != 0)) )
                       continue;

                if (!string.IsNullOrEmpty(curln) && !string.Equals(curln,ln, StringComparison.OrdinalIgnoreCase))
                    AddLAPeople(LALists, LAPeople);
                UpdateLAPeople(ref LALists, p);
                curln = ln;
            }
            AddLAPeople(LALists, LAPeople);
            return LAPeople;
        }

        private static void UpdateLAPeople (ref List<List<Person>> laLists, Person p)
        {
            if (laLists == null)
                laLists = new List<List<Person>>();

            foreach (var la in laLists)
            {
                
                if (String.Equals(la[0].LastName, p.LastName, StringComparison.OrdinalIgnoreCase) && Address.SameAddress(la[0].Address, p.Address))
                {
                    la.Add(p);
                    return;
                }
            }
            var nla = new List<Person>();
            nla.Add(p);
            laLists.Add(nla);
        }
        private static int AddLAPeople(List<List<Person>> laLists, List<Person> laPeople)
        {
            int startPpl = laPeople.Count;
            foreach (var la in laLists)
            {
                int totalCount = la.Count;
                var fam = la.Where(p => p.FirstName.StartsWith("Family", StringComparison.OrdinalIgnoreCase)).ToList();
                int famCount = fam.Count;
                if ((famCount >= 1) && (((totalCount - famCount) > 1) || (famCount == totalCount)))
                {
                    // Just output existing family person since there are {no individual person} or {multiple people} at same address 
                    var fp = fam.First();
                    fp.FirstName = "Family";
                    laPeople.Add(fp); // Add LastName Family
                }
                else
                {
                    if ((totalCount - famCount) > 1)
                    {
                        // Just output family since there are multiple people at same address
                        var fp = la.First();
                        fp.FirstName = "Family";
                        laPeople.Add(fp); // Add LastName Family
                    }
                    else
                    {
                        // Just output single person
                        var fp = la.Find(p => !p.FirstName.StartsWith("Family", StringComparison.OrdinalIgnoreCase));
                        laPeople.Add(fp); // Add individual
                    }
                }
            }
            laLists.RemoveAll(p => true);
            return laPeople.Count - startPpl;
        }

        private bool FindSpecificName(List<string> raw, out string fName, out string lName, out string name)
        {
            //{ "name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            //{ "names", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            //{ "last_name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            //{ "first_name", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },

            fName = "";
            lName = "";
            name = "";
            if (_cmdSrcIdx == -1)
                return false;

            bool hasFName = (raw[_cmdSrcIdx] == "first_name");
            bool hasLName = (raw[_cmdSrcIdx] == "last_name");

            int lastWord = raw.Count-1;
            if (hasFName || hasLName)
            {
                // See if fl_name is NOT end
                if (_cmdSrcIdx < lastWord)
                {
                    if (hasFName)
                        fName = raw[lastWord];
                    else
                        lName = raw[lastWord];
                    return true;
                }

                var words = new List<string>(raw);
                int priorWord = _cmdSrcIdx - 1;
                if ((priorWord >= 0) && (words[priorWord] == "a"))
                {
                    words.RemoveAt(priorWord);
                }
                words.RemoveAll(s => s == "as");
                words.RemoveAll(s => s == "with");
                var newCSI = words.IndexOf(raw[_cmdSrcIdx]);
                if (newCSI > 0)
                {
                    if (hasFName)
                        fName = words[newCSI - 1];
                    else
                        lName = words[newCSI - 1];
                    return true;
                }
            }

            if ((raw[_cmdSrcIdx] == "name") || (raw[_cmdSrcIdx] == "names"))
            {
                // See if name is NOT at end
                if (_cmdSrcIdx < lastWord)
                {
                    if ( (_cmdSrcIdx < (lastWord - 1)) && (raw[lastWord-1] != "of") && (raw[lastWord-1] != "with") )
                    {
                        fName = raw[lastWord - 1];
                        lName = raw[lastWord];
                    }
                    else
                        name = raw[lastWord];
                    return true;
                }
            }
            return false;
        }

        private string HandleEquip(List<string> words)
        {
            if (_cmdAction == CmdAction.BUY || _cmdSpecific == CmdSpecific.PHONE_OF || _cmdSpecific == CmdSpecific.LOCATION_OF || _cmdSpecific == CmdSpecific.EMAIL_OF || _cmdSpecific == CmdSpecific.NAME_OF)
                return HandleBiz(words);

            string result = "";
            if (_cmdSrc == CmdSource.PROPANE)
            {
                // Persist Daily Reading and Notify if needed for Propane
                PropaneMgr pmgr = new PropaneMgr(_hostEnv, _config, _userManager);
                var plNow = pmgr.GetTUTank(); // get value, log to file and check
                if (plNow == null)
                    return result;

                if (_cmdDateRange != null)
                {
                    var plList = GetFullPropaneList(pmgr, plNow);
                    var plCnt = plList.Count;
                    int NumOut = 0;
                    for (int i = 0; i < plCnt; ++i)
                    {
                        var pl = plList[i];
                        if (_cmdDateRange.In(pl.ReadingTime))
                        {
                            result = AppendPropLine(pmgr, plList[i], result);
                            if (++NumOut == 31)
                                return result;
                        }
                    }
                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.LAST) || (c == CmdInsts.CURRENT))).Any())
                {
                    if (_cmdNumber < 1)
                        _cmdNumber = 1;
                    if (_cmdNumber == 1)
                        return AppendPropLine(pmgr, plNow, result);

                    var plList = GetFullPropaneList(pmgr, plNow);
                    var plCnt = plList.Count;
                    var NumOut = _cmdNumber > 31 ? 31 : _cmdNumber;
                    for (int i = plCnt - 1; i >= 0; --i)
                    {
                        result = AppendPropLine(pmgr, plList[i], result);
                        if (--NumOut == 0)
                            break;
                    }

                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.PRIOR))).Any())
                {
                    if (_cmdNumber < 1)
                        _cmdNumber = 1;
                    var plList = pmgr.GetAll(_cmdNumber + 1);
                    var plCnt = plList.Count;
                    var NumOut = _cmdNumber;
                    for (int i = plCnt - 1; i >= 0; --i)
                    {
                        if (plList[i].IsSame(plNow))
                            continue;
                        result = AppendPropLine(pmgr, plList[i], result);
                        if (--NumOut == 0)
                            break;
                    }
                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.FIRST))).Any())
                {
                    var plList = GetFullPropaneList(pmgr, plNow);
                    var plCnt = plList.Count;
                    var NumOut = (_cmdNumber > 0) ? ((_cmdNumber > 31) ? 31 : _cmdNumber) : ((plCnt > 31) ? 31 : plCnt);
                    if (NumOut > plCnt)
                        NumOut = plCnt;
                    for (int i = 0; i < plCnt; ++i)
                    {
                        result = AppendPropLine(pmgr, plList[i], result);
                        if (--NumOut == 0)
                            break;
                    }
                }
                else
                {
                    return "As of " + AppendPropLine(pmgr, plNow, result) + "of propane.\n";
                }
            }
            else if (_cmdSrc == CmdSource.ELECTRIC)
            {
                result = "Electric Power Info :\n";
                ArdMgr am = new ArdMgr(_hostEnv, _config, _userManager);
                ArdRecord ar = am.GetLast();
                result += (ar == null) ? "Power Info NOT Available" : ("The Power at CCA is " + ((ar.PowerOn > 70) ? "on" : "off") + " as of " + ar.TimeStampStr() + "\n\n");

                List<Business> bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Electric)).ToList();
                if (bizList.Count > 0)
                {
                    foreach (var b in bizList)
                    {
                        result += b.Name;
                        if (b.Poc != null)
                        {
                           result += " : " + b.Poc.FirstName + " " + b.Poc.LastName;
                        }
                        if ((b.Address != null) && !string.IsNullOrEmpty(b.Address.Phone))
                            result += " : " + Address.FormatAddress(b.Address) + " " + Person.FixPhone(b.Address.Phone) + " ";

                        if (!string.IsNullOrEmpty(b.Terms))
                        {
                            var terms = b.Terms.Trim();
                            if (!string.IsNullOrEmpty(terms))
                                result += " : " + terms;
                        }

                        result += "\n\n";
                    }
                    result = result.Trim();
                    result += "\n";
                }

                var powMgr = new PowerMgr();
                double totKWHrs = 0;
                bool success = false;
                string powStr = "";
                if (_cmdDateRange != null)
                {
                    success = powMgr.TryGetKWHrs(_cmdDateRange.Start, _cmdDateRange.End, out totKWHrs);
                    if (_cmdDateRange.Start.Date != _cmdDateRange.End.Date)
                    {
                        powStr = "Power used from " + _cmdDateRange.Start.Month + "/" + _cmdDateRange.Start.Day + "/" + _cmdDateRange.Start.Year % 100 +
                                 " to " + _cmdDateRange.End.Month + "/" + _cmdDateRange.End.Day + "/" + _cmdDateRange.End.Year % 100 + " = " + totKWHrs + " KWHrs";
                    }
                    else
                        powStr = "Power used on " + _cmdDateRange.Start.Month + "/" + _cmdDateRange.Start.Day + "/" + _cmdDateRange.Start.Year % 100 + " = " + totKWHrs + " KWHrs";
                }
                else
                {
                    DateTime Date = PropMgr.ESTNow;
                    success = powMgr.TryGetKWHrs(Date, Date, out totKWHrs);
                    powStr = "Power used on " + Date.Month + "/" + Date.Day + "/" + Date.Year % 100 + " = " + totKWHrs + " KWHrs";
                }

                result = success ? powStr : "Sorry. Unable to get Total KwHrs now.";
            }
            else
            {
                string PropName = "";
                if (_equipPropMatch != null)
                    PropName = _equipPropMatch.PropName;
                else
                {
                    PropName = _cmdSrc switch
                    {
                        CmdSource.KITCH_TEMP => "kitchTemp",
                        CmdSource.FREEZER => "freezerTemp",
                        CmdSource.FRIDGE => "frigTemp",
                        CmdSource.PRESSURE => "waterPres",
                        CmdSource.PROPANE => "propaneTank",
                        CmdSource.ELECTRIC => "powerOn",
                        _ => ""
                    };
                }

                if (!string.IsNullOrEmpty(PropName))
                {
                    result = "";
                    CSSettings cset = CSSettings.GetCSSettings(_config, _userManager);
                    foreach (var ep in cset.EquipProps)
                    {
                        if (ep.PropName.Equals(PropName, StringComparison.OrdinalIgnoreCase))
                        {
                            if (ep.IsPropane())
                            {
                                var pmgr = new PropaneMgr(_hostEnv, _config, _userManager);
                                if (!pmgr.ReportLatestValue(ref result))
                                    result = "Failed to read value.";
                                break;
                            }
                            else
                            {
                                ArdMgr am = new ArdMgr(_hostEnv, _config, _userManager);
                                result = "CStat:Equip> " + ep.Title + " is " + CSSettings.GetEqValueStr(ep, am.GetLast(), null, false);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    result = "Equipment Report :\n";
                    ArdMgr am = new ArdMgr(_hostEnv, _config, _userManager);
                    var ar = am.ReportLastestValues(ref result, false); // Ard only : No propane

                    PropaneMgr pm = new PropaneMgr(_hostEnv, _config, _userManager);
                    pm.ReportLatestValue(ref result); // propane only

                }
            }

            return (result.Length == 0) ? "Huh?" : result;
        }

        private List<PropaneLevel> GetFullPropaneList (PropaneMgr pmgr, PropaneLevel plNow)
        {
            var plList = pmgr.GetAll();
            if (plNow != null)
            {
                var plCnt = plList.Count;if ((plCnt > 0) && (!plList[plCnt - 1].IsSame(plNow)))
                    plList.Add(plNow);
            }
            return plList;
        }
        string AppendPropLine (PropaneMgr pmgr, PropaneLevel pl, string res)
        {
            if (pl != null)
            {
                if (res.Length > 0)
                   res = res + $"\n";
                res = res + pl.ReadingDateStr(true) + " " + pl.LevelPct.ToString("0.##") + "% " + pmgr.PctToGals(pl.LevelPct).ToString("0.#") + " gals ";
            }
            return res;
        }

        //****************************************************
        private string HandleTasks(List<string> words)
        //****************************************************
        {
            int? pid = _hasMy ? _curUser.pid : null;
            List<Models.Task> tList = null;

            if (this._cmdDescList.Any(d => d == "due"))
                tList = CStat.Models.Task.GetDueTasks(_context, pid, 24 * 15);
            else
                tList = Models.Task.GetAllTasks(_context, _cmdDateRange, pid, _hasDoneOnly);
            int sidx=0, eidx=0;
            if (_cmdDateRange == null)
            {
                if ((_cmdInstsList.FindAll(c => (c == CmdInsts.LAST) || (c == CmdInsts.CURRENT))).Any())
                {
                    if (_cmdNumber < 1)
                        _cmdNumber = _isPlural ? tList.Count : 1;

                    eidx = tList.Count - 1;
                    sidx = eidx - (_cmdNumber - 1);
                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.PRIOR))).Any()) // TBD : Combine LAST and PRIOR
                {
                    if (_cmdNumber < 1)
                        _cmdNumber = _isPlural ? tList.Count : 1;

                    eidx = tList.Count - 1;
                    sidx = eidx - (_cmdNumber - 1);
                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.FIRST))).Any())
                {
                    if (_cmdNumber < 1)
                        _cmdNumber = 1;
                    sidx = 0;
                    eidx = sidx + (_cmdNumber - 1);
                }
                else
                {
                    sidx = 0;
                    eidx = tList.Count - 1;
                }
            }
            else
            {
                sidx = 0;
                eidx = tList.Count - 1;
            }

            sidx = Math.Max(0, Math.Min(tList.Count - 1, sidx));
            eidx = Math.Max(0, Math.Min(tList.Count - 1, eidx));

            // Dump tasks
            string report = "Requested Tasks :\n";
            if (tList.Count > 0)
            {
                for (int i = sidx; i <= eidx; ++i)
                {
                    var t = tList[i];
                    var DateStr = (t.DueDate.HasValue ? t.DueDate.Value.Date.ToShortDateString() : "???");
                    report += "Task " + t.Id + " " + t.Description + " [" + ((t.Person != null) ? t.Person.GetShortName() : "???") + "] Due:" + DateStr + ".\n";
                }
            }
            return report;
        }

        private string HandleEvents(List<string> words)
        {
            int sidx = 0, eidx = 0;
            List<Event> eList = null;
            if (_cmdDateRange == null)
            {
                if (_cmdInstsList.FindAll(c => c == CmdInsts.CURRENT).Any())
                {
                    eList = Event.GetEvents(_context, PropMgr.ESTNow, DateRangeType.On_Date);
                    sidx = 0;
                    eidx = eList.Count;
                }
                else if (_cmdInstsList.FindAll(c => (c == CmdInsts.LAST) || (c == CmdInsts.PRIOR)).Any())
                {
                    eList = Event.GetEvents(_context, PropMgr.ESTNow, DateRangeType.On_or_Before_Date);
                    if (_cmdNumber < 1)
                        _cmdNumber = _isPlural ? eList.Count : 1;
                    eidx = eList.Count-1;
                    sidx = eidx - (_cmdNumber-1);
                }
                else if ((_cmdInstsList.FindAll(c => (c == CmdInsts.FIRST))).Any())
                {
                    eList = Event.GetEvents(_context, new DateTime(1900, 1, 1), DateRangeType.On_or_After_Date); // All
                    if (_cmdNumber < 1)
                        _cmdNumber = _isPlural ? eList.Count : 1;
                    sidx = 0;
                    eidx = sidx + (_cmdNumber - 1);
                }
                else if (_cmdInstsList.FindAll(c => c == CmdInsts.NEXT).Any())
                {
                    eList = Event.GetEvents(_context, PropMgr.ESTNow, DateRangeType.On_or_After_Date);
                    if (_cmdNumber < 1)
                        _cmdNumber = _isPlural ? eList.Count : 1;
                    sidx = 0;
                    eidx = _cmdNumber-1;
                }
                else
                {
                    if (this._cmdNumber >= 1961)
                    {
                        eList = Event.GetEvents(_context, new DateTime(this._cmdNumber, 1, 1), DateRangeType.For_a_Year_From_Date); // For the Year
                        sidx = 0;
                        eidx = eList.Count - 1;
                    }
                    else
                    {
                        if (this._cmdEventIdx != -1)
                        {
                            eList = Event.GetEvents(_context, PropMgr.ESTNow, DateRangeType.On_or_After_Date).Where(e => e.Type == (int)this._cmdEvent || e.Type == (int)this._cmdEvent2).ToList();
                            sidx = 0;
                            eidx = eList.Count - 1;
                        }
                        else if (_cmdInstsList.FindAll(c => c == CmdInsts.ALL).Any())
                        {
                            eList = Event.GetEvents(_context, new DateTime(1900, 1, 1), DateRangeType.On_or_After_Date); // All
                            sidx = 0;
                            eidx = eList.Count - 1;
                        }
                        else if (_cmdInstsList.Count == 0) // Just "Events" -> now and in the future
                        {
                            eList = Event.GetEvents(_context, PropMgr.ESTNow, DateRangeType.On_or_After_Date);
                            if (_cmdNumber < 1)
                                _cmdNumber = _isPlural ? eList.Count : 1;
                            sidx = 0;
                            eidx = _cmdNumber - 1;
                        }
                    }
                }
            }

            // Dump tasks
            string report = "Requested Events :\n";

            if ((eList != null) && (eList.Count > 0))
            {
                sidx = Math.Max(0, Math.Min(eList.Count - 1, sidx));
                eidx = Math.Max(0, Math.Min(eList.Count - 1, eidx));
                for (int i = sidx; i <= eidx; ++i)
                {
                    var e = eList[i];
                    var DateStr = e.StartTime.Date.ToShortDateString() + "@" + e.StartTime.ToShortTimeString() + " to " + e.EndTime.Date.ToShortDateString() + "@" + e.EndTime.ToShortTimeString();
                    report += e.Description + " (" + DateStr + ").\n";
                }
            }
            return report;
        }

        //***************************************************************
        private string HandleBiz(List<string> words)
        //***************************************************************
        {
            List<Business> bizList = new List<Business>();

            if (_cmdSrc == CmdSource.INTERNET)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Internet)).ToList();
            }
            else if (_cmdSrc == CmdSource.PROPANE)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Propane)).ToList();
            }
            else if (_cmdSrc == CmdSource.TRASH)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Refuse)).ToList();
            }
            else if (_cmdSrc == CmdSource.NYSDOH)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.NYS)).ToList();
            }
            else if (_cmdSrc == CmdSource.FOOD)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && ((b.Type.Value == (int)Business.EType.Food) || (b.Type.Value == (int)Business.EType.Kitchen_Supply) || (b.Type.Value == (int)Business.EType.Department))).ToList();
            }
            else if (_cmdSrc == CmdSource.HARDWARE)
            {
                bizList = (words.IndexOf("lumber") != -1)
                    ? _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Hardware)).ToList()
                    : _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && ((b.Type.Value == (int)Business.EType.Hardware) || (b.Type.Value == (int)Business.EType.Department))).ToList();
            }
            else if (_cmdSrc == CmdSource.OFFICE)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && ((b.Type.Value == (int)Business.EType.Office_Supply) || (b.Type.Value == (int)Business.EType.Department))).ToList();
            }
            else if (_cmdSrc == CmdSource.POWER_CO)
            {
                bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Electric)).ToList();
            }
            else
            {
                if (_cmdDescList.Count > 0)
                {
                    // Try to determine from a descriptive word in command
                    var match = _cmdDescList[0];
                    string[] BizTypes = System.Enum.GetNames(typeof(Business.EType)).Select(b => b.Replace("_", " ")).ToArray();
                    int i;
                    for (i = 0; i < BizTypes.Length; ++i)
                    {
                        if (BizTypes[i].StartsWith(match, StringComparison.OrdinalIgnoreCase))
                            break;
                    }
                    if (i == BizTypes.Length)
                        bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Name.ToLower().StartsWith(match)).ToList();
                    else
                        bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == i)).ToList();
                }
                if (bizList.Count == 0)
                {
                    bizList = (words.IndexOf("power") != -1)
                              ? bizList = _context.Business.Include(b => b.Poc).Include(b => b.Address).Where(b => b.Type.HasValue && (b.Type.Value == (int)Business.EType.Electric)).ToList()
                              : _context.Business.Include(b => b.Poc).Include(b => b.Address).ToList();
                }
            }

            string report = "";
            if (bizList.Count == 0)
                report = "Not Found.";
            else
            {
                foreach (var b in bizList)
                {
                    report += b.Name;
                    if (b.Poc != null)
                    {
                        report += " : " + b.Poc.FirstName + " " + b.Poc.LastName;
                    }
                    if ((b.Address != null) && !string.IsNullOrEmpty(b.Address.Phone))
                        report += " : " + Address.FormatAddress(b.Address) + " " + Person.FixPhone(b.Address.Phone) + " ";
                   
                    if (!string.IsNullOrEmpty(b.Terms))
                    {
                        var terms = b.Terms.Trim();
                        if (!string.IsNullOrEmpty(terms))
                            report += " : " + terms;
                    }
                        
                    report += "\n\n";
                }
                return report.Trim();
            }
            report += "\n";
            return report;
        }

        List<string> GetSpecificChurchWords(List<string> raw)
        {
            return raw.Where((r, i) => (r != "of") && (r != "the") && (i != _cmdActionIdx) && (i != _cmdFormatIdx) && (r != "for") && (i != _cmdSrcIdx)).ToList();
        }

        //***************************************************************
        private string HandleChurch(List<string> words) // ZAA
        //***************************************************************
        {
            // TEST CODE 
            //ArdMgr ardMgr = new ArdMgr(_hostEnv, _config, _userManager);
            //var raw1 = "freezerTemp:40,frigTemp:38,kitchTemp:57,waterPres:81,powerOn:368,analog5:333,time:\"10/23/23 8:04:41 PM\"";
            //var raw2 = "freezerTemp:41,frigTemp:39,kitchTemp:57,waterPres:81,powerOn:370,analog5:333,time:\"10 /23/23 8:06:42 PM\"";
            //var raw3 = "freezerTemp:41,frigTemp:38,kitchTemp:57,waterPres:82,powerOn:365,analog5:329,time:\"10 /23/23 8:08:43 PM\"";
            //var raw4 = "freezerTemp:41,frigTemp:39,kitchTemp:57,waterPres:82,powerOn:365,analog5:327,time:\"10/23/23 8:10:45 PM\"";
            //var raw5 = "freezerTemp:41,frigTemp:39,kitchTemp:57,waterPres:82,powerOn:365,analog5:327,time:\"10/23/23 8:10:45 PM\"";
            //var raw6 = "freezerTemp:41,frigTemp:39,kitchTemp:57,waterPres:82,powerOn:365,analog5:327,time:\"10/23/23 8:10:45 PM\"";
            //ardMgr.CheckValues(raw1);
            //ardMgr.CheckValues(raw2);
            //ardMgr.CheckValues(raw3);
            //ardMgr.CheckValues(raw4);
            //ardMgr.CheckValues(raw5);
            //ardMgr.CheckValues(raw6);
            //var raw2 = "[{\"name\":\"Annas-iPhone-2\",\"alias\":\"\",\"mac\":\"74:42:18:86:5A:AF\",\"ip\":\"192.168.1.128\",\"rssi\":-83,\"rx\":12,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:01:05\",\"deviceType\":4,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:04:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"02:0F:B5:27:03:0B\",\"ip\":\"192.168.1.129\",\"rssi\":-80,\"rx\":6,\"tx\":6,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:05:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:00:09.9309938-04:00\"},{\"name\":\"CCASERVER\",\"alias\":\"\",\"mac\":\"98:E7:F4:BB:18:05\",\"ip\":\"192.168.1.15\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":34,\"isConnected\":true,\"vendor\":\"Hewlett Packard\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Shenzhen Baichuan Digital Techn\",\"alias\":\"Camera2\",\"mac\":\"EC:71:DB:22:89:5B\",\"ip\":\"192.168.1.224\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Shenzhen Baichuan Digital Technology Co., Ltd.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Arduino\",\"alias\":\"\",\"mac\":\"A8:40:41:12:4B:78\",\"ip\":\"192.168.1.39\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Dragino Technology Co., Limited\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"EYEDRO-004809AF\",\"alias\":\"\",\"mac\":\"60:54:64:48:09:AF\",\"ip\":\"192.168.1.153\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Eyedro Green Solutions Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Shenzhen Baichuan Digital Techn\",\"alias\":\"Camera1\",\"mac\":\"EC:71:DB:66:6F:41\",\"ip\":\"192.168.1.237\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Shenzhen Baichuan Digital Technology Co., Ltd.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"3A:33:FF:4E:E8:20\",\"ip\":\"192.168.101.248\",\"rssi\":-83,\"rx\":24,\"tx\":58.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"04:58:29\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T18:07:07.9309938-04:00\"},{\"name\":\"Laptop2023\",\"alias\":\"\",\"mac\":\"B4:8C:9D:17:B9:61\",\"ip\":\"192.168.101.180\",\"rssi\":-81,\"rx\":1,\"tx\":14.4,\"dRx\":0,\"dTx\":0,\"connTime\":\"01:47:32\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Microsoft Corp.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:18:10.9309938-04:00\"},{\"name\":\"NPIE8E01C\",\"alias\":\"NPIE8E01C\",\"mac\":\"6C:0B:5E:E8:E0:1C\",\"ip\":\"192.168.1.137\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"EX7000\",\"alias\":\"EX7000\",\"mac\":\"02:0F:B5:FB:CE:6E\",\"ip\":\"192.168.1.250\",\"rssi\":-83,\"rx\":526.5,\"tx\":520,\"dRx\":0,\"dTx\":0,\"connTime\":\"48:58:26\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-27T22:06:23.9309938-04:00\"},{\"name\":\"Betty-s-Galaxy-S10\",\"alias\":\"\",\"mac\":\"26:B2:24:2D:40:C5\",\"ip\":\"192.168.101.221\",\"rssi\":-90,\"rx\":1,\"tx\":43.299999999999997,\"dRx\":0,\"dTx\":0,\"connTime\":\"14:03:31\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T09:01:57.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"FA:4E:23:26:FD:E8\",\"ip\":\"192.168.101.51\",\"rssi\":-76,\"rx\":24,\"tx\":104,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:29:28\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:36:11.9309938-04:00\"},{\"name\":\"ABruc1195\",\"alias\":\"\",\"mac\":\"10:68:38:37:DE:B3\",\"ip\":\"192.168.101.73\",\"rssi\":-75,\"rx\":1,\"tx\":144.40000000000001,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:40:30\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Microsoft Corp.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:25:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"1A:44:8E:70:EA:54\",\"ip\":\"192.168.1.130\",\"rssi\":-90,\"rx\":24,\"tx\":86.700000000000003,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"52:80:A0:51:C1:F3\",\"ip\":\"192.168.101.234\",\"rssi\":-94,\"rx\":1,\"tx\":28.899999999999999,\"dRx\":0,\"dTx\":0,\"connTime\":\"13:35:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T09:29:58.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"CE:05:4A:EE:EF:8C\",\"ip\":\"192.168.101.142\",\"rssi\":-73,\"rx\":24,\"tx\":115.59999999999999,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:19:40\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:46:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"DE:BF:80:F1:1A:DA\",\"ip\":\"192.168.101.181\",\"rssi\":-86,\"rx\":11,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:03\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"32:09:18:19:FD:71\",\"ip\":\"192.168.101.104\",\"rssi\":-65,\"rx\":24,\"tx\":57.799999999999997,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:17:15\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:48:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"D6:E7:E5:AD:F1:C6\",\"ip\":\"192.168.101.116\",\"rssi\":-60,\"rx\":24,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:27:57\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:38:11.9309938-04:00\"},{\"name\":\"iPad\",\"alias\":\"\",\"mac\":\"56:2C:70:33:0E:C6\",\"ip\":\"192.168.101.10\",\"rssi\":-89,\"rx\":1,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:25:22\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:40:09.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"14:1A:97:82:B1:B7\",\"ip\":\"192.168.101.175\",\"rssi\":-83,\"rx\":1,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:01\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"1E:F1:B6:92:5F:F7\",\"ip\":\"192.168.101.176\",\"rssi\":-87,\"rx\":24,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:36:45\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:29:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"82:D4:28:04:E1:C5\",\"ip\":\"192.168.101.146\",\"rssi\":-41,\"rx\":24,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:31:33\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:34:09.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"36:E0:31:81:E8:B4\",\"ip\":\"192.168.101.132\",\"rssi\":-84,\"rx\":24,\"tx\":117,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:30:39\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:35:09.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"72:FB:73:61:89:E5\",\"ip\":\"192.168.101.192\",\"rssi\":-94,\"rx\":1,\"tx\":72.200000000000003,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:07:57\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:58:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"BE:C6:E0:59:99:54\",\"ip\":\"192.168.101.62\",\"rssi\":-88,\"rx\":24,\"tx\":26,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:13:37\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:52:11.9309938-04:00\"},{\"name\":\"Intel Corporate\",\"alias\":\"\",\"mac\":\"E0:D0:45:F1:4F:E1\",\"ip\":\"192.168.101.235\",\"rssi\":-71,\"rx\":1,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:00\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Intel Corporate\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"CE:0B:45:3C:D5:A5\",\"ip\":\"192.168.101.147\",\"rssi\":-94,\"rx\":1,\"tx\":19.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"01:05:45\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:00:10.9309938-04:00\"},{\"name\":\"espressif\",\"alias\":\"espressif\",\"mac\":\"44:67:55:27:03:0B\",\"ip\":\"192.168.1.71\",\"rssi\":-23,\"rx\":72.200000000000003,\"tx\":43.299999999999997,\"dRx\":66.200000000000003,\"dTx\":0,\"connTime\":\"46:57:09\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Orbit Irrigation\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-28T00:07:25.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"9E:57:BC:F5:D5:82\",\"ip\":\"192.168.101.219\",\"rssi\":-93,\"rx\":1,\"tx\":26,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:04:44\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:01:09.9309938-04:00\"},{\"name\":\"Watch\",\"alias\":\"\",\"mac\":\"C6:18:07:32:0E:7F\",\"ip\":\"192.168.101.161\",\"rssi\":-91,\"rx\":6,\"tx\":58.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"11:55:52\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T11:10:00.9309938-04:00\"},{\"name\":\"ESP_89D901\",\"alias\":\"\",\"mac\":\"E0:98:06:89:D9:01\",\"ip\":\"192.168.1.148\",\"rssi\":-78,\"rx\":2,\"tx\":65,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:04\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Espressif Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"DE:D5:27:F3:28:C2\",\"ip\":\"192.168.101.41\",\"rssi\":-94,\"rx\":1,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:55:43\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:10:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"AA:55:36:EE:B5:A4\",\"ip\":\"192.168.1.140\",\"rssi\":-75,\"rx\":11,\"tx\":65,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:03:58\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:02:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"74:71:8B:9C:3E:5B\",\"ip\":\"192.168.101.105\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Watch\",\"alias\":\"\",\"mac\":\"7E:34:3F:CE:ED:73\",\"ip\":\"192.168.101.106\",\"rssi\":-95,\"rx\":1,\"tx\":5.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:12:01\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:53:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"56:76:EC:DD:78:AE\",\"ip\":\"192.168.1.136\",\"rssi\":-78,\"rx\":6.5,\"tx\":1170,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:01\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"Pixel-8\",\"alias\":\"\",\"mac\":\"6A:C1:09:73:7B:FC\",\"ip\":\"192.168.1.82\",\"rssi\":-94,\"rx\":1,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:02:40\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:03:11.9309938-04:00\"},{\"name\":\"Pixel-8\",\"alias\":\"\",\"mac\":\"46:85:9F:46:70:DE\",\"ip\":\"192.168.101.166\",\"rssi\":-91,\"rx\":1,\"tx\":156,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:04\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"}]";
            //if (raw2.Length > 100)
            //{
            //    ClientReport[] clients = ClientReport.GetClientsFromJson(raw2);
            //    ClientReport.SetClients(clients);
            //    RConnMgr rcmgr = new RConnMgr(_hostEnv, _context);
            //    rcmgr.Update(clients);
            //    rcmgr.Close(out string newClient);
            //    return "";
            //}

            string report = "";

            List<Church> churches;

            if (!_isPlural)
            {
                churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => c.Affiliation == "ICCOC").OrderBy(c => c.Name).ToList();

                Church hitCh = null;
                var specWords = GetSpecificChurchWords(words);
                int maxHitCount = 0;
                foreach (var ch in churches)
                {
                    int hitCount = 0;
                    foreach (var sw in specWords)
                    {
                        if (ch.Name.ToLower().Replace("church","").Trim().IndexOf(sw.ToLower()) != -1)
                        {
                            hitCount += sw.Length;
                        }
                    }
                    if (hitCount > maxHitCount)
                    {
                        maxHitCount = hitCount;
                        hitCh = ch;
                    }
                }

                if (hitCh == null)
                {
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).OrderBy(c => c.Name).ToList();
                    int MinDist = 10000000;

                    var fullSpec = "";
                    foreach (var sw in specWords)
                    {
                        fullSpec = sw + " ";
                    }
                    fullSpec = fullSpec.ToLower().Replace(" christian", "").Replace(" christ", "").Replace("christ", "").Trim();

                    // try closest match of church name to fullSpec via Levenshtein Distance
                    foreach (var ch in churches)
                    { 
                        var chName = ch.Name.ToLower().Replace(" christian", "").Replace("church of church", "").Replace("church", "").Replace("of christ", "").Replace("christ", "").Trim();
                        int dist = LevenshteinDistance.Compute(fullSpec, chName);
                        if (dist < MinDist)
                        {
                            MinDist = dist;
                            hitCh = ch;
                        }
                    }
                }

                if (hitCh == null)
                    return "Can you be more specific?";

                var poc = Person.GetPOCMinister(hitCh);
                var status = Church.ChurchLists.MemberStatList[(int)hitCh.MembershipStatus].name;

                report += "CHURCH: " + hitCh.Name + " " +
                    (Address.FormatAddress(hitCh.Address) + " " +
                    (!string.IsNullOrEmpty(status) ? " " + status : "") +
                    (!string.IsNullOrEmpty(poc) ? " " + poc : "") +
                    (!string.IsNullOrEmpty(hitCh.Address?.Phone) ? " " + Person.FixPhone(hitCh.Address?.Phone) : " unknown #") +
                    (!string.IsNullOrEmpty(hitCh.Email) ? " " + hitCh.Email : "")).Trim() + "\n";
            }
            else
            {
                bool IsMemb = false;
                string filter = "";
                if (words.Contains("metro"))
                {
                    filter = "Metro";
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => c.StatusDetails.Contains("@Metro") || c.StatusDetails.Contains("@metro") || c.StatusDetails.Contains("@METRO")).ToList();
                }
                else if (words.Contains("independent") || words.Contains("christian"))
                {
                    filter = "IC/COC";
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => c.Affiliation == "ICCOC").OrderBy(c => c.Name).ToList();
                }
                else if (words.Contains("good") || words.Any(w => w.StartsWith("good")))
                {
                    filter = "Good Standing";
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => c.MembershipStatus == 1).OrderBy(c => c.Name).ToList();
                }
                else if (words.Contains("member"))
                {
                    filter = "Cur/Prior Members";
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => (c.MembershipStatus == 1) || (c.MembershipStatus == 2)).OrderBy(c => c.MembershipStatus).ThenBy(c => c.Name).ToList();
                }
                else
                {
                    filter = "IC/COC";
                    churches = _context.Church.Include(c => c.SeniorMinister).Include(c => c.YouthMinister).Include(c => c.Address).Where(c => c.Affiliation == "ICCOC").OrderBy(c => c.Name).ToList();
                }

                foreach (var c in churches)
                {
                    var poc = Person.GetPOCMinister(c);
                    var status = Church.ChurchLists.MemberStatList[(int)c.MembershipStatus].name;
                    report += "CHURCH: " + c.Name + " " +
                        (Address.FormatAddress(c.Address) + " " +
                        (!string.IsNullOrEmpty(status) ? " " + status : "") +
                        (!string.IsNullOrEmpty(poc) ? " " + poc : "") +
                        (!string.IsNullOrEmpty(c.Address?.Phone) ? " " + Person.FixPhone(c.Address?.Phone) : " unknown #") +
                        (!string.IsNullOrEmpty(c.Email) ? " " + c.Email : "")).Trim() + "\n";
                }

                // EMail a CSV File
                if (CSSpreadSheet.fLock.TryEnterWriteLock(3000))
                {
                    try
                    {
                        var TempPath = Path.GetTempPath();
                        var FullFile = Path.Combine(TempPath, "Churches.csv");
                        using (StreamWriter wFile = new StreamWriter(FullFile))
                        {
                            wFile.WriteLine("Church Name,Address,Member Status,Contact,Phone,EMail,Website");
                            foreach (var c in churches)
                            {
                                var poc = EncloseCommasForCSV(Person.GetPOCMinister(c));
                                var status = EncloseCommasForCSV(Church.ChurchLists.MemberStatList[(int)c.MembershipStatus].name);
                                string website = "";
                                if (!string.IsNullOrEmpty(c.StatusDetails))
                                {
                                    int windex = c.StatusDetails.IndexOf("@website=\"");
                                    if (windex > -1)
                                    {
                                        string wstr = c.StatusDetails.Substring(windex + 10);
                                        int eindex = wstr.IndexOf("\"");
                                        if (eindex != -1)
                                            website = EncloseCommasForCSV(wstr.Substring(0, eindex));
                                    }
                                }
                                wFile.WriteLine(EncloseCommasForCSV(c.Name) + "," +
                                       EncloseCommasForCSV(Address.FormatAddress(c.Address)) + "," +
                                       (!string.IsNullOrEmpty(status) ? status : "") + "," +
                                       (!string.IsNullOrEmpty(poc) ? poc : "") + "," +
                                       (!string.IsNullOrEmpty(c.Address?.Phone) ? " " + EncloseCommasForCSV(Person.FixPhone(c.Address?.Phone)) : "") + "," +
                                       (!string.IsNullOrEmpty(c.Email) ? EncloseCommasForCSV(c.Email) : "") + "," +
                                       website);
                            }
                            wFile.Close();
                            var email = new CSEMail(_config, _userManager);
                            _alreadySentEMail = true;
                            report = "EMail " + (email.Send(_curUser.EMail, _curUser.EMail, "CCA Churches Spreadsheet", "Hi\nAttached, please find a spreadsheet of CCA Churches based on " + filter + ".\nThanks!\nCee Stat", new string[] { FullFile })
                            ? "successfully sent to " : "FAILED to be sent to ") + _curUser.EMail;
                        }
                    }
                    catch (Exception e)
                    {
                        var cl = new CSLogger();
                        cl.Log("CmdMgr: EMail Churches CSV failed : " + ((e == null) ? "???" : e.Message));
                    }
                    finally
                    {
                        CSSpreadSheet.fLock.ExitWriteLock();
                    }
                }
                else
                {
                    report += " FAILED TO EMAIL EXCEL OF CHURCHES DUE TO LOCK.";
                }
            }
            return report;

            //*** OLDER CODE RELATED TO PEOPLE
            //var people = new List<Person>();
            //if (_isPlural)
            //{
            //    if (_cmdSrc == CmdSource.CHURCH)
            //    {
            //        people = _context.Person.Where(p => p.Roles.HasValue && ((p.Roles.Value & (long)Person.TitleRoles.Trustee) != 0)).Include(p => p.Church).ToList();
            //    }
            //}
            //else
            //{
            //    if (_cmdDescList.Count == 1)
            //    {
            //        people = _context.Person.Where(p => p.FirstName.StartsWith(_cmdDescList[0])).Include(p => p.Church).ToList();
            //    }
            //    else if (_cmdDescList.Count == 2)
            //    {
            //        people = _context.Person.Where(p => (p.FirstName.StartsWith(_cmdDescList[0])) && (p.LastName.StartsWith(_cmdDescList[1]))).Include(p => p.Church).ToList();
            //    }
            //    else
            //        return "Unable to determine a specific church";
            //}

            //foreach (var p in people)
            //{
            //    if (p.Church != null)
            //        churches.Add(p.Church);
            //}
            //var DistChurches = churches.Distinct<Church>().OrderBy(c => c.Name).ToList();

            //string report = "";
            //foreach (var c in DistChurches)
            //{
            //    report += "CHURCH: " + c.Name + Address.FormatAddress(c.Address, true) + ".\n";
            //}
            //return report;
        }

        private static string EncloseCommasForCSV(string instr)
        {
            if (!String.IsNullOrEmpty(instr) && instr.Contains(","))
            {
                return "\"" + instr + "\"";
            }
            return instr;
        }

        private string HandleCamera(List<string> words) // ZZZ
        {
            CamOps camOps = null;
            COp preset = COp.None;
            int maxMatchLen = 3;
            int index;

            string wordStr = "";
            foreach (var w in words)
            {
                wordStr += (w + " ");
            }
            wordStr = wordStr.Trim();

            if (wordStr.Contains("camera 1") || wordStr.Contains("camera one)"))
                camOps = Cam._CamOps1;
            else if (wordStr.Contains("camera 2") || wordStr.Contains("camera two)"))
                camOps = Cam._CamOps2;
            else if (wordStr.Contains("cam 1") || wordStr.Contains("cam one)"))
                camOps = Cam._CamOps1;
            else if (wordStr.Contains("cam 2") || wordStr.Contains("cam two)"))
                camOps = Cam._CamOps2;

            int foundCam = (camOps != null) ? (int)camOps._cam : 0;

            foreach (var desc in _cmdDescList)
            {
                if ((desc == "of") || (desc == "of"))
                    continue;

                int len = desc.Length;

                if ((foundCam == 1) || (foundCam == 0))
                {
                    index = Array.FindIndex(Cam._presets1, p => p.IndexOf(desc, StringComparison.InvariantCultureIgnoreCase) != -1);
                    if ((index != -1) && (len > maxMatchLen))
                    {
                        maxMatchLen = len;
                        if (foundCam == 0)
                            camOps = Cam._CamOps1;
                        preset = (COp)index;
                    }
                }

                if ((foundCam == 2) || (foundCam == 0))
                {
                    index = Array.FindIndex(Cam._presets2, p => p.IndexOf(desc, StringComparison.InvariantCultureIgnoreCase) != -1);
                    if ((index != -1) && (len > maxMatchLen))
                    {
                        maxMatchLen = len;
                        if (foundCam == 0)
                            camOps = Cam._CamOps2;
                        preset = (COp)index;
                    }
                }
            }

            if (preset == COp.None)
            {
                if (wordStr.Contains("preset 1)") || wordStr.Contains("preset1)"))
                    preset = COp.Preset1;
                else if (wordStr.Contains("preset one)"))
                    preset = COp.Preset1;
                else if (wordStr.Contains("preset 2)") || wordStr.Contains("preset2"))
                    preset = COp.Preset2;
                else if (wordStr.Contains("preset two)"))
                    preset = COp.Preset2;
                else if (wordStr.Contains("preset 3") || wordStr.Contains("preset3"))
                    preset = COp.Preset3;
                else if (wordStr.Contains("preset three)"))
                    preset = COp.Preset3;
                else if (wordStr.Contains("preset 4") || wordStr.Contains("preset4"))
                    preset = COp.Preset4;
                else if (wordStr.Contains("preset four)"))
                    preset = COp.Preset4;
                else if (wordStr.Contains("preset 5") || wordStr.Contains("preset5"))
                    preset = COp.Preset5;
                else if (wordStr.Contains("preset five)"))
                    preset = COp.Preset5;
                else if (wordStr.Contains("preset 6") || wordStr.Contains("preset6"))
                    preset = COp.Preset6;
                else if (wordStr.Contains("preset six)"))
                    preset = COp.Preset6;

                else if (wordStr.Contains("preset 7") || wordStr.Contains("preset7"))
                    preset = COp.Preset7;
                else if (wordStr.Contains("preset seven)"))
                    preset = COp.Preset7;

                else if (wordStr.Contains("preset 8") || wordStr.Contains("preset8"))
                    preset = COp.Preset8;
                else if (wordStr.Contains("preset eight)"))
                    preset = COp.Preset8;

                else if (wordStr.Contains("preset 9") || wordStr.Contains("preset9"))
                    preset = COp.Preset9;
                else if (wordStr.Contains("preset nine"))
                    preset = COp.Preset9;
            }
            if (camOps == null)
                camOps = Cam._CamOps1;

            Thread.Sleep(camOps.HandleOp(_hostEnv, COp.LightOn));

            if (preset != COp.None)
            {
                Thread.Sleep(camOps.HandleOp(_hostEnv, preset + camOps.PresetOffsetFrom1()));
                for (int i = 0; i < 8; ++i)
                {
                    camOps.SnapShot(_hostEnv, false, "&width=3840&height=2160");
                    Thread.Sleep(1000);
                }
            }

            _alreadySentEMail = true;
            string sshFile = camOps.SnapShotFile("", _hostEnv, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution
            string EMailTitle = "CCA Camera Photo : " + preset;
            var email = new CSEMail(_config, _userManager);
            string result = email.Send(_curUser.EMail, _curUser.EMail, EMailTitle, "Hi\nAttached, please find " + EMailTitle + ".\nThanks!\nCee Stat", new string[] { sshFile })
                ? "E-Mail sent with " + EMailTitle
                : "Failed to E-Mail : " + EMailTitle;

            Thread.Sleep(camOps.HandleOp(_hostEnv, COp.LightOff));

            return result; 
        }

        private string HandleAttendance(List<string> words)
        {
            return "Not Implemented";

        }

        public static void RenderFolder (CSDropBox dbox, string[] hints, string fld, string indent, string lastFolder, ref string report)
        {
            // TBD Handle Hint filtering

            ListFolderResult FldList = dbox.GetFolderList2(fld);

            bool NoHints = (hints == null) || (hints.Length == 0);
            if (FldList.Entries.Count > 0)
            {
                foreach (var entry in FldList.Entries)
                {
                    if (entry.IsFolder)
                    {
                        var lcName = entry.Name.ToLower();
                        if ((lcName == "archive") || (lcName == "_archive") || (lcName == "reference") || (lcName == "vault") || (lcName == "memorandum") || (lcName == "todo"))
                            continue;

                        if (NoHints)
                            report += (indent + entry.Name + "\n");

                        RenderFolder(dbox, hints, fld + "/" + entry.Name, indent + "    ", lastFolder, ref report);
                    }
                    else
                    {
                        if (NoHints)
                            report += indent + entry.Name + "\n";
                        else
                        {
                            bool hasAllHints = false;
                            foreach (var hint in hints)
                            {
                                var entryName = entry.Name.Replace("'s", "");
                                if (!entryName.Contains(hint, StringComparison.OrdinalIgnoreCase))
                                {
                                    hasAllHints = false;
                                    break;
                                }
                                else
                                    hasAllHints = true;
                            }
                            if (hasAllHints)
                                report += "[ " + GetFileNameHash(entry.PathDisplay).ToString("X8") + " ]" + entry.PathDisplay + "\n";
                        }
                    }
                }
            }
        }

        public static UInt32 GetFileNameHash (string fn)
        {
            char[] farr = fn.ToCharArray(0, fn.Length);
            UInt32 hash = Seed;
            foreach (var c in farr)
            {
                hash = ((UInt32)c ^ hash) * Prime;
            }
            return hash;
        }

        private string HandleDocs(List<string> words)
        {
            if (_cmdSrc == CmdSource.DOC)
            {
                if (_cmdDescList.Count > 0)
                {
                    var dbox = GetDropBox();
                    string report = "";
                    RenderFolder(dbox, ModifySOPDesc(), "/Manuals & Procedures", "", "", ref report);
                    var startIdx = report.IndexOf("]");
                    if (startIdx != -1)
                    {
                        var endIdx = report.IndexOf("\n");
                        if (endIdx != -1)
                        {
                            _alreadySentEMail = true;
                            var fname = report.Substring(startIdx + 1, (endIdx - startIdx) - 1);
                            report = EMailDropBoxFile(_config, _userManager, _curUser, fname);
                        }
                    }
                    else
                    {
                        RenderFolder(dbox, GetFilteredDesc(), "/Manuals & Procedures", "", "", ref report);
                        startIdx = report.IndexOf("]");
                        if (startIdx != -1)
                        {
                            var endIdx = report.IndexOf("\n");
                            if (endIdx != -1)
                            {
                                _alreadySentEMail = true;
                                var fname = report.Substring(startIdx + 1, (endIdx - startIdx) - 1);
                                report = EMailDropBoxFile(_config, _userManager, _curUser, fname);
                            }
                        }
                        else
                        {
                            RenderFolder(dbox, this.SingularizeDesc(), "/Manuals & Procedures", "", "", ref report);
                            startIdx = report.IndexOf("]");
                            if (startIdx != -1)
                            {
                                var endIdx = report.IndexOf("\n");
                                if (endIdx != -1)
                                {
                                    _alreadySentEMail = true;
                                    var fname = report.Substring(startIdx + 1, (endIdx - startIdx) - 1);
                                    report = EMailDropBoxFile(_config, _userManager, _curUser, fname);
                                }
                            }
                            else
                                return "No Matching Doc found. Try a different description.";
                        }
                    }
                    return report;
                }
                return "Not performed.";
            }
            else if (_cmdSrc == CmdSource.REQ)
            {
                var ReqLink = "https://ccaserve.org/Required.html";
                var SrcPath = Path.Combine(Utils.GetTempDir(_hostEnv, @"docs"), "Required2021.pdf");
                if (System.IO.File.Exists(SrcPath))
                {
                    _alreadySentEMail = true;
                    var email = new CSEMail(_config, _userManager);
                    if (!email.Send(_curUser.EMail, _curUser.EMail, "RE: Request For CCA Required by Law", "Hi\nAttached, please find the CCA Required by Law document.\n\nThanks!\nCee Stat", new string[] { SrcPath }))
                        return ReqLink + "\nERROR : Failed to EMail Required by Law";
                }
                else
                    return ReqLink + "\nERROR : Cannot find Required by Law";
                return ReqLink + "\nCCA Required by Law also sent to your EMail.";
            }
            else if (_cmdSrc == CmdSource.BYLAWS)
            {
                var ByLawsLink = "https://ccaserve.org/CCAByLaws2021.html";

                var dbox = GetDropBox();
                if (dbox == null)
                {
                    return ByLawsLink + "\nERROR : DropBox NOT Accessible.";
                }
                string SrcPath = "/Corporate/By-Laws, IDs & Docs";
                string FileName = "CCA By-Laws 2021.docx";
                string FullSrcPath = SrcPath + "/" + FileName;
                string FullDestPath = Path.Combine(Path.GetTempPath(), FileName);
                if (System.IO.File.Exists(FullDestPath))
                    System.IO.File.Delete(FullDestPath);

                try
                {
                    if (dbox.FileExists(FullSrcPath))
                    {
                        var task = dbox.DownloadToFile(SrcPath, FileName, FullDestPath);
                        task.Wait();
                        if (System.IO.File.Exists(FullDestPath))
                        {
                            _alreadySentEMail = true;
                            var email = new CSEMail(_config, _userManager);
                            if (!email.Send(_curUser.EMail, _curUser.EMail, "RE: Request For By-Laws", "Hi\nAttached, please find the CCA By-Laws\n\nThanks!\nCee Stat", new string[] { FullDestPath }))
                                return ByLawsLink + "\nERROR : Failed to EMail By-Laws";
                        }
                        else
                            return ByLawsLink + "\nERROR : Failed to Download By-Laws";
                        return ByLawsLink + "\nCCA By-Laws also sent to your EMail.";
                    }
                    else
                        return ByLawsLink + "\nERROR : DropBox file : " + FullSrcPath + " NOT FOUND!";
                }
                catch (Exception e)
                {
                    return ByLawsLink + "\nERROR : " + e.Message;
                }
            }
            return "Document sent to your EMail.";
        }


        private string HandleMMs(List<string> words)
        {
            if (_cmdSrc == CmdSource.MMS)
            {
                var mmMgr = new MeetingMinsMgr();
                mmMgr.ReadMins();
                string report = "";

                if (_cmdDateTimeStartIdx != -1)
                {
                    _alreadySentEMail = true;
                    List<MeetingMins> mms = mmMgr._List.Where(mm => (mm._dt.Date >= _cmdDateRange.Start.Date) && (mm._dt.Date <= _cmdDateRange.End.Date)).OrderBy(m => m._dt).ToList();
                    report = "Requested Meeting Minutes for " + _cmdDateRange.Start.ToShortDateString() + " : ";
                    if (mms.Count > 0)
                    {
                        foreach (MeetingMins m in mms)
                        {
                            report += EMailDropBoxFile(_config, _userManager, _curUser, m._fullPath) + "\n";
                        }
                    }
                    else
                    {
                        report += "not found.";
                    }
                    return report;
                }
                else
                {
                    if (_cmdNumberIdx != -1)
                    {
                        if (_cmdNumber < 2000)
                            _cmdNumber += 2000;

                        String results = "Enter : mm <date>  for one of the following " + _cmdNumber + " meetings :\n";
                        List<MeetingMins> mms = mmMgr._List.Where(mm => mm._dt.Year == _cmdNumber).OrderBy(m => m._dt).ToList();
                        foreach (MeetingMins m in mms)
                        {
                            results += m._dt.ToShortDateString() + " (" + m._type.ToString() + ")\n";
                        }
                        return results;
                    }
                    else
                    {
                        String results = "Enter : mm <year>  for one of the following years :\n";
                        List<int>years = mmMgr._List.Select(mm => mm._dt.Year).Distinct().OrderByDescending(y => y).ToList();
                        foreach (int y in years)
                        {
                            results += y + "\n";
                        }
                        return results;
                    }
                }
            }

            return "Not performed.";
        }

        private static string EMailDropBoxFile(IConfiguration config, UserManager<CStatUser> userManager, CSUser curUser, string dboxFile, string subjectFileDesc="")
        {
            var dbox = new CSDropBox(Startup.CSConfig, (curUser == null || curUser.IsFull));
            if (dbox == null)
            {
                return "ERROR : DropBox NOT Accessible.";
            }

            var fnIdx = dboxFile.LastIndexOf("/");
            if (fnIdx == -1)
                return "ERROR : DropBox File is NOT Valid.";

            var FileName = dboxFile.Substring(fnIdx + 1);
            var SrcPath = dboxFile.Substring(0, fnIdx);
            string FullSrcPath = dboxFile;

            string FullDestPath = Path.Combine(Path.GetTempPath(), FileName);
            if (System.IO.File.Exists(FullDestPath))
                System.IO.File.Delete(FullDestPath);

            try
            {
                if (dbox.FileExists(FullSrcPath))
                {
                    var task = dbox.DownloadToFile(SrcPath, FileName, FullDestPath);
                    task.Wait();
                    if (System.IO.File.Exists(FullDestPath))
                    {
                        var email = new CSEMail(config, userManager);
                        if (!email.Send(curUser.EMail, curUser.EMail, "RE: Request For " + (!String.IsNullOrEmpty(subjectFileDesc) ? subjectFileDesc : FileName), "Hi\nAttached, please find " + FileName + "\n\nThanks!\nCee Stat", new string[] { FullDestPath }))
                            return "ERROR : Failed to EMail" + FileName;
                    }
                    else
                        return dboxFile + "ERROR : Failed to Download By-Laws";
                    return "SUCCESS : " + FileName + " sent to your EMail.";
                }
                else
                    return FileName + "\nERROR : DropBox file : " + FullSrcPath + " NOT FOUND!";
            }
            catch (Exception e)
            {
                return dboxFile + "\nERROR : " + e.Message;
            }
        }

        CSDropBox GetDropBox ()
        {
            return new CSDropBox(Startup.CSConfig, (_curUser == null || _curUser.IsFull));
        }

        //***************************************************************
        private string HandleCorp(List<string> words)
        //***************************************************************
        {
            string report;
            string fname = "";

            if (_cmdDescList.Any(w => w == "trp"))
            {
                report = "Not available yet."; //_alreadySentEMail = true; //EMailDropBoxFile(_config, _userManager, _curUser, fname);
            }
            else if (_cmdDescList.Any(w => w == "insurance"))
            {
                report = "Not available yet."; //_alreadySentEMail = true; //EMailDropBoxFile(_config, _userManager, _curUser, fname);
            }
            else if (words.Any(w => w == "990"))
            {
                report = "Not available yet."; //_alreadySentEMail = true; //EMailDropBoxFile(_config, _userManager, _curUser, fname);
            }
            else if (words.Any(w => w == "1023"))
            {
                report = "Not available yet."; //_alreadySentEMail = true; //EMailDropBoxFile(_config, _userManager, _curUser, fname);
            }
            else
            {
                _alreadySentEMail = true;
                fname = "/Corporate/Permits & Certifications/CCA/NYS Tax Exempt Certification ST-119.1.pdf";
                report = EMailDropBoxFile(_config, _userManager, _curUser, fname);
            }
            return report;
        }



        //***************************************************************
        private string HandleCSTest(List<string> words)
        //***************************************************************
        {

            ArdMgr ardMgr = new ArdMgr(_hostEnv, _config, _userManager);
            ardMgr.CheckValues("freezerTemp:68,frigTemp:71,kitchTemp:71,waterPres:103,powerOn:364,analog5:318,time:\"7/7/24 12:40:42 PM\"");

            //var cse = new CSEMail(_config, _userManager);
            String report = "";
            //var eList = cse.ReadEMails();
            //foreach (var e in eList)
            //{
            //    report += e.Subject + ".\n";

            //    if ((e.HtmlBody != null) && (e.HtmlBody.Length > 5))
            //        e.AddHtmlAsPDFAttachment();

            //    if (e.Attachments.Count > 0)
            //    {
            //        var dbox = new CSDropBox(Startup.CSConfig);
            //        var destPath = "/Memorandums";
            //        foreach (var a in e.Attachments)
            //        {
            //            string FileName = e.GetFinalFileName(Path.GetFileName(a));
            //            if (dbox.FileExists(destPath + "/" + FileName))
            //            {
            //                var fBody = Path.GetFileNameWithoutExtension(FileName);
            //                var fExt = Path.GetExtension(FileName);
            //                for (char j = 'B'; j < 'Z'; ++j)
            //                {
            //                    FileName = fBody + "_Rev_" + j.ToString() + fExt;
            //                    if (!dbox.FileExists(destPath + "/" + FileName))
            //                        break;
            //                }
            //            }
            //            dbox.UploadFile(a, destPath, FileName);
            //            File.Delete(a);
            //        }
            //    }
            //}
            return report;
        }

        private string[] GetFilteredDesc()
        {
            return _cmdDescList.Where(s => !CmdActionDict.Any(a => s.Equals(a.Key, StringComparison.OrdinalIgnoreCase))
                                        && !CmdFormatDict.Any(f => s.Equals(f.Key, StringComparison.OrdinalIgnoreCase))
                                        && !CmdSrcDict.Any(src => s.Equals(src.Key, StringComparison.OrdinalIgnoreCase))
                                        && !CmdInstsDict.Any(inst => s.Equals(inst.Key, StringComparison.OrdinalIgnoreCase))
                                        && !s.Equals("email", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("show", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("send", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("me", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("please", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("the", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("on", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("about", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("how", StringComparison.OrdinalIgnoreCase)
                                        && !s.Equals("to", StringComparison.OrdinalIgnoreCase)
                                     ).Select(s2 => s2.Replace("'", "").Replace("-", " ")).ToArray();
        }

        private string[] SingularizeDesc()
        {
            var destArr = new List<string>();
            foreach (var s in _cmdDescList)
            {
                if (s.EndsWith("ies"))
                    destArr.Add(s.Substring(0, s.Length - 3) + "y");
                else if (s.EndsWith("s"))
                    destArr.Add(s.Substring(0, s.Length - 1));
                else
                    destArr.Add(s);
            }
            return destArr.ToArray();
        }

        private string[] ModifySOPDesc()
        {
            var destArr = new List<string>();
            var NumDesc = _cmdDescList.Count;
            for (int i=0; i < NumDesc; ++i)
            {
                var s = _cmdDescList[i];
                if (s.StartsWith("sop"))
                { 
                    if (s.Length == 3)
                    {
                        if (i < (NumDesc - 1))
                        {
                            // see if next word starts with a number
                            var s2 = _cmdDescList[i];
                            char[] sarr = s2.ToCharArray(0, s2.Length);
                            if ((sarr[0] >= '0') && (sarr[0] <= '9'))
                            {
                                // combine words
                                destArr.Add("sop-" + s2);
                                ++i;
                                continue;
                            }
                        }
                    }
                    else
                    {
                        char[] sarr = s.ToCharArray(0, s.Length);
                        if (sarr[3] != '-')
                        {
                            if ((sarr[3] == '~') || (sarr[3] == ':') || (sarr[3] == '.'))
                            {
                                destArr.Add("sop-" + s.Substring(4).Trim());
                                continue;
                            }

                            else if ((sarr[3] >= '0') && (sarr[3] <= '9'))
                            {
                                destArr.Add("sop-" + s.Substring(3).Trim());
                                continue;
                            }
                        }
                    }
                }
                destArr.Add(s);
            }
            return destArr.ToArray();
        }

        private bool IsTimeBased(List<string> words)
        {
            return words.Any(w => (w == "when") || (w == "date") || (w == "time") || (w == "day"));
        }

        private CmdAction _cmdAction = default;
        private int _cmdActionIdx = -1;
        private CmdFormat _cmdFormat = default;
        private int _cmdFormatIdx = -1;
        private CmdOutput _cmdOutput = default;
        private int _cmdOutputIdx = -1;

        private EventType _cmdEvent;
        private int _cmdEventIdx = -1;

        private EventType _cmdEvent2;
        private int _cmdEvent2Idx = -1;


        private List<CmdInsts>  _cmdInstsList = new List<CmdInsts>();
        private List<int> _cmdInstsIdxList = new List<int>();

        private bool _hasMy = false;
        private bool _isPlural = false;
        private bool _hasDoneOnly = false;
        private int _hasMyIdx = -1;
        private bool _isQuestion = false;
        private bool _isUrgent = false;
        private bool _isWhen = false;

        private List<string> _cmdDescList = new List<string>();
        private List<int> _cmdDescIdxList = new List<int>();

        private CmdSpecific _cmdSpecific = default;
        private List<int> _cmdSpecificIdxList = new List<int>();
        private List<int> _cmdIgnoreIdxList = new List<int>();

        private int _cmdNumber = -1;
        private int _cmdNumberIdx = -1;

        private CmdSource _cmdSrc = CmdSource.NONE;
        private int _cmdSrcIdx = -1;

        private DateRange _cmdDateRange = null;
        private int _cmdDateTimeStartIdx = -1;
        private int _cmdDateTimeEndIdx = -1;
        private EquipProp _equipPropMatch = null;
        private InventoryState _inventoryStateMatch = null;

        private string _rawCmd = "";
        private bool _alreadySentEMail = false;

        Dictionary<CmdSource, HandleSrcDel> _srcDelegateDict = new Dictionary<CmdSource, HandleSrcDel>();
    }

}
