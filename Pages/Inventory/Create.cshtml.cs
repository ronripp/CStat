using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;

namespace CStat
{
    public class CreateInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public CreateInvModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InventoryItem InventoryItem { get; set; }
        [BindProperty]
        public Item EditItem { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnGet()
        {
            EditItem = new CStat.Models.Item
            {
                Upc = "",
                Name = "",
                Units = (int)InventoryItem.ItemUnits.unknown
            };

            InventoryItem = new InventoryItem();
            InventoryItem.Units = (int)InventoryItem.ItemUnits.unknown;
            InventoryItem.Zone = (int)InventoryItem.ItemZone.unknown;

            ViewData["item"] = EditItem;
            ViewData["InventoryItem"] = InventoryItem;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.InventoryItem.Add(InventoryItem);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");

/**************************************************
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(InventoryItem).State = EntityState.Modified;
            _context.Attach(EditItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(InventoryItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
*************************************************/
        }
        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItem.Any(e => e.Id == id);
        }

    }
}
