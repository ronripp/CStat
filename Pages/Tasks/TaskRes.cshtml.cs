using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Task = CStat.Models.Task;
using Microsoft.AspNetCore.Hosting;

namespace CStat.Pages.Tasks
{
    public class TaskResModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly IWebHostEnvironment _hostEnv;

        public TaskResModel(CStat.Models.CStatContext context, IWebHostEnvironment hstEnv)
        {
            _context = context;
            _hostEnv = hstEnv;
        }

        [BindProperty]
        public Task _Task { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _Task = await _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.Event)
                .Include(t => t.ParentTask)
                .Include(t => t.Person)
                .Include(t => t.Worker1)
                .Include(t => t.Worker2)
                .Include(t => t.Worker3).FirstOrDefaultAsync(m => m.Id == id);

            if (_Task == null)
            {
                return NotFound();
            }
            return Page();
        }

        public string CreateEntryFields()
        {
            if (_Task == null)
            {
                return "";
            }
            TaskData tData = TaskData.ReadTaskData(_hostEnv, _Task.Id, _Task.ParentTaskId.HasValue ? _Task.ParentTaskId.Value : -1);

            // !Request_Results_When_Done~Enter Free Cl PPM for %DueDate%: %8.2lf ppm~
            string details = tData.Detail;
            int idx;
            while ((idx = details.IndexOf ("!Request_Results_When_Done~")) != -1)
            {
                // TODO 

                idx += 27;
                details = details.Substring(idx);
            }

            // < div class="form-group">
            //     <label asp-for="Task.Description" class="control-label"></label>
            //     <input asp-for="Task.Description" class="form-control" />
            //     <span asp-validation-for="Task.Description" class="text-danger"></span>
            // </div>
            // <div class="form-group">
            //     <label asp-for="Task.Priority" class="control-label"></label>
            //     <input asp-for="Task.Priority" class="form-control" />
            //     <span asp-validation-for="Task.Priority" class="text-danger"></span>
            // </div>

            return "";
        }


        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(Task.Id))
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

        private bool TaskExists(int id)
        {
            return _context.Task.Any(e => e.Id == id);
        }
    }
}
