using CCAAttendance;
using CCAEvents2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class Logger
    {
        public String name = "";
        public int id = 0;
        public String purpose = "";
        public DateTime? timeIn = null;
        public DateTime? timeOut = null;
        public bool FromUnSignOut = false;
        public bool HasEditTimeIn = false;
    }

    public class ImportPA
    {
        List<string> _Results = new List<string>();
        List<string> _StatusLog = new List<string>();
        string _Server = "";
  
        static Dictionary<string, string> ImportFields = new Dictionary<string, string>   //         {  {"entry", "entries"}, ... };
        {
 
//      X, CAMP        ,CAMPER            , PARENTS' /GUARDIANS' , ,ADDRESS          ,CITY     ,ST,ZIP  ,Cash,AMOUNT,DPST,PMNT,DSCNT,SCLSHP,CANTN,DNTION,TTL  ,OK ,EMAIL              ,CHURCH  ,TELEPHONE,
//      F, First Chance,"Edgar, Briana"   ,Edgar,Brian & Danielle  ,29 Seawanhaka Ave,Nesconset,NY,11767,396 ,$95.00,    ,95.00,    ,      ,     ,      ,95.00,  ,,,,
//      1F,First Chance,"Spataro, Alathea",Spataro,Glenn & Danielle,9 Longleaf Ln    ,Medford  ,NY,11763,2062,$95.00,    ,0.00 ,    ,      ,5.00 ,      ,5.00 ,CXD,daspataro@gmail.com,Holbrook,631-730-7320,

// Role,Check in,Check Out,Phone,Payment,Room,Reg ID,Reg Date,Last Name,First Name,Email,Address,City,State,Zip,Phone,Payment,Date of Birth,Home Church,PG1 Info,PG2 Info
// Camper,10/6/2017,,turned in,$75 #3929,Girl 2,236,9/30/2017 19:36,Charbonneau,Paige,bobcharb67@gmail.com,400 Hollow Rd.,Watertown,CT,06795,860-213-3646,75,2/7/2003,Woodland Church of Christ,PattyCharbonneau/Mom/860-945-6025/charbonep02@hotmail.com,BobCharbonneau/Dad/860-213-3646/bobcharb67@gmail.com

            {"CAMP PROGRAM","Event.Type"},
            {"CAMP","Event.Type"},
            {"WEEK","Event.Type"},
            {"Event","Event.Type"},

            {"CAMPER","Person.NameLF"},
            {"Staff/Camper", "Attendance.Role_Type"}, // Role : CAMPER = Camper,  S*=Staff
            {"Role", "Attendance.Role_Type"}, // Role : CAMPER = Camper,  S*=Staff
            {"PGFIRST","Person.PGFirst1"},
            {"Mom","Person.PGFirst1"},
            {"Mother","Person.PGFirst1"},
            {"Dad","Person.PGFirst2"},
            {"Father","Person.PGFirst2"},
            {"PGLAST","Person.PGLast1"},
            {"PG First","Person.PGFirst1"},
            {"PG Last","Person.PGLast1"},
            {"PGFIRST12","Person.PGFirst12"},
            {"PG First Both","Person.PGFirst12"},
            {"Parent's Name", "Person.PGFirstLast1"},

            {"Street","Address.Street"},
            {"ADDRESS","Address.Street"},

            {"CITY","Address.Town"},

            {"ST","Address.State"},
            {"STATE","Address.State"},

            {"ZIP","Address.ZipCode"},

            {"EMAIL","Person.EMail"},
            {"CHURCH","Person.Church"},
            {"TELEPHONE","Person.CellPhone"}, // can have H,h or C,c appended
            {"PHONE","Person.CellPhone"},

            {"Last","Person.LastName"},
            {"Last Name", "Person.LastName"},

            {"First","Person.FirstName"},
            {"First Name", "Person.FirstName"},

            {"DOB","Person.DOB"},
            {"D.O.B.", "Person.DOB"},
            {"Date of Birth", "Person.DOB"},
            {"Date Of Birth", "Person.DOB"},
            {"Check In", "Attendance.StartDT" },
            {"Check Out", "Attendance.EndDT" },

            {"PG1 Info", "Person.PG1Info" },
            {"PG2 Info", "Person.PG2Info" },

            {"Grade","Attendance.Grade"},
            {"Current Grade","Attendance.Grade"}, // G=K-12

            {"Home","Address.Phone"},
            {"H. Phone", "Address.Phone"},

            {"Cell","Person.CellPhone"},

            {"EMail","Person.EMail"},
            {"Church","Person.Church"},
            {"Home Church","Person.Church"},
            {"Gender","Person.Gender"},
            {"Room","Person.Gender"},
            {"Baptized","Person.Status & Baptized"}, // (Person.Status |= Baptized) & ~NotBaptized

            {"FC","Event.TypeFC"}, // R=Registered and Attended
            {"First Chance","Event.TypeFC"}, // R=Registered and Attended
            {"EL","Event.TypeEL"}, // R=Registered and Attended
            {"Elem","Event.TypeEL"}, // R=Registered and Attended
            {"Elementary","Event.TypeEL"}, // R=Registered and Attended
            {"IN","Event.TypeIN"}, // R=Registered and Attended
            {"Inter","Event.TypeIN"}, // R=Registered and Attended
            {"Intermediate","Event.TypeIN"}, // R=Registered and Attended
            {"SN","Event.TypeSN"}, // R=Registered and Attended
            {"Sen","Event.TypeSN"}, // R=Registered and Attended
            {"Senior","Event.TypeSN"}, // R=Registered and Attended
            {"SR","Event.TypeSPR"}, // R=Registered and Attended
            {"Spring","Event.TypeSPR"}, // R=Registered and Attended
            {"YR","Event.TypeYAR"}, // R=Registered and Attended
            {"TR","Event.TypeBWAC"}, // R=Registered and Attended
            {"BWAC","Event.TypeBWAC"}, // R=Registered and Attended
            {"Fall","Event.TypeBWAC"}, // R=Registered and Attended
            {"Fall Retreat","Event.TypeBWAC"}, // R=Registered and Attended
            {"WOM","Event.TypeWOM"}, // R=Registered and Attended
            {"Womans Retreat","Event.TypeWOM"}, // R=Registered and Attended
            {"MEN","Event.TypeMEN"}, // R=Registered and Attended
            {"FAM","Event.TypeFAM"}, // R=Registered and Attended
            {"VOE","Event.TypeVOE"}, // R=Registered and Attended
            {"Banq","Event.TypeBANQ"}, // R=Registered and Attended
            {"WINT","Event.TypeWINT"}, // R=Registered and Attended
            {"BkC","Operation.TypeBkC"}, // Background check Y or N
            {"BkgC","Operation.TypeBkC"}, // Background check Y or N
            {"BG","Operation.TypeBkC"}, // Background check Y or N
            {"Background Check","Operation.TypeBkC"}, // Background check Y or N
            {"Staff","Person.SkillSet"}, // One or more single letters for tasks done that year :  SDHTCM

            {"Notes", "Attendance.Detail_NoteA"},
        };

        static Dictionary<string, string> ExportPFullInfoFields = new Dictionary<string, string>   //         {  {"entry", "entries"}, ... };
        {
            {"Person.NameLF","LFName"},
            {"Person.FirstName","FName"},
            {"Person.LastName","LName"},
            {"Person.Gender","Gender"},
            {"Person.Church","Church"},
            {"Person.DOB","DOB"},
            {"Address.Street","Street"},
            {"Address.State","State"},
            {"Address.Town","City"},
            {"Address.ZipCode","Zip"},
            {"Address.Phone","HPhone"},
            {"Person.CellPhone","Cell"},
            {"Person.SSNum","SSNum"},
            {"Person.EMail","EMail"},
            {"Person.PGFirst1","PGFirst1"},
            {"Person.PGFirst2","PGFirst2"},
            {"Person.PGLast1","PGLast1"},
            {"Person.PGFirst12","PGFirst12"},
            {"Person.PGFirstLast1", "PGFirstLast1"},
            {"Person.PGFirstLast2","PG2_id"},
            {"Person.Note","CMT"},
            {"Person.Status & Baptized","Baptized"},
            {"Person.SkillSet","Skills"},
            {"Event.Type", "EventType"},

            {"Attendance.Grade", "AttGrade"},
            {"Attendance.StartDT", "AttStart"},
            {"Attendance.EndDT", "AttEnd"},
            {"Attendance.Role_Type", "AttSRole"},

            {"Person.PG1Info", "PG1_Info"},
            {"Person.PG2Info", "PG2_Info"},
            {"Event.TypeFC", "EventType=First_Chance_Retreat"},
            {"Event.TypeEL", "EventType=Elementary_Week"},
            {"Event.TypeIN", "EventType=Intermediate_Week"},
            {"Event.TypeSN", "EventType=Senior_Week"},
            {"Event.TypeSPR", "EventType=Spring_Work_Retreat"},
            {"Event.TypeBWAC", "EventType=BWAC_Fall_Retreat"},
            {"Event.TypeMR", "EventType=Mens_Retreat"},
            {"Event.TypeYAR", "EventType=Young_Adult_Retreat"},
            {"Event.TypeWOM", "EventType=Womans_Retreat"},
            {"Event.TypeMEN", "EventType=Mens_Retreat"},
            {"Event.TypeWINT", "EventType=Winter_Retreat"},
            {"Event.TypeVOE", "EventType=VOE_Week"},
            {"Event.TypeFAM", "EventType=Family_Retreat"},
            {"Event.TypeBANQ", "EventType=Banquet"},
        };

        public static String[] GetResFields(String[] rawFields)
        {
            String[] resFields = new String[rawFields.Count()];
            String res;
            for (int i = 0; i < rawFields.Count(); ++i)
            {
                rawFields[i] = rawFields[i].Trim();
                if (rawFields[i].Length == 0)
                {
                    resFields[i] = "Unk";
                    continue;
                }
                if (ImportFields.TryGetValue(rawFields[i], out res))
                    resFields[i] = res;
                else
                {
                    if (ImportFields.TryGetValue(rawFields[i].ToUpper(), out res))
                        resFields[i] = res;
                    else
                    {
                        // Try to find a close match
                        var key = ImportFields.Select(e => e.Key.ToUpper()).Where(k => (k.StartsWith(rawFields[i].ToUpper())) || (rawFields[i].ToUpper().StartsWith(k))).FirstOrDefault();
                        if (key != null)
                        {
                            var comparer = StringComparison.OrdinalIgnoreCase;
                            resFields[i] = ImportFields.FirstOrDefault(e => String.Equals(e.Key, key, comparer)).Value ?? "Unk";
                        }
                        else
                            resFields[i] = "Unk";
                    }
                }
            }
            return resFields;
        }

        async Task<int> GetEventID(String DescStr)
        {
            String found;
            try
            {
                String url = "";
                if (DescStr != null)
                {
                    if (!DescStr.Contains("year="))
                    {
                        int yindex = DescStr.IndexOf("20");
                        if (yindex == -1)
                            yindex = DescStr.IndexOf("21");
                        if (yindex != -1)
                        {
                            String ystr = DescStr.Substring(yindex, 4);
                            DescStr = DescStr.Replace(ystr, "").Trim();
                            DescStr = "year=" + ystr + "; type=" + DescStr;
                        }
                        else
                            DescStr = "type=" + DescStr;
                    }
                    url = "http://" + serverName + "/api/values/Matching_Events:" + DescStr;
                }
                found = await Get(url);
            }
            catch
            {
                StatusTBox.Text = "Failed to find Events (DB accessible?)";
                return 0;
            }

            var found2 = JsonConvert.DeserializeObject(found);
            List<EInfo> elist = JsonConvert.DeserializeObject<List<EInfo>>(found2.ToString());

            if (elist.Count != 1)
                return -1;
            else
                return elist[0].id;
        }

        public async Task<String> Get(string url)
        {
            //init the class
            using (HttpClient hc = new HttpClient())
            {
                //call the get method to get response
                return await hc.GetStringAsync(new Uri(url));
            }
        }

        public async Task<String> Put(string website, PFullInfo pfInfo)
        {
            //init client
            using (HttpClient hc = new HttpClient())
            {
                //send post request

                /***************************************
                                $("#divStatus_id").html("Waiting for Server...");
                                $.ajax({
                                    type: "PUT",
                                    crossDomain: true,
                                    data: raw,
                                    dataType: 'json',
                                    url: url,
                                    success: function(result)
                                    {

                Serialization:
                ==============
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var text = JsonConvert.SerializeObject(configuration, settings);

                Deserialization:
                ================
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var configuration = JsonConvert.DeserializeObject<YourClass>(json, settings);

                ****************************************/
                DataContractJsonSerializer jSer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(Raw));

                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = JsonConvert.SerializeObject(pfInfo, settings);

                var raw = new CCAAttendance.Raw();
                raw.type = "Person2";
                raw.data = json;

                // use the serializer to write the object to a MemoryStream 
                MemoryStream ms = new MemoryStream();
                jSer.WriteObject(ms, raw);
                ms.Position = 0;

                //use a Stream reader to construct the StringContent (Json) 
                StreamReader sr = new StreamReader(ms);
                StringContent content = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");

                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage hrm = await hc.PutAsync(website, content);

                //show response
                String resp = await hrm.Content.ReadAsStringAsync();
                //if (resp.ToLower().Contains("success"))
                //    ClearForm();

                return resp;
            }
        }

        public async Task<String> Put(string website, AttData afInfo)
        {
            //init client
            using (HttpClient hc = new HttpClient())
            {
                //send post request

                /***************************************
                                $("#divStatus_id").html("Waiting for Server...");
                                $.ajax({
                                    type: "PUT",
                                    crossDomain: true,
                                    data: raw,
                                    dataType: 'json',
                                    url: url,
                                    success: function(result)
                                    {

                Serialization:
                ==============
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var text = JsonConvert.SerializeObject(configuration, settings);

                Deserialization:
                ================
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var configuration = JsonConvert.DeserializeObject<YourClass>(json, settings);

                ****************************************/
                DataContractJsonSerializer jSer = new DataContractJsonSerializer(typeof(Raw));

                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var json = JsonConvert.SerializeObject(afInfo, settings);

                var raw = new CCAAttendance.Raw();
                raw.type = "Attendance";
                raw.data = json;

                // use the serializer to write the object to a MemoryStream 
                MemoryStream ms = new MemoryStream();
                jSer.WriteObject(ms, raw);
                ms.Position = 0;

                //use a Stream reader to construct the StringContent (Json) 
                StreamReader sr = new StreamReader(ms);
                StringContent content = new StringContent(sr.ReadToEnd(), System.Text.Encoding.UTF8, "application/json");

                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage hrm = await hc.PutAsync(website, content);

                //show response
                String resp = await hrm.Content.ReadAsStringAsync();
                //if (resp.ToLower().Contains("success"))
                //    ClearForm();

                return resp;
            }
        }

        public static String[] SplitDQ(String src, char seps)
        {
            String[] sarr = src.Split(seps);

            List<String> slist = new List<String>();
            String fld, comb = "";
            bool bInDQ = false;

            foreach (String str in sarr)
            {
                fld = str.Trim();
                if (!bInDQ)
                {
                    if (fld.StartsWith("\""))
                    {
                        bInDQ = true;
                        comb = fld.Substring(1);
                    }
                    else
                        slist.Add(fld);
                }
                else
                {
                    // bInDQ
                    comb = comb + "," + fld;

                    if (fld.EndsWith("\""))
                    {
                        comb = comb.Substring(0, comb.Length - 1);
                        bInDQ = false;
                        slist.Add(comb);
                    }
                }
            }
            if (bInDQ)
                slist.Add(comb);

            return slist.ToArray();
        }

        public async void Import(string fileToImport)
        {
            // Select a File
            Stream stream = null;
            OpenCSVFileDlg.InitialDirectory = @"c:\cca\attendance\csv";
            OpenCSVFileDlg.Filter = "Excel (*.csv)|*.csv|Sign-in Log Sheet(*.cls)|*.cls|All files (*.*)|*.*";
            OpenCSVFileDlg.FilterIndex = 1;
            OpenCSVFileDlg.RestoreDirectory = true;
            int NumRecordsImported = 0;
            AttLB.Items.Clear();
            String url = "http://" + _serverName + "/api/CCA";

            if (OpenCSVFileDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    //********************************************************
                    // Check for Sign-In Sheet : Format YYYY_LogSheet.cls
                    //********************************************************
                    if (OpenCSVFileDlg.FileName.EndsWith(".cls", true, CultureInfo.CurrentCulture))
                    {
                        int year = 0;
                        if (OpenCSVFileDlg.SafeFileName.EndsWith("_LogSheet.cls", true, CultureInfo.CurrentCulture) && OpenCSVFileDlg.SafeFileName.StartsWith("2", true, CultureInfo.CurrentCulture))
                            Int32.TryParse(OpenCSVFileDlg.SafeFileName.Substring(0, 4), out year);
                        if (year == 0)
                        {
                            MessageBox.Show("Bad CCA Log (Sign-In) Sheet File name.\n File Name needs to be YYYY_LogSheet.cls");
                            return;
                        }

                        int seid = await GetEventID(year + " SignIn Sheet");
                        if (seid <= 0)
                        {
                            MessageBox.Show(@"Please create an Event : """ + year + @" SignIn Sheet"" first before importing");
                            return;
                        }

                        AttData att = new AttData();
                        att.eid = seid;

                        // Parse Log file
                        if ((stream = OpenCSVFileDlg.OpenFile()) != null)
                        {
                            using (stream)
                            {
                                /*************************************************************
                                                 C=Camper           public class Logger
                                                 D=Drop off         {
                                                 P=Pick up             String name = "";
                                                 S=Staff               int pid = 0;
                                                 V=Visitor             String purpose = "";
                                                                       DateTime timeIn = new DateTime(1, 1, 1);
                                                                       DateTime timeOut = new DateTime(1, 1, 1);
                                                                    }                       
                                ***********************************************************************/
                                using (var sr = new StreamReader(stream, Encoding.UTF8))
                                {
                                    String line;
                                    List<Logger> LogList = new List<Logger>();
                                    while ((line = sr.ReadLine()) != null)
                                    {
                                        att.pid = -1;
                                        if (line.Length < 16)
                                            continue;

                                        // Parse each line and add to dictionary and update state when appropriate.
                                        String[] fields = line.Split('~');

                                        //if (!fields[1].Contains("briana e")) // Debug Lines : Comment out these 2 lines
                                        //    continue;

                                        if (fields.Count() >= 5)
                                        {
                                            String cmd = fields[0].ToUpper();
                                            switch (cmd)
                                            {
                                                // CMD~Name~ID~Purpose~Time in
                                                // SIGNIN~Charles Galartza~[-1]~S = Staff~7 / 15 / 17 3:32 PM
                                                case "SIGNIN":
                                                    {
                                                        Logger log = new Logger();
                                                        log.name = fields[1].Trim();
                                                        log.id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        log.purpose = fields[3].Trim();
                                                        log.timeIn = DateTime.Parse(fields[4].Trim());
                                                        LogList.Add(log);
                                                    }
                                                    break;

                                                // CMD~Name~id~Purpose~TIME - IN~TIME - OUT
                                                // SIGNOUT~Brianna Barbaro~[-1]~V = Visitor~7 / 12 / 17 5:25 PM~7 / 14 / 17 1:45 PM
                                                case "SIGNOUT":
                                                    {
                                                        String name = fields[1].Trim();
                                                        int id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        String purpose = fields[3].Trim();
                                                        DateTime timeIn = DateTime.Parse(fields[4].Trim());

                                                        int NumLoggers = LogList.Count();
                                                        for (int i = NumLoggers - 1; i >= 0; --i)
                                                        {
                                                            if ((LogList[i].name == name) && (LogList[i].id == id) && ((LogList[i].timeIn == timeIn) || LogList[i].FromUnSignOut) && (LogList[i].purpose == purpose) && !LogList[i].timeOut.HasValue)
                                                            {
                                                                LogList[i].timeOut = DateTime.Parse(fields[5].Trim());
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }

                                                // CMD~Name~id~Purpose~NewOut~CurTime
                                                // UNSIGNOUT~Brianna Barbaro~[-1]~V = Visitor~7 / 12 / 17 5:25 PM~- - -~7 / 14 / 17 1:45 PM
                                                case "UNSIGNOUT":
                                                    {
                                                        Logger log = new Logger();
                                                        log.name = fields[1].Trim();
                                                        log.id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        log.purpose = fields[3].Trim();
                                                        log.timeIn = DateTime.Parse(fields[6].Trim());
                                                        log.FromUnSignOut = true;
                                                        LogList.Add(log);
                                                    }
                                                    break;

                                                case "DELETE":
                                                    {
                                                        String name = fields[1].Trim();
                                                        int id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        String purpose = fields[3].Trim();
                                                        DateTime timeIn = DateTime.Parse(fields[4].Trim());
                                                        int NumLoggers = LogList.Count();
                                                        for (int i = NumLoggers - 1; i >= 0; --i)
                                                        {
                                                            if ((LogList[i].name == name) && (LogList[i].id == id) && (LogList[i].timeIn == timeIn) && (LogList[i].purpose == purpose))
                                                            {
                                                                LogList.Remove(LogList[i]);
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }

                                                // CMD~OldName~ID~Purpose~Time in~time out~New Name
                                                // EDITNAME~Charles Galartza~[-1]~S = Staff~7 / 15 / 17 3:32 PM~- - -~Charles Galarza
                                                case "EDITNAME":
                                                    {
                                                        String name = fields[1].Trim();
                                                        int id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        String purpose = fields[3].Trim();
                                                        DateTime timeIn = DateTime.Parse(fields[4].Trim());
                                                        int NumLoggers = LogList.Count();
                                                        for (int i = NumLoggers - 1; i >= 0; --i)
                                                        {
                                                            if ((LogList[i].name == name) && (LogList[i].id == id) && ((LogList[i].timeIn == timeIn) || LogList[i].FromUnSignOut) && (LogList[i].purpose == purpose))
                                                            {
                                                                LogList[i].name = fields[6].Trim();
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }

                                                // CMD~Name~id~Old Time In~Time out~New Time in
                                                // EDITIMEIN~Ron Ripperger~[-1]~D = Drop off~7 / 29 / 17 4:59 PM~- - -~7 / 29 / 17 4:00 PM
                                                case "EDITIMEIN":
                                                    {
                                                        String name = fields[1].Trim();
                                                        int id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        String purpose = fields[3].Trim();
                                                        DateTime timeIn = DateTime.Parse(fields[4].Trim());
                                                        int NumLoggers = LogList.Count();
                                                        for (int i = NumLoggers - 1; i >= 0; --i)
                                                        {
                                                            if ((LogList[i].name == name) && (LogList[i].id == id) && ((LogList[i].timeIn == timeIn) || LogList[i].FromUnSignOut) && (LogList[i].purpose == purpose))
                                                            {
                                                                LogList[i].timeIn = DateTime.Parse(fields[6].Trim());
                                                                LogList[i].HasEditTimeIn = true;
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }

                                                // CMD~Name~id~Old Purpose~Time in ~Time out~New Purpose
                                                // EDITPURPOSE~briana edgar~[-1]~V = Visitor~7 / 9 / 17 3:22 PM~- - -~S = Staff
                                                case "EDITPURPOSE":
                                                    {
                                                        String name = fields[1].Trim();
                                                        int id = int.Parse(fields[2].Trim().Trim(new char[] { '[', ']' }));
                                                        String purpose = fields[3].Trim();
                                                        DateTime timeIn = DateTime.Parse(fields[4].Trim());
                                                        int NumLoggers = LogList.Count();
                                                        for (int i = NumLoggers - 1; i >= 0; --i)
                                                        {
                                                            if ((LogList[i].name == name) && (LogList[i].id == id) && ((LogList[i].timeIn == timeIn) || LogList[i].FromUnSignOut) && (LogList[i].purpose == purpose))
                                                            {
                                                                LogList[i].purpose = fields[6].Trim();
                                                                break;
                                                            }
                                                        }
                                                        break;
                                                    }

                                                case "EDITTIMEOUT": // Not supported. Replaced by UNSIGNOUT
                                                    break;
                                            }
                                        }
                                    }

                                    // Report Import of Log File :
                                    foreach (Logger l in LogList)
                                    {
                                        // Get Person ID
                                        PFullInfo pf = new PFullInfo();
                                        int pid = -1;
                                        String[] narr = l.name.Split(' ');
                                        if (narr.Count() >= 2)
                                        {
                                            pf.FName = narr[0];
                                            pf.LName = narr[narr.Count() - 1];
                                            String res = await Put(url, pf);

                                            AttData afi = new AttData();
                                            //GET pid from StatusTBox.Text : "Update/Add Succeeded:[1568]"
                                            int soff = res.LastIndexOf('[');

                                            if (soff != -1)
                                            {
                                                int eoff = res.LastIndexOf(']');
                                                if (eoff != -1)
                                                {
                                                    String pidStr = res.Substring(soff + 1, (eoff - soff) - 1);
                                                    Int32.TryParse(pidStr, out pid);
                                                }
                                            }
                                        }

                                        if (pid != -1)
                                        {
                                            //******************************
                                            //*** ADD SIGN-IN ATTENDANCE ***
                                            //******************************
                                            att.pid = pid;
                                            if (l.timeIn.HasValue)
                                                att.StartDT = l.timeIn;
                                            else
                                                att.StartDT = null;
                                            if (l.timeOut.HasValue)
                                                att.EndDT = l.timeOut;
                                            else
                                                att.EndDT = null;

                                            String purpose = l.purpose.Trim(); // C= Camper, D=Drop off, P=Pick up,  S=Staff,  V=Visitor,
                                            if (purpose.StartsWith("C="))
                                                att.role = (int)AttendanceRoles.Camper;
                                            else if (purpose.StartsWith("D="))
                                                att.role = (int)AttendanceRoles.DropOff;
                                            else if (purpose.StartsWith("P="))
                                                att.role = (int)AttendanceRoles.PickUp;
                                            else if (purpose.StartsWith("S="))
                                                att.role = (int)AttendanceRoles.Staff;
                                            else if (purpose.StartsWith("V="))
                                                att.role = (int)AttendanceRoles.Visitor;
                                            else
                                                att.role = (int)AttendanceRoles.Unknown;
                                            String AttRes = await Put(url, att);

                                            if (l.name.Length > 14)
                                                AttLB.Items.Add(l.name + " \t[" + pid + "]\t " + l.purpose + " \t" + l.timeIn.ToString().Replace(":00 ", " ") + "-->" + l.timeOut.ToString().Replace(":00 ", " "));
                                            else
                                                AttLB.Items.Add(l.name + " \t\t[" + pid + "]\t " + l.purpose + " \t" + l.timeIn.ToString().Replace(":00 ", " ") + "-->" + l.timeOut.ToString().Replace(":00 ", " "));
                                        }
                                    }
                                }
                            }
                        }
                        return;
                    }

                    //*************************************
                    // EVENT ATTENDANCE INPUT *************
                    //*************************************
                    //OpenCSVFileDlg.FileName = @"C:\CCA\Attendance\csv\2011_Campers.csv"; // TEMP!!!!!!

                    // Try to determine the common event type and year from the filename itself and Attendance Event edit text field 
                    int EventID, EventYear = -1, EventYearF = -1, EventYearP = -1;
                    EventType eventType, eventTypeF, eventTypeP;
                    eventType = eventTypeF = eventTypeP = EventType.Other;
                    int eid = GetCurrentEventID(out EventYearF, out eventTypeF, System.IO.Path.GetFileName(OpenCSVFileDlg.FileName));
                    if (eid != -1)
                        EventID = eid;
                    else
                    {
                        if ((eventTypeF == EventType.Banquet) || (eventTypeF == EventType.Fund_Raiser) || (eventTypeF == EventType.Other) || EventYearF == -1)
                            eid = GetCurrentEventID(out EventYearP, out eventTypeP, System.IO.Path.GetDirectoryName(OpenCSVFileDlg.FileName));

                        if ((eventTypeF == EventType.Banquet) || (eventTypeF == EventType.Fund_Raiser) || (eventTypeF == EventType.Other))
                        {
                            if ((eventTypeP == EventType.Banquet) || (eventTypeP == EventType.Fund_Raiser) || (eventTypeP == EventType.Other))
                            {
                                if ((eventTypeP == EventType.Other))
                                    eventType = eventTypeF;
                                else
                                    eventType = eventTypeP;
                            }
                            else
                                eventType = eventTypeP;
                        }
                        else
                            eventType = eventTypeF;

                        if (EventYearF != -1)
                            EventYear = EventYearF;
                        else
                            EventYear = EventYearP;

                        if (eventType != EventType.Other)
                        {
                            String hints = "year=" + EventYear.ToString() + "; type=" + eventType.ToString();
                            EventID = await GetEventID(hints);
                        }
                        else
                            EventID = -1;
                    }

                    if ((stream = OpenCSVFileDlg.OpenFile()) != null)
                    {
                        using (stream)
                        {
                            var AList = new List<string>();
                            using (var sr = new StreamReader(stream, System.Text.Encoding.UTF8))
                            {
                                string hdr;
                                hdr = sr.ReadLine();
                                if (hdr.Length == 0)
                                {
                                    MessageBox.Show("Import Failed : No content");
                                    return;
                                }

                                while (!hdr.ToUpper().Contains("NAME") && !hdr.ToUpper().Contains("LAST"))
                                {
                                    hdr = sr.ReadLine();
                                }

                                String[] rawFields = hdr.Split(',');
                                if (rawFields.Count() == 0)
                                {
                                    MessageBox.Show("Import Failed : No content");
                                    return;
                                }

                                String[] resFields = Form1.GetResFields(rawFields);

                                //**************************************
                                // *** FOR EACH LINE IN THE FILE *******
                                //**************************************
                                String line;
                                while ((line = sr.ReadLine()) != null)
                                {
                                    List<String> ValList = new List<String>();

                                    String[] values = SplitDQ(line, ',');

                                    if (values.Count() == resFields.Count())
                                    {
                                        // Create PersonData, AddressData, EventData, and AttendanceData
                                        PFullInfo pf = new PFullInfo();
                                        List<int> PFRole = new List<int>();
                                        String prop;
                                        for (int i = 0; i < resFields.Count(); ++i)
                                        {
                                            if (ExportPFullInfoFields.TryGetValue(resFields[i], out prop))
                                            {
                                                PropertyInfo pi = pf.GetType().GetProperty(prop);
                                                if (pi != null)
                                                    pi.SetValue(pf, values[i]);
                                                else
                                                {
                                                    if ((values[i].IndexOfAny("YyTt1Ss".ToArray()) != -1) && prop.StartsWith("EventType="))
                                                    {
                                                        if ((pf._EventType.Length == 0) || String.Equals(pf._EventType, "Other", StringComparison.InvariantCultureIgnoreCase))
                                                            pf._EventType = prop.Substring(10);
                                                        else
                                                            pf._EventType = String.Join(",", pf._EventType, prop.Substring(10)); // Allow multiple events
                                                        pf.AttRole = (int)AttendanceRoles.Camper; // Needed?

                                                        if (values[i].IndexOfAny("Ss".ToArray()) != -1)
                                                            PFRole.Add((int)AttendanceRoles.SWAT);
                                                        else
                                                            PFRole.Add((int)AttendanceRoles.Camper);
                                                    }
                                                }
                                            }
                                        }

                                        if ((pf.LName != null) && (pf.FName != null) && (pf.LName.Length > 0) && (pf.FName.Length > 0))
                                        {
                                            StatusTBox.Text = await Put(url, pf);
                                            AttLB.Items.Add(line.Trim() + StatusTBox.Text);
                                            //TBD HAndle multiple events from ATT record from line
                                            AttData afi = new AttData();
                                            //GET pid from StatusTBox.Text : "Update/Add Succeeded:[1568]"
                                            int soff = StatusTBox.Text.LastIndexOf('[');
                                            bool bHasPID = false;
                                            int pid = -1;
                                            if (soff != -1)
                                            {
                                                int eoff = StatusTBox.Text.LastIndexOf(']');
                                                if (eoff != -1)
                                                {
                                                    String pidStr = StatusTBox.Text.Substring(soff + 1, (eoff - soff) - 1);
                                                    bHasPID = Int32.TryParse(pidStr, out pid);
                                                }
                                            }

                                            if (bHasPID)
                                            {
                                                afi.pid = pid;

                                                int curGrade;
                                                if (Int32.TryParse(pf.AttGrade, out curGrade))
                                                {
                                                    afi.CurGrade = curGrade;
                                                    afi.RegForm = "?";
                                                }

                                                // For each Event Type 
                                                String[] eventList = pf.EventType.Split(',');
                                                int i = -1;
                                                foreach (var eventTypeStr in eventList)
                                                {
                                                    ++i;
                                                    if ((EventYear != -1) && (eventTypeStr.Length > 0) && (eventTypeStr != "Other"))
                                                    {
                                                        String hints = "year=" + EventYear.ToString() + "; type=" + eventTypeStr;
                                                        int lEventID = await GetEventID(hints);

                                                        if (lEventID != -1)
                                                        {
                                                            // Add Attendance
                                                            afi.eid = lEventID;
                                                            afi.role = PFRole[i];
#if ADD_TO_DB
                                                            await Put(url, afi);
#endif
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show($"ERROR. IMPORT ABORTED : No Event found for : [{hints}]");
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (EventID != -1)
                                                        {
                                                            //Add Attendance
                                                            afi.eid = EventID;
                                                            afi.role = PFRole[i];
#if ADD_TO_DB
                                                            await Put(url, afi);
#endif
                                                        }
                                                        else
                                                        {
                                                            MessageBox.Show($"ERROR. IMPORT ABORTED : No Event found for File : {OpenCSVFileDlg.FileName}");
                                                            return;
                                                        }
                                                    }
                                                }
                                            }

                                            String full = pf.ToString();
                                            AttLB.Items.Add(full.Trim());

                                            AttLB.Update();
                                            ++NumRecordsImported;
                                        }
                                    }
                                    else
                                    {
                                        StatusTBox.Text = ("# Property Names[" + resFields.Count() + "] differs from # Values[" + values.Count() + "] in " + line);
                                        MessageBox.Show(StatusTBox.Text);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusTBox.Text = ex.Message + "Campers imported = " + NumRecordsImported;
                    MessageBox.Show("Error:" + ex.Message);
                }
            }
            StatusTBox.Text = "Campers imported = " + NumRecordsImported;
            MessageBox.Show(StatusTBox.Text);
        }

    }
}
