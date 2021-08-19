using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CStat
{
    //[Allow//Anonymous]
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
            ViewData["UPCError"] = "";
            ViewData["ItemErrors"] = "";

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

            if (!InventoryItem.Units.HasValue || (EditItem.Size <= 0))
            {
                ViewData["ItemErrors"] = "* Must have a non-zero Units/Item.";
                return Page();
            }

            if (InventoryItem.ReorderThreshold.HasValue && (InventoryItem.ReorderThreshold.Value <= 0))
                InventoryItem.ReorderThreshold = null;

            if (InventoryItem.UnitsPerDay.HasValue && (InventoryItem.UnitsPerDay.Value <= 0))
                InventoryItem.UnitsPerDay = null;

            if (ItemPhoto != null)
            {
                if ((EditItem.Upc == null) || (EditItem.Upc.Trim().Length == 0))
                {
                    ViewData["UPCError"] = "Error : UPC Must be specified with Photo.";
                    return Page();
                }
                string destFile = Path.Combine(hostEnv.WebRootPath, "items", "ItmImg_" + EditItem.Upc + ".jpg");
                
                if (System.IO.File.Exists(destFile))
                    System.IO.File.Delete(destFile); // Delete any existing photo for this item
                using (var fs = new FileStream(destFile, FileMode.Create))
                {
  
                    ItemPhoto.CopyTo(fs);
                }
            }

            if ((EditItem.Name != null) && (EditItem.Name.Length > 0))
                EditItem.Name.Trim();
            if ((EditItem.Upc != null) && (EditItem.Upc.Length > 0))
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
        public JsonResult OnGetDeleteItem()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);


            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
            if (NVPairs.TryGetValue("invItemId", out int invItmId))
            {
                InventoryItem = _context.InventoryItem.Find(invItmId);

                if (InventoryItem != null)
                {
                    try
                    {
                        // Delete Item
                        _context.InventoryItem.Remove(InventoryItem);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~: Exception : Delete Item Failed.");
                    }
                    return new JsonResult("SUCCESS~:Delete Item Succeeded.");
                }
            }
            return new JsonResult("ERROR~: Delete Item Failed.");
        }
        public ActionResult OnPostPingEInv()
        {
            return this.Content("Success:");  // Response 
        }
        public bool GetBuyAnchor(int buyIdx, out string buy1Anchor)
        {
            buy1Anchor = "<a href=\"https:" + "//www.amazon.com/gp/product/B015EXMB7M/ref=ppx_yo_dt_b_asin_image_o00_s00?ie=UTF8&psc=1" + "\"><b>#1 Buy from Amazon Prime</b></a>";
            return buyIdx == 1;
        }

    }
}
