using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace CStat.Pages.Tasks
{
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<CTask> Task { get;set; }

        public async Task OnGetAsync()
        {
            Task = await _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.Person).ToListAsync();
        }
    }
}
