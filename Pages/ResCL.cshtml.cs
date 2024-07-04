using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CStat.Models;
using CStat.Common;
using static CStat.Models.Event;

namespace CStat.Pages
{
    public class ResCLModel : PageModel
    {
        private string[] months =
        {
           "",
           "January",
           "February",
           "March",
           "April",
           "May",
           "June",
           "July",
           "August",
           "September",
           "October",
           "November",
           "December"
        };

        private readonly CStat.Models.CStatContext _context;
        private int _month, _year, _lastDay;

        public ResCLModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IActionResult OnGet(int month=-1, int year=-1)
        {
            if ((month == -1) || (year == -1))
            {
                var calDT = PropMgr.ESTNow.AddDays(15);
                _month = calDT.Month;
                _year = calDT.Year;
            }
            else
            {
                _month = month;
                _year = year;
            }

            int nmonth, nyear;
            if (_month == 12)
            {
                nmonth = 1;
                nyear = _year + 1;
            }
            else
            {
                nmonth = _month + 1;
                nyear = _year;
            }
            var dt = new DateTime(nyear, nmonth, 1, 12, 0, 0).AddDays(-1);
            _lastDay = dt.Day;

            return Page();
        }

        [BindProperty]
        public Event Event { get; set; }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            return RedirectToPage("./Index");
        }
        public string GetMMYY()
        {
            return months[_month] + " " + _year;
        }

        public string GenerateMMYYForm()
        {
            string fstr = "<form action=\"ResCL\">\n" +
                          "<select class=\"updateForm\" onchange=\"disablePrint()\" name=\"month\" id=\"month\">\n";
            for (int i = 1; i <= 12; ++i)
            {
               fstr += "<option value=\""+ i + "\"" + ((_month == i) ? " selected>\n" : ">") + months[i] + "</option>\n";
            }
            fstr += "</select>\n" +
                    "<select class=\"updateForm\" onchange=\"disablePrint()\" name=\"year\" id=\"year\">\n";

            for (int y = _year-1; y <= _year + 10; ++y)
            {
                fstr += "<option value=\"" + y + "\"" + ((y == _year) ? " selected>\n" : ">") + y + "</option>\n";
            }

            fstr += "</select >\n" +
                    "<input id=\"updateFrm\" class=\"updateForm\" style=\"border-style:outset; border-width:5px;\" disabled=\"disabled\" type=\"submit\" value=\"Update\">\n" +
                    "</form>\n";
            return fstr;
        }
        public string GetDayFields(int i)
        {
            if ((_month == -1) || (_year == -1) || (i > _lastDay))
            {
                return "<tr><td></td><td></td><td></td><td></td><td></td></tr>\n";
            }
            var dt = new DateTime(_year, _month, i, 12, 0, 0);
            List<Event> eList = Event.GetEvents(_context, dt, DateRangeType.On_Date);
            string eStr = (eList.Count > 0) ? "<span style=\"background-color:yellow\">" + ((EventType)(eList[0].Type)).ToString().Replace("_", " ") + " starts</span>" : "";
            var dow = dt.DayOfWeek;
            string dowStr = "";
            switch (dow)
            {
                case DayOfWeek.Sunday:
                    dowStr = "Sun";
                    break;
                case DayOfWeek.Monday:
                    dowStr = "Mon";
                    break;
                case DayOfWeek.Tuesday:
                    dowStr = "Tues";
                    break;
                case DayOfWeek.Wednesday:
                    dowStr = "Wed";
                    break;
                case DayOfWeek.Thursday:
                    dowStr = "Thur";
                    break;
                case DayOfWeek.Friday:
                    dowStr = "Fri";
                    break;
                case DayOfWeek.Saturday:
                    dowStr = "Sat";
                    break;
            }

            return "<tr><td>" + dowStr + " " + _month + "/" + i + "</td><td></td><td></td><td></td><td>" + eStr + "</td></tr>\n";
        }

    }
}
