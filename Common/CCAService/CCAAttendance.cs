using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using CCAEvents;

//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************

namespace CCAAttendance
{
    public enum AttendanceRoles { Unknown = 0, Visitor, Camper, Staff, Dean, Counselor, SWAT, Missionary, Speaker, Worship, HealthDir, nurse, Manager, Cook, kitchen, Worker, Trustee, ChurchRep, DropOff, PickUp, Other = 101 };
    public class PInfoData
    {
        public List<PInfo> data { get; set; }
    }
    public class Raw
    {
        public string type { get; set; }
        public string data { get; set; }
    }
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
        public String Summary()
        {
            String SumStr = "";
            if ((Name != null) && (Name.Length > 0))
            {
                // Get first letter from each word
                String[] words = Name.Split(' ');
                foreach (string word in words)
                {
                    if (word.Length <= 3)
                        SumStr = SumStr + word.Trim();
                    else
                        SumStr = SumStr + word.Substring(0, 1);
                }
            }
            if ((ZipCode != null) && (ZipCode.Length > 0))
            {
                if (SumStr.Length > 0)
                    SumStr = SumStr + " " + ZipCode;
                else
                    SumStr = ZipCode;
            }

            if ((SSNum != null) && (SSNum.Length >= 4))
            {
                if (SumStr.Length > 0)
                    SumStr = SumStr + " " + SSNum.Substring(SSNum.Length - 4);
                else
                    SumStr = SumStr + " " + SSNum.Substring(SSNum.Length - 4);
            }
            return SumStr;
        }
    }
    public class PFullInfo
    {
        public PFullInfo()
        {
            LFName = "";
            FLName = "";
            FName = "";
            LName = "";
            Alias = "";
            _Gender = "";
            Church = "";
            DOB = "";
            Street = "";
            State = "";
            City = "";
            Zip = "";
            HPhone = "";
            Cell = "";
            SSNum = "";
            EMail = "";
            CMT = "";
            EventType = "";
            Baptized = "";
            Skills = "";
            PGFirst1 = "";
            PGLast1 = "";
            PGFirst2 = "";
            PGLast2 = "";
            PGFirst12 = "";
            PGFirstLast1 = "";
            AttStart = "";
            AttEnd = "";
            AttGrade = "";
            AttSRole = "";
            PGInfo1 = "";
            PGInfo2 = "";
            PGPhone1 = "";
            PGEMail1 = "";
            PGPhone2 = "";
            PGEMail2 = "";
            PGAdr1 = "";
            AdrStatus = "";
        }

        public String LFName
        {
            set
            {
                if (value.Contains(','))
                {
                    String[] names = value.Split(',');
                    if (names.Count() > 0)
                    {
                        LName = names[0].Trim();
                        if (names.Count() > 1)
                            FName = names[1].Trim();
                    }
                }
                else
                {
                    if (value.Contains(' '))
                    {
                        String[] nms = value.Split(' ');
                        int count = nms.Count();
                        if (count > 0)
                        {
                            FName = nms[0].Trim();
                            if (count > 1)
                                LName = nms[FindLNIndex(nms)].Trim();
                        }
                    }
                    //FName = value ZZZ;
                    //LName = value ZZZ;
                }
            }
        }
        public String FLName { get; set; }
        public String FName { get; set; }
        public String LName { get; set; }
        public String Alias { get; set; }
        private String _Gender;
        public String Gender
        {
            get { return _Gender; }
            set
            {
                string gstr = value.Trim().ToUpper();
                if (gstr.StartsWith("M") || gstr.Contains("BOY"))
                    _Gender = "M";
                else if (gstr.StartsWith("F") || gstr.Contains("GIRL"))
                    _Gender = "F";
            }
        }

        public String Church { get; set; }
        public String DOB { get; set; }
        public String Street { get; set; }
        public String State { get; set; }
        public String City { get; set; }
        public String Zip { get; set; }
        public String HPhone { get; set; }
        public String Cell { get; set; }
        public String SSNum { get; set; }
        public String EMail { get; set; }
        public String PGInfo1 { get; set; }
        public String PGInfo2 { get; set; }
        public String CMT { get; set; }
        public String EventType
        {
            get
            {
                return _EventType;
            }
            set
            {
                String uvalue = value.ToUpper().Trim();
                if (uvalue.Contains("BWAC") || uvalue.StartsWith("FALL"))
                {
                    _EventType = CCAEvents.EventType.BWAC_Fall_Retreat.ToString();
                }
                else if (uvalue.StartsWith("MEN"))
                {
                    _EventType = CCAEvents.EventType.Mens_Retreat.ToString();
                }
                else if (uvalue.StartsWith("WINT"))
                {
                    _EventType = CCAEvents.EventType.Winter_Retreat.ToString();
                }
                else if (uvalue.StartsWith("WOM"))
                {
                    _EventType = CCAEvents.EventType.Womans_Retreat.ToString();
                }
                else if (uvalue.StartsWith("FAM"))
                {
                    _EventType = CCAEvents.EventType.Family_Retreat.ToString();
                }
                else if (uvalue.StartsWith("Y"))
                {
                    _EventType = CCAEvents.EventType.Young_Adult_Retreat.ToString();
                }
                else if (uvalue.StartsWith("V"))
                {
                    _EventType = CCAEvents.EventType.VOE_Week.ToString();
                }
                else if (uvalue.StartsWith("SE") || value.StartsWith("SN"))
                {
                    //Make sure split for Elem, Inter and Sen includes Retreat or Week
                    _EventType = uvalue.Contains("RETREAT") ? CCAEvents.EventType.Senior_Retreat.ToString() : CCAEvents.EventType.Senior_Week.ToString();
                }
                else if (uvalue.StartsWith("SP") || value.StartsWith("SR"))
                {
                    _EventType = CCAEvents.EventType.Spring_Work_Retreat.ToString();
                }
                else if (uvalue.StartsWith("I"))
                {
                    //Make sure split for Elem, Inter and Sen includes Retreat or Week
                    _EventType = uvalue.Contains("RETREAT") ? CCAEvents.EventType.Intermediate_Retreat.ToString() : CCAEvents.EventType.Intermediate_Week.ToString();
                }
                else if (uvalue.StartsWith("E"))
                {
                    //Make sure split for Elem, Inter and Sen includes Retreat or Week
                    _EventType = uvalue.Contains("RETREAT") ? CCAEvents.EventType.Elementary_Retreat.ToString() : CCAEvents.EventType.Elementary_Week.ToString();
                }
                else if (uvalue.StartsWith("FI") ||
                         uvalue.StartsWith("F") ||
                         uvalue.StartsWith("FC") ||
                         uvalue.StartsWith("CHA") ||
                         uvalue.StartsWith("CB"))
                {
                    _EventType = CCAEvents.EventType.First_Chance_Retreat.ToString();
                }
                else if (uvalue.StartsWith("B"))
                {
                    _EventType = CCAEvents.EventType.Banquet.ToString();
                }
                else if (uvalue.StartsWith("FUND"))
                {
                    _EventType = CCAEvents.EventType.Fund_Raiser.ToString();
                }
                else
                    _EventType = CCAEvents.EventType.Other.ToString();
            }
        }
        public String Baptized { get; set; }
        public String Skills
        {
            get
            {
                return _Skills;
            }
            set
            {
                char[] arr;
                arr = value.ToCharArray();
                foreach (char c in arr)
                {
                    switch (c)
                    {
                        case 'K':
                            if (_Skills.Length > 0) _Skills += ";";
                            _Skills += "multicheckbox[]:COK";
                            break;
                        case 'M':
                        case 'S':
                            if (_Skills.Length > 0) _Skills += ";";
                            _Skills += "multicheckbox[]:WRK;multicheckbox[]:KTH";
                            break;
                        case 'D':
                            if (_Skills.Length > 0) _Skills += ";";
                            _Skills += "multicheckbox[]:DEN";
                            break;
                        case 'T':
                        case 'C':
                        case 'R':
                            if (_Skills.Length > 0) _Skills += ";";
                            _Skills += "multicheckbox[]:CNS";
                            break;
                        case 'H':
                            if (_Skills.Length > 0) _Skills += ";";
                            _Skills += "multicheckbox[]:HLD";
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private String _PGLast = "";
        private String _Skills = "";
        public String _EventType = "";
        public override string ToString()
        {
            Type t = this.GetType();
            PropertyInfo[] props = t.GetProperties();
            String full = "";

            if ((FName == null) || (FName.Length == 0))
                return full;

            foreach (var prop in props)
            {
                if ((prop.GetMethod != null) && prop.CanRead && (prop.GetValue(this) != null) && (prop.GetValue(this).ToString().Length > 0))
                {
                    full = full + prop.Name + ":" + prop.GetValue(this) + ";";
                }
            }
            return full;
        }

        //******************************************************************
        //  PGn_id format is "First Last/Relationship/Phone/Email"
        //******************************************************************

        //TBD : Add PGPhone1 and PGEMail1 to all PG1 below with / delim when present at some point

        public String PGFirst1
        {
            set
            {
                if ((PGInfo1 != null) && (PGInfo1.Length > 0))
                    PGInfo1 = value.Trim() + " " + PGInfo1;
                else
                    PGInfo1 = value.Trim();

            }
        }
        public String PGLast1
        {
            set
            {
                _PGLast = value.Trim();

                if ((PGInfo1 != null) && (PGInfo1.Length > 0))
                    PGInfo1 = PGInfo1 + " " + _PGLast;
                else
                    PGInfo1 = _PGLast;
            }
        }

        public String PGFirst2
        {
            set
            {
                if ((PGInfo2 != null) && (PGInfo2.Length > 0))
                    PGInfo2 = value.Trim() + " " + PGInfo2;
                else
                    PGInfo2 = value.Trim();

            }
        }
        public String PGLast2
        {
            set
            {
                _PGLast = value.Trim();

                if ((PGInfo2 != null) && (PGInfo2.Length > 0))
                    PGInfo2 = PGInfo2 + " " + _PGLast;
                else
                    PGInfo2 = _PGLast;
            }
        }
        public String PGFirst12
        {
            set
            {
                char[] delims = { '&', ',' };
                String[] fnames = value.Split(delims);
                if (fnames.Count() > 0)
                {
                    if ((PGInfo1 != null) && PGInfo1.Length > 0)
                        PGInfo1 = fnames[0].Trim() + " " + PGInfo1;
                    else
                        PGInfo1 = fnames[0].Trim() + " " + _PGLast;
                    if (fnames.Count() > 1)
                    {
                        if ((PGInfo2 != null) && (PGInfo2.Length > 0))
                            PGInfo2 = fnames[1].Trim() + " " + PGInfo2;
                        else
                            PGInfo2 = fnames[1].Trim() + " " + _PGLast;
                    }
                }
            }
        }

        public string PGPhone1 { get; set; }

        public string PGEMail1 { get; set; }

        public string PGAdr1 { get; set; }

        public string PGPhone2 { get; set; }

        public string PGEMail2 { get; set; }

        public string PGAdr2 { get; set; }

        public void AdjustPGInfos()
        {
            // Ensure "First Last/Relationship/Phone/Email/Address"
            var pi1 = PGInfo1.Trim();
            var pi2 = PGInfo2.Trim();
            char delim = '/';

            var phone1 = PGPhone1.Trim();
            var email1 = PGEMail1.Trim();
            var adr1 = string.IsNullOrEmpty(PGAdr1) ? "" : PGAdr1.Trim();

            if (pi1.IndexOf(delim) == pi1.LastIndexOf(delim) && (!string.IsNullOrEmpty(phone1) || !string.IsNullOrEmpty(email1) || !string.IsNullOrEmpty(adr1)))
            {
                PGInfo1 = pi1 + "/PG/" + phone1 + "/" + email1 + "/" + adr1;
            }

            var phone2 = PGPhone2.Trim();
            var email2 = PGEMail2.Trim();
            var adr2 = string.IsNullOrEmpty(PGAdr2) ? "" : PGAdr2.Trim();

            if (pi2.IndexOf(delim) == pi2.LastIndexOf(delim) && (!string.IsNullOrEmpty(phone2) || !string.IsNullOrEmpty(email2) || !string.IsNullOrEmpty(adr2)))
            {
                PGInfo2 = pi2 + "/PG/" + phone2 + "/" + email2 + "/" + adr2;
            }
        }

        string RemovePhone(string val, out string phone)
        {
            phone = "";
            string newVal = "";
            string[] fields = val.Split(new char[] { ' ', '-' });
            for (int i = fields.Length - 1; i >= 0; --i)
            {
                bool phoneDone = false;
                var fld = fields[i].Replace("(", "").Replace(")", "").Trim();
                if (Int32.TryParse(fld, out int ival))
                    phone = fld + phone;
                else
                    phoneDone = true;

                if (phoneDone || (phone.Length >= 10))
                {
                    if (i <= 0)
                        return newVal;
                    for (int j = (phoneDone ? i : i - 1); j >= 0; --j)
                    {
                        newVal = fields[j].Trim() + (!string.IsNullOrEmpty(newVal) ? " " + newVal : "");
                    }
                    return newVal;
                }
            }
            return newVal;
        }

        public String PGFirstLast1
        {
            set
            {
                if (value.Contains(','))
                {
                    String[] names = value.Split(',');
                    if (names.Count() > 0)
                    {
                        PGInfo1 = names[0].Trim();
                        if (names.Count() > 1)
                            PGInfo1 = names[1].Trim() + " " + PGInfo1;
                    }
                }
                else
                {
                    if (value.Contains(' '))
                    {
                        String[] nms = value.Split(' ');
                        int count = nms.Count();
                        if (count > 0)
                        {
                            PGInfo1 = nms[0].Trim();
                            if (count > 1)
                                PGInfo1 = PGInfo1 + " " + nms[FindLNIndex(nms)].Trim();
                        }
                    }
                    //FName = value ZZZ;
                    //LName = value ZZZ;
                }
            }
        }

        public String PGFirstLast2
        {
            set
            {
                //  PGn_id format is "First Last/Relationship/Phone/Email"
                var valueR = RemovePhone(value, out string phone); // use valueN for name
                if (!string.IsNullOrEmpty(phone))
                    PGPhone2 = phone;

                if (valueR.Contains(','))
                {
                    String[] names = valueR.Split(',');
                    if (names.Count() > 0)
                    {
                        PGInfo2 = names[0].Trim();
                        if (names.Count() > 1)
                            PGInfo2 = names[1].Trim() + " " + PGInfo2;
                    }
                }
                else
                {
                    if (valueR.Contains(' '))
                    {
                        String[] nms = valueR.Split(new char[] { ' ', '-' });
                        int count = nms.Count();
                        if (count > 0)
                        {
                            PGInfo2 = nms[0].Trim();
                            if (count > 1)
                                PGInfo2 = PGInfo2 + " " + nms[FindLNIndex(nms)].Trim();
                        }
                    }
                    //FName = value ZZZ;
                    //LName = value ZZZ;
                }
            }
        }

        public String AttStart { get; set; }

        public String AttEnd { get; set; }

        private String _AttGrade = null;
        public String AttGrade
        {
            get { return _AttGrade; }
            set
            {
                String gstr = value.Trim().ToUpper();
                if (gstr.StartsWith("K"))
                    _AttGrade = "0";
                else if (gstr.Contains("C"))
                    _AttGrade = "13";
                else
                    _AttGrade = gstr;
            }
        }

        public int AttRole = (int)AttendanceRoles.Unknown;
        public String AttSRole
        {
            get { return ((AttendanceRoles)AttRole).ToString(); }
            set
            {
                String role = value.Trim();
                AttendanceRoles ar;
                if (Enum.TryParse<AttendanceRoles>(role, true, out ar))
                    AttRole = (int)ar;
                else
                    AttRole = (int)AttendanceRoles.Unknown;
            }
        }

        public static int FindLNIndex(String[] names)
        {
            int count = names.Length;
            if (count == 2)
                return 1;
            if (count >= 3)
            {
                string lstr = names[count - 1].ToLower().Replace(".", "");
                return ((lstr.StartsWith("jr") || lstr.StartsWith("sr") || lstr.StartsWith("ii") || lstr.StartsWith("iv") || (lstr == "v") || lstr.StartsWith("vi") || lstr.StartsWith("es") || Char.IsDigit(lstr[0]))
                      && (lstr.Length < 4)) ? count - 2 : count - 1;
            }
            return 0;
        }

        public String AdrStatus { get; set; }

    }
    public class ComboboxItem
    {
        public string Text { get; set; }
        public int PIndex { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
    public class AttData
    {
        public AttData()
        {
            id = pid = rid = eid = mid = tid = -1;
            PersonName = "";
            EventName = "";
            role = 0;
            CurGrade = -1;
            gender = '?';
            Address = "";
            EMail = "";
        }
        public int id { get; set; }
        public String PersonName { get; set; }
        public int AgeAtEvent { get; set; }
        public String EventName { get; set; }
        public int role { get; set; }
        public DateTime? StartDT { get; set; }
        public DateTime? EndDT { get; set; }
        public String MedForm { get; set; }
        public String RegForm { get; set; }
        public int CurGrade { get; set; }
        public String TSize { get; set; }
        public int pid { get; set; }
        public int rid { get; set; }
        public int eid { get; set; }
        public int mid { get; set; }
        public int tid { get; set; }
        public String note { get; set; }
        public char gender { get; set; }
        public String Address { get; set; }
        public String EMail { get; set; }
        public DateTime DOB { get; set; }

        public override string ToString()
        {
            return String.Format("{0,20} {1,2} {2,20} {3,10} {4,15} {5,15}", PersonName, AgeAtEvent, EventName, role.ToString(), StartDT.ToString(), EndDT.ToString());
        }
    }
}
//**************************************************************************************
//****** ONLY CHANGE IN CSTAT THEN RE-ADD to CCAEvent And CCAAttendance PROJECT ********
//**************************************************************************************
