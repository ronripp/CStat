using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

            //Changed += OnCreated;
            Created += OnCreated;
            //Deleted += OnDeleted;
            //Renamed += OnRenamed;
            Error += OnError;

            Filter = wildcard;
            IncludeSubdirectories = false;
            EnableRaisingEvents = true;
        }

        public string GetHRSnapShotFilePath(Cam.Camera cam)
        {
            DateTime now = PropMgr.ESTNow;
            string cPath;
            if (cam == Cam.Camera.Camera1)
                cPath = @"Camera1\Images\after\camp_01";
            else if (cam == Cam.Camera.Camera2)
                cPath = @"Camera2\Images\after\RJR_01";
            else
                return "";

            return System.IO.Path.Combine(_hostEnv.WebRootPath, cPath +
                now.Year.ToString() + now.Month.ToString().PadLeft(2, '0') + now.Day.ToString().PadLeft(2, '0') +
                now.Hour.ToString().PadLeft(2, '0') + now.Minute.ToString().PadLeft(2, '0') + now.Second.ToString().PadLeft(2, '0') + ".jpg");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            CSFileWatcher csFW = (CSFileWatcher)sender;
            csFW._cl.Log("CSFW: Created/Mod : " + e.FullPath);

            if (e.FullPath.Contains("Camera2\\Images"))
            {
                // Camera 2 has motion/activity
                Thread.Sleep(Cam._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.LightOn));
                Thread.Sleep(Cam._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.HRSnapShot, csFW.GetHRSnapShotFilePath(Cam.Camera.Camera2)));
                Thread.Sleep(Cam._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.Preset5 + Cam._CamOps2.PresetOffsetFrom1())); // Park/Field
                Thread.Sleep(10000); // Allow camera to move and focus in
                Thread.Sleep(Cam._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.HRSnapShot, csFW.GetHRSnapShotFilePath(Cam.Camera.Camera2)));
                Thread.Sleep(15000);
                Thread.Sleep(Cam._CamOps2.HandleOp(csFW._hostEnv, PtzCamera.COp.LightOff));
            }
            else if (e.FullPath.Contains("Camera1\\Images"))
            {
                // Camera 1 has motion/activity
                Thread.Sleep(Cam._CamOps1.HandleOp(csFW._hostEnv, PtzCamera.COp.LightOn));
                Thread.Sleep(Cam._CamOps1.HandleOp(csFW._hostEnv, PtzCamera.COp.HRSnapShot, csFW.GetHRSnapShotFilePath(Cam.Camera.Camera1)));
                Thread.Sleep(Cam._CamOps1.HandleOp(csFW._hostEnv, PtzCamera.COp.Preset5 + Cam._CamOps1.PresetOffsetFrom1())); // Front View
                Thread.Sleep(10000); // Allow camera to move and focus in
                Thread.Sleep(Cam._CamOps1.HandleOp(csFW._hostEnv, PtzCamera.COp.HRSnapShot, csFW.GetHRSnapShotFilePath(Cam.Camera.Camera1)));
                Thread.Sleep(15000);
                Thread.Sleep(Cam._CamOps1.HandleOp(csFW._hostEnv, PtzCamera.COp.LightOff));
            }

        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            CSFileWatcher csFW = (CSFileWatcher)sender;
            csFW._cl.Log("CSFW: Error : " + e.GetException().Message);
        }

        public static List<string> GetFilesDaysOlderThan(string path, string pattern, int daysBack, bool includeSubDirs)
        {
            DateTime targetDT = PropMgr.ESTNow.AddDays(-daysBack);
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles(pattern, (includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
               .Where(f => f.CreationTime < targetDT)
               .Select(f => f.FullName).ToList();
        }

        public static List<string> GetFileGroups(IWebHostEnvironment hostEnv, Cam.Camera cam)
        {
            List<string> groups = new List<string> { };
            string cDir="";
            if (cam == Cam.Camera.Camera1)
                cDir = "Camera1";
            else if (cam == Cam.Camera.Camera2)
                cDir = "Camera2";
            else
            {
                var cl = new CSLogger();
                cl.Log("CSFW: DeleteFilesOlderThanDays : Error : CAMERA {" + cam.ToString() + "} NOT Found.");
                return groups;
            }

            string TopPath = System.IO.Path.Combine(hostEnv.WebRootPath, cDir, "Images");
            string AfterPath = System.IO.Path.Combine(TopPath, "After");



            // TBD Generate groups

            return groups;
        }
        public static List<string> GetFilesWithinMinutesOf(string path, string pattern, DateTime srcDT, int withinMinutes, bool includeSubDirs)
        {
            var startDT = srcDT.AddMinutes(-withinMinutes);
            var endDT = srcDT.AddMinutes(withinMinutes);

            DirectoryInfo dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles(pattern, (includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                              .Where(f => f.CreationTime >= startDT && f.CreationTime <= endDT)
                              .Select(f => f.FullName).ToList();
        }

        public static List<string> GetFiles(string path, string pattern, DateTime startDT, DateTime endDT, bool includeSubDirs)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles(pattern, (includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                              .Where(f => f.CreationTime >= startDT && f.CreationTime <= endDT)
                              .Select(f => f.FullName).ToList();
        }

        public static void DeleteFilesOlderThanDays(IWebHostEnvironment hostEnv, Cam.Camera cam, int olderThanDays)
        {
            string cDir;
            if (cam == Cam.Camera.Camera1)
                cDir = "Camera1";
            else if (cam == Cam.Camera.Camera2)
                cDir = "Camera2";
            else
            {
                var cl = new CSLogger();
                cl.Log("CSFW: DeleteFilesOlderThanDays : Error : CAMERA {" + cam.ToString() + "} NOT Found.");
                return;
            }
            foreach (var fn in GetFilesDaysOlderThan(System.IO.Path.Combine(hostEnv.WebRootPath, cDir, "Images"), "*.jpg", olderThanDays, true))
            {
                System.IO.File.Delete(fn);
            }
        }
    }
}
