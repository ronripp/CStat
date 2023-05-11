using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CTask = CStat.Models.Task;

namespace CStat.Models
{
    public class FullTask
    {
        public FullTask(Task tsk, TaskData tskData, string basePth)
        {
            task = tsk;
            taskData = tskData;
            basePath = basePth;
        }
        public Task task;
        public TaskData taskData;
        public string basePath;

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

            string folderName = "Tasks";
            string webRootPath = hostEnv.WebRootPath;
            string basePath = Path.Combine(webRootPath, folderName);

            return new FullTask(task, taskData, basePath);
        }

        public static long CreateTaskReport(FullTask ft, string pdfFile)
        {
            // Create HTML Report HERE
            var t = ft.task;
            var td = ft.taskData;

            var hstr = "<!DOCTYPE html>\n<html>\n<head>\n<title><b>" + "[" + t.Id + "] " + t.Description + "</b></title>\n</head>\n<body>";

            hstr += "<h3><b><u>Title :</u>&nbsp;&nbsp;" + t.Description + "</b></h3>\n";

            hstr += "<p><b><u>Detail:</u></b><br>\n";
            hstr += td.Detail + "</p>\n";

            hstr += "<p><b><u>Status :</u></b>&nbsp;&nbsp;" + ((eTaskStatus)td.state).ToString().Replace("_", " ") + "&nbsp;&nbsp;" + (t.ActualDoneDate.HasValue ? t.ActualDoneDate.Value.Date.ToString("M-d-yyyy") : "") + "</b></p>\n";

            hstr += "<p><b><u>Comments:</u></b><br>\n";
            hstr += td.comments + "\n";
            hstr += "</p>\n";

            hstr += "<p><b><u>Photos:</u></b><br>\n";
            if (td.pics.Count() > 4)
            {
                // Draw 2 pictures across to keep from exceeding the 5 page limit of free SelectPDF
                int NumPics = td.pics.Count();
                for (int i=0; i < td.pics.Count();)
                {
                    hstr += "<p><span style=\"width:960px; border-style: solid; page-break-inside: avoid\">";
                    hstr += "<div style=\"width:425px; border-style: solid;\"><b>" + td.pics[i].title + "</b><br>\n<img src=\"" + Path.Combine(ft.basePath, td.pics[i].url.Replace("/", "\\")) + "\" width=\"800\"></div>";
                    if (++i < NumPics)
                        hstr += "<div style=\"width:425px; margin-left: 10px; border-style: solid;\"><b>" + td.pics[i].title + "</b><br>\n<img src=\"" + Path.Combine(ft.basePath, td.pics[i].url.Replace("/", "\\")) + "\" width=\"800\"></div>";
                    hstr += "</span></p>\n";
                }
            }
            else
            {
                foreach (var p in td.pics)
                {
                    hstr += "<p><div style=\"width:800px; border-style: solid; page-break-inside: avoid\"><b>" + p.title + "</b><br>\n";
                    hstr += "<img src=\"" + Path.Combine(ft.basePath, p.url.Replace("/", "\\")) + "\" width=\"800\"></div></p>\n";
                }
            }
            hstr += "</p>\n";

            hstr += "</body>\n</html>\n";

            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();

            // set converter options
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
            converter.Options.WebPageWidth = 1024;
            converter.Options.WebPageHeight = 0;

            // create a new pdf document converting an url
            PdfDocument doc = converter.ConvertHtmlString(hstr);

            // save pdf document
            doc.Save(pdfFile);

            if (!File.Exists(pdfFile))
                return 0;

            FileInfo fi = new FileInfo(pdfFile);
            return (fi != null) ? fi.Length : 0;
        }
    }
}