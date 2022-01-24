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
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public CreateModel(CStat.Models.CStatContext context)
        {
            _context = context;
            _Person = new Person();
        }

        public IActionResult OnGet()
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "", Value = "0" });
            //ViewData["Gender"] = new SelectList(gList, "Value", "Text");
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
            ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");
            ViewData["Pg1PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            ViewData["Pg2PersonId"] = new SelectList(_context.Person, "Id", "FirstName");

            UpdateLists();
            return Page();
        }

        private void UpdateLists()
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "?", Value = "0" });
            ViewData["Gender"] = new SelectList(gList, "Value", "Text");

            List<SelectListItem> bList = new List<SelectListItem> {
                new SelectListItem { Text = "?", Value = "0" },
                new SelectListItem { Text = "N", Value = "1" },
                new SelectListItem { Text = "Y", Value = "2" }
            };              
            ViewData["Baptized"] = new SelectList(bList, "Value", "Text");

            long Roles = _Person.Roles.HasValue ? _Person.Roles.Value : 0;
            IList<SelectListItem> trList = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Select(x => new SelectListItem { Text = x.ToString().Replace("_N", " & ").Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            int[] selIDs = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Where(x => ((long)x & Roles) != 0).Select(x => (int)x).ToArray<int>();
            ViewData["TitleRoles"] = new MultiSelectList(trList, "Value", "Text", selIDs);

            List<SelectListItem> csl = new SelectList(_context.Church.OrderBy(c => c.Name), "Id", "Name").ToList();
            csl.Insert(0, (new SelectListItem { Text = "none", Value = "-1" }));
            ViewData["ChurchId"] = csl;
            ViewData["Pg1PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            ViewData["Pg2PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
        }
        public String GenSkillButtons(int skills)
        {
            String btnStr = "<div style=\"display:inline-block\" width=100%>\n";
            int i = 1;
            foreach (var s in Enum.GetValues(typeof(eSkills)).Cast<eSkills>())
            {
                string btnClass = (((int)s & skills) != 0) ? "BtnOn" : "BtnOff";
                btnStr += "<button type=\"button\" id=\"BtnId" + (i++) + "\" abbr=\"" + ((eSkillsAbbr)s).ToString() + "\" class=\"" + btnClass + "\" value=\"" + ((int)s).ToString() + "\">" + s.ToString() + "</button>\n";
            }
            btnStr += "</div>\n";
            return btnStr;
        }

        [BindProperty]
        public Person _Person { get; set; }

        [BindProperty]
        public int[] _TitleRoles { get; set; }

        [BindProperty]
        public int? _Baptized { get; set; }

        [BindProperty]
        public string _SSNum { get; set; } = "";

        [BindProperty]
        public string _PG1 { get; set; } = "";

        [BindProperty]
        public string _PG2 { get; set; } = "";

        [BindProperty]
        public string _Street { get; set; } = "";

        [BindProperty]
        public string _Town { get; set; } = "";

        [BindProperty]
        public string _State { get; set; } = "";

        [BindProperty]
        public string _ZipCode { get; set; } = "";

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

            _Person.Roles = 0;
            foreach (var i in _TitleRoles)
            {
                _Person.Roles |= (long)i;
            }

            String bapStr = "";
            if ((_Baptized.HasValue) && (_Baptized.Value > 0))
                bapStr = (_Baptized.Value == 2) ? "Y" : "N";
            _PG1 = GetVS(_PG1);
            _PG2 = GetVS(_PG2);
            _Person.Address.Street = _Street;
            _Person.Address.Town = _Town;
            _Person.Address.State = _State;
            _Person.Address.ZipCode = _ZipCode;

            await _Person.AddPerson(_context, bapStr, _PG1.Trim(), _PG2.Trim());

            return RedirectToPage("./Find");
        }
    }
}
