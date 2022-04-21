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
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<Church> Church { get;set; }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
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
                .Include(c => c.YouthMinister).ToListAsync();
        }
    }
}
