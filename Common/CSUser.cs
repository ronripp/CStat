using CStat.Models;
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
        public string EMail { get; set; }
        public string Alias { get; set; }
        public bool ShowAllTasks { get; set; }  = false;
        public bool SendEquipText { get; set; } = false;
        public bool SendStockText { get; set; } = false;
        public bool SendTaskText { get; set; } = false;
        public bool SendEMailToo { get; set; } = false;
        [JsonIgnore]
        public string PhoneNum { get; set; } = "";
        public int? pid = null;
        public bool SetPersonIDByEmail (CStat.Models.CStatContext context)
        {
            var lcEMail = !string.IsNullOrEmpty(EMail) ? EMail.ToLower() : "EMPTY";
            List<Person> pList = context.Person.Where(p => !string.IsNullOrEmpty(p.Email) && lcEMail == p.Email.ToLower()).ToList<Person>();
            if (pList.Count == 1)
            {
                pid = pList[0].Id;
                return true;
            }
            return false;
        }
    }
}
