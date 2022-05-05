using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Newtonsoft.Json;

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
        public Business _Business { get; set; }
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
        [BindProperty]
        public string _RedirectURL { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _Business = await _context.Business.AsNoTracking()
                .Include(b => b.Address)
                .Include(b => b.Poc).FirstOrDefaultAsync(m => m.Id == id);
            if (_Business == null)
            {
                return NotFound();
            }
            if (!string.IsNullOrEmpty(_Business.UserLink))
                _Business.UserLink = _Business.UserLink.Trim();

            _poc = (_Business.PocId != null) ? _Business.Poc.FirstName + " " + _Business.Poc.LastName : "";
            _Street = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.Street)) ? _Business.Address.Street : "";
            _Town = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.Town)) ? _Business.Address.Town : "";
            _State = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.State)) ? _Business.Address.State : "";
            _ZipCode = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.ZipCode)) ? Address.FixZip(_Business.Address.ZipCode) : "";
            _Phone = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.Phone)) ? _Business.Address.Phone : "";
            _Fax = ((_Business.Address != null) && !string.IsNullOrEmpty(_Business.Address.Fax)) ? _Business.Address.Fax : "";
            ViewData["PocId"] = new SelectList(_context.Person, "Id", "FirstName");

            IList<SelectListItem> BizTypeList = Enum.GetValues(typeof(Business.EType)).Cast<Business.EType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizTypeList"] = BizTypeList;
            IList<SelectListItem> BizStatusList = Enum.GetValues(typeof(Business.EStatus)).Cast<Business.EStatus>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            ViewData["BizStatusList"] = BizStatusList;
            _RedirectURL = "";

            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(_Business.Name) || (_Business.Type == (int)Business.EType.Unknown))
            {
                return Page();
            }

            if (!string.IsNullOrEmpty(_ZipCode))
                _ZipCode = Address.StripZip(_ZipCode);

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
                _Business.AddressId = id;
                _Business.Address = null;
            }
            else
            {
                _Business.AddressId = null;
                _Business.Address = null;
            }
            _Business.PocId = !string.IsNullOrEmpty(_poc) ? Person.PersonIdFromExactName(_context, _poc) : null;
            if (!string.IsNullOrEmpty(_Business.UserLink))
                _Business.UserLink = _Business.UserLink.Trim();

            _context.Attach(_Business).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusinessExists(_Business.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (string.IsNullOrEmpty(_RedirectURL))
                return RedirectToPage("./Index");
            _RedirectURL = "";
            return NotFound();
        }

        private bool BusinessExists(int id)
        {
            return _context.Business.Any(e => e.Id == id);
        }
        public Business.EType GetBusinessType()
        {
            return _Business.Type.HasValue ? (Business.EType)_Business.Type.Value : Business.EType.Unknown;
        }
        public Business.EStatus GetBusinessStatus()
        {
            return _Business.Status.HasValue ? (Business.EStatus)_Business.Status.Value : Business.EStatus.Vendor;
        }
        public string GetAdrState()
        {
            return !string.IsNullOrEmpty(_State) ? _State : "NY";
        }
        public JsonResult OnGetDeleteVendor()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
            if (NVPairs.TryGetValue("vendorId", out int vendorId))
            {
                _Business = _context.Business.Find(vendorId);

                if (_Business != null)
                {
                    try
                    {
                        // Delete Vendor
                        _context.Business.Remove(_Business);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~: Exception : Delete Business/Dept Failed.");
                    }
                    return new JsonResult("SUCCESS~:Delete Business/Dept Succeeded.");
                }
            }
            return new JsonResult("ERROR~: Delete Business/Dept Failed.");
        }
    }
}
