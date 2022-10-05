using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

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

// TBD : !!!! IMPORTANT !!! INVESTIGATE RESPONSE TRANSFER AS OPTIMIZATION INSTEAD OF COPYING TO JPG FILE.

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

        public static string _token = "";
        public bool Login()
        {
            csl.Log("PtzC.Login START");

            if (!String.IsNullOrEmpty(_token))
                return true;

            try
            {
                // Get Token

                HttpReq req = new HttpReq();
                req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=Login");

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\":\"Login\",\"param\":{\"User\":{\"userName\":\"admin\", \"password\":\"cca2022\" }}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "name" : "8da4c31df166a94"
                if (String.IsNullOrEmpty(sResult))
                {
                    csl.Log("PtzC.Login EARLY RETURN : EMPTY sResult");

                    return true;
                }
                dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                _token = LoginResp[0].value.Token.name;
                csl.Log("PtzC.Login RETURN token=" + _token);
                return !string.IsNullOrEmpty(_token);
            }
            catch (Exception e)
            {
                csl.Log("PtzC.Login EXCEPTION e.Msg=" + e.Message);
                return false;
            }
        }

        public bool Logout(bool force=false)
        {
            csl.Log("PtzC.Logout token=" + _token);

            //if (!force && !String.IsNullOrEmpty(_token))
            if (!String.IsNullOrEmpty(_token))
                    return true;

            if (string.IsNullOrEmpty(_token))
            {
                csl.Log("PtzC.Logout EARLY RETURN : EMPTY TOKEN");
                return false;
            }

            var tok = _token;
            _token = "";
            try
            {
                // Get Token
                HttpReq req = new HttpReq();
                req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=Logout&token=" + tok);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\":\"Logout\",\"param\":{}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // "name" : "8da4c31df166a94"
                if (String.IsNullOrEmpty(sResult))
                {
                    csl.Log("PtzC.Logout EARLY RETURN : EMPTY sResult");
                    return true;
                }
                dynamic LogoutResp = JsonConvert.DeserializeObject(sResult);
                csl.Log("PtzC.Logout RETURN rspCode" + LogoutResp[0].value.rspCode);
                return LogoutResp[0].value.rspCode == 200;
            }
            catch (Exception e)
            {
                csl.Log("PtzC.Logout EXCEPTION e.Msg=" + e.Message);
            }
            return false;
        }

        public int GetPresetPicture(IWebHostEnvironment hostEnv, int preset)
        {
            csl.Log("PtzC.GetPresetPicture START preset=" + preset);
            try
            {
                if ((preset >= 1) && (preset <= 99))
                {

                    // Get Picture by Preset #
                    HttpReq req = new HttpReq();
                    req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);

                    req.AddHeaderProp("Connection: keep-alive");
                    req.AddHeaderProp("Accept: */*");
                    req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                    req.AddBody("[{\"cmd\":\"PtzCtrl\",\"param\":{\"channel\":0,\"op\":\"ToPos\",\"id\":" + preset + ",\"speed\":32}}]", "application/json; charset=utf-8");
                    var sRespStat = req.Send(out string sResult);

                    // "rspCode" : 200
                    if (String.IsNullOrEmpty(sResult))
                        return CheckForSomeStability();
                    dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                    if (LoginResp[0].value.rspCode != 200)
                    {
                        csl.Log("PtzC.GetPresetPicture ERR return rspCode=" + LoginResp[0].value.rspCode);
                        return 0;
                    }

                    return CheckForSomeStability();

                }
                csl.Log("PtzC.GetPresetPicture DONE rspCode=");

                return 0;
            }
            catch (Exception e)
            {
                _ = e;
                csl.Log("PtzC.GetPresetPicture EXCEPTION e.Msg=" + e.Message);
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
                req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token); // &user=admin&password=cca2022");

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                req.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"" + COpToStr[(COp)op] + "\",\"speed\":16}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);
                if (String.IsNullOrEmpty(sResult))
                    return CheckForSomeStability();
                dynamic PostResp = JsonConvert.DeserializeObject(sResult);
                if ((PostResp[0].value != null) && (PostResp[0].value.rspCode != 200)) // "rspCode" : 200
                    return 0;

                // Perform Stop
                HttpReq req2 = new HttpReq();
                req2.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token); // &user=admin&password=cca2022");

                req2.AddHeaderProp("Connection: keep-alive");
                req2.AddHeaderProp("Accept: */*");
                req2.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
                req2.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"Stop\"}}]", "application/json; charset=utf-8");
                var sRespStat2 = req2.Send(out string sResult2);
                dynamic PostResp2 = JsonConvert.DeserializeObject(sResult2);
                if ((PostResp2[0].value != null) && (PostResp2[0].value.rspCode != 200)) // "rspCode" : 200
                    return 0;

                var delay = CheckForSomeStability();
                if (delay == 500)
                    return 0; // no need for that extra snap with non-preset
                return delay;
            }
            catch (Exception e)
            {
                _ = e;
                return 0;
            }
        }
        public bool IsCameraDone(int retries = 1, int delay = 0)
        {
            // Get PTZ Check State
            HttpReq req3 = new HttpReq();
            req3.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=GetPtzCheckState&token=" + _token); // &user=admin&password=cca2022");

            req3.AddHeaderProp("Connection: keep-alive");
            req3.AddHeaderProp("Accept: */*");
            req3.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
            req3.AddBody("[{\"cmd\":\"GetPtzCheckState\",\"action\":0,\"param\":{\"channel\":0}}]", "application/json; charset=utf-8");

            for (int i = 0; i < retries; ++i)
            {
                var sRespStat3 = req3.Send(out string sResult3);
                dynamic PostResp3 = JsonConvert.DeserializeObject(sResult3);
                // PtzCheckState 0:idle, 1:doing, 2:finish
                if (PostResp3[0].value != null)
                {
                    //Debug.WriteLine("GetPtzCheckState PtzCheckState=" + (int)(PostResp3[0].value.PtzCheckState));
                    if (PostResp3[0].value.PtzCheckState != 1)
                        return true;
                }            
               //Thread.Sleep(delay);
            }
            return false;
        }
        public int CheckForSomeStability()
        {
            return 500;

            //RJR OLDint zoomC = -1;
            //RJR OLDint focusC = -1;
            //RJR OLDint CCount = 0;
            //RJR OLDint MaxCount = 20;
            //RJR OLDint MatchesNeeded = 8;
            //RJR OLDint MinMatches = 3;
            //RJR OLDint Needed = MatchesNeeded - MinMatches;

            //RJR OLDfor (int i=0; i<MaxCount; ++i)
            //RJR OLD{
            //RJR OLD    GetZoomAndFocus(out int zoom, out int focus);
            //RJR OLD    if ((zoom == zoomC) && (focus == focusC))
            //RJR OLD    {
            //RJR OLD        if (++CCount >= MatchesNeeded)
            //RJR OLD        {
            //RJR OLD            //Debug.WriteLine("*** PICTURE DONE ***");
            //RJR OLD            return 500;
            //RJR OLD        }
            //RJR OLD        if ((CCount == MinMatches) && (MaxCount< 30))
            //RJR OLD        {
            //RJR OLD            int diff = (MaxCount - i) - 1;
            //RJR OLD            if (diff<Needed)
            //RJR OLD                MaxCount += Needed - diff; // try a few more times to see if we can reach 10
            //RJR OLD        }
            //RJR OLD    }
            //RJR OLD    else
            //RJR OLD    {
            //RJR OLD        zoomC = zoom;
            //RJR OLD        focusC = focus;
            //RJR OLD        CCount = 0;
            //RJR OLD    }
            //RJR OLD    Thread.Sleep(20);
            //RJR OLD}
            //RJR OLDreturn IsCameraDone(20, 10) ? 500 : 1000; // still doing.. likely focusing and zooming
        }

        //http://IPC_IP/api.cgi?cmd=GetZoomFocus&token=TOKEN
        public bool GetZoomAndFocus(out int zoom, out int focus)
        {
            zoom = focus = -1;

            // Get PTZ Check State
            HttpReq req3 = new HttpReq();
            req3.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=GetZoomFocus&token=" + _token); // &user=admin&password=cca2022");

            req3.AddHeaderProp("Connection: keep-alive");
            req3.AddHeaderProp("Accept: */*");
            req3.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
            req3.AddBody("[{\"cmd\":\"GetZoomFocus\",\"action\":0,\"param\":{\"channel\":0}}]", "application/json; charset=utf-8");

            var sRespStat3 = req3.Send(out string sResult3);
            dynamic PostResp3 = JsonConvert.DeserializeObject(sResult3);
            if ((PostResp3[0].value != null) && (PostResp3[0].value.ZoomFocus != null))
            {
                try
                {
                    zoom = PostResp3[0].value.ZoomFocus.zoom.pos;
                    focus = PostResp3[0].value.ZoomFocus.focus.pos;
                }
                catch
                {
                    zoom = -1;
                    focus = -1;
                    //Debug.WriteLine("GetZoomAndFocus FAIL Zoom=" + zoom + " Focus=" + focus);
                }
                //Debug.WriteLine("GetZoomAndFocus Zoom=" + zoom + " Focus=" + focus);
                return true;
            }
            //Debug.WriteLine("GetZoomAndFocus FAIL Zoom=" + zoom + " Focus=" + focus);
            return false;
        }

        public int ExecuteOp(IWebHostEnvironment hostEnv, COp cop)
        {
            csl.Log("PtzC.ExecuteOp START cop=" + (int)cop);

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

        private string GetSnapDir(IWebHostEnvironment hostEnv)
        {
            string folderName = @"Camera\Snap";
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
            csl.Log("PtzC.GetSnapShot");
            return "https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token;

            //RJR OLDtry
            //RJR OLD{
            //RJR OLD    var tempPath = GetTempDir(hostEnv);

            //RJR OLD    var now = PropMgr.ESTNow;
            //RJR OLD    string baseFilename = "Cam" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
            //RJR OLD    string actFilename = baseFilename + ".jpg";
            //RJR OLD    string fullPath = Path.Combine(tempPath, actFilename);
            //RJR OLD    for (int i = 1; File.Exists(fullPath); ++i)
            //RJR OLD    {
            //RJR OLD        actFilename = baseFilename + i.ToString("00") + ".jpg";
            //RJR OLD        fullPath = Path.Combine(tempPath, actFilename);
            //RJR OLD    }

            //RJR OLD    //HttpReq req = new HttpReq();
            //RJR OLD    //req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);

            //RJR OLD    //req.AddHeaderProp("Connection: keep-alive");
            //RJR OLD    //req.AddHeaderProp("Accept: */*");
            //RJR OLD    //req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
            //RJR OLD    //req.Send(out string sResult);

            //RJR OLD    byte[] content;
            //RJR OLD    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token);
            //RJR OLD    WebResponse response = request.GetResponse();
            //RJR OLD    Stream stream = response.GetResponseStream();

            //RJR OLD    using (BinaryReader br = new BinaryReader(stream))
            //RJR OLD    {
            //RJR OLD        content = br.ReadBytes(50000000);
            //RJR OLD        br.Close();
            //RJR OLD    }
            //RJR OLD    response.Close();

            //RJR OLD    FileStream fs = new FileStream(fullPath, FileMode.Create);
            //RJR OLD    BinaryWriter bw = new BinaryWriter(fs);
            //RJR OLD    try
            //RJR OLD    {
            //RJR OLD        bw.Write(content);
            //RJR OLD    }
            //RJR OLD    finally
            //RJR OLD    {
            //RJR OLD        fs.Close();
            //RJR OLD        bw.Close();
            //RJR OLD    }
            //RJR OLD    GetZoomAndFocus(out int zoom, out int focus);
            //RJR OLD    csl.Log("PtzC.GetSnapShot Camera/Images/" + actFilename);

            //RJR OLD    return "Camera/Images/" + actFilename;  // Send back URL
            //RJR OLD}
            //RJR OLDcatch (Exception e)
            //RJR OLD{
            //RJR OLD    csl.Log("PtzC.GetSnapShot EXCEPTION e.Msg=" + e.Message);
            //RJR OLD    return "";
            //RJR OLD}
        }
        public string GetVideo(IWebHostEnvironment hostEnv, string url)
        {
            return "https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Download&source=" + url + "&output=" + url + "&token=" + _token;

            //RJR OLDtry
            //RJR OLD{
            //RJR OLD    CleanupMP4(hostEnv); // Delete any older MP4s as they take up a lot of room.
            //RJR OLD
            //RJR OLD    var tempPath = GetTempDir(hostEnv);
            //RJR OLD
            //RJR OLD    var now = PropMgr.ESTNow;
            //RJR OLD    string baseFilename = "Vid" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
            //RJR OLD    string actFilename = baseFilename + ".mp4";
            //RJR OLD    string fullPath = Path.Combine(tempPath, actFilename);
            //RJR OLD    for (int i = 1; File.Exists(fullPath); ++i)
            //RJR OLD    {
            //RJR OLD        actFilename = baseFilename + i.ToString("00") + ".mp4";
            //RJR OLD        fullPath = Path.Combine(tempPath, actFilename);
            //RJR OLD    }
            //RJR OLD
            //RJR OLD    //HttpReq req = new HttpReq();
            //RJR OLD    //req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=PtzCtrl&token=" + _token);
            //RJR OLD
            //RJR OLD    //req.AddHeaderProp("Connection: keep-alive");
            //RJR OLD    //req.AddHeaderProp("Accept: */*");
            //RJR OLD    //req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
            //RJR OLD    //req.Send(out string sResult);
            //RJR OLD
            //RJR OLD    byte[] content;
            //RJR OLD    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Download&source=" + url + "&output=" + url + "&token=" + _token);
            //RJR OLD    using (WebResponse response = request.GetResponse())
            //RJR OLD    {
            //RJR OLD        long bytesToRead = response.ContentLength + 5000;
            //RJR OLD        if (bytesToRead > (100000000 + 5000))
            //RJR OLD        {
            //RJR OLD            response.Close();
            //RJR OLD            return "";
            //RJR OLD        }
            //RJR OLD
            //RJR OLD        using (Stream stream = response.GetResponseStream())
            //RJR OLD        {
            //RJR OLD            using (BinaryReader br = new BinaryReader(stream))
            //RJR OLD            {
            //RJR OLD                content = br.ReadBytes((int)bytesToRead);
            //RJR OLD                br.Close();
            //RJR OLD            }
            //RJR OLD            response.Close();
            //RJR OLD
            //RJR OLD            FileStream fs = new FileStream(fullPath, FileMode.Create);
            //RJR OLD            BinaryWriter bw = new BinaryWriter(fs);
            //RJR OLD            try
            //RJR OLD            {
            //RJR OLD                bw.Write(content);
            //RJR OLD            }
            //RJR OLD            finally
            //RJR OLD            {
            //RJR OLD                fs.Close();
            //RJR OLD                bw.Close();
            //RJR OLD            }
            //RJR OLD            return "Camera/Images/" + actFilename;  // Send back URL
            //RJR OLD        }
            //RJR OLD    }
            //RJR OLD}
            //RJR OLDcatch (Exception e)
            //RJR OLD{
            //RJR OLD    _ = e;
            //RJR OLD    return "";
            //RJR OLD}
        }

        public string GetVideoAnchors(IWebHostEnvironment hostEnv, DateTime sdt, DateTime edt, ref int ancCount)
        {
            string allAnchors = "";
            try
            {
                DateTime sf = new DateTime(sdt.Year, sdt.Month, sdt.Day, 0, 0, 0);
                DateTime ef = new DateTime(edt.Year, edt.Month, edt.Day, 23, 59, 59);
                SearchCmd scmd = GetVideos(sf, ef);

                if (scmd.value == null)
                    return "";

                if (sdt.Date == edt.Date)
                {
                    if ((scmd.value.SearchResult == null) || (scmd.value.SearchResult.File == null) || (scmd.value.SearchResult.File.Length == 0))
                        return "";

                    // Return anchors for this day
                    foreach (var vf in scmd.value.SearchResult.File)
                    {
                        string timeStr;
                        if (vf.StartTime.hour > 12)
                            timeStr = (vf.StartTime.hour - 12) + ":" + vf.StartTime.min.ToString("00") + "P]";
                        else
                            timeStr = vf.StartTime.hour + ":" + vf.StartTime.min.ToString("00") + "A]";

                        //https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Playback&source=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&output=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&user=admin&password=cca2022

                        int RMegs = (int)Math.Round((double)vf.size/1000000);
                        string title = " [" + RMegs + "-" + vf.StartTime.mon + "/" + vf.StartTime.day + "@" + timeStr;
                        if (ancCount++ % 3 == 0)
                            allAnchors += "</tr><tr>\n";

                        // allAnchors += "<td><a href=\"#\" onclick=\"getVideo('" + vf.name + "','" + title + "')\">" + title + "</a></td>\n";
                        allAnchors += "<td><a href=\"" + GetVideo(hostEnv, vf.name) + "\">" + title + "</a></td>\n";

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
                    allAnchors += GetVideoAnchors(hostEnv, vDays[i], vDays[i], ref ancCount);
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
                req.Open("Post", "https://ccacamp.hopto.org:1961/cgi-bin/api.cgi?cmd=Search&rs=adshf4549f&user=admin&password=cca2022");

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
                _ = e;
                return null;
            }
        }

        public int ToggleLight()
        {
            try
            {
                // Get Light State
                HttpReq req = new HttpReq();
                req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=GetWhiteLed&token=" + _token);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\": \"GetWhiteLed\",\"action\": 0,\"param\": {\"channel\": 0}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);

                // value.PowerLed.state : 0 | 1
                if (String.IsNullOrEmpty(sResult))
                    return 0;
                dynamic GWLResp = JsonConvert.DeserializeObject(sResult);
                var newState = (GWLResp[0].value.WhiteLed.state == 0) ? 1 : 0;
                var SetJson = JsonConvert.SerializeObject(GWLResp);

                HttpReq req2 = new HttpReq();
                req2.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=SetWhiteLed&token=" + _token);

                req2.AddHeaderProp("Connection: keep-alive");
                req2.AddHeaderProp("Accept: */*");
                req2.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req2.AddBody("[{\"cmd\": \"SetWhiteLed\",\"param\": {\"WhiteLed\": {\"state\":" + newState + ",\"channel\": 0,\"mode\": 1,\"bright\": 85,\"LightingSchedule\": {\"EndHour\": 6,\"EndMin\": 0,\"StartHour\": 18,\"StartMin\": 0},\"wlAiDetectType\": {\"dog_cat\": 1,\"face\": 0,\"people\": 1,\"vehicle\": 0}}}}]", "application /json; charset=utf-8");
                sRespStat = req2.Send(out sResult);

                return 500; // need time to turn off light. This may be reduced.
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
                req.Open("Post", "https://ccacamp.hopto.org:1961/api.cgi?cmd=AudioAlarmPlay&token=" + _token);

                req.AddHeaderProp("Connection: keep-alive");
                req.AddHeaderProp("Accept: */*");
                req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");

                req.AddBody("[{\"cmd\": \"AudioAlarmPlay\",\"action\": 0,\"param\": {\"alarm_mode\": \"times\",\"manual_switch\": 0,\"times\":" + times + ",\"channel\": 0}}]", "application/json; charset=utf-8");
                var sRespStat = req.Send(out string sResult);
                if (String.IsNullOrEmpty(sResult))
                    return 0;

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

        //public int GetSnapPage(IWebHostEnvironment hostEnv)
        //{
        //    string[] filePaths = Directory.GetFiles(GetSnapDir(hostEnv));
        //    foreach (string filePath in filePaths)
        //    {
        //        var LName = new FileInfo(filePath).Name.ToLower();
        //        if (LName.EndsWith(".jpg"))
        //        {
        //            //Image image = Image.FromFile(fileName);
        //            //Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
        //            //thumb.Save(Path.ChangeExtension(fileName, "thumb"));
        //        }
        //    }


        //    //Image image = Image.FromFile(fileName);
        //    //Image thumb = image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);
        //    //thumb.Save(Path.ChangeExtension(fileName, "thumb"));

        //}

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

                // Delete FTP Capture folders 4 or more days old
                var baseSnap = GetSnapDir(hostEnv);
                int GoBackDays = 31;
                var delDT = PropMgr.ESTNow.AddDays(-GoBackDays);
                for (int i = 0; i < GoBackDays-4; ++i)
                {
                    try
                    {
                        var snapDir = Path.Combine(baseSnap, delDT.Year.ToString(), delDT.Month.ToString("D2"), delDT.Day.ToString("D2"));
                        if (Directory.Exists(snapDir))
                            Directory.Delete(snapDir, true);
                    }
                    catch { }
                    delDT = delDT.AddDays(1);
                }
             }
            catch
            {

            }
        }
    }
}
