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
using System.Threading;

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
            return new string[] { " Not Implemented." };
        }

        // GET api/Equip/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "Not Implemented";
        }

        // POST api/Equip
        [HttpPost]
        public HttpResponseMessage Post([FromBody] string raw)
        {
            ArdMgr ardMgr = new ArdMgr(hostEnv);
            HttpStatusCode hsCode = (ardMgr.Set(raw) > 0) ? HttpStatusCode.OK : HttpStatusCode.NotAcceptable;
            var response = new HttpResponseMessage(hsCode)
            {
                Content = new StringContent("Response from CStat PUT", System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = hsCode
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
    }
}
