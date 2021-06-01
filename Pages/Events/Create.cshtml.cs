using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;

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
            IList<SelectListItem> cList = new SelectList(_context.Church, "Id", "Name").OrderBy(c=>c.Text).ToList();
            cList.Insert(0, new SelectListItem { Text = "Select if Church Event", Value = "-1" });
            ViewData["Church"] = cList;

            IList<SelectListItem> tList = Enum.GetValues(typeof(Event.EventType)).Cast<Event.EventType>().Select(x => new SelectListItem { Text = x.ToString().Replace("_", " "), Value = ((int)x).ToString() }).ToList();
            tList.Insert(0, new SelectListItem { Text = "Select Event Type", Value = "-1" });
            ViewData["Type"] = tList;
            return Page();
        }

        [BindProperty]
        public Event Event { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if ((Event.Description == null) || (Event.Description.Trim().Length == 0) || (Event.Type == null) || (Event.Type <= 0))
            {
                return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Event.ChurchId == -1)
                Event.ChurchId = null;

            if (Event.StartTime >= Event.EndTime)
            {
                return Page();
            }

            _context.Event.Add(Event);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
