using CStat.Areas.Identity.Data;
using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using static CStat.Models.Person;
using static CStat.Models.Task;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    static public class TaskArrs
    {
        static public string[] weekDays = { "", "Sun", "Mon", "Tues", "Wed", "Thurs", "Fri", "Sat" };
    }

    //[Allow//Anonymous]
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private readonly IWebHostEnvironment hostEnv;

        public TaskData taskData { get; set; }

        [BindProperty]
        public CTask task { get; set; }

        public bool IsTemplate = false;

        public CreateModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            hostEnv = hstEnv;
            _config = config;
            _userManager = userManager;
            taskData = new TaskData();
            task = new CTask();
        }

        //private TaskData CreateTestTaskData ()
        //{
        //    TaskData tData = new TaskData(task.Id)
        //    {
        //        state = (int)CTask.eTaskStatus.Paused,
        //        reason = (int)CTask.eTaskStatus.Need_Funds,
        //        Desc = "The same came to Jesus by night, and said unto him, Rabbi, we know that thou art a teacher come from God: for no man can do these miracles that thou doest, except God be with him.             Jesus answered and said unto him, Verily, verily, I say unto thee, Except a man be born again, he cannot see the kingdom of God. Nicodemus saith unto him, How can a man be born when he is old ? can he enter the second time into his mother's womb, and be born? Jesus answered, Verily, verily, I say unto thee, Except a man be born of water and of the Spirit, he cannot enter into the kingdom of God. That which is born of the flesh is flesh; and that which is born of the Spirit is spirit. Marvel not that I said unto thee, Ye must be born again. The wind bloweth where it listeth, and thou hearest the sound thereof, but canst not tell whence it cometh, and whither it goeth: so is every one that is born of the Spirit."
        //    };

        //    Pic pic = new Pic(126, 1, "This is the title description for Pic 1", "Images/Img_126_1.jpg");
        //    tData.pics.Add(pic);
        //    pic = new Pic(126, 2, "This is the title description for Pic 22", "Images/Img_126_2.jpg");
        //    tData.pics.Add(pic);
        //    pic = new Pic(126, 3, "This is the title description for Pic 333", "Images/Img_126_3.jpg");
        //    tData.pics.Add(pic);
        //    return tData;
        //}

        private void InitializeTask(int tid=0)
        {
            task = new CTask();
            taskData = new TaskData();
            if (tid == -1)
            {
                // Initialize Template
                task.Type |= (int)CTask.eTaskType.Template;
                IsTemplate = true;
                taskData.FixedDueDate = true;
                tid = 0;
            }
            else
            {
                task.Type = 0;
            }

            task.Description = "";
            task.EstimatedManHours = 0;
            task.Id = tid;
            task.PersonId = null;
            task.TotalCost = 0;
            task.EstimatedDoneDate = new DateTime(1900, 1, 1);
            task.CommittedCost = 0;
            task.SetTaskStatus(CTask.eTaskStatus.Not_Started, CTask.eTaskStatus.Unknown, 0);
            task.Priority = (int)CTask.ePriority.High;
            task.RequiredSkills = "";
        }

        public IActionResult OnGet(int? id)
        {
            int tid = !id.HasValue ? tid = 0 : id.Value;
            int res = 0;
            if (tid <= 0)
            {
                // Create new task
                InitializeTask(tid);
                _context.Task.Add(task);
                res = _context.SaveChanges();
            }
            else
            {
                task = _context.Task.FirstOrDefault(m => m.Id == tid);
                if ((task != null) && (tid == task.Id))
                {
                    IsTemplate = (task.Type & (int)CTask.eTaskType.Template) != 0;
                    taskData = TaskData.ReadTaskData(hostEnv, tid, task.ParentTaskId.HasValue ? task.ParentTaskId.Value : -1);
                    if ((task.DueDate == null) && !IsTemplate)
                        task.DueDate = task.EstimatedDoneDate; // temporary for editing purpose
                    else
                        taskData.FixedDueDate = true;

                    task.GetTaskStatus(out eTaskStatus state, out eTaskStatus reason, out int PercentComplete);
                    taskData.state = (int)state;
                    taskData.reason = (int)reason;
                    taskData.PercentComplete = PercentComplete;

                    if ((task.Type & (int)CTask.eTaskType.Template) != 0)
                    {
                        // Initialize Template Type members
                        CTask.eTaskType CreateTaskDue;
                        CTask.eTaskType CreateTaskEach;
                        int CreateTaskDueVal;
                        task.GetTaskType(out CreateTaskDue, out CreateTaskEach, out CreateTaskDueVal);
                        taskData.CreateTaskDue = (int)CreateTaskDue;
                        taskData.CreateTaskEach = (int)CreateTaskEach;
                        taskData.TillDay = ((taskData.CreateTaskEach == (int)CTask.eTaskType.Day_from_Due_till) && task.EstimatedDoneDate.HasValue) ?
                                                task.EstimatedDoneDate.Value.Month + "/" + task.EstimatedDoneDate.Value.Day : "";
                        taskData.NumMOYs = task.Status.ToString();
                        switch (CreateTaskDue)
                        {
                            case CTask.eTaskType.Day_Of_Week_SunMon:
                                taskData.CreateTaskDueVal = ((CreateTaskDueVal >= 1) && (CreateTaskDueVal <= 7)) ? TaskArrs.weekDays[CreateTaskDueVal] : "";
                                break;

                            //case CTask.eTaskType.At_Date:
                            //    if (CreateTaskDueVal > (1 << 5))
                            //    {
                            //        int day = CreateTaskDueVal & 0x1F;
                            //        int month = CreateTaskDueVal >> 5;
                            //        taskData.CreateTaskDueVal = month.ToString() + "/" + day;
                            //    }
                            //    else
                            //        taskData.CreateTaskDueVal = "";
                            //    break;

                            default:
                                taskData.CreateTaskDueVal = CreateTaskDueVal.ToString();
                                break;
                        }
                    }
                }
                else
                    InitializeTask(tid);
            }

            IList<SelectListItem> sList = Enum.GetValues(typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e < (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["State"] = sList;
            IList<SelectListItem> rList = Enum.GetValues (typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e >= (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Reason"] = rList;

            IList<SelectListItem> dueList = Enum.GetValues(typeof(CTask.eTaskType)).Cast<CTask.eTaskType>().Where(e => ((int)e >= (int)CTask.eTaskType.Before_Start) && ((int)e <= (int)CTask.eTaskType.Day_Of_Week_SunMon)).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["CreateTaskDue"] = dueList;

            IList<SelectListItem> eachList = Enum.GetValues(typeof(CTask.eTaskType)).Cast<CTask.eTaskType>().Where(e => ((int)e >= (int)CTask.eTaskType.Retreat_Event) && ((int)e <= (int)CTask.eTaskType.Years_Period_from_Date)).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " ").Replace(" n ", "/"), Value = ((int)x).ToString() }).ToList();
            ViewData["CreateTaskEach"] = eachList;

            IList<SelectListItem> pList = Enum.GetValues(typeof(CTask.ePriority)).Cast<CTask.ePriority>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Priority"] = pList;

            ViewData["Blocking1Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["Blocking2Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");

            IList<SelectListItem> personList = _context.Person.OrderBy(p => p.LastName).ThenBy(p1 => p1.FirstName).Where(p => (p.SkillSets & (long)eSkillsAbbr.WRK) != 0).Select(p2 => new SelectListItem { Text = p2.FirstName + " " + p2.LastName, Value = p2.Id.ToString() } ).ToList<SelectListItem>();
            personList.Insert(0, new SelectListItem(" ", "-1"));
            ViewData["PersonId"] = personList;

            IList<SelectListItem> trList = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["TitleRoles"] = new SelectList(trList, "Value", "Text");

            return Page();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //            _context.Task.Add(task);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public ActionResult OnPostSendPic()
        {
            if ((this.Request != null) && (this.Request.Form != null))
            {
                var files = this.Request.Form.Files;
                string PicTitle = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "picTitle").Value);
                int imgTID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
                int imgPID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "picId").Value);

                if (files != null && (files.Count == 1) && (imgTID >= 0))
                {
                    string folderName = @"Tasks\Images";
                    string webRootPath = hostEnv.WebRootPath;
                    Pic pic = new Pic(imgTID, imgPID, PicTitle);
                    string newPath = Path.Combine(webRootPath, folderName);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }

                    string picFilename = "";
                    foreach (IFormFile item in files)
                    {
                        if (item.Length > 0)
                        {
                            string fileName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                            picFilename = pic.GetFileName(fileName);
                            string fullPath = Path.Combine(newPath, picFilename);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                item.CopyTo(stream);
                            }
                        }
                    }
                    return this.Content("Success:" + "Images" + "/" + picFilename);  // Send back URL
                }
            }
            return this.Content("Fail");
        }

        public ActionResult OnPostSave()
        {
            if ((this.Request != null) && (this.Request.Form != null))
            {
                bool IsTemplate = false;

                /******************

                        public int tid { get; set; }
                        public int state { get; set; }
                        [DataMember]
                        public int reason { get; set; }
                        [DataMember]
                        public int PercentComplete { get; set; }
                        [DataMember]
                        public string Desc { get; set; }
                        [DataMember]
                        public List<Pic> pics { get; set; }
                        [DataMember]
                        public List<BOMLine> bom { get; set; }
                        [DataMember]
                        public string comments { get; set; }

                ******************/

                string fStr = "Start";
                try
                {
                    fStr = "Priority";
                    task.Priority = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskPriority").Value);
                    fStr = "Title must be filled";
                    string title = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskTitle").Value).Trim();
                    if (title.Length == 0)
                        throw new ArgumentNullException("bad title");
                    taskData.Title = task.Description = title;
                    fStr = "Task Id";
                    taskData.tid = task.Id = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
                    fStr = "Parent Task Id";
                    string ptid = this.Request.Form.FirstOrDefault(kv => kv.Key == "parentTaskId").Value;
                    task.ParentTaskId = ((ptid.Length > 0) ? int.Parse(ptid) : (int ?)null);
                    fStr = "Task Type";
                    task.Type = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskType").Value);
                    fStr = "Person Id";
                    task.PersonId = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskPersonId").Value);
                    if (!task.PersonId.HasValue || (task.PersonId <= 0))
                        task.PersonId = null;
                    var oldTask = _context.Task.AsNoTracking().FirstOrDefaultAsync(t => (t.Id == task.Id) && (((int)t.Type & (int)eTaskType.AutoPersonID) != 0)).Result;
                    if (oldTask != null)
                    {
                        if (task.PersonId != oldTask.PersonId)
                            task.Type &= ~(int)eTaskType.AutoPersonID; // Remove Auto Person state since user change person.
                    }

                    fStr = "Committed Cost";
                    String ccostStr = this.Request.Form.FirstOrDefault(kv => kv.Key == "taskCommittedCost").Value;
                    if (ccostStr.Trim().Length > 0)
                        task.CommittedCost = decimal.Parse(ccostStr);
                    else
                        task.CommittedCost = 0;
                    fStr = "Total Cost";
                    String tcostStr = this.Request.Form.FirstOrDefault(kv => kv.Key == "taskTotalCost").Value;
                    if (tcostStr.Trim().Length > 0)
                        task.TotalCost = decimal.Parse(tcostStr);
                    else
                        task.TotalCost = 0;

                    IsTemplate = (task.Type & (int)CTask.eTaskType.Template) != 0;
                    if (IsTemplate)
                    {
                        //*******************************************************
                        //************** Template ****************************
                        //*******************************************************

                        fStr = "Template Task";

                        // Make sure to initialize ABOVE and also get these HERE only for a Template Task.
                        int dueType = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskCreateDue").Value);
                        if (dueType != 0)
                        {
                            // Get Template Type

                            int dueValue = 0;

                            if ((dueType == (int)CTask.eTaskType.Hours_Before_Start) ||
                                (dueType == (int)CTask.eTaskType.Hours_After_Start) ||
                                (dueType == (int)CTask.eTaskType.Hours_Before_End) ||
                                (dueType == (int)CTask.eTaskType.Hours_After_End) ||
                                (dueType == (int)CTask.eTaskType.Days_Before_Start) ||
                                (dueType == (int)CTask.eTaskType.Days_After_Start) ||
                                (dueType == (int)CTask.eTaskType.Days_Before_End) ||
                                (dueType == (int)CTask.eTaskType.Days_After_End) ||
                                (dueType == (int)CTask.eTaskType.Weeks_Before_Start) ||
                                (dueType == (int)CTask.eTaskType.Weeks_After_Start) ||
                                (dueType == (int)CTask.eTaskType.Weeks_Before_End) ||
                                (dueType == (int)CTask.eTaskType.Weeks_After_End) ||
                                (dueType == (int)CTask.eTaskType.Months_Before_Start) ||
                                (dueType == (int)CTask.eTaskType.Months_After_Start) ||
                                (dueType == (int)CTask.eTaskType.Months_Before_End) ||
                                (dueType == (int)CTask.eTaskType.Months_After_End) ||
                                (dueType == (int)CTask.eTaskType.Day_Of_Week_SunMon))
                            {
                                dueValue = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskCreateDueVal").Value);
                            }
                            else if (dueType == (int)CTask.eTaskType.Day_Of_Week_SunMon)
                            {
                                String dueStr = this.Request.Form.FirstOrDefault(kv => kv.Key == "taskCreateDueVal").Value;
                                if (dueStr.Length > 0)
                                {
                                    dueStr = dueStr.Trim().ToLower();
                                    if (dueStr.IndexOf("su") == 0)
                                        dueValue = 1;
                                    else if ((dueStr.IndexOf("m") == 0))
                                        dueValue = 2;
                                    else if ((dueStr.IndexOf("tu") == 0))
                                        dueValue = 3;
                                    else if ((dueStr.IndexOf("w") == 0))
                                        dueValue = 4;
                                    else if ((dueStr.IndexOf("th") == 0))
                                        dueValue = 5;
                                    else if ((dueStr.IndexOf("f") == 0))
                                        dueValue = 6;
                                    else if ((dueStr.IndexOf("sa") == 0))
                                        dueValue = 7;
                                    else
                                        return this.Content("Fail: Week Day needs to start with M, Tu, W, Th, Fr, Sa or Su"); // Send back results
                                }
                            }
                            int eachType = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskCreateEach").Value);

                            if (eachType == (int)CTask.eTaskType.Day_from_Due_till)
                            {
                                String TillDayStr = this.Request.Form.FirstOrDefault(kv => kv.Key == "TillDay").Value;
                                if (TillDayStr.Trim().Length > 0)
                                {
                                    bool TillFail = true;
                                    String[] numStr = TillDayStr.Split('/', '.', '-');
                                    if (numStr.Count() == 2)
                                    {
                                        if (Int32.TryParse(numStr[0], out int TillMo) && Int32.TryParse(numStr[1], out int TillDay))
                                        {
                                            try
                                            {
                                                task.EstimatedDoneDate = new DateTime(2020, TillMo, TillDay);
                                            }
                                            catch { }
                                            TillFail = !task.EstimatedDoneDate.HasValue;
                                        }
                                        if (TillFail)
                                            return this.Content("Fail: Day_from_Due_till needs to have a valid Month/Day with a / . or - separator"); // Send back results
                                    }
                                }
                            }

                            if ((eachType == (int)CTask.eTaskType.Due_Day) ||
                                (eachType == (int)CTask.eTaskType.Months_Period_from_Date) ||
                                (eachType == (int)CTask.eTaskType.Years_Period_from_Date)
                                )
                            {
                                string mdStr = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskDueDate").Value).Trim();
                                if (mdStr.Length >= 10)
                                {
                                    string dateOnly = mdStr.Substring(0, 10);
                                    task.DueDate = DateTime.ParseExact(dateOnly, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
                                    task.DueDate = new DateTime(task.DueDate.Value.Year, task.DueDate.Value.Month, task.DueDate.Value.Day, 23, 59, 0); // Give to end of day for these cases.
                                }
                                else
                                    return this.Content("Fail: A valid Due Date must be set with Every : Month n Day"); // Send back results
                            }
                            task.SetTaskType((CTask.eTaskType)dueType, (CTask.eTaskType)eachType, dueValue);
                        }

                        var moys = this.Request.Form.FirstOrDefault(kv => kv.Key == "NumMOYs").Value;
                        if (!string.IsNullOrEmpty(moys))
                            task.Status = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "NumMOYs").Value); // kludge for template to use Status for # of Months or Years

                        fStr = "Role123";
                        taskData.Role1 = long.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskRole1").Value);
                        taskData.Role2 = long.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskRole2").Value);
                        taskData.Role3 = long.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskRole3").Value);
                    }
                    else
                    {
                        //*******************************************************
                        //************** Normal Task ****************************
                        //*******************************************************
                        fStr = "% Comp";

                        uint pctComp = uint.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "pctComp").Value);
                        fStr = "Status";
                        ulong status = ulong.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskState").Value);
                        fStr = "Reason";
                        ulong reason = ulong.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskReason").Value);
                        task.SetTaskStatus((CTask.eTaskStatus)status, (CTask.eTaskStatus)reason, (int)pctComp);
                        taskData.state = (int)status;
                        taskData.reason = (int)reason;
                        taskData.PercentComplete = (int)pctComp;
                    }

                    fStr = "Task Doc";
                    task.PlanLink = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskDoc").Value);
                    fStr = "Detail Text";
                    taskData.Detail = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskDetail").Value);
                    fStr = "Comments";
                    taskData.comments = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskComments").Value);
                    fStr = "Pics";
                    string[] pics = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(this.Request.Form.FirstOrDefault(kv => kv.Key == "pics").Value);
                    fStr = "Pic Titles";
                    string[] picTitles = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(this.Request.Form.FirstOrDefault(kv => kv.Key == "picTitles").Value);
                    if (pics.Length == picTitles.Length)
                    {
                        for (int i = 0; i < pics.Length; ++i)
                        {
                            Pic pobj = new Pic(task.Id, i + 1, CCommon.UnencodeQuotes(picTitles[i]), pics[i]);
                            taskData.pics.Add(pobj);
                        }
                    }
                    else
                    {
                        return this.Content("Fail: Pics=" + pics.Length + " DOES NOT MATCH Pic Titles=" + picTitles.Length); // Send back results
                    }

                    fStr = "Costs /BOM";
                    var sBOM = this.Request.Form.FirstOrDefault(kv => kv.Key == "bom").Value;
                    taskData.bom = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BOMLine>>(sBOM);

                    fStr = "Due/Est.Done Date";
                    string msDueStr = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskDueDate").Value).Trim();
                    DateTime dueDateTime = new DateTime();
                    if (msDueStr.Length > 0)
                    {
                        //dueDateTime = DateTime.ParseExact(msDueStr, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
                        dueDateTime = DateTime.Parse(msDueStr, System.Globalization.CultureInfo.CurrentCulture);
                    }
                    taskData.FixedDueDate = bool.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "fixedDueDate").Value);
                    if (taskData.FixedDueDate)
                    {
                        if (msDueStr.Length > 0)
                        {
                            task.DueDate = dueDateTime;
                        }
                        else
                            task.DueDate = null;
                        if (!IsTemplate)
                            task.EstimatedDoneDate = null;
                    }
                    else
                    {
                        if (!IsTemplate)
                        {
                            if (msDueStr.Length > 0)
                                task.EstimatedDoneDate = dueDateTime;
                            else
                                task.EstimatedDoneDate = null;
                            task.DueDate = null;
                        }
                    }
                    fStr = "Completed Date";
                    string msDoneStr = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskDoneDate").Value).Trim();
                    if (msDoneStr.Length > 0)
                    {
                        int slashIdx = msDoneStr.IndexOf("/");
                        if ((slashIdx > 0) && (slashIdx <= 2))
                        {
                            task.ActualDoneDate = DateTime.ParseExact(msDoneStr, "M/d/yyyy h:mm:ss tt", System.Globalization.CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            long msSinceUXEpoc = long.Parse(msDoneStr);
                            task.ActualDoneDate = DateTimeOffset.FromUnixTimeMilliseconds(msSinceUXEpoc).DateTime.ToLocalTime();
                        }
                    }
                    else
                    {
                        task.ActualDoneDate = null;
                    }

                    task.EstimatedManHours = 0;
                    task.RequiredSkills = "";

                    if (IsTemplate)
                    {
                        // Remove exist non-done tasks so new ones can be created without task multiplicity
                        Models.Task.RemoveActiveChildTasks(_context, task.Id);
                    }

                    fStr = "Save File Update";
                    taskData.Write(hostEnv);
                    fStr = "DB Update";
                    _context.Attach(task).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.SaveChanges();

                    if (IsTemplate)
                    {
                        // Generate possibly new Tasks based on template add/changes
                        AutoGen ag = new AutoGen(_context);
                        ag.GenTasks(hostEnv, task.Id);
                    }

                    if (dueDateTime != DateTime.MinValue)
                        NotifyUserTaskDue(hostEnv, _config, _userManager, _context, 24); // Send out notications if needed.
                }
                catch(Exception e)
                {
                    return this.Content("Fail: " + fStr + " e.msg=" + e.Message); // Send back results
                }

                return this.Content("Success:" + task.Id); // Send back results
            }
            return this.Content("Fail: Bad/Missing Request"); // Send back results
        }

        public ActionResult OnPostClean()
        {
            if ((this.Request == null) || (this.Request.Form == null))
                return this.Content("Fail: Bad/Missing Request"); // Send back results

            try
            {
                string title = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskTitle").Value).Trim();
                if (title.Length != 0)
                    return this.Content("Fail: Has Title"); // Send back results

                string pidStr = this.Request.Form.FirstOrDefault(kv => kv.Key == "taskPersonId").Value;
                if (pidStr != null)
                {
                    int? pid = int.Parse(pidStr);
                    if (pid.HasValue && (task.PersonId > 0))
                        return this.Content("Fail: Assigned"); // Send back results
                }

                int id = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);

                if (id <= 0)
                {
                    return this.Content("Fail: No id"); // Send back results
                }

                // Delete empty task
                var Task = _context.Task.Find(id);

                if (Task != null)
                {
                   _context.Task.Remove(Task);
                   _context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return this.Content("Fail: e.msg=" + e.Message); // Send back results
            }

            return this.Content("Success:"); // Send back results
        }

        public ActionResult OnPostExport()
        {
            if ((this.Request == null) || (this.Request.Form == null))
                return this.Content("Fail: Bad/Missing Request"); // Send back results

            try
            {
                int id = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);

                if (id <= 0)
                {
                    return this.Content("Fail: No id"); // Send back results
                }

                var ft = CTask.GetFullTask(_context, hostEnv, id);
                CTask.CreateTaskReport(ft, "c:\\rr\\Task1.pdf");

            }
            catch (Exception e)
            {
                return this.Content("Fail: e.msg=" + e.Message); // Send back results
            }

            return this.Content("Success:"); // Send back results
        }


        public ActionResult OnPostPingCTask()
        {
            return this.Content("Success:");  // Response 
        }

    }
}
