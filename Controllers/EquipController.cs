using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using CStat.Common;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CStat.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class EquipController : ControllerBase
    {
        IWebHostEnvironment hostEnv;

        public EquipController (IWebHostEnvironment hstEnv)
        {
            hostEnv = hstEnv;
        }

        // GET: api/Equip
        [HttpGet]
        public IEnumerable<string> Get()
        {
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "tmpDBox"); // Test
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            string fullPath = Path.Combine(newPath, "EquipPost.txt");
            if (System.IO.File.Exists(fullPath))
            {
                FileInfo info = new FileInfo(fullPath);
                return new string[] { "Post Size = " + info.Length.ToString(), "AAA" };
            }
            return new string[] { fullPath, " Not Found." };
        }

        // GET api/Equip/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            PropaneLevel pl = null;
            var vals = EquipController.GetValues("https://data.tankutility.com/api/getToken", "ronripp@outlook.com", "Red3581!", "token");

            string token = vals["token"];
            if (token != null)
            {
                var readings = EquipController.GetValues("https://data.tankutility.com/api/devices/003a0028363537330b473931?token=" + token, null, null, "device.lastReading.tank", "device.lastReading.temperature", "device.lastReading.time_iso");

                double level;
                double temp;
                DateTime readTime;
                if (double.TryParse(readings["device.lastReading.tank"], out level) && DateTime.TryParse(readings["device.lastReading.time_iso"], out readTime) && double.TryParse(readings["device.lastReading.temperature"], out temp))
                {
                    pl = new PropaneLevel(level, readTime, temp);
                }

                string resp = "";

                foreach (var r in readings)
                {
                    resp += r.Key + "=" + r.Value + "\n";
                }
                return resp;
            }

            string webRootPath = hostEnv.WebRootPath;

            String qstr = this.Request.QueryString.Value;
            string newPath = Path.Combine(webRootPath, "tmpDBox");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            string fullPath = Path.Combine(newPath, "EquipPost.txt");
            using (StreamWriter sw = new StreamWriter(fullPath, true))
            {
                sw.WriteLine("Posted from Arduino = " + qstr);
                sw.Close();
            }
            return "Posted for ID=" + id + " Params=" + qstr;
        }

        // POST api/Equip
        [HttpPost]
        public HttpResponseMessage Post([FromBody] string raw)
        {
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, "tmpDBox");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            string fullPath = Path.Combine(newPath, "EquipPost.txt");

            using (StreamWriter sw = new StreamWriter(fullPath, true))
            {
                sw.WriteLine("Value=" + raw);
                sw.Close();
            }
            
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Response from CStat PUT", System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = HttpStatusCode.OK
            };
            return response;
        }

        // PUT api/Equip/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/Equip/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        static PropaneLevel GetTUPropaneLevel() // Tank Utility Propane meter
        {
            var vals = EquipController.GetValues("https://data.tankutility.com/api/getToken", "ronripp@outlook.com", "Red3581!", "token");

            string token = vals["token"];
            if (token == null)
                return null;

            var readings = EquipController.GetValues("https://data.tankutility.com/api/devices/003a0028363537330b473931?token=" + token, null, null, "device.lastReading.tank", "device.lastReading.temperature", "device.lastReading.time_iso");

            double level;
            double temp;
            DateTime readTime;
            if (double.TryParse(readings["device.lastReading.tank"], out level) && DateTime.TryParse(readings["device.lastReading.time_iso"], out readTime) && double.TryParse(readings["device.lastReading.temperature"], out temp))
            {
                return new PropaneLevel(level, readTime, temp);
            }
            return null;
        }

        static Dictionary<string, string> GetValues(string url, string userName, string password, params string[] tokens)
        {
            List<string> names = tokens.ToList();
            var dict = new Dictionary<string, string>();

            WebRequest myReq = WebRequest.Create(url);
            if ((userName != null) && (password != null))
            {
                string credentials = $"{userName}:{password}";
                //CredentialCache mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
            }
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            var json = "[" + content + "]"; // change this to array
            var objects = JArray.Parse(json); // parse as array  
            int numFinds = names.Count();
            foreach (JObject obj in objects.Children<JObject>())
            {
                if (EquipController.TraverseObject(obj, ref numFinds, names, dict))
                    return dict;
            }
            return dict;
        }

        static bool TraverseObject (JObject obj, ref int numFinds, List<string> names, Dictionary<string, string> dict, string objPath="")
        {
            foreach (JProperty p in obj.Properties())
            {
                string fullName = (objPath.Length > 0) ? objPath + "." + p.Name : p.Name;
                var nidx = names.FindIndex(n => (n == fullName));
                if (nidx != -1)
                {
                    dict.Add(fullName, p.Value.ToString());
                    names.RemoveAt(nidx);
                    if (--numFinds == 0)
                        return true;
                }
                if (p.Value.Type == JTokenType.Object)
                {
                    return TraverseObject(p.Value.ToObject<JObject>(), ref numFinds, names, dict, fullName);
                }
            }
            return false;
        }

    }
}
