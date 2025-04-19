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
using CStat.Common;
using Newtonsoft.Json;
using System.Globalization;

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
        public String Buy1URL { get; set; }
        [BindProperty]
        public String Buy2URL { get; set; }
        [BindProperty]
        public String Buy3URL { get; set; }
        [BindProperty]
        public Item CreateItem { get; set; }
        public IFormFile ItemPhoto { get; set; }
        public int _invType;

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnGet(int invType)
        {
            _invType = invType;
            CreateItem = new CStat.Models.Item
            {
                Upc = "",
                Name = "",
                Units = (int)InventoryItem.ItemUnits.unknown,
                Status = invType
            };

            InventoryItem = new InventoryItem();
            InventoryItem.Units = (int)InventoryItem.ItemUnits.unknown;
            InventoryItem.Zone = (int)InventoryItem.ItemZoneM.unknown;

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

            if ((CreateItem.Name != null) && (CreateItem.Name.Length > 0))
                CreateItem.Name.Trim();

            if ((CreateItem.Upc != null) && (CreateItem.Upc.Length > 0))
                CreateItem.Upc.Trim();

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

            GetBuyTransaction(1, Buy1URL);
            GetBuyTransaction(2, Buy2URL);
            GetBuyTransaction(3, Buy3URL);

            try
            {
                CreateItem.Units = InventoryItem.Units.HasValue ? InventoryItem.Units.Value : (int)InventoryItem.ItemUnits.unknown;
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
        private object GetEncoding(string v)
        {
            throw new NotImplementedException();
        }

        public int? GetBuyTransaction(int buyIdx, string buyUrl)
        {
            if ((buyUrl == null) || (buyUrl.Length == 0))
                return null;

            int? InvItBuyId = null;
            switch (buyIdx)
            {
                case 1:
                    InvItBuyId = InventoryItem.Buy1Id;
                    InventoryItem.Buy1Id = null; // initially clear
                    break;
                case 2:
                    InvItBuyId = InventoryItem.Buy2Id;
                    InventoryItem.Buy2Id = null; // initially clear
                    break;
                case 3:
                    InvItBuyId = InventoryItem.Buy3Id;
                    InventoryItem.Buy3Id = null; // initially clear
                    break;
                default:
                    return null;
            }

            if (buyUrl == "_.")
                return InvItBuyId;

            var Trans = _context.Transaction.Where(t => t.Link == buyUrl).FirstOrDefault();
            if (Trans == null)
            {
                Trans = new Transaction();
                Trans.Link = buyUrl;
                try
                {
                    var uri = new Uri(buyUrl);
                    var hostStrs = uri.Host.Split('.');

                    var NumHostStrs = hostStrs.Length;
                    if (NumHostStrs > 0)
                        Trans.Memo = hostStrs[Math.Max(0, NumHostStrs - 2)];
                    else
                        throw new InvalidOperationException("No Host Name");

                    _context.Transaction.Add(Trans);
                    _context.SaveChanges();
                }
                catch { Trans.Memo = "Store #" + buyIdx; }
            }
            switch (buyIdx)
            {
                case 1:
                    InventoryItem.Buy1Id = Trans.Id;
                    break;
                case 2:
                    InventoryItem.Buy2Id = Trans.Id;
                    break;
                case 3:
                    InventoryItem.Buy3Id = Trans.Id;
                    break;
                default:
                    return null;
            }
            return InvItBuyId;
        }


    }
}
