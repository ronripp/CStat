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
        public CameraModel (IWebHostEnvironment hostEnv)
        {
            _hostEnv = hostEnv;
        }

        public void OnGet(int preset)
        {
            var ptzCam = new PtzCamera();
            ptzCam.Login();

            if (preset == 100)
            {
                ptzCam.ToggleLight();
                Thread.Sleep(6000); // need time to turn off light. This may be reduced.
                preset = 0;
            }
            CameraLink = ptzCam.GetPicture(_hostEnv, preset);
            ptzCam.Logout();
        }
    }
}
