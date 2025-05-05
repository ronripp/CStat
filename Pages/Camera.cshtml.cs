using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Areas.Identity.Data;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using static CStat.Common.PtzCamera;

namespace CStat.Pages
{
    public class CameraModel : PageModel
    {
        public CamOps _CamOps = null;
        public Cam.Camera _cam;
        public static string[] LinkColorMap = new string[]
        {
            "Blue",  // 0 12 midnight
            "Blue",  // 1
            "Blue",  // 2
            "Blue",  // 3
            "Blue",  // 4
            "DeepSkyBlue",  // 5
            "MediumOrchid",  // 6
            "Orange",  // 7
            "Gold",  // 8
            "Gold",  // 9
            "Yellow",  // 10
            "Yellow",  // 11
            "Yellow",  // 12
            "Yellow",  // 13
            "Yellow",  // 14
            "Yellow",  // 15 
            "Gold",  // 16 4
            "Gold",  // 17 5
            "Orange",  // 18 6
            "MediumOrchid",  // 19 7
            "DeepSkyBlue",  // 20 8
            "Blue",  // 21 9
            "Blue",  // 22
            "Blue",  // 23
        };

        private const COp DefaultCOp = COp.Preset4;
        private CSLogger cl = new CSLogger();

        [BindProperty]
        public string CameraLink { get; set; } = "";

        [BindProperty]
        public string _VideoLinks { get; set; } = "";

        private readonly IWebHostEnvironment _hostEnv;

        public CameraModel (IWebHostEnvironment hostEnv)
        {
            _hostEnv = hostEnv;
        }

