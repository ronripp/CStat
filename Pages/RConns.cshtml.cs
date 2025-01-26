using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CStat.Areas.Identity.Data;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace CStat.Pages
{
    public class RConnsModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment _hostEnv;
        private readonly IConfiguration _config;
        public CSSettings _settings { get; set; }
        private RConnMgr _rconnMgr;
        public IList<ClientReport> ClientReps { get; set; } = null;
        private bool _rconnsValid = false;

        public RConnsModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            _hostEnv = hostEnv;
            _config = config;
            _settings = CSSettings.GetCSSettings(_config, userManager);
            _rconnMgr = new RConnMgr(_hostEnv, _context);
        }

        public void OnGet()
        {
            ClientReps = _rconnMgr.GetValidLast(out _rconnsValid);
        }

        public string Summary()
        {
            String lastDTStr = "";
            if (ClientReps.Count > 0)
            {
                DateTime maxDT = ClientReps.Max(lc => lc.curDT);
                if (System.Math.Abs((DateTime.Now - DateTime.UtcNow).TotalSeconds) < 2)
                {
                    maxDT = DateTime.SpecifyKind(maxDT, DateTimeKind.Utc);
                    TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    maxDT = TimeZoneInfo.ConvertTimeFromUtc(maxDT, easternZone);
                }
                lastDTStr = " Last Report: " + maxDT.ToShortDateString() + " " + maxDT.ToShortTimeString();
            }

            if (!_rconnsValid)
                return "People Detection is not operational. Please try later." + lastDTStr; 

            if (ClientReps.Count <= 0)
                return "NO people found @ CCA." + lastDTStr;

            return  ClientReps.Count + " people found @ CCA." + lastDTStr;
        }
    }
}
