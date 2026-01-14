using CStat.Data;
using Dropbox.Api.Files;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class MeetingMins
    {
        public MeetingMins(string fullPath, MMType type, DateTime dt, string doc)
        {
            _fullPath = fullPath;
            _type = type;
            _dt = dt;
            _docs = new List<string>();
            _docs.Add(doc);
        }

        public enum MMType { Annual, EC, Special, Letter};
        public enum MMMonth {JANUARY=1, FEBRUARY, MARCH, APRIL, MAY, JUNE, JULY, AUGUST, SEPTEMBER, OCTOBER, NOVEMBER, DECEMBER};

        public MMType _type;
        public DateTime _dt;
        public string _fullPath;
        public List<string> _docs;

        public string GetLatest()
        {
            if (_docs.Count() == 0) return "";
            if (_docs.Count() == 1) return _docs[0];
            return "TBD";
        }

        public static MMType GetType(string minFile)
        {
            String[] rwords = minFile.Split(".");
            string word = String.Join(" ", rwords, 0, (rwords.Length > 1) ? rwords.Length - 1 : rwords.Length);
            char[] delimChars = new char[] {' ', ',', '-', '_'};
            string[] words = word.ToUpper().Split(delimChars);

            if (words.Any(w => w.StartsWith("SPECIAL")))
                return MMType.Special;

            if (words.Any(w => w.StartsWith("LETTER")))
                return MMType.Letter;

            //bool isMeeting = words.Any(w => w.StartsWith("MEETING") || w.StartsWith("MINUTE") || w.StartsWith("MM"));
            bool hasVotes = words.Any(w => w.StartsWith("VOTE"));
            bool isAnnual = words.Any(w => w.StartsWith("ANNUAL") || w.StartsWith("BALANCE") || w.StartsWith("LOSS") || w.StartsWith("TREASUR") || w.StartsWith("FINAC") || w.StartsWith("SHEET") || hasVotes);
            //bool isEC = words.Any(w => w.StartsWith("EC") || w.StartsWith("ZOOM") || w.StartsWith("MM"));
            bool isNotAnnual = words.Any(w => w.StartsWith("EC") || w.StartsWith("PRE")) && !hasVotes;

            return (isAnnual && !isNotAnnual) ? MMType.Annual : MMType.EC;
        }

        public static int FullYear(int raw)
        {
            if (raw < 100)
                raw += (raw >= 60) ? 1900 : 2000;
            return raw;
        }

        public static DateTime GetDT(string minFile, MMType type)
        {
            String[] rwords = minFile.Split(".");
            string word = String.Join(" ", rwords, 0, (rwords.Length > 1) ? rwords.Length - 1 : rwords.Length);
            char[] delimChars = new char[] { ' ', ',', '-', '_' };
            string[] words = word.ToUpper().Split(delimChars);

            List<int> dateNums = new List<int>();
            foreach (var w in words)
            {
                if (int.TryParse(w, System.Globalization.NumberStyles.None, new CultureInfo("en-US"), out int num))
                {
                    dateNums.Add(num);
                }
                else
                {
                    if (MMMonth.TryParse(w, out MMMonth month))
                    {
                        dateNums.Add((int)month);
                    }
                }
            }

            if (type == MMType.Letter)
            {
                int month;
                int day;

                bool isAnnual = words.Any(w => w.StartsWith("ANNUAL") || w.StartsWith("BALANCE") || w.StartsWith("LOSS") || w.StartsWith("TREASUR") || w.StartsWith("FINAC") || w.StartsWith("SHEET"));
                if (isAnnual)
                {
                    month = 11;
                    day = 7;
                }
                else
                {
                    month = 12;
                    day = 15;
                }

                // There are only 2 values for Annual -> Years return smaller of 2
                if (dateNums.Count == 1)
                {
                    return new DateTime(MeetingMins.FullYear(dateNums[0]), month, day);
                }
                if ((dateNums.Count == 2) && (dateNums[0] <= 12))
                {
                    return new DateTime(MeetingMins.FullYear(dateNums[1]), dateNums[0], 15);
                }
            }

            if (type == MMType.Annual)
            {
                int year;
                int month = 11;
                int day = 7;
                // There are only 2 values for Annual -> Years return smaller of 2
                if (dateNums.Count == 1)
                {
                    return new DateTime(MeetingMins.FullYear(dateNums[0]), month, day);
                }
                else if (dateNums.Count == 2)
                {
                    dateNums[0] = MeetingMins.FullYear(dateNums[0]);
                    dateNums[1] = MeetingMins.FullYear(dateNums[1]);
                    year = (dateNums[0] <= dateNums[1]) ? dateNums[0] : dateNums[1];
                    return new DateTime(year, month, day);
                }
            }

            if (dateNums.Count == 3)
            {
                return ((dateNums[0] > 12) && (dateNums[1] <= 12)) 
                    ? new DateTime(MeetingMins.FullYear(dateNums[0]), dateNums[1], dateNums[2])  // YYYY MM DD
                    : new DateTime(MeetingMins.FullYear(dateNums[2]), dateNums[0], dateNums[1]); // MM DD YYYY
            }

            return (dateNums.Count == 1) ? new DateTime(MeetingMins.FullYear(dateNums[0]), 11, 7) : new DateTime(2000, 11, 7); // unknown
        }
    }   

    public class MeetingMinsMgr
    {
        public List<MeetingMins> _List;

        public MeetingMinsMgr()
        {
            _List = new List<MeetingMins>();
        }

        public int ReadMins()
        {
            _List.Clear();
            var dbox = new CSDropBox(Startup.CSConfig);
            var destPath = "/Corporate/Meeting Minutes";
            ListFolderResult FldList = dbox.GetFolderList2(destPath);

            if (FldList.Entries.Count > 0)
            {
                foreach (var entry in FldList.Entries)
                {
                    if (!entry.IsFolder)
                    {
                        MeetingMins.MMType mmType = MeetingMins.GetType(entry.Name);
                        DateTime dt = MeetingMins.GetDT(entry.Name, mmType);

                        MeetingMins found = _List.FirstOrDefault(m => (m._type == mmType) && (m._dt == dt));
                        if (found != null)
                            found._docs.Add(entry.Name);
                        else
                            _List.Add(new MeetingMins(entry.PathDisplay, mmType, dt, entry.Name));               
                    }
                }
            }

            return _List.Count();
        }
    }
}

