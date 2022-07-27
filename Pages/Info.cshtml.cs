using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using CStat.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;

namespace CStat
{
    public class InfoModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly CSUser _curUser;

        public InfoModel(CStat.Models.CStatContext context, IConfiguration config, IHttpContextAccessor httpCA, UserManager<CStatUser> userManager)
        {
            _context = context;
            _curUser = CCommon.GetCurUser(context, config, httpCA, userManager);
            IsFull = false; // (_curUser != null) ? _curUser.IsFull : false;
        }

        public IActionResult OnGet()
        {
        ViewData["EventId"] = new SelectList(_context.Event, "Id", "Id");
        ViewData["MedicalId"] = new SelectList(_context.Medical, "Id", "FormLink");
        ViewData["PersonId"] = new SelectList(_context.Person, "Id", "FirstName");
        ViewData["RegistrationId"] = new SelectList(_context.Registration, "Id", "FormLink");
        ViewData["TransactionId"] = new SelectList(_context.Transaction, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public Attendance Attendance { get; set; }

        [BindProperty]
        public bool IsFull { get; set; }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attendance.Add(Attendance);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
