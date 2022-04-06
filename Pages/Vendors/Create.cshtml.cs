using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;

namespace CStat.Pages.Vendors
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
            IList<SelectListItem> BizTypeList = Enum.GetValues(typeof(Business.EType)).Cast<Business.EType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizTypeList"] = BizTypeList;
            IList<SelectListItem> BizStatusList = Enum.GetValues(typeof(Business.EStatus)).Cast<Business.EStatus>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizStatusList"] = BizStatusList;

            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");

            Business = new Business();
            return Page();
        }

        [BindProperty]
        public Business Business { get; set; }
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
        [BindProperty]
        public string _poc { get; set; } = "";

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            try
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
                    Business.AddressId = id;
                    Business.Address = adr;
                    Business.Address.Id = id.Value;
                }
                else
                {
                    Business.AddressId = null;
                    Business.Address = null;
                }

                Business.PocId = !string.IsNullOrEmpty(_poc) ? Person.PersonIdFromExactName(_context, _poc) : null;
                _context.Business.Add(Business);
                await _context.SaveChangesAsync();
            }
            catch
            { }
            return RedirectToPage("./Index");
        }
    }
}
