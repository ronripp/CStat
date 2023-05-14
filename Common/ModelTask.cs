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

            hstr += "<h3><b><u>Task" + t.Id + " :</u>&nbsp;&nbsp;" + t.Description + "</b></h3>\n";

            hstr += "<p><b><u><font size=\" + 1\">Detail :</font></u></b><br>\n";
            hstr += td.Detail + "</p>\n";

            hstr += "<p><b><u><font size=\" + 1\">Status :</font></u></b>&nbsp;&nbsp;" + ((eTaskStatus)td.state).ToString().Replace("_", " ") + "&nbsp;&nbsp;" + (t.ActualDoneDate.HasValue ? t.ActualDoneDate.Value.Date.ToString("M-d-yyyy") : "") + "</b></p>\n";

            //===[ ronripp Thu 5/11/23 @ 9:56 PM ]===

            hstr += "<p><b><u><font size=\" + 1\">Comments:</font></u></b><br>\n";
            var clines = td.comments.Split("===");
            foreach (var rawCl in clines)
            {
                var cl = rawCl.Trim();
                if (!String.IsNullOrEmpty(cl))
                {
                    if (cl.StartsWith("[") && cl.EndsWith("]"))
                        hstr += "<span style=\"color: blue; font-weight:bold; font-size:0.5em\">" + cl.Substring(1, cl.Length-2).Trim() + "</span><br>\n";
                    else
                        hstr += cl + "<br>\n";
                }
            }
            hstr += "</p>\n";

            hstr += "<p><b><u><font size=\" + 1\">Photos:</font></u></b><br>\n";
            string filePath = "";

            int NumPics = td.pics.Count();

            // TEMP !!!REMOVE!!!
            td.pics.Add(new Pic(t.Id, NumPics, "Test EXIF Image UP-Mirrored",  "C:\\EXIF\\up-mirrored.jpg"));
            ++NumPics;

            if (NumPics > 4)
            {
                // Draw 2 pictures across to keep from exceeding the 5 page limit of free SelectPDF

                for (int i=0; i < NumPics; ++i)
                {
                    filePath = Path.Combine(ft.basePath, td.pics[i].url.Replace("/", "\\"));
                    CCommon.ExifOrientJPEGFile(filePath); // Orient JPEG Image based on how user took it if information is available

                    int j = i + 1;
                    if (j < NumPics)
                    {
                        // Add 2 pics
                        hstr += "<p><table width=960px style=\"margin-top: 10px; border-style: solid; page-break-inside: avoid\">";
                        hstr += "<tr>";
                        hstr += "<td style=\"border-right: solid;\">" + td.pics[i].title + "</td>";
                        hstr += "<td>" + td.pics[j].title + "</td>";
                        hstr += "<tr>";
                        hstr += "<td style=\"border-right: solid; vertical-align:top\"><img src=\"" + filePath + "\" width=\"480\"></td>";
                        filePath = Path.Combine(ft.basePath, td.pics[j].url.Replace("/", "\\"));
                        CCommon.ExifOrientJPEGFile(filePath); // Orient JPEG Image based on how user took it if information is available
                        hstr += "<td style=\"vertical-align:top\"><img src=\"" + filePath + "\" width=\"480\">></td>";
                        hstr += "</tr>";
                        hstr += "</table></p>\n";
                        ++i;
                    }
                    else
                    {
                        hstr += "<p><div style=\"width:960px; margin-top: 10px; border-style: solid; page-break-inside: avoid\"><b>" + td.pics[i].title + "</b><br>\n";
                        hstr += "<img src=\"" + filePath + "\" width=\"960\"></div></p>\n";
                    }
                }
            }
            else
            {
                foreach (var p in td.pics)
                {
                    filePath = Path.Combine(ft.basePath, p.url.Replace("/", "\\"));
                    CCommon.ExifOrientJPEGFile(filePath); // Orient JPEG Image based on how user took it if information is available

                    hstr += "<p><div style=\"width:800px; margin-top: 10px; border-style: solid; page-break-inside: avoid\"><b>" + p.title + "</b><br>\n";
                    hstr += "<img src=\"" + filePath + "\" width=\"800\"></div></p>\n";
                }
            }

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

        private static void ExifOrientJPEGFile()
        {
            throw new NotImplementedException();
        }
    }
}