using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Newtonsoft.Json.Linq;

namespace CStat.Pages
{
    public class MergeModel : PageModel
    {
        private readonly CStat.Models.CStatContext _ctx;

        public MergeModel(CStat.Models.CStatContext context)
        {
            _ctx = context;
        }

        [BindProperty]
        public Person Person { get; set; }

        [BindProperty]
        public int Id { get; set; } = 1;

        public IList<Person> People { get; set; }

        public IList<Address> Address { get; set; }

        public IList<Church> Church { get; set; }

        public IActionResult OnGet(int id)
        {
            Id = id;
            switch (Id)
            {
                default:
                case 1:
                    People = _ctx.Person.AsNoTracking().OrderBy(p => p.LastName).ThenBy(p => p.FirstName).ToList();
                    break;
                case 2:
                    Address = _ctx.Address.AsNoTracking().OrderBy(p => p.Town).ThenBy(p => p.Street).ToList();
                    break;
                case 3:
                    Church = _ctx.Church.AsNoTracking().OrderBy(p => p.Name).ToList();
                    break;
            }

            //id = 3025;
            //Person = await _ctx.Person
            //    .Include(p => p.Address)
            //    .Include(p => p.Church)
            //    .Include(p => p.Pg1Person)
            //    .Include(p => p.Pg2Person).FirstOrDefaultAsync(m => m.Id == id);

           //ViewData["AddressId"] = new SelectList(_ctx.Address, "Id", "Country");
           //ViewData["ChurchId"] = new SelectList(_ctx.Church, "Id", "Affiliation");
           //ViewData["Pg1PersonId"] = new SelectList(_ctx.Person, "Id", "FirstName");
           //ViewData["Pg2PersonId"] = new SelectList(_ctx.Person, "Id", "FirstName");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _ctx.Attach(Person).State = EntityState.Modified;

            try
            {
                await _ctx.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(Person.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PersonExists(int id)
        {
            return _ctx.Person.Any(e => e.Id == id);
        }
        public JsonResult OnGetDoMerge()
        {
            var rawQS = Uri.UnescapeDataString(Request.QueryString.ToString());
            if (String.IsNullOrEmpty(rawQS))
                return new JsonResult("ERROR~:No Parameters");

            int stIdx = rawQS.IndexOf("{'cmd");
            if (stIdx == -1)
                return new JsonResult("ERROR~:No Parameters");
            var jsonQS = rawQS.Substring(stIdx);
            try
            {
                JObject jObj = JObject.Parse(jsonQS);
                string cmdStr = (string)jObj["cmd"] ?? "";

                var allStrs = cmdStr.Split(",");
                int toIdx = 0;
                int cmd = 0;
                int valType = 0;
                var fromIdxList = new List<int>();
                foreach (var str in allStrs)
                {
                    if (str == "C")
                        valType = 1;
                    else if (str == "F")
                        valType = 2;
                    else if (str == "T")
                        valType = 3;
                    else
                    {
                        switch (valType)
                        {
                            case 1:
                                cmd = int.Parse(str);
                                break;
                            case 2:
                                fromIdxList.Add(int.Parse(str));
                                break;
                            case 3:
                                toIdx = int.Parse(str);
                                break;
                            default:
                            case 0:
                                break;
                        }
                    }
                }

                var res = MergeRecords(cmd, fromIdxList, toIdx);

                return new JsonResult("OK~:Cleanup");
            }
            catch (Exception e)
            {
                _ = e;
                return new JsonResult("ERROR~: OnGetCamCleanup Exception");
            }
        }

        public int MergeRecords(int cmd, List<int> fromIdxList, int toIdx)
        {
            try
            {
                switch (cmd)
                {
                case 1:
                    MergeToPerson(fromIdxList, toIdx);
                    break;
                case 2:
                    MergeToAddress(fromIdxList, toIdx);
                    break;
                case 3:
                    MergeToChurch(fromIdxList, toIdx);
                    break;
                }
            }
            catch (Exception e)
            {
                _ = e;
            }

            return 0;
        }

        public int MergeToPerson(List<int> fromIdxList, int toIdx) //*************************
        {
            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("Attendance")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var alist = _ctx.Attendance.AsNoTracking().Where(a => a.PersonId == fidx);
                foreach (var a in alist)
                {
                    a.PersonId = toIdx;
                    _ctx.Attendance.Attach(a);
                    _ctx.Entry(a).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(PocId))]
            //[InverseProperty(nameof(Person.Business))]
            //public virtual Person Poc { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var blist = _ctx.Business.AsNoTracking().Where(b => b.PocId == fidx);
                foreach (var b in blist)
                {
                    b.PocId = toIdx;
                    _ctx.Business.Attach(b);
                    _ctx.Entry(b).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Alternate1Id))]
            //[InverseProperty("ChurchAlternate1")]
            //public virtual Person Alternate1 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Alternate1Id == fidx);
                foreach (var c in clist)
                {
                    c.Alternate1Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Alternate2Id))]
            //[InverseProperty("ChurchAlternate2")]
            //public virtual Person Alternate2 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Alternate2Id == fidx);
                foreach (var c in clist)
                {
                    c.Alternate2Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(Alternate3Id))]
            //[InverseProperty("ChurchAlternate3")]
            //public virtual Person Alternate3 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Alternate3Id == fidx);
                foreach (var c in clist)
                {
                    c.Alternate3Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Elder1Id))]
            //[InverseProperty("ChurchElder1")]
            //public virtual Person Elder1 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Elder1Id == fidx);
                foreach (var c in clist)
                {
                    c.Elder1Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Elder2Id))]
            //[InverseProperty("ChurchElder2")]
            //public virtual Person Elder2 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Elder2Id == fidx);
                foreach (var c in clist)
                {
                    c.Elder2Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Elder3Id))]
            //[InverseProperty("ChurchElder3")]
            //public virtual Person Elder3 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Elder3Id == fidx);
                foreach (var c in clist)
                {
                    c.Elder3Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(Elder4Id))]
            //[InverseProperty("ChurchElder4")]
            //public virtual Person Elder4 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Elder4Id == fidx);
                foreach (var c in clist)
                {
                    c.Elder4Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Elder5Id))]
            //[InverseProperty("ChurchElder5")]
            //public virtual Person Elder5 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Elder5Id == fidx);
                foreach (var c in clist)
                {
                    c.Elder5Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(SeniorMinisterId))]
            //[InverseProperty("ChurchSeniorMinister")]
            //public virtual Person SeniorMinister { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.SeniorMinisterId == fidx);
                foreach (var c in clist)
                {
                    c.SeniorMinisterId = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Trustee1Id))]
            //[InverseProperty("ChurchTrustee1")]
            //public virtual Person Trustee1 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Trustee1Id == fidx);
                foreach (var c in clist)
                {
                    c.Trustee1Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Trustee2Id))]
            //[InverseProperty("ChurchTrustee2")]
            //public virtual Person Trustee2 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Trustee2Id == fidx);
                foreach (var c in clist)
                {
                    c.Trustee2Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Trustee3Id))]
            //[InverseProperty("ChurchTrustee3")]
            //public virtual Person Trustee3 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.Trustee3Id == fidx);
                foreach (var c in clist)
                {
                    c.Trustee3Id = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(YouthMinisterId))]
            //[InverseProperty("ChurchYouthMinister")]
            //public virtual Person YouthMinister { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var clist = _ctx.Church.AsNoTracking().Where(c => c.YouthMinisterId == fidx);
                foreach (var c in clist)
                {
                    c.YouthMinisterId = toIdx;
                    _ctx.Church.Attach(c);
                    _ctx.Entry(c).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Persion3Id))]
            //[InverseProperty(nameof(Person.IncidentPersion3))]
            //public virtual Person Persion3 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.Incident.AsNoTracking().Where(i => i.Persion3Id == fidx);
                foreach (var i in ilist)
                {
                    i.Persion3Id = toIdx;
                    _ctx.Incident.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Person1Id))]
            //[InverseProperty(nameof(Person.IncidentPerson1))]
            //public virtual Person Person1 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.Incident.AsNoTracking().Where(i => i.Person1Id == fidx);
                foreach (var i in ilist)
                {
                    i.Person1Id = toIdx;
                    _ctx.Incident.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Person2Id))]
            //[InverseProperty(nameof(Person.IncidentPerson2))]
            //public virtual Person Person2 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.Incident.AsNoTracking().Where(i => i.Person2Id == fidx);
                foreach (var i in ilist)
                {
                    i.Person2Id = toIdx;
                    _ctx.Incident.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Person4Id))]
            //[InverseProperty(nameof(Person.IncidentPerson4))]
            //public virtual Person Person4 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.Incident.AsNoTracking().Where(i => i.Person4Id == fidx);
                foreach (var i in ilist)
                {
                    i.Person4Id = toIdx;
                    _ctx.Incident.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Person5Id))]
            //[InverseProperty(nameof(Person.IncidentPerson5))]
            //public virtual Person Person5 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.Incident.AsNoTracking().Where(i => i.Person5Id == fidx);
                foreach (var i in ilist)
                {
                    i.Person5Id = toIdx;
                    _ctx.Incident.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("InventoryItem")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var ilist = _ctx.InventoryItem.AsNoTracking().Where(i => i.PersonId == fidx);
                foreach (var i in ilist)
                {
                    i.PersonId = toIdx;
                    _ctx.InventoryItem.Attach(i);
                    _ctx.Entry(i).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("Medical")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var mlist = _ctx.Medical.AsNoTracking().Where(m => m.PersonId == fidx);
                foreach (var m in mlist)
                {
                    m.PersonId = toIdx;
                    _ctx.Medical.Attach(m);
                    _ctx.Entry(m).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("Operations")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var oplist = _ctx.Operations.AsNoTracking().Where(op => op.PersonId == fidx);
                foreach (var op in oplist)
                {
                    op.PersonId = toIdx;
                    _ctx.Operations.Attach(op);
                    _ctx.Entry(op).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(Pg1PersonId))]
            //[InverseProperty(nameof(Person.InversePg1Person))]
            //public virtual Person Pg1Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var plist = _ctx.Person.AsNoTracking().Where(p => p.Pg1PersonId == fidx);
                foreach (var p in plist)
                {
                    p.Pg1PersonId = toIdx;
                    _ctx.Person.Attach(p);
                    _ctx.Entry(p).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Pg2PersonId))]
            //[InverseProperty(nameof(Person.InversePg2Person))]
            //public virtual Person Pg2Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var plist = _ctx.Person.AsNoTracking().Where(p => p.Pg2PersonId == fidx);
                foreach (var p in plist)
                {
                    p.Pg2PersonId = toIdx;
                    _ctx.Person.Attach(p);
                    _ctx.Entry(p).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("Registration")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var rlist = _ctx.Registration.AsNoTracking().Where(r => r.PersonId == fidx);
                foreach (var r in rlist)
                {
                    r.PersonId = toIdx;
                    _ctx.Registration.Attach(r);
                    _ctx.Entry(r).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            //[ForeignKey(nameof(PersonId))]
            //[InverseProperty("TaskPerson")]
            //public virtual Person Person { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var tlist = _ctx.Task.AsNoTracking().Where(t => t.PersonId == fidx);
                foreach (var t in tlist)
                {
                    t.PersonId = toIdx;
                    _ctx.Task.Attach(t);
                    _ctx.Entry(t).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Worker1Id))]
            //[InverseProperty("TaskWorker1")]
            //public virtual Person Worker1 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var tlist = _ctx.Task.AsNoTracking().Where(t => t.Worker1Id == fidx);
                foreach (var t in tlist)
                {
                    t.Worker1Id = toIdx;
                    _ctx.Task.Attach(t);
                    _ctx.Entry(t).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Worker2Id))]
            //[InverseProperty("TaskWorker2")]
            //public virtual Person Worker2 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var tlist = _ctx.Task.AsNoTracking().Where(t => t.Worker2Id == fidx);
                foreach (var t in tlist)
                {
                    t.Worker2Id = toIdx;
                    _ctx.Task.Attach(t);
                    _ctx.Entry(t).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(Worker3Id))]
            //[InverseProperty("TaskWorker3")]
            //public virtual Person Worker3 { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var tlist = _ctx.Task.AsNoTracking().Where(t => t.Worker3Id == fidx);
                foreach (var t in tlist)
                {
                    t.Worker3Id = toIdx;
                    _ctx.Task.Attach(t);
                    _ctx.Entry(t).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }
            //[ForeignKey(nameof(CcaPersonId))]
            //[InverseProperty(nameof(Person.Transaction))]
            //public virtual Person CcaPerson { get; set; }
            foreach (var fidx in fromIdxList)
            {
                var tlist = _ctx.Transaction.AsNoTracking().Where(t => t.CcaPersonId == fidx);
                foreach (var t in tlist)
                {
                    t.CcaPersonId = toIdx;
                    _ctx.Transaction.Attach(t);
                    _ctx.Entry(t).State = EntityState.Modified;
                    _ctx.SaveChanges();
                }
            }

            // Delete merged persons now no records reference them
            foreach (var fidx in fromIdxList)
            {
                Person p = _ctx.Person.AsNoTracking().FirstOrDefault(p => (p.Id == fidx));
                if (p != null)
                {
                    _ctx.Person.Remove(p);
                    _ctx.SaveChanges();
                }
            }

            return 0;
        }

        public int MergeToAddress(List<int> fromIdxList, int toIdx)
        {
            //[ForeignKey(nameof(AddressId))]
            //[InverseProperty("Business")]
            //public virtual Address Address { get; set; }

            //[ForeignKey(nameof(AddressId))]
            //[InverseProperty("Church")]
            //public virtual Address Address { get; set; }

            //[ForeignKey(nameof(AddressId))]
            //[InverseProperty("Person")]
            //public virtual Address Address { get; set; }

            return 0;
        }


        public int MergeToChurch(List<int> fromIdxList, int toIdx)
        {
            //[ForeignKey(nameof(ChurchId))]
            //[InverseProperty("Event")]
            //public virtual Church Church { get; set; }            
                
            //[ForeignKey(nameof(ChurchId))]
            //[InverseProperty("Operations")]
            //public virtual Church Church { get; set; }
            
            //[ForeignKey(nameof(ChurchId))]
            //[InverseProperty("Person")]
            //public virtual Church Church { get; set; }
            
            //[ForeignKey(nameof(ChurchId))]
            //[InverseProperty("Task")]
            //public virtual Church Church { get; set; }
            
            //[ForeignKey(nameof(ChurchId))]
            //[InverseProperty("Transaction")]
            //public virtual Church Church { get; set; }

            return 0;
        }


    }
}
