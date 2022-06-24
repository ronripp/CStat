//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using CStat.Models;
//using Task = CStat.Models.Task;
//using Microsoft.AspNetCore.Hosting;

using CStat.Areas.Identity.Data;
using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using static CStat.Models.Person;
using static CStat.Models.Task;
using CTask = CStat.Models.Task;

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
        public CTask _Task { get; set; }

        [BindProperty]
        public string _str1 { get; set; } = "";
        [BindProperty]
        public string _str2 { get; set; } = "";
        [BindProperty]
        public string _str3 { get; set; } = "";
        [BindProperty]
        public string _dbl1 { get; set; } = "";
        [BindProperty]
        public string _dbl2 { get; set; } = "";
        [BindProperty]
        public string _dbl3 { get; set; } = "";
        [BindProperty]
        public string _int1 { get; set; } = "";
        [BindProperty]
        public string _int2 { get; set; } = "";
        [BindProperty]
        public string _int3 { get; set; } = "";

        private int NumStr = 0, NumDbl = 0, NumInt = 0;
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
            string Entries = "";
            if (_Task == null)
            {
                return "";
            }
            TaskData tData = TaskData.ReadTaskData(_hostEnv, _Task.Id, _Task.ParentTaskId.HasValue ? _Task.ParentTaskId.Value : -1);

            try
            {
                // !{"Pre":"Enter Free Cl PPM for %DueDate%","Input":"%8.2lf","Range":"0 to 4",Post":" ppm"}!
                // !{"Pre":"Enter Free Cl PPM for %DueDate%","Input":"SELECT,No Usage,0 ppm,0.5 ppm,1 ppm,1.5 ppm,2 ppm,2.5 ppm,3 ppm,3.5 ppm,4 ppm,+4 ppm","Range":"",Post":" ppm"}!
                string details = tData.Detail;
                int sidx, eidx;
                while ((sidx = details.IndexOf("!{")) != -1)
                {
                    // Get Prefix Label
                    sidx += 1;
                    if ((eidx = details.Substring(sidx).IndexOf("}!")) != -1)
                    {
                        string jsInput = details.Substring(sidx, eidx + 1);
                        UserInput ui = Newtonsoft.Json.JsonConvert.DeserializeObject<UserInput>(jsInput);
                        if (ui != null)
                        {
                            double inpWidth = 6;
                            string inpVar = "";
                            int inpDot = ui.Input.IndexOf(".");
                            int inpIdx = -1;
                            if (ui.Input.StartsWith("%"))
                            {
                                do
                                {
                                    inpIdx = ui.Input.IndexOf("lf");
                                    if (inpIdx != -1) { inpVar = "Model._dbl" + (++NumDbl); break; }
                                    inpIdx = ui.Input.IndexOf("f");
                                    if (inpIdx != -1) { inpVar = "Model._dbl" + (++NumDbl); break; }
                                    inpIdx = ui.Input.IndexOf("s");
                                    if (inpIdx != -1) { inpVar = "Model._str" + (++NumStr); break; }
                                    inpIdx = ui.Input.IndexOf("i");
                                    if (inpIdx != -1) { inpVar = "Model._int" + (++NumInt); break; }
                                    inpIdx = ui.Input.IndexOf("i");
                                    if (inpIdx != -1) { inpVar = "Model._int" + (++NumInt); break; }
                                }
                                while (false);
                                if ((NumDbl > 3) || (NumStr > 3) || (NumInt > 3))
                                    continue;

                                if (inpIdx != -1)
                                    inpWidth = int.Parse(ui.Input.Substring(1, ((inpDot != -1) ? inpDot-1 : inpIdx-1)));

                                Entries += "<div class=\"form-group\">\n" +
                                          "<label for=\"" + inpVar + "\" class=\"control-label\">" + ui.Pre + "</label>\n" +
                                          "<input asp-for=\"" + inpVar + "].Pre\" class=\"form-control\" />\n" + ui.Post + "</div>\n";
                            }
                            else if (ui.Input.StartsWith("SELECT,"))
                            {
                                //<label for="cars">Choose a car:</label>
                                //<select name="cars" id="cars" form="carform">
                                //  <option value="volvo">Volvo</option>
                                //  <option value="saab">Saab</option>
                                //  <option value="opel">Opel</option>
                                //  <option value="audi">Audi</option>
                                //</select>
                                string[] options = ui.Input.Substring(7).Split(',');
                                Entries += "<div class=\"form-group\">\n" +
                                          "<label for=\"sel1\" class=\"control-label\">" + ui.Pre + "</label>\n" +
                                          "<select name=\"sel1\" id=\"sel1\">\n";
                                foreach(string s in options)
                                {
                                    Entries += "<option value\"" + s + "\">" + s + "</option>/n"; 
                                }
                                Entries += "</select>\n";
                            }
                        }
                    }
                    details = details.Substring(sidx + 2);
                }
            }
            catch (Exception e)
            {

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

            return Entries;
        }


        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(_Task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(_Task.Id))
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
