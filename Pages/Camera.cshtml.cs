using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json.Linq;
using static CStat.Common.PtzCamera;

namespace CStat.Pages
{
    public class CameraModel : PageModel
    {
        private static CamOps1 _CamOps1 = new CamOps1();
        private static CamOps2 _CamOps2 = new CamOps2();
        private CamOps _CamOps = null;

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
                    _CamOps = _CamOps2;
                else
                    _CamOps = _CamOps1;

                PtzCamera.ResetLogin();

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

        public JsonResult OnGetCamOp() // TBD Add Async
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            JObject jObj = JObject.Parse(jsonQS);
            string opVal = (string)jObj["op"];
            if (!int.TryParse((string)jObj["op"], out int op))
                return new JsonResult("ERROR~:Incorrect Parameters");

            try
            {
                cl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
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
                cl.Log("AJAX.OnGetCamOp [OK~:delay=" + delay.ToString() + ";link=" + link + "]");
                return new JsonResult("OK~:delay=" + delay.ToString() + ";link=" + link);
            }
            catch (Exception e)
            {
                _ = e;
                cl.Log("AJAX.OnGetCamOp [ERROR~: CamOp Exception]");
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
            string opVal = (string)jObj["op"];
            if (!int.TryParse((string)jObj["op"], out int op))
                return new JsonResult("ERROR~:Incorrect Parameters");
            try
            {
                cl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var link = _CamOps.SnapShot(_hostEnv, op==(int)PtzCamera.COp.HRSnapShot);
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
