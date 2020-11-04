using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using CStat.Models;
using CCAEvents2;
using CCAChurch;
using CCAAttendance;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace CStat.Controllers
{
    public class Raw
    {
        public string type { get; set; }
        public string data { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class CCAController : ControllerBase
    {
        private CStatContext entities;

        public CCAController(CStatContext csc)
        {
            entities = csc;
        }

        // PUT api/cca/5
        [HttpPut]
        [Obsolete]
        public String Put(Raw raw)
        {
            bool bAllowNoAddress;
            if ((bAllowNoAddress = (raw.type == "Person2")) && (raw.data != null))
            {
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var data = JsonConvert.DeserializeObject<Attendance.PFullInfo>(raw.data, settings);
                raw.type = "Person";
                raw.data = data.ToString();
            }

            if (raw.type == "PersonANA")
            {
                bAllowNoAddress = true;
                raw.type = "Person";
            }

            if (((raw.type == "Person") || (raw.type == "DeletePerson")) && (raw.data != null))
            {
                // Generate Key/Value Pairs to update person
                char[] outerDelims = { ';', '~', '&', ',' };
                char[] innerDelims = { ':', '=' };

                string[] pairs = raw.data.Split(outerDelims);

                List<KeyValuePair<string, string>> props = new List<KeyValuePair<string, string>>();
                foreach (string nvs in pairs)
                {
                    string[] nvp = nvs.Split(innerDelims);
                    if ((nvp.Length == 2) && (nvp[1].Length > 0))
                    {
                        props.Add(new KeyValuePair<string, string>(nvp[0], nvp[1]));
                    }
                }

                PersonMgr pmgr = new PersonMgr(entities);

                if (raw.type == "DeletePerson")
                    return pmgr.DeletePerson(ref props);

                Person rp;
                Address ra;

                MgrStatus result = pmgr.Update(ref props, false, bAllowNoAddress, out rp, out ra);

                if (result == MgrStatus.Add_Update_Succeeded)
                {
                    if ((rp != null) && (rp.Id > 0))
                        return "Update/Add Succeeded:" + "[" + rp.Id + "]";
                    else
                        return "Update/Add Succeeded.";
                }
                else
                    return "FAIL:" + result.ToString();
            }
            else if (raw.type == "Event")
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
                };

                var ev = JsonConvert.DeserializeObject<Event.EventData>(raw.data, settings);
                Event.EventData ed = (Event.EventData)ev;
                EventMgr emgr = new EventMgr(entities);
                MgrStatus ms = emgr.Update(ed);
                return "Event " + raw.type + " " + ms.ToString();
            }
            else if (raw.type == "Church")
            {
                //var settings = new JsonSerializerSettings
                //{
                //    TypeNameHandling = TypeNameHandling.Objects,
                //    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
                //};

                var cv = JsonConvert.DeserializeObject<Church.ChurchData>(raw.data);
                Church.ChurchData cd = (Church.ChurchData)cv;
                ChurchMgr cmgr = new ChurchMgr(entities);
                MgrStatus ms = cmgr.Update(cd);
                return ms.ToString();
            }
            else if (raw.type == "Attendance")
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects,
                    TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Full
                };

                var atd = JsonConvert.DeserializeObject<Attendance.AttData>(raw.data, settings);

                AttendanceMgr amgr = new AttendanceMgr(entities);
                String resultsStr = "";
                MgrStatus ams = amgr.Update(atd);
                if (resultsStr.Length > 0)
                    resultsStr = String.Join(",", resultsStr, ams.ToString());
                else
                    resultsStr = ams.ToString();

                return resultsStr;
            }

            return "FAIL: Type:" + raw.type + " Not Handled.";
        }
    }
}