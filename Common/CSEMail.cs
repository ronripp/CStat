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

namespace CStat.Common
{
	public class REMail
	{
		public string From;
		public string Subject;
		public string SentDateTime;
		public string Body;
	}	
	public class CSEMail
	{
		private readonly string fromName = "CCA Informer";
		//		private readonly string fromSMTP = "cccaserve.org";
		private readonly string fromAdr = "inform@ccaserve.org";
		private readonly string fromPass = null;
		public string sendResult = "";
		private MailKit.Net.Pop3.Pop3Client readClient = null;

		public CSEMail(IConfiguration configuration)
		{
			Configuration = configuration;
			fromPass = Configuration["CSEMail:SenderPassword"];
		}
		public IConfiguration Configuration { get; }

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

		public List<MimeMessage> ReadEMails()
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

				//Fetch Emails
				var eList = new List<MimeMessage>();
				for (int i = 0; i < readClient.Count; i++)
				{
					eList.Add(readClient.GetMessage(i));
					//Console.WriteLine("Subject: {0}", message.Subject);
				}
				readClient.Disconnect(true);
				return eList;
			}
		}
	}
}