using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Churches
{
    public class EditModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public EditModel(CStat.Models.CStatContext context)
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
           ViewData["AddressId"] = new SelectList(_context.Address, "Id", "Country");
           ViewData["Alternate1Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Alternate2Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Alternate3Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Elder1Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Elder2Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Elder3Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Elder4Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Elder5Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["SeniorMinisterId"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Trustee1Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Trustee2Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["Trustee3Id"] = new SelectList(_context.Person, "Id", "FirstName");
           ViewData["YouthMinisterId"] = new SelectList(_context.Person, "Id", "FirstName");
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

            _context.Attach(Church).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChurchExists(Church.Id))
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

        private bool ChurchExists(int id)
        {
            return _context.Church.Any(e => e.Id == id);
        }
    }
}
