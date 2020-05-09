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

namespace CStat.Pages.Tasks
{
    public class TaskData
    {
        public int state;
        public int reason;
        public int PercentComplete;
        public string FullDescription;
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

            taskData = new TaskData();
            taskData.state = (int)CTask.eTaskStatus.Paused;
            taskData.reason = (int)CTask.eTaskStatus.Need_Funds;


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

                //SSSSSSSSSSSSSS

                //var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
                //var idx = rawQS.IndexOf('{');
                //if (idx == -1)
                //    return new JsonResult("ERROR~:No Parameters");
                //var jsonQS = rawQS.Substring(idx);
                //Dictionary<string, string> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);

                //EEEEEEEEEEEEEE

                var kvp = this.Request.Form.First();
                String title = kvp.Value;

                if (files != null && files.Count > 0)
                {
                    string folderName = @"Tasks\Images";
                    string webRootPath = hostEnv.WebRootPath;
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
                            string fullPath = Path.Combine(newPath, fileName);
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
