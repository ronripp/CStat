using CStat.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CRSet
    {
        public string _fileBody;
        public string _fullFile;
        public static ReaderWriterLockSlim _fLock = new ReaderWriterLockSlim();
        private List<ClientReport> _crList = null;
        public bool _isDirty = false;

        public CRSet(IWebHostEnvironment hostEnv, string fBody)
        {
            _fileBody = fBody;
            string folderName = @"RConns";
            string webRootPath = hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            _fullFile = Path.Combine(newPath, _fileBody);
            InitializeCRs();
        }

        public bool Update(ClientReport cr)
        {
            if (cr?.curDT == null)
                return false;

            var foundIdx = _crList.FindIndex(c => (c.mac == cr.mac) && Math.Abs((c.startDT - cr.startDT).TotalHours) < 4);
            if (foundIdx == -1)
                _crList.Add(cr);
            else
                _crList[foundIdx] = cr;
            _isDirty = true;
            return false;
        }

        public void Close()
        {
            if (_isDirty)
            {
                _isDirty = false;
                WriteCRs();
            }
        }

        public void Add(ClientReport cr)
        {
            _isDirty = true;
            _crList.Add(cr);
        }

        public int FullCount()
        {
            return _crList.Count;
        }

        public int ConnectedCount()
        {
            return _crList.Count(c => c.isConnected);
        }
        public ClientReport[] GetConnected()
        {
            return _crList.Where(c => c.isConnected).ToArray();
        }

        public int InitializeCRs()
        {
            if (_fLock.TryEnterWriteLock(250))
            {
                try
                {
                    _crList = new List<ClientReport>();
                    if (File.Exists(_fullFile))
                    {
                        string[] fls = File.ReadAllLines(_fullFile);
                        foreach (var fl in fls)
                        {
                            ClientReport cr = JsonSerializer.Deserialize<ClientReport>(fl);
                            if (cr != null)
                            {
                                cr.isConnected = false; // initially set isConnected to false. If still connected, it will be set to true later
                                _crList.Add(cr);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var cl = new CSLogger();
                    cl.Log("CRSet.ReadCRs: Exception : " + e.Message);
                }
                finally
                {
                    _fLock.ExitWriteLock();
                }
                return _crList.Count;
            }
            else
            {
                return 0;
            }
        }

        public bool WriteCRs()
        {
            bool res = false;
            if ((_crList.Count > 0) && _fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamWriter wFile = new StreamWriter(_fullFile))
                    {
                        foreach (var cr in _crList)
                        {
                            string line = JsonSerializer.Serialize<ClientReport>(cr);
                            wFile.WriteLine(line);
                        }
                    }
                    res = true;
                }

                catch (Exception e)
                {
                    var cl = new CSLogger();
                    cl.Log("CRSet.WriteCRs: Exception : " + e.Message);
                }

                finally
                {
                    _fLock.ExitWriteLock();
                }
            }
            return res;
        }
    }

    public class RConnMgr
    {
        List<CRSet> _CRSetList = new List<CRSet>();
        private readonly IWebHostEnvironment _hostEnv;
        private readonly CStat.Models.CStatContext _context;

        public RConnMgr(IWebHostEnvironment hostEnv, CStat.Models.CStatContext context)
        {
            _hostEnv = hostEnv;
            _context = context;
        }

        public int Update(ClientReport[] crs)
        {
            int updated = 0;
            foreach (var cr in crs)
            {
                updated += AddClientReport(cr);
            }
            return updated;
        }
        private int AddClientReport(ClientReport cr)
        {
            if ((cr.deviceType != 31) || (cr?.curDT == null) )
                return 0;

            // Generate file body name to read existing CR records
            string fileBody = "RConn" + (cr.curDT.Year % 100).ToString("00") + cr.curDT.Month.ToString("00") + cr.curDT.Day.ToString("00") + ".txt";
            CRSet crSet = GetCRSet(fileBody);
            crSet.Update(cr);
            return 1;
        }

        CRSet GetCRSet (string fBody)
        {
            CRSet fd = _CRSetList.Find(f => f._fileBody == fBody);
            if (fd == default(CRSet))
            {
                fd = new CRSet(_hostEnv, fBody);
                _CRSetList.Add(fd);
            }
            return fd;
        }
        public bool Close(out string lastConnected)
        {
            List<ClientReport> connectedCRs = new List<ClientReport>();
            foreach (var crset in _CRSetList)
            {
                List<ClientReport> crconn = crset.GetConnected().ToList();
                if (crconn.Count > 0)
                    connectedCRs.AddRange(crconn);
                crset.Close();
            }

            lastConnected = "";
            if (connectedCRs.Count > 0)
            {
                // Check on this being an alert given it is not near the time of an event
                if (!Event.IsEventDay(_context, false, -8, 6))
                {
                    List<ClientReport> lastConnectedCRs = ReadLast();
                    DateTime maxDate = lastConnectedCRs.Max(lc => lc.curDT);
                    DateTime minDate = connectedCRs.Min(lc => lc.curDT);
                    if ((minDate - maxDate).TotalHours > 4)
                    {
                        ClientReport instCR = connectedCRs.Find(c => c.curDT == minDate);

                        lastConnected = "New Arrival : " + instCR.GetName() + " @ " + instCR.curDT.ToShortDateString() + " " + instCR.curDT.ToShortDateString();
                    }
                }
                WriteLast(connectedCRs);
            }
            return true;
        }

        string GetLastFilename()
        {
            string webRootPath = _hostEnv.WebRootPath;
            string newPath = Path.Combine(webRootPath, @"RConns");
            if (!Directory.Exists(newPath))
                Directory.CreateDirectory(newPath);
            return Path.Combine(newPath, "LastConnected.txt");
        }

        public List<ClientReport> ReadLast()
        {
            string fullFile = GetLastFilename();
            List<ClientReport> lastCRs = new List<ClientReport>();
            if (CRSet._fLock.TryEnterWriteLock(250))
            {
                try
                {
                    if (File.Exists(fullFile))
                    {
                        string[] fls = File.ReadAllLines(fullFile);
                        foreach (var fl in fls)
                        {
                            ClientReport cr = JsonSerializer.Deserialize<ClientReport>(fl);
                            if (cr != null)
                            {
                                lastCRs.Add(cr);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    var cl = new CSLogger();
                    cl.Log("RConnMgr.ReadLast: Exception : " + e.Message);
                }
                finally
                {
                    CRSet._fLock.ExitWriteLock();
                }
            }
            return lastCRs;
        }

        public bool WriteLast(List<ClientReport> crs)
        {
            string fullFile = GetLastFilename();
            bool res = false;
            if ((crs.Count > 0) && CRSet._fLock.TryEnterWriteLock(250))
            {
                try
                {
                    using (StreamWriter wFile = new StreamWriter(fullFile))
                    {
                        foreach (var cr in crs)
                        {
                            string line = JsonSerializer.Serialize<ClientReport>(cr);
                            wFile.WriteLine(line);
                        }
                    }
                    res = true;
                }

                catch (Exception e)
                {
                    var cl = new CSLogger();
                    cl.Log("RConnMgr.WriteLast: Exception : " + e.Message);
                }

                finally
                {
                    CRSet._fLock.ExitWriteLock();
                }
            }
            return res;
        }

    }
}