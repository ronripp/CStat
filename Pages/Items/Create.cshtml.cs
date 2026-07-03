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

        public static string[] FoodDataStrings =
        {
            //*** Baking & Spices *** = Bake Spc
            "{'upc':'610000000000','reorder':'0','units':'oz','name':'Almond extract'}",
            "{'upc':'610000000001','reorder':'0','units':'oz','name':'Baking powder'}",
            "{'upc':'610000000002','reorder':'0','units':'oz','name':'Baking soda'}",
            "{'upc':'610000000003','reorder':'0','units':'oz','name':'Basil'}",
            "{'upc':'610000000004'',reorder':'0','units':'oz','name':'Black Pepper'}",
            "{'upc':'610000000005','reorder':'0','units':'oz','name':'Brown Sugar'}",
            "{'upc':'610000000006','reorder':'0','units':'oz','name':'Cayenne Pepper'}",
            "{'upc':'610000000007','reorder':'0','units':'oz','name':'Chili Powder'}",
            "{'upc':'610000000008','reorder':'0','units':'oz','name':'Chocolate Chips'}",
            "{'upc':'610000000009','reorder':'0','units':'oz','name':'Cinnamon'}",
            "{'upc':'610000000010','reorder':'0','units':'oz','name':'Confections sugar (10X)'}",
            "{'upc':'610000000011','reorder':'0','units':'oz','name':'Cooking Spray (PAM)'}",
            "{'upc':'610000000012','reorder':'0','units':'oz','name':'Corn Starch'}",
            "{'upc':'610000000013','reorder':'0','units':'oz','name':'Everything bagel'}",
            "{'upc':'610000000014','reorder':'0','units':'oz','name':'Flour'}",
            "{'upc':'610000000015','reorder':'0','units':'oz','name':'Garlic Powder'}",
            "{'upc':'610000000016','reorder':'0','units':'oz','name':'Honey'}",
            "{'upc':'610000000017','reorder':'0','units':'oz','name':'Lemon Juice'}",
            "{'upc':'610000000018','reorder':'0','units':'oz','name':'Olive oil'}",
            "{'upc':'610000000019','reorder':'0','units':'oz','name':'Onion Powder'}",
            "{'upc':'610000000020','reorder':'0','units':'oz','name':'Oregano'}",
            "{'upc':'610000000021','reorder':'0','units':'oz','name':'Paprika'}",
            "{'upc':'610000000022','reorder':'0','units':'oz','name':'Parsley Flakes'}",
            "{'upc':'610000000023','reorder':'0','units':'oz','name':'Pumpkin spice'}",
            "{'upc':'610000000024','reorder':'0','units':'oz','name':'Raisins'}",
            "{'upc':'610000000025','reorder':'0','units':'oz','name':'Salt'}",
            "{'upc':'610000000026','reorder':'0','units':'oz','name':'Sugar'}",
            "{'upc':'610000000027','reorder':'0','units':'oz','name':'Vanilla extract'}",
            "{'upc':'610000000028','reorder':'0','units':'oz','name':'Veg oil'}",
            "{'upc':'610000000029','reorder':'0','units':'oz','name':'White Vinegar'}",
            					 
            //*** DAIRY *** = Dairy
            "{'upc':'610000000030','reorder':'0','units':'oz','name':'American Cheese'}",
            "{'upc':'610000000031','reorder':'0','units':'oz','name':'Butter'}",
            "{'upc':'610000000032','reorder':'0','units':'oz','name':'Coffee Creamer'}",
            "{'upc':'610000000033','reorder':'0','units':'oz','name':'Cream Cheese'}",
            "{'upc':'610000000034','reorder':'0','units':'oz','name':'Eggs'}",
            "{'upc':'610000000035','reorder':'0','units':'oz','name':'Grated Parmesan Cheese'}",
            "{'upc':'610000000036','reorder':'0','units':'oz','name':'Half n Half'}",
            "{'upc':'610000000037','reorder':'0','units':'oz','name':'Ice Cream (various)'}",
            "{'upc':'610000000038','reorder':'0','units':'oz','name':'Milk'}",
            "{'upc':'610000000039','reorder':'0','units':'oz','name':'Mozzarella Cheese'}",
            "{'upc':'610000000040','reorder':'0','units':'oz','name':'Sour Cream'}",
            "{'upc':'610000000041','reorder':'0','units':'oz','name':'Whipped Cream'}",
            					 
            //**** Starch/Bread ****** = Starch B
            "{'upc':'610000000042','reorder':'0','units':'oz','name':''}",
            "{'upc':'610000000043','reorder':'0','units':'oz','name':'Bagels (or frozen)'}",
            "{'upc':'610000000044','reorder':'0','units':'oz','name':'Bread Crumbs Plain, Italian Style and Panko'}",
            "{'upc':'610000000045','reorder':'0','units':'oz','name':'Brownie Mix'}",
            "{'upc':'610000000046','reorder':'0','units':'oz','name':'Cereal (various)'}",
            "{'upc':'610000000047','reorder':'0','units':'oz','name':'Corn Bread Mix Complete'}",
            "{'upc':'610000000048','reorder':'0','units':'oz','name':'Elbow Macaroni'}",
            "{'upc':'610000000049','reorder':'0','units':'oz','name':'English Muffins'}",
            "{'upc':'610000000050','reorder':'0','units':'oz','name':'Hamburger Buns'}",
            "{'upc':'610000000051','reorder':'0','units':'oz','name':'Hotdog Buns'}",
            "{'upc':'610000000052','reorder':'0','units':'oz','name':'Mashed Potato Mix'}",
            "{'upc':'610000000053','reorder':'0','units':'oz','name':'Noodles'}",
            "{'upc':'610000000054','reorder':'0','units':'oz','name':'Oatmeal (for oatmeal and cookies)'}",
            "{'upc':'610000000055','reorder':'0','units':'oz','name':'Pancake Mix Complete'}",
            "{'upc':'610000000056','reorder':'0','units':'oz','name':'Saltine Crackers'}",
            "{'upc':'610000000057','reorder':'0','units':'oz','name':'Sliced Bread'}",
            "{'upc':'610000000058','reorder':'0','units':'oz','name':'Spaghetti'}",
            "{'upc':'610000000059','reorder':'0','units':'oz','name':'White Rice'}",
            					 
            //***** Canned–Shelf Safe *****
            "{'upc':'610000000060','reorder':'0','units':'oz','name':''}",
            "{'upc':'610000000061','reorder':'0','units':'oz','name':'Apple Pie Filling'}",
            "{'upc':'610000000062','reorder':'0','units':'oz','name':'Apple Sauce'}",
            "{'upc':'610000000063','reorder':'0','units':'oz','name':'Can Chicken-white meat'}",
            "{'upc':'610000000064','reorder':'0','units':'oz','name':'Can Tuna'}",
            "{'upc':'610000000065','reorder':'0','units':'oz','name':'Cherry Pie Filling'}",
            "{'upc':'610000000066','reorder':'0','units':'oz','name':'Coffee-Decaf'}",
            "{'upc':'610000000067','reorder':'0','units':'oz','name':'Coffee-Regular'}",
            "{'upc':'610000000068','reorder':'0','units':'oz','name':'Corn (or Frozen)'}",
            "{'upc':'610000000069','reorder':'0','units':'oz','name':'Fruit Punch Mix'}",
            "{'upc':'610000000070','reorder':'0','units':'oz','name':'Green Beans (or Frozen)'}",
            "{'upc':'610000000071','reorder':'0','units':'oz','name':'Iced Tea Mix'}",
            "{'upc':'610000000072','reorder':'0','units':'oz','name':'Jelly'}",
            "{'upc':'610000000073','reorder':'0','units':'oz','name':'LARGE Cans Chicken Noodle Soup'}",
            "{'upc':'610000000074','reorder':'0','units':'oz','name':'LARGE Cans Tomato Soup'}",
            "{'upc':'610000000075','reorder':'0','units':'oz','name':'Lemonade Mix'}",
            "{'upc':'610000000076','reorder':'0','units':'oz','name':'Maraschino Cherries'}",
            "{'upc':'610000000077','reorder':'0','units':'oz','name':'Pasta Sauce'}",
            "{'upc':'610000000078','reorder':'0','units':'oz','name':'Peanut Butter'}",
            "{'upc':'610000000079','reorder':'0','units':'oz','name':'Teabags-Decaf'}",
            "{'upc':'610000000080','reorder':'0','units':'oz','name':'Teabags-Regular'}",
            "{'upc':'610000000081','reorder':'0','units':'oz','name':'Tomato Paste'}",
            "{'upc':'610000000082','reorder':'0','units':'oz','name':'Tomato Puree'}",
            					 
            //***** Condiments *****
            "{'upc':'610000000083','reorder':'0','units':'oz','name':'BBQ Sauce'}",
            "{'upc':'610000000084','reorder':'0','units':'oz','name':'Balsamic Vinegar'}",
            "{'upc':'610000000085','reorder':'0','units':'oz','name':'Chocolate Syrup'}",
            "{'upc':'610000000086','reorder':'0','units':'oz','name':'Hot Sauce'}",
            "{'upc':'610000000087','reorder':'0','units':'oz','name':'Ketchup'}",
            "{'upc':'610000000088','reorder':'0','units':'oz','name':'Maple Syrup'}",
            "{'upc':'610000000089','reorder':'0','units':'oz','name':'Mayo'}",
            "{'upc':'610000000090','reorder':'0','units':'oz','name':'Mustard (brown and yellow)'}",
            "{'upc':'610000000091','reorder':'0','units':'oz','name':'Olives'}",
            "{'upc':'610000000092','reorder':'0','units':'oz','name':'Pickles'}",
            "{'upc':'610000000093','reorder':'0','units':'oz','name':'Red Wine Vinegar'}",
            "{'upc':'610000000094','reorder':'0','units':'oz','name':'Relish'}",
            "{'upc':'610000000095','reorder':'0','units':'oz','name':'Salad Dressing (various)'}",
            "{'upc':'610000000096','reorder':'0','units':'oz','name':'Wing Sauce'}",
            					 
            //*** Frozen ****    
            "{'upc':'610000000097','reorder':'0','units':'oz','name':'Bacon'}",
            "{'upc':'610000000098','reorder':'0','units':'oz','name':'Bagels (or fresh)'}",
            "{'upc':'610000000099','reorder':'0','units':'oz','name':'Breakfast Sausage'}",
            "{'upc':'610000000100','reorder':'0','units':'oz','name':'Chicken Breast'}",
            "{'upc':'610000000101','reorder':'0','units':'oz','name':'Chicken Drums'}",
            "{'upc':'610000000102','reorder':'0','units':'oz','name':'Chicken Nuggets'}",
            "{'upc':'610000000103','reorder':'0','units':'oz','name':'Chicken Thighs'}",
            "{'upc':'610000000104','reorder':'0','units':'oz','name':'Chicken Wings'}",
            "{'upc':'610000000105','reorder':'0','units':'oz','name':'Chopped Meat'}",
            "{'upc':'610000000106','reorder':'0','units':'oz','name':'Dinner Rolls'}",
            "{'upc':'610000000107','reorder':'0','units':'oz','name':'French Fries'}",
            "{'upc':'610000000108','reorder':'0','units':'oz','name':'Hamburgers'}",
            "{'upc':'610000000109','reorder':'0','units':'oz','name':'Hotdogs'}",
            "{'upc':'610000000110','reorder':'0','units':'oz','name':'Smilie Fries'}",
            					 
            //*** Veg/Fruit ***  
            "{'upc':'610000000111','reorder':'0','units':'oz','name':''}",
            "{'upc':'610000000112','reorder':'0','units':'oz','name':'Apple Juice'}",
            "{'upc':'610000000113','reorder':'0','units':'oz','name':'Apples'}",
            "{'upc':'610000000114','reorder':'0','units':'oz','name':'Bananas'}",
            "{'upc':'610000000115','reorder':'0','units':'oz','name':'Broccoli'}",
            "{'upc':'610000000116','reorder':'0','units':'oz','name':'Carrots'}",
            "{'upc':'610000000117','reorder':'0','units':'oz','name':'Celery'}",
            "{'upc':'610000000118','reorder':'0','units':'oz','name':'Coleslaw'}",
            "{'upc':'610000000119','reorder':'0','units':'oz','name':'Corn on the Cob'}",
            "{'upc':'610000000120','reorder':'0','units':'oz','name':'Cucumbers'}",
            "{'upc':'610000000121','reorder':'0','units':'oz','name':'Garlic'}",
            "{'upc':'610000000122','reorder':'0','units':'oz','name':'Grapes'}",
            "{'upc':'610000000123','reorder':'0','units':'oz','name':'Lemons'}",
            "{'upc':'610000000124','reorder':'0','units':'oz','name':'Lettuce'}",
            "{'upc':'610000000125','reorder':'0','units':'oz','name':'Onions'}",
            "{'upc':'610000000126','reorder':'0','units':'oz','name':'Orange Juice'}",
            "{'upc':'610000000127','reorder':'0','units':'oz','name':'Oranges'}",
            "{'upc':'610000000128','reorder':'0','units':'oz','name':'Peppers'}",
            "{'upc':'610000000129','reorder':'0','units':'oz','name':'Potato Salad'}",
            "{'upc':'610000000130','reorder':'0','units':'oz','name':'Potatoes'}",
            "{'upc':'610000000131','reorder':'0','units':'oz','name':'Tangerines/Mandarin'}",
            "{'upc':'610000000132','reorder':'0','units':'oz','name':'Tomatoes'}",
        };


        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {

            if (Item.Name == "FIX_CHRS.List")
            {
                string srcFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "ChrsSrc.txt");
                string destFile = Path.Combine(hostEnv.WebRootPath, "FixDB", "ChrsList.txt");

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
                            string[] chrsList = line.Substring(sidx, eidx - sidx).Trim().Replace("[", "").Replace("]", "").Split(',');
                            bool isFirst = true;
                            int NewCID = -1;
                            foreach (var cid in chrsList)
                            {
                                int id = int.Parse(cid.Trim());
                                Church chr = _context.Church.FirstOrDefault(p => p.Id == id);

                                if (chr != null)
                                {
                                    if (isFirst)
                                    {
                                        sw.WriteLine("KEY=" + id + " Name=" + chr.Name);
                                        isFirst = false;
                                        NewCID = chr.Id;
                                    }
                                    else
                                    {
                                        sw.WriteLine(" id=" + id + " Name=" + chr.Name);

                                        // DELETE CHURCH
                                        _context.Church.Remove(chr);
                                        try
                                        {
                                            await _context.SaveChangesAsync();
                                        }
                                        catch (Exception)
                                        {
                                            sw.WriteLine("**** DB ERROR: ***** Changing person.cid. cid=[" + chr.Id + "]");
                                            return RedirectToPage("./Index");
                                        }

                                        //// MERGE CHURCH
                                        //var parr = _context.Person.Where(p => p.ChurchId == chr.Id).ToList<Person>();
                                        //foreach (var p in parr)
                                        //{
                                        //    p.ChurchId = NewCID;
                                        //    _context.Attach(p).State = EntityState.Modified;

                                        //    try
                                        //    {
                                        //        await _context.SaveChangesAsync();
                                        //    }
                                        //    catch (Exception e)
                                        //    {
                                        //        sw.WriteLine("**** DB ERROR: ***** Changing person.cid. cid=[" + chr.Id + "]");
                                        //        return RedirectToPage("./Index");
                                        //    }
                                        //}
                                    }
                                }
                                else
                                {
                                    sw.WriteLine("**** Cant Find id [" + cid + "]");
                                }
                            }
                        }
                        sw.WriteLine("----------------------------------------------");
                    }
                }
            }

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
                                        catch (Exception)
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
                                        catch (Exception)
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
                    InventoryItem.ItemUnits units;
                    if (InventoryItem.ItemUnits.TryParse(idobj.units, out units))
                        item.Units = (int)units;
                    else
                        item.Units = (int)InventoryItem.ItemUnits.unknown;
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
