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
        public int? pid { get; set; } = null;
        public string UserId { get; set; } = "";
        public bool IsFull { get; set; } = false;
        public bool IsAdmin { get; set; } = false;
        public bool SetPersonIDByEmailPhoneAlias (CStat.Models.CStatContext context)
        {
            var lcEMail = !string.IsNullOrEmpty(EMail) ? EMail.ToLower() : "EMPTY";
            List<Person> pList = context.Person.Where(p => !string.IsNullOrEmpty(p.Email) && lcEMail == p.Email.ToLower()).ToList<Person>();
            if (pList.Count == 1)
            {
                pid = pList[0].Id;
                return true;
            }

            // Try to narrow down by phone #
            var lcCell = !string.IsNullOrEmpty(PhoneNum) ? PhoneNum : "";
            if (!String.IsNullOrEmpty(lcCell))
            {
                List<Person> pList2 = pList.Where(p => CCommon.SamePhone(lcCell, p.CellPhone)).ToList<Person>();
                if (pList2.Count == 1)
                {
                    pid = pList2[0].Id;
                    return true;
                }
            }

            // Try to narrow down by Alias associated with First name
            var lcFN = !string.IsNullOrEmpty(Alias) ? Alias : "E!MPTY";
            if (!String.IsNullOrEmpty(lcFN))
            {
                List<Person> pList3 = pList.Where(p => Person.SameFirstName(lcFN, p.FirstName)).ToList<Person>();
                if (pList3.Count == 1)
                {
                    pid = pList3[0].Id;
                    return true;
                }
            }

            return false;
        }
    }
}
