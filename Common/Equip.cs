﻿using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class PropaneLevel
    {
        public PropaneLevel(double levelPct, DateTime readingTime, double outsideTempF)
        {
            LevelPct = levelPct;
            ReadingTime = readingTime;
            OutsideTempF = outsideTempF;
        }

        public String ReadingTimeStr()
        {
            return ReadingTime.ToString("M/d/yy h:mm:ss tt");
        }

        public double OutsideTempF { get; set; } = -40;
        public double LevelPct { get; set; } = 0;
        public DateTime ReadingTime { get; set; }
    }

    public class ArdRecord
    {
        public ArdRecord(double freezerTempF, double fridgeTempF, double waterPress, DateTime timeStamp)
        {
            FreezerTempF = freezerTempF;
            FridgeTempF = fridgeTempF;
            WaterPress = waterPress;
            TimeStamp = timeStamp;
        }
        public ArdRecord(string freezerTempF, string fridgeTempF, string waterPress, DateTime timeStamp)
        {
            double _FreezerTempF = FreezerTempF;
            if (double.TryParse(freezerTempF, out _FreezerTempF))
                FreezerTempF = _FreezerTempF;
            double _FridgeTempF = FridgeTempF;
            if (double.TryParse(fridgeTempF, out _FridgeTempF))
                FridgeTempF = _FridgeTempF;
            double _WaterPress = WaterPress;
            if (double.TryParse(waterPress, out _WaterPress))
                WaterPress = _WaterPress;
            TimeStamp = timeStamp;
        }

        public String TimeStampStr()
        {
            return TimeStamp.ToString("M/d/yy h:mm:ss tt");
        }

        public double FreezerTempF { get; set; } = -40;
        public double FridgeTempF { get; set; } = -40;
        public double WaterPress { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
    }

    public class ArdMgr
    {
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        private IWebHostEnvironment HostEnv;
        private string FullLatest = "";
        private string FullAll = "";

        public ArdMgr(IWebHostEnvironment hostEnv)
        {
            HostEnv = hostEnv;
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "Equip");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            FullLatest = Path.Combine(newPath, "ardlatest.txt");
            FullAll = Path.Combine(newPath, "ardall.txt");
        }

        public int Set(string jsonStr)
        {
            if (ArdMgr.fLock.TryEnterWriteLock(100))
            {
                int retVal = -2;
                try
                {
                    string fullJSON = jsonStr + ",time:\"" + PropMgr.ESTNowStr + "\"";

                    using (StreamWriter sw = new StreamWriter(FullLatest, false))
                    {
                        sw.WriteLine(fullJSON);
                        sw.Close();

                    }

                    using (StreamWriter sw = new StreamWriter(FullAll, true))
                    {
                        sw.WriteLine(fullJSON);
                        sw.Close();
                        retVal = 1;
                    }
                }
                catch
                {

                }
                finally
                {
                    ArdMgr.fLock.ExitWriteLock();
                }
                return retVal;
            }
            else
            {
                return -2;
            }
        }
        public ArdRecord GetLast()
        {
            if (ArdMgr.fLock.TryEnterWriteLock(100))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(FullLatest, System.Text.Encoding.UTF8))
                    {
                        string raw = sr.ReadToEnd();
                        sr.Close();
                        if (raw.Length > 20)
                        {
                            string latest = (raw.StartsWith("[") || raw.StartsWith("{")) ? raw.Trim() : "{" + raw.Trim() + "}";
                            Dictionary<string, string> props = PropMgr.GetProperties(latest, "freezerTemp", "frigTemp", "waterPres", "time");
                            return new ArdRecord(props["freezerTemp"], props["frigTemp"], props["waterPres"], PropMgr.ParseEST(props["time"]));
                        }
                    }
                    return null;
                }
                catch
                {

                }
                finally
                {
                    ArdMgr.fLock.ExitWriteLock();
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public ArdRecord[] Get(DateTime startDT, DateTime endDT)
        {
            return null;
        }
    }

    public static class PropaneMgr
    {
        public static PropaneLevel GetTUTank() // Tank Utility Propane meter
        {
            var vals = PropMgr.GetSiteProperties("https://data.tankutility.com/api/getToken", "ronripp@outlook.com", "Red3581!", "token");

            string token = vals["token"];
            if (token == null)
                return null;

            var readings = PropMgr.GetSiteProperties("https://data.tankutility.com/api/devices/003a0028363537330b473931?token=" + token, null, null, "device.lastReading.tank", "device.lastReading.temperature", "device.lastReading.time_iso");

            double level;
            double temp;
            DateTime readTime;
            if (double.TryParse(readings["device.lastReading.tank"], out level) && DateTime.TryParse(readings["device.lastReading.time_iso"], out readTime) && double.TryParse(readings["device.lastReading.temperature"], out temp))
            {
                return new PropaneLevel(level, PropMgr.UTCtoEST(readTime), temp);
            }
            return null;
        }
    }

    public static class PropMgr
    {
        public static Dictionary<string, string> GetSiteProperties(string url, string userName, string password, params string[] tokens)
        {
            List<string> names = tokens.ToList();
            var dict = new Dictionary<string, string>();

            WebRequest myReq = WebRequest.Create(url);
            if ((userName != null) && (password != null))
            {
                string credentials = $"{userName}:{password}";
                //CredentialCache mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            }
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            return GetProperties(content, tokens);
        }
        public static Dictionary<string, string> GetProperties(string content, params string[] tokens)
        {
            List<string> names = tokens.ToList();
            var dict = new Dictionary<string, string>();
            var json = content.StartsWith("[") ? content : "[" + content + "]"; // change this to array
            var objects = JArray.Parse(json); // parse as array  
            int numFinds = names.Count();
            foreach (JObject obj in objects.Children<JObject>())
            {
                if (TraverseObject(obj, ref numFinds, names, dict))
                    return dict;
            }
            return dict;
        }
        public static bool TraverseObject(JObject obj, ref int numFinds, List<string> names, Dictionary<string, string> dict, string objPath = "")
        {
            foreach (JProperty p in obj.Properties())
            {
                string fullName = (objPath.Length > 0) ? objPath + "." + p.Name : p.Name;
                var nidx = names.FindIndex(n => (n == fullName));
                if (nidx != -1)
                {
                    dict.Add(fullName, p.Value.ToString());
                    names.RemoveAt(nidx);
                    if (--numFinds == 0)
                        return true;
                }
                if (p.Value.Type == JTokenType.Object)
                {
                    return TraverseObject(p.Value.ToObject<JObject>(), ref numFinds, names, dict, fullName);
                }
            }
            return false;
        }
        public static DateTime ESTNow
        {
            get
            {
                var timeUtc = DateTime.UtcNow;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            }
        }

        public static DateTime ParseEST(string ESTStr)
        {
            return DateTime.ParseExact(ESTStr, "M/d/yy h:mm:ss tt", CultureInfo.InvariantCulture);
        }

        public static DateTime UTCtoEST(DateTime utcDT)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utcDT, easternZone);
        }

        public static string ESTNowStr
        {
            get
            {
                var timeUtc = DateTime.UtcNow;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone).ToString("M/d/yy h:mm:ss tt");
            }
        }
    }
}