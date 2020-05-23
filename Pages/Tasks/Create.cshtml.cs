using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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

        public TaskData(int taskId = -1)
        {
            tid = taskId;
            state = 0;
            reason = 0;
            PercentComplete = 30;
            Desc = "";
            pics = new List<Pic>();
            bom = new List<BOMLine>();
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
            using (StreamReader r = new StreamReader(fullPath))
            {
                string json = r.ReadToEnd();
                var ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(TaskData));
                taskData = (TaskData)deserializer.ReadObject(ms);
                taskData.tid = taskId;
            }
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

        private TaskData CreateTestTaskData ()
        {
            TaskData tData = new TaskData(task.Id)
            {
                state = (int)CTask.eTaskStatus.Paused,
                reason = (int)CTask.eTaskStatus.Need_Funds,
                Desc = "The same came to Jesus by night, and said unto him, Rabbi, we know that thou art a teacher come from God: for no man can do these miracles that thou doest, except God be with him.             Jesus answered and said unto him, Verily, verily, I say unto thee, Except a man be born again, he cannot see the kingdom of God. Nicodemus saith unto him, How can a man be born when he is old ? can he enter the second time into his mother's womb, and be born? Jesus answered, Verily, verily, I say unto thee, Except a man be born of water and of the Spirit, he cannot enter into the kingdom of God. That which is born of the flesh is flesh; and that which is born of the Spirit is spirit. Marvel not that I said unto thee, Ye must be born again. The wind bloweth where it listeth, and thou hearest the sound thereof, but canst not tell whence it cometh, and whither it goeth: so is every one that is born of the Spirit."
            };

            Pic pic = new Pic(126, 1, "This is the title description for Pic 1", "Images/Img_126_1.jpg");
            tData.pics.Add(pic);
            pic = new Pic(126, 2, "This is the title description for Pic 22", "Images/Img_126_2.jpg");
            tData.pics.Add(pic);
            pic = new Pic(126, 3, "This is the title description for Pic 333", "Images/Img_126_3.jpg");
            tData.pics.Add(pic);
            return tData;
        }

        public IActionResult OnGet()
        {
            task = new CTask();
            task.Description = "Fix Miller House Shed";
            task.EstimatedManHours = 24;
            task.Id = 126;
            task.PersonId = 1632;
            task.TotalCost = 562;
            task.EstimatedDoneDate = new DateTime(2020, 8, 15);
            task.CommittedCost = 385;
            task.SetTaskStatus(CTask.eTaskStatus.Paused, CTask.eTaskStatus.Need_Funds, 60);
            task.Priority = (int)CTask.ePriority.High;
            taskData = CreateTestTaskData();

            IList<SelectListItem> sList = Enum.GetValues(typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e < (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["State"] = sList;
            IList<SelectListItem> rList = Enum.GetValues(typeof(CTask.eTaskStatus)).Cast<CTask.eTaskStatus>().Where(e => (int)e >= (int)CTask.eTaskStatus.Need_Funds).Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Reason"] = rList;
            IList<SelectListItem> pList = Enum.GetValues(typeof(CTask.ePriority)).Cast<CTask.ePriority>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["Priority"] = pList;

            ViewData["Blocking1Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["Blocking2Id"] = new SelectList(_context.Task, "Id", "Description");
            ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");
            ViewData["PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
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

        public ActionResult OnPostSend()
        {
            if ((this.Request != null) && (this.Request.Form != null))
            {
                var files = this.Request.Form;
                string PicTitle = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "picTitle").Value);
                int imgTID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
                int imgPID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "picId").Value);

                return this.Content("Success:");  // Send back URL
            }
            return this.Content("Fail");
        }

    }
}
