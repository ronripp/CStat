using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
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
        private string _link = "";
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
                    _queue.Enqueue(cop);
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
                        _queue.Dequeue();
                    return (_qEmpty) ? COp.None : _queue.Peek(); // Indicate what if anything to execute COp now
                }
                catch
                {
                    return COp.None;
                }
            }
        }
        public void HandleOp (IWebHostEnvironment hostEnv, COp rcop)
        {
            PtzCamera ptzCam = new PtzCamera();
            int MaxDelay = 0;
            var cop = Add(rcop);
            if (cop == COp.None)
                return;
            while (cop != COp.None)
            {
                var delay = ptzCam.ExecuteOp(hostEnv, cop);
                if (delay > MaxDelay)
                    MaxDelay = delay;
                cop = Delete();
            }

            // Give time for Camera to move. Delay can be adjusted
            Thread.Sleep(MaxDelay);

            SetLink(ptzCam.GetSnapshot(hostEnv));
            ptzCam.Logout();
        }

        public void SetLink(string str)
        {
            if (string.IsNullOrEmpty(str))
                return;

            lock (_qLock)
            {
                _link = str;
            }
        }
        public string GetLink()
        {
            lock (_qLock)
            {
                return _link;
            }
        }
    }
}
