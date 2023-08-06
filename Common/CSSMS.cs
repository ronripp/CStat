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
        public CSResult()
        { 
        }
        public CSResult(bool succeeded, string respStr, string refStr, string sendHash)
        {
            Set(succeeded, respStr, refStr, sendHash);
        }

        public bool Succeeded;
        public string RespStr;
        public string RefStr;
        public string SendHash;
        public void Set (bool succeeded, string respStr, string refStr, string sendHash)
        {
            Succeeded = succeeded;
            RespStr = respStr;
            RefStr = refStr;
            SendHash = sendHash;
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

        public bool NeededToSend(string SMSSentFile, string hashStr, bool allowResend, bool cleanLog)
        {
            // Check for Debugging locally mode
            if ((LogPath == "C:\\cstat\\wwwroot\\Tasks") && !hashStr.StartsWith("203-770-3393["))
                return false; //only send to me when running local

            if (hashStr.Contains("CCA Power is OFF") || hashStr.Contains("CCA Power is back ON"))
                return true; // Always send power on / off messages whenever they occur.

            if (!File.Exists(SMSSentFile))
                return true;
            List<string> lines = new List<string>();
            bool IsNeeded = true;
            DateTime baseDT = new DateTime(2010, 1, 1);
            DateTime now = PropMgr.ESTNow;
            double DelaySecs = now.AddHours(-25).Subtract(baseDT).TotalSeconds + 2.5 * 3600; // Sent 22.5 hours or less ago to handle daily even with send with clock changes TBD: revise
            double ResendSecs = allowResend ? DelaySecs : -100;
            double CleanSecs = now.AddDays(-14).Subtract(baseDT).TotalSeconds; // When cleaning, don't save a record of what we already sent 2 or more weeks ago
            int linesRead = 0;
            using (StreamReader sr = new StreamReader(SMSSentFile))
            {
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    ++linesRead;
                    string[] fields = line.Split('~');
                    if ((fields.Length < 2) || (fields[0].Trim().Length == 0))
                        continue;
                    var sentSecs = PropMgr.ParseEST(fields[0]).Subtract(baseDT).TotalSeconds;
                    if ((sentSecs >= ResendSecs) && (IsSimilarHash(fields[1], hashStr)))
                    {
                        IsNeeded = false;
                        if (!cleanLog) // Not IsNeeded but keep looping to clean log file
                            return false;
                    }

                    if (cleanLog && (sentSecs > CleanSecs))
                            lines.Add(line);
                }
            }
            if (!cleanLog)
                return true;
            try
            {
                if (linesRead > 0)
                {
                    using (StreamWriter sw = new StreamWriter(SMSSentFile))
                    {
                        foreach (var ln in lines)
                        {
                            sw.WriteLine(ln);
                        }
                    }
                }
            }
            catch
            {
                return true; // exception : assume needed.
            }
            return IsNeeded;
        }
        private bool IsSimilarHash (string target, string hash)
        {
            if (target == hash)
                return true; // matches exactly

            if (target.Contains("CStat:Equip") && hash.Contains("CStat:Equip"))
            {
                // See if Equip messages are the same and values are 4 or less from each other.
                String[] tparts = target.Split(" ");
                String[] hparts = hash.Split(" ");
                if (tparts.Length != hparts.Length)
                    return false;
                int length = tparts.Length-2;
                if (length < 0)
                    return false;
                for (int i=0; i < length; ++i)
                {
                    if (tparts[i] != hparts[i])
                         return false;
                }
                if (tparts[length + 1] != hparts[length + 1])
                    return false;
                if (double.TryParse(tparts[length], out double tval) && double.TryParse(hparts[length], out double hval)) // See if values are close
                {
                    if (Math.Abs(tval - hval) > 4)
                        return false;
                }
                return true;
            }
            return false;
        }

        public CSResult NotifyUsers(NotifyType nt, string msg, bool allowResend, bool cleanLog)
        {
            var Settings = CSSettings.GetCSSettings(_config, _userManager);

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
                CSResult csRes = NotifyUser(u.EMail, nt, msg, allowResend, cleanLog);
                if (csRes.Succeeded)
                    ++sentCount;
                cleanLog = false;
            }

            return new CSResult(sentCount == nUsers.Count, "NotifyUsers", msg, "All Users");
        }

        public CSResult NotifyUser(string username, NotifyType nt, string msg, bool allowResend, bool cleanLog)
        {
            var Settings = CSSettings.GetCSSettings(_config, _userManager);

            var u = Settings.GetUser(username);
            if ((u == null) || (u.PhoneNum == null) || (u.PhoneNum.Length < 7) || !u.SendTaskText)
            {
                var csRes = new CSResult();
                csRes.Set(false, (((u != null) && !u.SendTaskText) ? "User does not allow texting." : ((u == null) ? "No user found." : "No phone # found.")), nt.ToString() + ">" + username, "");
                return csRes;
            }

            return SendMessage(u.PhoneNum, msg, allowResend, cleanLog);
        }

        private object GetUsersAsync(UserManager<CStatUser> userManager)
        {
            throw new NotImplementedException();
        }

        public CSResult SendMessage(string toPhone, string msg, bool allowResend, bool cleanLog)
        {
            string SMSSentFile = Path.Combine(LogPath, "SMSSent.log");
            string SendHash = toPhone + "[" + msg + "]";
            if (!NeededToSend(SMSSentFile, SendHash, allowResend, cleanLog))
            {
                return new CSResult(false, "Same message to same phone recently", toPhone, SendHash);
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
                csRes.SendHash = SendHash;
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

        internal void NotifyUsers(object notifyType, object nt, string v1, object msg, bool v2, object p)
        {
            throw new NotImplementedException();
        }
    }
}
