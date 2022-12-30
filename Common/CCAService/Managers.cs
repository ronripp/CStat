using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using CStat.Models;
using Microsoft.EntityFrameworkCore;
using static CStat.Models.Church;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CStat.Common;

namespace CStat
{
    public enum MgrStatus
    {
        Add_Update_Succeeded = 1,
        Add_Person_Failed = -1,
        Save_Person_Failed = -2,
        Invalid_Address = -3,
        Invalid_DOB = -4,
        No_Gender = -5,
        Save_Address_Failed = -6,
        Save_Event_Failed = -7,
        Bad_Event_Type = -8,
        Add_Address_Failed = -9,
        Invalid_Church = -10,
        Add_Church_Failed = -11,
        Save_Attendance_Failed = -12,
        Add_Attendance_Failed = -13,
        Save_Reg_Failed = -14,
        Add_Reg_Failed = -15,
        Save_Med_Failed = -16,
        Add_Med_Failed = -17,
        Save_PG_Failed = -18,
    }
    public class AttendanceMgr // AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    {
        private CStatContext ce;

        public AttendanceMgr (CStatContext csc)
        {
            ce = csc;
        }

        public bool Validate(ref Attendance att)
        {
            return true; // TBD
        }

        public String DeleteAttendance(int id)
        {
            return "NotImplemented";
        }

        public String FindAttendee(int id)
        {
            if (id > -1)
            {
                var attList = from a in ce.Attendance
                              where (a.Id == id)
                              join p in ce.Person on a.PersonId equals p.Id
                              join e in ce.Event on a.EventId equals e.Id
                              join r in ce.Registration on new { eid = e.Id, pid = p.Id } equals new { eid = r.EventId.Value, pid = r.PersonId.Value }
                              join m in ce.Medical on new { eid = e.Id, pid = p.Id } equals new { eid = m.EventId.Value, pid = m.PersonId }
                              select new { aid = a.Id, fName = p.FirstName, lName = p.LastName, DOB = p.Dob, eSDate = e.StartTime,
                                  aSDate = a.StartTime, aRole = a.RoleType, eEDate = a.EndTime, RegForm = r.FormLink, MedForm = m.FormLink, CurGrade=r.CurrentGrade, TSize=r.TShirtSize };

                if (attList.Count() == 1)
                {
                    Attendance.AttData ad = new Attendance.AttData();
                    foreach (var aobj in attList)
                    {
                        ad.PersonName = aobj.fName + " " + aobj.lName;
                        ad.AgeAtEvent = 0;
                        if (aobj.DOB.HasValue)
                        {
                            ad.AgeAtEvent = aobj.eSDate.Year - aobj.DOB.Value.Year;
                            if ((aobj.eSDate.Month > aobj.DOB.Value.Month) || ((aobj.eSDate.Month == aobj.DOB.Value.Month) && (aobj.eSDate.Day > aobj.DOB.Value.Day)))
                                --ad.AgeAtEvent;
                        }
                        else
                            ad.AgeAtEvent = 0;
                        ad.id = aobj.aid;
                        ad.RegForm = aobj.RegForm;
                        ad.MedForm = aobj.MedForm;
                        ad.TSize = aobj.TSize;
                        ad.CurGrade = aobj.CurGrade.Value;
                        ad.role = aobj.aRole;

                        var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                        var json = JsonConvert.SerializeObject(ad, settings);
                        return json.ToString();
                    }
                }
            }

            return "Failed";
        }

