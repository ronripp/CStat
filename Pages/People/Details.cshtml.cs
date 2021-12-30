using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat
{
    public class DetailsModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public DetailsModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public Person Person { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Person = await _context.Person
                .Include(p => p.Address)
                .Include(p => p.Church)
                .Include(p => p.Pg1Person)
                .Include(p => p.Pg2Person).FirstOrDefaultAsync(m => m.Id == id);

            if (Person == null)
            {
                return NotFound();
            }

            //var cse = new Common.CSEMail(Startup.CSConfig);
            //cse.Send("Ron Ripperger", "ronripp3@gmail.com", "This is a CStat test 3 to Ron", "This is a third text body\nThat has 2 lines\nThanks!\nRon Ripperger\n203-770-3393");

            return Page();
        }
    }
}
