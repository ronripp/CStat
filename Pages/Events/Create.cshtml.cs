using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using CStat.Pages.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CStat.Pages.Events
{
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        public DateTime CreateStartDate;

        public CreateModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(string edate)
        {
            DateTime.TryParse(edate, out CreateStartDate);
            UpdateViewData();
            return Page();
        }

        [BindProperty]
        public Event _Event { get; set; } = null;

        [BindProperty]
        public string _Staff { get; set; } = "";

        private IList<SelectListItem> rList = null;
        private IList<SelectListItem> eList = null;

        private void UpdateViewData(string err = "")
        {
            IList<SelectListItem> cList = new SelectList(_context.Church, "Id", "Name").OrderBy(c => c.Text).ToList();
            cList.Insert(0, new SelectListItem { Text = "Select if Church Event", Value = "-1" });
            ViewData["Church"] = cList;

            IList<SelectListItem> tList = Enum.GetValues(typeof(Event.EventType)).Cast<Event.EventType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            tList.Insert(0, new SelectListItem { Text = "Select Event Type", Value = "-1" });
            ViewData["Type"] = tList;

            rList = Enum.GetValues(typeof(Attendance.AttendanceRoles)).Cast<Attendance.AttendanceRoles>().Where(a => (int)a >= (int)Attendance.AttendanceRoles.Sch_Host).Select(x => new SelectListItem { Text = x.ToString().Replace("Sch_", "").Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            eList = rList.Skip(11).ToList();
            rList.Insert(0, new SelectListItem { Text = "* Add Staff Role", Value = "-1" });
            ViewData["StaffRoles"] = rList;
            eList.Insert(0, new SelectListItem { Text = "* Add Staff Role", Value = "-1" });
            ViewData["ExtraRoles"] = eList;

            ViewData["EventError"] = err;
        }

        public bool IsStaffSelected(int index)
        {
            return (_Event != null) && _Event.Staff.HasValue && ((int.Parse(rList[index].Value) & _Event.Staff.Value) != 0);
        }

        // 128=~256=Eli Russo~512=~2048=~8192=Christine Gerkhardt~32768=~65536=~131072=~1048576=~
        public bool UpdateAttendance(int evid, int activeRoles, string staffStr)
        {
            try
            {
                // Check for and delete people assigned to un-needed roles.
                List<int> RoleList = Enum.GetValues(typeof(Attendance.AttendanceRoles)).Cast<int>().Where(r => (int)r >= (int)Attendance.AttendanceRoles.Sch_Host).Select(x => x).ToList();

                foreach (var r in RoleList)
                {
                    if ((r & activeRoles) == 0)
                    {
                        // Staff Role now not needed
                        Attendance delAtt = _context.Attendance.AsNoTracking().FirstOrDefault(a => (a.EventId == evid) && (a.RoleType == r));
                        if (delAtt != null)
                        {
                            _context.Attendance.Remove(delAtt);
                            _context.SaveChanges();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(staffStr))
                {
                    List<KeyValuePair<int, string>> kvpList = new List<KeyValuePair<int, string>>();
                    var pairs = staffStr.Split("~");
                    if (pairs.Length == 0)
                        return false;
                    foreach (var p in pairs)
                    {
                        var flds = p.Split("=");
                        if (flds.Length >= 1)
                        {
                            var keyStr = flds[0].Trim();
                            if (!string.IsNullOrEmpty(keyStr))
                            {
                                if (int.TryParse(keyStr, out int key))
                                {
                                    var val = (flds.Length == 2) ? flds[1].Trim() : "";
                                    var kvp = new KeyValuePair<int, string>(key, val);
                                    kvpList.Add(kvp);
                                }
                            }
                        }
                    }

                    kvpList.ForEach(kv =>
                    {
                        Attendance att = _context.Attendance.AsNoTracking().FirstOrDefault(a => (a.EventId == evid) && (a.RoleType == kv.Key));
                        if (att != null)
                        {
                            if (string.IsNullOrEmpty(kv.Value))
                            {
                            // Delete current
                            _context.Attendance.Remove(att);
                                _context.SaveChanges();
                            }
                            else
                            {
                            // Update Current
                            List<Person> persList = Person.PeopleFromNameHint(_context, kv.Value);
                                if (persList.Count == 1)
                                {
                                    att.PersonId = persList[0].Id;

                                    _context.Attendance.Attach(att);
                                    _context.Entry(att).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(kv.Value))
                            {
                            // Add new Attendance
                            var newAtt = new Attendance();
                                newAtt.EventId = evid;
                                newAtt.RoleType = kv.Key;

                                List<Person> persList = Person.PeopleFromNameHint(_context, kv.Value);
                                newAtt.PersonId = (persList.Count == 1) ? persList[0].Id : Person.AddPersonFromNameHint(_context, kv.Value);
                                if (newAtt.PersonId > 0)
                                {
                                    _context.Attendance.Add(newAtt);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    });
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if ((_Event.Description == null) || (_Event.Description.Trim().Length == 0) || (_Event.Type <= 0))
            {
                CreateStartDate = _Event.StartTime;
                UpdateViewData("Event Type and/or Event Description must be set.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                CreateStartDate = _Event.StartTime;
                UpdateViewData("One or more fields are not valid.");
                return Page();
            }

            if (_Event.ChurchId == -1)
                _Event.ChurchId = null;

            if (_Event.StartTime >= _Event.EndTime)
            {
                CreateStartDate = _Event.StartTime;
                UpdateViewData("End Date/Time must be later than Start Date/Time.");
                return Page();
            }

            UpdateAttendance(_Event.Id, _Event.Staff ?? 0, _Staff);

            _context.Event.Add(_Event);
            await _context.SaveChangesAsync();

            // Generate any tasks based on events
            var autogen = new AutoGen(_context);

            return RedirectToPage("./Index");
        }
        public JsonResult OnGetFindPersonEvent()
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
