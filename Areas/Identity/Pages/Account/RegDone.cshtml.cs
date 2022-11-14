using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CStat.Areas.Identity.Pages.Account
{
    public class RegDoneModel : PageModel
    {
        [BindProperty(Name = "Msg", SupportsGet = true)]
        public string Msg { get; set; }

        public void OnGet()
        {
        }
    }
}
