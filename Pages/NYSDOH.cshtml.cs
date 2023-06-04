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

namespace CStat.Pages
{
    public class NYSDOHModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;

        public NYSDOHModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            _hostEnv = hstEnv;
        }

        public IList<CTask> Task { get;set; }

        public async Task OnGetAsync()
        {

        }

        public string TaskPDF(int tid)
        {
            string webRootPath = _hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "_NYSDOH");
            var ft = CTask.GetFullTask(_context, _hostEnv, tid);
            string basePDF = "Task" + ft.task.Id.ToString() + ".pdf";
            string pdfFile = Path.Combine(newPath, basePDF);
            if (System.IO.File.Exists(pdfFile))
                return "<a href=\"_NYSDOH/" + basePDF + "\" download><button class=\"TC\">" + "Task " + tid + "</button></a>";
            else
                return "<div style=\"color:darkred; font-weight:bold;\">[Pending]</div>";
        }

    }
}
