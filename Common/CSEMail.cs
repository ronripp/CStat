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

namespace CStat.Common
{
	public class REMail
	{
		public string From { get; set; } = "";
		public string Subject { get; set; } = "";
		public DateTime Date { get; set; } = default;
		public string TextBody { get; set; } = "";
		public string HtmlBody { get; set; } = "";
		public List<string> Attachments;

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
					fname = Path.Combine(Path.GetTempPath(), fBody + ".pdf");
				}
				fname = REMail.GetUniqueTempFileName(fname);

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
		public static string GetUniqueTempFileName (string fileName)
        {
			var TempPath = Path.GetTempPath();
			if (File.Exists(fileName))
			{
				var fBody = Path.GetFileNameWithoutExtension(fileName);
				var fExt = Path.GetExtension(fileName);
				for (char j = 'A'; j < 'Z'; ++j)
				{
					fileName = Path.Combine(TempPath, fBody + "_Rev_" + j.ToString() + fExt);
					if (!File.Exists(fileName))
						return fileName;
				}
			}
			return fileName;
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
					var LatestSentDT = (_settings.LastEMailRead == default) ? _settings.LastEMailRead : _settings.LastEMailRead.AddMinutes(-30); // TBD: Make sure we do not miss slow sent emails.
					for (int i = 0; i < readClient.Count; i++)
					{
						var msg = readClient.GetMessage(i);
						DateTime sentDate = PropMgr.UTCtoEST(msg.Date.DateTime);
						if (sentDate <= LatestSentDT)
							continue;
						var re = new REMail();
						re.Subject = msg.Subject;
						re.TextBody = msg.TextBody;
						re.HtmlBody = msg.HtmlBody;
						re.From = (msg.Sender != null) ? msg.Sender.ToString() : (((msg.From != null) && (msg.From.Count > 0)) ? msg.From[0].ToString() : "unknown");
						re.Date = sentDate;
						re.Attachments = new List<string>();
						foreach (var attachment in msg.Attachments)
						{
							var TempPath = Path.GetTempPath();
							var fileName = Path.Combine(TempPath, attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name ?? "Att");
							fileName = REMail.GetUniqueTempFileName(fileName);
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
						if (sentDate > _settings.LastEMailRead)
						{
							NeedSave = true;
							_settings.LastEMailRead = sentDate;
						}
						reList.Add(re);
					}
					readClient.Disconnect(true);

					if (NeedSave)
						_settings.Save();

					return reList;
				}
			}
			catch
            {
				return new List<REMail>();
            }
		}
	}
}