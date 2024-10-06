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
        public static string[] _presets1 = new string[7] { "1", "Camper", "Work Zone", "Chapel", "Front View", "Deck/Doors", "End Door" };
        public static string[] _presets2 = new string[7] { "2", "Lower Path", "Propane", "Drive up", "Girls Path", "Park/Field", "Deck/Doors" };
        public static CamOps1 _CamOps1 = new CamOps1();
        public static CamOps2 _CamOps2 = new CamOps2();
        public CamOps _CamOps = null;
        public CamOps.Camera _cam;

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

        public void OnGet(CamOps.Camera cam, int op)
        {
            try
            {
                if (cam == CamOps.Camera.Camera2)
                {
                    _CamOps = _CamOps2;
                    _cam = CamOps.Camera.Camera2;
                }
                else
                {
                    _CamOps = _CamOps1;
                    _cam = CamOps.Camera.Camera1;
                }

                PtzCamera.ResetLogin(_cam);

                cl.Log("CAMERA.OnGet HandleOp " + op.ToString());
                if (op == (int)PtzCamera.COp.HRSnapShot)
                {
                    CameraLink = _CamOps.SnapShot(_hostEnv, true, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution
                }
                else
                {
                    var delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                    CameraLink = _CamOps.SnapShot(_hostEnv, false);
                }
                cl.Log("OnGet CameraLink[" + CameraLink + "]");
                var now = PropMgr.ESTNow;
                _VideoLinks = _CamOps.GetVideoAnchors(_hostEnv, now.AddDays(-14), now);
            }
            catch (Exception e)
            {
                _ = e;
               CameraLink = "";
            }
        }

        public static string GetPhoto(IWebHostEnvironment hostEnv, IConfiguration config, Microsoft.AspNetCore.Identity.UserManager<CStatUser> userManager, CSUser curUser, CamOps.Camera cam, COp preset)
        {
            CamOps camOps;
            if (cam == CamOps.Camera.Camera2)
                camOps = _CamOps2;
            else
                camOps = _CamOps2;

            if (preset != COp.None)
            {
                System.Threading.Thread.Sleep(camOps.HandleOp(hostEnv, preset));
            }
            string sshFile = camOps.SnapShotFile(hostEnv, true, "&width=3840&height=2160"); // 3840 X 2160 (8.0 MP) 4K ultra HD video resolution
            string EMailTitle = "CCA Camera Photo";
            var email = new CSEMail(config, userManager);
            string result = email.Send(curUser.EMail, curUser.EMail, EMailTitle, "Hi\nAttached, please find " + EMailTitle + ".\nThanks!\nCee Stat", new string[] { sshFile })
                ? "E-Mail sent with " + EMailTitle
                : "Failed to E-Mail : " + EMailTitle;
            return result; // TBD
        }

        public string PresetName(int cam, int index)
        {
            return (cam == 2) ? _presets2[index] : _presets1[index];
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
            if ((CamOps.Camera)cam == CamOps.Camera.Camera2)
            {
                _CamOps = _CamOps2;
                _cam = CamOps.Camera.Camera2;
            }
            else
            {
                _CamOps = _CamOps1;
                _cam = CamOps.Camera.Camera1;
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
            if ((CamOps.Camera)cam == CamOps.Camera.Camera2)
            {
                _CamOps = _CamOps2;
                _cam = CamOps.Camera.Camera2;
            }
            else
            {
                _CamOps = _CamOps1;
                _cam = CamOps.Camera.Camera1;
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
            if ((CamOps.Camera)cam == CamOps.Camera.Camera2)
            {
                _CamOps = _CamOps2;
                _cam = CamOps.Camera.Camera2;
            }
            else
            {
                _CamOps = _CamOps1;
                _cam = CamOps.Camera.Camera1;
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
            if ((CamOps.Camera)cam == CamOps.Camera.Camera2)
            {
                _CamOps = _CamOps2;
                _cam = CamOps.Camera.Camera2;
            }
            else
            {
                _CamOps = _CamOps1;
                _cam = CamOps.Camera.Camera1;
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

    }
}
