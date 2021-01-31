using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
        public PropaneLevel(double levelPct, DateTime readingTime, double outsideTempF)
        {
            LevelPct = levelPct;
            ReadingTime = readingTime;
            OutsideTempF = outsideTempF;
        }

        public String ReadingTimeStr()
        {
            return ReadingTime.ToString("M/d/yy h:mmt");
        }

        public double OutsideTempF { get; set; } = -40;
        public double LevelPct { get; set; } = 0;
        public DateTime ReadingTime { get; set; }
    }

    public class EquipProp
    {
        public const int UNDEFINED_TB = -4321;
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


        public double FreezerTempF { get; set; } = -40;
        public double FridgeTempF { get; set; } = -40;
        public double KitchTempF { get; set; } = -40;
        public double WaterPress { get; set; } = 0;
        public DateTime TimeStamp { get; set; }
    }

    public class ArdMgr
    {
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        private readonly IWebHostEnvironment HostEnv;
        private string FullLatest = "";
        private string FullAll = "";
        private static int MAX_FILE_ARS = 200;
        private static int MAX_USE_ARS = 100;

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

        public string CheckValues(string jsonStr)
        {
            return "";
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

                            return new ArdRecord(props.ContainsKey("freezerTemp") ? props["freezerTemp"] : "-40",
                                                 props.ContainsKey("frigTemp") ? props["frigTemp"] : "-40",
                                                 props.ContainsKey("kitchTemp") ? props["kitchTemp"] : "-40",
                                                 props.ContainsKey("waterPres") ? props["waterPres"] : "-40",
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
                    using (StreamReader sr = new StreamReader(FullAll, System.Text.Encoding.UTF8))
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

                                    ArdRecord ar = new ArdRecord(props.ContainsKey("freezerTemp") ? props["freezerTemp"] : "-40",
                                                         props.ContainsKey("frigTemp") ? props["frigTemp"] : "-40",
                                                         props.ContainsKey("kitchTemp") ? props["kitchTemp"] : "-40",
                                                         props.ContainsKey("waterPres") ? props["waterPres"] : "-40",
                                                         arTime);
                                    arList.Add(ar);
                                }
                            }
                        }
                    }
                    
                    if (lineCount > MAX_FILE_ARS)
                    {
                        using (StreamWriter sw = new StreamWriter(FullAll, false))
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

        public bool RemoveFirstLinesFromFile(string filePath, int skip)
        {
            if (!File.Exists(filePath))
                return false;
            try
            {
                var filePathOld = Path.Combine(filePath, ".old");
                File.Move(filePath, filePathOld);
                File.WriteAllLines(filePath, File.ReadAllLines(filePathOld).Skip(skip));
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        public ArdRecord[] Get(DateTime startDT, DateTime endDT)
        {
            return null;
        }
    }


    public class PropaneMgr
    { 
        private readonly IWebHostEnvironment HostEnv;
        private readonly string FullAll;
        public PropaneMgr(IWebHostEnvironment hostEnv)
        {
            HostEnv = hostEnv;
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "Equip");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            FullAll = Path.Combine(newPath, "propaneall.txt");
        }

        public PropaneLevel GetTUTank(bool onlyAppendToFile=false) // Tank Utility Propane meter
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
                if (onlyAppendToFile)
                {
                    using (StreamWriter sw = new StreamWriter(FullAll, true))
                    {
                        sw.WriteLine(JsonConvert.SerializeObject(pl));
                        sw.Close();
                    }
                }
                else
                    return pl;
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
}
