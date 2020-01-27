using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat
{
    public class DetailsInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DetailsInvModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public InventoryItem InventoryItem { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            InventoryItem = await _context.InventoryItem
                .Include(i => i.Inventory)
                .Include(i => i.Item).FirstOrDefaultAsync(m => m.Id == id);

            if (InventoryItem == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
