using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CStat.Common.PtzCamera;

namespace CStat.Common
{
    public class CamOps1 : CamOps
    {
        protected override object _qLock { get; set; } = null;
        public override Queue<COpObj> _queue { get; set; }

        public CamOps1()
        {
            _qLock = _qLock1;
            _cam = Camera.Camera1;
            lock (_qLock)
            {
                _queue = new Queue<COpObj>();
            }
        }
        public override int PresetOffsetFrom1()
        {
            return 9;
        }
    }

    public class CamOps2 : CamOps
    {
        protected override object _qLock { get; set; } = null;
        public override Queue<COpObj> _queue { get; set; }

        public CamOps2()
        {
            _qLock = _qLock1;
            _cam = Camera.Camera2;
            lock (_qLock)
            {
                _queue = new Queue<COpObj>();
            }
        }
        public override int PresetOffsetFrom1()
        {
            return 7;
        }
    }

    public abstract class CamOps
    {
        public enum Camera { none = 0, Camera1 = 1, Camera2 = 2 };

        protected static object _qLock1 { get; set; } = new object();
        protected static object _qLock2 { get; set; } = new object();
        protected abstract object _qLock { get; set; }
        public abstract Queue<COpObj> _queue { get; set; }
        public abstract int PresetOffsetFrom1();

        public CamOps.Camera _cam;

        //private string _link = "";
        public CamOps()
        {
        }

        public COpObj Add(COpObj copObj)
        {
            lock (_qLock)
            {
                try
                {
                    bool _qEmpty = _queue.Count == 0;
                    gLog.Log("CamOps.Add to Queue " + (int)copObj._cop);
                    _queue.Enqueue(copObj);
                    gLog.Log("VCamOps.Add returning " + ((_qEmpty) ? (int)copObj._cop : (int)COp.None));
                    return (_qEmpty) ? copObj : null; // Indicate what if anything to execute COp now
                }
                catch
                {
                    return null;
                }
            }
        }

        public int PendingOps()
        {
            lock (_qLock)
            {
                return _queue.Count();
            }
        }

        public COpObj Delete()
        {
            lock (_qLock)
            {
                try
                {
                    bool _qEmpty = _queue.Count <= 1;
                    if (_queue.Count > 0)
                    {
                        gLog.Log("CamOps.Delete from Queue " + _queue.Peek()._cop);
                        _queue.Dequeue();
                    }
                    gLog.Log("CamOps.Delete returning " + ((_qEmpty) ? null :  _queue.Peek()));

                    return (_qEmpty) ? null : _queue.Peek(); // Indicate what if anything to execute COp now
                }
                catch
                {
                    return null;
                }
            }
        }
        public int HandleOp (IWebHostEnvironment hostEnv, COp rcop, string rcopPath="", string rcopAttr="")
        {
            COpObj rCOpObj = new COpObj(rcop, rcopPath, rcopAttr);
            gLog.Log("CamOps.HandleOp START rcop=" + (int)rcop);
            try
            {
                using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    int MaxDelay = 0;
                    var copObj = Add(rCOpObj);
                    if (copObj == null)
                        return 6000;
                    do
                    {
                        gLog.Log("CamOps.HandleOp " + (int)copObj._cop);
                        var delay = ptzCam.ExecuteOp(hostEnv, copObj);
                        if (delay > MaxDelay)
                            MaxDelay = delay;
                        copObj = Delete();
                    }
                    while (copObj != null);

                    // Give time for Camera to move. Delay can be adjusted
                    ptzCam.Logout();
                    gLog.Log("CamOps.HandleOp RETURN MaxDelay=" + rcop);

                    return MaxDelay;
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** HandleOp(" + (int)rcop + ") e=" + e.Message);
                return 0;
            }
        }

        //public string HandleSnapShotFile(IWebHostEnvironment hostEnv,  string resStr = "&width=1024&height=768"))
        //{

        //    public string SnapShotFile(IWebHostEnvironment hostEnv, bool asFile, string resStr = "&width=1024&height=768")

        //    gLog.Log("CamOps.HandleOp START rcop=" + (int)rcop);
        //    try
        //    {
        //        using (PtzCamera ptzCam = new PtzCamera(_cam))
        //        {
        //            int MaxDelay = 0;
        //            var cop = Add(rcop);
        //            if (cop == COp.None)
        //                return 6000;
        //            while (cop != COp.None)
        //            {
        //                gLog.Log("CamOps.HandleOp " + (int)cop);
        //                var delay = ptzCam.ExecuteOp(hostEnv, cop);
        //                if (delay > MaxDelay)
        //                    MaxDelay = delay;
        //                cop = Delete();
        //            }

        //            // Give time for Camera to move. Delay can be adjusted
        //            ptzCam.Logout();
        //            gLog.Log("CamOps.HandleOp RETURN MaxDelay=" + rcop);

        //            return MaxDelay;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _ = e;
        //        gLog.Log("**EXCEPTION*** HandleOp(" + (int)rcop + ") e=" + e.Message);
        //        return 0;
        //    }
        //}

        public string GetVideo(IWebHostEnvironment hostEnv, string url)
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    var link = ptzCam.GetVideo(hostEnv, url);
                    ptzCam.Logout();
                    return link;
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** GetVideo e=" + e.Message);
                return "";
            }
        }

        public string SnapShot(IWebHostEnvironment hostEnv, bool asFile, string resStr = "&width=1024&height=768")
        {
            gLog.Log("CamOps.SnapShot START");
            try
            {
                using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    var link = ptzCam.GetSnapshot(hostEnv, asFile, resStr);
                    gLog.Log("CamOps.SnapShot LINK=" + link);
                    ptzCam.Logout();
                    return link;
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** SnapShot e=" + e.Message);
                return "";
            }
        }

        public string SnapShotFile(string ssfile, IWebHostEnvironment hostEnv, string resStr = "&width=1024&height=768")
        {
            gLog.Log("CamOps.SnapShot START");
            try
            {
               using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    var file = ptzCam.GetSnapshotFile(ssfile, hostEnv, resStr);
                    gLog.Log("CamOps.SnapShot FILE=" + file);
                    ptzCam.Logout();
                    return file;
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** SnapShot e=" + e.Message);
                return "";
            }
        }

        public void Cleanup(IWebHostEnvironment hostEnv, string exceptFile = "")
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    ptzCam.Cleanup(hostEnv, exceptFile);
                    ptzCam.Logout(true);
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** Cleanup e=" + e.Message);
            }
        }

        public string GetVideoAnchors (IWebHostEnvironment hostEnv, DateTime sdt, DateTime edt)
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera(_cam))
                {
                    int ancCount = 0;
                    return ptzCam.GetVideoAnchors(hostEnv, sdt, edt, ref ancCount);
                }
            }
            catch (Exception e)
            {
                _ = e;
                gLog.Log("**EXCEPTION*** GetVideoAnchors e=" + e.Message);
                return null;
            }
        }

        //public void SetLink(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //        return;

        //    lock (_qLock)
        //    {
        //        _link = str;
        //        csl.Log("CamOps.SetLink " + _link);
        //    }
        //}
        //public string GetLink()
        //{
        //    lock (_qLock)
        //    {
        //        csl.Log("CamOps.GetLink " + _link);
        //        return _link;
        //    }
        //}
    }
}
