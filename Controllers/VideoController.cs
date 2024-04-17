using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace CStat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class VideoController : Controller
    {
        private readonly CStatContext entities;
        private readonly IWebHostEnvironment _hostEnv;
        public VideoController(CStatContext context, IWebHostEnvironment hostEnv)
        {
            entities = context;
            _hostEnv = hostEnv;
        }

        public object Results { get; private set; }

        private readonly String[] _videos =
        {
            "",
            "Videos\\CCA_Dave_Miller_21.mp4",
            "chapelHeat\\Overview from front of attic.mp4",
            "chapelHeat\\Overview of Heater.mp4",
            "chapelHeat\\Propane Coming in, Air in, Heat out of Unit.mp4"
        };

        [HttpGet("{vidx}")]
        public FileResult Index(int vidx)
        {
            string fileName = _videos[vidx] ?? "";
            if (string.IsNullOrEmpty(fileName))
                return null;

            string physPath = Path.Combine(_hostEnv.WebRootPath, fileName);

            //var content = new FileStream(physPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var res = File(System.IO.File.OpenRead(physPath), "video/mp4");
            res.FileDownloadName = fileName;
            res.EnableRangeProcessing = true;
            return res;

            //content
            ////var filestream = System.IO.File.OpenRead(physPath);
            ////return Results.File(filestream, contentType: "video/mp4", fileDownloadName: filename, enableRangeProcessing: true);
            //var response = File(content, "application/octet-stream");
            //return response;
        }
    }
}

