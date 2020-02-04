using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;

namespace CStat
{
    public class IndexInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexInvModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<InventoryItem> InventoryItem { get;set; }

        public async Task OnGetAsync()
        {
            InventoryItem = await _context.InventoryItem
                .Include(i => i.Inventory)
                .Include(i => i.Item).ToListAsync();
        }

        public JsonResult OnGetItemStateChange()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);

            if (NVPairs.TryGetValue("invItmId", out int invItmId) && NVPairs.TryGetValue("pid", out int pid) && NVPairs.TryGetValue("atate", out int state))
            {
                return new JsonResult("OK"); // TBD
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }

    }
}
