﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Newtonsoft.Json.Linq;

namespace CStat.Pages
{
    public class MergeModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public MergeModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Person Person { get; set; }

        [BindProperty]
        public int Id { get; set; } = 1;

        public IList<Person> People { get; set; }

        public IList<Address> Address { get; set; }

        public IList<Church> Church { get; set; }

        public IActionResult OnGet(int id)
        {
            Id = id;
            switch (Id)
            {
                default:
                case 1:
                    People = _context.Person.OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
                    break;
                case 2:
                    Address = _context.Address.OrderBy(p => p.Town).ThenBy(p => p.Street).ToList();
                    break;
                case 3:
                    Church = _context.Church.OrderBy(p => p.Name).ToList();
                    break;
            }

            //id = 3025;
            //Person = await _context.Person
            //    .Include(p => p.Address)
            //    .Include(p => p.Church)
            //    .Include(p => p.Pg1Person)
            //    .Include(p => p.Pg2Person).FirstOrDefaultAsync(m => m.Id == id);

           //ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
           //ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");
           //ViewData["Pg1PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
           //ViewData["Pg2PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Person).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(Person.Id))
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

        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
        public JsonResult OnGetDoMerge()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            if (String.IsNullOrEmpty(rawQS))
                return new JsonResult("ERROR~:No Parameters");

            int stIdx = rawQS.IndexOf("{'cmd");
            if (stIdx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(stIdx);
            try
            {
                JObject jObj = JObject.Parse(jsonQS);
                string cmdStr = (string)jObj["cmd"] ?? "";
                return new JsonResult("OK~:Cleanup");
            }
            catch (Exception e)
            {
                _ = e;
                return new JsonResult("ERROR~: OnGetCamCleanup Exception");
            }
        }


    }
}