using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Events
{
    public class IndexModel : PageModel
    {
        public readonly string[] dowStr = {"Sun", "Mon", "Tues", "Wed", "Thur", "Fri", "Sat"};
        public readonly string[] monStr = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};

        private readonly CStat.Models.CStatContext _context;
        public List<string> Calendar;
        public IList<Event> Events { get; set; }

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
            Calendar = new List<string>();
        }


        public async System.Threading.Tasks.Task OnGetAsync()
        {
            var sDT = DateTime.Now.AddDays(-2);
            var eDT = DateTime.Now.AddDays(366);
            Events = await _context.Event
                .Include(e => e.Church).Where(e => (e.EndTime >= sDT) && (e.StartTime <= eDT)).ToListAsync();

            DateTime day;
            DateTime now = DateTime.Now;
            DateTime today = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, 0);
            string dayStr;
            int monthCnt = 0;
            string curMon, lastMon = "";

            for (int i = -2; i < 367; ++i)
            {
                day = today.AddDays(i);
                curMon = monStr[day.Month - 1];
                var dateStr = "edate=" + day.Month.ToString("D2") + "/" + day.Day.ToString("D2") + "/" + day.Year.ToString("D4");
                string trStyle = "";
                if (curMon != lastMon)
                {
                    ++monthCnt;
                    lastMon = curMon;
                    trStyle = "border-top:2px solid #000000;";
                }
                
                if ((monthCnt%2) != 0)
                {
                    trStyle += "background-color: #FFFDF0";
                }

                if (trStyle.Length > 0)
                {
                    trStyle = "style=\"" + trStyle + "\"";
                }

                var matches = Events.Where(e => (day >= e.StartTime) && (day <= e.EndTime)).ToList();


                dayStr = "<tr " + trStyle + " " + dateStr + ">" +
                            "<td id=\"DateField\">" + dowStr[(int)day.DayOfWeek] + "</td><td  id=\"DateField\"><b>" + curMon + " " + day.Day.ToString() + "</b></td>";

                switch (matches.Count)
                {
                    case 0:
                        dayStr += "<td width=\"90%\" eid =\"-1\"><a href=\"Create?" + dateStr + "\">Add Event</a></td>";
                        dayStr += "<td width=\"10%\" eid =\"-1\">Add</td>";
                        break;
                    case 1:
                        dayStr += "<td width=\"100%\" eid =\"1\">Event 1</td>";
                        break;
                    default:
                    case 2:
                        dayStr += "<td width=\"100%\" eid =\"1\">Event 2</td>";
                        break;
                }
                dayStr += "</tr>";
                Calendar.Add(dayStr);
            }
        }
    }
}
