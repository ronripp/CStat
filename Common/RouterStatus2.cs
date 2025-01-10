using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading;

namespace CStat.Common
{
    public class RouterSettings
    {
        public string acs_dfs { get; set; }
        public string model { get; set; }
        public string productid { get; set; }
        public string label_mac { get; set; }
        public string buildinfo { get; set; }
        public string firmver { get; set; }
        public string firmver_org { get; set; }
        public string buildno { get; set; }
        public string buildno_org { get; set; }
        public string extendno { get; set; }
        public string extendno_org { get; set; }
        public string innerver { get; set; }
        public string apps_sq { get; set; }
        public string lan_hwaddr { get; set; }
        public string lan_ipaddr { get; set; }
        public string lan_proto { get; set; }
        public string x_Setting { get; set; }
        public string lan_netmask { get; set; }
        public string lan_gateway { get; set; }
        public string http_enable { get; set; }
        public string https_lanport { get; set; }
        public string cfg_device_list { get; set; }
        public string wl0_country_code { get; set; }
        public string wl1_country_code { get; set; }
        public string time_zone { get; set; }
        public string time_zone_dst { get; set; }
        public string time_zone_x { get; set; }
        public string time_zone_dstoff { get; set; }
        public string ntp_server0 { get; set; }
        public RouterSettings ShallowCopy()
        {
            return (RouterSettings)MemberwiseClone();
        }
    }
    public class FullClientInfo // Dict mac, FullClientInfo
    {
        public string type { get; set; }
        public string defaultType { get; set; }
        public string name { get; set; }
        public string nickName { get; set; }
        public string ip { get; set; }
        public string mac { get; set; }
        public string from { get; set; }
        public string macRepeat { get; set; }
        public string isGateway { get; set; }
        public string isWebServer { get; set; }
        public string isPrinter { get; set; }
        public string isITunes { get; set; }
        public string dpiType { get; set; }
        public string dpiDevice { get; set; }
        public string vendor { get; set; }
        public string isWL { get; set; }
        public string isGN { get; set; }
        public string ssid { get; set; }
        public string isLogin { get; set; }
        public string opMode { get; set; }
        public string rssi { get; set; }
        public string curTx { get; set; }
        public string curRx { get; set; }
        public string totalTx { get; set; }
        public string totalRx { get; set; }
        public string wlConnectTime { get; set; }
        public string ipMethod { get; set; }
        public string ROG { get; set; }
        public string group { get; set; }
        public string callback { get; set; }
        public string keeparp { get; set; }
        public string qosLevel { get; set; }
        public string wtfast { get; set; }
        public string internetMode { get; set; }
        public string internetState { get; set; }
        public string amesh_isReClient { get; set; }
        public string amesh_papMac { get; set; }
        public string isOnline { get; set; }
        public string amesh_bind_mac { get; set; }
        public string amesh_bind_band { get; set; }
        public FullClientInfo ShallowCopy()
        {
            return (FullClientInfo)MemberwiseClone();
        }
    }
    public class ConnectedDevices // '68:54:FD:87:52:F9': Device(mac='68:54:FD:87:52:F9', ip='192.168.1.220', name='Amazon', node='192.168.1.1', is_wl=True),
    {
        public string mac { get; set; }
        public string ip { get; set; }
        public string name { get; set; }
        public string node { get; set; }
        public bool is_wl { get; set; }
        public ConnectedDevices ShallowCopy()
        {
            return (ConnectedDevices)MemberwiseClone();
        }
    }
    public class MemoryUsage
    {
        public double mem_usage_perc { get; set; }
        public ulong mem_total { get; set; }
        public ulong mem_free { get; set; }
        public ulong mem_used { get; set; }
        public MemoryUsage ShallowCopy()
        {
            return (MemoryUsage)MemberwiseClone();
        }
    }
    public class CPUUsage
    {
        public double cpu1_usage { get; set; }
        public double cpu2_usage { get; set; }
        public double cpu_total_usage { get; set; }
        public CPUUsage ShallowCopy()
        {
            return (CPUUsage)MemberwiseClone();
        }
    }
    public class TrafficBytes
    {
        public double rx { get; set; }
        public double tx { get; set; }
        public TrafficBytes ShallowCopy()
        {
            return (TrafficBytes)MemberwiseClone();
        }
    }
    public class UpTime
    {
        public string last_boot { get; set; }
        public ulong uptime { get; set; }
        public UpTime ShallowCopy()
        {
            return (UpTime)MemberwiseClone();
        }
    }
    public class WANInfo
    {
        public string status { get; set; }
        public string statusstr { get; set; }
        public string type { get; set; }
        public string ipaddr { get; set; }
        public string netmask { get; set; }
        public string gateway { get; set; }
        public string dns { get; set; }
        public string lease { get; set; }
        public string expires { get; set; }
        public string xtype { get; set; }
        public string xipaddr { get; set; }
        public string xnetmask { get; set; }
        public string xgateway { get; set; }
        public string xdns { get; set; }
        public string xlease { get; set; }
        public string xexpires { get; set; }
        public WANInfo ShallowCopy()
        {
            return (WANInfo)MemberwiseClone();
        }
    }
    public class CPUTemp
    {
        public double CPU { get; set; }
        public CPUTemp ShallowCopy()
        {
            return (CPUTemp)MemberwiseClone();
        }
    }

