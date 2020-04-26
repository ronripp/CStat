using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using CStat.Models;
using Microsoft.EntityFrameworkCore;

namespace CStat
{
    public class PInfo
    {
        public PInfo()
        {
            id = -1;
            FirstName = null;
            LastName = null;
            SSNum = null;
            ZipCode = null;
            Event e = new Event();
            Attendance a = new Attendance();
        }
        public int id { get; set; }
        public String FirstName { get; set; }
        public String LastName { get; set; }
        public String SSNum { get; set; }
        public String ZipCode { get; set; }
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

    public class PG
    {
        public PG()
        {
            Adr_id = 0;
            PG1_id = 0;
            PG2_id = 0;
            PG1Name = null;
            PG2Name = null;
        }
        public int Adr_id { get; set; }
        public string PG1Name { get; set; }
        public int PG1_id { get; set; }
        public string PG2Name { get; set; }
        public int PG2_id { get; set; }
    }

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
        public string CellPhone { get; set; }
        public string EMail { get; set; }
        public string ContactPref { get; set; }
        public string Notes { get; set; }
        public MinAddress Address;
    }
    
    public class ValuesController
    {
        private CStatContext entities;

        public ValuesController(CStatContext csc)
        {
            entities = csc;
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(string id) //********************************************************
        {
            if (id == null)
                return "Bad Get id";

            id = id.Replace("@@", "*");

            String MatchingChurchesStr = "Matching_Churches:";
            int MatchingChurchesIndex = id.IndexOf(MatchingChurchesStr);
            if (MatchingChurchesIndex >= 0)
            {
                id = id.Trim();
                String ChurchHint = id.Substring(MatchingChurchesIndex + MatchingChurchesStr.Length).Trim();
                ChurchMgr cmgr = new ChurchMgr(entities);
                return cmgr.FindChurches(ChurchHint);
            }

            String MatchingEventsStr = "Matching_Events:";
            int MatchingEventsIndex = id.IndexOf(MatchingEventsStr);
            if (MatchingEventsIndex >= 0)
            {
                id = id.Trim();
                String EventHint = id.Substring(MatchingEventsIndex + MatchingEventsStr.Length).Trim();
                EventMgr emgr = new EventMgr(entities);
                return emgr.FindEvents(EventHint);
            }

            String DeleteEventStr = "Delete_Event:";
            int DeleteEventIndex = id.IndexOf(DeleteEventStr);
            if (DeleteEventIndex >= 0)
            {
                id = id.Trim();
                String idStr = id.Substring(DeleteEventIndex + DeleteEventStr.Length).Trim();
                EventMgr emgr = new EventMgr(entities);
                if (idStr.Length > 0)
                {
                    int idVal = Int32.Parse(idStr);
                    return emgr.DeleteEvent(idVal);
                }
                else
                {
                    return "Missing Event id to Delete";
                }
            }

            // FindAttendance:id=

            // Matching_Attendees:eid=
            // Matching_Attendees:" + "eyear = " + ystr + "; etype = " + DescStr;
            String MatchingAttendeesStr = "Matching_Attendees:";
            int MatchingAttendeesIndex = id.IndexOf(MatchingAttendeesStr);
            if (MatchingAttendeesIndex >= 0)
            {
                id = id.Trim();
                String AttendeeHint = id.Substring(MatchingAttendeesIndex + MatchingAttendeesStr.Length).Trim();
                AttendanceMgr amgr = new AttendanceMgr(entities);
                return amgr.FindAttendees(AttendeeHint);
            }

            String DeleteChurchStr = "Delete_Church:";
            int DeleteChurchIndex = id.IndexOf(DeleteChurchStr);
            if (DeleteChurchIndex >= 0)
            {
                id = id.Trim();
                String idStr = id.Substring(DeleteChurchIndex + DeleteChurchStr.Length).Trim();
                ChurchMgr cmgr = new ChurchMgr(entities);
                if (idStr.Length > 0)
                {
                    int idVal = Int32.Parse(idStr);
                    return cmgr.DeleteChurch(idVal);
                }
                else
                {
                    return "Missing Church id to Delete";
                }
            }

            String MatchingNamesStr = "Matching_Names:";
            int MatchingNamesIndex = id.IndexOf("Matching_Names:");
            if (MatchingNamesIndex >= 0)
            {
                id = id.Trim();
                String FNTarget = id.Substring(MatchingNamesIndex + MatchingNamesStr.Length).Trim();
                String LNTarget;
                if (FNTarget.Length == 0)
                    return "No Name to Match";
                int SPIndex = FNTarget.LastIndexOf(' ');
                if (SPIndex != -1)
                {
                    LNTarget = FNTarget.Substring(SPIndex + 1).Trim();
                    FNTarget = FNTarget.Substring(0, SPIndex).Trim();
                }
                else
                    LNTarget = "";

                List<PInfo> PInfoList = new List<PInfo>();
                String falias = "F:" + FNTarget;
                if (LNTarget.Length > 0)
                {
                    String lalias = "L:" + LNTarget;
                    var PInfoListFL = (from p in entities.Person
                                       where (p.FirstName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias))) &&
                                       (p.LastName.ToLower().StartsWith(LNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == lalias)))
                                       join a in entities.Address on
                                       p.AddressId equals a.Id
                                       join c in entities.Church on
                                       p.ChurchId equals c.Id into pc
                                       from pcn in pc.DefaultIfEmpty()
                                       select new { p.Id, p.FirstName, p.LastName, p.Ssnum, a.ZipCode, pcn.Name, p.Alias }).ToList();

                    var jsonPG = JsonConvert.SerializeObject(PInfoListFL, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    return jsonPG.ToString();
                }
                else
                {
                    var PInfoListF = (from p in entities.Person
                                      where (p.FirstName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias))) ||
                                      (p.LastName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias)))
                                      join a in entities.Address on
                                      p.AddressId equals a.Id
                                      join c in entities.Church on
                                      p.ChurchId equals c.Id into pc
                                      from pcn in pc.DefaultIfEmpty()
                                      select new { p.Id, p.FirstName, p.LastName, p.Ssnum, a.ZipCode, pcn.Name}).ToList();

                    var jsonPG = JsonConvert.SerializeObject(PInfoListF, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    return jsonPG.ToString();
                }
            }

            String target = "Lookup Person:";
            int lookup_index = id.IndexOf(target);
            if (lookup_index >= 0)
            {
                String pidStr = id.Substring(lookup_index + target.Length);
                int pid = Int32.Parse(pidStr);

                String PG1Str = null, PG2Str = null;
                int PG1_id = 0, PG2_id = 0;

                //var PList = from p in entities.People
                //            where p.id == pid
                //            join a in entities.Addresses on
                //            p.Address_id equals a.id
                //            join c in entities.Churches on
                //            p.Church_id equals c.id into pc
                //            from pcn in pc.DefaultIfEmpty()
                //            select p;

                var PList = from p in entities.Person
                            where p.Id == pid
                            join a in entities.Address on
                            p.AddressId equals a.Id into pa
                            from a in pa.DefaultIfEmpty()
                            join c in entities.Church on
                            p.ChurchId equals c.Id into pc
                            from c in pc.DefaultIfEmpty()
                            from pcn in pc.DefaultIfEmpty()
                            select p;

                var pgObj = new PG();
                var MPList = new List<MinPerson>();

                foreach (Person pr in PList)
                {
                    pr.FirstName = PersonMgr.MakeFirstName(pr);
                    pr.LastName = PersonMgr.MakeLastName(pr);

                    // TEST pr.PG1_Person_id = 3;
                    // TEST pr.PG2_Person_id = 4;

                    if (pr.AddressId != null)
                        pgObj.Adr_id = (int)pr.AddressId;

                    Person p1 = entities.Person.FirstOrDefault(p => p.Id == pr.Pg1PersonId);
                    if (p1 != null)
                    {
                        p1.FirstName = PersonMgr.MakeFirstName(p1);
                        p1.LastName = PersonMgr.MakeLastName(p1);
                        PG1Str = p1.FirstName + " " + p1.LastName;
                        PG1_id = p1.Id;
                    }

                    Person p2 = entities.Person.FirstOrDefault(p => p.Id == pr.Pg2PersonId);
                    if (p2 != null)
                    {
                        p2.FirstName = PersonMgr.MakeFirstName(p2);
                        p2.LastName = PersonMgr.MakeLastName(p2);
                        PG2Str = p2.FirstName + " " + p2.LastName;
                        PG2_id = p2.Id;
                    }

                    if ((PG1Str != null) || (PG2Str != null))
                    {
                        if (PG1Str != null)
                        {
                            pgObj.PG1Name = PG1Str;
                            pgObj.PG1_id = PG1_id;
                        }
                        if (PG2Str != null)
                        {
                            pgObj.PG2Name = PG2Str;
                            pgObj.PG2_id = PG2_id;
                        }

                        var jsonPG = JsonConvert.SerializeObject(pgObj, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        pr.ContactPref = jsonPG.ToString();
                    }

                    var m = new MinPerson();
                    m.FirstName = pr.FirstName;
                    m.LastName = pr.LastName;
                    m.id = pr.Id;
                    m.Alias = pr.Alias;
                    m.DOB = pr.Dob;
                    m.Gender = pr.Gender;
                    m.Status = pr.Status;
                    m.SSNum = pr.Ssnum;
                    m.Address_id = pr.AddressId;
                    m.PG1_Person_id = pr.Pg1PersonId;
                    m.PG2_Person_id = pr.Pg2PersonId;
                    m.Church_id = pr.ChurchId;
                    m.SkillSets = pr.SkillSets;
                    m.CellPhone = pr.CellPhone;
                    m.EMail = pr.Email;
                    m.ContactPref = pr.ContactPref;
                    m.Notes = pr.Notes;
                    if (pr.Address != null)
                    {
                        m.Address.id = pr.Address.Id;
                        m.Address.Street = pr.Address.Street;
                        m.Address.Town = pr.Address.Town;
                        m.Address.State = pr.Address.State;
                        m.Address.ZipCode = pr.Address.ZipCode;
                        m.Address.Phone = pr.Address.Phone;
                        m.Address.Fax = pr.Address.Fax;
                        m.Address.Country = pr.Address.Country;
                        m.Address.WebSite = pr.Address.WebSite;
                    }
                    MPList.Add(m);
                }

                if (MPList != null)
                {
                    var json = JsonConvert.SerializeObject(MPList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    string jstr = json.ToString();
                    return jstr;
                }
                else
                    return ("Lookup Failed with id:" + pid);
            }

            //******************************************************
            // MULTI_FIELD FIND PEOPLE SEARCH
            //******************************************************

            // Generate Key/Value Pairs to update person
            char[] outerDelims = { ';', '~', '&' };
            char[] innerDelims = { ':', '=' };

            string[] pairs = id.Split(outerDelims);

            List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
            foreach (string nvs in pairs)
            {
                string[] nvp = nvs.Split(innerDelims);
                if ((nvp.Length == 2) && (nvp[1].Length > 0))
                {
                    props.Add(new KeyValuePair<string, string>(nvp[0], nvp[1]));
                }
            }

            // SQL Based Query

            String sel = "Select * from Person FULL OUTER JOIN Address ON (Person.Address_id = Address.id)";
            int orgSelLen = sel.Length;
            bool bFiltered = false;

            PersonMgr pmgr = new PersonMgr(entities);
            Person fp;
            Address fa;
            if (pmgr.Update(ref props, true, true, out fp, out fa) != MgrStatus.Add_Update_Succeeded)
                return "Invalid Search Criteria";

            PersonMgr.AddToSelect(ref sel, "Person.FirstName", fp.FirstName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.LastName", fp.LastName, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.Street", fa.Street, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.Town", fa.Town, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.CellPhone", fp.CellPhone, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.EMail", fp.Email, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Church_id", fp.ChurchId, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.SSNum", fp.Ssnum, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Address.State", fa.State, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Gender", fp.Gender.GetValueOrDefault(0), ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.SkillSets", fp.SkillSets, ref bFiltered);
            PersonMgr.AddToSelect(ref sel, "Person.Status", fp.Status, ref bFiltered);
            if (!fp.Dob.GetValueOrDefault().Equals(new DateTime(1900,1,1)))
                PersonMgr.AddToSelect(ref sel, "Person.DOB", fp.Dob.GetValueOrDefault(), true, ref bFiltered);

            if (sel.Length == orgSelLen)
                sel += "order by Address.Street ASC";

            //                Also, it is recommended to use ISO8601 format YYYY - MM - DDThh:mm: ss.nnn[Z], as this one will not depend on your server's local culture.
            //SELECT*
            //FROM TABLENAME
            //WHERE
            //    DateTime >= '2011-04-12T00:00:00.000' AND
            //    DateTime <= '2011-05-25T03:53:04.000'

            try
            {
                var PList = entities.Person.FromSqlRaw(sel).ToList<Person>();

                var MPList = new List<MinPerson>();

                if (PList != null)
                {
                    foreach (var p in PList)
                    {
                        if (p == null)
                            continue;
                        var m = new MinPerson();
                        m.FirstName = p.FirstName;
                        m.LastName = p.LastName;
                        m.id = p.Id;
                        m.Alias = p.Alias;
                        m.DOB = p.Dob;
                        m.Gender = p.Gender;
                        m.Status = p.Status;
                        m.SSNum = p.Ssnum;
                        m.Address_id = p.AddressId;
                        m.PG1_Person_id = p.Pg1PersonId;
                        m.PG2_Person_id = p.Pg2PersonId;
                        m.Church_id = p.ChurchId;
                        m.SkillSets = p.SkillSets;
                        m.CellPhone = p.CellPhone;
                        m.EMail = p.Email;
                        m.ContactPref = p.ContactPref;
                        m.Notes = p.Notes;
                        if (p.Address != null)
                        { 
                            m.Address.id = p.Address.Id;
                            m.Address.Street = p.Address.Street;
                            m.Address.Town = p.Address.Town;
                            m.Address.State = p.Address.State;
                            m.Address.ZipCode = p.Address.ZipCode;
                            m.Address.Phone = p.Address.Phone;
                            m.Address.Fax = p.Address.Fax;
                            m.Address.Country = p.Address.Country;
                            m.Address.WebSite = p.Address.WebSite;
                        }
                        else
                        {
                            m.Address.Street = "";
                            m.Address.Town = "";
                            m.Address.State = "";
                            m.Address.ZipCode = "";
                            m.Address.Phone = "";
                            m.Address.Fax = "";
                            m.Address.Country = "";
                            m.Address.WebSite = "";
                        }
                        MPList.Add(m);
                    }
                }
            
                if (MPList.Count > 0)
                {
                    var json = JsonConvert.SerializeObject(MPList, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    string jstr = json.ToString();
                    return jstr;
                }
            }
            catch(Exception)
            {
            }

            return "No one found with Name";
        }

        // PUT api/values/5
        public void Put(int id, string value)
        {
            if (value == null)
                return;

            if (id == 12)
            {
                int i = 13;
                i *= 3;
                return;
            }

            // Generate Key/Value Pairs to update person
            char[] outerDelims = { ';', '~', '&' };
            char[] innerDelims = { ':', '=' };

            string[] pairs = value.Split(outerDelims);

            List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
        }


        // Not using Post : POST api/values
        public void Post(string value)
        {
            // Generate Key/Value Pairs to update person
            char[] outerDelims = { ';', '~', '&' };
            char[] innerDelims = { ':', '=' };

            string[] pairs = value.Split(outerDelims);

            List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
    }

