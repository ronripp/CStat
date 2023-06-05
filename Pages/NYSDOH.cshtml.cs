using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using static CStat.Models.Task;

namespace CStat.Pages
{
    public class NYSDOHModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;
        private string _usage = "default";

        public NYSDOHModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            _hostEnv = hstEnv;
        }

        public IList<CTask> Task { get;set; }

        public IActionResult OnGet(string usage)
        {
            if (!string.IsNullOrEmpty(usage))
                _usage = usage;

            return Page();
        }

        public string TaskPDF(int tid)
        {
            string webRootPath = _hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "_NYSDOH");
            var ft = CTask.GetFullTask(_context, _hostEnv, tid);
            string basePDF = "Task" + ft.task.Id.ToString() + ".pdf";
            string pdfFile = Path.Combine(newPath, basePDF);
            if (_usage == "staffOct22zs")
            {
                var task = _context.Task.Where(t => t.Id == tid).FirstOrDefault();
                task.GetTaskStatus(out eTaskStatus state, out eTaskStatus reason, out int PercentComplete);
                string tc = "TI";
                if ((state & eTaskStatus.Completed) != 0)
                    tc = "TC";

                return "<button onclick=\"window.location.href='/Tasks/Create?id=" + tid + "';\" class=\"" + tc + "\">" + "Task " + tid + "</button></a>";
            }
            else
            {
                if (System.IO.File.Exists(pdfFile))
                    return "<a href=\"_NYSDOH/" + basePDF + "\" download><button class=\"TC\">" + "Task " + tid + "</button></a>";
                else
                    return "<div style=\"color:darkred; font-weight:bold;\">[Pending]</div>";
            }
        }

    }
}
