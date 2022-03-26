using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Vendors
{
    public class DeleteModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DeleteModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Business Business { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Business = await _context.Business
                .Include(b => b.Address)
                .Include(b => b.Poc).FirstOrDefaultAsync(m => m.Id == id);

            if (Business == null)
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

            Business = await _context.Business.FindAsync(id);

            if (Business != null)
            {
                _context.Business.Remove(Business);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
