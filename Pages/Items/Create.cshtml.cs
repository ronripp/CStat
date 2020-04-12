using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace CStat
{
    public class ItemData
    {
        public double reorder;
        public string units;
        public string name;
    }
    public class CreateItemsModel : PageModel
    {
        public enum ItemUnits
        {
            unknown = 0,
            bags = 1,
            bladders = 2,
            bottles = 3,
            boxes = 4,
            bulbs = 5,
            drums = 6,
            jugs = 7,
            ounces = 8,
            pairs = 9,
            pieces = 10,
            reams = 11,
            rolls = 12,
            sheets = 13,
            tablets = 14
        };

        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment hostEnv;

        public CreateItemsModel(IWebHostEnvironment hstEnv, CStat.Models.CStatContext context)
        {
            _context = context;
            hostEnv = hstEnv;
        }

        public IActionResult OnGet()
        {
        ViewData["MfgId"] = new SelectList(_context.Business, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Item Item { get; set; }

        public static string[] ItemDataStrings =
        {
            "{'reorder':'1','units':'reams','name':'Printer/Copy Paper Reams'}",
            "{'reorder':'1','units':'pieces','name':'Brother Printer Laser Jet Toner'}",
            "{'reorder':'0.5','units':'boxes','name':'D8 Enzyme Pre-soak (before dishwasher)'}",
            "{'reorder':'0.5','units':'drums','name':'D21 (Red) Dishwasher Detergent (Restaurant Depot equivalent)'}",
            "{'reorder':'0.5','units':'drums','name':'D22 (Blue) Dishwasher Rinse Additive (Restaurant Depot equivalent)'}",
            "{'reorder':'0.5','units':'drums','name':'D23 (White) Low-Temp Sanitizer (Restaurant Depot equivalent)'}",
            "{'reorder':'0.5','units':'boxes','name':'Heavy Duty Aluminum Foil 18^^ x 1000^'}",
            "{'reorder':'0.5','units':'boxes','name':'Plastic Wrap 18^^ x 1000^'}",
            "{'reorder':'50','units':'sheets','name':'Parchment Paper for baking (24^^ x 16^^)'}",
            "{'reorder':'50','units':'pieces','name':'Hair Nets by Keystone'}",
            "{'reorder':'200','units':'pieces','name':'Toothpicks  (Round)'}",
            "{'reorder':'1','units':'boxes','name':'White Tip Wooden Matches '}",
            "{'reorder':'4','units':'pieces','name':'PIC Fly Ribbons'}",
            "{'reorder':'16','units':'ounces','name':'Scotch Brite Quick Clean Griddle Liquid : packets Rest. Depot or Lemon Juice'}",
            "{'reorder':'20','units':'pieces','name':'Scotch Brite Griddle Cleaning Screens 4^^x5.5^^ (No. 200)'}",
            "{'reorder':'20','units':'pieces','name':'Scotch-Brite Griddle Polishing Pad 46CC, 4 in x 5.25 in '}",
            "{'reorder':'1','units':'boxes','name':'P6 Pot and Pan Detergent'}",
            "{'reorder':'1','units':'boxes','name':'D7 Quat Sanitizer'}",
            "{'reorder':'2','units':'pairs','name':'Long Rubber Gloves size L/XL (Pots & Pans)'}",
            "{'reorder':'2','units':'pairs','name':'Long Rubber Gloves size M (Pots & Pans)'}",
            "{'reorder':'2','units':'bottles','name':'Windex Glass Cleaner'}",
            "{'reorder':'0.5','units':'jugs','name':'Pine Cleaner'}",
            "{'reorder':'0.5','units':'jugs','name':'Mess Hall Floor Cleaner'}",
            "{'reorder':'5','units':'pieces','name':'Coarse Nylon Pot Scrubbers'}",
            "{'reorder':'0.5','units':'rolls','name':'Masking Tape'}",
            "{'reorder':'30','units':'tablets','name':'DPD1 Free Chlorine Test Tablets '}",
            "{'reorder':'20','units':'bags','name':'Quart Non-Freezer Bags'}",
            "{'reorder':'20','units':'bags','name':'Gallon Non-Freezer Bags'}",
            "{'reorder':'20','units':'bags','name':'Quart Freezer Bags'}",
            "{'reorder':'20','units':'bags','name':'Gallon Freezer Bags'}",
            "{'reorder':'20','units':'pieces','name':'Coffee Filters '}",
            "{'reorder':'64','units':'bags','name':'Heavy Duty (2 mils) 55 Gal Garbage Bags (US Foods 726187)'}",
            "{'reorder':'64','units':'bags','name':'Heavy Duty (2 mils) 55 Gal Garbage Bags (US Foods 726187)'}",
            "{'reorder':'100','units':'bags','name':'Trash Pail Liners 7-10 gallons'}",
            "{'reorder':'6','units':'boxes','name':'Nitrile Gloves Food handling Large BJs 88867004547'}",
            "{'reorder':'200','units':'pieces','name':'Paper Plates'}",
            "{'reorder':'200','units':'pieces','name':'Plastic Spoons'}",
            "{'reorder':'200','units':'pieces','name':'Plastic Forks'}",
            "{'reorder':'200','units':'pieces','name':'Plastic Knives'}",
            "{'reorder':'2000','units':'pieces','name':'Cone Water Cups'}",
            "{'reorder':'12','units':'pieces','name':'D Cell Batteries'}",
            "{'reorder':'16','units':'pieces','name':'AA'}",
            "{'reorder':'16','units':'pieces','name':'AAA'}",
            "{'reorder':'4','units':'pieces','name':'9V Batteries'}",
            "{'reorder':'10','units':'pieces','name':'Candle Jars'}",
            "{'reorder':'2000','units':'pieces','name':'Square Family Napkins'}",
            "{'reorder':'10','units':'rolls','name':'Bounty Paper Towel Rolls'}",
            "{'reorder':'4','units':'bottles','name':'Hand Sanitizer (8 oz)'}",
            "{'reorder':'4','units':'bladders','name':'H1 Hand Soap'}",
            "{'reorder':'6','units':'rolls','name':'EnMotion Automatic Paper Towel Rolls 10^^ x 800^'}",
            "{'reorder':'100','units':'rolls','name':'Marcal 1-plyToilet Paper rolls'}",
            "{'reorder':'6','units':'rolls','name':'9^^ toilet paper rolls, 2 ply, 3 21/50^^ Atlas Paper Mills 800GREEN JJRTP'}",
            "{'reorder':'0.5','units':'jugs','name':'Liquid Laundry Soap 5-6 quart container'}",
            "{'reorder':'0.25','units':'boxes','name':'OxyClean 14 lb box'}",
            "{'reorder':'2','units':'jugs','name':'Cleaning Bleach (8.25% Sodium Hypochlorite) 121 fl oz (3.78 qt)'}",
            "{'reorder':'4','units':'jugs','name':'NSF/ANSI 60 Water System Bleach'}",
            "{'reorder':'6','units':'bulbs','name':'LED Light Bulbs 60 Watt equivalent'}",
            "{'reorder':'2','units':'bulbs','name':'Philips Exit Sign Light Bulbs 20W, T6 ½ Int. Base 046677416263'}",
            "{'reorder':'2','units':'bulbs','name':'15 Watt Equivalent (normal pear shape) Exit bulbs'}",
            "{'reorder':'2','units':'bulbs','name':'Par 38, 500 Lumens Exterior Flood Lights'}",
            "{'reorder':'2','units':'bulbs','name':'Large Area Halogen Bulbs (cabins, lodge, walkway)'}"
        };

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (Item.Name == "FIX_PPL.List")
            {
                string srcFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "PplSrc.txt");
                string destFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "PplList.txt");

                using (StreamReader sr = new StreamReader(srcFile))
                using (StreamWriter sw = new StreamWriter(destFile))
                {
                    string line;
                    while (sr.Peek() >= 0)
                    {
                        line = sr.ReadLine();
                        sw.WriteLine(line);
                        int sidx = line.IndexOf("[");
                        int eidx = line.IndexOf("]");
                        if ((sidx != -1) && (eidx != -1))
                        {
                            string[] pplList = line.Substring(sidx,eidx-sidx).Trim().Replace("[", "").Replace("]", "").Split(',');
                            bool isFirst = true;
                            int NewPID = -1;
                            foreach (var pid in pplList)
                            {
                                int id = int.Parse(pid.Trim());
                                Person psn = _context.Person.FirstOrDefault(p => p.Id == id);

                                if (psn != null)
                                {
                                    if (isFirst)
                                    {
                                        sw.WriteLine("KEY=" + id + " First=" + psn.FirstName + " Last=" + psn.LastName + " Cell=" + psn.CellPhone + " EMail=" + psn.Email);
                                        isFirst = false;
                                        NewPID = psn.Id;
                                    }
                                    else
                                    {
                                        sw.WriteLine(" id=" + id + " First=" + psn.FirstName + " Last=" + psn.LastName + " Cell=" + psn.CellPhone + " EMail=" + psn.Email);

                                        //**** DELETE ****
                                        _context.Person.Remove(psn);
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e)
                                        {
                                            sw.WriteLine("**** DB ERROR: ***** Delete person. id=[" + psn.Id + "]");
                                        }

                                        //**** UPDATE ****
                                        //var pg1arr = _context.Person.Where(p => p.Pg1PersonId == psn.Id).ToList<Person>();
                                        //foreach (var p in pg1arr)
                                        //{
                                        //    p.Pg1PersonId = NewPID;
                                        //    _context.Attach(p).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch (Exception e)
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing PG1 pid=[" + psn.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}

                                        //var pg2arr = _context.Person.Where(p => p.Pg2PersonId == psn.Id).ToList<Person>();
                                        //foreach (var p in pg2arr)
                                        //{
                                        //    p.Pg2PersonId = NewPID;
                                        //    _context.Attach(p).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch (Exception e)
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing PG2 pid=[" + psn.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}

                                        //var attarr = _context.Attendance.Where(a => a.PersonId == psn.Id).ToList<Attendance>();
                                        //foreach (var a in attarr)
                                        //{
                                        //    a.PersonId = NewPID;
                                        //    _context.Attach(a).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch (Exception e)
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing Attendance pid=[" + psn.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("**** Cant Find id [" + pid + "]");
                                }
                            }
                        }
                        sw.WriteLine("----------------------------------------------");
                    }
                }
            }

            if (Item.Name == "FIX_ADRS.List")
            {
                string srcFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "AdrSrc.txt");
                string destFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "AdrList.txt");

                using (StreamReader sr = new StreamReader(srcFile))
                using (StreamWriter sw = new StreamWriter(destFile))
                {
                    string line;
                    while (sr.Peek() >= 0)
                    {
                        line = sr.ReadLine();
                        sw.WriteLine(line);
                        int idx = line.IndexOf("[");
                        if (idx != -1)
                        {
                            string[] adrList = line.Substring(idx).Trim().Replace("[", "").Replace("]", "").Split(',');
                            bool isFirst = true;
                            int NewAdrID = -1;
                            foreach (var aid in adrList)
                            {
                                int id = int.Parse(aid.Trim());
                                Address adr = _context.Address.FirstOrDefault(a => a.Id == id);

                                if (adr != null)
                                {
                                    if (isFirst)
                                    {
                                        sw.WriteLine("KEY=" + id + " Street=" + adr.Street + " Town=" + adr.Town + " State=" + adr.State + " Zip=" + adr.ZipCode);
                                        isFirst = false;
                                        NewAdrID = adr.Id;
                                    }
                                    else
                                    {
                                        sw.WriteLine(" id=" + id + " Street=" + adr.Street + " Town=" + adr.Town + " State=" + adr.State + " Zip=" + adr.ZipCode);

                                        _context.Address.Remove(adr);
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception e)
                                        {
                                            sw.WriteLine("**** DB ERROR: ***** Changing person.aid. aid=[" + adr.Id + "]");
                                            return RedirectToPage("./Index");
                                        }

                                        //var parr = _context.Person.Where(p => p.AddressId == adr.Id).ToList<Person>();
                                        //foreach (var p in parr)
                                        //{
                                        //    p.AddressId = NewAdrID;
                                        //    _context.Attach(p).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch (Exception e)
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing person.aid. aid=[" + adr.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}

                                        //var carr = _context.Church.Where(c => c.AddressId == adr.Id).ToList<Church>();
                                        //foreach (var c in carr)
                                        //{
                                        //    c.AddressId = NewAdrID;
                                        //    _context.Attach(c).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing church.aid. aid=[" + adr.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("**** Cant Find id [" + aid + "]");
                                }
                            }
                        }
                        sw.WriteLine("----------------------------------------------");
                    }
                }
            }


            if (Item.Name == "ALL_ITEMS")
            {
                //Item itemA = new Item();
                //itemA.Name = "AAA";
                //itemA.Size = 1;
                //itemA.Units = 1;
                //itemA.Status = 1;
                //itemA.Upc = "111";
                //_context.Item.Add(itemA);
                //int resA = await _context.SaveChangesAsync();

                //InventoryItem invItemA = new InventoryItem();
                //invItemA.ItemId = itemA.Id;
                //invItemA.InventoryId = 1;
                //invItemA.ReorderThreshold = (float)10;
                //invItemA.Units = itemA.Units;
                //_context.InventoryItem.Add(invItemA);
                //int resA2 = await _context.SaveChangesAsync();

                //Item itemB = new Item();
                //itemB.Name = "BBB";
                //itemB.Size = 1;
                //itemB.Units = 2;
                //itemB.Status = 1;
                //itemB.Upc = "222";
                //_context.Item.Add(itemB);
                //int resB = await _context.SaveChangesAsync();

                //InventoryItem invItemB = new InventoryItem();
                //invItemB.ItemId = itemB.Id;
                //invItemB.InventoryId = 1;
                //invItemB.ReorderThreshold = (float)20;
                //invItemB.Units = itemB.Units;
                //_context.InventoryItem.Add(invItemB);
                //int resB2 = await _context.SaveChangesAsync();

                //Console.WriteLine(resA + "," + resA2 + "," + resB + "," + resB2);

                // Add all items and Inventory Items

                //Inventory inv = new Inventory();
                //inv.Id = 1;
                //inv.Name = "Non-Edible";
                //inv.Type = 1;
                //_context.Inventory.Add(inv);
                //await _context.SaveChangesAsync();

                foreach (var str in ItemDataStrings)
                {
                    ItemData idobj = JsonConvert.DeserializeObject<ItemData>(str);

                    Item item = new Item();
                    item.Name = idobj.name.Replace("^^", "\"").Replace("^", "'");
                    item.Size = 1;
                    ItemUnits units;
                    if (ItemUnits.TryParse(idobj.units, out units))
                        item.Units = (int)units;
                    else
                        item.Units = (int)ItemUnits.unknown;
                    item.Status = 1;
                    item.Upc = "";

                    _context.Item.Add(item);
                    int res1 = await _context.SaveChangesAsync();

                    InventoryItem invItem = new InventoryItem();
                    invItem.ItemId = item.Id;
                    invItem.InventoryId = 1;
                    invItem.ReorderThreshold = (float)idobj.reorder;
                    invItem.Units = item.Units;
                    _context.InventoryItem.Add(invItem);
                    int res2 = await _context.SaveChangesAsync();
                }
            }

            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}


            return RedirectToPage("./Index");
        }
    }
}
