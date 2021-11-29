using CStat.Areas.Identity.Data;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CStat.Models.Event;

namespace CStat.Common
{
    public class CmdMgr
    {
        public delegate string HandleSrcDel(List<string> words);

        public enum CmdAction
        {
            FIND = 0x00000000,
            LIST = 0x00000001,
            COMPUTE = 0x00000002,
            ADD = 0x00000004,
            DELETE = 0x00000008,
            UPDATE = 0x00000010,
        }

        private static readonly Dictionary<string, CmdAction> CmdActionDict = new Dictionary<string, CmdAction>(StringComparer.OrdinalIgnoreCase)
        {
            {"Get", CmdAction.FIND},
            {"Locate", CmdAction.FIND},
            {"Modify", CmdAction.UPDATE},
            {"Change", CmdAction.UPDATE},
            {"Append", CmdAction.ADD}
        };

        //=================================================================
        public enum CmdSource
        {
            MENU       = 0x00000000,
            INVENTORY  = 0x00000001,
            PROPANE    = 0x00000002,
            PERSON     = 0x00000004,
            DOC        = 0x00000008,
            TASKS      = 0x00000010,
            EQUIP      = 0x00000020,
            FRIDGE     = 0x00000040,
            FREEZER    = 0x00000040,
            PRESSURE   = 0x00000080,
            ELECTRIC   = 0x00000100,
            CAMERA     = 0x00000200,
            REQ        = 0x00000400,
            BYLAWS     = 0x00000800,
            EVENT      = 0x00001000,
            ATTENDANCE = 0x00002000
        }

        private static readonly Dictionary<string, CmdSource> CmdSrcDict = new Dictionary<string, CmdSource>(StringComparer.OrdinalIgnoreCase)
        {
            {"Stock", CmdSource.INVENTORY},
            {"tank", CmdSource.PROPANE},
            {"fuel", CmdSource.PROPANE},
            {"gas", CmdSource.PROPANE},
            {"inv", CmdSource.INVENTORY},
            {"Supplies", CmdSource.INVENTORY},
            {"?", CmdSource.MENU},
            {"pic", CmdSource.CAMERA},
            {"photo", CmdSource.CAMERA},
            {"shot", CmdSource.CAMERA},
            {"snapshot", CmdSource.CAMERA}, // C -
            {"refrigerator", CmdSource.FRIDGE},
            {"doc", CmdSource.DOC},
            {"manual", CmdSource.DOC},
            {"document", CmdSource.DOC},
            {"info", CmdSource.DOC},
            {"procedure", CmdSource.DOC},
            {"procedures", CmdSource.DOC},
            {"instructions", CmdSource.DOC},
            {"requirements", CmdSource.REQ},
            {"required", CmdSource.REQ},
            {"todo", CmdSource.TASKS },
            {"phone#", CmdSource.PERSON },
            {"phonenumber", CmdSource.PERSON},
            {"people", CmdSource.PERSON},
            {"name", CmdSource.PERSON }
        };

        //===============================================================
        public enum CmdInsts
        {
            ALL        = 0x00000000,
            LAST       = 0x00000001,
            NEXT       = 0x00000002,
            LAST_N     = 0x00000004,
            NEXT_N     = 0x00000008,
            YEAR       = 0x00000010,
            YEAR_MONTH = 0x00000020,
            DATE_RANGE = 0x00000040,
            OVERDUE    = 0x00000080,
            MY         = 0x00000100,
            SPECIFIC   = 0x00000200
        }

        private static readonly Dictionary<string, CmdInsts> CmdInstsDict = new Dictionary<string, CmdInsts>(StringComparer.OrdinalIgnoreCase)
        {
            {"every", CmdInsts.ALL},
            {"full", CmdInsts.ALL},
            {"prior", CmdInsts.LAST},
            {"past", CmdInsts.LAST},
            {"future", CmdInsts.NEXT},
            {"coming", CmdInsts.NEXT},
            {"upcoming", CmdInsts.NEXT},    // c -
            {"overdue", CmdInsts.OVERDUE},  // c -
            {"delinquent", CmdInsts.OVERDUE},
            {"late", CmdInsts.OVERDUE},
            {"mine", CmdInsts.MY},
            {"me", CmdInsts.MY}
        };

        public enum CmdDest
        {
            REPLY = 0x00000000,
            EMAIL = 0x00000001
        }

        private readonly CStat.Models.CStatContext Context;
        private readonly CSSettings csSettings;
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;

