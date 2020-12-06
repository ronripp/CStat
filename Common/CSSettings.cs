using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CSSettings
    {
        public CSSettings (IConfiguration config)
        {
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
            var json =JsonConvert.SerializeObject(new { CSSettings = this });
            File.WriteAllText("CStat.json", json);
        }

        public DateTime LastTaskUpdate;
        public DateTime LastStockUpdate;
        public List<CSUser> UserSettings;
    }
}
