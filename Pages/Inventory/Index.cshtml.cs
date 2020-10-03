using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CStat.Common;
using Task = System.Threading.Tasks.Task;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Internal;

namespace CStat
{
    public class IndexInvModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public static int STOCKED_STATE = 0;
        public static int NEEDED_STATE = 1;
        public static int ASSIGNED_STATE = 2;
        public static int INV_STATE = 3;

        public class InvItemState
        {
            public int invItemId = -1;
            public int state = 0;
            public int pid = -1;
            public string displayName = "";
            public string btnClass = "";
        }
        public class InvItemStock
        {
            public int invItemId = -1;
            public float stock = 0;
        }

        public class CurState
        {
            public InvItemState invItemState;
            public int allState = STOCKED_STATE;
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
                    case (int)InventoryItem.States.InInv:
                        invIS.btnClass = "InStockBtn";
                        invIS.displayName = "Need?";
                        break;
                    case (int)InventoryItem.States.OpenNeed:
                        invIS.btnClass = "OpenNeedBtn";
                        invIS.displayName = "Take";
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

                CurState cs = new CurState
                {
                    invItemState = GetInvItemState(invItem),
                    allState = GetAllState()
                };
                return new JsonResult(JsonConvert.SerializeObject(cs));
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }

        public JsonResult OnGetSetInvMode()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, bool> setParam = JsonConvert.DeserializeObject<Dictionary<string, bool>>(jsonQS);
            if (setParam.TryGetValue("set", out bool IsSet))
            {
                int TargetState = IsSet ? STOCKED_STATE : INV_STATE;
                int NewState = IsSet ? INV_STATE : STOCKED_STATE;

                if (IsSet)
                    InventoryItems = _context.InventoryItem.Where(ii => (ii.State == TargetState) || (ii.State == null)).ToList();
                else
                    InventoryItems = _context.InventoryItem.Where(ii => ii.State == TargetState).ToList();

                foreach (var ii in InventoryItems)
                {
                     ii.State = NewState;
                    _context.Attach(ii).State = EntityState.Modified;

                    try
                    {
                        _context.SaveChanges();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        return new JsonResult("ERROR~:Update DB Failed"); // TBD
                    }
                }
                CurState cs = new CurState
                {
                    invItemState = null,
                    allState = GetAllState()
                };
                return new JsonResult(JsonConvert.SerializeObject(cs));
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }

        public JsonResult OnGetEMailInv() 
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No user provided");
            var jsonQS = rawQS.Substring(idx);
            Dictionary<string, string> setParam = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonQS);
            if (setParam.TryGetValue("user", out string user))
            {
                InventoryItems = _context.InventoryItem.ToList();
            }

            return new JsonResult("ERROR~:Not Implemented");
        }

        public int GetAllState()
        {
            return (_context.InventoryItem.FirstOrDefault(ii => ii.State == INV_STATE) != null) ? INV_STATE : STOCKED_STATE;
        }

        public JsonResult OnGetItemStockChange()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            var idx = rawQS.IndexOf('{');
            if (idx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(idx);

            InvItemStock invItmStk = JsonConvert.DeserializeObject<InvItemStock>(jsonQS);
            if (invItmStk != null)
            { 
                InventoryItem invItem = _context.InventoryItem.FirstOrDefault(m => m.ItemId == invItmStk.invItemId);
                if (invItem != null)
                {
                    if (invItem.State == INV_STATE)
                        invItem.State = STOCKED_STATE;
                    invItem.CurrentStock = Math.Max(invItmStk.stock, (float)0);
                    invItem.Date = DateTime.Now;
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

                CurState cs = new CurState
                {
                    invItemState = (invItem != null) ? GetInvItemState(invItem) : null,
                    allState = GetAllState()
                };
                return new JsonResult(JsonConvert.SerializeObject(cs));
            }
            return new JsonResult("ERROR~:Incorrect Parameters");
        }
    }
}

