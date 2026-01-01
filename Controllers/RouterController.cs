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
        //private static int msgCnt = 0;

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
            HttpStatusCode hsCode = HttpStatusCode.OK;
            var response = new HttpResponseMessage(hsCode)
            {
                Content = new StringContent("ok", System.Text.Encoding.UTF8, "text/plain"),
                StatusCode = hsCode
            };
            return response;

            //DISABLED FOR NOW HttpStatusCode hsCode;
            //DISABLED FOR NOW String RespMsg;
            //DISABLED FOR NOW var cl = new CSLogger();
            //DISABLED FOR NOW try
            //DISABLED FOR NOW {
            //DISABLED FOR NOW     //Interlocked.Increment(ref msgCnt);
            //DISABLED FOR NOW     //if ((msgCnt % 100) == 0)
            //DISABLED FOR NOW     //    cl.Log("RouterCont: Post raw length=" + raw.Length + " sample[" + raw + "]");
            //DISABLED FOR NOW 
            //DISABLED FOR NOW     //var raw2 = "[{\"name\":\"Annas-iPhone-2\",\"alias\":\"\",\"mac\":\"74:42:18:86:5A:AF\",\"ip\":\"192.168.1.128\",\"rssi\":-83,\"rx\":12,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:01:05\",\"deviceType\":4,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:04:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"02:0F:B5:27:03:0B\",\"ip\":\"192.168.1.129\",\"rssi\":-80,\"rx\":6,\"tx\":6,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:05:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:00:09.9309938-04:00\"},{\"name\":\"CCASERVER\",\"alias\":\"\",\"mac\":\"98:E7:F4:BB:18:05\",\"ip\":\"192.168.1.15\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":34,\"isConnected\":true,\"vendor\":\"Hewlett Packard\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Shenzhen Baichuan Digital Techn\",\"alias\":\"Camera2\",\"mac\":\"EC:71:DB:22:89:5B\",\"ip\":\"192.168.1.224\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Shenzhen Baichuan Digital Technology Co., Ltd.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Arduino\",\"alias\":\"\",\"mac\":\"A8:40:41:12:4B:78\",\"ip\":\"192.168.1.39\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Dragino Technology Co., Limited\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"EYEDRO-004809AF\",\"alias\":\"\",\"mac\":\"60:54:64:48:09:AF\",\"ip\":\"192.168.1.153\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Eyedro Green Solutions Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Shenzhen Baichuan Digital Techn\",\"alias\":\"Camera1\",\"mac\":\"EC:71:DB:66:6F:41\",\"ip\":\"192.168.1.237\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Shenzhen Baichuan Digital Technology Co., Ltd.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"3A:33:FF:4E:E8:20\",\"ip\":\"192.168.101.248\",\"rssi\":-83,\"rx\":24,\"tx\":58.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"04:58:29\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T18:07:07.9309938-04:00\"},{\"name\":\"Laptop2023\",\"alias\":\"\",\"mac\":\"B4:8C:9D:17:B9:61\",\"ip\":\"192.168.101.180\",\"rssi\":-81,\"rx\":1,\"tx\":14.4,\"dRx\":0,\"dTx\":0,\"connTime\":\"01:47:32\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Microsoft Corp.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:18:10.9309938-04:00\"},{\"name\":\"NPIE8E01C\",\"alias\":\"NPIE8E01C\",\"mac\":\"6C:0B:5E:E8:E0:1C\",\"ip\":\"192.168.1.137\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"EX7000\",\"alias\":\"EX7000\",\"mac\":\"02:0F:B5:FB:CE:6E\",\"ip\":\"192.168.1.250\",\"rssi\":-83,\"rx\":526.5,\"tx\":520,\"dRx\":0,\"dTx\":0,\"connTime\":\"48:58:26\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-27T22:06:23.9309938-04:00\"},{\"name\":\"Betty-s-Galaxy-S10\",\"alias\":\"\",\"mac\":\"26:B2:24:2D:40:C5\",\"ip\":\"192.168.101.221\",\"rssi\":-90,\"rx\":1,\"tx\":43.299999999999997,\"dRx\":0,\"dTx\":0,\"connTime\":\"14:03:31\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T09:01:57.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"FA:4E:23:26:FD:E8\",\"ip\":\"192.168.101.51\",\"rssi\":-76,\"rx\":24,\"tx\":104,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:29:28\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:36:11.9309938-04:00\"},{\"name\":\"ABruc1195\",\"alias\":\"\",\"mac\":\"10:68:38:37:DE:B3\",\"ip\":\"192.168.101.73\",\"rssi\":-75,\"rx\":1,\"tx\":144.40000000000001,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:40:30\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Microsoft Corp.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:25:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"1A:44:8E:70:EA:54\",\"ip\":\"192.168.1.130\",\"rssi\":-90,\"rx\":24,\"tx\":86.700000000000003,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"52:80:A0:51:C1:F3\",\"ip\":\"192.168.101.234\",\"rssi\":-94,\"rx\":1,\"tx\":28.899999999999999,\"dRx\":0,\"dTx\":0,\"connTime\":\"13:35:27\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T09:29:58.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"CE:05:4A:EE:EF:8C\",\"ip\":\"192.168.101.142\",\"rssi\":-73,\"rx\":24,\"tx\":115.59999999999999,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:19:40\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:46:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"DE:BF:80:F1:1A:DA\",\"ip\":\"192.168.101.181\",\"rssi\":-86,\"rx\":11,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:03\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"32:09:18:19:FD:71\",\"ip\":\"192.168.101.104\",\"rssi\":-65,\"rx\":24,\"tx\":57.799999999999997,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:17:15\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:48:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"D6:E7:E5:AD:F1:C6\",\"ip\":\"192.168.101.116\",\"rssi\":-60,\"rx\":24,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:27:57\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:38:11.9309938-04:00\"},{\"name\":\"iPad\",\"alias\":\"\",\"mac\":\"56:2C:70:33:0E:C6\",\"ip\":\"192.168.101.10\",\"rssi\":-89,\"rx\":1,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:25:22\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:40:09.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"14:1A:97:82:B1:B7\",\"ip\":\"192.168.101.175\",\"rssi\":-83,\"rx\":1,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:01\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"1E:F1:B6:92:5F:F7\",\"ip\":\"192.168.101.176\",\"rssi\":-87,\"rx\":24,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:36:45\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:29:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"82:D4:28:04:E1:C5\",\"ip\":\"192.168.101.146\",\"rssi\":-41,\"rx\":24,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:31:33\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:34:09.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"36:E0:31:81:E8:B4\",\"ip\":\"192.168.101.132\",\"rssi\":-84,\"rx\":24,\"tx\":117,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:30:39\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T20:35:09.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"72:FB:73:61:89:E5\",\"ip\":\"192.168.101.192\",\"rssi\":-94,\"rx\":1,\"tx\":72.200000000000003,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:07:57\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:58:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"BE:C6:E0:59:99:54\",\"ip\":\"192.168.101.62\",\"rssi\":-88,\"rx\":24,\"tx\":26,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:13:37\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:52:11.9309938-04:00\"},{\"name\":\"Intel Corporate\",\"alias\":\"\",\"mac\":\"E0:D0:45:F1:4F:E1\",\"ip\":\"192.168.101.235\",\"rssi\":-71,\"rx\":1,\"tx\":130,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:00\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Intel Corporate\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"CE:0B:45:3C:D5:A5\",\"ip\":\"192.168.101.147\",\"rssi\":-94,\"rx\":1,\"tx\":19.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"01:05:45\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:00:10.9309938-04:00\"},{\"name\":\"espressif\",\"alias\":\"espressif\",\"mac\":\"44:67:55:27:03:0B\",\"ip\":\"192.168.1.71\",\"rssi\":-23,\"rx\":72.200000000000003,\"tx\":43.299999999999997,\"dRx\":66.200000000000003,\"dTx\":0,\"connTime\":\"46:57:09\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"Orbit Irrigation\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-28T00:07:25.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"9E:57:BC:F5:D5:82\",\"ip\":\"192.168.101.219\",\"rssi\":-93,\"rx\":1,\"tx\":26,\"dRx\":0,\"dTx\":0,\"connTime\":\"02:04:44\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T21:01:09.9309938-04:00\"},{\"name\":\"Watch\",\"alias\":\"\",\"mac\":\"C6:18:07:32:0E:7F\",\"ip\":\"192.168.101.161\",\"rssi\":-91,\"rx\":6,\"tx\":58.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"11:55:52\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T11:10:00.9309938-04:00\"},{\"name\":\"ESP_89D901\",\"alias\":\"\",\"mac\":\"E0:98:06:89:D9:01\",\"ip\":\"192.168.1.148\",\"rssi\":-78,\"rx\":2,\"tx\":65,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:04\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Espressif Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"iPhone\",\"alias\":\"\",\"mac\":\"DE:D5:27:F3:28:C2\",\"ip\":\"192.168.101.41\",\"rssi\":-94,\"rx\":1,\"tx\":39,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:55:43\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:10:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"AA:55:36:EE:B5:A4\",\"ip\":\"192.168.1.140\",\"rssi\":-75,\"rx\":11,\"tx\":65,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:03:58\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:02:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"74:71:8B:9C:3E:5B\",\"ip\":\"192.168.101.105\",\"rssi\":0,\"rx\":0,\"tx\":0,\"dRx\":0,\"dTx\":0,\"connTime\":\"\",\"deviceType\":0,\"isConnected\":true,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"0001-01-01T00:00:00\"},{\"name\":\"Watch\",\"alias\":\"\",\"mac\":\"7E:34:3F:CE:ED:73\",\"ip\":\"192.168.101.106\",\"rssi\":-95,\"rx\":1,\"tx\":5.5,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:12:01\",\"deviceType\":23,\"isConnected\":false,\"vendor\":\"Apple, Inc.\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T22:53:11.9309938-04:00\"},{\"name\":\"\",\"alias\":\"\",\"mac\":\"56:76:EC:DD:78:AE\",\"ip\":\"192.168.1.136\",\"rssi\":-78,\"rx\":6.5,\"tx\":1170,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:01\",\"deviceType\":0,\"isConnected\":false,\"vendor\":\"\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"},{\"name\":\"Pixel-8\",\"alias\":\"\",\"mac\":\"6A:C1:09:73:7B:FC\",\"ip\":\"192.168.1.82\",\"rssi\":-94,\"rx\":1,\"tx\":52,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:02:40\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:03:11.9309938-04:00\"},{\"name\":\"Pixel-8\",\"alias\":\"\",\"mac\":\"46:85:9F:46:70:DE\",\"ip\":\"192.168.101.166\",\"rssi\":-91,\"rx\":1,\"tx\":156,\"dRx\":0,\"dTx\":0,\"connTime\":\"00:00:04\",\"deviceType\":34,\"isConnected\":false,\"vendor\":\"Others\",\"curDT\":\"2025-09-29T23:05:11.9309938-04:00\",\"startDT\":\"2025-09-29T23:05:11.9309938-04:00\"}]";
            //DISABLED FOR NOW     if (raw.Length > 100)
            //DISABLED FOR NOW     {
            //DISABLED FOR NOW         ClientReport[] clients = ClientReport.GetClientsFromJson(raw);
            //DISABLED FOR NOW         ClientReport.SetClients(clients);
            //DISABLED FOR NOW 
            //DISABLED FOR NOW 
            //DISABLED FOR NOW         RConnMgr rcmgr = new RConnMgr(_hostEnv, _context);
            //DISABLED FOR NOW         rcmgr.Update(clients);
            //DISABLED FOR NOW         rcmgr.Close(out string newClient);
            //DISABLED FOR NOW         hsCode = (clients != null) ? HttpStatusCode.OK : HttpStatusCode.NotAcceptable;
            //DISABLED FOR NOW     }
            //DISABLED FOR NOW     else
            //DISABLED FOR NOW         hsCode = HttpStatusCode.NotAcceptable;
            //DISABLED FOR NOW     RespMsg = "Response from CStat Router Post";
            //DISABLED FOR NOW }
            //DISABLED FOR NOW catch (Exception e)
            //DISABLED FOR NOW {
            //DISABLED FOR NOW     cl.Log("RouterCont: Post Exception :" + e.Message);
            //DISABLED FOR NOW     hsCode = HttpStatusCode.InternalServerError;
            //DISABLED FOR NOW     RespMsg = e.Message;
            //DISABLED FOR NOW }
            //DISABLED FOR NOW 
            //DISABLED FOR NOW var response = new HttpResponseMessage(hsCode)
            //DISABLED FOR NOW {
            //DISABLED FOR NOW     Content = new StringContent(RespMsg, System.Text.Encoding.UTF8, "text/plain"),
            //DISABLED FOR NOW     StatusCode = hsCode
            //DISABLED FOR NOW };
            //DISABLED FOR NOW return response;

        }
    }

}


