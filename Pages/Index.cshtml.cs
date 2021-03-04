using CStat.Areas.Identity.Data;
using CStat.Common;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
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
        private readonly CSSettings Settings;

        public Index1Model(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            _hostEnv = hostEnv;
            Settings = new CSSettings(config, userManager);
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
            var dueTasks = Task.GetDueTasks(_context, 24);
            if (dueTasks.Count > 0)
            {
                return dueTasks.Any(t => t.DueDate.HasValue && (t.DueDate.Value < PropMgr.ESTNow)) ? CSSettings.red : CSSettings.yellow;
            }
            return CSSettings.green;
        }
        public string GetEquipColor()
        {
            ArdMgr ardMgr = new ArdMgr(_hostEnv);
            ArdRecord ar = ardMgr.GetLast();
            PropaneMgr pmgr = new PropaneMgr(_hostEnv);
            PropaneLevel pl = pmgr.GetTUTank();
            return Settings.GetColor("All", ar, pl, false);
        }
    }
}
