using Dropbox.Api.TeamLog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using CStat.Models;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    public class AutoGen
    {
        private readonly CStat.Models.CStatContext _context;

        public AutoGen(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public bool GenTasks (int tmplId = -1)
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
                DateTime now = DateTime.Now;
                DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime endTime = new DateTime(startTime.Year+1, 12, 31, 23, 59, 59);
                DateRange lim = new DateRange(startTime, endTime);
                List<Event> evList = GetEvents(lim);
                List<CTask> newCreatedTasks = GetTemplateTasks(tmpl, evList, lim);

                // Filter out tasks that already exist.
                taskList.AddRange(newCreatedTasks.Where(n => !curCreatedTasks.Any(c => (c.ParentTaskId == n.ParentTaskId) && (c.DueDate == n.DueDate)) ).ToList());
            }

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
        public List<CTask> GetTemplateTasks(CTask tmpl, List<Event> events, DateRange lim)
        {
            List<CTask> taskList = new List<CTask>();

            if (tmpl.GetDueDates(events, out List<DateTime> dueDates, lim))
            {
                foreach (var dueDate in dueDates)
                {
                    // Create new task and add it to the list
                    CTask task = new CTask();
                    task.ParentTaskId = tmpl.Id;
                    TaskData taskData = new TaskData();
                    task.Type = 0;
                    task.Description = tmpl.Description;
                    task.PlanLink = tmpl.PlanLink;
                    task.EstimatedManHours = tmpl.EstimatedManHours;
                    task.Id = 0;
                    task.PersonId = tmpl.PersonId;
                    task.TotalCost = tmpl.TotalCost;
                    task.EstimatedDoneDate = new DateTime(1900, 1, 1);
                    task.CommittedCost = 0;
                    task.DueDate = dueDate;
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

