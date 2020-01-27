using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Task = System.Threading.Tasks.Task;

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
    }
}
