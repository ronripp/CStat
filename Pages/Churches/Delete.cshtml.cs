using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Churches
{
    public class DeleteModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DeleteModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Church Church { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Church = await _context.Church
                .Include(c => c.Address)
                .Include(c => c.Alternate1)
                .Include(c => c.Alternate2)
                .Include(c => c.Alternate3)
                .Include(c => c.Elder1)
                .Include(c => c.Elder2)
                .Include(c => c.Elder3)
                .Include(c => c.Elder4)
                .Include(c => c.Elder5)
                .Include(c => c.SeniorMinister)
                .Include(c => c.Trustee1)
                .Include(c => c.Trustee2)
                .Include(c => c.Trustee3)
                .Include(c => c.YouthMinister).FirstOrDefaultAsync(m => m.Id == id);

            if (Church == null)
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

            Church = await _context.Church.FindAsync(id);

            if (Church != null)
            {
                _context.Church.Remove(Church);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
