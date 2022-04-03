using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Vendors
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
        {
            _context = context;
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
        public int _pocId { get; set; } = -1;
        [BindProperty]
        public string _poc { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Business = await _context.Business
                .Include(b => b.Address)
                .Include(b => b.Poc).FirstOrDefaultAsync(m => m.Id == id);
            if (Business == null)
            {
                return NotFound();
            }

            _poc = (Business.PocId != null) ? Business.Poc.FirstName + " " + Business.Poc.LastName : "";
            ViewData["PocId"] = new SelectList(_context.Person, "Id", "FirstName");

            IList<SelectListItem> BizTypeList = Enum.GetValues(typeof(Business.EType)).Cast<Business.EType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizTypeList"] = BizTypeList;
            IList<SelectListItem> BizStatusList = Enum.GetValues(typeof(Business.EStatus)).Cast<Business.EStatus>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizStatusList"] = BizStatusList;

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

            Address adr = new Address();
            adr.Street = _Street;
            adr.Town = _Town;
            adr.ZipCode = _ZipCode;
            adr.Phone = _Phone;
            adr.State = _State;
            adr.Fax = _Fax;
            adr.Country = "USA";

            int? id = Address.UpdateAddress(_context, adr);
            Business.AddressId = (id > 0) ? id : null;
            Business.PocId = !string.IsNullOrEmpty(_poc) ? Person.PersonIdFromExactName(_context, _poc) : null;


            _context.Attach(Business).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusinessExists(Business.Id))
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

        private bool BusinessExists(int id)
        {
            return _context.Business.Any(e => e.Id == id);
        }
    }
}
