using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CStat.Common.EquipProp;

namespace CStat.Common
{
    public class CSSettings
    {
        private readonly IConfiguration _config;
        public CSSettings (IConfiguration config)
        {
            _config = config;
            var csect = config.GetSection("CSSettings");
            LastStockUpdate = csect.GetValue<DateTime>("LastStockUpdate");
            LastTaskUpdate = csect.GetValue<DateTime>("LastTaskUpdate");
            var ch = config.GetSection("CSSettings:UserSettings").GetChildren();
            UserSettings = new List<CSUser>();
            foreach (var c in ch)
            {
                var user = new CSUser();
                user.Name = c.GetValue<string>("Name");
                user.ShowAllTasks = c.GetValue<bool>("ShowAllTasks");
                user.SendEquipEMail = c.GetValue<bool>("SendEquipEMail");
                UserSettings.Add(user);
            }
            var eqProps = config.GetSection("CSSettings:EquipProps").GetChildren();
            EquipProps = new List<EquipProp>();
            foreach (var e in eqProps)
            {
                EquipProp eqProp = new EquipProp();
                eqProp.Title = e.GetValue<string>("Title");
                eqProp.PropName = e.GetValue<string>("PropName");
                eqProp.EquipUnits = e.GetValue<int>("EquipUnit"); // TBD enum with stable, supported api
                eqProp.ChartBottom = e.GetValue<double>("ChartBottom");
                eqProp.ChartTop = e.GetValue<double>("ChartTop");
                eqProp.RedBottom = e.GetValue<double>("RedBottom");
                eqProp.RedTop = e.GetValue<double>("RedTop");
                eqProp.GreenBottom = e.GetValue<double>("GreenBottom");
                eqProp.GreenTop = e.GetValue<double>("GreenTop");
                eqProp.MinsPerSample = e.GetValue<double>("MinsPerSample");
                EquipProps.Add(eqProp);
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
        public void Save()
        {
            if (EquipProps.Count == 0)
            {
                EquipProp e1 = new EquipProp();
                e1.Title = "Walk-in Freezer";
                e1.PropName = "freezerTemp";
                e1.EquipUnits = (int)EquipUnitsType.TemperatureF;
                e1.ChartBottom = 0;
                e1.ChartTop = 36;
                e1.RedBottom = 0;
                e1.RedTop = 30;
                e1.GreenBottom = 0;
                e1.GreenTop = 20;
                e1.MinsPerSample = 15;
                EquipProps.Add(e1);

                EquipProp e2 = new EquipProp();
                e2.Title = "Walk-in Frig.";
                e2.PropName = "frigTemp";
                e2.EquipUnits = (int)EquipUnitsType.TemperatureF;
                e2.ChartBottom = 30;
                e2.ChartTop = 50;
                e2.RedBottom = 34;
                e2.RedTop = 45;
                e2.GreenBottom = 36;
                e2.GreenTop = 42;
                e2.MinsPerSample = 15;
                EquipProps.Add(e2);

                EquipProp e5 = new EquipProp();
                e5.Title = "Kitchen Temp";
                e5.PropName = "kitchTemp";
                e5.EquipUnits = (int)EquipUnitsType.TemperatureF;
                e5.ChartBottom = 0;
                e5.ChartTop = 100;
                e5.RedBottom = 50;
                e5.RedTop = 95;
                e5.GreenBottom = 60;
                e5.GreenTop = 85;
                e5.MinsPerSample = 15;
                EquipProps.Add(e5);

                EquipProp e3 = new EquipProp();
                e3.Title = "Propane Tank";
                e3.PropName = "?";
                e3.EquipUnits = (int)EquipUnitsType.PercentFull;
                e3.ChartBottom = 0;
                e3.ChartTop = 100;
                e3.RedBottom = 0;
                e3.RedTop = 10;
                e3.GreenBottom = 20;
                e3.GreenTop = 100;
                e3.MinsPerSample = 15;
                EquipProps.Add(e3);

                EquipProp e4 = new EquipProp();
                e4.Title = "Water Plumbing";
                e4.PropName = "waterPres";
                e4.EquipUnits = (int)EquipUnitsType.PSI;
                e4.ChartBottom = 0;
                e4.ChartTop = 60;
                e4.RedBottom = 25;
                e4.RedTop = 55;
                e4.GreenBottom = 35;
                e4.GreenTop = 50;
                e4.MinsPerSample = 15;
                EquipProps.Add(e4);

                EquipProp e6 = new EquipProp();
                e6.Title = "Electric Power";
                e6.PropName = "?";
                e6.EquipUnits = (int)EquipUnitsType.KWH;
                e6.ChartBottom = 0;
                e6.ChartTop = 100;
                e6.RedBottom = 0;
                e6.RedTop = 70;
                e6.GreenBottom = 0;
                e6.GreenTop = 50;
                e6.MinsPerSample = 15;
                EquipProps.Add(e6);
            }
            var json =JsonConvert.SerializeObject(new { CSSettings = this });
            File.WriteAllText("CStat.json", json);
        }

        public DateTime LastTaskUpdate { get; set; }
        public DateTime LastStockUpdate { get; set; }
        public List<CSUser> UserSettings { get; set; }
        public List<EquipProp> EquipProps { get; set; }
    }
}
