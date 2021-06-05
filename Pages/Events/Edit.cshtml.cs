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

namespace CStat.Pages.Events
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Event Event { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Event = await _context.Event
                .Include(e => e.Church).FirstOrDefaultAsync(m => m.Id == id);

            if (Event == null)
            {
                return NotFound();
            }

            UpdateViewData();

            return Page();
        }
        private void UpdateViewData(string err="")
        {
            IList<SelectListItem> cList = new SelectList(_context.Church, "Id", "Name").OrderBy(c => c.Text).ToList();
            cList.Insert(0, new SelectListItem { Text = "Select if Church Event", Value = "-1" });
            ViewData["Church"] = cList;

            IList<SelectListItem> tList = Enum.GetValues(typeof(Event.EventType)).Cast<Event.EventType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            tList.Insert(0, new SelectListItem { Text = "Select Event Type", Value = "-1" });
            ViewData["Type"] = tList;

            ViewData["EventError"] = err;
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (Event.ContractLink.StartsWith("~~~"))
            {
                UpdateViewData("Browsing for a Contract Link : ~~~");
                return NotFound();
            }

            if ((Event.Description == null) || (Event.Description.Trim().Length == 0) || (Event.Type <= 0))
            {
                UpdateViewData("Event Type and/or Event Description must be set.");
                return Page();
            }

            if (!ModelState.IsValid)
            {
                UpdateViewData("One or more fields are not valid.");
                return Page();
            }

            if (Event.ChurchId == -1)
                Event.ChurchId = null;

            if (Event.StartTime >= Event.EndTime)
            {
                UpdateViewData("End Date/Time must be later than Start Date/Time.");
                return Page();
            }

            // Check if Start or End Dates have changed and delete tasks associated with event.
            RemoveChangedEventTasks(_context, Event);

            _context.Attach(Event).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(Event.Id))
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

        public static int RemoveChangedEventTasks (CStat.Models.CStatContext context, Event newEvent)
        {
            int NumDeleted = 0;
            var oldEvent = context.Event.FirstOrDefaultAsync(e => e.Id == newEvent.Id).Result;
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
                Event = _context.Event.Find(eventId);

                if (Event != null)
                {
                    try
                    {
                        // Delete Event
                        _context.Event.Remove(Event);
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
