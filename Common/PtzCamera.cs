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
    public class PtzCamera
    {
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
            try
            {
                // Get Token
                HttpReq req = new HttpReq();
                req.Open("Post", "http://ccacamp.hopto.org:1961/api.cgi?cmd=Logout&token=" + _token);

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
                return false;
            }
        }

        public string GetPicture(IWebHostEnvironment hostEnv, int preset)
        {
            try
            {
                if (preset > 0)
                {
                    // Get Picture
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
                        return "";

                    // Give time for Camera to move
                    Thread.Sleep(4000);
                }

                return GetSnapshot(hostEnv);
            }
            catch (Exception e)
            {
                Logout();
                return "";
            }
        }

        public string GetSnapshot(IWebHostEnvironment hostEnv)
        {
            try
            {

                string folderName = @"Camera\Images";
                string webRootPath = hostEnv.WebRootPath;
                string newPath = Path.Combine(webRootPath, folderName);
                if (!Directory.Exists(newPath))
                {
                    Directory.CreateDirectory(newPath);
                }

                var now = PropMgr.ESTNow;
                string baseFilename = "Cam" + (now.Year % 100).ToString("00") + now.Month.ToString("00") + now.Day.ToString("00") + now.Hour.ToString("00") + now.Minute.ToString("00") + now.Second.ToString("00");
                string actFilename = baseFilename + ".jpg";
                string fullPath = Path.Combine(newPath, actFilename);
                for (int i = 1; File.Exists(fullPath); ++i)
                {
                    actFilename = baseFilename + i.ToString("00") + ".jpg";
                    fullPath = Path.Combine(newPath, actFilename);
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
                Logout();
                return "";
            }
        }

        public void ToggleLight()
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

                // value.PowerLed.state : 0 | 1
                dynamic SWLResp2 = JsonConvert.DeserializeObject(sResult);
            }
            catch (Exception e)
            {
                _ = e;
            }
        }
    }
}
