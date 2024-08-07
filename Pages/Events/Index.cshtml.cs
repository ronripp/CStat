﻿using System;
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
        public readonly string[] ECol =  { "#ffb3ba", "#ffdfba", "#baffc9", "#bae1ff", "#E0BBE4", "#9CECFF", "#94FA92"};
        public readonly string[] EBord = { "#b30000", "#e62e00", "#008000", "#0000ff", "#b30000", "#0000ff", "#008000" };

        public List<KeyValuePair<int, string>> ECList;
        public int ECIndex = 0;

        private readonly CStat.Models.CStatContext _context;
        public List<string> Calendar;
        public IList<Event> Events { get; set; }

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
            Calendar = new List<string>();
            ECList = new List<KeyValuePair<int, string>>();
        }
        private string GetEC (int eid)
        {
            var kvpRes = ECList.Where(kvp => kvp.Key == eid).ToArray();
            if ((kvpRes.Count() > 0) && (kvpRes[0].Key == eid))
                return kvpRes[0].Value;
            KeyValuePair<int, string> newKVP = new KeyValuePair<int, string>(eid, "EVCol"+ECIndex.ToString());
            ECIndex = (ECIndex + 1) % ECol.Length;
            ECList.Add(newKVP);
            return newKVP.Value;
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
                            "<td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\" id=\"DateField\">" + dowStr[(int)day.DayOfWeek] + "</td><td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\" id=\"DateField\"><a href=\"/Events/Create?" + dateStr + "\"><b>" + curMon + " " + day.Day.ToString() + "</b></a></td>" +
                            "<td style=\"padding: 2px 2px 2px 2px;\" class=\"rowAttrs\"><div class=\"row rowAttrs\">";

                switch (matches.Count)
                {
                    case 0:
                        dayStr += "<div class=\"rowAttrs col-sm-12 AddDesc\"><a href=\"/Events/Create?" + dateStr + "\"><i>Add Event</i></a></div>";
                        curID = -1;
                        break;
                    case 1:
                        if ((day.Date == matches[0].StartTime.Date) && (day.Date == matches[0].EndTime.Date))
                        {
                            dayStr += "<div class=\"rowAttrs col-sm-2 AddDesc\"><a href=\"/Events/Create?" + dateStr + "\"><i>Add Event</i></a></div>";
                            dayStr += "<div class=\"rowAttrs col-sm-10 EventDesc " + GetEC(matches[0].Id) + "\"><a href=\"/Events/Edit?id=" + matches[0].Id + "\">" + matches[0].Description + " (" + matches[0].StartTime.ToString("h:mm tt") + " - " + matches[0].EndTime.ToString("h: mm tt") + ")</a></div>";
                        }
                        else if (day.Date == matches[0].StartTime.Date)
                        {
                            dayStr += "<div class=\"rowAttrs col-sm-2 AddDesc\"><a href=\"/Events/Create?" + dateStr + "\"><i>Add Event</i></a></div>";
                            dayStr += "<div class=\"rowAttrs col-sm-10 EventDesc " + GetEC(matches[0].Id) + "\"><a href=\"/Events/Edit?id=" + matches[0].Id + "\">" + matches[0].Description + " (st. " + matches[0].StartTime.ToString("h:mm tt") + ")</a></div>";
                        }
                        else if (day.Date == matches[0].EndTime.Date)
                        {
                            dayStr += "<div class=\"rowAttrs col-sm-10 EventDesc " + GetEC(matches[0].Id) + "\"><a href=\"/Events/Edit?id=" + matches[0].Id + "\">" + matches[0].Description + " (ends " + matches[0].EndTime.ToString("h:mm tt") + ")</a></div>";
                            dayStr += "<div class=\"rowAttrs col-sm-2 AddDesc\"><a href=\"/Events/Create?" + dateStr + "\"><i>Add Event</i></a></div>";
                        }
                        else
                        {
                            dayStr += "<div class=\"rowAttrs col-sm-12 EventDesc " + GetEC(matches[0].Id) + "\"><a href=\"/Events/Edit?id=" + matches[0].Id + "\">" + matches[0].Description + "</a></div>";
                        }
                        curID = matches[0].Id;
                        break;
                    default:
                    case 2:
                        int sidx=0, eidx=1;
                        if (matches[0].StartTime > matches[1].StartTime)
                        {
                            sidx = 1;
                            eidx = 0;
                        }

                        String sidxExtra = "";
                        if (day.Date == matches[sidx].StartTime.Date)
                        {
                            if (day.Date == matches[sidx].EndTime.Date)
                                sidxExtra = " (" + matches[sidx].StartTime.ToString("h: mm tt") + " - " + matches[sidx].EndTime.ToString("h: mm tt") + ")";
                            else
                                sidxExtra = " (st. " + matches[sidx].StartTime.ToString("h: mm tt") + ")";
                        }
                        else
                        {
                            if (day.Date == matches[sidx].EndTime.Date)
                            {
                                sidxExtra = " (ends " + matches[sidx].EndTime.ToString("h: mm tt") + ")";
                            }
                        }
                        String eidxExtra = "";
                        if (day.Date == matches[eidx].StartTime.Date)
                        {
                            if (day.Date == matches[eidx].EndTime.Date)
                                eidxExtra = " (" + matches[eidx].StartTime.ToString("h: mm tt") + " - " + matches[eidx].EndTime.ToString("h: mm tt") + ")";
                            else
                                eidxExtra = " (st. " + matches[eidx].StartTime.ToString("h: mm tt") + ")";

                        }
                        else
                        {
                            if (day.Date == matches[eidx].EndTime.Date)
                            {
                                eidxExtra = " (ends " + matches[eidx].EndTime.ToString("h: mm tt") + ")";
                            }
                        }

                        if (matches.Count > 2)
                        {
                            sidxExtra = "More than 2 Events. " + sidx;
                            eidxExtra = "More than 2 Events. " + eidx;
                        }

                        dayStr += "<div class=\"rowAttrs col-md-6 EventDesc " + GetEC(matches[sidx].Id) + "\"><a href=\"/Events/Edit?id=" + matches[sidx].Id + "\">" + matches[sidx].Description + sidxExtra + "</a></div>";
                        dayStr += "<div class=\"rowAttrs col-md-6 EventDesc " + GetEC(matches[eidx].Id) + "\"><a href=\"/Events/Edit?id=" + matches[eidx].Id + "\">" + matches[eidx].Description + eidxExtra + "</a></div>";

                        break;
                }
                dayStr += "</div></td></tr>";
                Calendar.Add(dayStr);
            }
        }
    }
}