        public String FindAttendees(String hint) // FFFF AAAAAAAAAAAAAAAAAA
        {
            int EType = -1, eid = -1;
            int EYear = -1;
            String TypeStr = "";
            if (hint.Length > 0)
            {
                if (hint.StartsWith("eid="))
                    eid = Int32.Parse(hint.Substring(3));
                else
                {
                    int tindex = hint.IndexOf("etype=");
                    if (tindex != -1)
                        TypeStr = hint.Substring(tindex + 5);
                    int yindex = hint.IndexOf("eyear=");
                    if (yindex != -1)
                    {
                        int eoff = hint.IndexOf(";");
                        if (eoff != -1)
                            EYear = Int32.Parse(hint.Substring(yindex + 5, eoff - 5));
                        else
                            EYear = Int32.Parse(hint.Substring(yindex + 5));
                    }

                    if ((EYear == -1) && hint.Trim().StartsWith("2"))
                    {
                        int yval;
                        if (Int32.TryParse(hint.Trim().Substring(0, 4), out yval))
                        {
                            EYear = yval;
                            TypeStr = hint.Trim().Substring(4).Trim();
                        }
                    }
                    EType = EventMgr.TypeFromString(TypeStr);
                }
            }

            if (eid != -1)
            {
                var attList = from a in ce.Attendance
                              join p in ce.Person on a.PersonId equals p.Id
                              join e in ce.Event on a.EventId equals e.Id
                              join r in ce.Registration on new { eid = e.Id, pid = p.Id } equals new { eid = r.EventId.Value, pid = r.PersonId.Value }
                              join m in ce.Medical on new { eid = e.Id, pid = p.Id } equals new { eid = m.EventId.Value, pid = m.PersonId }
                              select new
                              {
                                  aid = a.Id,
                                  fName = p.FirstName,
                                  lName = p.LastName,
                                  DOB = p.Dob,
                                  eSDate = e.StartTime,
                                  aSDate = a.StartTime,
                                  eEDate = e.EndTime,
                                  aEDate = a.EndTime,
                                  RegForm = r.FormLink,
                                  MedForm = m.FormLink,
                                  CurGrade = r.CurrentGrade,
                                  TSize = r.TShirtSize,
                                  aRole = a.RoleType,
                                  gender = p.Gender.HasValue ? p.Gender.Value : 63,
                                  EventName = e.Description
                              };

                Attendance.AttData adList = new Attendance.AttData();
                foreach (var a in attList)
                {
                    Attendance.AttData ad = new Attendance.AttData();
                    ad.PersonName = a.fName + " " + a.lName;
                    ad.EventName = a.EventName;

                    if ((a.aSDate.HasValue) && (a.aSDate.Value.Year > 1990))
                        ad.StartDT = a.aSDate;
                    else
                        ad.StartDT = a.eSDate;
                    if ((a.aEDate.HasValue) && (a.aEDate.Value.Year > 1990))
                        ad.EndDT = a.aEDate;
                    else
                    {
                        if (ad.EventName.Contains("SignIn"))
                            ad.EndDT = null;
                        else
                            ad.EndDT = a.eEDate;
                    }
                    ad.role = a.aRole;
                    ad.gender = (char)a.gender;
                    if (a.DOB.HasValue)
                    {
                        if (a.DOB.HasValue && (a.DOB.Value.Year > 1900))
                        {
                            ad.AgeAtEvent = ad.StartDT.Value.Year - a.DOB.Value.Year;
                            if ((ad.StartDT.Value.Month < a.DOB.Value.Month) || ((ad.StartDT.Value.Month == a.DOB.Value.Month) && (ad.StartDT.Value.Day < a.DOB.Value.Day)))
                                --ad.AgeAtEvent;
                        }
                        else
                            ad.AgeAtEvent = 0;
                    }
                    var jsonPG = JsonConvert.SerializeObject(ad, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    return jsonPG.ToString();
                }
                return ("FAIL");
            }
            else
            {   // TBD include EType
                var attList = from a in ce.Attendance
                              where ((EType == -1) || (int)a.Event.Type == EType)
                              join p in ce.Person on a.PersonId equals p.Id
                              join e in ce.Event on new { eid = a.EventId.Value, eyear = EYear } equals new { eid = e.Id, eyear = e.StartTime.Year }

                              //join r in ce.Registrations on new { eid2 = e.id, pid2 = p.id } equals new { eid2 = r.Event_id.Value, pid2 = r.Person_id.Value }
                              //join m in ce.Medicals on new { eid = e.id, pid = p.id } equals new { eid = m.Event_id.Value, pid = m.Person_id }
                              select new
                              {
                                  aid = a.Id,
                                  fName = p.FirstName,
                                  lName = p.LastName,
                                  DOB = p.Dob,
                                  eSDate = e.StartTime,
                                  aSDate = a.StartTime,
                                  eEDate = e.EndTime,
                                  aEDate = a.EndTime,
                                  aRole = a.RoleType,
                                  gender = p.Gender.HasValue ? p.Gender.Value : (byte)63,
                                  //RegForm = r.Form_Link,
                                  //MedForm = m.Form_Link,
                                  //CurGrade = r.Current_Grade,
                                  //TSize = r.T_Shirt_Size,
                                  EventName = e.Description
                              };
                List<Attendance.AttData> AttList = new List<Attendance.AttData>();
                foreach (var a in attList)
                {
                    Attendance.AttData ad = new Attendance.AttData();
                    ad.PersonName = a.fName + " " + a.lName;
                    ad.EventName = a.EventName;

                    if ((a.aSDate.HasValue) && (a.aSDate.Value.Year > 1990))
                        ad.StartDT = a.aSDate;
                    else
                        ad.StartDT = a.eSDate;
                    if ((a.aEDate.HasValue) && (a.aEDate.Value.Year > 1990))
                        ad.EndDT = a.aEDate;
                    else
                    {
                        if (ad.EventName.Contains("SignIn"))
                            ad.EndDT = null;
                        else
                            ad.EndDT = a.eEDate;
                    }
                    ad.role = a.aRole;
                    ad.EventName = a.EventName;
                    ad.gender = (char)a.gender;
                    if (a.DOB.HasValue)
                    {
                        if (a.DOB.HasValue && (a.DOB.Value.Year > 1900))
                        {
                            ad.AgeAtEvent = ad.StartDT.Value.Year - a.DOB.Value.Year;
                            if ((ad.StartDT.Value.Month < a.DOB.Value.Month) || ((ad.StartDT.Value.Month == a.DOB.Value.Month) && (ad.StartDT.Value.Day < a.DOB.Value.Day)))
                                --ad.AgeAtEvent;
                        }
                        else
                            ad.AgeAtEvent = 0;
                    }
                    AttList.Add(ad);
                }

                var jsonPG = JsonConvert.SerializeObject(AttList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return jsonPG.ToString();
            }
            
        }
        public MgrStatus Update(Attendance.AttData adata) // aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
        {
            Attendance a;
            Registration reg = null;
            Medical med = null;
           
            if (adata.id > -1)
            {
                a = ce.Attendance.FirstOrDefault(ai => ai.Id == adata.id);
                if (a == null)
                {
                    // Cant find it
                    a = new Attendance();
                    adata.id = -1;
                }
            }
            else
                a = new Attendance();

            if (adata.id != -1)
                a.Id = adata.id;

            if (adata.pid != -1)
                a.PersonId = adata.pid;

            a.RoleType = adata.role;


            //*********************************
            //*** Add / Update Registration ***
            //*********************************
            if ((adata.rid != -1) || ((adata.RegForm != null) && (adata.RegForm.Length > 0)))
            {
               
                if (adata.rid != -1)
                {
                    a.RegistrationId = adata.rid;
                    try
                    {
                        reg = ce.Registration.First(r => r.Id == adata.rid);
                    }
                    catch
                    {
                        reg = null;
                    }
                    if (reg != null)
                    {
                        reg.FormLink = adata.RegForm;
                        reg.TShirtSize = adata.TSize;
                        if (adata.CurGrade >= 0)
                            reg.CurrentGrade = adata.CurGrade;
                    }
                }
                if (reg == null)
                {
                    reg = new Registration();
                    reg.PersonId = adata.pid;
                    if (adata.RegForm != null)
                        reg.FormLink = adata.RegForm;
                    else
                        reg.FormLink = "?";

                    if (adata.TSize != null)
                        reg.TShirtSize = adata.TSize;
                    else
                        reg.TShirtSize = "?";
                    if (adata.CurGrade >= 0)
                        reg.CurrentGrade = adata.CurGrade;
                    if (adata.eid != -1)
                        reg.EventId = adata.eid;
                    else
                    {
                        if (adata.EventName.Length > 1)
                        {
                            Event ev = null;
                            ev = ce.Event.First(e => e.Description == adata.EventName);
                            if (ev != null)
                                reg.EventId = adata.eid = ev.Id;
                        }
                    }

                    Registration treg = null;
                    try
                    {
                        treg = ce.Registration.First(r => (r.EventId == reg.EventId) && (r.PersonId == reg.PersonId));
                    }
                    catch
                    {
                        treg = null;
                    }

                    if (treg != null)
                    {
                        reg.Id = treg.Id;
                        if ((adata.RegForm == null) || (adata.RegForm.Length <= 2))
                            reg.FormLink = treg.FormLink;
                        if (adata.TSize == null)
                            reg.TShirtSize = treg.TShirtSize;
                    }
                }

                
                try
                {
                    if (reg.Id <= 0)
                    {
                        // Add a new Registration
                        if (ce.Registration.Add(reg) == null)
                            return MgrStatus.Add_Reg_Failed;
                        a.RegistrationId = reg.Id;
                    }
                    else
                    {
                        // Update an existing Registration
                        a.RegistrationId = reg.Id;
                        ce.Registration.Attach(reg);
                        ce.Entry(reg).State = EntityState.Modified;
                    }
                    if (ce.SaveChanges() < 1)
                        return MgrStatus.Save_Reg_Failed;
                }
                catch
                {
                    return MgrStatus.Save_Reg_Failed;
                }
                
            }

            if ((adata.rid != -1) || ((adata.MedForm != null) && (adata.MedForm.Length > 0)))
            {
                //****************************
                //*** Add / Update Medical ***
                //****************************
                if (adata.mid != -1)
                {
                    a.MedicalId = adata.mid;
                    try
                    {
                        med = ce.Medical.First(m => m.Id == adata.mid);
                    }
                    catch
                    {
                        med = null;
                    }
                    if ((med != null) && (adata.MedForm != null) && (adata.MedForm.Length > 2))
                        med.FormLink = adata.MedForm;
                }
                if (med == null)
                {
                    med = new Medical();
                    med.PersonId = adata.pid;
                    if (adata.MedForm != null)
                        med.FormLink = adata.MedForm;
                    else
                        med.FormLink = "?";
                    if (adata.eid != -1)
                        med.EventId = adata.eid;
                    else
                    {
                        if (adata.EventName.Length > 1)
                        {
                            Event ev = null;
                            ev = ce.Event.First(e => e.Description == adata.EventName);
                            if (ev != null)
                                med.EventId = adata.eid = ev.Id;
                        }
                    }

                    Medical tmed = null;
                    try
                    {
                        tmed = ce.Medical.First(m => (m.EventId == med.EventId) && (m.PersonId == med.PersonId));
                    }
                    catch
                    {
                        tmed = null;
                    }

                    if (tmed != null)
                    {
                        med.Id = tmed.Id;
                        if ((adata.MedForm == null) || (adata.MedForm.Length <= 2))
                            med.FormLink = tmed.FormLink;
                    }
                }
      
                try
                {
                    if (med.Id <= 0)
                    {
                        // Add a new Medical
                        if (ce.Medical.Add(med) == null)
                            return MgrStatus.Add_Med_Failed;
                         a.MedicalId = med.Id;
                    }
                    else
                    {
                        // Update an existing Medical
                        a.MedicalId = med.Id;
                        ce.Medical.Attach(med);
                        ce.Entry(reg).State = EntityState.Modified;
                    }
                    if (ce.SaveChanges() < 1)
                        return MgrStatus.Save_Med_Failed;
                }
                catch
                {
                    return MgrStatus.Save_Med_Failed;
                }
            }

             if (adata.eid != -1)
                 a.EventId = adata.eid;
             else
             {
                 if (adata.EventName.Length > 1)
                 {
                     Event ev = null;
                     ev = ce.Event.First(e => e.Description == adata.EventName);
                     if (ev != null)
                         a.EventId = adata.eid = ev.Id;
                 }
             }

             // Add/Update Transaction Record
             if (adata.tid > 0)
                 a.TransactionId = adata.tid;

             if ((adata.note != null) && (adata.note.Length > 0))
                 a.DetailNote = adata.note;

             if (adata.StartDT.HasValue)
                 a.StartTime = adata.StartDT;
             if (adata.EndDT.HasValue)
                 a.EndTime = adata.EndDT;
             try
             {
                 Attendance a2 = null;
                 try
                 {
                     a2 = ce.Attendance.FirstOrDefault(ai => (ai.PersonId == a.PersonId) && (ai.EventId == a.EventId) &&
                                                              (((!ai.StartTime.HasValue || !a.StartTime.HasValue)) || (ai.StartTime == a.StartTime)) &&
                                                              (((!ai.EndTime.HasValue || !a.EndTime.HasValue)) || (ai.EndTime == a.EndTime)));
                 }
                 catch (Exception)
                 {
                     a2 = null;
                 }
                 if (a2 != null)
                     adata.id = a2.Id;

                 if (adata.id <= 0)
                 {
                     // Add a new Attendance
                     if (ce.Attendance.Add(a) == null)
                         return MgrStatus.Add_Attendance_Failed;
                 }
                 else
                 {
                     // Update an existing Attendance
                     ce.Attendance.Attach(a);
                     ce.Entry(a).State = EntityState.Modified;
                 }

                 if (ce.SaveChanges() < 1)
                     return MgrStatus.Save_Attendance_Failed;
             }
             catch (Exception e)
             {
                 string err = e.ToString();
                 return MgrStatus.Save_Attendance_Failed;
             }

            return MgrStatus.Add_Update_Succeeded;
       }
    }
    public class EventMgr
    {
        private CStatContext ce;

        public EventMgr(CStatContext csc)
        {
            ce = csc;
        }

        public bool Validate(ref Event e)
        {
            return true; // TBD
        }

        public String DeleteEvent(int id)
        {
            if (id != -1)
            {
                Event e = ce.Event.FirstOrDefault(ev => ev.Id == id);
                if (e != null)
                {
                    try
                    {
                        ce.Event.Attach(e);
                        ce.Entry(e).State = EntityState.Deleted;
                        ce.SaveChanges();
                    }
                    catch
                    {
                        return "DB Exception in deleting";
                    }
                    return "Event Successfully Deleted";
                }
            }
            return "Could not delete because Event was not found";
            
        }

        public String FindEvents(String hint)
        {
            int Type = -1, id = -1;
            int Year = -1;
            String TypeStr = "";
            if (hint.Length > 0)
            {
                if (hint.StartsWith("id="))
                    id = Int32.Parse(hint.Substring(3));
                else
                {
                    int tindex = hint.IndexOf("type=");
                    if (tindex != -1)
                        TypeStr = hint.Substring(tindex + 5);
                    int yindex = hint.IndexOf("year=");
                    if (yindex != -1)
                    {
                        int eoff = hint.IndexOf(";");
                        if (eoff != -1)
                            Year = Int32.Parse(hint.Substring(yindex + 5, eoff - 5));
                        else
                            Year = Int32.Parse(hint.Substring(yindex + 5));
                    }
                    Type = TypeFromString(TypeStr);
                }
            }
          
            List<Event.EInfo> EInfoList = new List<Event.EInfo>();
            if (id != -1)
            {
                var EInfoListD = (from e in ce.Event
                                  where (e.Id == id)
                                  orderby e.Description descending
                                  select e);

                Event.EventData edata = new Event.EventData();
                foreach (Event ev in EInfoListD)
                {
                    edata.id = ev.Id;
                    edata.Description = ev.Description;
                    edata.Type = TypeToString(ev.Type);
                    edata.StartDT = ev.StartTime;
                    edata.EndDT = ev.EndTime;
                    edata.churchID = ev.ChurchId.GetValueOrDefault(-1);
                    edata.costAdult = ev.CostAdult.GetValueOrDefault(0);
                    edata.costChild = ev.CostChild.GetValueOrDefault(0);
                    edata.costFamily = ev.CostFamily.GetValueOrDefault(0);
                    edata.costLodge = ev.CostLodge.GetValueOrDefault(0);
                    edata.costCabin = ev.CostCabin.GetValueOrDefault(0);
                    edata.costTent = ev.CostTent.GetValueOrDefault(0);
                    edata.contractLink = ev.ContractLink;
                    break;
                }

                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = JsonConvert.SerializeObject(edata, settings);
                return json.ToString();
            }
            else if ((Type != -1) || (Year != -1)) // Type=Y, Year=N 
            {
                if ((Type != -1) && (Year == -1))
                {
                    var EInfoListD = (from e in ce.Event
                                      where (e.Type == Type)
                                      orderby e.Description descending
                                      select e);

                    foreach (Event ev in EInfoListD)
                    {
                        Event.EInfo ei = new Event.EInfo();
                        ei.id = ev.Id;
                        ei.Description = ev.Description;
                        ei.Type = TypeToString(ev.Type);
                        ei.StartDate = ev.StartTime.Month + "-" + ev.StartTime.Day + "-" + ev.StartTime.Year;
                        EInfoList.Add(ei);
                    }
                    var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    return jsonPG.ToString();
                }
                else if ((Type == -1) && (Year != -1)) // Type=N, Year=Y
                {
                    if (TypeStr.Length > 0)
                    {
                        var EInfoListD = (from e in ce.Event
                                          where (e.StartTime.Year == Year) && e.Description.ToLower().Contains(TypeStr.ToLower())
                                          orderby e.Description descending
                                          select e);

                        foreach (Event ev in EInfoListD)
                        {
                            Event.EInfo ei = new Event.EInfo();
                            ei.id = ev.Id;
                            ei.Description = ev.Description;
                            ei.Type = TypeToString(ev.Type);
                            ei.StartDate = ev.StartTime.Month + "-" + ev.StartTime.Day + "-" + ev.StartTime.Year;
                            EInfoList.Add(ei);
                        }
                        var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        return jsonPG.ToString();
                    }
                    else
                    {
                        var EInfoListD = (from e in ce.Event
                                          where (e.StartTime.Year == Year)
                                          orderby e.Description descending
                                          select e);

                        foreach (Event ev in EInfoListD)
                        {
                            Event.EInfo ei = new Event.EInfo();
                            ei.id = ev.Id;
                            ei.Description = ev.Description;
                            ei.Type = TypeToString(ev.Type);
                            ei.StartDate = ev.StartTime.Month + "-" + ev.StartTime.Day + "-" + ev.StartTime.Year;
                            EInfoList.Add(ei);
                        }
                        var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        return jsonPG.ToString();
                    }
                }
                else if ((Type != -1) && (Year != -1)) // Type=Y, Year=Y
                {
                    var EInfoListD = (from e in ce.Event
                                      where (e.Type == Type) && (e.StartTime.Year == Year)
                                      orderby e.Description descending
                                      select e);

                    foreach (Event ev in EInfoListD)
                    {
                        Event.EInfo ei = new Event.EInfo();
                        ei.id = ev.Id;
                        ei.Description = ev.Description;
                        ei.Type = TypeToString(ev.Type);
                        ei.StartDate = ev.StartTime.Month + "-" + ev.StartTime.Day + "-" + ev.StartTime.Year;
                        EInfoList.Add(ei);
                    }
                    var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    return jsonPG.ToString();
                }
            }
            else if (TypeStr.Length > 0)  // Type=N, Desc=Y
            {
                var EInfoListD = (from e in ce.Event
                                  where e.Description.ToLower().Contains(TypeStr.ToLower())
                                  orderby e.Description descending
                                  select e);

                foreach (Event ev in EInfoListD)
                {
                    Event.EInfo ei = new Event.EInfo();
                    ei.id = ev.Id;
                    ei.Description = ev.Description;
                    ei.Type = TypeToString(ev.Type);
                    ei.StartDate = ev.StartTime.Month + "-" + ev.StartTime.Day + "-" + ev.StartTime.Year;
                    EInfoList.Add(ei);
                }
                var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return jsonPG.ToString();
            }

            //else if (hint.Length > 0)
            //{
            //    List<EInfo> EInfoList = new List<EInfo>();
            //    var EInfoListD = (from e in ce.Events
            //                      where (e.Description.ToLower().StartsWith(hint.ToLower()))
            //                      orderby e.Description descending
            //                      select e);

            //    foreach (Event ev in EInfoListD)
            //    {
            //        EInfo ei = new EInfo();
            //        ei.id = ev.id;
            //        ei.Description = ev.Description;
            //        ei.Type = TypeToString(ev.Type);
            //        ei.StartDate = ev.Start_Time.Month + "-" + ev.Start_Time.Day + "-" + ev.Start_Time.Year;
            //        EInfoList.Add(ei);
            //    }

            //    var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            //    return jsonPG.ToString();
            //}
            //else
            //{
            //    List<EInfo> EInfoList = new List<EInfo>();
            //    var EInfoListD = (from e in ce.Events
            //                      orderby e.Description descending
            //                      select e);

            //    foreach (Event ev in EInfoListD)
            //    {
            //        EInfo ei = new EInfo();
            //        ei.id = ev.id;
            //        ei.Description = ev.Description;
            //        ei.Type = TypeToString(ev.Type);
            //        ei.StartDate = ev.Start_Time.Month + "-" + ev.Start_Time.Day + "-" + ev.Start_Time.Year;
            //        EInfoList.Add(ei);
            //    }

            //    var jsonPG = JsonConvert.SerializeObject(EInfoList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            //    return jsonPG.ToString();
            //}
            return "Error : No Events Found";

        }
        public MgrStatus Update(Event.EventData edata) // eeeeeeeeeeeeeeeeeeeeeeeeeee
        {
            Event e;
            if (edata.id > -1)
            {
                e = ce.Event.FirstOrDefault(ei => ei.Id == edata.id);
                if (e == null)
                {
                    // Cant find it
                    e = new Event();
                    edata.id = -1;
                }
            }    
            else
                e = new Event();

            e.Description = edata.Description.Trim();
            e.Type = TypeFromString(edata.Type);

            e.StartTime = edata.StartDT;
            e.EndTime = edata.EndDT;

            if (edata.costChild > 0)
                e.CostChild = edata.costChild;
            else
                e.CostChild = null;
            if (edata.costAdult > 0)
                e.CostAdult = edata.costAdult;
            else
                e.CostAdult = null;
            if (edata.costFamily > 0)
                e.CostFamily = edata.costFamily;
            else
                e.CostFamily = null;
            if (edata.costLodge > 0)
                e.CostLodge = edata.costLodge;
            else
                e.CostLodge = null;
            if (edata.costCabin > 0)
                e.CostCabin = edata.costCabin;
            else
                e.CostCabin = null;
            if (edata.costTent > 0)
                e.CostTent = edata.costTent;
            else
                e.CostTent = null;
            e.ContractLink = edata.contractLink.Trim();
            e.ChurchId = edata.churchID;
            if (e.ChurchId == -1)
                e.ChurchId = null;
            try
            {
                if (edata.id == -1)
                {
                    // Add a new Event
                    if (ce.Event.Add(e) == null)
                        return MgrStatus.Add_Person_Failed;
                }
                else
                {
                    // Update an existing Event
                    ce.Event.Attach(e);
                    ce.Entry(e).State = EntityState.Modified;
                }

                if (ce.SaveChanges() < 1)
                    return MgrStatus.Save_Event_Failed;
            }
            catch
            {
                return MgrStatus.Save_Event_Failed;
            }

            return MgrStatus.Add_Update_Succeeded;
        }
        public static int TypeFromString(String typeStr)
        {
            String typeStr2 = typeStr.Replace(' ', '_');
            Event.EventType eType;
            if (Enum.TryParse<Event.EventType>(typeStr2, true, out eType))
                return (int)eType;
            else
                return -1;
        }
        public String TypeToString (int type)
        {
            Event.EventType et = (Event.EventType)type;
            String tstr = et.ToString();
            if (tstr != null)
                return tstr.Replace('_', ' ');
            else
                return MgrStatus.Bad_Event_Type.ToString();
        }
    }

    public class ChurchMgr // *********** CHURCH MANAGER ******************************************************************
    {
        private CStatContext ce;

        public ChurchMgr(CStatContext csc)
        {
            ce = csc;
        }
        
        public object ChurchLists { get; private set; }

        public bool Validate(ref Church c)
        {
            if (c.Name == null)
                return false;

            c.Name = c.Name.Trim();
            if (c.Name.Length < 1)
                return false;

            if (c.Affiliation == null)
                c.Affiliation = "?";

            if (c.StatusDetails == null)
                c.StatusDetails = "?";

            return true;
        }
        public String FindChurches(String hint)
        {
            if ((hint.Length > 0) && (hint[0] != '*'))
            {
                if (hint.StartsWith("id="))
                {
                    int id;
                    Church c;
                    if (Int32.TryParse(hint.Substring(3), out id))
                    {
                        c = ce.Church.FirstOrDefault(ci => ci.Id == id);
                        if (c != null)
                        {
                            Church.ChurchData cd = new Church.ChurchData();
                            cd.Name = c.Name;
                            cd.id = c.Id;
                            cd.Affiliation = c.Affiliation;
                            cd.MembershipStatus = c.MembershipStatus;
                            if (c.SeniorMinisterId != null)
                                cd.Minister_id = c.SeniorMinisterId.ToString();
                            if (c.YouthMinisterId != null)
                                cd.YouthPastor_id = c.YouthMinisterId.ToString();
                            if (c.Trustee1Id != null)
                                cd.Trustee1_id = c.Trustee1Id.ToString();
                            if (c.Trustee2Id != null)
                                cd.Trustee2_id = c.Trustee2Id.ToString();
                            if (c.Trustee3Id != null)
                                cd.Trustee3_id = c.Trustee3Id.ToString();
                            if (c.Alternate1Id != null)
                                cd.Alt1_id = c.Alternate1Id.ToString();
                            if (c.Alternate2Id != null)
                                cd.Alt2_id = c.Alternate2Id.ToString();
                            if (c.Alternate3Id != null)
                                cd.Alt3_id = c.Alternate3Id.ToString();

                            if (c.Address != null)
                            {
                                cd.Street = c.Address.Street;
                                cd.City = c.Address.Town;
                                cd.Zip = c.Address.ZipCode;
                                cd.State = c.Address.State;
                                cd.Phone = c.Address.Phone;
                            }
                            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                            var json = JsonConvert.SerializeObject(cd, settings);
                            return json.ToString();
                        }
                    }
                }

                List<CInfo> CInfoList = new List<CInfo>();
                var CInfoListN = (from c in ce.Church
                                  where (c.Name.ToLower().StartsWith(hint.ToLower()))
                                  select new { c.Id, c.Name, c.Affiliation }).ToList();

                var jsonPG = JsonConvert.SerializeObject(CInfoListN, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return jsonPG.ToString();
            }
            else
            {
                List<CInfo> CInfoList = new List<CInfo>();
                var CInfoListN = (from c in ce.Church
                                  orderby c.Name
                                  select new { c.Id, c.Name, c.Affiliation }).ToList();

                var jsonPG = JsonConvert.SerializeObject(CInfoListN, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                return jsonPG.ToString();
            }
            
        }

        public MgrStatus Update(Church.ChurchData cdata) //            UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU
        {
            bool bChurchFound = false;
            
            Church c;
            if (cdata.id > -1)
            {
                c = ce.Church.FirstOrDefault(ci => ci.Id == cdata.id);
                if (c == null)
                {
                    // Cant find it
                    c = new Church();
                    cdata.id = -1;
                }
                else
                    bChurchFound = true;
            }
            else
                c = new Church();

            c.Name = cdata.Name.Trim();
            if (c.Name.Length < 3)
            {
                return MgrStatus.Add_Church_Failed;
            }

            c.Affiliation = AffiliationType.UNK.ToString();
            for (int i = 0; i < (int)AffiliationType.NumAffiliations; ++i)
            {
                if (cdata.Affiliation == Church.ChurchLists.AffiliationList[i].aff_type.ToString())
                {
                    c.Affiliation = cdata.Affiliation;
                    break;
                }

                if (cdata.Affiliation == Church.ChurchLists.AffiliationList[i].name)
                {
                    c.Affiliation = Church.ChurchLists.AffiliationList[i].aff_type.ToString();
                    break;
                }
            }

            c.StatusDetails = MemberStatType.NOT_M.ToString();
            for (int i = 0; i < (int)MemberStatType.NumMemberStats; ++i)
            {
                if (cdata.Status == Church.ChurchLists.MemberStatList[i].ms_type.ToString())
                {
                    c.MembershipStatus = (int)Church.ChurchLists.MemberStatList[i].ms_type;
                    break;
                }

                if (cdata.Status == Church.ChurchLists.MemberStatList[i].name)
                {
                    c.MembershipStatus = (int)Church.ChurchLists.MemberStatList[i].ms_type;
                    break;
                }
            }

            AddressMgr am = new AddressMgr(ce);
            if (am.Update(-1, cdata.Street, cdata.City, cdata.State, cdata.Zip, cdata.Phone, out Address newAdr) == MgrStatus.Add_Update_Succeeded)
                c.AddressId = newAdr.Id;

            if (Validate(ref c))
            {
                try
                {
                    if (!bChurchFound)
                    {
                        ce.Church.Add(c);
                        ce.SaveChanges();
                        bChurchFound = true;
                        cdata.id = c.Id; // ensures people get new church id
                    }
                }
                catch
                {
                    return MgrStatus.Add_Church_Failed;
                }
            }

            int id;
            if (GetPersonID(cdata.Minister_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Minister_id, true, out id))
                c.SeniorMinisterId = id;
            if (GetPersonID(cdata.YouthPastor_id, out id) || GetChurchPerson(ce, ref cdata, cdata.YouthPastor_id, true, out id))
                c.YouthMinisterId = id;
            if (GetPersonID(cdata.Trustee1_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Trustee1_id, false, out id))
                c.Trustee1Id = id;
            if (GetPersonID(cdata.Trustee2_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Trustee2_id, false, out id))
                c.Trustee2Id = id;
            if (GetPersonID(cdata.Trustee3_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Trustee3_id, false, out id))
                c.Trustee3Id = id;
            if (GetPersonID(cdata.Alt1_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Alt1_id, false, out id))
                c.Alternate1Id = id;
            if (GetPersonID(cdata.Alt2_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Alt2_id, false, out id))
                c.Alternate2Id = id;
            if (GetPersonID(cdata.Alt3_id, out id) || GetChurchPerson(ce, ref cdata, cdata.Alt3_id, false, out id))
                c.Alternate3Id = id;

            if (Validate(ref c))
            {
                try
                {
                    if (!bChurchFound)
                        ce.Church.Add(c);
                    else
                    {
                        // Update an existing Church
                        ce.Church.Attach(c);
                        ce.Entry(c).State = EntityState.Modified;
                    }
                    ce.SaveChanges();
                }
                catch
                {
                    return MgrStatus.Add_Church_Failed;
                }
            }
            else
                return MgrStatus.Invalid_Church;


            return MgrStatus.Add_Update_Succeeded;
        }

        private bool GetChurchPerson(CStatContext ce, ref ChurchData cdata, String name, bool bIsMinister, out int id)
        {
            if (name.Trim().Length == 0)
            {
                id = -1;
                return false;
            }

            PersonMgr pmgr = new PersonMgr(ce);

            Person p = new Person();
            if (PersonMgr.SplitName(name, ref p))
            {
                int adr_id;
                id = pmgr.FindPersonIDByName(ce, ref p, true, out adr_id);
                if (id == -1)
                {
                    // Person not found. Add Person with bIsMinister
                    // FName: FFFFF; LName: LLLLLLL; Church: CCCCCC; multicheckbox[]:MNS; multicheckbox[]:DEN;
                    List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
                    props.Add(new KeyValuePair<string, string>("FName", p.FirstName));
                    props.Add(new KeyValuePair<string, string>("LName", p.LastName));
                    props.Add(new KeyValuePair<string, string>("Church", cdata.id.ToString()));
                    if (bIsMinister)
                    {
                        props.Add(new KeyValuePair<string, string>("multicheckbox[]", "MNS"));
                        props.Add(new KeyValuePair<string, string>("multicheckbox[]", "DEN"));
                    }

                    Person rp;
                    Address ra;
                    MgrStatus result = pmgr.Update(ref props, false, true, out rp, out ra);

                    if (result == MgrStatus.Add_Update_Succeeded)
                    {
                        id = rp.Id;
                        return true;
                    }
                }
            }

            id = -1;
            return false;
        }

        static private bool GetPersonID(String pstr, out int id)
        {
            id = -1;
            if ((pstr == null) || (pstr.Length == 0))
                return false;
            int sindex = pstr.IndexOf("[");
            if (sindex == -1)
            {
                if (Int32.TryParse(pstr.Trim(), out id))
                    return true;
                else
                    return false;
            }
            int eindex = pstr.IndexOf("]");
            if (eindex == (sindex + 1))
                return false;
            String NumStr = pstr.Substring(sindex + 1, (eindex - sindex) - 1);
            if (Int32.TryParse(NumStr, out id))
                return true;
            else
                return false;
        }

        public String DeleteChurch(int id)
        {
            if (id != -1)
            {
                Church c = ce.Church.FirstOrDefault(ch => ch.Id == id);
                if (c != null)
                {
                    try
                    {
                        ce.Church.Attach(c);
                        ce.Entry(c).State = EntityState.Deleted;
                        ce.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        return "DB Exception in deleting :" + e.Message;
                    }
                    return "Church Successfully Deleted";
                }
            }
            return "Could not delete because Church was not found";
        }
    }
        public class AddressMgr //                     ADDRESS MGR *************************************************
        {

        private CStatContext ce;

        public AddressMgr(CStatContext csc)
        {
            ce = csc;
        }

        public static bool Validate(ref Address a)
            {
                if (a.Street == null)
                    return false;

                a.Street = a.Street.Trim();
                if (a.Street.Length < 1)
                    return false;

                if (a.Town == null)
                    return false;

                a.Town = a.Town.Trim();
                if (a.Town.Length < 1)
                    return false;

                if (a.Town == null)
                    return false;

                a.Town = a.Town.Trim();
                if (a.Town.Length < 1)
                    return false;

                if (a.State == null)
                    return false;

                a.State = a.State.Trim();
                if (a.State.Length < 1)
                    return false;

                if (a.ZipCode == null)
                    return false;

                a.ZipCode = a.ZipCode.Trim();
                if (a.ZipCode.Length < 1)
                    return false;

                if (a.Country == null)
                    a.Country = "USA";
                else
                {
                    a.Country = a.Country.Trim();
                    if (a.ZipCode.Length < 1)
                        a.Country = "USA";
                    else
                    {
                        if (String.Compare(a.Country, "US", true) == 0)
                            a.Country = "USA";
                    }
                }

                if (a.Phone != null)
                    a.Phone = a.Phone.Trim();

                if (a.Fax != null)
                    a.Fax = a.Fax.Trim();

                if (a.WebSite != null)
                    a.WebSite = a.WebSite.Trim();

                return true;
            }

            public MgrStatus Update(int adr_id, String street, String city, String state, String zip, String hphone, out Address newAdr)
            {
                newAdr = new Address();
                bool bFoundAdr = false;

                bool bHasZip = zip.Length > 0;
                bool bHasStreet = street.Length > 0;
                bool bHasCS = (city.Length > 0) && (state.Length > 0);
                String streetValue;
                if (bHasStreet)
                    streetValue = PersonMgr.CleanStreet(street);
                else
                    streetValue = "";

                if (adr_id != -1)
                    bFoundAdr = true;

                if (!bFoundAdr && bHasStreet && (bHasZip || bHasCS))
                {
                    bool bTrySCS = false;

                    // Try to find an existing address by looping here up to 2 times.
                    do
                    {
                        int ld, MinLD = 10000;
                        List<Address> adrList = new List<Address>();

                        if (bHasZip && !bTrySCS)
                        {
                            if (zip != "11111")
                            {
                                var adrL = from adr in ce.Address
                                           where (adr.ZipCode == zip)
                                           select adr;
                                foreach (Address a in adrL)
                                {
                                    adrList.Add(a);
                                }
                            }
                        }
                        else
                        {
                            if (!city.Contains("<missing>"))
                            {
                                var adrL = from adr in ce.Address
                                           where (adr.Town == city) && (adr.State == state)
                                           select adr;

                                foreach (Address a in adrL)
                                {
                                    adrList.Add(a);
                                }
                            }
                        }

                        if (adrList.Count() > 0)
                        {
                            Address MinAdr = new Address();
                            foreach (Address a in adrList)
                            {
                                ld = a.Street.Contains("<missing>") ? 10000 : LevenshteinDistance.Compute(a.Street, streetValue);
                                if (ld < MinLD)
                                {
                                    MinAdr = a;
                                    MinLD = ld;
                                }
                            }

                            int si = MinAdr.Street.IndexOf(' ');
                            if ((MinLD < 2) && ((si > 1) && ((si + 3) < MinAdr.Street.Length) && streetValue.StartsWith(MinAdr.Street.Substring(0, si + 3))))
                            {
                                // Found matching address. Just set address id
                                adr_id = newAdr.Id = MinAdr.Id;
                                bFoundAdr = true;
                            }
                        }

                        if (bTrySCS)
                            break; // We already looped. Break here.

                        if (!bFoundAdr && bHasZip)
                        {
                            // Zip may be bad or changed try to match by street, city, state
                            bTrySCS = true;
                        }
                        else
                            break; // Do not loop because there is nothing to retry.
                    }
                    while (!bFoundAdr);
                }

                //***************************
                //*** Add Address fields ****
                //***************************
                if (bHasStreet)
                    newAdr.Street = streetValue;
                if (bHasCS)
                {
                    newAdr.Town = city;
                    newAdr.State = state;
                }

                if (bHasZip)
                    newAdr.ZipCode = zip;

                if (hphone.Length > 0)
                    newAdr.Phone = hphone;

                //AddressMgr amgr = new AddressMgr(ce);
                if (!AddressMgr.Validate(ref newAdr))
                {
                    return MgrStatus.Invalid_Address;
                }
                
                try
                {
                    if (!bFoundAdr)
                    {
                        ce.Address.Add(newAdr);
                    }
                    else
                    {
                        ce.Address.Attach(newAdr);
                        ce.Entry(newAdr).State = EntityState.Modified;
                    }
                    ce.SaveChanges();
                }
                catch
                {
                   return MgrStatus.Save_Address_Failed;
                }
                return MgrStatus.Add_Update_Succeeded;
          }
    }

    public class PersonMgr
    {
        private CStatContext ce;

        public PersonMgr(CStatContext csc)
        {
            ce = csc;
        }

        public static Dictionary<string, long> Skills = new Dictionary<string, long>
        {
            {"CRP", 0x0000000000000001},  //"CRP"== Carpentry
            {"PLB", 0x0000000000000002},  //"PLB"=Plumbing
            {"ROF", 0x0000000000000004},  //"ROF"=Roofing
            {"COK", 0x0000000000000008},  //"COK"=Cook
            {"HLD", 0x0000000000000010},  //"HLD"=Health Dir
            {"SWT", 0x0000000000000020},  //"SWT"=SWAT
            {"CMG", 0x0000000000000040},  //"CMG"=CampMgr
            {"PNT", 0x0000000000000080},  //"PNT"=Painter
            {"MNS", 0x0000000000000100},  //"MNS"=Minister
            {"DEN", 0x0000000000000200},  //"DEN"=Dean
            {"ACC", 0x0000000000000400},  //"ACC"=Accoutant
            {"ELE", 0x0000000000000800},  //"ELE"=Electrician
            {"CMP", 0x0000000000001000},  //"CMP"=Computer
            {"KTH", 0x0000000000002000},  //"KTH"=Kitchen.Hlp
            {"CNS", 0x0000000000004000},  //"CNS"=Counselor
            {"GAR", 0x0000000000008000},  //"GAR"=Gardening
            {"TRR", 0x0000000000010000},  //"TRR"=Tree Remove
            {"WRS", 0x0000000000020000},  //"WRS"=Worship
            {"WRK", 0x0000000000040000},  //"WRK"=Worker
            {"MSN", 0x0000000000080000},  //"MSN"=Mason
            {"CON", 0x0000000000100000},  //"CON"=Constructon
            {"SPT", 0x0000000000200000},  //"SPT"=SepticDrain
            {"UNK", 0x4000000000000000}
        };

        enum PersonStatus
        {
            NotBaptized = 0x0000000000000010, Baptized =0x0000000000000020
        }
        public static String MakeFirstName(Person p)
        {
            String ret = p.FirstName;
            if ((p.Alias != null) && (p.Alias.StartsWith("F:")))
            {
                ret += "(" + p.Alias.Substring(2) + ")";
            }
            return ret;
        }
        public static String MakeLastName (Person p)
        {
            String ret = p.LastName;
            if ((p.Alias != null) && (p.Alias.StartsWith("L:")))
            {
                ret += "(" + p.Alias.Substring(2) + ")";
            }
            return ret;
        }

        public String DeletePerson(ref List<KeyValuePair<String, String>> props)
        {
            KeyValuePair<string, string> pv;
            String FirstName="", LastName="";
            int id = -1;
            pv = props.Find(prop => prop.Key == "FName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                FirstName = pv.Value.Trim();

            pv = props.Find(prop => prop.Key == "LName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                LastName = pv.Value.Trim();

            pv = props.Find(prop => prop.Key == "id");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                id = Int32.Parse(pv.Value.Trim());

            if (id > 0)
            {
                Person p = ce.Person.FirstOrDefault(pr => (pr.Id == id) && (pr.FirstName.ToUpper() == FirstName.ToUpper()) && (pr.LastName.ToUpper() == LastName.ToUpper()));
                if (p != null)
                {
                    try
                    {
                        ce.Person.Attach(p);
                        ce.Entry(p).State = EntityState.Deleted;
                        ce.SaveChanges();
                    }
                    catch
                    {
                        return "DB Exception in deleting";
                    }
                    return "Person Successfully Deleted";
                }
            }
            return "Could not delete because Person was not found";
        }

        public MgrStatus Update(ref List<KeyValuePair<String, String>> props, bool bJustGetPerson, bool bAllowNoAddress, out Person ResPerson, out Address ResAddress)
        {
            Person person = new Person();
            Address newAdr = new Address();
            bool bPersonFound = false;
            bool bFoundAdr = false;
            bool bNeedToAddPG1 = false;
            bool bNeedToModPG1 = false;
            Person PG1Person = new Person();
            bool bNeedToAddPG2 = false;
            bool bNeedToModPG2 = false;
            Person PG2Person = new Person();
            bool bValidAddress = false;
         
            KeyValuePair<string, string> pv;

            pv = props.Find(prop => prop.Key == "FName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.FirstName = pv.Value.Trim();
                int spar = person.FirstName.IndexOf('(');
                if (spar > 0)
                {
                    int epar = person.FirstName.IndexOf(')');
                    if (epar > spar)
                    {
                        person.Alias = person.FirstName.Substring(spar + 1, (epar - spar) - 1);
                        person.Alias = "F:" + person.Alias.Trim();
                        person.FirstName = person.FirstName.Substring(0, spar);
                        person.FirstName = person.FirstName.Trim();
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "LName");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.LastName = pv.Value.Trim();
                int spar = person.LastName.IndexOf('(');
                if (spar > 0)
                {
                    int epar = person.LastName.IndexOf(')');
                    if (epar > spar)
                    {
                        person.Alias = person.LastName.Substring(spar + 1, (epar - spar) - 1);
                        person.Alias = "L:" + person.Alias.Trim();
                        person.LastName = person.LastName.Substring(0, spar);
                        person.LastName = person.LastName.Trim();
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "Alias");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Alias = "F:" + pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "Church");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.All(c => Char.IsDigit(c)))
                {
                    person.ChurchId = Convert.ToInt32(pv.Value);
                }
                else
                {
                    String churchValue = pv.Value.Trim().Replace(",", " ").Replace(".", " ").Replace("  ", " ").ToLower();
                    Church ch = ce.Church.AsNoTracking().FirstOrDefault(c => c.Name.Replace(",", " ").Replace(".", " ").Replace("  ", " ").ToLower() == churchValue);
                    if (ch != null)
                    {
                        person.ChurchId = ch.Id;
                    }
                    else
                    {
                        if (!bJustGetPerson)
                        {
                            // Add new Church
                            Church nc = new Church();
                            nc.Name = pv.Value.Trim();
                            nc.Affiliation = "?";
                            nc.StatusDetails = "Home church of " + person.FirstName + " " + person.LastName;
                            ChurchMgr cmgr = new ChurchMgr(ce);
                            if (cmgr.Validate(ref nc))
                            {
                                try
                                {
                                    ce.Church.Add(nc);
                                    ce.SaveChanges();
                                    person.ChurchId = nc.Id;
                                }
                                catch (Exception e)
                                {
                                    person.ChurchId = null;
                                    csl.Log("Add Church Exception : " + e.Message);
                                }
                            }
                        }
                    }
                }
            }

            KeyValuePair<string, string> street, zip, city, state;
            zip = props.Find(prop => prop.Key == "Zip");
            city = props.Find(prop => prop.Key == "City");
            state = props.Find(prop => prop.Key == "State");
            street = props.Find(prop => prop.Key == "Street");
            bool bHasSSNum = false;
            bool bHasDOB = false;
            bool bHasZip = !zip.Equals(default(KeyValuePair<String, String>)) && (zip.Value.Length > 0);
            if (bHasZip)
               zip = new KeyValuePair<string, string>("Zip", zip.Value.Replace("-", string.Empty));
            bool bHasStreet = !street.Equals(default(KeyValuePair<String, String>)) && (street.Value.Length > 0);
            bool bHasCS = (!city.Equals(default(KeyValuePair<String, String>)) && (city.Value.Length > 0)) && (!state.Equals(default(KeyValuePair<String, String>)) && (state.Value.Length > 0));
            String streetValue;
            if (bHasStreet)
                streetValue = CleanStreet(street.Value);
            else
                streetValue = "";
            if (((bHasStreet != bHasCS) || (bHasCS != bHasZip)) && !bJustGetPerson && !bAllowNoAddress)
            {
                ResPerson = null;
                ResAddress = null;
                return MgrStatus.Invalid_Address;
            }

            pv = props.Find(prop => prop.Key == "Adr_id");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.All(c => Char.IsDigit(c)))
                {
                    person.AddressId = Convert.ToInt32(pv.Value);
                    bFoundAdr = true;
                }
            }

            if (!bFoundAdr && bHasStreet && (bHasZip || bHasCS))
            {
                bool bTrySCS = false;

                // Try to find an existing address by looping here up to 2 times.
                do
                {
                    List<Address> adrList = new List<Address>();
                    if (bHasZip && !bTrySCS)
                    {
                        if (zip.Value != "11111")
                        {
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.ZipCode == zip.Value)
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                adrList.Add(a);
                            }
                        }
                    }
                    else
                    {
                        if (!city.Value.Contains("<missing>"))
                        {
                            var adrL = from adr in ce.Address.AsNoTracking()
                                       where (adr.Town == city.Value) && (adr.State == state.Value)
                                       select adr;
                            foreach (Address a in adrL)
                            {
                                adrList.Add(a);
                            }
                        }
                    }

                    if (adrList.Count() > 0)
                    {
                        foreach (Address a in adrList)
                        {
                            int ld = a.Street.Contains("<missing>") ? 10000 : LevenshteinDistance.Compute(a.Street, streetValue);
                            if (ld < 2)
                            {
                                int si = a.Street.IndexOf(' ');
                                if (String.Equals(a.Street, streetValue, StringComparison.OrdinalIgnoreCase) || ((si > 1) && ((si + 4) < a.Street.Length) && streetValue.StartsWith(a.Street.Substring(0, si + 4))))
                                {
                                    // Found matching address. Just set address id
                                    person.AddressId = newAdr.Id = a.Id;
                                    bFoundAdr = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (bTrySCS)
                        break; // We already looped. Break here.

                    if (!bFoundAdr && bHasZip)
                    {
                        // Zip may be bad or changed try to match by street, city, state
                        bTrySCS = true;
                    }
                    else
                        break; // Do not loop because there is nothing to retry.
                }
                while (!bFoundAdr);
            }

            //***************************
            //*** Add Address fields ****
            //***************************
            if (bHasStreet)
                newAdr.Street = streetValue;
            if (bHasCS)
            {
                newAdr.Town = city.Value;
                newAdr.State = state.Value;
            }
            else
            {
                if (bJustGetPerson)
                {
                    if (!city.Equals(default(KeyValuePair<String, String>)) && (city.Value.Length > 0))
                        newAdr.Town = city.Value;

                    if (!state.Equals(default(KeyValuePair<String, String>)) && (state.Value.Length > 0))
                       newAdr.State = state.Value;
                }
            }

            if (bHasZip)
                newAdr.ZipCode = zip.Value;

            KeyValuePair<string, string> hphone = props.Find(prop => prop.Key == "HPhone");
            if (!hphone.Equals(default(KeyValuePair<String, String>)) && (hphone.Value.Length > 0))
                newAdr.Phone = hphone.Value;

            //AddressMgr amgr = new AddressMgr(ce);
            if (!(bValidAddress = AddressMgr.Validate(ref newAdr)) && !bJustGetPerson && !bAllowNoAddress)
            {
                ResPerson = null;
                ResAddress = null;
                return MgrStatus.Invalid_Address;
            }

            pv = props.Find(prop => prop.Key == "EMail");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Email = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "SSNum");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (bJustGetPerson)
                {
                    person.Ssnum = pv.Value;
                    bHasSSNum = person.Ssnum.Length > 1;
                }
                else
                {
                    person.Ssnum = new string(pv.Value.Where(c => char.IsDigit(c)).ToArray()); // strip out non-numbers
                    bHasSSNum = person.Ssnum.Length > 3;
                }
            }

            pv = props.Find(prop => prop.Key == "Cell");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.CellPhone = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "DOB");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                String dob = pv.Value.Trim();
                int month, day, year;
                string[] tokens = dob.Split('/', '-', '.');
                if (tokens.Count() != 3)
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Invalid_DOB;
                }

                if ((tokens[0].Length < 1) || (tokens[1].Length < 1) || (tokens[2].Length < 1))
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Invalid_DOB;
                }

                bool T0_Num = ((tokens[0][0] >= '0') && (tokens[0][0] <= '9'));
                bool T1_Num = ((tokens[1][0] >= '0') && (tokens[1][0] <= '9'));

                if (T0_Num && T1_Num)
                {
                    month = Convert.ToInt32(tokens[0]);
                    day = Convert.ToInt32(tokens[1]);
                    year = Convert.ToInt32(tokens[2]);
                }
                else
                {
                    if (T0_Num)
                    {
                        month = DateTime.ParseExact(tokens[1], "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
                        day = Convert.ToInt32(tokens[0]);
                        year = Convert.ToInt32(tokens[2]);
                    }
                    else
                    {
                        month = DateTime.ParseExact(tokens[0], "MMMM", System.Globalization.CultureInfo.CurrentCulture).Month;
                        day = Convert.ToInt32(tokens[1]);
                        year = Convert.ToInt32(tokens[2]);
                    }
                }

                if (year < 100)
                {
                    int YNow = PropMgr.ESTNow.Year;
                    if (year > (YNow - 2000))
                        year += 1900;
                    else
                        year += 2000;
                }
                person.Dob = new DateTime(year, month, day);
                if (person.Dob == null)
                {
                    //ResPerson = null;
                    //ResAddress = null;
                    //return MgrStatus.Invalid_DOB;
                    person.Dob = new DateTime(1900, 1, 1); // To handle poor records
                }
                bHasDOB = true;
            }
            else
            {
                if (!bJustGetPerson)
                {
                    //ResPerson = null;
                    //ResAddress = null;
                    //return MgrStatus.Invalid_DOB;
                    person.Dob = new DateTime(1900, 1, 1);  // To handle poor records
                }
            }

            pv = props.Find(prop => prop.Key == "Gender");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                String gender = pv.Value.Trim().ToUpper();
                if ((gender == "F") || (gender == "70") || (gender == "FEMALE"))
                    person.Gender = (byte)'F';
                else if ((gender == "M") || (gender == "77") || (gender == "MALE"))
                    person.Gender = (byte)'M';
                else
                {
                    person.Gender = 0;
                    if (!bJustGetPerson && (gender.Length > 0))
                    {
                        ResPerson = null;
                        ResAddress = null;
                        return MgrStatus.No_Gender;
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "SkillSets");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.SkillSets = long.Parse(pv.Value.Trim());
 
            }

            pv = props.Find(prop => prop.Key == "Roles");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Roles = long.Parse(pv.Value.Trim());

            }

            var pva = from pair in props
                      where (pair.Key == "multicheckbox[]")
                      select pair;

            int NumPairs = pva.Count();
            if (NumPairs > 0)
            {
                person.SkillSets = 0;
                foreach (KeyValuePair<string, string> pair in pva)
                {
                    String sval = pair.Value.Trim();
                    long lval;
                    if (Skills.TryGetValue(sval, out lval))
                        person.SkillSets |= lval;
                }
            }

            pv = props.Find(prop => prop.Key == "Comments");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Notes = pv.Value.Trim();
            }

