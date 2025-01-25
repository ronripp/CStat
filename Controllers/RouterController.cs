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
        private readonly IWebHostEnvironment _hostEnv;
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private readonly CStat.Models.CStatContext _context;

        public RouterController(IWebHostEnvironment hostEnv, IConfiguration config, CStat.Models.CStatContext context, UserManager<CStatUser> userManager)
        {
            _hostEnv = hostEnv;
            _config = config;
            _userManager = userManager;
            _context = context;
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

   
                    RConnMgr rcmgr = new RConnMgr(_hostEnv, _context);
                    rcmgr.Update(clients);
                    rcmgr.Close(out string newClient);
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


