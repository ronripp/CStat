using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using static CStat.Models.Church;

namespace CStat.Pages.Churches
{
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public CreateModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            IList<SelectListItem> AffList = ChurchLists.AffiliationList.Select(a => new SelectListItem { Text = a.name, Value = a.GetEnumType().ToString() }).ToList();
            IList<SelectListItem> MembList = ChurchLists.MemberStatList.Select(m => new SelectListItem { Text = m.name, Value = m.GetEnumType().ToString() }).ToList();
            ViewData["Affil"] = AffList;
            ViewData["Memb"] = MembList;
            return Page();
        }

        [BindProperty]
        public Church Church { get; set; }

        [BindProperty]
        public string _SeniorMinister { get; set; } = "";
        [BindProperty]
        public string _YouthMinister { get; set; } = "";
        [BindProperty]
        public string _Trustee1 { get; set; } = "";
        [BindProperty]
        public string _Trustee2 { get; set; } = "";
        [BindProperty]
        public string _Trustee3 { get; set; } = "";
        [BindProperty]
        public string _Alternate1 { get; set; } = "";
        [BindProperty]
        public string _Alternate2 { get; set; } = "";
        [BindProperty]
        public string _Alternate3 { get; set; } = "";
        [BindProperty]
        public string _Street { get; set; } = "";
        [BindProperty]
        public string _Town { get; set; } = "";
        [BindProperty]
        public string _State { get; set; } = "";
        [BindProperty]
        public string _ZipCode { get; set; } = "";
        [BindProperty]
        public string _Phone { get; set; } = "";
        [BindProperty]
        public string _Fax { get; set; } = "";

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Address adr = new Address();
            adr.Street = _Street;
            adr.Town = _Town;
            adr.ZipCode = _ZipCode;
            adr.Phone = _Phone;
            adr.State = _State;
            adr.Fax = _Fax;
            adr.Country = "USA";

            if (!string.IsNullOrEmpty(adr.Phone) || !string.IsNullOrEmpty(adr.Fax))
            {
                if (!AddressMgr.Validate(ref adr))
                {
                    if (string.IsNullOrEmpty(adr.Street)) adr.Street = "<missing>";
                    if (string.IsNullOrEmpty(adr.Town)) adr.Town = "<missing>";
                    if (string.IsNullOrEmpty(adr.State)) adr.State = "NY";
                    if (string.IsNullOrEmpty(adr.ZipCode)) adr.ZipCode = "11111";
                }
            }

            int? id = Address.UpdateAddress(_context, adr);
            if (id.HasValue && (id.Value > 0))
            {
                Church.AddressId = id;
                Church.Address = null;
            }
            else
            {
                Church.AddressId = null;
                Church.Address = null;
            }

            Church.SeniorMinisterId = Person.PersonIdFromExactName(_context, _SeniorMinister);
            Church.YouthMinisterId = Person.PersonIdFromExactName(_context, _YouthMinister);
            Church.Trustee1Id = Person.PersonIdFromExactName(_context, _Trustee1);
            Church.Trustee2Id = Person.PersonIdFromExactName(_context, _Trustee2);
            Church.Trustee3Id = Person.PersonIdFromExactName(_context, _Trustee3);
            Church.Alternate1Id = Person.PersonIdFromExactName(_context, _Alternate1);
            Church.Alternate2Id = Person.PersonIdFromExactName(_context, _Alternate2);
            Church.Alternate3Id = Person.PersonIdFromExactName(_context, _Alternate3);

            _context.Church.Add(Church);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }

        public JsonResult OnGetFindPersonChurch()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            string name = "";
            var idx = rawQS.IndexOf("{'name':");
            if (idx != -1)
            {
                name = rawQS.Substring(idx + 8);
                var eidx = name.IndexOf("'}");
                if (eidx != -1)
                    name = name.Substring(0, eidx);
            }

            var pList = Person.PeopleFromNameHint(_context, name);
            if (pList.Count == 0)
                return new JsonResult("FAIL");

            string options = "";
            pList.ForEach(p =>
            {
                options += "   <option value=\"" + p.FirstName + " " + p.LastName + "\">\n";
            });

            return new JsonResult(options);
        }
    }
}
