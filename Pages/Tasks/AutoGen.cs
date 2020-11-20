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
    public interface IDueTime
    {
        DateTime Before_Start();
        DateTime At_Start();
        DateTime After_Start();
        DateTime At_End();
        DateTime Before_End();
        DateTime After_End();
        DateTime At_Hour();
        DateTime Hours_Before();
        DateTime On_Day();
        DateTime Days_Before();
        DateTime On_Week_Day();
        DateTime Before_Week_Day();
        DateTime On_Week();
        DateTime Before_Week();
        DateTime In_Month();
        DateTime Before_Month();
        DateTime At_Date();
    }

    public class DueTime
    {
        DateTime Before_Start()
        {
            return DateTime.Now;
        }
        DateTime At_Start()
        {
            return DateTime.Now;
        }
        DateTime After_Start()
        {
            return DateTime.Now;
        }
        DateTime At_End()
        {
            return DateTime.Now;
        }
        DateTime Before_End()
        {
            return DateTime.Now;
        }
        DateTime After_End()
        {
            return DateTime.Now;
        }
        DateTime At_Hour()
        {
            return DateTime.Now;
        }
        DateTime Hours_Before()
        {
            return DateTime.Now;
        }
        DateTime On_Day()
        {
            return DateTime.Now;
        }
        DateTime Days_Before()
        {
            return DateTime.Now;
        }
        DateTime On_Week_Day()
        {
            return DateTime.Now;
        }
        DateTime Before_Week_Day()
        {
            return DateTime.Now;
        }
        DateTime On_Week()
        {
            return DateTime.Now;
        }
        DateTime Before_Week()
        {
            return DateTime.Now;
        }
        DateTime In_Month()
        {
            return DateTime.Now;
        }
        DateTime Before_Month()
        {
            return DateTime.Now;
        }
        DateTime At_Date()
        {
            return DateTime.Now;
        }
    }

    public class DayDueTime : DueTime
    {
        public DayDueTime(DateTime day, int startHour=0, int startMins=0)
        {
            this.day = new DateTime(day.Year, day.Month, day.Day, startHour, startMins, 0);
        }

        DateTime Before_Start()
        {
            return day.AddMinutes(-1);
        }
        DateTime At_Start()
        {
            return day;
        }
        DateTime After_Start()
        {
            return day.AddMinutes(1);
        }
        DateTime At_End()
        {
            return new DateTime(day.Year, day.Month, day.Day, 23, 59, 59);
        }
        DateTime Before_End()
        {
            return new DateTime(day.Year, day.Month, day.Day, 23, 59, 0);
        }
        DateTime After_End()
        {
            return new DateTime(day.Year, day.Month, day.Day, 23, 59, 59).AddSeconds(1);
        }
        DateTime Hours_After (int hrsAfterStart)
        {
            return day.AddHours(hrsAfterStart);
        }
        DateTime Hours_Before(int hrsAfterStart)
        {
            return DateTime.Now;
        }

        private DateTime day;
    }
    public class WeekDueTime : DueTime
    {

    }
    public class MonthDueTime : DueTime
    {

    }
    public class YearDueTime : DueTime
    {

    }
    public class SeasonDueTime : DueTime
    {

    }


    public class EventDueTime : DueTime
    {
        public EventDueTime(Event ev)
        {
            this.ev = ev;
        }

        private Event ev = null;
    }

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
            var TemplateTasks = _context.Task.Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();

            // Determine what needs to be Auto generated
            foreach (var tmpl in TemplateTasks)
            {
                var curCreatedTasks = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

                tmpl.GetTaskType(out CTask.eTaskType dueType, out CTask.eTaskType eachType, out int dueVal);

                DateTime now = DateTime.Now;
                DateTime startTime = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                DateTime endTime = new DateTime(startTime.Year+1, 12, 31, 23, 59, 59);
                List<Event> evList = GetEvents(startTime, endTime);

                // Create Tasks starting at the ActualDoneDate/Time which marks the date/time tasks were last created.
                if (tmpl.ActualDoneDate == null)
                {
                    tmpl.ActualDoneDate = DateTime.Now.AddDays(-1);
                }

                // Add task creation here

                tmpl.ActualDoneDate = DateTime.Now;
            }

            // Filter out existing Auto generated Tasks having a parent task id that matches template task id and due date.
            foreach (var tmpl in TemplateTasks)
            {
                var AutoGenList = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

            }

            // Generate active tasks 

            return null;
        }

        public List<Event> GetEvents(DateTime startDate, DateTime endDate)
        {
            return _context.Event.Where(e => (e.StartTime >= startDate) && (e.EndTime <= endDate)).ToList();
        }


        public DateTime[] GetDueDates(CTask.eTaskType eachType, CTask.eTaskType dueType)
        {
            DateTime[] dtArr = new DateTime[1];
            dtArr[0] = DateTime.Now; // TBD replace
            return dtArr;
        }
    }
}

