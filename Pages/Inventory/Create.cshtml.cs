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
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace CStat
{
    //[Allow//Anonymous]
    public class CreateInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;

        public CreateInvModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            hostEnv = hstEnv;
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
            ViewData["UPCError"] = "";
            ViewData["ItemErrors"] = "";
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!InventoryItem.Units.HasValue || (CreateItem.Size <= 0))
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
                if ((CreateItem.Upc == null) || (CreateItem.Upc.Trim().Length == 0))
                {
                    ViewData["UPCError"] = "Error : UPC Must be specified with Photo.";
                    return Page();
                }
                string destFile = Path.Combine(hostEnv.WebRootPath, "items", "ItmImg_" + CreateItem.Upc + ".jpg");

                if (System.IO.File.Exists(destFile))
                    System.IO.File.Delete(destFile); // Delete any existing photo for this item
                using (var fs = new FileStream(destFile, FileMode.Create))
                {

                    ItemPhoto.CopyTo(fs);
                }
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
        public ActionResult OnPostPingCInv()
        {
            return this.Content("Success:");  // Response 
        }
    }
}