        ~CameraModel()
        {
            
        }
        public void OnGet(Cam.Camera cam, int op)
        {
            try
            {
                if (cam == Cam.Camera.Camera2)
                {
                    _CamOps = Cam._CamOps2;
                    _cam = Cam.Camera.Camera2;
                }
                else
                {
                    _CamOps = Cam._CamOps1;
                    _cam = Cam.Camera.Camera1;
                }

                PtzCamera.ResetLogin(_cam);

                cl.Log("CAMERA.OnGet HandleOp " + op.ToString());
                if (op == (int)PtzCamera.COp.HRSnapShot)
                {
                    CameraLink = _CamOps.SnapShot(_hostEnv, true, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD camera resolution
                }
                else
                {
                    var delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                    CameraLink = _CamOps.SnapShot(_hostEnv, false);
                }
                cl.Log("OnGet CameraLink[" + CameraLink + "]");

                //var now = PropMgr.ESTNow;
                //_VideoLinks = _CamOps.GetVideoAnchors(_hostEnv, now.AddDays(-14), now);

                // Create Event Strips
                //TBD 

            }
            catch (Exception e)
            {
                _ = e;
               CameraLink = "";
            }
        }

        public static string GetPhoto(IWebHostEnvironment hostEnv, IConfiguration config, Microsoft.AspNetCore.Identity.UserManager<CStatUser> userManager, CSUser curUser, Cam.Camera cam, COp preset)
        {
            CamOps camOps;
            if (cam == Cam.Camera.Camera2)
                camOps = Cam._CamOps2;
            else
                camOps = Cam._CamOps1;

            if (preset != COp.None)
            {
                System.Threading.Thread.Sleep(camOps.HandleOp(hostEnv, preset));
            }
            string sshFile = camOps.SnapShotFile("", hostEnv, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution
            string EMailTitle = "CCA Camera Photo";
            var email = new CSEMail(config, userManager);
            string result = email.Send(curUser.EMail, curUser.EMail, EMailTitle, "Hi\nAttached, please find " + EMailTitle + ".\nThanks!\nCee Stat", new string[] { sshFile })
                ? "E-Mail sent with " + EMailTitle
                : "Failed to E-Mail : " + EMailTitle;
            return result; // TBD
        }

        public string PresetName(int cam, int index)
        {
            return (cam == 2) ? Cam._presets2[index] : Cam._presets1[index];
        }

        public JsonResult OnGetCamOp() // TBD Add Async
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            JObject jObj = JObject.Parse(jsonQS);

            if (!int.TryParse((string)jObj["cam"], out int cam))
                return new JsonResult("ERROR~:Incorrect Parameters");
            if ((Cam.Camera)cam == Cam.Camera.Camera2)
            {
                _CamOps = Cam._CamOps2;
                _cam = Cam.Camera.Camera2;
            }
            else
            {
                _CamOps = Cam._CamOps1;
                _cam = Cam.Camera.Camera1;
            }

            if (!int.TryParse((string)jObj["op"], out int op))
                return new JsonResult("ERROR~:Incorrect Parameters");

            try
            {
                //cl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
                string link;
                int delay;
                if (op == (int)PtzCamera.COp.HRSnapShot)
                {
                    delay = 0;
                    link = _CamOps.SnapShot(_hostEnv, true, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution
                }
                else
                {
                    delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                    link = _CamOps.SnapShot(_hostEnv, false);
                }
                //cl.Log("AJAX.OnGetCamOp [OK~:delay=" + delay.ToString() + ";link=" + link + "]");
                return new JsonResult("OK~:delay=" + delay.ToString() + ";link=" + link);
            }
            catch (Exception e)
            {
                _ = e;
                //cl.Log("AJAX.OnGetCamOp [ERROR~: CamOp Exception]");
                return new JsonResult("ERROR~: CamOp Exception");
            }
        }

        public JsonResult OnGetVideo()         {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            JObject jObj = JObject.Parse(jsonQS);
            string url = (string)jObj["url"];

            if (!int.TryParse((string)jObj["cam"], out int cam))
                return new JsonResult("ERROR~:Incorrect Parameters");
            if ((Cam.Camera)cam == Cam.Camera.Camera2)
            {
                _CamOps = Cam._CamOps2;
                _cam = Cam.Camera.Camera2;
            }
            else
            {
                _CamOps = Cam._CamOps1;
                _cam = Cam.Camera.Camera1;
            }

            try
            {
                var link = _CamOps.GetVideo(_hostEnv, url);
                if (string.IsNullOrEmpty(link))
                    return new JsonResult("ERROR~: Likely too large.");
                return new JsonResult("OK~:" + link);
            }
            catch (Exception e)
            {
                _ = e;
                return new JsonResult("ERROR~: CamOp Exception");
            }
        }
        public JsonResult OnGetSnapShot() 
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            JObject jObj = JObject.Parse(jsonQS);

            if (!int.TryParse((string)jObj["cam"], out int cam))
                return new JsonResult("ERROR~:Incorrect Parameters");
            if ((Cam.Camera)cam == Cam.Camera.Camera2)
            {
                _CamOps = Cam._CamOps2;
                _cam = Cam.Camera.Camera2;
            }
            else
            {
                _CamOps = Cam._CamOps1;
                _cam = Cam.Camera.Camera1;
            }

            if (!int.TryParse((string)jObj["op"], out int op))
                return new JsonResult("ERROR~:Incorrect Parameters");
            try
            {
                //cl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var link = _CamOps.SnapShot(_hostEnv, op==(int)PtzCamera.COp.HRSnapShot, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution);
                var pending = _CamOps.PendingOps();
                return new JsonResult("OK~:todo=" + pending + "&" + link);
            }
            catch (Exception e)
            {
                _ = e;
                return new JsonResult("ERROR~: CamOp Exception");
            }
        }
        public JsonResult OnGetCamCleanup()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            JObject jObj = JObject.Parse(jsonQS);

            if (!int.TryParse((string)jObj["cam"], out int cam))
                return new JsonResult("ERROR~:Incorrect Parameters");
            if ((Cam.Camera)cam == Cam.Camera.Camera2)
            {
                _CamOps = Cam._CamOps2;
                _cam = Cam.Camera.Camera2;
            }
            else
            {
                _CamOps = Cam._CamOps1;
                _cam = Cam.Camera.Camera1;
            }

            string exceptFile = (string)jObj["except"] ?? "";
            try
            {
                 _CamOps.Cleanup(_hostEnv, exceptFile);
                return new JsonResult("OK~:Cleanup");
            }
            catch (Exception e)
            {
                _ = e;
                return new JsonResult("ERROR~: OnGetCamCleanup Exception");
            }
        }
        public string CreatePicEventsDiv()
        {
            var groups = CSFileWatcher.GetFileInfoGroups(_hostEnv, _cam).OrderByDescending(fi => fi[0].LastAccessTimeUtc);
            string divStr;

            /**************************
                    <div id="imageEvents">
                        <div id="ce1" display:inline>
                            <div id="ce1t" onclick="onCamEvTitle(this.id)" style="display:inline">[Sun 4/6/25 8:05P] </div>
                            <div id="ce1s" onclick="onCamEvStrip(this.id)" style="display:none">
                                <img src="\quilts\quilt01.jpg" width="100" />
                                <img src="\quilts\quilt02.jpg" width="100" />
                                <img src="\quilts\quilt03.jpg" width="100" />
                            </div>
                        </div>
                    </div>
            ***************************/

            int ceIdx = 1;
            divStr = "<div id=\"imageEvents\">\n"; // IMGEVENTS

            //int G = 0;

            foreach (var grp in groups)
            {
                if (grp.Count > 0)
                {
                    var tfi = grp[0];

                    DateTime ImgDT = TimeZoneInfo.ConvertTimeFromUtc(tfi.LastWriteTimeUtc, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                    string titleStr = ImgDT.ToString("ddd M/d/yy h:mmt");

                    divStr += "<div id=\"ce" + ceIdx + "\" display:inline>"; // ce#

                    // Opened [ - ] with arrows : ce#h
                    divStr += "<div class=\"CEHeader\" id=\"ce" + ceIdx + "h\" onclick=\"onCamEvStrip(this.id)\" style=\"display: none;\">&nbsp;&nbsp;[ - ]&nbsp;&nbsp;[" + titleStr + "]"; // ce#h
                    //int g1 = 0;
                    foreach (var dfi in grp)
                    {
                        int idx = dfi.FullName.IndexOf("Camera");
                        if (idx != -1)
                            divStr += "&nbsp;&nbsp;&nbsp;&nbsp;<a href=\"" + dfi.FullName.Substring(idx) + "\" download>&#x21E9;</a>";

                        //if (++g1 >= 3) break;
                    }
                    divStr += "&nbsp;&nbsp;&nbsp;&nbsp;";
                    divStr += "</div>"; // ce#h

                    // Closed [ + ] ce#t
                    divStr += "<div id=\"ce" + ceIdx + "t\" onclick=\"onCamEvTitle(this.id)\" style=\"display: inline; color:" + LinkColorMap[ImgDT.Hour] + "\">&nbsp;&nbsp;[ + ]&nbsp;&nbsp;[" + titleStr + "]</div>";

                    // Opened Images Strip ce#s
                    divStr += "<div id=\"ce" + ceIdx + "s\" onclick=\"onCamEvStrip(this.id)\" style=\"display: none\">";
                    //int g2 = 0;
                    foreach (var fi in grp)
                    {
                        int idx = fi.FullName.IndexOf("Camera");
                        if (idx != -1)
                            divStr += "<img src=\"" + fi.FullName.Substring(idx) + "\" class=\"CEStrip\" width=\"300\"/>";

                        //if (++g2 >= 3) break;
                    }
                    divStr += "</div>";  // ce#s  Opened Images Strip

                    divStr += "</div>";  // ce#

                    ++ceIdx;
                }

                //if (++G >= 10) break;
            }
            divStr += "</div>";  // IMGEVENTS
            return divStr;
        }



    }
}