/****************************

10-19-23 Zoom Meeting Agenda.docx
2009 EC Votes.pdf
2010-10-01.pdf
2010-11-20 Annual.pdf
2011-11-19 Annual.pdf
2011-12-01.pdf
2012-04-14.pdf
2012-05-12.pdf
2012-09-18.pdf
2012-10-15.pdf
2012-11-15.pdf
2013-01-12 Annual Meeting Attendance & Information Sheet.pdf
2013-01-12 Annual.pdf
2013-07-13.pdf
2013-10-19.pdf
2013-11-09 Annual Meeting Attendance & Information Sheet.pdf
2013-11-09 Annual.pdf
2014-03-08.pdf
2014-11-01.pdf
2014-11-22 Annual Meeting Attendance & Information Sheet.pdf
2014-11-22 Annual.pdf
2015-03-07.pdf
2015-11-07.pdf
2015-11-14 Annual.pdf
2016-05-23.pdf
2016-10-22.pdf
2016-11-12 Annual.pdf
2017-02-11.pdf
2018 annual meeting letter.pdf
2018-02-13.pdf
2018-03-03.pdf
2018-08-28.pdf
2018-10-13.pdf
2018-11-3 Annual.PDF
2018-12-15.pdf
2019-02-09.pdf
2019-03-16.pdf
2019-05-18.pdf
2019-10-05.pdf
2019-11-16 Annual.PDF
2019-11-16 Pre Annual.pdf
2019-11-16_Annual_MM.pdf
2019-11-16_Annual_MM_Rev_B.pdf
2019-11-16_EC_MM.pdf
2019-11-16_EC_MM_Rev_B.pdf
2020 AGENDA FOR 2020 Annual.pdf
2020-01-25.pdf
2020-02-22.pdf
2020-02-22_EC_MM.pdf
2020-02-22_EC_MM_Rev_B.pdf
2020-02-22_EC_MM_Rev_C.pdf
2020-04-14.pdf
2020-05-14.pdf
2020-07-30.pdf
2020-10-10.pdf
2020-11-07 Annual.pdf
2020-11-07_Annual_MM.pdf
2020-11-07_Annual_MM_Rev_B.pdf
2021-03-18.pdf
2021-06.pdf
2021-10-14.pdf
2021-11-13 Annual.pdf
2021-11-13_Annual_MM.pdf
2021-11-13_Annual_MM_Rev_B.pdf
2022-01-08_EC_MM.pdf
2022-01-08_EC_MM_Rev_B.pdf
2022-02-19_EC_MM.pdf
2022-02-19_EC_MM_Rev_B.pdf
2022-02-19_EC_MM_Rev_C.pdf
2022-03-12_Special_MM.pdf
2022-03-12_Special_MM_Rev_B.pdf
2022-03-24_EC_MM.pdf
2022-03-24_EC_MM_Rev_B.pdf
2022-04-16_EC_MM.pdf
2022-04-16_EC_MM_Rev_B.pdf
2022-05-17_EC_MM.pdf
2022-06-27_EC_MM.pdf
2022-09-15_EC_MM.pdf
2022-09-15_EC_MM_Rev_B.pdf
2022-10-27_EC_MM.pdf
2022-10-27_EC_MM_Rev_B.pdf
april agenda.docx

CCA 10-10-24 Zoom Minutes- final.docx
CCA 10-20-23 Zoom Meeting.docx
CCA 10-20-23 Zoom Meeting_Rev_B.docx
CCA 11-2-2024 Annual Meeting Minutes.docx
CCA 2023-24 Annual Meeting Minutes.docx
CCA 2024-2025 Annual Meeting Agenda.pdf
CCA 2024-2025 Profit and Loss Statement.pdf
cca 3-13-23 minutes.docx
CCA 4-1-24 Zoom Minutes.docx
CCA 4-1-24 Zoom Minutes_Rev_B.docx
CCA 4-29-24 Zoom Meeting final.docx
CCA 4-3-23 Zoom minutes final.docx
CCA 6-1-23 zoom minutes.docx
CCA 6-3-24 Zoom Minutes-final.docx
CCA 7-1-24 Zoom Meeting- final.docx
CCA 7-10-23 Zoom minutes.docx
CCA 9-30-23 Zoom minutes final.docx
CCA 9-9-24 Zoom Minutes- final.docx
CCA Annual Membership Form 24-25.docx
CCA Annual Membership Form 24-25.pdf
CCA Zoom Meeting 12-1-24 Outlook Direct.docx
CCA Zoom Meeting 12-2-24 GMail Forward.docx
CCA Zoom Meeting 12-3-24 GMail Direct.docx
CCA Zoom Meeting 12-4-24 Outlook Forward.docx
christmas letter 23.docx
December 5 2022 Zoom Minutes.docx
December 5 2022 Zoom Minutes_Rev_B.docx
February 13 2023 Zoom Meeting Minutes.docx
 
****************************/