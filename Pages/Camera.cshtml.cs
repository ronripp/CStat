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
        private static CamOps _CamOps = new CamOps();
        private const COp DefaultCOp = COp.Preset4;

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

        public void OnGet(int op)
        {
            try
            {
                csl.Log("CAMERA.OnGet HandleOp " + op.ToString());
                var delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                //if (delay == 500)
                //    delay = 0;
                //Thread.Sleep(delay); // Give time for Camera to move. Delay can be adjusted
                CameraLink = _CamOps.SnapShot(_hostEnv);
                csl.Log("OnGet CameraLink[" + CameraLink + "]");
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
                csl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var link = _CamOps.SnapShot(_hostEnv);
                var delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                csl.Log("AJAX.OnGetCamOp [OK~:delay=" + delay.ToString() + ";link=" + link + "]");
                return new JsonResult("OK~:delay=" + delay.ToString() + ";link=" + link);
            }
            catch (Exception e)
            {
                _ = e;
                csl.Log("AJAX.OnGetCamOp [ERROR~: CamOp Exception]");
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
                csl.Log("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var link = _CamOps.SnapShot(_hostEnv);
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
