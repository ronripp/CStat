using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    [DataContract]
    public class Pic
    {
        public Pic(int taskID, int picID, string desc, string urL="")
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
        public TaskData(int taskId = -1)
        {
            tid = taskId;                               // These top 4 memmbers are redundant but they must match DB Record
            state = (int)CTask.eTaskStatus.Not_Started; 
            reason = (int)CTask.eTaskStatus.Unknown;    
            PercentComplete = 0;                        
            Title = "";
            Detail = "";
            pics = new List<Pic>();
            bom = new List<BOMLine>();
            comments = "";
            FixedDueDate = false;
        }

        public static TaskData ReadTaskData(IWebHostEnvironment hstEnv, int taskId)
        {
            TaskData taskData = null;
            string folderName = @"Tasks";
            string webRootPath = hstEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            string fullPath = Path.Combine(newPath, "Task_" + taskId.ToString() + ".json");
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

    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;

        public TaskData taskData { get; set; }

        [BindProperty]
        public CTask task { get; set; }

        public CreateModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            hostEnv = hstEnv;
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
            taskData = new TaskData();
        }

        public IActionResult OnGet(int? id)
        {
             int tid = !id.HasValue ? tid = 0 : id.Value;

            int res = 0;
            if (tid == 0)
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
                    taskData = TaskData.ReadTaskData(hostEnv, tid);
                    if (task.DueDate == null)
                        task.DueDate = task.EstimatedDoneDate; // temporary for editing purpose
                }
                else
                    InitializeTask(tid);
            }

            IList<SelectListItem> sList = Enum.GetValues(typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e < (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["State"] = sList;
            IList<SelectListItem> rList = Enum.GetValues(typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e >= (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Reason"] = rList;
            IList<SelectListItem> pList = Enum.GetValues(typeof(CTask.ePriority)).Cast<CTask.ePriority>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Priority"] = pList;

            ViewData["Blocking1Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["Blocking2Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");

            IList<SelectListItem> personList = _context.Person.OrderBy(p => p.LastName).ThenBy(p1 => p1.FirstName).Select(p2 => new SelectListItem { Text = p2.FirstName + " " + p2.LastName, Value = p2.Id.ToString() } ).ToList<SelectListItem>();
            personList.Insert(0, new SelectListItem(" ", "-1"));
            ViewData["PersonId"] = personList;

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

                string fStr = "% Comp";
                try
                {
                    uint pctComp = uint.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "pctComp").Value);
                    fStr = "Status";
                    ulong status = ulong.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskState").Value);
                    fStr = "Reason";
                    ulong reason = ulong.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskReason").Value);
                    fStr = "Priority";
                    task.Priority = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskPriority").Value);
                    task.SetTaskStatus((CTask.eTaskStatus)status, (CTask.eTaskStatus)reason, (int)pctComp);
                    taskData.state = (int)status;
                    taskData.reason = (int)reason;
                    taskData.PercentComplete = (int)pctComp;
                    fStr = "Title must be filled";
                    string title = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskTitle").Value).Trim();
                    if  (title.Length == 0)
                        throw new ArgumentNullException("bad title");
                    taskData.Title = task.Description = title;
                    fStr = "Task Id";
                    taskData.tid = task.Id = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
                    fStr = "Person Id";
                    task.PersonId = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskPersonId").Value);
                    if (!task.PersonId.HasValue || (task.PersonId <= 0))
                        task.PersonId = null;
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
                        for (int i = 0; i < pics.Length; ++ i)
                        {
                            Pic pobj = new Pic(task.Id, i+1, picTitles[i], pics[i]);
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
                        dueDateTime = DateTime.ParseExact(msDueStr, "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
                    }
                    taskData.FixedDueDate = bool.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "fixedDueDate").Value);
                    if (taskData.FixedDueDate)
                    {
                        if (msDueStr.Length > 0)
                            task.DueDate = dueDateTime;
                        else
                            task.DueDate = null;
                        task.EstimatedDoneDate = null;
                    }
                    else
                    {
                        if (msDueStr.Length > 0)
                            task.EstimatedDoneDate = dueDateTime;
                        else
                            task.EstimatedDoneDate = null;
                        task.DueDate = null;
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

                    fStr = "Save File Update";
                    taskData.Write(hostEnv);
                    fStr = "DB Update";
                    _context.Attach(task).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    _context.SaveChanges();
                }
                catch(Exception e)
                {
                    return this.Content("Fail: " + fStr + " e.msg=" + e.Message); // Send back results
                }

                return this.Content("Success:" + task.Id); // Send back results
            }
            return this.Content("Fail: Bad/Missing Request"); // Send back results
        }
    }
}