        public CmdMgr (CStat.Models.CStatContext context, CSSettings cset, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            Context = context;
            csSettings = cset;
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;

            HandleSrcDel handleMenu = HandleMenu;
            HandleSrcDel handleInventory = HandleInventory;
            HandleSrcDel handleEquip = HandleEquip;
            HandleSrcDel handleDocs = HandleDocs;
            HandleSrcDel handlePeople = HandlePeople;
            HandleSrcDel handleAttendance = HandleAttendance;
            HandleSrcDel handleEvents = HandleEvents;

            _srcDelegateDict.Add(CmdSource.MENU, HandleMenu);
            _srcDelegateDict.Add(CmdSource.INVENTORY, HandleMenu);
            _srcDelegateDict.Add(CmdSource.PERSON, HandleMenu);

            _srcDelegateDict.Add(CmdSource.DOC, HandleDocs);
            _srcDelegateDict.Add(CmdSource.REQ, HandleDocs);
            _srcDelegateDict.Add(CmdSource.BYLAWS, HandleDocs);
            _srcDelegateDict.Add(CmdSource.TASKS, HandleDocs);

            _srcDelegateDict.Add(CmdSource.EQUIP, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PROPANE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FRIDGE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FREEZER, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PRESSURE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.ELECTRIC, HandleEquip);
            _srcDelegateDict.Add(CmdSource.CAMERA, HandleEquip);

            _srcDelegateDict.Add(CmdSource.EVENT, HandleAttendance);
            _srcDelegateDict.Add(CmdSource.ATTENDANCE, HandleEvents);
        }
        public bool ParseCmd (List<string> rawWords)
        {
            var NumWords = rawWords.Count;

            // Create Clean List
            List<string> words = new List<string>();
            for (int i = 0; i < NumWords; ++i)
            {
                var word = rawWords[i].ToLower().Replace("-", "");
                if (rawWords[i] == "camp")
                    continue; // skip because it is implied / gets in the way
                int j = i + 1;
                if (j < NumWords)
                {
                    var word2 = rawWords[j];
                    if (((word == "young") && (word2.StartsWith("adult"))) ||
                        ((word == "spring") && (word2.StartsWith("work"))) ||
                        ((word == "first") && (word2.StartsWith("chance"))))
                    {
                        word = word + " " + word2; // Combine with space
                    }
                    else if (((word == "up") && (word2.StartsWith("coming"))) ||
                             ((word == "to") && (word2.StartsWith("do"))) ||
                             ((word == "phone") && (word2.StartsWith("number"))) ||
                             ((word == "phone") && (word2.StartsWith("#"))) ||
                             ((word == "over") && (word2.StartsWith("due"))))
                    {
                        word = word + word2; // Combine with NO space
                    }
                }
                words.Add(word);
            }

            // Look for command source first
            FindCmdSource(words);
            FindCmdAction(words);
            FindNumber(words);
            FindDates(words);
            FindEvent(words);
            FindDesc(words); // Check for descriptive hints
           
            return true;
        }

        public string ExecuteCmd(List<string> words)
        {
            return "Huh?";
        }

