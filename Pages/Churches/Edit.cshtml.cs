using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using static CStat.Models.Church;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CStat.Pages.Churches
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            _Church = await _context.Church.AsNoTracking()
                .Include(c => c.Address)
                .Include(c => c.SeniorMinister)
                .Include(c => c.YouthMinister)
                .Include(c => c.Trustee1)
                .Include(c => c.Trustee2)
                .Include(c => c.Trustee3)
                .Include(c => c.Alternate1)
                .Include(c => c.Alternate2)
                .Include(c => c.Alternate3)
                .FirstOrDefaultAsync(c => c.Id == id);

            IList<SelectListItem> AffList = ChurchLists.AffiliationList.Select(a => new SelectListItem { Text = a.name, Value = a.GetEnumType().ToString() }).ToList();
            IList<SelectListItem> MembList = ChurchLists.MemberStatList.Select(m => new SelectListItem { Text = m.name, Value = m.GetEnumType().ToString() }).ToList();

            AffiliationType affType;
            _Affiliation = (!string.IsNullOrEmpty(_Church.Affiliation) && Enum.TryParse(_Church.Affiliation, out affType)) ? (int)affType : 0;

            _SeniorMinister = (_Church.SeniorMinister != null) ? _Church.SeniorMinister.FirstName + " " + _Church.SeniorMinister.LastName : "";
            _YouthMinister =  (_Church.YouthMinister != null) ? _Church.YouthMinister.FirstName + " " + _Church.YouthMinister.LastName : "";
            _Trustee1 = (_Church.Trustee1 != null) ? _Church.Trustee1.FirstName + " " + _Church.Trustee1.LastName : "";
            _Trustee2 = (_Church.Trustee2 != null) ? _Church.Trustee2.FirstName + " " + _Church.Trustee2.LastName : "";
            _Trustee3 = (_Church.Trustee3 != null) ? _Church.Trustee3.FirstName + " " + _Church.Trustee3.LastName : "";
            _Alternate1 = (_Church.Alternate1 != null) ? _Church.Alternate1.FirstName + " " + _Church.Alternate1.LastName : "";
            _Alternate2 = (_Church.Alternate2 != null) ? _Church.Alternate2.FirstName + " " + _Church.Alternate2.LastName : "";
            _Alternate3 = (_Church.Alternate3 != null) ? _Church.Alternate3.FirstName + " " + _Church.Alternate3.LastName : "";

        ViewData["Affil"] = AffList;
            ViewData["Memb"] = MembList;
            return Page();
        }

        [BindProperty]
        public Church _Church { get; set; }

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
        [BindProperty]
        public int _Affiliation { get; set; } = 0;

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            //if (!ModelState.IsValid)
            //{
            //    return Page();
            //}

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
                _Church.AddressId = id;
                _Church.Address = null;
            }
            else
            {
                _Church.AddressId = null;
                _Church.Address = null;
            }

            if (!_Church.SeniorMinisterId.HasValue)
                _Church.SeniorMinisterId = Person.PersonIdFromExactName(_context, _SeniorMinister);
            if (!_Church.YouthMinisterId.HasValue)
                _Church.YouthMinisterId = Person.PersonIdFromExactName(_context, _YouthMinister);
            if (!_Church.Trustee1Id.HasValue)
                _Church.Trustee1Id = Person.PersonIdFromExactName(_context, _Trustee1);
            if (!_Church.Trustee2Id.HasValue)
                _Church.Trustee2Id = Person.PersonIdFromExactName(_context, _Trustee2);
            if (!_Church.Trustee3Id.HasValue)
                _Church.Trustee3Id = Person.PersonIdFromExactName(_context, _Trustee3);
            if (!_Church.Alternate1Id.HasValue)
                _Church.Alternate1Id = Person.PersonIdFromExactName(_context, _Alternate1);
            if (!_Church.Alternate2Id.HasValue)
                _Church.Alternate2Id = Person.PersonIdFromExactName(_context, _Alternate2);
            if (!_Church.Alternate3Id.HasValue)
                _Church.Alternate3Id = Person.PersonIdFromExactName(_context, _Alternate3);

            _Church.Affiliation = ((AffiliationType)_Affiliation).ToString();

            _context.Attach(_Church).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            }

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
        public JsonResult OnGetDeleteChurch()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
            if (NVPairs.TryGetValue("churchId", out int churchId))
            {
                _Church = _context.Church.Find(churchId);

                if (_Church != null)
                {
                    try
                    {
                        // Delete Church
                        _context.Church.Remove(_Church);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~: Exception : Delete Church Failed.");
                    }
                    return new JsonResult("SUCCESS~:Delete Church Succeeded.");
                }
            }
            return new JsonResult("ERROR~: Delete Church Failed.");
        }
    }
}
