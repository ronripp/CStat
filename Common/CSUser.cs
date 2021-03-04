using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CSUser
    {
        public CSUser()
        {
        }
        public string Name { get; set; }
        public bool ShowAllTasks { get; set; }  = false;
        public bool SendEquipText { get; set; } = false;
        public bool SendStockText { get; set; } = false;
        public bool SendTaskText { get; set; } = false;
        public bool SendEMailToo { get; set; } = false;
        [JsonIgnore]
        public string PhoneNum { get; set; } = "";
    }
}