    public class ClientReport
    {
        private static object _crLock { get; set; } = new object();
        private static ClientReport[] _clients { get; set; } = null;
        private static int LockTimeOutSecs = 5;

        public static void SetClients(ClientReport[] clients)
        {
            _clients = clients; // setting and returning is Atomic in multi-threading

            //if (Monitor.TryEnter(_crLock, new TimeSpan(0, 0, LockTimeOutSecs)))
            //{
            //    try
            //    {
            //        _clients = clients;
            //    }
            //    finally
            //    {
            //        Monitor.Exit(_crLock);
            //    }
            //}
        }

        public static ClientReport[] GetClients()
        {
            return _clients; // setting and returning is Atomic in multi-threading 

            //ClientReport[] tclients;
            //if (Monitor.TryEnter(_crLock, new TimeSpan(0, 0, LockTimeOutSecs)))
            //{
            //    tclients = _clients;
            //    Monitor.Exit(_crLock);
            //    return tclients;
            //}
        }


        public string name { get; set; }
        public string alias { get; set; }
        public string mac { get; set; }
        public string ip { get; set; }
        public int rssi { get; set; } // -30 is best
        public double rx { get; set; }
        public double tx { get; set; }
        public double dRx { get; set; }
        public double dTx { get; set; }
        public string connTime { get; set; }
        public int deviceType { get; set; }
        public bool isConnected { get; set; }
        public string vendor { get; set; }
        public DateTime curDT { get; set; }

        public ClientReport()
        {
        }
        public ClientReport(string macId, RouterStatus curRS, RouterStatus prevRS)
        {
            FullClientInfo curFC = ((curRS != null) && curRS._clients.ContainsKey(macId)) ? curRS._clients[macId] : null;
            if (curFC == null) return;
            FullClientInfo prevFC = ((prevRS != null) && prevRS._clients.ContainsKey(macId)) ? prevRS._clients[macId] : null;

            curDT = DateTime.Now;

            name = curFC.name;
            alias = curFC.nickName;
            vendor = curFC.vendor;

            mac = curFC.mac;
            ip = curFC.ip;
            rssi = GetInt(curFC.rssi);

            //Trace.WriteLine("curFC.curRxTx [" + curFC.curRx + "] [" + curFC.curTx + "]");
            rx = GetDouble(curFC.curRx);
            tx = GetDouble(curFC.curTx);

            if (prevFC != null)
            {
                dRx = rx - GetDouble(prevFC.curRx);
                dTx = tx - GetDouble(prevFC.curTx);
            }
            else
                dRx = dTx = 0;
            connTime = curFC.wlConnectTime;

            deviceType = GetInt(curFC.type);
            isConnected = curRS._connDevs.ContainsKey(macId);
        }
        public static string GetClientsAsJson(RouterStatus curRS, RouterStatus prevRS)
        {
            string clients = "[";
            foreach (FullClientInfo fc in curRS._clients.Values)
            {
                if (fc != null && fc.isPrinter == "0" && fc.isGateway == "0" && fc.isWebServer == "0")
                    clients += (JsonSerializer.Serialize<ClientReport>(new ClientReport(fc.mac, curRS, prevRS), new JsonSerializerOptions()) + ",");
            }
            if (clients.Length > 10)
                clients = clients.Substring(0, clients.Length - 1); // remove the last trailing ,

            return clients + "]";
        }

        public static ClientReport[] GetClientsFromJson(string crJson)
        {
            return JsonSerializer.Deserialize<ClientReport[]>(crJson); 
        }

        public static int GetInt(string istr)
        {
            return int.TryParse(istr, out int i) ? i : 0;
        }

        public static double GetDouble(string dstr)
        {
            return double.TryParse(dstr, out double d) ? d : 0;
        }

    }

    /*****
    "~RouterSettings:":
    "~FullClientInfo:":
    "~ConnectedDevices:":
    "~MemoryUsage:":
    "~CPUUsage:":
    "~TratfficBytes:":
    "~UpTime:":
    "~WANInfo":
    "~CPUTemp":
    *****/

    public class RouterStatus
    {
        public RouterSettings _rsettings;
        public Dictionary<string, FullClientInfo> _clients;
        public Dictionary<string, ConnectedDevices> _connDevs; // '68:54:FD:87:52:F9': Device(mac='68:54:FD:87:52:F9', ip='192.168.1.220', name='Amazon', node='192.168.1.1', is_wl=True),
        public MemoryUsage _memUsage;
        public CPUUsage _cpuUsage;
        public TrafficBytes _trafficBytes;
        public UpTime _upTime;
        public WANInfo _wanInfo;
        public CPUTemp _cpuTemp;

