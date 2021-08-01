using Dropbox.Api.TeamLog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using CStat.Models;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;
using CStat.Common;

namespace CStat.Pages.Tasks
{
    public class AutoGen
    {
        private readonly CStat.Models.CStatContext _context;

        public AutoGen(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public bool GenTasks (IWebHostEnvironment hstEnv, int tmplId = -1)
        {
            // Get a List of Template Tasks

            var TemplateTasks = (tmplId != -1) ? _context.Task.Include(t => t.Person).Where(t => ((t.Type & (int)CTask.eTaskType.Template) != 0) && (t.Id == tmplId)).ToList() :
                                                 _context.Task.Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();

            List<CTask> taskList = new List<CTask>();

            // Determine what needs to be Auto generated
            foreach (var tmpl in TemplateTasks)
            {
                List<CTask> curCreatedTasks = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

                tmpl.GetTaskType(out CTask.eTaskType dueType, out CTask.eTaskType eachType, out int dueVal);
                DateTime now = PropMgr.ESTNow;
                DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime endTime = startTime.AddMonths(6);
                //DateTime endTime = new DateTime(startTime.Year+1, 12, 31, 23, 59, 59);
                DateRange lim = new DateRange(startTime, endTime);
                List<Event> evList = GetEvents(lim);
                List<CTask> newCreatedTasks = GetTemplateTasks(hstEnv, _context, tmpl, evList, lim).Where(n => !curCreatedTasks.Any(c => ((c.ParentTaskId == n.ParentTaskId) || (c.Description == n.Description)) && (c.DueDate == n.DueDate))).ToList();

                if (taskList.Count > 0)
                {
                    TaskData taskData = TaskData.ReadTaskData(hstEnv, tmpl.Id, -1);
                    foreach (var t in taskList)
                    {
                        if (!tmpl.PersonId.HasValue || (tmpl.PersonId <= 0))
                        {
                            // Determine person from roles
                            var pid = CTask.ResolvePersonId(_context, taskData.Role1, taskData.Role2, taskData.Role3);
                            if (pid.HasValue && (pid > 0))
                            {
                                t.PersonId = pid;
                                t.SetType(CTask.eTaskType.AutoPersonID);
                            }
                        }
                    }
                }
                // Filter out tasks that already exist.
                taskList.AddRange(newCreatedTasks);
            }

            // Search for any created tasks that are assigned to a person who is now unavailable
            List<CTask> unavailTasks = _context.Task.Include(t => t.Person).Where(t => t.ParentTaskId.HasValue && (t.ParentTaskId.Value > 0) && t.PersonId.HasValue && t.Person.Roles.HasValue && (((long)t.Person.Roles & (long)Person.TitleRoles.Unavailable) != 0)).ToList();
            foreach (var ut in unavailTasks)
            {
                TaskData utd = TaskData.ReadTaskData(hstEnv, ut.ParentTaskId.Value, -1);
                ut.PersonId =  CTask.ResolvePersonId(_context, utd.Role1, utd.Role2, utd.Role3); // Reassign from the unavailable person to someone else
                _context.Attach(ut).State = EntityState.Modified;
                try
                {
                    _context.SaveChanges();
                }
                catch { }
            }
    
           // TBD: Maintain AutoPersonID state properly.

            // Add needed tasks to DB
            foreach (var t in taskList)
            {
                _context.Task.Add(t);
                _context.SaveChanges();
            }
            return true;
        }
        public List<Event> GetEvents(DateRange range)
        {
            return _context.Event.Where(e => (e.StartTime >= range.Start) && (e.EndTime <= range.End)).ToList();
        }
        public List<CTask> GetTemplateTasks(IWebHostEnvironment hstEnv, CStat.Models.CStatContext context, CTask tmpl, List<Event> events, DateRange lim)
        {
            List<CTask> taskList = new List<CTask>();

            if (tmpl.GetDueDates(events, out List<DateTimeEv> dueDateEvs, lim))
            {
                foreach (var dueDateEv in dueDateEvs)
                {
                    // Create new task and add it to the list
                    CTask task = new CTask();
                    task.ParentTaskId = tmpl.Id;
                    task.Type = 0;
                    task.Description = tmpl.Description;
                    task.PlanLink = tmpl.PlanLink;
                    task.EstimatedManHours = tmpl.EstimatedManHours;
                    task.Id = 0;
                    task.PersonId = tmpl.PersonId;
                    task.TotalCost = tmpl.TotalCost;
                    task.EstimatedDoneDate = new DateTime(1900, 1, 1);
                    task.CommittedCost = 0;
                    task.DueDate = dueDateEv.dt;
                    task.EventId = dueDateEv.eventID;
                    task.SetTaskStatus(CTask.eTaskStatus.Not_Started, CTask.eTaskStatus.Unknown, 0);
                    task.Priority = (int)tmpl.Priority;
                    task.RequiredSkills = tmpl.RequiredSkills;
                    taskList.Add(task);
                }
            }

            return taskList;
        }
    }
}

