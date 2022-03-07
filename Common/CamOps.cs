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
    public class CamOps
    {
        private static readonly object _qLock = new object();
        public Queue<COp> _queue;
        //private string _link = "";
        public CamOps()
        {
            lock (_qLock)
            {
                _queue = new Queue<COp>();
            }
        }

        public COp Add(COp cop)
        {
            lock (_qLock)
            {
                try
                {
                    bool _qEmpty = _queue.Count == 0;
                    Debug.WriteLine("CamOps.Add to Queue " + (int)cop);
                    _queue.Enqueue(cop);
                    Debug.WriteLine("VCamOps.Add returning " + ((_qEmpty) ? (int)cop : (int)COp.None));
                    return (_qEmpty) ? cop : COp.None; // Indicate what if anything to execute COp now
                }
                catch
                {
                    return COp.None;
                }
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
                        Debug.WriteLine("CamOps.Delete from Queue " + (int)_queue.Peek());
                        _queue.Dequeue();
                    }
                    Debug.WriteLine("CamOps.Delete returning " + ((_qEmpty) ? (int)COp.None : (int)_queue.Peek()));

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
                        Debug.WriteLine("CamOps.HandleOp " + (int)cop);
                        var delay = ptzCam.ExecuteOp(hostEnv, cop);
                        if (delay > MaxDelay)
                            MaxDelay = delay;
                        cop = Delete();
                    }

                    // Give time for Camera to move. Delay can be adjusted
                    ptzCam.Logout();
                    return MaxDelay;
                }
            }
            catch
            {
                return 0;
            }
        }

        public string SnapShot(IWebHostEnvironment hostEnv)
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    var link = ptzCam.GetSnapshot(hostEnv);
                    ptzCam.Logout();
                    return link;
                }
            }
            catch
            {
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
                }
            }
            catch
            {
            }
        }

        public string GetVideos()
        {
            try
            {
                using (PtzCamera ptzCam = new PtzCamera())
                {
                    return ptzCam.GetVideos();
                }
            }
            catch
            {
                return "";
            }
        }

        //public void SetLink(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //        return;

        //    lock (_qLock)
        //    {
        //        _link = str;
        //        Debug.WriteLine("CamOps.SetLink " + _link);
        //    }
        //}
        //public string GetLink()
        //{
        //    lock (_qLock)
        //    {
        //        Debug.WriteLine("CamOps.GetLink " + _link);
        //        return _link;
        //    }
        //}
    }
}
