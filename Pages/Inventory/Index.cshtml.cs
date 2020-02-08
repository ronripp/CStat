using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;

namespace CStat
{
    public class IndexInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public class InvItemState
        {
            public int invItemId = -1;
            public int state = 0;
            public int pid = -1;
            public string displayName = "";
            public string btnClass = "";
        }

        public IndexInvModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<InventoryItem> InventoryItems { get;set; }

        public async Task OnGetAsync()
        {
            InventoryItems = await _context.InventoryItem
                .Include(i => i.Inventory)
                .Include(i => i.Item).ToListAsync();
        }

        public Person GetPersonFromEMail (string email)
        {
            Person person = _context.Person.FirstOrDefault(m => m.Email == email);
            return person;
        }

        public string MakeAbbr(Person p)
        {
            if (p != null)
                return p.FirstName.Substring(0, Math.Min(p.FirstName.Length, 5)) + p.LastName.Substring(0, Math.Min(p.LastName.Length, 1));
            return ("Unknown");
        }

        public string GetDisplayNameByPid(int pid)
        {
            Person person = _context.Person.FirstOrDefault(m => m.Id == pid);
            return MakeAbbr(person);
        }

        public InvItemState GetInvItemState (InventoryItem invItem)
        {
            InvItemState invIS = new InvItemState();

            // Get item DisplayName
            invIS.pid = -1;
            invIS.displayName = "Need?";
            invIS.state = (int)InventoryItem.States.InStock;
            invIS.btnClass = "InStockBtn";
            if (invItem.PersonId.HasValue)
            {
                invIS.pid = invItem.PersonId.Value;
                invIS.displayName = GetDisplayNameByPid(invIS.pid);
            }

            if (invItem.State.HasValue)
            {
                invIS.state = invItem.State.Value;
                switch (invIS.state)
                {
                    case (int)InventoryItem.States.InStock:
                        invIS.btnClass = "InStockBtn";
                        invIS.displayName = "Need?";
                        break;
                    case (int)InventoryItem.States.OpenNeed:
                        invIS.btnClass = "OpenNeedBtn";
                        invIS.displayName = "Take it";
                        break;
                    case (int)InventoryItem.States.TakenNeed:
                        invIS.btnClass = "TakenNeedBtn";
                        break;
                }
            }
            return invIS;
        }

        public JsonResult OnGetItemStateChange()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

//            Dictionary<string, int> NVPairs = JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonQS);
//            if (NVPairs.TryGetValue("invItemId", out int invItmId) && NVPairs.TryGetValue("pid", out int pid) && NVPairs.TryGetValue("state", out int state))

            InvItemState invIS = JsonConvert.DeserializeObject<InvItemState>(jsonQS);
            if (invIS != null)
            {
                InventoryItem invItem = _context.InventoryItem.FirstOrDefault(m => m.ItemId == invIS.invItemId);
                if (invItem != null)
                {
                    invItem.State = invIS.state;
                    if (invIS.pid > 0)
                       invItem.PersonId = invIS.pid;
                }
                _context.Attach(invItem).State = EntityState.Modified;

                try
                {
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return new JsonResult("ERROR~:Update DB Failed"); // TBD
                }

                return new JsonResult(JsonConvert.SerializeObject(GetInvItemState(invItem)));
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }
    }
}
