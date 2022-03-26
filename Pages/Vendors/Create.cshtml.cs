using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;

namespace CStat.Pages.Vendors
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
        ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
        ViewData["PocId"] = new SelectList(_context.Person, "Id", "FirstName");
            return Page();
        }

        [BindProperty]
        public Business Business { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Business.Add(Business);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
