using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        static string[] DOWStr = { "Sun", "Mon", "Tues", "Wed", "Thu", "Fri", "Sat" };
        public PropaneLevel(double levelPct, DateTime readingTime, double outsideTempF)
        {
            LevelPct = levelPct;
            ReadingTime = readingTime;
            OutsideTempF = outsideTempF;
        }

        public String ReadingTimeStr(bool withDOW=false)
        {
            string str = ReadingTime.ToString("M/d/yy h:mmt");
            return (withDOW) ? DOWStr[(int)ReadingTime.DayOfWeek] + " " + str : str;
        }
        public String ReadingDateStr(bool withDOW = false)
        {
            string str = ReadingTime.ToString("M/d/yy");
            return (withDOW) ? DOWStr[(int)ReadingTime.DayOfWeek] + " " + str : str;
        }

        public bool IsSame(PropaneLevel pl)
        {
            return (ReadingTime == pl.ReadingTime) && (LevelPct == pl.LevelPct);
        }

        public double OutsideTempF { get; set; } = PropMgr.NotSet;
        public double LevelPct { get; set; } = 0;
        public DateTime ReadingTime { get; set; }
    }

    public class EquipProp
    {
        public enum EquipUnitsType {
            [Display(Name = "Temp. Farenh.")]
            TemperatureF =0x10,
            [Display(Name = "Temp. Celc.")]
            TemperatureC =0x11,
            [Display(Name = "PSI")]
            PSI =0x20,
            [Display(Name = "KiloWatt Hrs.")]
            KWH = 0x30,
            [Display(Name = "Percent Full")]
            PercentFull =0x40,
            [Display(Name = "Unknown")]
            Unknown = 0x50
        }
        public string Title { get; set; } = "* UNUSED *";
        public string PropName { get; set; } = "";
        [JsonConverter(typeof(StringEnumConverter))]
        public EquipUnitsType EquipUnits { get; set; } = EquipUnitsType.Unknown;
        public double ChartBottom { get; set; } = 0;
        public double ChartTop { get; set; } = 0;
        public double GreenBottom { get; set; } = 0;
        public double GreenTop { get; set; } = 0;
        public double RedBottom { get; set; } = 0;
        public double RedTop { get; set; } = 0;
        public bool Active { get; set; } = false;
        public double MinsPerSample { get; set; } = 0;
        public string Attributes { get; set; } = "";
        public bool IsPropane ()
        {
            return (this.PropName == "propaneTank");
        }

        public List<KeyValuePair<string, double>> GetProps()
        {
            var kvList = new List<KeyValuePair<string, double>>();
            if (Attributes.Length > 0)
            {
                string[] kvPairs = Attributes.Split(';');
                foreach (var kvp in kvPairs)
                {
                    String[] vals = kvp.Split('=');
                    if ((vals.Length == 2) && double.TryParse(vals[1], out double dval))
                        kvList.Add(new KeyValuePair<string, double>(vals[0], dval));
                }
            }
            return kvList;
        }

        public void UpdateAttributeValues (string[] attrVals, ref int curIdx, int NumAttrVals)
        {
            var kvList = new List<KeyValuePair<string, double>>();
            if (Attributes.Length > 0)
            {
                string newAttributes = "";
                string[] kvPairs = Attributes.Split(';');
                int orgPairs = kvPairs.Length;
                int newPairs = 0;
                foreach (var kvp in kvPairs)
                {
                    String[] vals = kvp.Split('=');
                    if ((vals.Length == 2) && double.TryParse(vals[1], out double dval) && (curIdx < NumAttrVals))
                    {
                        ++newPairs;
                        if (newAttributes.Length == 0)
                            newAttributes = vals[0] + "=" + attrVals[curIdx++];
                        else
                            newAttributes += (";" + vals[0] + "=" + attrVals[curIdx++]);
                    }
                }
                if ((newAttributes.Length > 0) && (orgPairs == newPairs))
                    Attributes = newAttributes;
            }
        }

        public string GetColor(double value, bool returnClass = true)
        {
            string color = "";
            if ((RedTop > GreenTop) && (value > GreenTop))
                color = (value >= RedTop) ? "red" : "yellow";
            else if ((RedBottom < GreenBottom) && (value < GreenBottom))
                color = (value <= RedBottom) ? "red" : "yellow";
            else
                color = "green";

            if (returnClass)
                return color + "Class";
            else
            {
                return color switch
                {
                    "red" => CSSettings.red,
                    "yellow" => CSSettings.yellow,
                    "green" => CSSettings.green,
                    _ => "",
                };
            }
        }
    }
    public class ArdRecord
    {
        public ArdRecord(double freezerTempF, double fridgeTempF, double kitchTempF, double waterPress, DateTime timeStamp)
        {
            FreezerTempF = freezerTempF;
            FridgeTempF = fridgeTempF;
            KitchTempF = kitchTempF;
            WaterPress = waterPress;
            TimeStamp = timeStamp;
        }
        public ArdRecord(string freezerTempF, string fridgeTempF, string kitchTempF, string waterPress, DateTime timeStamp)
        {
            double _FreezerTempF = FreezerTempF;
            if (double.TryParse(freezerTempF, out _FreezerTempF))
                FreezerTempF = _FreezerTempF;
            double _FridgeTempF = FridgeTempF;
            if (double.TryParse(fridgeTempF, out _FridgeTempF))
                FridgeTempF = _FridgeTempF;
            double _KitchTempF = KitchTempF;
            if (double.TryParse(kitchTempF, out _KitchTempF))
                KitchTempF = _KitchTempF;
            double _WaterPress = WaterPress;
            if (double.TryParse(waterPress, out _WaterPress))
                WaterPress = _WaterPress;
            TimeStamp = timeStamp;
        }

        public String TimeStampStr()
        {
            return TimeStamp.ToString("M/d/yy h:mmt");
        }

        public double FreezerTempF { get; set; } = PropMgr.NotSet;
        public double FridgeTempF { get; set; } = PropMgr.NotSet;
        public double KitchTempF { get; set; } = PropMgr.NotSet;
        public double WaterPress { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
    }

    public class ArdMgr
    {
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;

        private string FullLatest = "";
        private string ArdFullAll = "";
        private static int MAX_FILE_ARS = 200;
        public static int MAX_USE_ARS = 100;

        public ArdMgr(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "Equip");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            FullLatest = Path.Combine(newPath, "ardlatest.txt");
            ArdFullAll = Path.Combine(newPath, "ardall.txt");
        }

        public bool CheckValues(string jsonStr) // Ard only : No propane
        {
            ArdRecord ar = GetArdRecord(jsonStr);
            CSSettings cset = CSSettings.GetCSSettings(Config, UserManager);

            foreach (var ep in cset.EquipProps)
            {
                if (ep.IsPropane() || !ep.Active)
                    continue;
                if (CSSettings.GetColor(cset.EquipProps, ep.PropName, ar, null, false) != CSSettings.green)
                {
                    CSSMS sms = new CSSMS(HostEnv, Config, UserManager);
                    sms.NotifyUsers(CSSMS.NotifyType.EquipNT, "CStat:Equip> " + ep.Title + " is " + CSSettings.GetEqValueStr(ep, ar, null, false), true, false); // Allow resend
                }
            }
            return true;
        }

        public bool ReportLastestValues(ref string report, bool alertsOnly) // Ard only : No propane
        {
            ArdRecord ar = GetLast();
            CSSettings cset = CSSettings.GetCSSettings(Config, UserManager);
            foreach (var ep in cset.EquipProps)
            {
                if (ep.IsPropane() || !ep.Active)
                    continue;
                if (!alertsOnly || CSSettings.GetColor(cset.EquipProps, ep.PropName, ar, null, false) != CSSettings.green)
                {
                    report += "* " + ep.Title + " is " + CSSettings.GetEqValueStr(ep, ar, null, false) + "\n";
                }
            }
            return true;
        }

        public int Set(string jsonStr)
        {
            if (ArdMgr.fLock.TryEnterWriteLock(250))
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

                    using (StreamWriter sw = new StreamWriter(ArdFullAll, true))
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
            if (ArdMgr.fLock.TryEnterWriteLock(250))
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
                            Dictionary<string, string> props = PropMgr.GetProperties(latest,
                                "freezerTemp",
                                "frigTemp",
                                "kitchTemp",
                                "waterPres",
                                "time"
                                );

                            DateTime arTime = (props["time"] != null) ? PropMgr.ParseEST(props["time"]) : PropMgr.MissingDT;

                            return new ArdRecord(props.ContainsKey("freezerTemp") ? props["freezerTemp"] : PropMgr.sNotSet,
                                                 props.ContainsKey("frigTemp") ? props["frigTemp"] : PropMgr.sNotSet,
                                                 props.ContainsKey("kitchTemp") ? props["kitchTemp"] : PropMgr.sNotSet,
                                                 props.ContainsKey("waterPres") ? props["waterPres"] : PropMgr.sNotSet,
                                                 arTime);
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
        public ArdRecord GetArdRecord (string raw)
        {
            string latest = (raw.StartsWith("[") || raw.StartsWith("{")) ? raw.Trim() : "{" + raw.Trim() + "}";
            Dictionary<string, string> props = PropMgr.GetProperties(latest,
                "freezerTemp",
                "frigTemp",
                "kitchTemp",
                "waterPres",
                "time"
                );

            DateTime arTime;
            if (props.TryGetValue("time", out string arTimeStr))
                arTime = PropMgr.ParseEST(arTimeStr);
            else
                arTime = PropMgr.ESTNow;
            return new ArdRecord(props.ContainsKey("freezerTemp") ? props["freezerTemp"] : PropMgr.sNotSet,
                                 props.ContainsKey("frigTemp") ? props["frigTemp"] : PropMgr.sNotSet,
                                 props.ContainsKey("kitchTemp") ? props["kitchTemp"] : PropMgr.sNotSet,
                                 props.ContainsKey("waterPres") ? props["waterPres"] : PropMgr.sNotSet,
                                 arTime);
        }

        public List<ArdRecord> GetAll(bool justCheckSize=false)
        {
            List<ArdRecord> arList = new List<ArdRecord>();
            List<string> lineList = new List<string>();
            int lineCount=0;
            int startIndex=0;
            if (ArdMgr.fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(ArdFullAll, System.Text.Encoding.UTF8))
                    {
                        string raw;
                        while ((raw = sr.ReadLine()) != null)
                        {
                            lineList.Add(raw);
                        }
                        sr.Close();

                        lineCount = lineList.Count;
                        startIndex = lineList.Count > MAX_USE_ARS ? lineCount - MAX_USE_ARS : 0;

                        if (!justCheckSize)
                        {
                            for (int i = startIndex; i < lineCount; ++i)
                            {
                                raw = lineList[i];
                                if (raw.Length > 20)
                                {
                                    arList.Add(GetArdRecord(raw));
                                }
                            }
                        }
                    }
                    
                    if (lineCount > MAX_FILE_ARS)
                    {
                        using (StreamWriter sw = new StreamWriter(ArdFullAll, false))
                        {
                            for (int i = startIndex; i < lineCount; ++i)
                            {
                                sw.WriteLine(lineList[i]);
                            }
                            sw.Close();
                        }
                    }

                    return arList;
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

        //public bool RemoveFirstLinesFromFile(string filePath, int skip)
        //{
        //    if (!File.Exists(filePath))
        //        return false;
        //    try
        //    {
        //        var filePathOld = Path.Combine(filePath, ".old");
        //        File.Move(filePath, filePathOld);
        //        File.WriteAllLines(filePath, File.ReadAllLines(filePathOld).Skip(skip));
        //        return true;
        //    }
        //    catch (System.Exception)
        //    {
        //        return false;
        //    }
        //}

        //public ArdRecord[] Get(DateTime startDT, DateTime endDT)
        //{
        //    return null;
        //}
    }

    public class PropaneMgr
    { 
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly CSSettings _cset;
        private readonly UserManager<CStatUser> UserManager;
        private readonly string PropaneFullAll, PropaneHistAll;
        private readonly double _TankGals, _PricePerGal;
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        public const int MAX_USE_PLS = 400;
        private const int MAX_FILE_PLS = 600;

        public PropaneMgr(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "Equip");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            PropaneFullAll = Path.Combine(newPath, "propaneall.txt");
            PropaneHistAll = Path.Combine(newPath, "propaneHist.txt");
            _cset = CSSettings.GetCSSettings(config, userManager);
            _cset.GetPropaneProperties(out _TankGals, out _PricePerGal);
        }

        public bool CheckValue(PropaneLevel pl) // propane only
        {
            CSSettings cset = CSSettings.GetCSSettings(Config, UserManager);
            foreach (var ep in cset.EquipProps)
            {
                if (!ep.IsPropane())
                    continue;
                if (CSSettings.GetColor(cset.EquipProps, ep.PropName, null, pl, false) != CSSettings.green)
                {
                    CSSMS sms = new CSSMS(HostEnv, Config, UserManager);
                    sms.NotifyUsers(CSSMS.NotifyType.EquipNT, "CStat:Equip> " + ep.Title + "is " + CSSettings.GetEqValueStr(ep, null, pl, false), true, false); // Allow resend
                }
            }
            return true;
        }

        public bool ReportLatestValue(ref string report) // propane only
        {
            var pl = this.GetTUTank();
            CSSettings cset = CSSettings.GetCSSettings(Config, UserManager);
            foreach (var ep in cset.EquipProps)
            {
                if (!ep.IsPropane())
                    continue;
                if (CSSettings.GetColor(cset.EquipProps, ep.PropName, null, pl, false) != CSSettings.green)
                {
                    CSSMS sms = new CSSMS(HostEnv, Config, UserManager);
                    report += "CStat:Equip> " + ep.Title + "is " + CSSettings.GetEqValueStr(ep, null, pl, false) + "\n";
                }
            }
            return true;
        }

        public PropaneLevel GetTUTank(bool appendToFile=false) // Tank Utility Propane meter
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
                PropaneLevel pl = new PropaneLevel(level, PropMgr.UTCtoEST(readTime), temp);
                if (appendToFile)
                {
                    using (StreamWriter sw = new StreamWriter(PropaneFullAll, true))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(pl));
                        sw.Close();
                    }
                    using (StreamWriter sw = new StreamWriter(PropaneHistAll, true))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(pl));
                        sw.Close();
                    }
                }
                return pl;
            }
            return null;
        }

        public double PctToGals(double pct)
        {
            return pct * _TankGals/100;
        }

        public double PctToPrice(double pct)
        {
            return pct * _TankGals/100 * _PricePerGal;
        }

        public double GalsToPrice(double gals)
        {
            return gals * _PricePerGal;
        }

        public List<PropaneLevel> GetAll(int maxUsePls= MAX_USE_PLS)
        {
            List<PropaneLevel> plList = new List<PropaneLevel>();
            List<string> rLineList = new List<string>();
            List<string> lineList = new List<string>();
            int lineCount = 0;
            int startIndex = 0;
            if (fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(PropaneFullAll, System.Text.Encoding.UTF8))
                    {
                        string raw;
                        while ((raw = sr.ReadLine()) != null)
                        {
                            rLineList.Add(raw);
                        }
                        sr.Close();

                        lineList = rLineList.Distinct().ToList();
                        lineCount = lineList.Count;
                        startIndex = lineList.Count > maxUsePls ? lineCount - maxUsePls : 0;

                        for (int i = startIndex; i < lineCount; ++i)
                        {
                            raw = lineList[i];
                            if (raw.Length > 20)
                            {
                                string latest = (raw.StartsWith("[") || raw.StartsWith("{")) ? raw.Trim() : "{" + raw.Trim() + "}";
                                Dictionary<string, string> props = PropMgr.GetProperties(latest,
                                                            "LevelPct",
                                                            "OutsideTempF",
                                                            "ReadingTime");
                                double level = 0;
                                double temp = PropMgr.NotSet;
                                DateTime readTime;
                                if (double.TryParse(props["LevelPct"], out level) && DateTime.TryParse(props["ReadingTime"], out readTime) && double.TryParse(props["OutsideTempF"], out temp))
                                {
                                    PropaneLevel pl = new PropaneLevel(level, readTime, temp);
                                    plList.Add(pl);
                                }
                            }
                        }
                    }

                    if (rLineList.Count > MAX_FILE_PLS)
                    {
                        using (StreamWriter sw = new StreamWriter(PropaneFullAll, false))
                        {
                            for (int i = startIndex; i < lineCount; ++i)
                            {
                                sw.WriteLine(lineList[i]);
                            }
                            sw.Close();
                        }
                    }

                    return plList;
                }
                catch
                {

                }
                finally
                {
                    PropaneMgr.fLock.ExitWriteLock();
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public List<PropaneLevel> GetRecentToLastList()
        {
            List<PropaneLevel> plList = new List<PropaneLevel>();
            List<string> rLineList = new List<string>();
            List<string> lineList = new List<string>();
            int lineCount = 0;
            if (fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(PropaneFullAll, System.Text.Encoding.UTF8))
                    {
                        string raw;
                        while ((raw = sr.ReadLine()) != null)
                        {
                            rLineList.Add(raw);
                        }
                        sr.Close();

                        lineList = rLineList.Distinct().ToList();
                        lineCount = lineList.Count;
                        int startIndex = lineList.Count > MAX_USE_PLS ? lineCount - MAX_USE_PLS : 0;

                        for (int i = startIndex; i < lineCount; ++i)
                        {
                            raw = lineList[i];
                            if (raw.Length > 20)
                            {
                                string latest = (raw.StartsWith("[") || raw.StartsWith("{")) ? raw.Trim() : "{" + raw.Trim() + "}";
                                Dictionary<string, string> props = PropMgr.GetProperties(latest,
                                                            "LevelPct",
                                                            "OutsideTempF",
                                                            "ReadingTime");
                                double level = 0;
                                double temp = PropMgr.NotSet;
                                DateTime readTime;
                                if (double.TryParse(props["LevelPct"], out level) && DateTime.TryParse(props["ReadingTime"], out readTime) && double.TryParse(props["OutsideTempF"], out temp))
                                {
                                    PropaneLevel pl = new PropaneLevel(level, readTime, temp);
                                    plList.Add(pl);
                                }

                            }
                        }
                        DateTime baseDT = new DateTime(2020, 1, 1, 0, 0, 0, 0);
                        var ordPList = plList.OrderByDescending(p => p.ReadingTime.Subtract(baseDT).TotalSeconds).ToList();
                        var len = ordPList.Count;
                        var _pl = GetTUTank(); // get latest
                        if ((_pl != null) && (len > 0) && (ordPList[0].ReadingTime < _pl.ReadingTime))
                            ordPList.Insert(0, _pl); // Add latest to the front of the list
                        return ordPList;
                    }
                }
                catch
                {
                    return null;
                }
                finally
                {
                    PropaneMgr.fLock.ExitWriteLock();
                }
            }
            else
            {
                return null;
            }
        }

        public double GetPropaneGallonsForDateRange(DateTime startDate, DateTime endDate, double maxGallons)
        {
            List<PropaneLevel> plList = new List<PropaneLevel>();
            List<string> rLineList = new List<string>();
            List<string> lineList = new List<string>();
            int lineCount = 0;
            int startIndex = 0;
            if (fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(PropaneFullAll, System.Text.Encoding.UTF8))
                    {
                        string raw;
                        while ((raw = sr.ReadLine()) != null)
                        {
                            rLineList.Add(raw);
                        }
                        sr.Close();

                        lineList = rLineList.Distinct().ToList();
                        lineCount = lineList.Count;
                        startIndex = lineList.Count > MAX_USE_PLS ? lineCount - MAX_USE_PLS : 0;

                        DateTime MinDateTime = startDate.AddDays(-2);
                        DateTime MaxDateTime = endDate.AddDays(2);
                        PropaneLevel ClosestToMin = null;
                        PropaneLevel ClosestToMax = null;
                        TimeSpan ClosestToMinDiff = new TimeSpan(30, 0, 0, 0);
                        TimeSpan ClosestToMaxDiff = new TimeSpan(30, 0, 0, 0);

                        for (int i = startIndex; i < lineCount; ++i)
                        {
                            raw = lineList[i];
                            if (raw.Length > 20)
                            {
                                string latest = (raw.StartsWith("[") || raw.StartsWith("{")) ? raw.Trim() : "{" + raw.Trim() + "}";
                                Dictionary<string, string> props = PropMgr.GetProperties(latest,
                                                            "LevelPct",
                                                            "OutsideTempF",
                                                            "ReadingTime");
                                double level = 0;
                                double temp = PropMgr.NotSet;
                                DateTime readTime;
                                if (double.TryParse(props["LevelPct"], out level) && DateTime.TryParse(props["ReadingTime"], out readTime) && double.TryParse(props["OutsideTempF"], out temp))
                                {
                                    PropaneLevel pl = new PropaneLevel(level, readTime, temp);
                                   
                                    if ((pl.ReadingTime >= MinDateTime) && (pl.ReadingTime <= MaxDateTime))
                                    {
                                        TimeSpan minDiff = startDate - pl.ReadingTime;
                                        if ((minDiff.TotalSeconds >= 0) && (minDiff.TotalSeconds < ClosestToMinDiff.TotalSeconds))
                                        {
                                            ClosestToMinDiff = minDiff;
                                            ClosestToMin = pl;
                                        } 
                                        else
                                        {
                                            TimeSpan maxDiff = pl.ReadingTime - endDate;
                                            if ((maxDiff.TotalSeconds >= 0) && (maxDiff.TotalSeconds < ClosestToMaxDiff.TotalSeconds))
                                            {
                                                ClosestToMaxDiff = maxDiff;
                                                ClosestToMax = pl;
                                            }
                                        }

                                        plList.Add(pl);
                                    }
                                }

                            }
                        }
                        if ((ClosestToMin != null) && (ClosestToMax != null))
                        {
                            double galDiff = ClosestToMin.LevelPct - ClosestToMax.LevelPct;
                            return (galDiff < 0) ? 0 : galDiff * maxGallons / 100;
                        }
                        return 0;
                    }
                  
                }
                catch
                {

                }
                finally
                {
                    PropaneMgr.fLock.ExitWriteLock();
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }
    }
    public static class PropMgr
    {
        public const int NotSet = -99;
        public const string sNotSet = "-99";
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

        public static DateTime MissingDT
        {
            get
            {
                return DateTime.ParseExact("1/1/1900 12:00:00 AM", "M/d/yy h:mm:ss tt", CultureInfo.CurrentCulture);
            }
        }

        public static string MissingDTStr
        {
            get
            {
                return "1/1/1900 12:00:00 AM";
            }
        }
    }

    public class Power
    {
        public DateTime Day { get; set; } = default;
        public double AKwHrs { get; set; } = 0;
        public double BKwHrs { get; set; } = 0;
        public double TotalKwHrs { get; set; } = 0;
    }
    public class PowerMgr
    {
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public bool TryGetKWHrs(DateTime startDT, DateTime endDT, out double totKWHrs)
        {
            totKWHrs = 0;
            try
            {
                // Get Session Id
                HttpReq req = new HttpReq();
                req.Open("POST", "https://my.eyedro.com/e2");

                req.AddHeaderProp("Host: my.eyedro.com");
                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("sec-ch-ua: \" Not A; Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\"");
                req.AddHeaderProp("X-Requested-With: XMLHttpRequest");
                req.AddHeaderProp("sec-ch-ua-mobile: ?0");
                req.AddHeaderProp("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36");
                req.AddHeaderProp("sec-ch-ua-platform: \"Windows\"");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Origin: https://my.eyedro.com");
                req.AddHeaderProp("Sec-Fetch-Site: same-origin");
                req.AddHeaderProp("Sec-Fetch-Mode: cors");
                req.AddHeaderProp("Sec-Fetch-Dest: empty");
                req.AddHeaderProp("Referer: https://my.eyedro.com/");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                req.AddHeaderProp("Accept-Language: en-US,en;q=0.9");
                req.AddHeaderProp("Cookie: _gcl_au=1.1.1412530007.1638932992; _ga=GA1.2.2142543981.1638932992; aelia_cs_selected_currency=USD; _gid=GA1.2.1674436603.1639189476");

                req.AddBody("Cmd=Login&Username=ronripp@outlook.com&PwdMd5=3458f2736dff802f3abb225ab09706ad&SessionId=&ApiKey=", "application/x-www-form-urlencoded; charset=UTF-8");
                var sRespStat = req.Send(out string sResult);

                // "SessionId": "120218346240b5ff9dc55e894b6cbe3fa8d70153"
                int index = sResult.IndexOf("\"SessionId\"");
                if (index == -1)
                    return false;
                int sidx = sResult.IndexOf("\"", index + 12);
                if (sidx == -1)
                    return false;
                int eidx = sResult.IndexOf("\"", sidx + 1);
                if (eidx == -1)
                    return false;
                var SessionId = sResult.Substring(sidx + 1, (eidx - sidx) - 1);
                if (string.IsNullOrEmpty(SessionId))
                    return false;

                // Get Daily Readings
                HttpReq greq = new HttpReq();

                string DateRangeStr = ""; // StartDateCode=211111&DateNumSteps=31
                int days = (endDT - startDT).Days + 1;
                DateRangeStr = "StartDateCode=" + (startDT.Year % 100).ToString() + startDT.Month.ToString() + startDT.Day.ToString() + "&DateNumSteps=" + days;

                //greq.Open("GET", "https://my.eyedro.com/e2?Cmd=GetData&DataFormat=csv&DataTypeId=7&DateStepSizeId=4&StartDateCode=211111&DateNumSteps=31&SessionId=" + SessionId + "&AliasRId=1&SiteId=1");
                greq.Open("GET", "https://my.eyedro.com/e2?Cmd=GetData&DataFormat=csv&DataTypeId=7&DateStepSizeId=4&" + DateRangeStr + "&SessionId=" + SessionId + "&AliasRId=1&SiteId=1");

                greq.AddHeaderProp("Host: my.eyedro.com");
                greq.AddHeaderProp("Connection: keep-alive");
                greq.AddHeaderProp("sec-ch-ua: \" Not A; Brand\";v=\"99\", \"Chromium\";v=\"96\", \"Google Chrome\";v=\"96\"");
                greq.AddHeaderProp("sec-ch-ua-mobile: ?0");
                greq.AddHeaderProp("sec-ch-ua-platform: \"Windows\"");
                greq.AddHeaderProp("Upgrade-Insecure-Requests: 1");
                greq.AddHeaderProp("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.93 Safari/537.36");
                greq.AddHeaderProp("Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                greq.AddHeaderProp("Sec-Fetch-Site: same-origin");
                greq.AddHeaderProp("Sec-Fetch-Mode: navigate");
                greq.AddHeaderProp("Sec-Fetch-User: ?1");
                greq.AddHeaderProp("Sec-Fetch-Dest: iframe");
                greq.AddHeaderProp("Referer: https://my.eyedro.com/");
                greq.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                greq.AddHeaderProp("Accept-Language: en-US,en;q=0.9");
                greq.AddHeaderProp("Cookie: _gcl_au=1.1.1412530007.1638932992; _ga=GA1.2.2142543981.1638932992; aelia_cs_selected_currency=USD; _gid=GA1.2.1674436603.1639189476");
                var respStat = greq.Send(out string result);

                // Parse Records then total kwHrs.
                // make sure time does not exclude days from total

                var readings = result.Split($"\n");
                List<Power> powerList = new List<Power>();

                foreach (var rd in readings.Skip(1))
                {
                    //    Date,               Epoch Date,  A  B, Total
                    //05-Dec-2021 12:00:00 AM,1638680400,97,97,194
                    var flds = rd.Split(",");
                    if (flds.Length == 5)
                    {
                        var power = new Power();
                        long epoc = 0;
                        if (long.TryParse(flds[1], out epoc))
                            power.Day = FromUnixTime(epoc);
                        double dval = 0;
                        if (double.TryParse(flds[2], out dval))
                            power.AKwHrs = dval;
                        if (double.TryParse(flds[3], out dval))
                            power.BKwHrs = dval;
                        if (double.TryParse(flds[4], out dval))
                            power.TotalKwHrs = dval;
                        powerList.Add(power);
                    }
                }

                // Ensure Date Range is in List
                int NumDays = (endDT.Date - startDT.Date).Days + 1;
                int totDays = 0;
                foreach (var pow in powerList)
                {
                    if (pow.Day.Date >= startDT.Date)
                    {
                        if (pow.Day.Date > endDT.Date)
                            break;
                        totKWHrs += pow.TotalKwHrs;
                        ++totDays;
                    }
                }
                if (totDays != NumDays)
                    throw new Exception();
            }
            catch
            {
                totKWHrs = 0;
                return false;
            }
            return true;
        }
    }
}
