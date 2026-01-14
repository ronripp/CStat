using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CStat.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CStat.Pages
{
    public class MeetMinsModel : PageModel
    {
        [BindProperty]
        public MeetingMinsMgr _mmMgr { get; set; }
        public void OnGet()
        {
            _mmMgr = new MeetingMinsMgr();
            _mmMgr.ReadMins();
        }
    }
}
