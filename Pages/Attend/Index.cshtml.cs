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
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<Attendance> Attendance { get;set; }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Attendance = await _context.Attendance
                .Include(a => a.Event)
                .Include(a => a.Medical)
                .Include(a => a.Person)
                .Include(a => a.Registration)
                .Include(a => a.Transaction).ToListAsync();
        }
    }
}