            pv = props.Find(prop => prop.Key == "Baptized");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                if (pv.Value.ToLower() == "yes")
                {
                    person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.NotBaptized;
                    person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.Baptized;
                }
                else
                {
                    if (pv.Value.ToLower() == "no")
                    {
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.Baptized;
                        person.Status = person.Status.GetValueOrDefault(0) | (long)PersonStatus.NotBaptized;
                    }
                    else
                    {
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.Baptized;
                        person.Status = person.Status.GetValueOrDefault(0) & ~(long)PersonStatus.NotBaptized;
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "id"); // ZZZ 
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                Int32 pid = Int32.Parse(pv.Value);
                if (pid > 0)
                {
                    bPersonFound = true;
                    person.Id = pid;
                }
            }

            pv = props.Find(prop => prop.Key == "PG1_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg1PersonId = int.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PG1_id");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    bool bParent = true;

                    if (pga.Count() > 1)
                    {
                        String rel = pga[1].Trim().ToUpper();
                        if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                            bParent = false;
                    }

                    if (bParent)
                    {
                        bool bAbort = false;
                        for (int i = 0; i < pga.Count(); ++i)
                        {
                            if (bAbort)
                                break;
                            switch (i)
                            {
                                case 0: // "First Last" name
                                    {
                                        if (SplitName(pga[0], ref PG1Person))
                                        {
                                            int adr_id;
                                            int id = FindPersonIDByName(ce, ref PG1Person, bAllowNoAddress, out adr_id);
                                            if (id != -1)
                                            {
                                                person.Pg1PersonId = PG1Person.Id = id;
                                                if (adr_id != -1)
                                                    PG1Person.AddressId = adr_id;
                                            }
                                            else
                                            {
                                                if (!bJustGetPerson)
                                                    bNeedToAddPG1 = true;
                                            }
                                        }
                                        else
                                            bAbort = true;
                                    }
                                    break;

                                case 2: // Phone
                                    {
                                        if (PG1Person != null)
                                        {
                                            PG1Person.CellPhone = pga[2].Trim();
                                            bNeedToModPG1 = true;
                                        }
                                    }
                                    break;

                                case 3: // EMail
                                    {
                                        if (PG1Person != null)
                                        {
                                            PG1Person.Email = pga[3].Trim();
                                            bNeedToModPG1 = true;
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            pv = props.Find(prop => prop.Key == "PG2_pid");
            if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
            {
                person.Pg2PersonId = Int32.Parse(pv.Value);
            }
            else
            {
                pv = props.Find(prop => prop.Key == "PG2_id");

                if (!pv.Equals(default(KeyValuePair<String, String>)) && (pv.Value.Length > 0))
                {
                    //  First Last/Relationship/Phone/Email
                    String[] pga = pv.Value.Split('/');
                    bool bParent = true;

                    if (pga.Count() > 1)
                    {
                        String rel = pga[1].Trim().ToUpper();
                        if ((rel.Length > 0) && !rel.Contains("MOTHER") && !rel.Contains("MOM") && !rel.Contains("FATHER") && !rel.Contains("DAD"))
                            bParent = false;
                    }

                    if (bParent)
                    {
                        bool bAbort = false;
                        for (int i = 0; i < pga.Count(); ++i)
                        {
                            if (bAbort)
                                break;

                            switch (i)
                            {
                                case 0: // "First Last" name
                                    {
                                        if (SplitName(pga[0], ref PG2Person))
                                        {
                                            int adr_id;
                                            int id = FindPersonIDByName(ce, ref PG2Person, bAllowNoAddress, out adr_id);
                                            if (id != -1)
                                            {
                                                person.Pg2PersonId = PG2Person.Id = id;
                                                if (adr_id != -1)
                                                    PG2Person.AddressId = adr_id;
                                            }
                                            else
                                            {
                                                if (!bJustGetPerson)
                                                    bNeedToAddPG2 = true;
                                            }
                                        }
                                        else
                                            bAbort = true;
                                    }
                                    break;

                                case 2: // Phone
                                    {
                                        if (PG2Person != null)
                                        {
                                            PG2Person.CellPhone = pga[2].Trim();
                                            bNeedToModPG2 = true;
                                        }
                                    }
                                    break;

                                case 3: // EMail
                                    {
                                        {
                                            if (PG2Person != null)
                                            {
                                                PG2Person.Email = pga[3].Trim();
                                                bNeedToModPG2 = true;
                                            }
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            Person JGPerson = (bJustGetPerson) ? person.ShallowCopy() : person;
            Address JGnewAdr = (bJustGetPerson) ? newAdr.ShallowCopy() : newAdr;

            //**************************************************************************
            // Find Person #1 : Find person by SS# First
            //**************************************************************************
            if (bHasSSNum)
            {
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.Ssnum == person.Ssnum)
                           select psn;
                
                // Match first name only due to marriage changing last name
                int adr_id = -1;
                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id, false);
                if (id != -1)
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (adr_id != -1)
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (!bPersonFound && bHasDOB)
            {
                //**************************************************************************
                // Find Person #2 : Find person by DOB and close name match
                //**************************************************************************
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.Dob == person.Dob) 
                           select psn;

                int adr_id = -1;
                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id);
                if (!bFoundAdr && (id != -1))
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (adr_id != -1)
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (!bPersonFound && (person.LastName != null) && (person.LastName.Length > 0))
            {
                //**************************************************************************
                // Find Person #3 : Find person by Last Name and close first name match
                //**************************************************************************
                int adr_id;
                var psnL = from psn in ce.Person.AsNoTracking()
                           where (psn.LastName.ToUpper() == person.LastName.ToUpper())
                           select psn;

                int id = FindPersonIDByName(psnL, ref person, bAllowNoAddress, out adr_id, false); // already checking last name in query
                if (id != -1)
                {
                    bPersonFound = true;
                    person.Id = id;
                    if (!bFoundAdr && (id != -1))
                    {
                        bValidAddress = true;
                        bFoundAdr = true;
                        person.AddressId = adr_id;
                    }
                }
            }

            if (bJustGetPerson)
            { 
                ResPerson = JGPerson;
                ResAddress = JGnewAdr;
                return MgrStatus.Add_Update_Succeeded;
            }

            int tryState = 1;
            try
            {
                if (bValidAddress)
                {
                    if (!bFoundAdr)
                    {
                        // Add new, valid address and set Person.Address_id
                        ce.Address.Add(newAdr);
                        if (ce.SaveChanges() < 1)
                        {
                            ResPerson = null;
                            ResAddress = null;
                            return MgrStatus.Save_Address_Failed;
                        }
                        if (newAdr.Id != 0)
                            person.AddressId = newAdr.Id;  // addedAdr.id;
                    }
                }

                if (bNeedToAddPG1)
                {
                    tryState = 2;
                    if (newAdr.Id != 0)
                        PG1Person.AddressId = newAdr.Id; // Assume PG lives with child.
                    if (ce.Person.Add(PG1Person) != null)
                    {
                        ce.SaveChanges();
                        if (PG1Person.Id != 0)
                            person.Pg1PersonId = PG1Person.Id;
                        else
                            person.Pg1PersonId = null;
                    }
                }
                else
                {
                    if (bNeedToModPG1)
                    {
                        ce.Person.Attach(PG1Person);
                        ce.Entry(PG1Person).State = EntityState.Modified;
                        ce.SaveChanges();
                    }
                }

                if (bNeedToAddPG2)
                {
                    tryState = 3;
                    if (newAdr.Id != 0)
                        PG2Person.AddressId = newAdr.Id; // Assume PG lives with child.

                    if (ce.Person.Add(PG2Person) != null)
                    {
                        ce.SaveChanges();
                        if (PG2Person.Id != 0)
                            person.Pg2PersonId = PG2Person.Id;
                        else
                            person.Pg2PersonId = null;
                    }
                }
                else
                {
                    if (bNeedToModPG2)
                    {
                        ce.Person.Attach(PG2Person);
                        ce.Entry(PG2Person).State = EntityState.Modified;
                        ce.SaveChanges();
                    }
                }

                if (!bPersonFound)
                {
                    // Add a new Person
                    tryState = 4;
                    if (ce.Person.Add(person) == null)
                    {
                        ResPerson = null;
                        ResAddress = null;
                        return MgrStatus.Add_Person_Failed;
                    }
                }
                else
                {
                    // Update an existing Person
                    ce.Person.Attach(person);
                    ce.Entry(person).State = EntityState.Modified;
                }

                if (ce.SaveChanges() < 1)
                {
                    ResPerson = null;
                    ResAddress = null;
                    return MgrStatus.Save_Person_Failed;
                }
            }
            catch (Exception e)
            {
                ResPerson = null;
                ResAddress = null;
                if (tryState > 1)
                    return MgrStatus.Add_Person_Failed;
                else
                    return MgrStatus.Add_Address_Failed;
            }

            ResPerson = person;
            ResAddress = newAdr;
            return MgrStatus.Add_Update_Succeeded;

        }

        public static void AddToSelect(ref String sel, String name, String value, ref bool bFiltered)
        {
            if ((value != null) && (value.Length > 0))
            {
                if (!bFiltered)
                    sel = sel + " WHERE ";
                else
                    sel = sel + " AND ";
                bFiltered = true;

                if (value.StartsWith("*"))
                {
                    value = value.Substring(1);
                    if (name.EndsWith("Name"))
                        sel = sel + "(" + name + " LIKE '%" + value + "' OR Alias LIKE '" + name.Substring(7, 1) + ":%" + value + "')";
                    else
                        sel = sel + name + " LIKE '%" + value + "'";
                }
                else
                {
                    if (value.EndsWith("*"))
                        value = value.Substring(0, value.Length-1);

                    if (name.EndsWith("Name"))
                        sel = sel + "(" + name + " LIKE '" + value + "%' OR Alias LIKE '" + name.Substring(7, 1) + ":" + value + "%')";
                     else
                        sel = sel + name + " LIKE '" + value + "%'";
                }
            }
        }

        public static void AddToSelect(ref String sel, String name, byte value, ref bool bFiltered)
        {
            if (value != 0)
            {
                if (!bFiltered)
                    sel = sel + " WHERE ";
                else
                    sel = sel + " AND ";
                bFiltered = true;

               sel = sel + name + "=" + value.ToString();
            }
        }

        public static void AddToSelect(ref String sel, String name, long value, ref bool bFiltered)
        {
            if (value != 0)
            {
                if (!bFiltered)
                    sel = sel + " WHERE ";
                else
                    sel = sel + " AND ";
                bFiltered = true;

                sel = sel + "(" + name + " & " + value.ToString() + " = " + value.ToString() + ")"; 
            }
        }
        public static void AddToSelect(ref String sel, String name, long? value, ref bool bFiltered)
        {
            if (value.HasValue)
            {
                if (!bFiltered)
                    sel = sel + " WHERE ";
                else
                    sel = sel + " AND ";
                bFiltered = true;

                sel = sel + "(" + name + " = " + value.ToString() + ")";
            }
        }

        public static void AddToSelect(ref String sel, String name, DateTime value, bool bDateOnly, ref bool bFiltered)
        {
            if ((value != null) && (value != default(DateTime)))
            {
                if (!bFiltered)
                    sel = sel + " WHERE ";
                else
                    sel = sel + " AND ";
                bFiltered = true;

                // 2009-05-01T14:18:12.430
                if (value != null)
                {
                    String dt;
                    if (bDateOnly)
                        dt = String.Format("'{0:####}-{1:##}-{2:##}'", value.Year, value.Month, value.Day);
                    else
                        dt = String.Format("'{0:####}-{1:##}-{2:##}T{3:##}:{4:##}.000'", value.Year, value.Month, value.Day, value.Hour, value.Minute);

                    sel = sel + name + "=" + dt;
                }
            }
        }

        public bool Validate(ref Person p)
        {
            return true; // TBD
        }

        public int FindPersonIDByName(CStatContext ce, ref Person person, bool bMerge, out int address_id, bool bCheckLastName = true)
        {
            String LName = person.LastName;

            var psnL = from psn in ce.Person.AsNoTracking()
                       where (psn.LastName == LName)
                       select psn;

            return FindPersonIDByName(psnL, ref person, bMerge, out address_id, bCheckLastName);
        }

        public int FindPersonIDByName(IQueryable<Person> psnL, ref Person person, bool bMerge, out int address_id, bool bCheckLastName=true)
        {
            address_id = -1;
            if (psnL.Count() > 0)
            {
                int ld, MinLD = 1000000;
                Person MinP = null;

                int FStartLen3 = 0;
                String FStart3 = "";
                int FStartLen4 = 0;
                String FStart4 = "";
                if (person.FirstName != null)
                {
                    FStartLen3 = person.FirstName.Length < 3 ? person.FirstName.Length : 3;
                    FStart3 = person.FirstName.Substring(0, FStartLen3).ToUpper();
                    FStartLen4 = person.FirstName.Length < 4 ? person.FirstName.Length : 4;
                    FStart4 = person.FirstName.Substring(0, FStartLen4).ToUpper();
                }

                foreach (Person p in psnL)
                {
                    if (person.Gender.HasValue && (person.Gender != 0))
                    {
                        if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                            continue;
                    }
                    if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                    {
                        if (String.Compare(p.LastName, person.LastName, true) != 0)
                            continue;
                    }

                    if ((p.Alias != null) && (person.Alias != null) && (p.Alias.Length > 0) && (String.Compare(p.Alias, person.Alias, true) == 0))
                    {
                        if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                            address_id = MinP.AddressId.Value;

                        // Merge existing info onto referenced person
                        if (bMerge)
                            MergePersonAttrs(p, ref person);

                        return p.Id;
                    }

                    if (FStartLen3 == 0)
                    {
                        MinP = p;
                        MinLD = 0;
                    }
                    else
                    {
                        ld = LevenshteinDistance.Compute(p.FirstName, person.FirstName);
                        if ((ld < MinLD) && (p.FirstName.ToUpper().StartsWith(FStart4)))
                        {
                            MinP = p;
                            MinLD = ld;
                        }
                        else
                        {
                            if ((ld < MinLD) && (p.FirstName.ToUpper().StartsWith(FStart3)))
                            {
                                MinP = p;
                                MinLD = ld;
                            }
                        }
                    }
                }

                if (MinLD < 3)
                {
                    if (MinP.AddressId.HasValue && (MinP.AddressId.Value > 0))
                        address_id = MinP.AddressId.Value;

                    // Merge existing info onto referenced person
                    if (bMerge)
                        MergePersonAttrs(MinP, ref person);

                    return MinP.Id;
                }

                if (FStartLen4 > 0)
                {
                    foreach (Person p in psnL)
                    {
                        if (person.Gender.HasValue && (person.Gender != 0))
                        {
                            if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                                continue;
                        }
                        if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                        {
                            if (String.Compare(p.LastName, person.LastName, true) != 0)
                                continue;
                        }

                        if (p.FirstName.ToUpper().StartsWith(FStart4))
                        {
                            if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                                address_id = p.AddressId.Value;

                            // Merge existing info onto referenced person
                            if (bMerge)
                                MergePersonAttrs(p, ref person);

                            return p.Id;
                        }
                    }
                }

                if (FStartLen3 > 0)
                {
                    foreach (Person p in psnL)
                    {
                        if (person.Gender.HasValue && (person.Gender != 0))
                        {
                            if ((p.Gender.HasValue) && (person.Gender != p.Gender))
                                continue;
                        }
                        if (bCheckLastName && (person.LastName != null) && (person.LastName.Length > 0))
                        {
                            if (String.Compare(p.LastName, person.LastName, true) != 0)
                                continue;
                        }

                        if (p.FirstName.ToUpper().StartsWith(FStart3))
                        {
                            if (p.AddressId.HasValue && (p.AddressId.Value > 0))
                                address_id = p.AddressId.Value;

                            // Merge existing info onto referenced person
                            if (bMerge)
                                MergePersonAttrs(p, ref person);

                            return p.Id;
                        }
                    }
                }
            }
            return -1;
        }

        public void MergePersonAttrs(Person src, ref Person dst)
        {
            if (src.Gender.HasValue && !dst.Gender.HasValue)
                dst.Gender = src.Gender;

            if ((src.Dob.HasValue && (src.Dob.Value.Year > 1900)) && (!dst.Dob.HasValue || (dst.Dob.Value.Year == 1900)))
                dst.Dob = src.Dob;

            if ( ((src.Email != null) && (src.Email.Length > 0)) && ((dst.Email == null) || (dst.Email.Length == 0)) )
                dst.Email = src.Email;

            if (((src.Ssnum != null) && (src.Ssnum.Length > 0)) && ((dst.Ssnum == null) || (dst.Ssnum.Length == 0)))
                dst.Ssnum = src.Ssnum;

            if (((src.CellPhone != null) && (src.CellPhone.Length > 0)) && ((dst.CellPhone == null) || (dst.CellPhone.Length == 0)))
                dst.CellPhone = src.CellPhone;

            if ((src.SkillSets != 0) && (dst.SkillSets == 0))
                dst.SkillSets = src.SkillSets;

            if ((src.ChurchId.HasValue) && (!dst.ChurchId.HasValue))
                dst.ChurchId = src.ChurchId;
        }
    
        static public bool SplitName (String name, ref Person p) // Currently assumes no possible last name alias
        {
            String raw = name.Trim();
            int isp = raw.LastIndexOf(" ");
            if (isp <= 0)
                return false;
            int iop = raw.IndexOf('(');
            if (iop > isp)
                return false;
            if (iop > 0)
            {
                int icp = p.FirstName.IndexOf(')');
                if (icp > iop)
                {
                    p.Alias = p.FirstName.Substring(iop + 1, (icp - iop) - 1);
                    p.Alias = "F:" + p.Alias.Trim();
                    p.FirstName = raw.Substring(0, iop);
                    p.FirstName = p.FirstName.Trim();
                }
            }
            else
            {
                p.FirstName = raw.Substring(0, isp);
                p.FirstName = p.FirstName.Trim();
            }
            p.LastName = raw.Substring(isp + 1);
            p.LastName = p.LastName.Trim();
            return true;
        }

        static public String CleanStreet(String oldStr)
        {
            String newStr = oldStr.Trim();
            newStr = newStr.Replace("  ", " ");
            String upStr = newStr.ToUpper();
            int len = newStr.Length;
            if (upStr.EndsWith(" DRIVE"))
                newStr = newStr.Remove(len - 5) + "Dr.";
            else if (upStr.EndsWith(" DR"))
                newStr = newStr.Remove(len - 2) + "Dr.";
            else if (upStr.EndsWith(" PLACE"))
                newStr = newStr.Remove(len - 5) + "Pl.";
            else if (upStr.EndsWith(" PL"))
                newStr = newStr.Remove(len - 2) + "Pl.";
            else if (upStr.EndsWith(" AVENUE"))
                newStr = newStr.Remove(len - 6) + "Ave.";
            else if (upStr.EndsWith(" AVE"))
                newStr = newStr.Remove(len - 3) + "Ave.";
            else if (upStr.EndsWith(" AV"))
                newStr = newStr.Remove(len - 2) + "Ave.";
            else if (upStr.EndsWith(" STREET"))
                newStr = newStr.Remove(len - 6) + "St.";
            else if (upStr.EndsWith(" ST"))
                newStr = newStr.Remove(len - 2) + "St.";
            else if (upStr.EndsWith(" ROAD"))
                newStr = newStr.Remove(len - 4) + "Rd.";
            else if (upStr.EndsWith(" RD"))
                newStr = newStr.Remove(len - 2) + "Rd.";
            else if (upStr.EndsWith(" COURT"))
                newStr = newStr.Remove(len - 5) + "Ct.";
            else if (upStr.EndsWith(" CT"))
                newStr = newStr.Remove(len - 2) + "Ct.";
            else if (upStr.EndsWith(" TRAIL"))
                newStr = newStr.Remove(len - 5) + "Tr.";
            else if (upStr.EndsWith(" TR"))
                newStr = newStr.Remove(len - 2) + "Tr.";
            else if (upStr.EndsWith(" TERRACE"))
                newStr = newStr.Remove(len - 7) + "Ter.";
            else if (upStr.EndsWith(" TER"))
                newStr = newStr.Remove(len - 3) + "Ter.";
            else if (upStr.EndsWith(" TERR"))
                newStr = newStr.Remove(len - 4) + "Ter.";
            else if (upStr.EndsWith(" Boulevard"))
                newStr = newStr.Remove(len - 9) + "Blvd.";
            else if (upStr.EndsWith(" Blvd"))
                newStr = newStr.Remove(len - 4) + "Blvd.";
            else if (upStr.EndsWith(" Blv"))
                newStr = newStr.Remove(len - 3) + "Blvd.";
            else if (upStr.EndsWith(" Bl"))
                newStr = newStr.Remove(len - 2) + "Blvd.";
            else if (upStr.EndsWith(" HIGHWAY"))
                newStr = newStr.Remove(len - 7) + "Hwy.";
            else if (upStr.EndsWith(" WAY"))
                newStr = newStr.Remove(len - 3) + "Way";

            return newStr;
        }
    }

    static class LevenshteinDistance
    {
        public static int Compute(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }
    
            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }
    
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
    
            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;
    
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (char.ToUpperInvariant(t[j - 1]) == char.ToUpperInvariant(s[i - 1])) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }

}
