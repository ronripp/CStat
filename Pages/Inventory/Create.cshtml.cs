using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

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
        public Item CreateItem { get; set; }
        public IFormFile ItemPhoto { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnGet()
        {
            CreateItem = new CStat.Models.Item
            {
                Upc = "",
                Name = "",
                Units = (int)InventoryItem.ItemUnits.unknown
            };

            InventoryItem = new InventoryItem();
            InventoryItem.Units = (int)InventoryItem.ItemUnits.unknown;
            InventoryItem.Zone = (int)InventoryItem.ItemZone.unknown;

            ViewData["item"] = CreateItem;
            ViewData["InventoryItem"] = InventoryItem;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                CreateItem.Units = InventoryItem.Units.HasValue ? InventoryItem.Units.Value : (int)InventoryItem.ItemZone.unknown;
                _context.Item.Add(CreateItem);
                int res1 = await _context.SaveChangesAsync();
                InventoryItem.ItemId = CreateItem.Id;
                InventoryItem.InventoryId = 1;
                _context.InventoryItem.Add(InventoryItem);
                int res2 = await _context.SaveChangesAsync();
             }
            catch (Exception e)
            {
                String err = e.Message;
            }

            return RedirectToPage("./Index");
        }
    }
}
