using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class MeetingMins
    {
        public enum MMType { Annual, EC, Special };
        public MMType _type;
        public DateTime _dt;
        public List<string> _docs;

        public string GetLatest()
        {
            if (_docs.Count() == 0) return "";
            if (_docs.Count() == 1) return _docs[0];
            return "TBD";
        }

        public static MMType GetType(string minFile)
        {
            return MMType.Special; // TBD;
        }

        public static DateTime GetDT(string minFile)
        {
            return DateTime.Now; // TBD;
        }
    }   

    public class MeetingMinsMgr
    {
        private readonly IConfiguration _config;
        private List<MeetingMinsMgr> _meetingMins;

        public MeetingMinsMgr(IConfiguration config)
        {
            _config = config;
        }

        public int ReadMins()
        {
            return 0;
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