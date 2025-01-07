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
    public class RouterController : ControllerBase
    {
        private readonly IWebHostEnvironment HostEnv;
        private readonly IConfiguration Config;
        private readonly UserManager<CStatUser> UserManager;

        public RouterController(IWebHostEnvironment hostEnv, IConfiguration config, UserManager<CStatUser> userManager)
        {
            HostEnv = hostEnv;
            Config = config;
            UserManager = userManager;
        }

        [HttpPost]
        public HttpResponseMessage Post([FromBody] string raw)
        {
            HttpStatusCode hsCode;
            String RespMsg;
            try
            {
                if (raw.Length > 100)
                {
                    ClientReport[] clients = ClientReport.GetClientsFromJson(raw);
                    ClientReport.SetClients(clients);
                    hsCode = (clients != null) ? HttpStatusCode.OK : HttpStatusCode.NotAcceptable;
                }
                else
                    hsCode = HttpStatusCode.NotAcceptable;
                RespMsg = "Response from CStat Router Post";
            }
            catch (Exception e)
            {
                hsCode = HttpStatusCode.InternalServerError;
                RespMsg = e.Message;
            }

            var response = new HttpResponseMessage(hsCode)
            {
                Content = new StringContent(RespMsg, System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = hsCode
            };
            return response;

        }
    }

}


