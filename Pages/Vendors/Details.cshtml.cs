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
    public class DetailsModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DetailsModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

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
    }
}
