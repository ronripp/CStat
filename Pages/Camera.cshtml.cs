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

        private string _Videos;

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
                Debug.WriteLine("CAMERA.OnGet HandleOp " + op.ToString());
                var delay = _CamOps.HandleOp(_hostEnv, (COp)op);

                // Give time for Camera to move. Delay can be adjusted
                Thread.Sleep(delay);
                CameraLink = _CamOps.SnapShot(_hostEnv);
                SearchCmd vres = _CamOps.GetVideos();
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
                Debug.WriteLine("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var delay = _CamOps.HandleOp(_hostEnv, (COp)op);
                return new JsonResult("OK~:" + delay.ToString());
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
                Debug.WriteLine("AJAX.OnGetCamOp HandleOp " + op.ToString());
                var link = _CamOps.SnapShot(_hostEnv);
                return new JsonResult("OK~:" + link);
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
