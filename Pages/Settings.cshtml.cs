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

namespace CStat
{
    public class SettingsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
