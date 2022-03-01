using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static CStat.Common.PtzCamera;

namespace CStat.Pages
{
    public class CameraModel : PageModel
    {
        private static CamOps _CamOps = new CamOps();

        [BindProperty]
        public string CameraLink { get; set; } = "";

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
                CameraLink = _CamOps.HandleOp(_hostEnv, (COp)op);
            }
            catch (Exception e)
            {
                CameraLink = "";
            }
        }
    }
}
