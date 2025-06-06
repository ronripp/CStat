﻿using CStat.Common;
using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Net.Sockets;
//using System.Text;
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
        [BindProperty]
        public String Buy1URL { get; set; }
        [BindProperty]
        public String Buy2URL { get; set; }
        [BindProperty]
        public String Buy3URL { get; set; }
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

            if (!InventoryItem.Expires.HasValue)
                InventoryItem.Expires = new DateTime();

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

            List<int?> tidsToDel = new List<int?>();
            tidsToDel.Add(GetBuyTransaction(1, Buy1URL));
            tidsToDel.Add(GetBuyTransaction(2, Buy2URL));
            tidsToDel.Add(GetBuyTransaction(3, Buy3URL));

            if (InventoryItem.Expires == DateTime.MinValue)
                InventoryItem.Expires = null;

                _context.Attach(InventoryItem).State = EntityState.Modified;
            _context.Attach(EditItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            // Try to delete unused transactions AFTER updating InventoryItem
            foreach (var tid in tidsToDel)
            {
                try
                {
                    if (tid.HasValue)
                    {
                        var OldTrans = _context.Transaction.Find(tid.Value);
                        if (OldTrans != null)
                        {
                            _context.Transaction.Remove(OldTrans);
                            _context.SaveChanges();
                        }
                    }
                }
                catch { }
            }

            return RedirectToPage("./Index", new { InvType = ((EditItem.Status == 1) ? "1" : "0")});
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
        public bool GetBuyAnchor(int buyIdx, out string buyAnchor)
        {
            Transaction Trans = null;
            buyAnchor = "<button style=\"margin-left:6px\" id=\"DelLink" + buyIdx + "\">x</button>";

            switch (buyIdx)
            {
                case 1:
                    if (InventoryItem.Buy1Id.HasValue)
                    {
                        Trans = _context.Transaction.Find(InventoryItem.Buy1Id);
                        if ((Trans == null) || (Trans.Link.Length < 10))
                            return false;
                    }
                    else
                        return false;
                    break;
                case 2:
                    if (InventoryItem.Buy2Id.HasValue)
                    {
                        Trans = _context.Transaction.Find(InventoryItem.Buy2Id);
                        if ((Trans == null) || (Trans.Link.Length < 10))
                            return false;
                    }
                    else
                        return false;
                    break;
                case 3:
                    if (InventoryItem.Buy3Id.HasValue)
                    {
                        Trans = _context.Transaction.Find(InventoryItem.Buy3Id);
                        if ((Trans == null) || (Trans.Link.Length < 10))
                            return false;
                    }
                    else
                        return false;
                    break;
                default:
                    return false;
            }

            buyAnchor = "<a href=\"" + Trans.Link + "\" id=\"Anc" + buyIdx + "\" BuyId=\"" + Trans.Id + "\">" +
                ((Trans.Memo.Length > 1) ? "Buy from " + ItemSale.ResolveVendor(Trans.Memo) : "Store #" + buyIdx) + "</a><button style=\"margin-left:6px\" id=\"DelLink" + buyIdx + "\">x</button>";
            return true;
        }

        public JsonResult OnGetUrlPrice()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> setParam = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            if (!setParam.TryGetValue("id", out string idStr) || !setParam.TryGetValue("link", out string url) || !setParam.TryGetValue("host", out string host))
                return new JsonResult("");

            var lhost = CCommon.UnencodeQuotes(host).ToLower().Trim();

            var pageContents = ItemSale.GetPageContent(url).Result;

            if (pageContents.Length < 50)
                return new JsonResult("");

            double price = 0;

            //************************
            // AMAZON
            //************************
            if (lhost.Contains(" amazon") || (lhost == "amazon"))
            {
                var priceIdx = pageContents.IndexOf("id=\"attach-base-product-price\" value=\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 38, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
                else
                {
                    // "priceAmount":10.94,
                    priceIdx = pageContents.IndexOf("\"priceAmount\":");
                    if (priceIdx != -1)
                    {
                        var priceStr = pageContents.Substring(priceIdx + 14, 15);
                        var endIdx = priceStr.IndexOf(",");
                        if (endIdx != -1)
                        {
                            priceStr = priceStr.Substring(0, endIdx);
                            if (!double.TryParse(priceStr, out price))
                                price = 0;
                        }
                    }
                    else
                    {
                        priceIdx = pageContents.IndexOf("id=\"price_inside_buybox\"");
                        if (priceIdx != -1)
                        {
                            //<span id="price_inside_buybox" class="a-size-medium a-color-price">
                            //$64.31
                            //</span>
                            var priceStr = pageContents.Substring(priceIdx + 24, 100);
                            var dollIdx = priceStr.IndexOf("$");
                            if (dollIdx != -1)
                            {
                                var dollStr = priceStr.Substring(dollIdx+1, 50);
                                var endDel = dollStr.IndexOf("<");
                                if (endDel != -1)
                                {
                                    priceStr = dollStr.Substring(0, endDel).Trim();
                                    if (!double.TryParse(priceStr, out price))
                                        price = 0;
                                }
                            }
                        }
                    }
                }
            }
            //*********************
            // WALMART
            //*********************
            else if (ItemSale.IsVendor(lhost, "walmart"))
            {
                var priceIdx = pageContents.IndexOf("<span class=\"price-characteristic\" itemprop=\"price\" content=\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 61, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
                else
                {
                    priceIdx = pageContents.IndexOf("\"price\":");
                    if (priceIdx != -1)
                    {
                        var priceStr = pageContents.Substring(priceIdx + 8, 15);
                        var endIdx = priceStr.IndexOf(",");
                        if (endIdx != -1)
                        {
                            priceStr = priceStr.Substring(0, endIdx);
                            if (!double.TryParse(priceStr, out price))
                                price = 0;
                        }
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "bj's"))
            {
                var priceIdx = pageContents.IndexOf(";offerPrice&q;:");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 15, 15);
                    var endIdx = priceStr.IndexOf(",");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "home depot"))
            {
                var priceIdx = pageContents.IndexOf("\"priceCurrency\":\"USD\",\"price\":");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 30, 15);
                    var endIdx = priceStr.IndexOf(",");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "webstaurantstore"))
            {
                var priceIdx = pageContents.IndexOf("<meta property=\"og:price:amount\" content=\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 42, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else  if (ItemSale.IsVendor(lhost, "lowes"))
            {
                var priceIdx = pageContents.IndexOf(",\"price\":");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 9, 15);
                    var endIdx = priceStr.IndexOf(",");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "filtersfast.com"))
            {
                var priceIdx = pageContents.IndexOf("value:");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 6, 15);
                    var endIdx = priceStr.IndexOf(",");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            // Costco
            else if (ItemSale.IsVendor(lhost, "costco"))
            {
                var priceIdx = pageContents.IndexOf("\"product:price:amount\" content=\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 32, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            // USRS
            else if (ItemSale.IsVendor(lhost, "USRS"))
            {
                var priceIdx = pageContents.IndexOf("\"Offer\",\"price\":\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 17, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "Salamone Supplies"))
            {
                var priceIdx = pageContents.IndexOf("\"price\":\"");
                if (priceIdx != -1)
                {
                    var priceStr = pageContents.Substring(priceIdx + 9, 15);
                    var endIdx = priceStr.IndexOf("\"");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }
            else if (ItemSale.IsVendor(lhost, "target"))
            {
                string apiKey;
                var apiKeyIdx = pageContents.IndexOf("\"apiKey\":\"");
                apiKey = pageContents.Substring(apiKeyIdx + 10, 50);
                if (apiKeyIdx != -1)
                {
                    var endIdx = apiKey.IndexOf("\"");
                    apiKey = apiKey.Substring(0, endIdx);
                }

                string[] urlParts = url.Split("/");
                string tcinKey = urlParts[(urlParts.Length < 7) ? (urlParts.Length - 1) : 6].Substring(2);
                var attrIdxP = tcinKey.IndexOf("#");
                if (attrIdxP == -1) attrIdxP = 1000;
                var attrIdxQ = tcinKey.IndexOf("?");
                if (attrIdxQ == -1) attrIdxQ = 1000;
                var attrIdx = Math.Min(attrIdxP, attrIdxQ);
                if ((attrIdx != -1) && (attrIdx <= 15))
                    tcinKey = tcinKey.Substring(0, attrIdx);
                else
                    tcinKey = tcinKey.Substring(0, 8);

                //"https://redsky.target.com/redsky_aggregations/v1/web/pdp_client_v1?key=ff457966e64d5e877fdbad070f276d18ecec4a01&tcin=13276131&store_id=1528&pricing_store_id=1528&has_financing_options=true&visitor_id=017BA97BC6FA02019E64C97AB60EA049&has_size_context=true&latitude=41.540&longitude=-73.430&state=CT&zip=06776";
                string bUrl = "https://redsky.target.com/redsky_aggregations/v1/web/pdp_client_v1?key=" + apiKey + "&tcin=" + tcinKey + "&store_id=1528&pricing_store_id=1528&has_financing_options=true&visitor_id=017BA97BC6FA02019E64C97AB60EA049&has_size_context=true&latitude=41.540&longitude=-73.430&state=CT&zip=06776";

                var subPageStr = ItemSale.GetPageContent(bUrl).Result;
                var priceIdx = subPageStr.IndexOf("\"current_retail\":");
                if (priceIdx != -1)
                {
                    var priceStr = subPageStr.Substring(priceIdx + 17, 15);
                    var endIdx = priceStr.IndexOf(",");
                    if (endIdx != -1)
                    {
                        priceStr = priceStr.Substring(0, endIdx);
                        if (!double.TryParse(priceStr, out price))
                            price = 0;
                    }
                }
            }

            return new JsonResult((price != 0) ? (idStr + "; $" + price.ToString("#.00", CultureInfo.InvariantCulture)) : pageContents.Length.ToString());
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

