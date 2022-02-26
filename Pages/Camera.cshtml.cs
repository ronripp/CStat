using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CStat.Pages
{
    public class CameraModel : PageModel
    {
        [BindProperty]
        public string CameraLink { get; set; } = "";

        private readonly IWebHostEnvironment _hostEnv;
        private static bool InCamOp=false;

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
                if (InCamOp)
                    return;
                InCamOp = true;

                var ptzCam = new PtzCamera();
                ptzCam.Login();

                if ((PtzCamera.COp)op == PtzCamera.COp.ToggleLight)
                {
                    ptzCam.ToggleLight();
                    op = 0;
                }
                if ((PtzCamera.COp)op == PtzCamera.COp.TurnOnAlarm)
                {
                    ptzCam.TurnOnAlarm(10);
                    op = 0;
                }

                CameraLink = (op <= 99) ? ptzCam.GetPresetPicture(_hostEnv, op) : ptzCam.GetPtzPicture(_hostEnv, op);
                ptzCam.Logout();
            }
            catch (Exception e)
            {
                InCamOp = false;
            }
            finally
            {
                InCamOp = false;
            }
        }
    }
}
