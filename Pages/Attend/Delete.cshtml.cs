using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Attend
{
    public class DeleteModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DeleteModel(CStat.Models.CStatContext context)
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Attendance = await _context.Attendance.FindAsync(id);

            if (Attendance != null)
            {
                _context.Attendance.Remove(Attendance);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
