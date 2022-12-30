#define ADD_TO_DB
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

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
            PG1_id = "";
            PG2_id = "";
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
            PG1_Info = "";
            PG2_Info = "";
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
                        if (nms.Count() > 0)
                        {
                            FName = nms[0].Trim();
                            if (nms.Count() > 1)
                                LName = nms[1].Trim();
                        }
                    }
                    //FName = value ZZZ;
                    //LName = value ZZZ;
                }
            }
        }
        public String FLName
        {
            set
            {
                LFName = value;
            }
        }
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
        public String PG1_id { get; set; }
        public String PG2_id { get; set; }
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
                    _EventType = CCAEvents2.EventType.BWAC_Fall_Retreat.ToString();
                }
                else if (uvalue.StartsWith("MEN"))
                {
                    _EventType = CCAEvents2.EventType.Mens_Retreat.ToString();
                }
                else if (uvalue.StartsWith("WINT"))
                {
                    _EventType = CCAEvents2.EventType.Winter_Retreat.ToString();
                }
                else if (uvalue.StartsWith("WOM"))
                {
                    _EventType = CCAEvents2.EventType.Womans_Retreat.ToString();
                }
                else if (uvalue.StartsWith("FAM"))
                {
                    _EventType = CCAEvents2.EventType.Family_Retreat.ToString();
                }
                else if (uvalue.StartsWith("Y"))
                {
                    _EventType = CCAEvents2.EventType.Young_Adult_Retreat.ToString();
                }
                else if (uvalue.StartsWith("V"))
                {
                    _EventType = CCAEvents2.EventType.VOE_Week.ToString();
                }
                else if (uvalue.StartsWith("SE") || value.StartsWith("SN"))
                {
                    _EventType = CCAEvents2.EventType.Senior_Week.ToString();
                }
                else if (uvalue.StartsWith("SP") || value.StartsWith("SR"))
                {
                    _EventType = CCAEvents2.EventType.Spring_Work_Retreat.ToString();
                }
                else if (uvalue.StartsWith("I"))
                {
                    _EventType = CCAEvents2.EventType.Intermediate_Week.ToString();
                }
                else if (uvalue.StartsWith("E"))
                {
                    _EventType = CCAEvents2.EventType.Elementary_Week.ToString();
                }
                else if (uvalue.StartsWith("FI") ||
                         uvalue.StartsWith("F") ||
                         uvalue.StartsWith("FC") ||
                         uvalue.StartsWith("CHA") ||
                         uvalue.StartsWith("CB"))
                {
                    _EventType = CCAEvents2.EventType.First_Chance_Retreat.ToString();
                }
                else if (uvalue.StartsWith("B"))
                {
                    _EventType = CCAEvents2.EventType.Banquet.ToString();
                }
                else if (uvalue.StartsWith("FUND"))
                {
                    _EventType = CCAEvents2.EventType.Fund_Raiser.ToString();
                }
                else
                    _EventType = CCAEvents2.EventType.Other.ToString();
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
        public String PGFirst1
        {
            set
            {
                if ((PG1_id != null) && (PG1_id.Length > 0))
                    PG1_id = value.Trim() + " " + PG1_id;
                else
                    PG1_id = value.Trim();

            }
        }
        public String PGLast1
        {
            set
            {
                _PGLast = value.Trim();

                if ((PG1_id != null) && (PG1_id.Length > 0))
                    PG1_id = PG1_id + " " + value.Trim();
                else
                    PG1_id = value.Trim();
            }
        }

        public String PGFirst2
        {
            set
            {
                if ((PG2_id != null) && (PG2_id.Length > 0))
                    PG2_id = value.Trim() + " " + PG2_id;
                else
                    PG2_id = value.Trim();

            }
        }
        public String PGLast2
        {
            set
            {
                _PGLast = value.Trim();

                if ((PG2_id != null) && (PG2_id.Length > 0))
                    PG2_id = PG2_id + " " + value.Trim();
                else
                    PG2_id = value.Trim();
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
                    if ((PG1_id != null) && PG1_id.Length > 0)
                        PG1_id = fnames[0].Trim() + " " + PG1_id;
                    else
                        PG1_id = fnames[0].Trim() + " " + _PGLast;
                    if (fnames.Count() > 1)
                    {
                        if ((PG2_id != null) && (PG2_id.Length > 0))
                            PG2_id = fnames[1].Trim() + " " + PG2_id;
                        else
                            PG2_id = fnames[1].Trim() + " " + _PGLast;
                    }
                }
            }
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
                        PG1_id = names[0].Trim();
                        if (names.Count() > 1)
                            PG1_id = names[1].Trim() + " " + PG1_id;
                    }
                }
                else
                {
                    if (value.Contains(' '))
                    {
                        String[] nms = value.Split(' ');
                        if (nms.Count() > 0)
                        {
                            PG1_id = nms[0].Trim();
                            if (nms.Count() > 1)
                                PG1_id = PG1_id + nms[1].Trim();
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

        public String PG1_Info
        {
            set
            {
                PG1_id = value;
            }
        }

        public String PG2_Info
        {
            set
            {
                PG2_id = value;
            }
        }
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

        public override string ToString()
        {
            return String.Format("{0,20} {1,2} {2,20} {3,10} {4,15} {5,15}", PersonName, AgeAtEvent, EventName, role.ToString(), StartDT.ToString(), EndDT.ToString());
        }
    }
}
