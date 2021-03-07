using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CStat.Common.EquipProp;
using CStat.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static CStat.Common.ArdMgr;

namespace CStat.Common
{
    public class CSSettings
    {
        static private CSSettings _gCSet = null;

        private static ReaderWriterLockSlim sfLock = new ReaderWriterLockSlim();
        private readonly IConfiguration _config;
        private readonly UserManager<CStatUser> _userManager;
        private const int MAX_EQUIP = 8;

        public const string green = "#00FF00";
        public const string yellow = "#FFFF00";
        public const string red = "#FF0000";
        public const string gray = "#C0C0C0";

        public DateTime LastTaskUpdate { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public List<CSUser> UserSettings { get; set; }
        public List<EquipProp> EquipProps { get; set; }
        public List<EquipProp> ActiveEquip { get; set; }

        public static CSSettings GetCSSettings()
        {
            if (_gCSet != null)
                return _gCSet;
            _gCSet = new CSSettings();
            return _gCSet;
        }

        public static CSSettings GetCSSettings(IConfiguration config, UserManager<CStatUser> userManager)
        {
            if (_gCSet != null)
                return _gCSet;
            _gCSet = new CSSettings(config, userManager);
            return _gCSet;
        }

        public static void ResetCSSettings()
        {
            //_gCSet = null;
        }

        public CSSettings()
        {
        }
        public CSSettings (IConfiguration config, UserManager<CStatUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            List<CStatUser> Users = GetUsersAsync(userManager).Result;
            var csect = config.GetSection("CSSettings");
            LastStockUpdate = csect.GetValue<DateTime>("LastStockUpdate");
            LastTaskUpdate = csect.GetValue<DateTime>("LastTaskUpdate");
            var ch = config.GetSection("CSSettings:UserSettings").GetChildren();
            UserSettings = new List<CSUser>();
            foreach (var c in ch)
            {
                var user = new CSUser();
                user.Name = c.GetValue<string>("Name");
                int cIndex = Users.FindIndex(u => u.UserName == user.Name);
                if (cIndex != -1)
                {
                    string PhoneNum = Users[cIndex].PhoneNumber;
                    user.PhoneNum = !String.IsNullOrEmpty(PhoneNum) ? PhoneNum : "";
                }
                user.ShowAllTasks = c.GetValue<bool>("ShowAllTasks");
                user.SendEquipText = c.GetValue<bool>("SendEquipText");
                user.SendStockText = c.GetValue<bool>("SendStockText");
                user.SendTaskText = c.GetValue<bool>("SendTaskText");
                user.SendEMailToo = c.GetValue<bool>("SendEMailToo");

                UserSettings.Add(user);
            }

            var eqProps = config.GetSection("CSSettings:EquipProps").GetChildren();

            GetEquipLists(config, out List<EquipProp> allEPs, out List<EquipProp> activeEPs);
            EquipProps = allEPs;
            ActiveEquip = activeEPs;

            if (UserSettings.Count == 0)
            {
                CSUser u1 = new CSUser
                {
                    Name = "ronripp@outlook.com",
                    ShowAllTasks = true,
                    SendEquipText = false,
                    SendStockText = false,
                    SendTaskText = false,
                    SendEMailToo = false
                };
                UserSettings.Add(u1);
                CSUser u2 = new CSUser
                {
                    Name = "ellenripp@gmail.com",
                    ShowAllTasks = false,
                    SendEquipText = false,
                    SendStockText = false,
                    SendTaskText = false,
                    SendEMailToo = false
                };
                UserSettings.Add(u2);
            }

            if (EquipProps.Count == 0)
            {
                EquipProp e1 = new EquipProp
                {
                    Active = true,
                    Title = "Walk-in Freezer",
                    PropName = "freezerTemp",
                    EquipUnits = EquipUnitsType.TemperatureF,
                    ChartBottom = 0,
                    ChartTop = 36,
                    RedBottom = 0,
                    RedTop = 30,
                    GreenBottom = 0,
                    GreenTop = 20,
                    MinsPerSample = 15
                };
                EquipProps.Add(e1);

                EquipProp e2 = new EquipProp
                {
                    Active = true,
                    Title = "Walk-in Frig.",
                    PropName = "frigTemp",
                    EquipUnits = EquipUnitsType.TemperatureF,
                    ChartBottom = 30,
                    ChartTop = 50,
                    RedBottom = 34,
                    RedTop = 45,
                    GreenBottom = 36,
                    GreenTop = 42,
                    MinsPerSample = 15
                };
                EquipProps.Add(e2);

                EquipProp e5 = new EquipProp
                {
                    Active = true,
                    Title = "Kitchen Temp",
                    PropName = "kitchTemp",
                    EquipUnits = EquipUnitsType.TemperatureF,
                    ChartBottom = 0,
                    ChartTop = 100,
                    RedBottom = 50,
                    RedTop = 95,
                    GreenBottom = 60,
                    GreenTop = 85,
                    MinsPerSample = 15
                };
                EquipProps.Add(e5);

                EquipProp e3 = new EquipProp
                {
                    Active = true,
                    Title = "Propane Tank",
                    PropName = "propaneTank",
                    EquipUnits = EquipUnitsType.PercentFull,
                    ChartBottom = 0,
                    ChartTop = 100,
                    RedBottom = 10,
                    RedTop = 0,
                    GreenBottom = 20,
                    GreenTop = 100,
                    MinsPerSample = 15
                };
                EquipProps.Add(e3);

                EquipProp e4 = new EquipProp
                {
                    Active = true,
                    Title = "Water Plumbing",
                    PropName = "waterPres",
                    EquipUnits = EquipUnitsType.PSI,
                    ChartBottom = 0,
                    ChartTop = 60,
                    RedBottom = 25,
                    RedTop = 55,
                    GreenBottom = 35,
                    GreenTop = 50,
                    MinsPerSample = 15
                };
                EquipProps.Add(e4);

                EquipProp e6 = new EquipProp
                {
                    Active = false,
                    Title = "Electric Power",
                    PropName = "?",
                    EquipUnits = EquipUnitsType.KWH,
                    ChartBottom = 0,
                    ChartTop = 100,
                    RedBottom = 0,
                    RedTop = 70,
                    GreenBottom = 0,
                    GreenTop = 50,
                    MinsPerSample = 15
                };
                EquipProps.Add(e6);
            }

            for (int i = EquipProps.Count; i < CSSettings.MAX_EQUIP; ++i)
            {
                EquipProps.Add(new EquipProp());
            }
        }   

        public static bool GetEquipLists (IConfiguration config, out List<EquipProp>allEquipProps, out List<EquipProp> activeEquipProps)
        {
            var eqProps = config.GetSection("CSSettings:EquipProps").GetChildren();
            allEquipProps = new List<EquipProp>();
            activeEquipProps = new List<EquipProp>();
            foreach (var e in eqProps)
            {
                EquipProp eqProp = new EquipProp();
                eqProp.Active = e.GetValue<bool>("Active");
                eqProp.Title = e.GetValue<string>("Title");
                eqProp.PropName = e.GetValue<string>("PropName");
                eqProp.EquipUnits = e.GetValue<EquipUnitsType>("EquipUnits"); // TBD enum with stable, supported api
                eqProp.ChartBottom = e.GetValue<double>("ChartBottom");
                eqProp.ChartTop = e.GetValue<double>("ChartTop");
                eqProp.RedBottom = e.GetValue<double>("RedBottom");
                eqProp.RedTop = e.GetValue<double>("RedTop");
                eqProp.GreenBottom = e.GetValue<double>("GreenBottom");
                eqProp.GreenTop = e.GetValue<double>("GreenTop");
                eqProp.MinsPerSample = e.GetValue<double>("MinsPerSample");
                if (eqProp.Active)
                    activeEquipProps.Add(eqProp);
                allEquipProps.Add(eqProp);
            }
            return eqProps.Count() > 0;
        }

        public CSUser GetUser(string userName)
        {
            if (UserSettings == null)
                return null;

            foreach(var u in UserSettings)
            {
                if (u.Name == userName)
                    return u;
            }
            return null;
        }

        public CSUser SetUser(string userName, CSUser modUser)
        {
            if (UserSettings == null)
                return null;
            int i = 0;
            for (i = 0; i < UserSettings.Count; ++i)
            {
                if (UserSettings[i].Name == userName)
                {
                    UserSettings[i] = modUser; // Replace existing user
                    break;
                }
            }
            if (i == UserSettings.Count)
                UserSettings.Add(modUser); // Add new user

            return null;
        }
        public bool Save()
        {
            bool res = _Save();
            ResetCSSettings();
            return res;
        }

        private bool _Save()
        {
            for (int i = 0; i < 4; ++i)
            {
                if (sfLock.TryEnterWriteLock(250))
                {
                    try
                    {
                        var json = JsonConvert.SerializeObject(new { CSSettings = this });
                        File.WriteAllText("CStat.json", json);
                        return true;
                    }
                    catch
                    {

                    }
                    finally
                    {
                        sfLock.ExitWriteLock();
                    }
                }
            }
            return false;
        }

        public static string GetColor(List<EquipProp>equipProps, string propName, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
           string ltPropName = propName.ToLower().Trim();
           if (ltPropName == "all")
                return GetEqColor(equipProps, null, ar, pl, returnClass);
            EquipProp ep = equipProps.Find(e => e.PropName.ToLower().Trim() == ltPropName);
            if (ep != null)
                return GetEqColor(equipProps, ep, ar, pl, returnClass);
            return (returnClass) ? "greenClass" : CSSettings.green;
        }

        public static double GetValue(string propName, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            string tPropName = propName.Trim();
            return tPropName switch
            {
                "freezerTemp" => (ar != null) ? ar.FreezerTempF : PropMgr.NotSet,
                "frigTemp" => (ar != null) ? ar.FridgeTempF : PropMgr.NotSet,
                "kitchTemp" => (ar != null) ? ar.KitchTempF : PropMgr.NotSet,
                "propaneTank" => (pl != null) ? pl.LevelPct : PropMgr.NotSet,
                "waterPres" => (ar != null) ? ar.WaterPress : PropMgr.NotSet,
                _ => PropMgr.NotSet,
            };
        }

        public static double GetEqValue(EquipProp ep, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            return (ep != null) ? GetValue(ep.PropName, ar, pl, returnClass) : PropMgr.NotSet;
        }

        public static string GetEqValueStr(EquipProp ep, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            double value = (ep != null) ? GetValue(ep.PropName, ar, pl, returnClass) : PropMgr.NotSet;
            return value.ToString("0.##") + " " + ep.EquipUnits.ToString();
        }

        public static string GetEqColor(List<EquipProp> equipProps, EquipProp ep, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            string color = "green";

            if (ep != null)
            {
                return ep.PropName switch
                {
                    "freezerTemp" => (ar != null) ? ep.GetColor(ar.FreezerTempF, returnClass) : CSSettings.red,
                    "frigTemp" => (ar != null) ? ep.GetColor(ar.FridgeTempF, returnClass) : CSSettings.red,
                    "kitchTemp" => (ar != null) ? ep.GetColor(ar.KitchTempF, returnClass) : CSSettings.red,
                    "propaneTank" => (pl != null) ? ep.GetColor(pl.LevelPct, returnClass) : CSSettings.red,
                    "waterPres" => (ar != null) ? ep.GetColor(ar.WaterPress, returnClass) : CSSettings.red,
                    _ => "",
                };
            }

            // Perform All
            if (equipProps.Count > 0)
            {
                string[] colors = equipProps.Select(e => e.GetColor(GetEqValue(e, ar, pl, returnClass), returnClass)).ToArray();
                if (colors.Any(c => c == CSSettings.red))
                    color = "red";
                else if (colors.Any(c => c == CSSettings.yellow))
                    color = "yellow";
            }

            if (returnClass)
                return color + "Class";
            else
            {
                return color switch
                {
                    "red" => CSSettings.red,
                    "yellow" => CSSettings.yellow,
                    "green" => CSSettings.green,
                    _ => "",
                };
            }
        }

        public static async Task<List<CStatUser>> GetUsersAsync(UserManager<CStatUser> userManager)
        {
            return await userManager.Users.ToListAsync();
        }
    }
}
