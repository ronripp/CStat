using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CSFileWatcher : FileSystemWatcher
    {
        public readonly IWebHostEnvironment _hostEnv;
        public readonly CSLogger _cl;
        public CSFileWatcher(string path, string wildcard, IWebHostEnvironment hostEnv) : base(path, wildcard)
        {
            _cl = new CSLogger();
            _hostEnv = hostEnv;
            NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            Changed += OnCreated;
            Created += OnCreated;
            //Deleted += OnDeleted;
            //Renamed += OnRenamed;
            Error += OnError;

            Filter = wildcard;
            IncludeSubdirectories = false;
            EnableRaisingEvents = true;
        }

        public string GetHRSnapShotFilePath(CamOps.Camera cam)
        {
            return ""; // ZZZ
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            CSFileWatcher csFW = (CSFileWatcher)sender;
            csFW._cl.Log("CSFW: Created/Mod : " + e.FullPath);

            if (e.FullPath.Contains("Camera2\\Images"))
            {
                // Camera 2 has motion/activity
                CamOps._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.HRSnapShot, csFW.GetHRSnapShotFilePath(CamOps.Camera.Camera2), "");
                cam2.GetPresetPicture(csFW._hostEnv, 2);
            }
            else if (e.FullPath.Contains("Camera1\\Images"))
            {
                // Camera 1 has motion/activity
                //PtzCamera cam1 = new PtzCamera(CamOps.Camera.Camera1);
                //cam1.GetPresetPicture(csFW._hostEnv, 
            }

        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            CSFileWatcher csFW = (CSFileWatcher)sender;
            csFW._cl.Log("CSFW: Error : " + e.GetException().Message );
        }
    }
}
