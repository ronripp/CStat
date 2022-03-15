using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class MonthStatus
    {
        public int mon;
        public string table;
        public int year;
    }

    public class DateTimeV
    {
        public int day;  // 6
        public int hour; // 2
        public int min;  // 1
        public int mon;  // 3
        public int sec;  // 18
        public int year; // 2022
    }

    public class VideoFile
    {
        public string name;      // "Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4",
        public string type;      // "main"
        public long size;        // 42081868
        public int frameRate;    // 0
        public int width;        // 0
        public int height;       // 0
        public DateTimeV StartTime;
        public DateTimeV EndTime;
    }

    public class VideoSearch
    {
        public VideoFile[] File;
        public MonthStatus[] Status;
        public int channel;
    }

    public class SearchRes
    {
        public VideoSearch SearchResult;
    }

    public class SearchCmd
    {
        public string cmd;
        public int code;
        public SearchRes value;
    }

public class PtzCamera : System.IDisposable
    {
        public enum COp
        {
            None = -1,
            Refresh = 0,
            Preset1 = 1,
            Preset2 = 2,
            Preset3 = 3,
            Preset4 = 4,
            Preset5 = 5,
            Preset6 = 6,
            ToggleLight = 100,
            TurnOnAlarm = 101,
            UL = 200,
            UC = 201,
            UR = 202,
            CL = 203,
            CR = 204,
            LL = 205,
            LC = 206,
            LR = 207,
            ZoomDec = 300,
            ZoomInc = 301,
            FocusDec = 400,
            FocusInc = 401
        };

        public static Dictionary<COp, string> COpToStr = new Dictionary<COp, string>
        {
            {COp.UL       ,"LeftUp"},
            {COp.UC       ,"Up"},
            {COp.UR       ,"RightUp"},
            {COp.CL       ,"Left"},
            {COp.CR       ,"Right"},
            {COp.LL       ,"LeftDown"},
            {COp.LC       ,"Down"},
            {COp.LR       ,"RightDown"},
            {COp.ZoomDec  ,"ZoomDec"},
            {COp.ZoomInc  ,"ZoomInc"},
            {COp.FocusDec ,"FocusDec"},
            {COp.FocusInc ,"FocusInc" }
        };

        public PtzCamera()
        {
            Login();
        }

        //***************** start DISPOSABLE **********************
        private bool disposed;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose managed objects
                Logout();
            }
            // Dispose unmanaged objects

            disposed = true;
        }

        //********************* end DISPOSABLE *****************************

        ~PtzCamera()
        {
            if (!string.IsNullOrEmpty(_token))
                Logout();
        }

        public string _token = "";
        public bool Login()
        {
            try
            {
                // Get Token
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=Login");

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\":\"Login\",\"param\":{\"User\":{\"userName\":\"admin\", \"password\":\"Red35845!\" }}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "name" : "8da4c31df166a94"
                dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                _token = LoginResp[0].value.Token.name;
                return !string.IsNullOrEmpty(_token);
            }
            catch
            {
                return false;
            }
        }

        public bool Logout()
        {
            if (string.IsNullOrEmpty(_token))
                return false;

            var tok = _token;
            _token = "";
            try
            {
                // Get Token
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=Logout&token=" + tok);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\":\"Logout\",\"param\":{}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "name" : "8da4c31df166a94"
                dynamic LogoutResp = JsonConvert.DeserializeObject(sResult);
                return LogoutResp[0].value.rspCode == 200;
            }
            catch
            {
              
            }
            return false;
        }

        public int GetPresetPicture(IWebHostEnvironment hostEnv, int preset)
        {
            try
            {
                if ((preset >= 1) && (preset <= 99))
                {

                    // Get Picture by Preset #
                    HttpReq req = new HttpReq();
                    req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);

                    req.AddHeaderProp("Connection: keep-alive");
                    req.AddHeaderProp("Accept: */*");
                    req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                    req.AddBody("[{\"cmd\":\"PtzCtrl\",\"param\":{\"channel\":0,\"op\":\"ToPos\",\"id\":" + preset + ",\"speed\":32}}]", "application/json; charset=utf-8");
                    var sRespStat = req.Send(out string sResult);

                    // "rspCode" : 200
                    dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                    if (LoginResp[0].value.rspCode != 200)
                        return 0;

                    return 6000;
                }
                return 0;
            }
            catch (Exception e)
            {
                _ = e;
                return 0;
            }
        }
        public int GetPtzPicture(IWebHostEnvironment hostEnv, int op)
        {
            try
            {
                if (op < 200)
                    return 0;

                // Perform PTZ op
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token); // &user=admin&password=Red35845!");

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                req.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"" + COpToStr[(COp)op] + "\",\"speed\":32}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "rspCode" : 200
                dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                if ((LoginResp[0].value != null) && (LoginResp[0].value.rspCode != 200))
                    return 0;

                Thread.Sleep(100);

                // Perform Stop
                HttpReq req2 = new HttpReq();
                req2.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token); // &user=admin&password=Red35845!");

                req2.AddHeaderProp("Connection: keep-alive");
                req2.AddHeaderProp("Accept: */*");
                req2.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                req2.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"Stop\"}}]", "application/json; charset=utf-8");

                var sRespStat2 = req2.Send(out string sResult2);

                // "rspCode" : 200
                dynamic LoginResp2 = JsonConvert.DeserializeObject(sResult2);
                if ((LoginResp[0].value != null) && (LoginResp[0].value.rspCode != 200))
                    return 0;

                return 6000;
            }
            catch (Exception e)
            {
                _ = e;
                return 0;
            }
        }

        public int ExecuteOp(IWebHostEnvironment hostEnv, COp cop)
        {
            if ((cop == COp.None) || (cop == COp.Refresh))
                return 0;
            if ((int)cop <= 99)
                return GetPresetPicture(hostEnv, (int)cop);

            if (cop == PtzCamera.COp.ToggleLight)
            {
                return ToggleLight();
            }
            if (cop == PtzCamera.COp.TurnOnAlarm)
            {
                return TurnOnAlarm(10);

            }
            return GetPtzPicture(hostEnv, (int)cop);
        }
        private string GetTempDir(IWebHostEnvironment hostEnv)
        {
            string folderName = @"Camera\Images";
            string webRootPath = hostEnv.WebRootPath;
            string TempPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            return TempPath;
        }
        public string GetSnapshot(IWebHostEnvironment hostEnv)
        {
            try
            {
                var tempPath = GetTempDir(hostEnv);

                var now = PropMgr.ESTNow;
                string baseFilename = "Cam" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
                string actFilename = baseFilename + ".jpg";
                string fullPath = Path.Combine(tempPath, actFilename);
                for (int i = 1; File.Exists(fullPath); ++i)
                {
                    actFilename = baseFilename + i.ToString("00") + ".jpg";
                    fullPath = Path.Combine(tempPath, actFilename);
                }

                //HttpReq req = new HttpReq();
                //req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);

                //req.AddHeaderProp("Connection: keep-alive");
                //req.AddHeaderProp("Accept: */*");
                //req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                //req.Send(out string sResult);

                byte[] content;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token);
                WebResponse response = request.GetResponse();
                Stream stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    content = br.ReadBytes(10000000);
                    br.Close();
                }
                response.Close();

                FileStream fs = new FileStream(fullPath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                try
                {
                    bw.Write(content);
                }
                finally
                {
                    fs.Close();
                    bw.Close();
                }
                return "Camera/Images/" + actFilename;  // Send back URL
            }
            catch
            {
                return "";
            }
        }
        public string GetVideo(IWebHostEnvironment hostEnv, string url)
        {
            try
            {
                CleanupMP4(hostEnv); // Delete any older MP4s as they take up a lot of room.

                var tempPath = GetTempDir(hostEnv);

                var now = PropMgr.ESTNow;
                string baseFilename = "Vid" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
                string actFilename = baseFilename + ".mp4";
                string fullPath = Path.Combine(tempPath, actFilename);
                for (int i = 1; File.Exists(fullPath); ++i)
                {
                    actFilename = baseFilename + i.ToString("00") + ".mp4";
                    fullPath = Path.Combine(tempPath, actFilename);
                }

                //HttpReq req = new HttpReq();
                //req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);

                //req.AddHeaderProp("Connection: keep-alive");
                //req.AddHeaderProp("Accept: */*");
                //req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                //req.Send(out string sResult);

                byte[] content;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Download&source=" + url + "&output=" + url + "&token=" + _token);
                using (WebResponse response = request.GetResponse())
                {
                    long bytesToRead = response.ContentLength + 5000;
                    if (bytesToRead > (100000000 + 5000))
                    {
                        response.Close();
                        return "";
                    }

                    using (Stream stream = response.GetResponseStream())
                    {
                        using (BinaryReader br = new BinaryReader(stream))
                        {
                            content = br.ReadBytes((int)bytesToRead);
                            br.Close();
                        }
                        response.Close();

                        FileStream fs = new FileStream(fullPath, FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        try
                        {
                            bw.Write(content);
                        }
                        finally
                        {
                            fs.Close();
                            bw.Close();
                        }
                        return "Camera/Images/" + actFilename;  // Send back URL
                    }
                }
            }
            catch (Exception e)
            {
                return "";
            }
        }

        public string GetVideoAnchors(DateTime sdt, DateTime edt, ref int ancCount)
        {
            string allAnchors = "";
            try
            {
                DateTime sf = new DateTime(sdt.Year, sdt.Month, sdt.Day, 0, 0, 0);
                DateTime ef = new DateTime(edt.Year, edt.Month, edt.Day, 23, 59, 59);
                SearchCmd scmd = GetVideos(sf, ef);

                if (sdt.Date == edt.Date)
                {
                    if ((scmd.value == null) || (scmd.value.SearchResult == null) || (scmd.value.SearchResult.File == null) || (scmd.value.SearchResult.File.Length == 0))
                        return "";

                    // Return anchors for this day
                    foreach (var vf in scmd.value.SearchResult.File)
                    {
                        string timeStr;
                        if (vf.StartTime.hour > 12)
                            timeStr = (vf.StartTime.hour - 12) + ":" + vf.StartTime.min.ToString("00") + " PM]";
                        else
                            timeStr = vf.StartTime.hour + ":" + vf.StartTime.min.ToString("00") + " AM]";

                        //http://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Playback&source=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&output=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&user=admin&password=Red35845!

                        string title = " [" + vf.StartTime.mon + "/" + vf.StartTime.day + " @" + timeStr;
                        if (ancCount++ % 3 == 0)
                            allAnchors += "</tr><tr>\n";

                        allAnchors += "<td><a href=\"#\" onclick=\"getVideo('" + vf.name + "','" + title + "')\">" + title + "</a></td>\n";
                    }
                    return allAnchors;
                }

                // Determine Days that have videos
                if ((scmd.value.SearchResult == null) || (scmd.value.SearchResult.Status == null) || (scmd.value.SearchResult.Status.Count() <= 0))
                    return "";
                int year = sdt.Year;
                var lastMonth = sdt.Month;
                var vDays = new List<DateTime>();
                foreach (var rmon in scmd.value.SearchResult.Status)
                {
                    if (rmon.mon < lastMonth)
                        year = edt.Year;

                    char[] dayArr = rmon.table.ToCharArray();
                    for (int i = 0; i < dayArr.Length; ++i)
                    {
                        if (dayArr[i] == '1')
                        {
                            var vDay = new DateTime(year, rmon.mon, i + 1);
                            if (vDay.Date < sdt.Date)
                                continue;
                            if (vDay.Date > edt.Date)
                                break;
                            vDays.Add(vDay);
                        }
                    }
                    lastMonth = rmon.mon;
                }

                allAnchors = "<table id=\"vtid\"><tr>\n";
                ancCount = 0;

                // Get Links for each day from newer to older
                var NumVD = vDays.Count;
                for (int i = NumVD-1; i >= 0; --i)
                {
                    allAnchors += GetVideoAnchors(vDays[i], vDays[i], ref ancCount);
                }

                allAnchors += "</tr></table>\n";

                return allAnchors;
            }
            catch
            {

            }

            return "";
        }

        public SearchCmd GetVideos(DateTime sdt, DateTime edt) // TBD : Dates
        {
            try
            {
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Search&rs=adshf4549f&user=admin&password=Red35845!");

                req.AddHeaderProp("content-type: application/json");
                req.AddHeaderProp("accept: application/json");
                req.AddHeaderProp("accept-encoding: en-US,en;q=0.8");
                req.AddHeaderProp("user-agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
//                req.AddBody("[{\"cmd\": \"Search\",\"action\": 0,\"param\": {\"Search\": {\"channel\": 0,\"onlyStatus\": 0,\"streamType\": \"main\",\"StartTime\": {\"year\": 2022,\"mon\": 3,\"day\": 6,\"hour\": 1,\"min\": 0,\"sec\": 0},\"EndTime\": {\"year\": 2022,\"mon\": 3,\"day\": 6,\"hour\": 15,\"min\": 0,\"sec\": 0}}}}]");
                req.AddBody("[{\"cmd\": \"Search\",\"action\": 0,\"param\": {\"Search\": {\"channel\": 0,\"onlyStatus\": 0,\"streamType\": \"main\",\"StartTime\": {\"year\": " + sdt.Year + ",\"mon\": " + sdt.Month + ",\"day\": " + sdt.Day + ",\"hour\": " + sdt.Hour + ",\"min\": " + sdt.Minute + ",\"sec\": " + sdt.Second + "},\"EndTime\": {\"year\": " + edt.Year + ",\"mon\": " + edt.Month + ",\"day\": " + edt.Day + ",\"hour\": " + edt.Hour + ",\"min\": " + edt.Minute + ",\"sec\": " + edt.Second + "}}}}]");
                var resp = req.Send(out string sResult);
                SearchCmd [] searchCmds =  JsonConvert.DeserializeObject<SearchCmd[]>(sResult);
                return (searchCmds.Count() >= 1) ? searchCmds[0] : null;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public int ToggleLight()
        {
            try
            {
                // Get Light State
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=GetWhiteLed&token=" + _token);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\": \"GetWhiteLed\",\"action\": 0,\"param\": {\"channel\": 0}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // value.PowerLed.state : 0 | 1
                dynamic GWLResp = JsonConvert.DeserializeObject(sResult);
                var newState = (GWLResp[0].value.WhiteLed.state == 0) ? 1 : 0;
                var SetJson = JsonConvert.SerializeObject(GWLResp);

                HttpReq req2 = new HttpReq();
                req2.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=SetWhiteLed&token=" + _token);

                req2.AddHeaderProp("Connection: keep-alive");
                req2.AddHeaderProp("Accept: */*");
                req2.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req2.AddBody("[{\"cmd\": \"SetWhiteLed\",\"param\": {\"WhiteLed\": {\"state\":" + newState + ",\"channel\": 0,\"mode\": 1,\"bright\": 85,\"LightingSchedule\": {\"EndHour\": 6,\"EndMin\": 0,\"StartHour\": 18,\"StartMin\": 0},\"wlAiDetectType\": {\"dog_cat\": 1,\"face\": 0,\"people\": 1,\"vehicle\": 0}}}}]", "application /json; charset=utf-8");
                sRespStat = req2.Send(out sResult);

                return 6000; // need time to turn off light. This may be reduced.
            }
            catch (Exception e)
            {
                _ = e;
            }
            return 0;
        }
        public int TurnOnAlarm(int times)
        {
            try
            {
                // Get Light State
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=AudioAlarmPlay&token=" + _token);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\": \"AudioAlarmPlay\",\"action\": 0,\"param\": {\"alarm_mode\": \"times\",\"manual_switch\": 0,\"times\":" + times + ",\"channel\": 0}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "rspCode" : 200
                dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                //return (LoginResp[0].value.rspCode == 200);
            }
            catch (Exception e)
            {
                _ = e;
            }
            return 0;
        }

        public void CleanupMP4(IWebHostEnvironment hostEnv)
        {
            try
            {
                // Delete all temp camera images
                string[] filePaths = Directory.GetFiles(GetTempDir(hostEnv));
                foreach (string filePath in filePaths)
                {
                    var LName = new FileInfo(filePath).Name.ToLower();
                    if (LName.EndsWith(".mp4"))
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch
            {

            }
        }

        public void Cleanup(IWebHostEnvironment hostEnv, string exceptFile="")
        {
            try
            {
                GetPresetPicture(hostEnv, (int)COp.Preset4); // reset back to full view so we can return to surveilence

                // Delete all temp camera images
                string[] filePaths = Directory.GetFiles(GetTempDir(hostEnv));

                // Get the file name of the exception. Example : camera/images/cam220304173422.jpg
                var LFull = exceptFile.ToLower();
                var idx = LFull.IndexOf("/images/");
                var LExcept = (idx != -1) ? LFull[(idx + 8)..] : LFull;

                foreach (string filePath in filePaths)
                {
                    var LName = new FileInfo(filePath).Name.ToLower();
                    if (LName != LExcept)
                    {
                        File.Delete(filePath);
                    }
                }
            }
            catch
            {

            }
        }
    }
}
