using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using CStat.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace CStat.Pages.Tasks
{
    public class DeleteModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnv;

        public DeleteModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _hostEnv = hostEnv;
        }

        [BindProperty]
        public CTask Task { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Task = await _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.Person).FirstOrDefaultAsync(m => m.Id == id);

            if (Task == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            //var sms = new CSSMS(_hostEnv, _configuration);
            //sms.SendMessage("12037700732", "Hello there Test2");

            if (id == null)
            {
                return NotFound();
            }

            Task = await _context.Task.FindAsync(id);

            if (Task != null)
            {
                _context.Task.Remove(Task);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
