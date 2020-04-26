using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using Newtonsoft.Json;
using static CStat.Models.Person;
using Microsoft.EntityFrameworkCore;
using Dropbox.Api.Sharing;

namespace CStat
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string id)
        {
            int pid = int.Parse(id);

            _Person = _context.Person.FirstOrDefault(p => p.Id == pid);
            if ((_Person != null) && (_Person.AddressId != null))
            {
                _Address = _context.Address.FirstOrDefault(a => a.Id == _Person.AddressId);
            }
            if ((_Person == null))
                _Person = new Person();

            if ((_Address == null))
                _Address = new Address();

            UpdateLists(_Person.ChurchId);
            return Page();
        }

        private void UpdateLists(int? cid)
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "", Value = "0" });
            ViewData["Gender"] = new SelectList(gList, "Value", "Text");
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");

            List<SelectListItem> csl = new SelectList(_context.Church.OrderBy(c => c.Name), "Id", "Name").ToList();
            csl.Insert(0, (new SelectListItem { Text = "none", Value = "-1" }));
            if (cid.HasValue)
            {
                foreach (var cs in csl)
                {
                    if (cs.Value == cid.ToString())
                    {
                        cs.Selected = true;
                        break;
                    }
                }
            }
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
        public Address _Address { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnPost()
        {
            try
            {
                bool isValidAdr = ((_Address.Street != null) && (_Address.Street.Length > 0) &&
                                (_Address.Town != null) && (_Address.Town.Length > 0) &&
                                (_Address.State != null) && (_Address.State.Length > 0));
                if ((_Person.AddressId == null) && isValidAdr)
                {
                    _context.Address.Add(_Address);
                    _context.SaveChanges();
                    _Person.AddressId = _Address.Id;
                }
                else
                {
                    if (isValidAdr)
                    {
                        _context.Attach(_Address).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }

                if (_Person.ChurchId == -1)
                    _Person.ChurchId = null;
                _context.Attach(_Person).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return Page();
            }

            return RedirectToPage("./Find");
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
        private bool PersonExists(int id)
        {
            return _context.Person.Any(e => e.Id == id);
        }
    }
}
