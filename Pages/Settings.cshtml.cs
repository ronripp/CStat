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

        [BindProperty]
        public CSSettings Settings { get; set; }
        [BindProperty]
        public CSUser UserSettings { get; set; }

        public SettingsModel(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            Settings = new CSSettings(_config);
            string UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
            UserSettings = Settings.GetUser(UserId);
            if (UserSettings == null)
            {
                UserSettings = new CSUser();
                UserSettings.Name = UserId;
                UserSettings.SendEquipEMail = false;
                UserSettings.ShowAllTasks = false;
                Settings.UserSettings.Add(UserSettings);
            }
        }
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            CSSettings ModSettings = new CSSettings(_config);
            ModSettings.SetUser(UserSettings.Name, UserSettings);
            ModSettings.EquipProps = Settings.EquipProps;
            ModSettings.Save();

            return RedirectToPage("./Index");
        }

    }
}