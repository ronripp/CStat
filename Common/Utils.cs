using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class Utils
    {
        public static string GetTempDir(IWebHostEnvironment hostEnv, string pathFromWWWRoot)
        {
            string webRootPath = hostEnv.WebRootPath;
            string TempPath = Path.Combine(webRootPath, pathFromWWWRoot);
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
            }
            return TempPath;
        }
    }
}
