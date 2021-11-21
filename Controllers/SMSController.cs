// Code sample for ASP.NET Core on .NET Core
// From command prompt, run:
// dotnet add package Twilio.AspNet.Core

using Microsoft.AspNetCore.Mvc;
using System.Web.Mvc;
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace TwilioReceive.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SmsController : TwilioController
    {
        [System.Web.Mvc.HttpPost]
        public TwiMLResult ReceiveSms([FromForm] SmsRequest incomingMessage)
        {
            var messagingResponse = new MessagingResponse();
            //messagingResponse.Message("Thank you. Your message was received.");
            messagingResponse.Message("The copy cat says: " + incomingMessage.Body);
            return TwiML(messagingResponse);
        }
    }
}