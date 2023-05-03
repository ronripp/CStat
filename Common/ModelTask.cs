using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Models
{
    public class FullTask
    {
        public FullTask(Task tsk, TaskData tskData)
        {
            task = tsk;
            taskData = tskData;
        }
        public Task task;
        public TaskData taskData;
        public bool IsTemplate;
    }

    public partial class Task
    {
        public static int RemoveActiveChildTasks(CStat.Models.CStatContext context, int parentTaskId)
        {
            // Delete only those task that are unabiguously not done
            int NumDeleted = 0;
            var oldTasks = context.Task.AsNoTracking().Where(t => t.ParentTaskId.HasValue &&
                                                             (t.ParentTaskId.Value == parentTaskId) && !t.ActualDoneDate.HasValue &&
                                                             ((t.Status & (int)Task.eTaskStatus.Completed) == 0));
            if (oldTasks != null)
            {
                foreach (var t in oldTasks)
                {
                    try
                    {
                        context.Task.Remove(t);
                        context.SaveChangesAsync();
                        ++NumDeleted;
                    }
                    catch
                    {
                        // TBD : log these errors
                    }
                }
            }
            return NumDeleted;
        }

        public static FullTask GetFullTask(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, int tid)
        {
            var task = context.Task.FirstOrDefault(m => m.Id == tid);
            if ((task == null) || (tid != task.Id) || ((task.Type & (int)CTask.eTaskType.Template) != 0))
                return null;

            var taskData = TaskData.ReadTaskData(hostEnv, tid, task.ParentTaskId.HasValue ? task.ParentTaskId.Value : -1);
            if (task.DueDate == null)
                task.DueDate = task.EstimatedDoneDate; // temporary for editing purpose
            else
                taskData.FixedDueDate = true;

            task.GetTaskStatus(out eTaskStatus state, out eTaskStatus reason, out int PercentComplete);
            taskData.state = (int)state;
            taskData.reason = (int)reason;
            taskData.PercentComplete = PercentComplete;

            return new FullTask(task, taskData);
        }

        public static int CreateTaskReport(FullTask ft, string pdfFile = "")
        {
            // Create HTML Report HERE
            string HtmlStr = "Hello";

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(HtmlStr);

            // save pdf document
            doc.Save(pdfFile);

            return 0;
        }
    }
}