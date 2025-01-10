using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    class FileData
    {
        public string _fileBody;
        public string _fullFile;
        public ReaderWriterLockSlim _fLock = new ReaderWriterLockSlim();
        public string[] _fileLines = null;

        FileData(IWebHostEnvironment hostEnv, string fBody)
        {
            _fileBody = fBody;
            string folderName = @"RConns";
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            _fullFile = Path.Combine(newPath, _fileBody);
        }

        public bool ReadAllCRs(FileData fd)
        {
            if (fd._fLock.TryEnterWriteLock(250))
            {
                try
                {
                    if (File.Exists(fd._fullFile))
                        fd._fileLines = File.ReadAllLines(fd._fullFile);
                    else
                        fd._fileLines = new string[] { };
                }
                catch
                {

                }
                finally
                {
                    fd._fLock.ExitWriteLock();
                }
                return fd._fileLines.Length > 0;
            }
            else
            {
                return false;
            }
        }
    }

    public class RConnMgr
    {
        List<FileData> _fileData = new List<FileData>();
        private readonly IWebHostEnvironment _hostEnv;
        public RConnMgr(IWebHostEnvironment hostEnv)
        {
            _hostEnv = hostEnv;
        }

        public int AddClientReport(ClientReport cr)
        {

            return 0;
        }
    }
}