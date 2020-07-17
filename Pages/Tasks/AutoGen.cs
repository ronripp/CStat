using Dropbox.Api.TeamLog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    public class AutoGen
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;


        public AutoGen(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            hostEnv = hstEnv;
        }

        public List<CTask> GenTasks ()
        {
            // Get a List of Template Tasks
            var task = _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();


            // Determine what needs to be Auto generated

            // Filter out existing Auto generated Tasks having a parent task id that matches template task id and due date.

            // Generate active tasks 

            return null;
        }

    }
}
