using CStat.Areas.Identity.Data;
using CStat.Data;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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
            FIND    = 0x00000000,
            COMPUTE = 0x00000001,
            ADD     = 0x00000002,
            DELETE  = 0x00000004,
            UPDATE  = 0x00000008,
            CALL    = 0x00000010,
            NEED    = 0x00000020
        };

        private static readonly Dictionary<string, CmdAction> CmdActionDict = new Dictionary<string, CmdAction>(StringComparer.OrdinalIgnoreCase)
        {
            {"Phone", CmdAction.CALL},
            {"Phones", CmdAction.CALL},
            {"needed", CmdAction.NEED},
            {"needs", CmdAction.NEED},
            {"Get", CmdAction.FIND},
            {"Show", CmdAction.FIND},
            {"Report", CmdAction.FIND},
            {"List", CmdAction.FIND},
            {"Locate", CmdAction.FIND},
            {"Modify", CmdAction.UPDATE},
            {"Change", CmdAction.UPDATE},
            {"Append", CmdAction.ADD}
        };

        //=================================================================
        public enum CmdSource
        {
            QUESTION = 0x00000000,
            INVENTORY = 0x00000001,
            PROPANE = 0x00000002,
            PERSON = 0x00000004,
            DOC = 0x00000008,
            TASK = 0x00000010,
            EQUIP = 0x00000020,
            FRIDGE = 0x00000040,
            FREEZER = 0x00000080,
            PRESSURE = 0x00000100,
            ELECTRIC = 0x00000200,
            CAMERA   = 0x00000400,
            REQ      = 0x00000800,
            BYLAWS   = 0x00001000,
            EVENT    = 0x00002000,
            ATTENDANCE = 0x00004000,
            MENU     = 0x00008000,
            URGENCY  = 0x00010000,
            CSTEST   = 0x00020000
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
            {"meter", new Tuple<CmdSource, bool>(CmdSource.ELECTRIC, false) },
            {"inv", new Tuple<CmdSource, bool>(CmdSource.INVENTORY, false) },
            {"?", new Tuple<CmdSource, bool>(CmdSource.QUESTION, false) },
            {"pic", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"photo", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"shot", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) },
            {"snapshot", new Tuple<CmdSource, bool>(CmdSource.CAMERA, false) }, // C -
            {"refrigerator", new Tuple<CmdSource, bool>(CmdSource.FRIDGE, false) },
            {"doc", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"manual", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
            {"document", new Tuple<CmdSource, bool>(CmdSource.DOC, false) },
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
            {"requirements", new Tuple<CmdSource, bool>(CmdSource.REQ, false) },
            {"required", new Tuple<CmdSource, bool>(CmdSource.REQ, false) },
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
            {"people", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"name", new Tuple<CmdSource, bool>(CmdSource.PERSON, false) },
            {"names", new Tuple<CmdSource, bool>(CmdSource.PERSON, true) },
            {"cstest", new Tuple<CmdSource, bool>(CmdSource.CSTEST, false) }
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
            ALL =        0x00000800
        }

        private static readonly Dictionary<string, CmdInsts> CmdInstsDict = new Dictionary<string, CmdInsts>(StringComparer.OrdinalIgnoreCase)
        {
            {"every", CmdInsts.ALL},
            {"full", CmdInsts.ALL},
            {"present", CmdInsts.CURRENT},
            {"todays", CmdInsts.CURRENT},
            {"today's", CmdInsts.CURRENT},
            {"yesterday's", CmdInsts.PRIOR},
            {"yesterday", CmdInsts.PRIOR},
            {"new", CmdInsts.NEXT},
            {"old", CmdInsts.PRIOR},
            {"past", CmdInsts.PRIOR},
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

        private readonly CStat.Models.CStatContext _context;
        private readonly CSSettings _csSettings;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private readonly CSUser _curUser;

        public CmdMgr(CStat.Models.CStatContext context, CSSettings cset, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CSUser curUser)
        {
            _context = context;
            _csSettings = cset;
            _hostEnv = hostEnv;
            _config = config;
            _userManager = userManager;
            _curUser = curUser;

            _srcDelegateDict.Add(CmdSource.MENU, HandleMenu);
            _srcDelegateDict.Add(CmdSource.INVENTORY, HandleInventory);
            _srcDelegateDict.Add(CmdSource.TASK, HandleTasks);
            _srcDelegateDict.Add(CmdSource.PERSON, HandlePeople);

            _srcDelegateDict.Add(CmdSource.DOC, HandleDocs);
            _srcDelegateDict.Add(CmdSource.REQ, HandleDocs);
            _srcDelegateDict.Add(CmdSource.BYLAWS, HandleDocs);

            _srcDelegateDict.Add(CmdSource.EQUIP, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PROPANE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FRIDGE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.FREEZER, HandleEquip);
            _srcDelegateDict.Add(CmdSource.PRESSURE, HandleEquip);
            _srcDelegateDict.Add(CmdSource.ELECTRIC, HandleEquip);
            _srcDelegateDict.Add(CmdSource.CAMERA, HandleEquip);

            _srcDelegateDict.Add(CmdSource.EVENT, HandleEvents);
            _srcDelegateDict.Add(CmdSource.ATTENDANCE, HandleAttendance);
            _srcDelegateDict.Add(CmdSource.URGENCY, HandleUrgency);
            _srcDelegateDict.Add(CmdSource.CSTEST, HandleCSTest);
        }

        public static List<string> GetWords(string cmdStr)
        {
            return new List<string>(cmdStr.Split(new char[] { ' ', '\n', ',', ';' })).Select(w => w.Trim()).ToList();
        }

        public bool ParseCmd(List<string> rawWords)
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
            FindInsts(words);
            FindNumber(words);
            FindDates(words);
            FindEvent(words);
            FindDesc(words); // Check for descriptive hints

            if ((_cmdSrc == default) && ((_cmdAction == CmdAction.CALL) || (_cmdDescList.Count == 1) || (_cmdDescList.Count == 2)))
                _cmdSrc = CmdSource.PERSON;

            if ((_cmdAction == CmdAction.NEED) && ((_cmdSrc == default) || (_cmdSrc == CmdSource.MENU)))
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

            if (!noEMail && (IsEMail(words) || (result.Length > 600)))
            {
                CSEMail csEMail = new CSEMail(_config, _userManager);
                return SrcTitle(_cmdSrc) + (csEMail.Send(_curUser.EMail, _curUser.EMail, "RE: " + _rawCmd, result) ? "Successfully Sent to " : "Failed to be sent to ") + _curUser.EMail;
            }
            return (result.Length > 0) ? result : "No Results.";
        }

        private bool IsEMail(List<string> words)
        {
            return ((words[0] == "email") || ((words.Count > 2) && (words[0] == "send" && words[1] == "email")));
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

        public bool FindNumber(List<string> words)
        {
            _cmdNumber = -1;

            var NumWords = words.Count;
            for (int i = 0; i < NumWords; ++i)
            {
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
            _cmdAction = CmdAction.FIND; // default to FIND

            var NumWords = words.Count;

            var CmdActionList = Enum.GetNames(typeof(CmdAction)).Cast<string>().ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                var word = words[i];
                var match = CmdActionList.Find(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(match))
                {
                    _cmdAction = (CmdAction)Enum.Parse(typeof(CmdAction), match);
                    _cmdSrcIdx = i;
                    return true;
                }

                if (CmdActionDict.TryGetValue(word, out CmdAction cmdAction))
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

            var CmdInstsList = Enum.GetNames(typeof(CmdInsts)).Cast<string>().ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                if (i == _cmdSrcIdx)
                    continue;

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
        public bool FindEvent(List<string> words)
        {
            // Filter out Sources and actions not associated with Event
            if (((_cmdSrc != CmdSource.QUESTION) && (_cmdSrc != CmdSource.ATTENDANCE) && (_cmdSrc != CmdSource.MENU) && (_cmdSrc != CmdSource.EVENT)) ||
                (_cmdAction == CmdAction.CALL))
            {
                return false;
            }

            var NumWords = words.Count;
            var eventList = Enum.GetNames(typeof(EventType)).Cast<string>().Select(e => { return e.Replace('_', ' ').ToLower(); }).Where(s => s != "other").ToList();
            for (int i = 0; i < NumWords; ++i)
            {
                if ((i == _cmdActionIdx) || (i == _cmdSrcIdx) || (i == _cmdNumberIdx) || (_cmdDateTimeStartIdx == i) || (_cmdDateTimeEndIdx == i))
                    return false;
                if (_cmdInstsIdxList.Any(j => j == i))
                    return false;

                // Make sure word match is a whole Event word
                var eventStrs = eventList.Where(es => es.StartsWith(words[i] + " ")).ToList();
                if (eventStrs.Count > 0)
                {
                    string str = eventStrs[0];
                    if (eventStrs.Count > 1)
                    {
                        if (!string.IsNullOrEmpty(words.Skip(i + 1).First(w => w.StartsWith("retreat")))) // the word, "retreat" is found futher in list
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
            var CmdSrcList = Enum.GetNames(typeof(CmdSource)).Cast<string>().ToList();
            for (int i = NumWords - 1; i >= 0; --i)
            {
                var word = words[i];
                var match = CmdSrcList.Find(c => c.Equals(word, StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(match))
                {
                    _cmdSrc = (CmdSource)Enum.Parse(typeof(CmdSource), match);
                    _cmdSrcIdx = i;
                    return true;
                }

                if (CmdSrcDict.TryGetValue(word, out Tuple<CmdSource, bool> cmdSrc))
                {
                    _cmdSrc = cmdSrc.Item1;
                    _isPlural = cmdSrc.Item2;
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
            for (int i = 0; i < NumWords; ++i)
            {
                if ((i == _cmdActionIdx) || (i == _cmdEventIdx) || (_cmdInstsIdxList.IndexOf(i) != -1) || (i == _hasMyIdx) || (i == _cmdNumberIdx) ||
                    (i == _cmdSrcIdx) || ((i >= _cmdDateTimeStartIdx) && (i <= _cmdDateTimeEndIdx)))
                    continue;
                if ((LastDescIdx != -1) && (LastDescIdx != (i - 1)))
                    break;
                
                _cmdDescList.Add(words[i]);
                LastDescIdx = i;
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
            return "Ask about Inventory, propane, specific measuments, people, camera, Requirements, By-Laws, docs, Events, urgencies, attendance";
        }
        private string HandleInventory(List<string> words)
        {
            var justNeeded = _cmdAction == CmdAction.NEED;
            return InventoryItem.GetInventoryReport(_context, _config, false, out string subject, true);
        }
        private string HandleUrgency(List<string> words)
        {
            var justNeeded = _cmdAction == CmdAction.NEED;
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

            return report;
        }
        private string HandlePeople(List<string> words)
        {
            string result = "";
            List<Person> people = null;
            if (_cmdDescList.Count == 1)
            {
               people = _context.Person.Where(p => p.FirstName == _cmdDescList[0]).Include(p => p.Address).ToList();
            }
            else if (_cmdDescList.Count == 2)
            {
                people = _context.Person.Where(p => (p.FirstName == _cmdDescList[0]) && (p.LastName.StartsWith(_cmdDescList[1]))).Include(p => p.Address).ToList();
            }
            if (people.Count > 0)
            {
                foreach (var p in people)
                {
                    result += p.FirstName + " " + p.LastName + " : " + ((!string.IsNullOrEmpty(p.CellPhone) ? Person.FixPhone(p.CellPhone) :
                        ((p.Address != null) && (!string.IsNullOrEmpty(p.Address.Phone)) ? Person.FixPhone(p.Address.Phone) : "unknown #"))) + $"\n";
                }
            }
            return result;
        }
        private string HandleEquip(List<string> words)
        {
            string result = "";
            if (_cmdSrc == CmdSource.PROPANE)
            {
                // Persist Daily Reading and Notify if needed for Propane
                PropaneMgr pmgr = new PropaneMgr(_hostEnv, _config, _userManager);
                var plNow = pmgr.GetTUTank(); // get value, log to file and check

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
                    return AppendPropLine(pmgr, plNow, result);
                }
            }
            else if (_cmdSrc == CmdSource.ELECTRIC)
            {
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
                result = "Equipment Report :\n";
                ArdMgr am = new ArdMgr(_hostEnv, _config, _userManager);
                var ar = am.ReportLastestValues(ref result, false); // Ard only : No propane
            }

            return (result.Length == 0) ? "Huh?" : result;
        }

        private List<PropaneLevel> GetFullPropaneList (PropaneMgr pmgr, PropaneLevel plNow)
        {
            var plList = pmgr.GetAll();
            if (plNow != null)
            {
                var plCnt = plList.Count;
                if ((plCnt > 0) && (!plList[plCnt - 1].IsSame(plNow)))
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

        //***************************************************ZZZ
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
                    eList = Event.GetEvents(_context, new DateTime(1900, 1, 1), DateRangeType.On_or_After_Date); // All
                    sidx = 0;
                    eidx = eList.Count - 1;
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
                    var DateStr = e.StartTime.Date.ToShortDateString() + "-" + e.EndTime.Date.ToShortDateString();
                    report += e.Description + "(" + DateStr + ").\n";
                }
            }
            return report;
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

        //***************************************************************
        private string HandleCSTest(List<string> words)
        //***************************************************************
        {
            var cse = new CSEMail(_config, _userManager);
            String report = "";
            var eList = cse.ReadEMails();
            foreach (var e in eList)
            {
                report += e.Subject + ".\n";

                if ((e.HtmlBody != null) && (e.HtmlBody.Length > 5))
                    e.AddHtmlAsPDFAttachment();

                if (e.Attachments.Count > 0)
                {
                    var dbox = new CSDropBox(Startup.CSConfig);
                    var destPath = "/Memorandums";
                    foreach (var a in e.Attachments)
                    {
                        string FileName = e.GetFinalFileName(Path.GetFileName(a));
                        if (dbox.FileExists(destPath + "/" + FileName))
                        {
                            var fBody = Path.GetFileNameWithoutExtension(FileName);
                            var fExt = Path.GetExtension(FileName);
                            for (char j = 'B'; j < 'Z'; ++j)
                            {
                                FileName = fBody + "_Rev_" + j.ToString() + fExt;
                                if (!dbox.FileExists(destPath + "/" + FileName))
                                    break;
                            }
                        }
                        dbox.UploadFile(a, destPath, FileName);
                        File.Delete(a);
                    }
                }
            }
            return report;
            return "";
        }

        private CmdAction _cmdAction = default;
        private int _cmdActionIdx = -1;

        private EventType _cmdEvent;
        private int _cmdEventIdx = -1;

        private List<CmdInsts>  _cmdInstsList = new List<CmdInsts>();
        private List<int> _cmdInstsIdxList = new List<int>();

        private bool _hasMy = false;
        private bool _isPlural = false;
        private bool _hasDoneOnly = false;
        private int _hasMyIdx = -1;

        private List<string> _cmdDescList = new List<string>();

        private int _cmdNumber = -1;
        private int _cmdNumberIdx = -1;

        private CmdSource _cmdSrc = default;
        private int _cmdSrcIdx = -1;

        private DateRange _cmdDateRange = null;
        private int _cmdDateTimeStartIdx = -1;
        private int _cmdDateTimeEndIdx = -1;

        private string _rawCmd = "";

        Dictionary<CmdSource, HandleSrcDel> _srcDelegateDict = new Dictionary<CmdSource, HandleSrcDel>();
    }
}
