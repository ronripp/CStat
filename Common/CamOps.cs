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
        public override Queue<COp> _queue { get; set; }

        public CamOps1()
        {
            _qLock = _qLock1;
            lock (_qLock)
            {
                _queue = new Queue<COp>();
            }
        }
    }

    public class CamOps2 : CamOps
    {
        protected override object _qLock { get; set; } = null;
        public override Queue<COp> _queue { get; set; }

        public CamOps2()
        {
            _qLock = _qLock1;
            lock (_qLock)
            {
                _queue = new Queue<COp>();
            }
        }
    }

    public abstract class CamOps
    {
        public enum Camera { Camera1 = 1, Camera2 = 2 };

        protected static object _qLock1 { get; set; } = new object();
        protected static object _qLock2 { get; set; } = new object();
        protected abstract object _qLock { get; set; }
        public abstract Queue<COp> _queue { get; set; }

        //private string _link = "";
        public CamOps()
        {
        }

        public COp Add(COp cop)
        {
            lock (_qLock)
            {
                try
                {
                    bool _qEmpty = _queue.Count == 0;
                    //gLog.Log("CamOps.Add to Queue " + (int)cop);
                    _queue.Enqueue(cop);
                    //gLog.Log("VCamOps.Add returning " + ((_qEmpty) ? (int)cop : (int)COp.None));
                    return (_qEmpty) ? cop : COp.None; // Indicate what if anything to execute COp now
                }
                catch
                {
                    return COp.None;
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


        public COp Delete()
        {
            lock (_qLock)
            {
                try
                {
                    bool _qEmpty = _queue.Count <= 1;
                    if (_queue.Count > 0)
                    {
                        //gLog.Log("CamOps.Delete from Queue " + (int)_queue.Peek());
                        _queue.Dequeue();
                    }
                    //gLog.Log("CamOps.Delete returning " + ((_qEmpty) ? (int)COp.None : (int)_queue.Peek()));

                    return (_qEmpty) ? COp.None : _queue.Peek(); // Indicate what if anything to execute COp now
                }
                catch
                {
                    return COp.None;
                }
            }
        }
        public int HandleOp (IWebHostEnvironment hostEnv, COp rcop)
        {
            //gLog.Log("CamOps.HandleOp START rcop=" + (int)rcop);
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    int MaxDelay = 0;
                    var cop = Add(rcop);
                    if (cop == COp.None)
                        return 6000;
                    while (cop != COp.None)
                    {
                        //gLog.Log("CamOps.HandleOp " + (int)cop);
                        var delay = ptzCam.ExecuteOp(hostEnv, cop);
                        if (delay > MaxDelay)
                            MaxDelay = delay;
                        cop = Delete();
                    }

                    // Give time for Camera to move. Delay can be adjusted
                    ptzCam.Logout();
                    //gLog.Log("CamOps.HandleOp RETURN MaxDelay=" + rcop);

                    return MaxDelay;
                }
            }
            catch (Exception e)
            {
                _ = e;
                //gLog.Log("**EXCEPTION*** HandleOp(" + (int)rcop + ") e=" + e.Message);
                return 0;
            }
        }
        public string GetVideo(IWebHostEnvironment hostEnv, string url)
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    var link = ptzCam.GetVideo(hostEnv, url);
                    ptzCam.Logout();
                    return link;
                }
            }
            catch (Exception e)
            {
                _ = e;
                //gLog.Log("**EXCEPTION*** GetVideo e=" + e.Message);
                return "";
            }
        }

        public string SnapShot(IWebHostEnvironment hostEnv, bool asFile, string resStr = "&width=1024&height=768")
        {
            //gLog.Log("CamOps.SnapShot START");
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    var link = ptzCam.GetSnapshot(hostEnv, asFile, resStr);
                    //gLog.Log("CamOps.SnapShot LINK=" + link);
                    ptzCam.Logout();
                    return link;
                }
            }
            catch (Exception e)
            {
                _ = e;
                //gLog.Log("**EXCEPTION*** SnapShot e=" + e.Message);
                return "";
            }
        }

        public void Cleanup(IWebHostEnvironment hostEnv, string exceptFile = "")
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    ptzCam.Cleanup(hostEnv, exceptFile);
                    ptzCam.Logout(true);
                }
            }
            catch (Exception e)
            {
                _ = e;
                //gLog.Log("**EXCEPTION*** Cleanup e=" + e.Message);
            }
        }

        public string GetVideoAnchors (IWebHostEnvironment hostEnv, DateTime sdt, DateTime edt)
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    int ancCount = 0;
                    return ptzCam.GetVideoAnchors(hostEnv, sdt, edt, ref ancCount);
                }
            }
            catch (Exception e)
            {
                _ = e;
                //gLog.Log("**EXCEPTION*** GetVideoAnchors e=" + e.Message);
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
