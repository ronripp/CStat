using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Attend
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Attendance Attendance { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Attendance = await _context.Attendance
                .Include(a => a.Event)
                .Include(a => a.Medical)
                .Include(a => a.Person)
                .Include(a => a.Registration)
                .Include(a => a.Transaction).FirstOrDefaultAsync(m => m.Id == id);

            if (Attendance == null)
            {
                return NotFound();
            }
           ViewData["EventId"] = new SelectList(_context.Event, "Id", "Id");
           ViewData["MedicalId"] = new SelectList(_context.Medical, "Id", "FormLink");
           ViewData["PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["RegistrationId"] = new SelectList(_context.Registration, "Id", "FormLink");
           ViewData["TransactionId"] = new SelectList(_context.Transaction, "Id", "Id");
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

            _context.Attach(Attendance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(Attendance.Id))
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

        private bool AttendanceExists(int id)
        {
            return _context.Attendance.Any(e => e.Id == id);
        }
    }
}
