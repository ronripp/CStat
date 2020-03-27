using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using static CStat.Models.Person;

namespace CStat
{
    public class FindModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public FindModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "", Value = "0" });
            ViewData["Gender"] = new SelectList(gList, "Value", "Text");
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
            ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");
            ViewData["Pg1PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            ViewData["Pg2PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            return Page();
        }

        public String GenSkillButtons()
        {
            String btnStr = "<div style=\"display:inline-block\" width=100%>\n";
            int i = 1;
            foreach (var s in Enum.GetValues(typeof(eSkills)).Cast<eSkills>())
            {
                btnStr += "<button type=\"button\" id=\"BtnId" + (i++) + "\" class=\"BtnOff\" value=\"" + ((int)s).ToString() + "\">" + s.ToString() + "</button>\n";
            }
            btnStr += "</div>\n";
            return btnStr;
        }

        [BindProperty]
        public Person Person { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Person.Add(Person);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
