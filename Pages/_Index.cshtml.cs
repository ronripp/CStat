using CStat.Areas.Identity.Data;
using CStat.Common;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace CStat.Pages
{
    public class Index1Model : PageModel
    {

        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly UserManager<CStatUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpCA;
        public CSSettings _cset;

        public Index1Model(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager, IHttpContextAccessor httpCA)
        {
            _context = context;
            _hostEnv = hostEnv;
            _userManager = userManager;
            _config = config;
            _httpCA = httpCA;
            _cset = CSSettings.GetCSSettings(config, userManager); // prime settings.
        }

        public void OnGet()
        {

        }

        public string GetDocsColor()
        {
            return CSSettings.green;
        }
        public string GetInvColor()
        {
            InventoryItem person = _context.InventoryItem.FirstOrDefault(i => i.State == 1);
            if (person != null)
                return CSSettings.red;
            person = _context.InventoryItem.FirstOrDefault(i => i.State == 2);
            if (person != null)
                return CSSettings.yellow;
            return CSSettings.green;
        }
        public string GetTasksColor()
        {
            CSUser curUser = CStat.Models.CCommon.GetCurUser(_context, _config, _httpCA, _userManager);
            int? pid = ((curUser != null) && curUser.pid.HasValue && (curUser.pid.Value != -1) && !curUser.ShowAllTasks) ? curUser.pid : null;
            var dueTasks = Task.GetDueTasks(_context, pid, 24);
            if (dueTasks.Count > 0)
                return dueTasks.Any(t => t.DueDate.HasValue && (t.DueDate.Value < PropMgr.ESTNow)) ? CSSettings.red : CSSettings.yellow;
            return CSSettings.green;
        }
        public string GetEquipColor()
        {
            ArdMgr ardMgr = new ArdMgr(_hostEnv, _config, _userManager);
            ArdRecord ar = ardMgr.GetLast();
            PropaneMgr pmgr = new PropaneMgr(_hostEnv, _config, _userManager);
            PropaneLevel pl = pmgr.GetTUTank();
            return CSSettings.GetColor(CSSettings.GetCSSettings().ActiveEquip, "All", ar, pl, false);
        }
    }
}
