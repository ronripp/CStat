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
            int curID = -1;

            for (int i = -2; i < 367; ++i)
            {
                day = today.AddDays(i);
                var eod = day.AddMinutes((23 * 60) + 59);
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

                var matches = Events.Where(e => (eod >= e.StartTime) && (day <= e.EndTime)).ToList();

                dayStr = "<tr class=\"rowAttrs\"" + trStyle + " " + dateStr + ">" +
                            "<td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\" id=\"DateField\">" + dowStr[(int)day.DayOfWeek] + "</td><td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\" id=\"DateField\"><b>" + curMon + " " + day.Day.ToString() + "</b></td>" +
                            "<td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\"><div class=\"row rowAttrs\">";

                switch (matches.Count)
                {
                    case 0:
                        dayStr += "<div class=\"rowAttrs col-md-12\"><a href=\"Create?" + dateStr + "\">Add Event</a></div>";
                        curID = -1;
                        break;
                    case 1:
                        if (day.Date == matches[0].StartTime.Date)
                        {
                            dayStr += "<div class=\"rowAttrs col-md-2\"><a href=\"Create?" + dateStr + "\">Add Event</a></div>";
                            dayStr += "<div class=\"rowAttrs col-md-10 EventDesc\"><a href=\"Edit?id=" + matches[0].Id + "\">" + matches[0].Description + " (st. " + matches[0].StartTime.ToString("h:mm tt") + ")</a></div>";
                        }
                        else if (day.Date == matches[0].EndTime.Date)
                        {
                            dayStr += "<div class=\"rowAttrs col-md-10 EventDesc\"><a href=\"Edit?id=" + matches[0].Id + "\">" + matches[0].Description + " (ends " + matches[0].EndTime.ToString("h:mm tt") + ")</a></div>";
                            dayStr += "<div class=\"rowAttrs col-md-2\"><a href=\"Create?" + dateStr + "\">Add Event</a></div>";
                        }
                        else
                        {
                            dayStr += "<div class=\"rowAttrs col-md-12 EventDesc\"><a href=\"Edit?id=" + matches[0].Id + "\">" + matches[0].Description + "</a></div>";
                        }
                        curID = matches[0].Id;
                        break;
                    default:
                    case 2:
                        curID = matches[0].Id + (100000 * matches[0].Id);
                        break;
                }
                dayStr += "</div></td></tr>";
                Calendar.Add(dayStr);
            }
        }
    }
}
