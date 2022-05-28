using CStat.Areas.Identity.Data;
using CStat.Data;
using MailKit.Net.Pop3;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SelectPdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace CStat.Common
{
    public class REMail
    {
        public REMail(int index)
        {
            Index = index;
        }
        public string From { get; set; } = "";
        public string Subject { get; set; } = "";
        public DateTime Date { get; set; } = default;
        public string TextBody { get; set; } = "";
        public string HtmlBody { get; set; } = "";
        public List<string> Attachments;
        private int Index = 0;

        public bool AddHtmlAsPDFAttachment(string fname = null)
        {
            try
            {
                String HtmlStr = !String.IsNullOrEmpty(HtmlBody) ? HtmlBody : TextBody;

                if (String.IsNullOrEmpty(HtmlStr))
                    return false;

                // instantiate a html to pdf converter object
                HtmlToPdf converter = new HtmlToPdf();

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.WebPageWidth = 1024;
                converter.Options.WebPageHeight = 0;

                if (string.IsNullOrEmpty(fname))
                {
                    string fBody = "";
                    if (Subject.Length > 0)
                    {
                        var subjStr = Subject.Trim();
                        if (subjStr.StartsWith("FW", StringComparison.OrdinalIgnoreCase))
                            subjStr = subjStr.Replace("Fwd:", "", StringComparison.OrdinalIgnoreCase).Replace("Fw:", "", StringComparison.OrdinalIgnoreCase);
                        subjStr = subjStr.Replace(":", "", StringComparison.OrdinalIgnoreCase).Replace("\\", "-").Replace("/", "-").Trim();
                        fBody = subjStr + " (" + Date.Month + "-" + Date.Day + "-" + Date.Year % 100 + ")";
                    }
                    else
                    {
                        if (Attachments.Count > 0)
                        {
                            string fName = Attachments[0];
                            var aBody = Path.GetFileNameWithoutExtension(Attachments[0]);
                            fBody = aBody.Trim() + "_INTRO (" + Date.Month + "-" + Date.Day + "-" + Date.Year % 100 + ")";
                        }
                        else
                            fBody = "Memo (" + Date.Month + "-" + Date.Day + "-" + Date.Year % 100 + ")";
                    }
                    fname = fBody + ".pdf";
                }
                fname = GetUniqueTempFileName(fname, false);

                // create a new pdf document converting an url
                PdfDocument doc = converter.ConvertHtmlString(HtmlStr);

                // save pdf document
                doc.Save(fname);

                Attachments.Add(fname);
            }
            catch
            {
                return false;
            }
            return File.Exists(fname);
        }
        public string GetUniqueTempFileName(string fName, bool isAtt)
        {
            var TempPath = Path.GetTempPath();
            var fBody = (isAtt ? "A" : "T") + Index.ToString("000") + Path.GetFileNameWithoutExtension(fName) // // Replace characters not allowed in filename
                .Replace("<", "_")  // < (less than)
                .Replace(">", "_")  // > (greater than)
                .Replace(":", "_")  // : (colon)
                .Replace("\"", "_") // " (double quote)
                .Replace("/", "_")  // / (forward slash)
                .Replace("\\", "_") // \ (backslash)
                .Replace("|", "_")  // | (vertical bar or pipe)
                .Replace("?", "_")  // ? (question mark)
                .Replace("*", "_"); // *(asterisk)
            var fExt = Path.GetExtension(fName);
            var fileName = Path.Combine(TempPath, fBody + fExt);
            if (File.Exists(fileName))
            {
                for (char j = 'B'; j < 'Z'; ++j)
                {
                    fileName = Path.Combine(TempPath, fBody + "_Rev_" + j.ToString() + fExt);
                    if (!File.Exists(fileName))
                        return fileName;
                }
            }
            return fileName;
        }
        public string GetFinalFileName(string fName)
        {
            var IndexStr = Index.ToString("000");
            var IsEmailText = fName.StartsWith("T" + IndexStr, StringComparison.OrdinalIgnoreCase);
            return ((fName.Length <= 4) || (!fName.StartsWith("A" + IndexStr, StringComparison.OrdinalIgnoreCase) && !IsEmailText)) ?
                fName : (IsEmailText ? "body_" + fName.Substring(4) : fName.Substring(4));
        }
    }

    public class CSEMail
    {
        public static readonly string[] dateFormats = { "M/d/yy", "MM/dd/yy", "MM/d/yy", "M/dd/yy", "M/d/yyyy", "MM/dd/yyyy", "MM/d/yyyy", "M/dd/yyyy"};
        private readonly string fromName = "CCA Informer";
        //		private readonly string fromSMTP = "cccaserve.org";
        private readonly string fromAdr = "inform@ccaserve.org";
        private readonly string fromPass = null;
        public string sendResult = "";
        private MailKit.Net.Pop3.Pop3Client readClient = null;
        public CSSettings _settings { get; set; }

        public CSEMail(IConfiguration config, UserManager<CStatUser> userManager)
        {
            fromPass = config["CSEMail:SenderPassword"];
            _settings = CSSettings.GetCSSettings(config, userManager);
        }

        public bool Send(string toName, string toAdr, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this.fromName, fromAdr));
            message.To.Add(new MailboxAddress(toName, toAdr));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };
            using (var client = new SmtpClient())
            {
                try
                {
                    // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect("mail.ccaserve.org", 587, false);

                    // Note: only needed if the SMTP server requires authentication
                    client.Authenticate(fromAdr, fromPass);

                    client.Send(message);
                    client.Disconnect(true);
                }
                catch (Exception e)
                {
                    sendResult = e.Message;
                    return false;
                }
            }
            sendResult = "Sent";
            return true;
        }
        public List<REMail> ReadEMails()
        {
            bool NeedSave = false;
            try
            {
                using (readClient = new Pop3Client())
                {
                    var Server = "mail.ccaserve.org";
                    var Port = "110";
                    var UseSsl = false;
                    var credentials = new NetworkCredential(fromAdr, fromPass);
                    var cancel = new CancellationTokenSource();
                    var uri = new Uri(string.Format("pop{0}://{1}:{2}", (UseSsl ? "s" : ""), Server, Port));

                    //Connect to email server
                    readClient.Connect(uri, cancel.Token);
                    readClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    readClient.Authenticate(credentials, cancel.Token);

                    // Read EMails roughly after those we read last
                    var reList = new List<REMail>();
                    var LatestEMailRead = _settings.LastEMailRead;
                    for (int i = 0; i < readClient.Count; i++)
                    {
                        var msg = readClient.GetMessage(i);

                        // Get Received Date and use it to keep track of emails we have already read and processed.
                        DateTime EMailReceived = default;
                        if (msg.Headers != null)
                        {
                            if (msg.Headers["Received"] != null)
                            {
                                var rval = msg.Headers["Received"];
                                var rFields = rval.Split(";");
                                foreach (var f in rFields)
                                {
                                    var fld = f.Trim();
                                    if (fld.Length < 45)
                                    {
                                        EMailReceived = CSEMail.ParseDetailedDate(fld);
                                        if (EMailReceived != default)
                                            break;
                                    }
                                }
                            }
                        }

                        // Get DateTime Originally Sent

                        if ((EMailReceived == default) || (EMailReceived <= LatestEMailRead))
                            continue; // Either an Admin Delivery failure alert or already read. TBD : Case where multiple emails read the same second but on or more not proccessed.
                        var re = new REMail(i);
                        re.Subject = msg.Subject;
                        re.HtmlBody = msg.HtmlBody;
                        re.TextBody = msg.TextBody;
                        var tbFlds = msg.TextBody.Split("\r\n");
                        foreach (var t in tbFlds)
                        {
                            var tfld = t.Trim();
                            if (tfld.StartsWith("Sent:") || tfld.StartsWith("Date:"))
                            {
                                re.Date = CSEMail.ParseDetailedDate(tfld);
                            }
                        }
                        re.From = (msg.Sender != null) ? msg.Sender.ToString() : (((msg.From != null) && (msg.From.Count > 0)) ? msg.From[0].ToString() : "unknown");
                        if (re.Date == default)
                            re.Date = PropMgr.UTCtoEST(msg.Date.DateTime);
                        re.Attachments = new List<string>();
                        foreach (var attachment in msg.Attachments)
                        {
                            var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name ?? "Att";
                            fileName = re.GetUniqueTempFileName(fileName, true);
                            using (var stream = File.Create(fileName))
                            {
                                if (attachment is MessagePart)
                                {
                                    var part = (MessagePart)attachment;

                                    part.Message.WriteTo(stream);
                                }
                                else
                                {
                                    var part = (MimePart)attachment;

                                    part.Content.DecodeTo(stream);
                                }
                            }
                            re.Attachments.Add(fileName);
                        }

                        // Sometimes images are in BodyParts. filter out other content
                        foreach (var attachment in msg.BodyParts)
                        {
                            var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name ?? "";
                            if (string.IsNullOrEmpty(fileName))
                                continue; // filter out other non-file content
                            fileName = re.GetUniqueTempFileName(fileName, true);
                            using (var stream = File.Create(fileName))
                            {
                                if (attachment is MessagePart)
                                {
                                    var part = (MessagePart)attachment;

                                    part.Message.WriteTo(stream);
                                }
                                else
                                {
                                    var part = (MimePart)attachment;

                                    part.Content.DecodeTo(stream);
                                }
                            }
                            re.Attachments.Add(fileName);
                        }


                        if (EMailReceived > _settings.LastEMailRead)
                        {
                            NeedSave = true;
                            _settings.LastEMailRead = EMailReceived;
                        }
                        reList.Add(re);
                    }
                    readClient.Disconnect(true);

                    if (NeedSave)
                        _settings.Save();

                    return reList;
                }
            }
            catch (Exception e)
            {
                _ = e;
                return new List<REMail>(); // TBD Log failure
            }
        }
        public static DateTime ParseDetailedDate(string rawDStr)
        {
            CultureInfo enUS = new CultureInfo("en-US");
            DateTime ResDT = default;
            var dStr = rawDStr.Substring(5).Trim().Replace(" at ", " ").Replace("{", "").Replace("}", "");
            bool isMilitary = (!dStr.Contains("AM", StringComparison.OrdinalIgnoreCase) && !dStr.Contains("PM", StringComparison.OrdinalIgnoreCase));

            var dFlds = dStr.Split(" ").Select(s => s.Trim()).ToArray();

            if (dFlds.Count() < 1)
                return default;
            try
            {
                if (dFlds[0].Contains("/"))
                    return DateTime.Parse(dStr); // Simpler DateTime
            }
            catch
            {
                return default;
            }
            var format = "";
            int idx = 0;
            if (PropMgr.IsDOW(dFlds[0]))
            {
                idx = 1;
                if ((dFlds.Count() < 6) || (dFlds.Count() > 7))
                    return default;
                format += (dFlds[0].Length > 4) ? "dddd, " : "ddd, ";
            }
            else
            {
                if ((dFlds.Count() < 5) || (dFlds.Count() > 6))
                    return default;
            }

            int day = 0;
            int month = 0;
            if (PropMgr.TryParseMonth(dFlds[idx], out month))
            {
                if (!int.TryParse(dFlds[idx+1].Replace(",", ""), out day))
                    return default;
                format += (dFlds[idx].Length > 3) ? "MMMM " : "MMM ";
                format += (dFlds[idx+1].Length > 2) ? "dd, " : "d, ";
            }
            else if (PropMgr.TryParseMonth(dFlds[idx+1], out month))
            {
                if (!int.TryParse(dFlds[idx], out day))
                    return default;
                format += (dFlds[idx].Length > 1) ? "dd " : "d ";
                format += (dFlds[idx+1].Length > 3) ? "MMMM " : "MMM ";
            }

            int year = 0;
            var yStr = dFlds[idx + 2];
            if (yStr.EndsWith(","))
            {
                yStr = yStr.Replace(",", "");
                format += "yyyy, ";
            }
            else
                format += "yyyy ";
            if (!int.TryParse(yStr, out year) || (year < 2000))
                return default;

            int hours = -1;
            int minutes = -1;
            int seconds = -1;
            bool hasSeconds = false;
            var times = dFlds[idx+3].Split(":");
            if (times.Count() == 3)
            {
                hasSeconds = int.TryParse(times[2], out seconds);
            }
            if ((times.Count() == 2) || hasSeconds)
            {
                int.TryParse(times[0], out hours);
                int.TryParse(times[1], out minutes);
                if (isMilitary)
                    format += (times[0].Length > 1) ? "HH:" : "H:";
                else
                    format += (times[0].Length > 1) ? "hh:" : "h:";
                format += (times[1].Length > 1) ? "mm" : "m";
                if (hasSeconds)
                    format += (times[2].Length > 1) ? ":ss" : ":s";
                else
                    seconds = 0;
            }
            else
                return default;

            bool hasPM = dFlds[idx+4].Contains("PM", StringComparison.OrdinalIgnoreCase);
            string formatStr = "";

            if (dFlds[idx+4].Contains("AM", StringComparison.OrdinalIgnoreCase) || hasPM)
            {
                format += " tt";
                if (dFlds.Length == 7)
                {
                    format += " zzz";
                    formatStr = dFlds[idx+5];
                }
            }
            else
            {
                format += " zzz";
                formatStr = dFlds[idx+4];
            }

            int OffsetHrs = 0;
            int OffsetMins = 0;
            if (!string.IsNullOrEmpty(formatStr))
            {
                formatStr = formatStr.Replace("+0", "").Replace("+", "").Replace("-0", "-").Replace("(", "").Replace(")", "");
                var offs = formatStr.Split(":");
                if ((offs.Count() == 1) && (offs[0].EndsWith("00")))
                    offs[0] = offs[0].Substring(0, offs[0].Length - 2);
                int.TryParse(offs[0], out OffsetHrs);
                if (offs.Count() == 2)
                    int.TryParse(offs[1], out OffsetMins);
            }
            try
            {
                ResDT = DateTime.ParseExact(dStr, format, enUS, DateTimeStyles.None);
            }
            catch (Exception e)
            {
                _ = e;
                // TBD Determine Date by hand
                if (hasPM)
                    hours += 12;
                bool IsUTC = (formatStr.Length > 0) && (OffsetHrs == 0) && (OffsetMins == 0);
                ResDT = new DateTime(year, month, day, hours, minutes, seconds, (IsUTC ? DateTimeKind.Utc : DateTimeKind.Local));
                if (IsUTC)
                    ResDT = PropMgr.UTCtoEST(ResDT);
            }
            return ResDT;
        }

        public static string ProcessEMails(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CSSettings csSettings, CStat.Models.CStatContext context)
        {
            var cse = new CSEMail(config, userManager);
            String report = "Save EMails.\n";
            var eList = cse.ReadEMails();
            foreach (var e in eList)
            {
                report += e.Subject + ".\n";

                if (ExecuteEMailCmd(hostEnv, config, userManager, csSettings, context, e))
                    continue;

                if ((e.HtmlBody != null) && (e.HtmlBody.Length > 5))
                    e.AddHtmlAsPDFAttachment();

                if (e.Attachments.Count > 0)
                {
                    var dbox = new CSDropBox(Startup.CSConfig);
                    var destPath = "/Memorandums";
                    foreach (var a in e.Attachments)
                    {
                        try
                        {
                            string FileName = e.GetFinalFileName(Path.GetFileName(a));
                            if (dbox.FileExists(destPath + "/" + FileName))
                            {
                                var fBody = Path.GetFileNameWithoutExtension(FileName);
                                var fExt = Path.GetExtension(FileName);
                                for (char j = 'B'; j < 'Z'; ++j)
                                {
                                    FileName = fBody + "_Rev_" + j.ToString() + fExt;
                                    if (!dbox.FileExists(destPath + "/" + FileName))
                                        break;
                                }
                            }
                            if (dbox.UploadFile(a, destPath, FileName))
                                report += "Saved to DropBox " + destPath + " > " + FileName + ".\n";

                            File.Delete(a);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return report;
        }

        // Check for requests
        public static bool ExecuteEMailCmd(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CSSettings csSettings, CStat.Models.CStatContext context, REMail e)
        {
            // Examine the subject of email
            var subject = e.Subject.Trim().ToLower();
            Char[] delimiters = {' ', ','};
            var words = subject.Split(delimiters);

            if (words.Any(w => w == "meeting") || words.Any(w => w == "minutes") || words.Any(w => w == "meeting-minutes"))
            {
                DateTime mmDate = DateTime.MinValue; 
                foreach (var w in words)
                {
                    if (DateTime.TryParseExact(w, CSEMail.dateFormats, new CultureInfo("en-US"), DateTimeStyles.None, out mmDate))
                        break;
                }
                if (mmDate != DateTime.MinValue)
                {
                    // Meeting Minutes with a specific date
                    string mmFileName = mmDate.Year + "-" + mmDate.Month.ToString("D2") + "-" + mmDate.Day.ToString("D2") + "_MM.pdf";
                    if (e.Attachments.Count == 0)
                    {
                        // This email is likely meeting minutes
                        if ((e.HtmlBody != null) && (e.HtmlBody.Length > 5))
                            e.AddHtmlAsPDFAttachment(mmFileName);
                    }
                    else
                    { 
                        var dbox = new CSDropBox(Startup.CSConfig);
                        var destPath = "/Corporate/CCA Minutes";
                        foreach (var a in e.Attachments)
                        {
                            try
                            {
                                var FileName = mmFileName;
                                if (dbox.FileExists(destPath + "/" + mmFileName))
                                {
                                    var fBody = Path.GetFileNameWithoutExtension(mmFileName);
                                    var fExt = Path.GetExtension(mmFileName);
                                    for (char j = 'B'; j < 'Z'; ++j)
                                    {
                                        FileName = fBody + "_Rev_" + j.ToString() + fExt;
                                        if (!dbox.FileExists(destPath + "/" + FileName))
                                            break;
                                    }
                                }
                                dbox.UploadFile(a, destPath, FileName);
                                File.Delete(a);
                            }
                            catch
                            {
                            }
                        }
                    }
                    return true;
                }
            }

            return false;
        }

    }
}