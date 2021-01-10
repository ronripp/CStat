using CStat.Common;
using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace CStat.Pages
{
    public class Index1Model : PageModel
    {
        const string green = "#00FF00";
        const string yellow = "#FFFF00";
        const string red = "#FF0000";
        const string gray = "#C0C0C0";

        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;

        public Index1Model(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;
        }

        public void OnGet()
        {

        }

        public string GetDocsColor()
        {
            return green;
        }
        public string GetInvColor()
        {
            InventoryItem person = _context.InventoryItem.FirstOrDefault(i => i.State == 1);
            if (person != null)
                return red;
            person = _context.InventoryItem.FirstOrDefault(i => i.State == 2);
            if (person != null)
                return yellow;
            return green;
        }
        public string GetTasksColor()
        {
            return green;
        }
        public string GetEquipColor()
        {
            ArdMgr ardMgr = new ArdMgr(_hostEnv);
            ArdRecord ar = ardMgr.GetLast();
            PropaneLevel prLevel = PropaneMgr.GetTUTank();
            return ar.GetColor("All", false, prLevel);
        }

    }
}
