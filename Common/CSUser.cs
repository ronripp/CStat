using System;
using System.Collections.Generic;
using System.Linq;
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
        public bool SendEquipEMail { get; set; } = true;
    }
}
