using CStat.Common;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
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

        public Index1Model(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration config)
        {
            _context = context;
            _hostEnv = hostEnv;
            Settings = new CSSettings(config);
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
