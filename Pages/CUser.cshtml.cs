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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CStat.Pages
{
    public class CUserModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private readonly CSSettings _csSettings;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private readonly CSUser _curUser;

        public CUserModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv, IHttpContextAccessor httpContextAccessor, IConfiguration config, UserManager<CStatUser> userManager)
        {
            _context = context;
            _hostEnv = hostEnv;
            _config = config;
            _userManager = userManager;
            _csSettings = CSSettings.GetCSSettings(_config, _userManager);
            string UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
            _curUser = _csSettings.GetUser(UserId);
            _curUser.SetPersonIDByEmail(context);
        }

        public IList<CTask> Task { get;set; }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Task = await _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.Event)
                .Include(t => t.ParentTask)
                .Include(t => t.Person)
                .Include(t => t.Worker1)
                .Include(t => t.Worker2)
                .Include(t => t.Worker3).ToListAsync();
        }
        public ActionResult OnGetInput2()
        {
            // Get Command String
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var cmdIndex = rawQS.IndexOf("&");
            var cmdStr = (cmdIndex != -1) ? rawQS.Substring(cmdIndex + 1, rawQS.Length - (cmdIndex + 1)) : rawQS;

            // Execute Command String
            CmdMgr cmdMgr = new CmdMgr(_context, _csSettings, _hostEnv, _config, _userManager, _curUser, false);
            return this.Content(cmdMgr.ExecuteCmd(cmdStr, true));  // Response 
        }
    }
}
