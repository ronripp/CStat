﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using Newtonsoft.Json;
using static CStat.Models.Person;

namespace CStat
{
    public class PersonHints : Person
    {
        public string AgeRange { get; set; }
    }

    public class FindModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public FindModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            UpdateLists();
            return Page();
        }

        private void UpdateLists()
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "", Value = "0" });
            ViewData["Gender"] = new SelectList(gList, "Value", "Text");
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
            ViewData["ChurchId"] = new SelectList(_context.Church.OrderBy(c => c.Name), "Id", "Name");
            ViewData["Pg1PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            ViewData["Pg2PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            IList<SelectListItem> trList = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Select(x => new SelectListItem { Text = x.ToString().Replace("_N", " & ").Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["TitleRoles"] = new MultiSelectList(trList, "Value", "Text");
        }

        public String GenSkillButtons()
        {
            String btnStr = "<div style=\"display:inline-block\" width=100%>\n";
            int i = 1;
            foreach (var s in Enum.GetValues(typeof(eSkills)).Cast<eSkills>())
            {
                btnStr += "<button type=\"button\" id=\"BtnId" + (i++) + "\" abbr=\"" + Enum.GetName(typeof(eSkillsAbbr), s) + "\" class=\"BtnOff\" value=\"" + ((int)s).ToString() + "\">" + s.ToString() + "</button>\n";
            }
            btnStr += "</div>\n";
            return btnStr;
        }

        [BindProperty]
        public PersonHints _Person { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    UpdateLists();
            //    return Page();
            //}

            _context.Person.Add(_Person);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public JsonResult OnGetFindPeople()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            return new JsonResult(Person.FindPeople(_context, "Find People:" + jsonQS));
        }
    }
}