        private string HandleMenu(List<string> words)
        {
            return "Not Implemented";
        }
        private string HandleInventory(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandlePeople(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandleEquip(List<string> words)
        {
            if (_cmdSrc == CmdSource.PROPANE)
            {
                var pmgr = new PropaneMgr(HostEnv, Config, UserManager);
                var pl = pmgr.GetTUTank();
                return "Propane read " + pl.LevelPct.ToString("0.#") + "% at " + pl.ReadingTimeStr();
            }

            return "Not Implemented";
        }

        private string HandleTasks(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandleEvents(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandleCamera(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandleAttendance(List<string> words)
        {
            return "Not Implemented";
        }

        private string HandleDocs(List<string> words)
        {
            return "Not Implemented";
        }

        public bool FindNumber(List<string> words)
        {
            _cmdNumber = -1;

            var NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
                if (int.TryParse(words[i], out int num))
                {
                    _cmdNumber = num;
                    return true;
                }
            }
            return false;
        }

        public bool FindDates(List<string> words)
        {
            _cmdDateTime = null;

            var NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
                if (DateTime.TryParse(words[i], out DateTime dt))
                {
                    if (!_cmdDateTime.HasValue)
                    {
                        _cmdDateTime = dt;
                        _cmdDateTimeStartIdx = i;
                    }
                    else
                    {
                        _cmdDateRange = new DateRange(_cmdDateTime.Value, dt);
                        _cmdDateTimeEndIdx = i;
                        _cmdDateTime = null;
                        return true;
                    }
                }
            }
            if ((_cmdDateTimeStartIdx != -1) && (_cmdDateTimeEndIdx == -1))
                _cmdDateTimeEndIdx = _cmdDateTimeStartIdx;
            return _cmdDateTime.HasValue;
        }

        public bool FindCmdAction(List<string> words)
        {
            _cmdAction = CmdAction.FIND; // default to FIND

            var NumWords = words.Count;
            for (int i = NumWords - 1; i >= 0; --i)
            {
                CmdAction cmdAction;
                if (Enum.TryParse(words[i], true, out cmdAction))
                {
                    _cmdAction = cmdAction;
                    _cmdActionIdx = i;
                    return true;
                }

                if (CmdActionDict.TryGetValue(words[i], out cmdAction))
                {
                    _cmdAction = cmdAction;
                    _cmdActionIdx = i;
                    return true;
                }
            }
            return true;
        }

        public bool FindInsts(List<string> words)
        {
            var NumWords = words.Count;
            _cmdInstsList.Clear();
            _cmdInstsIdxList.Clear();

            for (int i = NumWords - 1; i >= 0; --i)
            {
                if (i == _cmdSrcIdx)
                    continue;

                CmdInsts cmdInsts;
                if (Enum.TryParse(words[i], true, out cmdInsts))
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

                if (CmdInstsDict.TryGetValue(words[i], out cmdInsts))
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
            return true;
        }
        public bool FindEvent(List<string> words)
        {
            var NumWords = words.Count;
            var eventList = Enum.GetValues(typeof(EventType)).Cast<string>().Select(e => { return e.Replace('_', ' ').ToLower(); }).ToList();
            for (int i=0; i < NumWords; ++i)
            {
                var eventStrs = eventList.Where(es => es.StartsWith(words[i])).ToList();
                if (eventStrs.Count > 0)
                {
                    string str = eventStrs[0];
                    if (eventStrs.Count > 1)
                    {
                        if (!string.IsNullOrEmpty(words.Skip(i+1).First(w => w.StartsWith("retreat")))) // the word, "retreat" is found futher in list
                        {
                            str = eventStrs.First(es => es.Contains("retreat")); // a Matching event has "retreat" in it. favor it. in this case. 
                            if (string.IsNullOrEmpty(str))
                                str = eventStrs[0];
                        }
                    }
                    if (Enum.TryParse(str.Replace(' ', '_'), true, out EventType et))
                    {
                        _cmdEvent = et;
                        _cmdEventIdx = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool FindCmdSource(List<string> words) 
        {
            var NumWords = words.Count;
            for (int i = NumWords - 1; i >= 0; --i)
            {
                CmdSource cmdSrc;
                if (Enum.TryParse(words[i], true, out cmdSrc))
                {
                    _cmdSrc = cmdSrc;
                    _cmdSrcIdx = i;
                    return true;
                }

                if (CmdSrcDict.TryGetValue(words[i], out cmdSrc))
                {
                    _cmdSrc = cmdSrc;
                    _cmdSrcIdx = i;
                    return true;
                }
            }
            return false;
        }

        public bool FindDesc(List<string> words)
        {
            _cmdDescList.Clear();
            var NumWords = words.Count;
            int LastDescIdx = -1;
            for (int i = NumWords - 1; i >= 0; --i)
            {
                if ((i == _cmdActionIdx) || (i == _cmdEventIdx) || (_cmdInstsIdxList.IndexOf(i) != -1) || (i == _hasMyIdx) || (i == _cmdNumberIdx) ||
                    (i == _cmdSrcIdx) || ((i >= _cmdDateTimeStartIdx) && (i <= _cmdDateTimeEndIdx)))
                    continue;
                if ((LastDescIdx != -1) && (LastDescIdx != (i - 1)))
                    break;
                
                _cmdDescList.Add(words[i]);
                LastDescIdx = i;
            }
            return _cmdDescList.Count > 0;

        }

        private CmdAction _cmdAction = default;
        private int _cmdActionIdx = -1;

        private EventType _cmdEvent;
        private int _cmdEventIdx = -1;

        private List<CmdInsts>  _cmdInstsList = new List<CmdInsts>();
        private List<int> _cmdInstsIdxList = new List<int>();

        private bool _hasMy = false;
        private int _hasMyIdx = -1;

        private List<string> _cmdDescList = new List<string>();

        private int _cmdNumber = -1;
        private int _cmdNumberIdx = -1;

        private CmdSource _cmdSrc = default;
        private int _cmdSrcIdx = -1;

        private DateTime? _cmdDateTime = null;
        private DateRange _cmdDateRange = null;
        private int _cmdDateTimeStartIdx = -1;
        private int _cmdDateTimeEndIdx = -1;

        Dictionary<CmdSource, HandleSrcDel> _srcDelegateDict = new Dictionary<CmdSource, HandleSrcDel>();
    }
}
