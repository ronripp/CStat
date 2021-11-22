// Code sample for ASP.NET Core on .NET Core
// From command prompt, run:
// dotnet add package Twilio.AspNet.Core

using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;
        private readonly CStat.Models.CStatContext Context;

        public SmsController(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CStat.Models.CStatContext context)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
            Context = context;
        }

        [System.Web.Mvc.HttpPost]
        public TwiMLResult ReceiveSms([FromForm] SmsRequest req)
        {
            if (req.Body == null)
                return SendMsgResp("", "How can I help?");
            string reqStr = req.Body.Trim();
            if (reqStr.Length == 0)
               return SendMsgResp("Ron", "How can I help?");

            string[] words = reqStr.Split(new char[] { ' ', '\n', ',', ';' });

            return SendMsgResp(req.From, reqStr);
        }

        private TwiMLResult SendMsgResp(string name, string respStr)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("Hi " + name + ".\n" + respStr);
            return TwiML(messagingResponse);
        }

    }
}