using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Threading;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Pop3;
using System.Net;
using System.IO;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;
using CStat.Common;
using SelectPdf;
using System.Globalization;
using CStat.Data;

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
		public string GetUniqueTempFileName (string fName, bool isAtt)
        {
			var TempPath = Path.GetTempPath();
			var fBody = (isAtt ? "A" : "T") + Index.ToString("000") + Path.GetFileNameWithoutExtension(fName);
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
			return ((fName.Length <= 4) || (!fName.StartsWith("A" + IndexStr, StringComparison.OrdinalIgnoreCase) && !fName.StartsWith("T" + IndexStr, StringComparison.OrdinalIgnoreCase))) ?
				fName : fName.Substring(4);
		}
	}

	public class CSEMail
	{
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
					CultureInfo enUS = new CultureInfo("en-US");
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
										string format = "ddd, dd MMM yyyy HH:mm:ss zzz"; // Parse date and time with custom specifier.
										try
										{
											EMailReceived = DateTime.ParseExact(fld, format, enUS, DateTimeStyles.None);
										}
										catch (FormatException)
										{
											EMailReceived = default;
										}
									}
									if (EMailReceived != default)
										break;
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
							if (tfld.StartsWith ("Sent:") || tfld.StartsWith("Date:"))
							{
								tfld = tfld.Substring(5).Trim().Replace(" at ", " ");
								string format = "ddd, dd MMM yyyy HH:mm:ss zzz";
								if (tfld.Contains("AM", StringComparison.OrdinalIgnoreCase) || tfld.Contains("PM", StringComparison.OrdinalIgnoreCase))
								{
									// Determine exact format
									var off1 = tfld.IndexOf(", ");
									format =  (off1 > 3) ? "dddd, " : "ddd, ";

									off1 += 2;
									var off2 = tfld.IndexOf(" ", off1);
									format += ((off2 - off1) > 3) ? "MMMM " : "MMM ";

									var rFlds = (tfld.Substring(off2 + 1).Replace(":", " ").Replace(",", "")).Split(" ");
									if (rFlds.Count() >= 4)
                                    {
										format += (rFlds[0].Length > 1) ? "dd, yyyy " : "d, yyyy "; // Day #
										format += (rFlds[2].Length > 1) ? "hh:" : "h:"; // Hour
										format += (rFlds[3].Length > 1) ? "mm tt" : "m tt"; // Minutes
									}
									if (!tfld.EndsWith("AM", StringComparison.OrdinalIgnoreCase) && !tfld.EndsWith("PM", StringComparison.OrdinalIgnoreCase))
										format += "zzz";
								}
								try
								{ 
									re.Date = DateTime.ParseExact(tfld, format, enUS, DateTimeStyles.None);
									break;
								}
								catch (Exception e)
                                {
									re.Date = PropMgr.UTCtoEST(msg.Date.DateTime);
								}
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
			catch(Exception e)
            {
				return new List<REMail>(); // TBD Log failure
            }
		}

		public static string SaveEMails(IConfiguration config, UserManager<CStatUser> userManager)
		{
			var cse = new CSEMail(config, userManager);
			String report = "";
			var eList = cse.ReadEMails();
			foreach (var e in eList)
			{
				report += e.Subject + ".\n";

				if ((e.HtmlBody != null) && (e.HtmlBody.Length > 5))
					e.AddHtmlAsPDFAttachment();

				if (e.Attachments.Count > 0)
				{
					var dbox = new CSDropBox(Startup.CSConfig);
					var destPath = "/Memorandums";
					foreach (var a in e.Attachments)
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
						dbox.UploadFile(a, destPath, FileName);
						File.Delete(a);
					}
				}
			}
			return report;
		}
	}
}