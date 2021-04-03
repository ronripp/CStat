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
using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;

namespace CStat
{
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _config;

        [BindProperty]
        public CSSettings Settings { get; set; }
        [BindProperty]
        public CSUser UserSettings { get; set; }
        private UserManager<CStatUser> _userManager;

        public SettingsModel(IConfiguration config, IHttpContextAccessor httpContextAccessor, UserManager<CStatUser> userManager)
        {
            _config = config;
            _userManager = userManager; 
            Settings = CSSettings.GetCSSettings(_config, userManager);
            string UserId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Subject.Name;
            UserSettings = Settings.GetUser(UserId);
            if (UserSettings == null)
            {
                UserSettings = new CSUser();
                UserSettings.Name = UserId;
                UserSettings.ShowAllTasks = false;
                UserSettings.SendEquipText = false;
                UserSettings.SendStockText = false;
                UserSettings.SendTaskText = false;
                UserSettings.SendEMailToo = false;
                Settings.UserSettings.Add(UserSettings);
            }
        }
        public IActionResult OnGet()
        {
            return Page();
        }

        public IActionResult OnPost()
        {
            CSSettings ModSettings = CSSettings.GetCSSettings(_config, _userManager);
            ModSettings.SetUser(UserSettings.Name, UserSettings);
            ModSettings.EquipProps = Settings.EquipProps;
            ModSettings.ActiveEquip = ModSettings.EquipProps.Where(e => e.Active).ToList();
            ModSettings.Save();
            return RedirectToPage("./Index");
        }
    }
}
