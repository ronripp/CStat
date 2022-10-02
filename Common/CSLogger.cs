using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public static class csl
    {
        public static void Log(string str)
        { 
            DateTime now = PropMgr.ESTNow;
            long msecs = (long)Math.Round((now - new DateTime(now.Year, now.Month, now.Day, 0, 0, 0)).TotalMilliseconds);
            Trace.WriteLine("[" + msecs + "] " + str + "\n");
        }
    }
     public class CSLogger
    {
        [DllImport("Kernel32", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        public static extern Int32 GetCurrentWin32ThreadId();

        private string LogFile;
        private static ReaderWriterLockSlim fLock = new ReaderWriterLockSlim();
        public CSLogger (IWebHostEnvironment hostEnv)
        {
            string WebRootPath = hostEnv.WebRootPath;
            string LogPath = Path.Combine(WebRootPath, "Log");
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
            LogFile = Path.Combine(LogPath, "CSProc.log");
        }

        public int Log(string LogStr)
        {
            if (fLock.TryEnterWriteLock(250))
            {
                int retVal = 0;
                try
                {
                    //string FullLine = "[" + PropMgr.ESTNowStr + "][" + Thread.CurrentThread.ManagedThreadId +  "] " + LogStr;
                    string FullLine = "[" + PropMgr.ESTNowStr + "][" + GetCurrentWin32ThreadId() +  "] " + LogStr;
                    using (StreamWriter sw = new StreamWriter(LogFile, true))
                    {
                        retVal = 1;
                        sw.WriteLine(FullLine);
                        sw.Close();
                    }
                }
                catch
                {
                    retVal = -3;
                }
                finally
                {
                    fLock.ExitWriteLock();
                }
                return retVal;
            }
            else
            {
                return -2;
            }
        }

        private string GetCurrentThreadId()
        {
            throw new NotImplementedException();
        }
    }
}
