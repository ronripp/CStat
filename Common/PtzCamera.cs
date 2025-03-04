using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;

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

    public class CamFile
    {
        public DateTime dt;
        public string path = "";
        public string desc = "";
        public CamFile (string fpath)
        {
            //                                     YYYYMMDDhhmmss
            //C:\cstat\wwwroot\Camera\Snap\camp_00_20221014033637.jpg
            path = fpath;
            var dot = fpath.LastIndexOf(".");
            if (dot == -1) return;
            int sec = int.Parse(fpath.Substring(dot - 2, 2));
            int min = int.Parse(fpath.Substring(dot - 4, 2));
            int hr = int.Parse(fpath.Substring(dot - 6, 2));
            int day = int.Parse(fpath.Substring(dot - 8, 2));
            int month = int.Parse(fpath.Substring(dot - 10, 2));
            int year = int.Parse(fpath.Substring(dot - 14, 4));

            dt = new DateTime(year, month, day, hr, min, sec, DateTimeKind.Local);
        }
        public string Url()
        {
            int sidx = path.ToLower().IndexOf("camera");
            if (sidx == -1) return "";
            return "https://ccaserve.org/" + path.Substring(sidx); 
        }
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
            Preset7 = 7,
            Preset8 = 8,
            Preset9 = 9,

            ToggleLight = 100,
            TurnOnAlarm = 101,
            HRSnapShot = 102,
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
            FocusInc = 401,
            LightOn = 500,
            LightOff = 501,
        };

        public class COpObj
        {
            public COp _cop;
            public string _path;
            public string _attrs;
            public COpObj(COp cop, string path = "", string attrs = "")
            {
                _cop = cop;
                _path = path;
                _attrs = attrs;
            }
        }

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

        public static string[] _token = {"", "", ""};
        public static DateTime[] _tokenTime = { new DateTime(2000, 1, 1), new DateTime(2000, 1, 1), new DateTime(2000, 1, 1) };
        private static readonly object _tokenLock1 = new object();
        private static readonly object _tokenLock2 = new object();
        public static CSLogger gLog = null;
        private Cam.Camera _cam;
        private int _cIdx;
        private string _host;
        private string _password;
        private object _tokenLock;

        private static string host1 = "https://ccacamp.hopto.org:1961/api.cgi";
        private static string host2 = "https://ccacamp.hopto.org:1962/api.cgi";

        private bool disposed = false;

        public PtzCamera (Cam.Camera cam)
        {
            _cam = cam;
            _cIdx = (int)_cam;
            if (cam == Cam.Camera.Camera2)
            {
                _host = host2;
                _password = "CCACert25";
                _tokenLock = _tokenLock2;
            }
            else
            {
                _host = host1;
                _password = "CCACert25";
                _tokenLock = _tokenLock1;
            }

            if (gLog == null)
                gLog = new CSLogger();
            Login();
        }

        //***************** start DISPOSABLE **********************

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
            Logout();
        }
        
        public bool Login()
        {
            lock (_tokenLock)
            {
                if (!String.IsNullOrEmpty(_token[_cIdx]))
                {
                    if ((PropMgr.ESTNow - _tokenTime[_cIdx]).TotalSeconds >= 0)
                        Logout();
                    else
                    {
                        gLog.Log("PtzC.Login START Returning existing token=" + _token[_cIdx]);
                        return true;
                    }
                }

                gLog.Log("PtzC.Login START *** GETTING TOKEN ***");

                try
                {
                    // Get Token
                    HttpReq2 req = new HttpReq2();
                    req.Open(HttpMethod.Post, _host + "?cmd=Login");

                    req.AddHeaderProp("Connection","keep-alive");
                    req.AddHeaderProp("Accept","*/*");
                    req.AddHeaderProp("Accept-Encoding","gzip, deflate, br");
                    req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                    req.AddBody("[{\"cmd\":\"Login\",\"param\":{\"User\":{\"userName\":\"admin\", \"password\":\"" + _password + "\" }}}]", "application/json");
                    var sResult = req.SendForString();

                    // "name" : "8da4c31df166a94"
                    if (String.IsNullOrEmpty(sResult))
                    {
                        gLog.Log("PtzC.Login EARLY RETURN : EMPTY sResult. " + req.RespCode);

                        return true;
                    }
                    dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                    _token[_cIdx] = LoginResp[0].value.Token.name;
                    _tokenTime[_cIdx] = PropMgr.ESTNow.AddSeconds((int)LoginResp[0].value.Token.leaseTime-60);
                    gLog.Log("PtzC.Login RETURN token=" + _token[_cIdx]);
                    return !string.IsNullOrEmpty(_token[_cIdx]);
                }
                catch (Exception e)
                {
                    _ = e;
                    gLog.Log("PtzC.Login EXCEPTION e.Msg=" + e.Message);
                    return false;
                }
            }
        }

        public bool Logout(bool force=false)
        {
            lock (_tokenLock)
            {
                gLog.Log("PtzC.Logout token=" + _token[_cIdx]);

                if (!force && (_tokenTime[_cIdx] - PropMgr.ESTNow).TotalSeconds > 0)
                {
                    gLog.Log("PtzC.Logout NOT Logging out. Keeping existing token=" + _token[_cIdx]);
                    return true;
                }

                if (string.IsNullOrEmpty(_token[_cIdx]))
                {
                    gLog.Log("PtzC.Logout EARLY RETURN : EMPTY TOKEN");
                    return false;
                }

                var tok = _token[_cIdx];
                _token[_cIdx] = "";
                _tokenTime[_cIdx] = new DateTime(2000, 1, 1);
                try
                {
                    // Get Token
                    HttpReq2 req = new HttpReq2();
                    req.Open(HttpMethod.Post, _host + "?cmd=Logout&token=" + tok);

                    req.AddHeaderProp("Connection", "keep-alive");
                    req.AddHeaderProp("Accept", "*/*");
                    req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                    req.AddBody("[{\"cmd\":\"Logout\",\"param\":{}}]", "application/json");
                    var sResult = req.SendForString();

                    // "name" : "8da4c31df166a94"
                    if (String.IsNullOrEmpty(sResult))
                    {
                        gLog.Log("PtzC.Logout EARLY RETURN : EMPTY sResult");
                        return true;
                    }
                    dynamic LogoutResp = JsonConvert.DeserializeObject(sResult);
                    gLog.Log("PtzC.Logout RETURN rspCode" + LogoutResp[0].value.rspCode);
                    return LogoutResp[0].value.rspCode == 200;
                }
                catch (Exception e)
                {
                    _ = e;
                    gLog.Log("PtzC.Logout EXCEPTION e.Msg=" + e.Message);
                }
                return false;
            }
        }

        public static void ResetLogin(Cam.Camera cam)
        {
            using (var ptz = new PtzCamera(cam))
            {
                ptz.Logout(true);
            }
        }

        public int GetPresetPicture(IWebHostEnvironment hostEnv, int preset)
        {
            gLog.Log("PtzC.GetPresetPicture START preset=" + preset);
            try
            {
                if ((preset >= 1) && (preset <= 99))
                {

                    // Get Picture by Preset #
                    HttpReq2 req = new HttpReq2();
                    req.Open(HttpMethod.Post, _host + "?cmd=PtzCtrl&token=" + _token[_cIdx]);

                    req.AddHeaderProp("Connection", "keep-alive");
                    req.AddHeaderProp("Accept", "*/*");
                    req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
                    req.AddBody("[{\"cmd\":\"PtzCtrl\",\"param\":{\"channel\":0,\"op\":\"ToPos\",\"id\":" + preset + ",\"speed\":32}}]", "application/json");
                    var sResult = req.SendForString();

                    // "rspCode" : 200
                    if (String.IsNullOrEmpty(sResult))
                        return CheckForSomeStability();
                    dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                    if (LoginResp[0].value.rspCode != 200)
                    {
                        gLog.Log("PtzC.GetPresetPicture ERR return rspCode=" + LoginResp[0].value.rspCode);
                        return 0;
                    }

                    return CheckForSomeStability();

                }
                gLog.Log("PtzC.GetPresetPicture DONE rspCode=");

                return 0;
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("PtzC.GetPresetPicture EXCEPTION e.Msg=" + e.Message);
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
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=PtzCtrl&token=" + _token[_cIdx]); // &user=admin&password=<this._password>");

                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
                string speedStr;
                speedStr = ((op == (int)PtzCamera.COp.FocusInc) || (op == (int)PtzCamera.COp.FocusDec)) ? speedStr = "1" : speedStr = "6";
                req.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"" + COpToStr[(COp)op] + "\",\"speed\":" + speedStr + "}}]", "application/json");
                var sResult = req.SendForString();
                if (String.IsNullOrEmpty(sResult))
                    return CheckForSomeStability();
                dynamic PostResp = JsonConvert.DeserializeObject(sResult);
                if ((PostResp[0].value != null) && (PostResp[0].value.rspCode != 200)) // "rspCode" : 200
                    return 0;

                // Perform Stop
                HttpReq2 req2 = new HttpReq2();
                req2.Open(HttpMethod.Post, _host + "?cmd=PtzCtrl&token=" + _token[_cIdx]); // &user=admin&password=<this._password>");

                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
                req2.AddBody("[{\"cmd\":\"PtzCtrl\",\"action\":0,\"param\":{\"channel\":0,\"op\":\"Stop\"}}]", "application/json");
                var sResult2 = req2.SendForString();
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
            HttpReq2 req3 = new HttpReq2();
            req3.Open(HttpMethod.Post, _host + "?cmd=GetPtzCheckState&token=" + _token[_cIdx]); // &user=admin&password=<this._password>");

            req3.AddHeaderProp("Connection", "keep-alive");
            req3.AddHeaderProp("Accept", "*/*");
            req3.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
            req3.AddBody("[{\"cmd\":\"GetPtzCheckState\",\"action\":0,\"param\":{\"channel\":0}}]", "application/json");

            for (int i = 0; i < retries; ++i)
            {
                var sResult3 = req3.SendForString();
                dynamic PostResp3 = JsonConvert.DeserializeObject(sResult3);
                // PtzCheckState 0:idle, 1:doing, 2:finish
                if (PostResp3[0].value != null)
                {
                    gLog.Log("GetPtzCheckState PtzCheckState=" + (int)(PostResp3[0].value.PtzCheckState));
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
            //RJR OLD            gLog.Log("*** PICTURE DONE ***");
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
            HttpReq2 req3 = new HttpReq2();
            req3.Open(HttpMethod.Post, _host + "?cmd=GetZoomFocus&token=" + _token[_cIdx]); // &user=admin&password=<this._password>);

            req3.AddHeaderProp("Connection", "keep-alive");
            req3.AddHeaderProp("Accept", "*/*");
            req3.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
            req3.AddBody("[{\"cmd\":\"GetZoomFocus\",\"action\":0,\"param\":{\"channel\":0}}]", "application/json");

            var sResult3 = req3.SendForString();
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
                    gLog.Log("GetZoomAndFocus FAIL Zoom=" + zoom + " Focus=" + focus);
                }
                gLog.Log("GetZoomAndFocus Zoom=" + zoom + " Focus=" + focus);
                return true;
            }
            gLog.Log("GetZoomAndFocus FAIL Zoom=" + zoom + " Focus=" + focus);
            return false;
        }

        public int ExecuteOp(IWebHostEnvironment hostEnv, COpObj copObj)
        {
            COp cop = copObj._cop;
            gLog.Log("PtzC.ExecuteOp START cop=" + (int)cop);

            if ((cop == COp.None) || (cop == COp.Refresh))
                return 0;
            if ((int)cop <= 99)
                return GetPresetPicture(hostEnv, (int)cop);

            if (cop == PtzCamera.COp.HRSnapShot)
            {
                GetSnapshotFile(copObj._path, hostEnv, "&width=3840&height=2160");
                return 0; // HRSnapShot just gets a High Resolution snapshot.
            }
            if (cop == PtzCamera.COp.ToggleLight)
            {
                return ToggleLight();
            }
            if (cop == PtzCamera.COp.LightOn)
            {
                return ToggleLight(1);
            }
            if (cop == PtzCamera.COp.LightOff)
            {
                return ToggleLight(0);
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
            string folderName = ((this._cam == Cam.Camera.Camera2) ? "Camera2 " : "Camera1 ") + @"\Snap";
            string webRootPath = hostEnv.WebRootPath;
            string TempPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            return TempPath;
        }

        public string GetSnapshot(IWebHostEnvironment hostEnv, bool asFile, string resStr ="&width=1024&height=768")
        {
            gLog.Log("PtzC.GetSnapShot");

            //RJR AVOID CLIENT CERT ERROR if (!asFile)
            //RJR AVOID CLIENT CERT ERROR {
            //RJR AVOID CLIENT CERT ERROR     return _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token + resStr; //"&width=1024&height=768";
            //RJR AVOID CLIENT CERT ERROR 
            //RJR AVOID CLIENT CERT ERROR     //return _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token + resStr;
            //RJR AVOID CLIENT CERT ERROR     //return _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token + "&width=640&height=480";
            //RJR AVOID CLIENT CERT ERROR     //return _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token + "&width=1024&height=768";
            //RJR AVOID CLIENT CERT ERROR     //return _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&user=admin&password=" + _password + "&width=640&height=480";
            //RJR AVOID CLIENT CERT ERROR }

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

                byte[] content;
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token[_cIdx] + resStr);
                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
                Stream stream = req.SendForStream();
          
                using (BinaryReader br = new BinaryReader(stream))
                {
                    content = br.ReadBytes(50000000);
                    br.Close();
                }
            
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
                GetZoomAndFocus(out int zoom, out int focus);
                gLog.Log("PtzC.GetSnapShot Camera/Images/" + actFilename);
            
                return "Camera/Images/" + actFilename;  // Send back URL
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("PtzC.GetSnapShot EXCEPTION e.Msg=" + e.Message);
                return "";
            }
        }

        public string GetSnapshotFile(string fullFile, IWebHostEnvironment hostEnv, string resStr = "&width=1024&height=768")
        {
            try
            {
                string fullPath;
                string baseFilename;
                string actFilename;
                string tempPath;
                if (!string.IsNullOrEmpty(fullFile))
                {
                    fullPath = fullFile;
                    int lastBS = fullFile.LastIndexOf('\\');
                    tempPath = (lastBS > 0) ? fullFile.Substring(0, lastBS) : "";
                    actFilename = (lastBS > 0) ? fullFile.Substring(lastBS+1) : fullFile;
                    int lastDot = actFilename.LastIndexOf('.');
                    baseFilename = (lastDot > 0) ? actFilename.Substring(0, lastDot) : actFilename;
                }
                else
                {
                    tempPath = GetTempDir(hostEnv);
                    var now = PropMgr.ESTNow;
                    baseFilename = "Cam" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
                    actFilename = baseFilename + ".jpg";
                    fullPath = Path.Combine(tempPath, actFilename);
                }

                for (int i = 1; File.Exists(fullPath); ++i)
                {
                    actFilename = baseFilename + i.ToString("00") + ".jpg";
                    fullPath = Path.Combine(tempPath, actFilename);
                }

                byte[] content;
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=Snap&channel=0&rs=flsYJfZgM6RTB_os&token=" + _token[_cIdx] + resStr);
                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");
                Stream stream = req.SendForStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    content = br.ReadBytes(50000000);
                    br.Close();
                }

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
                GetZoomAndFocus(out int zoom, out int focus);

                return fullPath;
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("PtzC.GetSnapShot EXCEPTION e.Msg=" + e.Message);
                return "";
            }
        }

        public string GetVideo(IWebHostEnvironment hostEnv, string url)
        {
            return "";
            //RJR return _host + "?cmd=Download&source=" + url + "&output=" + url + "&token=" + _token;

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
            //RJR OLD    //HttpReq2 req = new HttpReq2();
            //RJR OLD    //req.Open("Post", _host + "/api.cgi?cmd=PtzCtrl&token=" + _token);
            //RJR OLD
            //RJR OLD    //req.AddHeaderProp("Connection: keep-alive");
            //RJR OLD    //req.AddHeaderProp("Accept: */*");
            //RJR OLD    //req.AddHeaderProp("Accept-Encoding: gzip, deflate, br");
            //RJR OLD    //req.Send(out string sResult);
            //RJR OLD
            //RJR OLD    byte[] content;
            //RJR OLD    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_host + "?cmd=Download&source=" + url + "&output=" + url + "&token=" + _token);
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
            try
            {
                var tempPath = GetSnapDir(hostEnv);
                List<CamFile> rawfiles = new List<CamFile>();
                List<string> dirs = new List<string>(Directory.EnumerateDirectories(tempPath));
                foreach (var dir in dirs)
                {
                    List<string> sdirs = new List<string>(Directory.EnumerateDirectories(dir));
                    foreach (var sdir in sdirs)
                    {
                        List<string> ssdirs = new List<string>(Directory.EnumerateDirectories(sdir));
                        foreach (var ssdir in ssdirs)
                        {
                            List<string> sssfiles = new List<string>(Directory.EnumerateFiles(ssdir));
                            foreach (var sssfile in sssfiles)
                            {
                                rawfiles.Add(new CamFile(sssfile));
                            }
                        }
                    }
                }

                var files = rawfiles.OrderBy(f => f.dt).ToList();
                //https://ccaserve.org/camera/snap/2022/10/14/camp_00_20221014003057.mp4
                string anchors = "<table id=\"vtid\">\n";
                bool inTR = false;
                for (int i = 0; i < files.Count; ++i)
                {
                    var f = files[i];
                    if (i % 3 == 0)
                    {
                        if (inTR)
                        {
                            anchors += "</tr>";
                            if (i < (files.Count - 1))
                                anchors += "<tr>";
                            else
                                inTR = false;
                        }
                        else
                        {
                            anchors += "<tr>";
                            inTR = true;
                        }
                    }
                    int hr = (f.dt.Hour % 12);
                    string hrStr = (hr != 0) ? hr.ToString() : "12";
                    var title = (f.path.ToLower().Contains(".jpg") ? "P&#10139;" : "V&#10148;") + f.dt.Month + "/" + f.dt.Day + "@" + hrStr + ":" + (f.dt.Minute).ToString().PadLeft(2,'0') + ((f.dt.Hour >= 12) ? "P" : "A");
                    anchors += "<td><a href=\"" + f.Url() + "\">" + title + "</a></td>\n";
                }
                if (inTR)
                    anchors += "</tr>";
                anchors += "</table>\n";
                return anchors;
            }

            catch (UnauthorizedAccessException ex)
            {
                _ = ex;
                gLog.Log("PtzCamera Unautorized Access:" + ex.Message);
            }

            return "";
            //RJROLD string allAnchors = "";
            //RJROLD try
            //RJROLD {
            //RJROLD     DateTime sf = new DateTime(sdt.Year, sdt.Month, sdt.Day, 0, 0, 0);
            //RJROLD     DateTime ef = new DateTime(edt.Year, edt.Month, edt.Day, 23, 59, 59);
            //RJROLD     SearchCmd scmd = GetVideos(sf, ef);
            //RJROLD 
            //RJROLD     if ((scmd == null) || (scmd.value == null))
            //RJROLD         return "";
            //RJROLD 
            //RJROLD     if (sdt.Date == edt.Date)
            //RJROLD     {
            //RJROLD         if ((scmd.value.SearchResult == null) || (scmd.value.SearchResult.File == null) || (scmd.value.SearchResult.File.Length == 0))
            //RJROLD             return "";
            //RJROLD 
            //RJROLD         // Return anchors for this day
            //RJROLD         foreach (var vf in scmd.value.SearchResult.File)
            //RJROLD         {
            //RJROLD             string timeStr;
            //RJROLD             if (vf.StartTime.hour > 12)
            //RJROLD                 timeStr = (vf.StartTime.hour - 12) + ":" + vf.StartTime.min.ToString("00") + "P]";
            //RJROLD             else
            //RJROLD                 timeStr = vf.StartTime.hour + ":" + vf.StartTime.min.ToString("00") + "A]";
            //RJROLD 
            //RJROLD             //_host + ?cmd=Playback&source=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&output=Mp4Record/2022-03-06/RecM01_20220306_020024_020118_6732828_2821E4C.mp4&user=admin&password=<this._password>
            //RJROLD 
            //RJROLD             int RMegs = (int)Math.Round((double)vf.size/1000000);
            //RJROLD             string title = " [" + RMegs + "-" + vf.StartTime.mon + "/" + vf.StartTime.day + "@" + timeStr;
            //RJROLD             if (ancCount++ % 3 == 0)
            //RJROLD                 allAnchors += "</tr><tr>\n";
            //RJROLD 
            //RJROLD             // https://ccaserve.org/camera/snap/2022/10/14/camp_00_20221014003057.mp4
            //RJROLD             allAnchors += "<td><a href=\"" + GetVideo(hostEnv, vf.name) + "\">" + title + "</a></td>\n";
            //RJROLD 
            //RJROLD         }
            //RJROLD         return allAnchors;
            //RJROLD     }
            //RJROLD 
            //RJROLD     // Determine Days that have videos
            //RJROLD     if ((scmd.value.SearchResult == null) || (scmd.value.SearchResult.Status == null) || (scmd.value.SearchResult.Status.Count() <= 0))
            //RJROLD         return "";
            //RJROLD     int year = sdt.Year;
            //RJROLD     var lastMonth = sdt.Month;
            //RJROLD     var vDays = new List<DateTime>();
            //RJROLD     foreach (var rmon in scmd.value.SearchResult.Status)
            //RJROLD     {
            //RJROLD         if (rmon.mon < lastMonth)
            //RJROLD             year = edt.Year;
            //RJROLD 
            //RJROLD         char[] dayArr = rmon.table.ToCharArray();
            //RJROLD         for (int i = 0; i < dayArr.Length; ++i)
            //RJROLD         {
            //RJROLD             if (dayArr[i] == '1')
            //RJROLD             {
            //RJROLD                 var vDay = new DateTime(year, rmon.mon, i + 1);
            //RJROLD                 if (vDay.Date < sdt.Date)
            //RJROLD                     continue;
            //RJROLD                 if (vDay.Date > edt.Date)
            //RJROLD                     break;
            //RJROLD                 vDays.Add(vDay);
            //RJROLD             }
            //RJROLD         }
            //RJROLD         lastMonth = rmon.mon;
            //RJROLD     }
            //RJROLD 
            //RJROLD     allAnchors = "<table id=\"vtid\"><tr>\n";
            //RJROLD     ancCount = 0;
            //RJROLD 
            //RJROLD     // Get Links for each day from newer to older
            //RJROLD     var NumVD = vDays.Count;
            //RJROLD     for (int i = NumVD-1; i >= 0; --i)
            //RJROLD     {
            //RJROLD         allAnchors += GetVideoAnchors(hostEnv, vDays[i], vDays[i], ref ancCount);
            //RJROLD     }
            //RJROLD 
            //RJROLD     allAnchors += "</tr></table>\n";
            //RJROLD 
            //RJROLD     return allAnchors;
            //RJROLD }
            //RJROLD catch
            //RJROLD {
            //RJROLD 
            //RJROLD }
            //RJROLD 
            //RJROLD return "";
        }

        public SearchCmd GetVideos(DateTime sdt, DateTime edt) // TBD : Dates
        {
            try
            {
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=Search&rs=adshf4549f&user=admin&password=" + _password);

                req.AddHeaderProp("content-type", "application/json");
                req.AddHeaderProp("accept","application/json");
                req.AddHeaderProp("accept-encoding","en-US,en;q=0.8");
                req.AddHeaderProp("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.51 Safari/537.36");
//                req.AddBody("[{\"cmd\": \"Search\",\"action\": 0,\"param\": {\"Search\": {\"channel\": 0,\"onlyStatus\": 0,\"streamType\": \"main\",\"StartTime\": {\"year\": 2022,\"mon\": 3,\"day\": 6,\"hour\": 1,\"min\": 0,\"sec\": 0},\"EndTime\": {\"year\": 2022,\"mon\": 3,\"day\": 6,\"hour\": 15,\"min\": 0,\"sec\": 0}}}}]");
                req.AddBody("[{\"cmd\": \"Search\",\"action\": 0,\"param\": {\"Search\": {\"channel\": 0,\"onlyStatus\": 0,\"streamType\": \"main\",\"StartTime\": {\"year\": " + sdt.Year + ",\"mon\": " + sdt.Month + ",\"day\": " + sdt.Day + ",\"hour\": " + sdt.Hour + ",\"min\": " + sdt.Minute + ",\"sec\": " + sdt.Second + "},\"EndTime\": {\"year\": " + edt.Year + ",\"mon\": " + edt.Month + ",\"day\": " + edt.Day + ",\"hour\": " + edt.Hour + ",\"min\": " + edt.Minute + ",\"sec\": " + edt.Second + "}}}}]");
                var sResult = req.SendForString();
                SearchCmd [] searchCmds =  JsonConvert.DeserializeObject<SearchCmd[]>(sResult);
                return (searchCmds.Count() >= 1) ? searchCmds[0] : null;
            }
            catch (Exception e)
            {
                _ = e;
                return null;
            }
        }

        public int ToggleLight(int forceState=-1)
        {
            try
            {
                int newState;
                if (forceState == -1)
                {
                    // Get Light State
                    HttpReq2 req = new HttpReq2();
                    req.Open(HttpMethod.Post, _host + "?cmd=GetWhiteLed&token=" + _token[_cIdx]);

                    req.AddHeaderProp("Connection", "keep-alive");
                    req.AddHeaderProp("Accept", "*/*");
                    req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                    req.AddBody("[{\"cmd\": \"GetWhiteLed\",\"action\": 0,\"param\": {\"channel\": 0}}]", "application/json");
                    var sResult = req.SendForString();

                    // value.PowerLed.state : 0 | 1
                    if (String.IsNullOrEmpty(sResult))
                        return 0;
                    dynamic GWLResp = JsonConvert.DeserializeObject(sResult);
                    newState = (GWLResp[0].value.WhiteLed.state == 0) ? 1 : 0;
                }
                else
                    newState = forceState;

                HttpReq2 req2 = new HttpReq2();
                req2.Open(HttpMethod.Post, _host + "?cmd=SetWhiteLed&token=" + _token[_cIdx]);

                req2.AddHeaderProp("Connection", "keep-alive");
                req2.AddHeaderProp("Accept", "*/*");
                req2.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                req2.AddBody("[{\"cmd\": \"SetWhiteLed\",\"param\": {\"WhiteLed\": {\"state\":" + newState + ",\"channel\": 0,\"mode\": 1,\"bright\": 85,\"LightingSchedule\": {\"EndHour\": 6,\"EndMin\": 0,\"StartHour\": 18,\"StartMin\": 0},\"wlAiDetectType\": {\"dog_cat\": 1,\"face\": 0,\"people\": 1,\"vehicle\": 0}}}}]", "application/json");
                var sResult2 = req2.SendForString();

                return 500; // need time to turn on/off light. This may be reduced.
            }
            catch (Exception e)
            {
                _ = e;
            }
            return 0;
        }
        public int EnableEMailAlerts(bool enable)
        {
            var cl = new CSLogger();
            try
            {
                // Get Light State
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=SetEmailV20&token=" + _token[_cIdx]);

                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                if (enable)
                    req.AddBody("[{\"cmd\": \"SetEmailV20\", \"param\": {\"Email\": {\"ssl\": 1, \"smtpPort\": 587, \"userName\": \"ronripp@outlook.com\", \"password\": \"Red35823\", \"addr1\": \"ronripp3@gmail.com\", \"addr2\": \"gmbesthelp@gmail.com\"}}}]");
                else
                    req.AddBody("[{\"cmd\": \"SetEmailV20\", \"param\": {\"Email\": {\"ssl\": 1, \"smtpPort\": 588, \"userName\": \"ronripp@outlook.com\", \"password\": \"Red35823\", \"addr1\": \"ronripp@charter.net\", \"addr2\": \"ellenripp@gmail.com\"}}}]");
                    
                var sResult = req.SendForString();
                if (String.IsNullOrEmpty(sResult))
                {
                    cl.Log("Enable EMail Camera Alert : sResult Null / Empty");
                    return 0;
                }

                // "rspCode" : 200
                dynamic LoginResp = JsonConvert.DeserializeObject(sResult);
                
                cl.Log("Enable EMail Camera Alert enable=" + enable + " Response Code =" + LoginResp[0].value.rspCode);

            }
            catch (Exception e)
            {
                _ = e;
                cl.Log("Enable EMail Camera Alert Exception : " + e.Message);
            }
            return 0;
        }

        public int TurnOnAlarm(int times)
        {
            try
            {
                // Get Light State
                HttpReq2 req = new HttpReq2();
                req.Open(HttpMethod.Post, _host + "?cmd=AudioAlarmPlay&token=" + _token[_cIdx]);

                req.AddHeaderProp("Connection", "keep-alive");
                req.AddHeaderProp("Accept", "*/*");
                req.AddHeaderProp("Accept-Encoding", "gzip, deflate, br");

                req.AddBody("[{\"cmd\": \"AudioAlarmPlay\",\"action\": 0,\"param\": {\"alarm_mode\": \"times\",\"manual_switch\": 0,\"times\":" + times + ",\"channel\": 0}}]", "application/json");
                var sResult = req.SendForString();
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
                // Should go back to Monitor Point on its own : GetPresetPicture(hostEnv, (int)COp.Preset4); // reset back to full view so we can return to surveilence

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
