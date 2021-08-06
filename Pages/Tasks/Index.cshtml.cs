using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;
using System.Runtime.CompilerServices;
using CStat.Common;

namespace CStat.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;

        public bool IsTemplate = false;

        public IndexModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
             _context = context;
             hostEnv = hstEnv;
        }

        public IList<CTask> Task { get;set; }

        public async Task OnGetAsync(bool showTemplates = false)
        {
            IsTemplate = showTemplates;

            if (!IsTemplate)
            {
                Task = await _context.Task
                    .Include(t => t.Blocking1)
                    .Include(t => t.Blocking2)
                    .Include(t => t.Church)
                    .Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) == 0) && ((t.Type & (int)CTask.eTaskType.Template) == 0)).OrderBy(t => (t.DueDate.HasValue ? t.DueDate.Value : new DateTime(2020,1,1))).ThenBy(t => t.Priority).ToListAsync();

                IList<CTask> CompTasks = await _context.Task
                    .Include(t => t.Blocking1)
                    .Include(t => t.Blocking2)
                    .Include(t => t.Church)
                    .Include(t => t.Person).Where(t => ((t.Status & (int)CTask.eTaskStatus.Completed) != 0) && ((t.Type & (int)CTask.eTaskType.Template) == 0)).OrderByDescending(t => t.ActualDoneDate).ToListAsync();

                foreach (var ct in CompTasks) // Task = Task.Concat(CompTasks);
                {
                    Task.Add(ct);
                }

                // TBD : Add AutoGen

                // TBD : Set Normal(Green) Warning(Yellow),  Alert(Red) state
            }
            else
            {
                Task = await _context.Task
                    .Include(t => t.Blocking1)
                    .Include(t => t.Blocking2)
                    .Include(t => t.Church)
                    .Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToListAsync();
            }
        }
        public ActionResult OnPostAutoGen()
        {
            // Generate possibly new Tasks based on template add/changes
            AutoGen ag = new AutoGen(_context);
            ag.GenTasks(hostEnv);
            return this.Content("Success");
        }

        public ActionResult OnPostMarkComplete()
        {
            int taskId = int.Parse(this.Request.Form.FirstOrDefault(kv => kv.Key == "taskId").Value);
            if (taskId > 0)
            {
                var taskData = TaskData.ReadTaskData(hostEnv, taskId, -1);
                CTask task = _context.Task.First(t => (t.Id == taskId));
                if ((task != null) && (taskData != null))
                {
                    taskData.PercentComplete = 100;
                    taskData.Write(hostEnv);
                    task.GetTaskStatus(out CTask.eTaskStatus status, out CTask.eTaskStatus reason, out int pctComp);
                    pctComp = 100;
                    status = CTask.eTaskStatus.Completed;
                    reason = CTask.eTaskStatus.Job_Done;
                    task.SetTaskStatus((CTask.eTaskStatus)status, (CTask.eTaskStatus)reason, (int)pctComp);
                    try
                    {
                        taskData.Write(hostEnv);
                        _context.Attach(task).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        _context.SaveChanges();
                        return this.Content("Success");
                    }
                    catch { }
                }
            }
            return this.Content("Failed");
        }
    }

}
