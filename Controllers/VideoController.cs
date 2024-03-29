﻿using CStat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public FileResult Index()
        {
            //string fileName = "Videos\\southford.mp4";
            string fileName = "Videos\\CCA_Dave_Miller_21.mp4";
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

