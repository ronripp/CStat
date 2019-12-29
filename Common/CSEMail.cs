using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Threading;

namespace CStat.Common
{
    public class CSEMail
    {
		private readonly string fromName = "CCA Informer";
		private readonly string fromSMTP = "cccaserve.org";
		private readonly string fromAdr = "inform@ccaserve.org";
		private readonly string fromPass = "Red35868!";
		public string sendResult = "";

		public bool send(string toName, string toAdr, string subject, string body)
		{
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(this.fromName, fromAdr));
			message.To.Add(new MailboxAddress(toName, toAdr));
			message.Subject = subject;
			message.Body = new TextPart("plain") {Text = body};
			using (var client = new SmtpClient())
			{
				try
				{
					// For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
					client.ServerCertificateValidationCallback = (s, c, h, e) => true;

					client.Connect("mail.ccaserve.org", 25, false);

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
	}
}
