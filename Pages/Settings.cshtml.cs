using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CStat.Models;
using CStat.Common;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CStat
{
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly CSSettings _settings;
        private string userId = "";
        public SettingsModel(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _settings = new CSSettings(_config);
            userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
        }
        public IActionResult OnGet()
        {
            ViewData["user"] = userId;
            CSUser uset = _settings.GetUser(userId);
            ViewData["ShowAllTasks"] = (uset != null) ? uset.ShowAllTasks : false;
            ViewData["SendEquipEMail"] = (uset != null) ? uset.SendEquipEMail : false;
            ViewData["EquipProps"] = _settings.EquipProps;
            return Page();
        }
    }
}
