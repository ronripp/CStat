using System;

namespace CCAChurch
{
    //"Canarse COC"
    //"East Nothport CC"
    //"Greenpoint CC"
    //"Reclaim CC"
    //"Holbrook CC"
    //"Maspeth CC"
    //"Nesconset CC"
    //"Newtown CC"
    //"North Shore CC"
    //"South Nassau CC"
    //"South Shore CC"
    //"Taconic CC"
    //"Woodland COC"
    public enum MemberStatType
    {
        UNK_M = 0x0,
        MIGS, // Member in Good Standing
        PRIOR_M,
        EXP_M,
        SUSP_M,
        NOT_M,
        NumMemberStats
    }
    public enum AffiliationType
    {
        ICCOC = 0,
        RC,
        LUTH,
        ANG,
        EPIS,
        SOBAP,
        METH,
        AMBAP,
        IFBAP,
        AOG,
        PRESB,
        NICOC,
        CONG,
        NOND,
        OTHER,
        UNK,
        NumAffiliations
    }

    public class ChurchData
    {
        public int id { get; set; }
        public String Name { get; set; }
        public String Affiliation { get; set; }
        public String Status { get; set; }

        public int MembershipStatus { get; set; }
        public String Street { get; set; }
        public String City { get; set; }
        public String State { get; set; }
        public String Zip { get; set; }
        public String Phone { get; set; }
        public String Minister_id { get; set; }
        public String YouthPastor_id { get; set; }
        public String Trustee1_id { get; set; }
        public String Trustee2_id { get; set; }
        public String Trustee3_id { get; set; }
        public String Alt1_id { get; set; }
        public String Alt2_id { get; set; }
        public String Alt3_id { get; set; }
    }
    public class Raw
    {
        public string type { get; set; }
        public string data { get; set; }
    }
    public class ComboboxItem
    {
        public string Text { get; set; }
        public int CIndex { get; set; }
        public String CStr { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
    public class ChurchAtt
    {
        public ChurchAtt(String n, AffiliationType t) { name = n; aff_type = t; bAffiliation = true; }
        public ChurchAtt(String n, MemberStatType t) { name = n; ms_type = t; bAffiliation = false; }
        public String name { get; set; }
        public AffiliationType aff_type { get; set; }
        public MemberStatType ms_type { get; set; }
        private bool bAffiliation;
        public int GetEnumType()
        {
            if (bAffiliation)
                return (int)aff_type;
            else
                return (int)ms_type;
        }
        public override string ToString()
        {
            return name;
        }
        public string ToAbbrString()
        {
            if (bAffiliation)
                return aff_type.ToString();
            else
                return ms_type.ToString();
        }

    }
    public class ChurchLists
    {
        public static ChurchAtt[] AffiliationList = new ChurchAtt[]
        {
        new ChurchAtt("Indep. Christian / COC",   AffiliationType.ICCOC),
        new ChurchAtt("Roman Catholic",       AffiliationType.RC),
        new ChurchAtt("Lutheran",             AffiliationType.LUTH),
        new ChurchAtt("Anglican",             AffiliationType.ANG),
        new ChurchAtt("Episcopal",            AffiliationType.EPIS),
        new ChurchAtt("Southern Baptist",     AffiliationType.SOBAP),
        new ChurchAtt("Methodist",            AffiliationType.METH),
        new ChurchAtt("American Baptist",     AffiliationType.AMBAP),
        new ChurchAtt("Indep. Fund. Baptist",     AffiliationType.IFBAP),
        new ChurchAtt("Assembly of God",      AffiliationType.AOG),
        new ChurchAtt("Presbyterian",         AffiliationType.PRESB),
        new ChurchAtt("Non Instrumental COC", AffiliationType.NICOC),
        new ChurchAtt("Congregational UCOC",    AffiliationType.CONG),
        new ChurchAtt("Non Denominational",   AffiliationType.NOND),
        new ChurchAtt("Other",                AffiliationType.OTHER),
        new ChurchAtt("Unknown",              AffiliationType.UNK)
        };

        public static ChurchAtt[] MemberStatList = new ChurchAtt[]
        {
        new ChurchAtt("Membership Unknown", MemberStatType.UNK_M),
        new ChurchAtt("Mem. Good Standing", MemberStatType.MIGS),
        new ChurchAtt("Prior Member",       MemberStatType.PRIOR_M),
        new ChurchAtt("Expelled Member",    MemberStatType.EXP_M),
        new ChurchAtt("Suspended Member",   MemberStatType.SUSP_M),
        new ChurchAtt("Not a Member",       MemberStatType.NOT_M)
        };
    };
    public class PInfo
    {
        public PInfo()
        {
            id = -1;
            FirstName = null;
            LastName = null;
            SSNum = null;
            ZipCode = null;
            Name = null;
        }

        public int id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String SSNum { get; set; }
        public String ZipCode { get; set; }
        public String Name { get; set; }

        public String Hint()
        {
            // Church Abbr + Zip + L4(SSNum)
            return "";
        }
    }
}
