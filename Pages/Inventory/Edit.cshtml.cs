using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CStat
{
    public class EditInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;

        public EditInvModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            hostEnv = hstEnv;
            _context = context;
        }

        [BindProperty]
        public InventoryItem InventoryItem { get; set; }
        [BindProperty]
        public Item EditItem { get; set; }
        public IFormFile ItemPhoto { get; set; }

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
            EditItem = await _context.Item.FirstOrDefaultAsync(i => i.Id == InventoryItem.ItemId);
            if (EditItem == null)
            {
                return NotFound();
            }

            if (EditItem.Upc == null)
                EditItem.Upc = "";

            ViewData["item"] = EditItem;
            ViewData["InventoryItem"] = InventoryItem;

            return Page();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if ((ItemPhoto != null) && (EditItem.Upc.Length > 0))
            {
                string destFile = Path.Combine(hostEnv.WebRootPath, "images", "ItmImg_" + EditItem.Upc + ".jpg");
                
                if (System.IO.File.Exists(destFile))
                    System.IO.File.Delete(destFile); // Delete any existing photo for this item
                using (var fs = new FileStream(destFile, FileMode.Create))
                {
  
                    ItemPhoto.CopyTo(fs);
                }
            }

            if (EditItem.Name.Length > 0)
                EditItem.Name.Trim();
            if (EditItem.Upc.Length > 0)
                EditItem.Upc.Trim();

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
        }

        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItem.Any(e => e.Id == id);
        }
    }
}
