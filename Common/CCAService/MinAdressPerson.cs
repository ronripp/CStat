using System;

//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************

namespace MinAddressPerson
{
    public class MinAddress
    {
        public int id { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Country { get; set; }
        public string WebSite { get; set; }
    }

    public class MinPerson
    {
        public MinPerson()
        {
            Address = new MinAddress();
        }
        public int id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Alias { get; set; }
        public Nullable<System.DateTime> DOB { get; set; }
        public Nullable<byte> Gender { get; set; }
        public Nullable<long> Status { get; set; }
        public string SSNum { get; set; }
        public Nullable<int> Address_id { get; set; }
        public Nullable<int> PG1_Person_id { get; set; }
        public Nullable<int> PG2_Person_id { get; set; }
        public Nullable<int> Church_id { get; set; }
        public long SkillSets { get; set; }
        public long Roles { get; set; } = 0;
        public string CellPhone { get; set; }
        public string EMail { get; set; }
        public string ContactPref { get; set; }
        public string Notes { get; set; }
        public MinAddress Address;
    }
}
//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************