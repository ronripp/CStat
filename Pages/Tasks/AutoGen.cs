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
            // Get a List of Template Tasks for a specific template id(when template task is changed) ...or... ALL template tasks (tmplId = -1) when looking to generate tasks in general.
            var TemplateTasks = (tmplId != -1) ? _context.Task.Include(t => t.Person).Where(t => ((t.Type & (int)CTask.eTaskType.Template) != 0) && (t.Id == tmplId)).ToList() :
                                                 _context.Task.Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();

            // Get the range limit of time to possibly create new tasks and a list of events that fully or partially overlap this range limit of time
            DateTime now = PropMgr.ESTNow;
            DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            DateTime endTime = startTime.AddMonths(6);
            //DateTime endTime = new DateTime(startTime.Year+1, 12, 31, 23, 59, 59);
            DateRange lim = new DateRange(startTime, endTime);
            List<Event> evList = GetEvents(lim);

            List<CTask> AllNewTaskList = new List<CTask>(); // Create a list for collecting all potentially new tasks created for all templates in TemplateTasks

            // Determine what needs to be Auto generated
            foreach (var tmpl in TemplateTasks)
            {
                // Get a list of existing tasks create from the specific template : tmpl
                List<CTask> curCreatedTasks = _context.Task.Include(t => t.Person).Where(t => ((t.ParentTaskId == tmpl.Id) || (t.Description == tmpl.Description))).ToList();

                // Filter out new, created tasks : n that already exist : Having the {same_task_id ...or... same_description} ...AND... {same Year, Month, Day } ...AND... {newlyCreatedTime >= currExistingTime}
                // Note : to be safe, we keep newlyCreatedTime earlier times than currExistingTime and may add as a duplicate in some special cases.
                List<CTask> newCreatedTasks = GetTemplateTasks(hstEnv, _context, tmpl, evList, lim).Where(n => !curCreatedTasks.Any(c => ((c.ParentTaskId == n.ParentTaskId) || (c.Description == n.Description)) &&
                                                                c.DueDate.HasValue && n.DueDate.HasValue && (c.DueDate.Value.Year == n.DueDate.Value.Year) && (c.DueDate.Value.Month == n.DueDate.Value.Month) && 
                                                                (c.DueDate.Value.Day == n.DueDate.Value.Day) && n.DueDate >= c.DueDate)).ToList();

                // Make sure the newly filtered potential tasks are assigned to a person
                // NOTE : we cannot rely on tmpl as the only parent tasks as newCreatedTasks may contain other tasks not having t.ParentTaskId = tmpl.Id 
                if (newCreatedTasks.Count > 0)
                {
                    // ensure each newCreatedTask is assigned to someone and that person is avaiable
                    // Cache Task Data by task id
                    var TDCache = new Dictionary<int, TaskData>();
                    var PIDList = new List<int>();
                    foreach (var t in newCreatedTasks)
                    {
                        TaskData taskData;
                        if (!TDCache.TryGetValue(t.Id, out taskData))
                        {
                            taskData = TaskData.ReadTaskData(hstEnv, t.Id, -1);
                            TDCache[t.Id] = taskData;
                        }

                        // For each newCreatedTask, check for a person who is unavailable and if unavailable, nullify person id and person;
                        if (t.PersonId.HasValue && !PIDList.Contains(t.PersonId.Value))
                        {
                            int pid = t.PersonId.Value;
                            PIDList.Add(pid);
                            Person person = (Person)_context.Person.First(p => p.Id == pid);
                            if ((person != null) && person.Roles.HasValue && (((long)person.Roles & (long)Person.TitleRoles.Unavailable) != 0))
                            {
                                t.PersonId = null;
                                t.Person = null;
                            }
                        }

                        if (!t.PersonId.HasValue || (t.PersonId <= 0))
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
                AllNewTaskList.AddRange(newCreatedTasks);
            }

            // Search for any existing tasks that are assigned to a person who is now unavailable
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

            // Check for duplicates by same description and same day before Adding to DB.
            var OrdTasks = AllNewTaskList.OrderBy(t => t.Description).ThenBy(t => t.DueDate);
            string lastDesc = "";
            int lastYear = -1, lastMonth = -1, lastDay = -1;
            foreach (var t in OrdTasks)
            {
                bool bSkip = (t.Description == lastDesc) && t.DueDate.HasValue && (t.DueDate.Value.Year == lastYear) && (t.DueDate.Value.Month == lastMonth) && (t.DueDate.Value.Day == lastDay);
                lastDesc = t.Description;
                lastYear = t.DueDate.Value.Year;
                lastMonth = t.DueDate.Value.Month;
                lastDay = t.DueDate.Value.Day;

                if (!bSkip)
                {
                    _context.Task.Add(t);
                    _context.SaveChanges();
                }
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

