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
            var TemplateTasks = _context.Task.Include(t => t.Person).Where(t => (t.Type & (int)CTask.eTaskType.Template) != 0).OrderBy(t => t.Priority).ToList();

            // Determine what needs to be Auto generated
            foreach (var tmpl in TemplateTasks)
            {
                var curCreatedTasks = _context.Task.Include(t => t.Person).Where(t => (t.ParentTaskId == tmpl.Id)).ToList();

                tmpl.GetTaskType(out CTask.eTaskType dueType, out CTask.eTaskType eachType, out int dueVal);

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

        public DateTime[] GetDueDates(CTask.eTaskType eachType, CTask.eTaskType dueType)
        {
            switch (eachType)
            {
                /***************************/
                case CTask.eTaskType.Retreat_Event:
                /***************************/
                    switch (dueType)
                    {
                        case CTask.eTaskType.Before_Start:
                            break;
                        case CTask.eTaskType.At_Start:
                            break;
                        case CTask.eTaskType.After_Start:
                            break;
                        case CTask.eTaskType.At_End:
                            break;
                        case CTask.eTaskType.Before_End:
                            break;
                        case CTask.eTaskType.After_End:
                            break;
                        case CTask.eTaskType.At_Hour:
                            break;
                        case CTask.eTaskType.Before_Hour:
                            break;
                        case CTask.eTaskType.On_Day:
                            break;
                        case CTask.eTaskType.Before_Day:
                            break;
                        case CTask.eTaskType.On_Week_Day:
                            break;
                        case CTask.eTaskType.Before_Week_Day:
                            break;
                        case CTask.eTaskType.On_Week:
                            break;
                        case CTask.eTaskType.Before_Week:
                            break;
                        case CTask.eTaskType.In_Month:
                            break;
                        case CTask.eTaskType.Before_Month:
                            break;
                        case CTask.eTaskType.At_Date:
                            break;
                        default:
                            break;
                    }
                    break;

                /***************************/
                case CTask.eTaskType.Week_Event:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Event_Day:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Work_Day:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Day:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Event_Week:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Camp_Week_Month:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Active_Month:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Month:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Quarter:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Active_Quarter:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Year:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Season:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Number_of_Years:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                case CTask.eTaskType.Event:
                /***************************/
                switch (dueType)
                {
                    case CTask.eTaskType.Before_Start:
                        break;
                    case CTask.eTaskType.At_Start:
                        break;
                    case CTask.eTaskType.After_Start:
                        break;
                    case CTask.eTaskType.At_End:
                        break;
                    case CTask.eTaskType.Before_End:
                        break;
                    case CTask.eTaskType.After_End:
                        break;
                    case CTask.eTaskType.At_Hour:
                        break;
                    case CTask.eTaskType.Before_Hour:
                        break;
                    case CTask.eTaskType.On_Day:
                        break;
                    case CTask.eTaskType.Before_Day:
                        break;
                    case CTask.eTaskType.On_Week_Day:
                        break;
                    case CTask.eTaskType.Before_Week_Day:
                        break;
                    case CTask.eTaskType.On_Week:
                        break;
                    case CTask.eTaskType.Before_Week:
                        break;
                    case CTask.eTaskType.In_Month:
                        break;
                    case CTask.eTaskType.Before_Month:
                        break;
                    case CTask.eTaskType.At_Date:
                        break;
                    default:
                        break;
                }
                break;

                /***************************/
                default:
                /***************************/
                    break;
            }


            DateTime[] dtArr = new DateTime[1];
            dtArr[0] = DateTime.Now; // TBD replace
            return dtArr;
        }
    }
}

