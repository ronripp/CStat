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

namespace CStat.Common
{
    public class CSSettings
    {
        private static ReaderWriterLockSlim sfLock = new ReaderWriterLockSlim();
        private readonly IConfiguration _config;
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

        public CSSettings()
        {
        }
        public CSSettings (IConfiguration config, UserManager<CStatUser> userManager)
        {
            _config = config;
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
                user.SendEquipEMail = c.GetValue<bool>("SendEquipEMail");
                UserSettings.Add(user);
            }
            var eqProps = config.GetSection("CSSettings:EquipProps").GetChildren();
            EquipProps = new List<EquipProp>();
            ActiveEquip = new List<EquipProp>();
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
                    ActiveEquip.Add(eqProp);
                EquipProps.Add(eqProp);
            }

            if (UserSettings.Count == 0)
            {
                CSUser u1 = new CSUser
                {
                    Name = "ronripp@outlook.com",
                    ShowAllTasks = true,
                    SendEquipEMail = true
                };
                UserSettings.Add(u1);
                CSUser u2 = new CSUser
                {
                    Name = "ellenripp@gmail.com",
                    ShowAllTasks = false,
                    SendEquipEMail = false
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
                    PropName = "?",
                    EquipUnits = EquipUnitsType.PercentFull,
                    ChartBottom = 0,
                    ChartTop = 100,
                    RedBottom = 0,
                    RedTop = 10,
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

        public string GetColor(string propName, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
           string ltPropName = propName.ToLower().Trim();
           if (ltPropName == "all")
                return GetEqColor(null, ar, pl, returnClass);
            EquipProp ep = ActiveEquip.Find(e => e.PropName.ToLower().Trim() == ltPropName);
            if (ep != null)
                return GetEqColor(ep, ar, pl, returnClass);
            return (returnClass) ? "greenClass" : CSSettings.green;
        }

        public double GetValue(string propName, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            string tPropName = propName.Trim();
            return tPropName switch
            {
                "freezerTemp" => ar.FreezerTempF,
                "frigTemp" => ar.FridgeTempF,
                "kitchTemp" => ar.KitchTempF,
                "propaneTank" => pl.LevelPct,
                "waterPres" => ar.WaterPress,
                _ => 0,
            };
        }

        public double GetEqValue(EquipProp ep, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            return (ep != null) ? GetValue(ep.PropName, ar, pl, returnClass) : 0;
        }

        public string GetEqColor(EquipProp ep, ArdRecord ar, PropaneLevel pl, bool returnClass = true)
        {
            string color = "green";

            if (ep != null)
            {
                return ep.PropName switch
                {
                    "freezerTemp" => ep.GetColor(ar.FreezerTempF, returnClass),
                    "frigTemp" => ep.GetColor(ar.FridgeTempF, returnClass),
                    "kitchTemp" => ep.GetColor(ar.KitchTempF, returnClass),
                    "propaneTank" => ep.GetColor(pl.LevelPct, returnClass),
                    "waterPres" => ep.GetColor(ar.WaterPress, returnClass),
                    _ => "",
                };
            }

            // Perform All
            if (ActiveEquip.Count > 0)
            {
                string[] colors = ActiveEquip.Select(e => e.GetColor(GetEqValue(e, ar, pl, returnClass), returnClass)).ToArray();
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

        public async Task<List<CStatUser>> GetUsersAsync(UserManager<CStatUser> userManager)
        {
            return await userManager.Users.ToListAsync();
        }
    }
}
