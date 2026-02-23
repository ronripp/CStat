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
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using CStat.Areas.Identity.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CStat.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AlertController : ControllerBase
    {
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;

        public AlertController (IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
        }

        // GET: api/Alert
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { " Not Implemented." };
        }

        // GET api/Alert/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "Not Implemented";
        }

        // POST api/Alert
        [HttpPost]
        public HttpResponseMessage Post([FromBody] string raw)
        {
            CSSMS sms = new CSSMS(HostEnv, Config, UserManager);
            sms.SendMessage("203-770-3393", "Alert Test :" + raw, true, false);
            HttpStatusCode hsCode = HttpStatusCode.OK;
            var response = new HttpResponseMessage(hsCode)
            {
                Content = new StringContent("Response from CStat POST", System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = hsCode
            };
            return response;
        }

        // PUT api/Alert/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/Alert/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

    }
}
