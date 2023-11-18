using System;

//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************

namespace CCAEvents
{
    public enum EventType
    {
        Winter_Retreat = 0x101,
        Womans_Retreat,
        Mens_Retreat,
        Church_Retreat,
        Spring_Work_Retreat,
        First_Chance_Retreat,
        Elementary_Week,
        Intermediate_Week,
        Senior_Week,
        VOE_Week,
        Young_Adult_Retreat,
        BWAC_Fall_Retreat,
        Banquet,
        Fund_Raiser,
        Family_Retreat,
        Annual_Meeting,
        SignIn_Sheet,
        Church_Event,
        Private_Event,
        Private_Retreat,
        VOE_Retreat,
        Work_Event,
        Elementary_Retreat,
        Intermediate_Retreat,
        Senior_Retreat,
        Leadership_Retreat,
        SWAT_Training,
        Other
    }

    public class CInfo
    {
        public CInfo()
        {
            id = -1;
            Name = null;
            Affiliation = null;
        }
        public int id { get; set; }
        public String Name { get; set; }
        public String Affiliation { get; set; }
    }

    public class EInfo
    {
        public EInfo()
        {
            id = -1;
            Description = null;
            Type = null;
        }
        public int id { get; set; }
        public String Description { get; set; }
        public String Type { get; set; }
        public String StartDate { get; set; }
    }

    public class EventData
    {
        public int id { get; set; }
        public String Description { get; set; }
        public String Type { get; set; }
        public DateTime StartDT { get; set; }
        public DateTime EndDT { get; set; }
        public decimal costChild { get; set; }
        public decimal costAdult { get; set; }
        public decimal costFamily { get; set; }
        public int churchID { get; set; }
        public decimal costLodge { get; set; }
        public decimal costCabin { get; set; }
        public decimal costTent { get; set; }
        public String contractLink { get; set; }
    }

    public class Raw
    {
        public string type { get; set; }
        public string data { get; set; }
    }

    public class ELine
    {
        public ELine(int i, String ln)
        {
            id = i;
            line = ln;
        }
        public int id { get; set; }
        public string line { get; set; }
        public override string ToString()
        {
            return this.line;
        }
    }
}

//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************
