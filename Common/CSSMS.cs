using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.TwiML;
using Twilio.AspNet.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;

namespace CStat.Common
{
    public class CSResult
    {
        public bool Succeeded;
        public string RespStr;
        public string RefStr;
        public void Set (bool succeeded, string respStr, string refStr)
        {
            Succeeded = succeeded;
            RespStr = respStr;
            RefStr = refStr;
        }
    }

    public class CSSMS
    {
        public enum NotifyType { EquipNT=1, StockNT=2, TaskNT=3};

        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly UserManager<CStatUser> _userManager;
        private string LogPath;
        public CSSMS (IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _config = config;
            _hostEnv = hostEnv;
            _userManager = userManager;
            string webRootPath = hostEnv.WebRootPath;
            LogPath = Path.Combine(webRootPath, "Tasks");
            if (!Directory.Exists(LogPath))
                Directory.CreateDirectory(LogPath);
        }

        public bool NeededToSend(string SMSSentFile, string hashStr, bool cleanLog=false)
        {
            List<string> lines = new List<string>();
            bool IsNeeded = true;
            DateTime baseDT = new DateTime(2010, 1, 1);
            double KeepSecs = PropMgr.ESTNow.AddHours(-25).Subtract(baseDT).TotalSeconds;
            double NeedSecs = 2.5*3600; // Sent 22.5 hours or less ago to handle daily even with send with clock changes TBD: revise
            using (StreamReader sr = new StreamReader(SMSSentFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] fields = line.Split('~');
                    var secs = PropMgr.ParseEST(fields[0]).Subtract(baseDT).TotalSeconds;
                    if ((secs >= NeedSecs) && (fields[1] == hashStr))
                    {
                        IsNeeded = false;
                        if (!cleanLog)
                            return false;
                    }

                    if (cleanLog && (secs >= KeepSecs))
                            lines.Add(line);
                }
            }
            if (!cleanLog)
                return true;

            using (StreamWriter sw = new StreamWriter(SMSSentFile))
            {
                foreach (var ln in lines)
                {
                    sw.WriteLine(ln);
                }
            }
            return IsNeeded;

        }

        public CSResult NotifyUsers(NotifyType nt, string msg, bool cleanLog = false)
        {
            var Settings = new CSSettings(_config, _userManager);

            List<CSUser> nUsers;
            switch (nt)
            {
                case NotifyType.EquipNT:
                    nUsers = Settings.UserSettings.Where(u => u.SendEquipText && (u.PhoneNum.Length > 6)).ToList();
                    break;
                case NotifyType.StockNT:
                    nUsers = Settings.UserSettings.Where(u => u.SendStockText && (u.PhoneNum.Length > 6)).ToList();
                    break;
                case NotifyType.TaskNT:
                    nUsers = Settings.UserSettings.Where(u => u.SendTaskText && (u.PhoneNum.Length > 6)).ToList();
                    break;
                default:
                    nUsers = new List<CSUser>();
                    break;
            }

            int sentCount = 0;
            foreach (var u in nUsers)
            {
                CSResult csRes = SendMessage(u.PhoneNum, msg, cleanLog);
                if (csRes.Succeeded)
                    ++sentCount;
                cleanLog = false;
            }

            CSResult csAllRes = new CSResult();
            csAllRes.Set(sentCount == nUsers.Count, "NotifyUsers", msg);
            return csAllRes;
        }

        public CSResult NotifyUser(string username, NotifyType nt, string msg, bool cleanLog = false)
        {
            var Settings = new CSSettings(_config, _userManager);

            var u = Settings.GetUser(username);
            if ((u == null) || (u.PhoneNum.Length < 7))
            {
                var csRes = new CSResult();
                csRes.Set(false, "No user found or user has no phone #", nt.ToString() + ">" + username);
                return csRes;
            }

            return SendMessage(u.PhoneNum, msg, cleanLog);
        }

        private object GetUsersAsync(UserManager<CStatUser> userManager)
        {
            throw new NotImplementedException();
        }

        public CSResult SendMessage(string toPhone, string msg, bool cleanLog=false)
        {
            string SMSSentFile = Path.Combine(LogPath, "SMSSent.log");
            string SendHash = toPhone + "[" + msg + "]";
            if (!NeededToSend(SMSSentFile, SendHash, cleanLog))
            {

            }

            var accountSid = _config["Twilio:AccountSID"];
            var authToken = _config["Twilio:AuthToken"];

            TwilioClient.Init(accountSid, authToken);

            var msgResponse = MessageResource.Create(
                body: msg,
                from: new Twilio.Types.PhoneNumber(_config["Twilio:PhoneNum"]),
                to: new Twilio.Types.PhoneNumber(toPhone)
            );

            CSResult csRes = new CSResult();
            if (msgResponse.ErrorCode == null)
            {
                //successfully queued
                csRes.Succeeded = true;
                csRes.RefStr = toPhone;
                csRes.RespStr = "Successfully Sent";
                using (StreamWriter sw = new StreamWriter(SMSSentFile, true))
                {
                    sw.WriteLine(PropMgr.ESTNowStr + "~" + SendHash);
                    sw.Close();
                }
            }
            else
            {
                //Twilio sent an error back
                string SMSFailedFile = Path.Combine(LogPath, "SMSFailed.log");
                csRes.Succeeded = false;
                csRes.RefStr = toPhone;
                csRes.RespStr = msgResponse.ErrorCode + "[" + PropMgr.ESTNowStr + "]:" + msgResponse.ErrorMessage;
                using (StreamWriter sw = new StreamWriter(SMSFailedFile, true))
                {
                    sw.WriteLine(PropMgr.ESTNowStr + "~" + SendHash + "~" + csRes.RespStr);
                    sw.Close();
                }
            }

            return csRes;
        }
    }
}
