using CStat.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace CStat.Pages
{
    public class Index1Model : PageModel
    {
        const string green = "#00FF00";
        const string yellow = "#FFFF00";
        const string red = "#FF0000";

        private readonly CStat.Models.CStatContext _context;

        public Index1Model(CStat.Models.CStatContext context)
        {
            _context = context;
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
            return green;
        }

    }
}
