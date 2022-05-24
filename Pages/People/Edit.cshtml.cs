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

            UpdateLists(_Person.ChurchId);
            InitECExire(_Person);

            return Page();
        }

        private void InitECExire(Person person)
        {
            _NeedECExpire = (person.Roles & (long)(Person.TitleRoles.President | Person.TitleRoles.Vice_Pres | Person.TitleRoles.Secretary | Person.TitleRoles.Treasurer | Person.TitleRoles.Memb_at_Lg)) != 0;

            _ECExpire = "";
            if (!string.IsNullOrEmpty(person.Notes))
            {
                var sidx = person.Notes.IndexOf("{ExpiresNov:");
                if (sidx != -1)
                {
                    sidx += 12;
                    var eidx = person.Notes.Substring(sidx).IndexOf("}");
                    if ((eidx != -1) && (eidx > 0))
                    {
                        _ECExpire = person.Notes.Substring(sidx, eidx);
                        person.Notes = person.Notes.Replace("{ExpiresNov:" + _ECExpire + "}", "").Trim();
                    }
                }
            }
        }

        private void UpdateECExire(Person person)
        {
            bool needECExpire = (person.Roles & (long)(Person.TitleRoles.President | Person.TitleRoles.Vice_Pres | Person.TitleRoles.Secretary | Person.TitleRoles.Treasurer | Person.TitleRoles.Memb_at_Lg)) != 0;
            string ECExpireYear = _ECExpire.Trim();
            var expYearLen = ECExpireYear.Length;
            if (expYearLen > 4)
                ECExpireYear = ECExpireYear.Substring(0, 4);
            else if (expYearLen == 2)
                ECExpireYear = "20" + ECExpireYear;

            if (!string.IsNullOrEmpty(person.Notes))
            {
                var sidx = person.Notes.IndexOf("{ExpiresNov:");
                if (sidx != -1)
                {
                    // This should not happen but code is here to clean it up just in case.
                    sidx += 12;
                    var eidx = person.Notes.Substring(sidx).IndexOf("}");
                    if ((eidx != -1) && (eidx > 0))
                    {
                        _ECExpire = person.Notes.Substring(sidx, eidx);
                        person.Notes = person.Notes.Replace("{ExpiresNov:" + _ECExpire + "}", "").Trim();
                    }
                }
            }
            if (needECExpire && (ECExpireYear.Length > 0))
            {
                person.Notes = person.Notes.Trim() + "{ExpiresNov:" + ECExpireYear + "}";
            }
        }

        private void UpdateLists(int? cid)
        {
            IList<SelectListItem> gList = Enum.GetValues(typeof(eGender)).Cast<eGender>().Select(x => new SelectListItem { Text = x.ToString(), Value = ((int)x).ToString() }).ToList();
            gList.Insert(0, new SelectListItem { Text = "", Value = "0" });
            ViewData["Gender"] = new SelectList(gList, "Value", "Text");
            ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
            long Roles = _Person.Roles.HasValue ? _Person.Roles.Value : 0;
            IList<SelectListItem> trList = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Select(x => new SelectListItem { Text = x.ToString().Replace("_N", " & ").Replace("_", " "), Value = ((int)x).ToString()}).ToList();
            int[] selIDs = Enum.GetValues(typeof(Person.TitleRoles)).Cast<Person.TitleRoles>().Where(x => ((long)x & Roles) != 0).Select(x => (int)x).ToArray<int>();
            ViewData["TitleRoles"] = new MultiSelectList(trList, "Value", "Text", selIDs);

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
        public int [] _TitleRoles { get; set; }

        [BindProperty]
        public Address _Address { get; set; }

        [BindProperty]
        public string _HandleAdrId { get; set; }

        [BindProperty]
        public string _ECExpire { get; set; } = "";

        [BindProperty]
        public bool _NeedECExpire { get; set; } = false;

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public IActionResult OnPost()
        {
            try
            {
                if (_Person.ChurchId == -1)
                    _Person.ChurchId = null;

                bool isValidAdr = ((_Address.Street != null) && (_Address.Street.Length > 0) &&
                                (_Address.Town != null) && (_Address.Town.Length > 0) &&
                                (_Address.State != null) && (_Address.State.Length > 0));

                if (isValidAdr && (_HandleAdrId == "KeepOld"))
                {
                    _Address.Id = 0;
                    _Person.AddressId = null;
                }

                _Person.Roles = 0;
                foreach( var i in  _TitleRoles)
                {
                    _Person.Roles |= (long)i;
                }

                UpdateECExire(_Person);

                if ((_Person.AddressId == null) && isValidAdr)
                {
                    if (_Address.Country == null)
                        _Address.Country = "USA"; // Assume USA if not specified.

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

                _context.Attach(_Person).State = EntityState.Modified;
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                _ = e;
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

        public JsonResult OnGetDeletePerson()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
            if (NVPairs.TryGetValue("personId", out int personId))
            {
                _Person = _context.Person.Find(personId);

                if (_Person != null)
                {
                    try
                    {
                        // Delete Person
                        _context.Person.Remove(_Person);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~: Exception : Delete Person Failed.");
                    }
                    return new JsonResult("SUCCESS~:Delete Person Succeeded.");
                }
            }
            return new JsonResult("ERROR~: Delete Person Failed.");
        }


    }
}
