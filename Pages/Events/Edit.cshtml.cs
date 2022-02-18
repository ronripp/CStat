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
using CStat.Pages.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace CStat.Pages.Events
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;

        public EditModel(IWebHostEnvironment hostEnv, CStat.Models.CStatContext context)
        {
            _context = context;
            _hostEnv = hostEnv;
        }

        [BindProperty]
        public Event _Event { get; set; }

        [BindProperty]
        public string _Staff { get; set; } = "";

        [BindProperty]
        public string _RedirectURL { get; set; } = "";

        private IList<SelectListItem> rList = null;
        private IList<SelectListItem> eList = null;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _Event = await _context.Event
                .Include(e => e.Church).FirstOrDefaultAsync(m => m.Id == id);

            if (_Event == null)
            {
                return NotFound();
            }

            _RedirectURL = "";
            UpdateViewData();

            return Page();
        }
        public bool IsStaffSelected(int index)
        {
            return (_Event != null) && _Event.Staff.HasValue && ((int.Parse(rList[index].Value) & _Event.Staff.Value) != 0);
        }
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

        public string GetRoleName (string roleStr)
        {
            if (!int.TryParse(roleStr, out int role))
                return "";

            var att = _context.Attendance.Include(a => a.Person).FirstOrDefault(a => (a.EventId == _Event.Id) && (a.RoleType == role));
            if (att == null)
                return "";
            return ((att.PersonId > 0) && (att.Person != null)) ? att.Person.FirstName + " " + att.Person.LastName : "";
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

                if (string.IsNullOrEmpty(staffStr))
                    return false;
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
                UpdateViewData("Event Type and/or Event Description must be set.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                UpdateViewData("One or more fields are not valid.");
                return Page();
            }

            if (_Event.ChurchId == -1)
                _Event.ChurchId = null;

            if (_Event.StartTime >= _Event.EndTime)
            {
                UpdateViewData("End Date/Time must be later than Start Date/Time.");
                return Page();
            }

            // Check if Start or End Dates have changed and delete tasks associated with event and generate new ones.
            int NumTasksRemoved = RemoveChangedEventTasks(_context, _Event);

            UpdateAttendance(_Event.Id, _Event.Staff ?? 0, _Staff);

            _context.Attach(_Event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(_Event.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            if (NumTasksRemoved > 0)
            {
                // Regenerate tasks based on event now
                var autogen = new AutoGen(_context);
                autogen.GenTasks(_hostEnv);
            }

            if (string.IsNullOrEmpty(_RedirectURL))
                return RedirectToPage("./Index");
            return NotFound();
        }

        public static int RemoveChangedEventTasks (CStat.Models.CStatContext context, Event newEvent)
        {
            int NumDeleted = 0;
            var oldEvent = context.Event.AsNoTracking().FirstOrDefaultAsync(e => e.Id == newEvent.Id).Result;
            if ((oldEvent != null) && ((newEvent.StartTime != oldEvent.StartTime) || (newEvent.EndTime != oldEvent.EndTime)))
            {
                var evTasks = context.Task.Where(t => (t.EventId.HasValue && t.EventId == newEvent.Id));
                foreach (var t in evTasks)
                {
                    try
                    {
                        context.Task.Remove(t);
                        context.SaveChangesAsync();
                        ++NumDeleted;
                    }
                    catch
                    { 
                        // TBD : log these errors
                    }
                }
            }
            return NumDeleted;
        }
        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.Id == id);
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

        public JsonResult OnGetDeleteEvent()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
            if (NVPairs.TryGetValue("eventId", out int eventId))
            {
                _Event = _context.Event.Find(eventId);

                if (_Event != null)
                {
                    try
                    {
                        // Delete Event
                        _context.Event.Remove(_Event);
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~: Exception : Delete Event Failed.");
                    }
                    return new JsonResult("SUCCESS~:Delete Event Succeeded.");
                }
            }
            return new JsonResult("ERROR~: Delete Event Failed.");
        }
    }
}
