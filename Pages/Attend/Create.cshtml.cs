using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;

namespace CStat.Pages.Attend
{
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public CreateModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["EventId"] = new SelectList(_context.Event, "Id", "Id");
        ViewData["MedicalId"] = new SelectList(_context.Medical, "Id", "FormLink");
        ViewData["PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
        ViewData["RegistrationId"] = new SelectList(_context.Registration, "Id", "FormLink");
        ViewData["TransactionId"] = new SelectList(_context.Transaction, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Attendance Attendance { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attendance.Add(Attendance);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
