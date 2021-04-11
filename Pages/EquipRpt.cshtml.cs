using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using static CStat.Common.EquipProp;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;

namespace CStat.Pages
{
    public class EquipRptModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment _hostEnv;
        private readonly ArdMgr _ardMgr;
        PropaneMgr _pMgr;
        private readonly IConfiguration _config;
        public CSSettings Settings { get; set; }
        public List<PropaneLevel> PList { get; set; }

    public EquipRptModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            _hostEnv = hostEnv;
            _ardMgr = new ArdMgr(_hostEnv, _config, userManager);
            _config = config;
            Settings = CSSettings.GetCSSettings(_config, userManager);
            _pMgr = new PropaneMgr(_hostEnv, _config, userManager);
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnGet(string propName= "propaneTank")
        {
            if (propName == "propaneTank")
            {
                PList = _pMgr.GetRecentToLastList();
            }
            return Page();
        }

        public JsonResult OnGetPropaneCost()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            //InvItemState invIS = JsonConvert.DeserializeObject<InvItemState>(jsonQS);
            //return new JsonResult(JsonConvert.SerializeObject(cs));
            return new JsonResult("ERROR~:Incorrect Parameters");
        }


    }
}