        public RouterStatus()
        {
        }

        public bool Parse(string rstr)
        {
            string[] infoStrs = rstr.Split('~');

            /// Replace DEBUG:__main__: with |
            //string[] statStrs = rstr.Replace("DEBUG:__main__:", "|").Split('|');

            foreach (string st in infoStrs)
            {
                string jstr;

                if (st.StartsWith("RouterSettings"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    jstr = jstr.Replace('\'', '"');
                    _rsettings = JsonSerializer.Deserialize<RouterSettings>(jstr);
                    continue;
                }

                if (st.StartsWith("FullClientInfo"))
                {
                    int soff = st.IndexOf("[");
                    int eoff = st.LastIndexOf("}, 'maclist':");
                    jstr = st.Substring(soff + 2, (eoff - soff) - 1);
                    string[] pairs = jstr.Replace("': {'type'", "~{'type'")
                                         .Replace("}, '", "}~")
                                         .Replace('\'', '"')
                                         .Split('~');
                    _clients = new Dictionary<string, FullClientInfo>();
                    for (int i = 1; i < pairs.Length; i += 2)
                    {
                        FullClientInfo fc = JsonSerializer.Deserialize<FullClientInfo>(pairs[i]);
                        _clients.Add(fc.mac, fc);
                    }
                    continue;
                }

                if (st.StartsWith("ConnectedDevices"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff + 2, (eoff - soff) - 1);
                    string[] pairs = jstr.Replace("': Device(", "~{").Replace("), '", "}~").Replace('\'', '"').Replace("mac=", "\"mac\":").Replace("ip=", "\"ip\":")
                                                  .Replace("name=", "\"name\":").Replace("node=", "\"node\":").Replace("is_wl=", "\"is_wl\":")
                                                  .Replace("\"is_wl\":T", "\"is_wl\":t").Replace("\"is_wl\":F", "\"is_wl\":f").Replace(")}", "}").Split('~');
                    _connDevs = new Dictionary<string, ConnectedDevices>();
                    for (int i = 1; i < pairs.Length; i += 2)
                    {
                        ConnectedDevices cd = JsonSerializer.Deserialize<ConnectedDevices>(pairs[i]);
                        _connDevs.Add(cd.mac, cd);
                    }
                    continue;
                }

                // {'cpu1_usage': 0.0, 'cpu2_usage': 0.0, 'cpu_total_usage': 0.0}
                if (st.StartsWith("MemoryUsage"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _memUsage = JsonSerializer.Deserialize<MemoryUsage>(jstr.Replace('\'', '"'));
                    continue;
                }

                if (st.StartsWith("CPUUsage"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _cpuUsage = JsonSerializer.Deserialize<CPUUsage>(jstr.Replace('\'', '"'));
                    continue;
                }

                if (st.StartsWith("TrafficBytes"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _trafficBytes = JsonSerializer.Deserialize<TrafficBytes>(jstr.Replace('\'', '"'));
                    continue;
                }

                if (st.StartsWith("UpTime"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _upTime = JsonSerializer.Deserialize<UpTime>(jstr.Replace('\'', '"'));
                    continue;
                }

                if (st.StartsWith("WANInfo"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _wanInfo = JsonSerializer.Deserialize<WANInfo>(jstr.Replace('\'', '"'));
                    continue;
                }

                if (st.StartsWith("CPUTemp"))
                {
                    int soff = st.IndexOf("{");
                    int eoff = st.LastIndexOf("}");
                    jstr = st.Substring(soff, (eoff - soff) + 1);
                    _cpuTemp = JsonSerializer.Deserialize<CPUTemp>(jstr.Replace('\'', '"'));
                    continue;
                }
            }
            return true;
        }

        public RouterStatus(RouterStatus srcRS)
        {
            _rsettings = srcRS._rsettings.ShallowCopy();
            _clients = new Dictionary<string, FullClientInfo>();
            foreach (var cl in srcRS._clients)
            {
                _clients.Add(cl.Key, cl.Value.ShallowCopy());
            }
            _connDevs = new Dictionary<string, ConnectedDevices>();
            foreach (var cd in srcRS._connDevs)
            {
                _connDevs.Add(cd.Key, cd.Value.ShallowCopy());
            }
            _memUsage = srcRS._memUsage.ShallowCopy();
            _cpuUsage = srcRS._cpuUsage.ShallowCopy();
            _trafficBytes = srcRS._trafficBytes.ShallowCopy();
            _upTime = srcRS._upTime.ShallowCopy();
            _wanInfo = srcRS._wanInfo.ShallowCopy();
            _cpuTemp = srcRS._cpuTemp.ShallowCopy();
        }

    }
}

