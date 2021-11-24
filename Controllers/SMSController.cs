// Code sample for ASP.NET Core on .NET Core
// From command prompt, run:
// dotnet add package Twilio.AspNet.Core

using CStat.Areas.Identity.Data;
using CStat.Common;
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
        private readonly CSSettings csSettings;

        public SmsController(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, CStat.Models.CStatContext context)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
            Context = context;
            csSettings = CSSettings.GetCSSettings(Config, userManager);
        }

        [System.Web.Mvc.HttpPost]
        public TwiMLResult ReceiveSms([FromForm] SmsRequest req)
        {
            var user = csSettings.GetUserByPhone(req.From);
            if (user == null)
                return SendMsgResp("I don't know you.");

            var name = !string.IsNullOrEmpty(user.Alias) ? user.Alias : CSSettings.GetDefAlias(user.EMail);

            if (string.IsNullOrEmpty(req.Body))
                return SendMsgResp(name, "How can I help?");

            string reqStr = req.Body.Trim();
            if (reqStr.Length == 0)
                return SendMsgResp(name, "How can I help?");

            string[] words = reqStr.Split(new char[] { ' ', '\n', ',', ';' });
            var cmdMgr = new CmdMgr(Context, csSettings, HostEnv, Config, UserManager);
            return SendMsgResp(cmdMgr.ParseCmd(words));
        }

        private TwiMLResult SendMsgResp(string name, string respStr)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("Hi " + name + ".\n" + respStr);
            return TwiML(messagingResponse);
        }
        private TwiMLResult SendMsgResp(string respStr)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message(respStr + "\n");
            return TwiML(messagingResponse);
        }

    }
}