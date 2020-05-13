using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using CTask = CStat.Models.Task;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Net.Http.Headers;
using Org.BouncyCastle.Ocsp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Runtime.Serialization;

namespace CStat.Pages.Tasks
{
    [DataContract]
    public class Pic
    {
        public Pic (int taskID, int picID, string desc)
        {
            taskId = taskID;
            picId = picID;
            title = desc;
        }
        [DataMember]
        public int taskId { get; set; }
        [DataMember]
        public int picId { get; set; }
        [DataMember]
        public string title { get; set; }
        public string GetFileName (string srcFile)
        {
            string ext = Path.GetExtension(srcFile);
            return "Img_" + taskId + "_" + picId + ext;
        }
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

        public TaskData(int taskId=-1)
        {
            tid = taskId;
            state = 0;
            reason = 0;
            PercentComplete = 0;
            Desc = "";
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

        public bool Write (IWebHostEnvironment hstEnv)
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

        [BindProperty]
        public TaskData taskData { get; set; }

        [BindProperty]
        public CTask task { get; set; }

        public CreateModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            hostEnv = hstEnv;
        }

        //SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS

        //[DataContract]
        //class BlogSite
        //{
        //    [DataMember]
        //    public string Name { get; set; }

        //    [DataMember]
        //    public string Description { get; set; }
        //}
        //private void TestJSon()
        //{
        //    BlogSite bsObj = new BlogSite()
        //    {
        //        Name = "C-sharpcorner",
        //        Description = "Share Knowledge"
        //    };

        //    DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(BlogSite));
        //    MemoryStream msObj = new MemoryStream();
        //    js.WriteObject(msObj, bsObj);
        //    msObj.Position = 0;
        //    StreamReader sr = new StreamReader(msObj);

        //    // "{\"Description\":\"Share Knowledge\",\"Name\":\"C-sharpcorner\"}"  
        //    string json1 = sr.ReadToEnd();

        //    sr.Close();
        //    msObj.Close();

        //    //============================

        //    string json = "{\"Description\":\"Share Knowledge\",\"Name\":\"C-sharpcorner\"}";

        //    using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
        //    {
        //        // Deserialization from JSON  
        //        DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(BlogSite));
        //        BlogSite bsObj2 = (BlogSite)deserializer.ReadObject(ms);
        //        //Response.Write("Name: " + bsObj2.Name); // Name: C-sharpcorner
        //        //Response.Write("Description: " + bsObj2.Description); // Description: Share Knowledge  
        //    }

        //}
        //EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE

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
            //task.
            //task.
            //task.
            //task.
            //task.
            //task.
            //task.
            //task.

            taskData = new TaskData(task.Id)
            {
                state = (int)CTask.eTaskStatus.Paused,
                reason = (int)CTask.eTaskStatus.Need_Funds,
                Desc = "The same came to Jesus by night, and said unto him, Rabbi, we know that thou art a teacher come from God: for no man can do these miracles that thou doest, except God be with him.             Jesus answered and said unto him, Verily, verily, I say unto thee, Except a man be born again, he cannot see the kingdom of God. Nicodemus saith unto him, How can a man be born when he is old ? can he enter the second time into his mother's womb, and be born? Jesus answered, Verily, verily, I say unto thee, Except a man be born of water and of the Spirit, he cannot enter into the kingdom of God. That which is born of the flesh is flesh; and that which is born of the Spirit is spirit. Marvel not that I said unto thee, Ye must be born again. The wind bloweth where it listeth, and thou hearest the sound thereof, but canst not tell whence it cometh, and whither it goeth: so is every one that is born of the Spirit."
            };

            Pic pic = new Pic(126, 1, "This is the title description for Pic 1");
            taskData.pics.Add(pic);
            pic = new Pic(126, 2, "This is the title description for Pic 22");
            taskData.pics.Add(pic);
            pic = new Pic(126, 3, "This is the title description for Pic 333");
            taskData.pics.Add(pic);

            taskData.Write(hostEnv);

            TaskData tsRead = TaskData.ReadTaskData(hostEnv, task.Id);

            //Not_Started = 0x00000080,
            //Planning = 0x00000100,
            //Ready = 0x00000200,
            //Active = 0x00000400,
            //Paused = 0x00000800,
            //Completed = 0x00001000,

            ////Reason
            //Need_Funds = 0x00100000,
            //Need_Labor = 0x00200000,
            //Need_Material = 0x00400000,
            //Need_Inspection = 0x00800000,
            //Need_Planning = 0x01000000

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

        public ActionResult OnPostSend()
        {
            if ((this.Request != null) && (this.Request.Form != null))
            {
                var files = this.Request.Form.Files;
                string PicTitle = CCommon.UnencodeQuotes(this.Request.Form.FirstOrDefault(kv => kv.Key == "picTitle").Value);
                int imgTID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
                int imgPID = Int32.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "picId").Value);

                if (files != null && (files.Count == 1) && (imgTID >=0))
                {
                    string folderName = @"Tasks\Images";
                    string webRootPath = hostEnv.WebRootPath;
                    Pic pic = new Pic (imgTID, imgPID, PicTitle);
                    string newPath = Path.Combine(webRootPath, folderName);
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    foreach (IFormFile item in files)
                    {
                        if (item.Length > 0)
                        {
                            string fileName = ContentDispositionHeaderValue.Parse(item.ContentDisposition).FileName.Trim('"');
                            string fullPath = Path.Combine(newPath, pic.GetFileName(fileName));
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                item.CopyTo(stream);
                            }
                        }
                    }
                    return this.Content("Success");
                }
            }
            return this.Content("Fail");
        }
    }
}
