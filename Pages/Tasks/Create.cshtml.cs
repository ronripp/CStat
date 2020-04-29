using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using CTask = CStat.Models.Task;

namespace CStat.Pages.Tasks
{
    public class CreateModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        [BindProperty]
        CTask _CTask { get; set; }

        public CreateModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            _CTask = new CTask();
            _CTask.Description = "Fix Miller House Shed";
            _CTask.EstimatedManHours = 24;
            _CTask.Id = 126;
            _CTask.PersonId = 1632;
            _CTask.TotalCost = 562;
            _CTask.EstimatedDoneDate = new DateTime(2020, 8, 15);
            _CTask.CommittedCost = 385;
            _CTask.SetTaskStatus(CTask.eTaskStatus.Paused, CTask.eTaskStatus.Need_Funds, 60);
            _CTask.Priority = 3; 
            //_CTask.
            //_CTask.
            //_CTask.
            //_CTask.
            //_CTask.
            //_CTask.
            //_CTask.
            //_CTask.


        ViewData["Blocking1Id"] = new SelectList(_context.Task, "Id", "Description");
        ViewData["Blocking2Id"] = new SelectList(_context.Task, "Id", "Description");
        ViewData["ChurchId"] = new SelectList(_context.Church, "Id", "Affiliation");
        ViewData["PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
            return Page();
        }

        [BindProperty]
        public CTask Task { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Task.Add(Task);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
