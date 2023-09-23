using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Newtonsoft.Json;
using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace CStat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CStatController : ControllerBase
    {
        private readonly CStatContext entities;
        private readonly IWebHostEnvironment _hostEnv;

        public CStatController(CStatContext context, IWebHostEnvironment hostEnv)
        {
            entities = context;
            _hostEnv = hostEnv;
        }

        // GET: api/CStat
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Person>>> GetPerson()
        {
            return await entities.Person.ToListAsync();
        }

        //*************************************
        // GET: api/CStat/video
        //*************************************
        [HttpGet]
        [Route("api/CStatAAA")]
        public FileResult getVideoById ()
        {
            string fileName = "video\\southford.mp4";
            string physPath = Path.Combine(_hostEnv.WebRootPath, fileName);
            var content = new FileStream(physPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var response = File(content, "application/octet-stream");
            return response;
        }

        // GET: api/CStat/5
        [HttpGet("{id}")]
        [Obsolete]
        //        public async Task<ActionResult<Person>> GetPerson(int id)
        public string Get(string id) //********************************************************
        {
           if (id == null)
               return "Bad Get id";

           id = id.Replace("@@", "*");

           String MatchingChurchesStr = "Matching_Churches:";
           int MatchingChurchesIndex = id.IndexOf(MatchingChurchesStr);
           if (MatchingChurchesIndex >= 0)
           {
               id = id.Trim();
               String ChurchHint = id.Substring(MatchingChurchesIndex + MatchingChurchesStr.Length).Trim();
               ChurchMgr cmgr = new ChurchMgr(entities);
               return cmgr.FindChurches(ChurchHint);
           }

           String MatchingEventsStr = "Matching_Events:";
           int MatchingEventsIndex = id.IndexOf(MatchingEventsStr);
           if (MatchingEventsIndex >= 0)
           {
               id = id.Trim();
               String EventHint = id.Substring(MatchingEventsIndex + MatchingEventsStr.Length).Trim();
               EventMgr emgr = new EventMgr(entities);
               return emgr.FindEvents(EventHint);
           }

           String DeleteEventStr = "Delete_Event:";
           int DeleteEventIndex = id.IndexOf(DeleteEventStr);
           if (DeleteEventIndex >= 0)
           {
               id = id.Trim();
               String idStr = id.Substring(DeleteEventIndex + DeleteEventStr.Length).Trim();
               EventMgr emgr = new EventMgr(entities);
               if (idStr.Length > 0)
               {
                   int idVal = Int32.Parse(idStr);
                   return emgr.DeleteEvent(idVal);
               }
               else
               {
                   return "Missing Event id to Delete";
               }
           }

           // FindAttendance:id=

           // Matching_Attendees:eid=
           // Matching_Attendees:" + "eyear = " + ystr + "; etype = " + DescStr;
           String MatchingAttendeesStr = "Matching_Attendees:";
           int MatchingAttendeesIndex = id.IndexOf(MatchingAttendeesStr);
           if (MatchingAttendeesIndex >= 0)
           {
               id = id.Trim();
               var idLen = id.Length;
               String AttendeeHint = id.Substring(Math.Min(idLen-1, MatchingAttendeesStr.Length)).Trim();
               AttendanceMgr amgr = new AttendanceMgr(entities);
               return amgr.FindAttendees(AttendeeHint);
           }

           String DeleteChurchStr = "Delete_Church:";
           int DeleteChurchIndex = id.IndexOf(DeleteChurchStr);
           if (DeleteChurchIndex >= 0)
           {
               id = id.Trim();
               String idStr = id.Substring(DeleteChurchIndex + DeleteChurchStr.Length).Trim();
               ChurchMgr cmgr = new ChurchMgr(entities);
               if (idStr.Length > 0)
               {
                   int idVal = Int32.Parse(idStr);
                   return cmgr.DeleteChurch(idVal);
               }
               else
               {
                   return "Missing Church id to Delete";
               }
           }

           String MatchingNamesStr = "Matching_Names:";
           int MatchingNamesIndex = id.IndexOf("Matching_Names:");
           if (MatchingNamesIndex >= 0)
           {
               id = id.Trim();
               String FNTarget = id.Substring(MatchingNamesIndex + MatchingNamesStr.Length).Trim();
               String LNTarget;
               if (FNTarget.Length == 0)
                   return "No Name to Match";
               int SPIndex = FNTarget.LastIndexOf(' ');
               if (SPIndex != -1)
               {
                   LNTarget = FNTarget.Substring(SPIndex + 1).Trim();
                   FNTarget = FNTarget.Substring(0, SPIndex).Trim();
               }
               else
                   LNTarget = "";

               List<PInfo> PInfoList = new List<PInfo>();
               String falias = "F:" + FNTarget;
               if (LNTarget.Length > 0)
               {
                   String lalias = "L:" + LNTarget;
                   var PInfoListFL = (from p in entities.Person
                                      where (p.FirstName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias))) &&
                                      (p.LastName.ToLower().StartsWith(LNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == lalias)))
                                      join a in entities.Address on
                                      p.AddressId equals a.Id
                                      join c in entities.Church on
                                      p.ChurchId equals c.Id into pc
                                      from pcn in pc.DefaultIfEmpty()
                                      select new { p.Id, p.FirstName, p.LastName, p.Ssnum, a.ZipCode, pcn.Name, p.Alias }).ToList();

                   var jsonPG = JsonConvert.SerializeObject(PInfoListFL, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                   return jsonPG.ToString();
               }
               else
               {
                   var PInfoListF = (from p in entities.Person
                                     where (p.FirstName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias))) ||
                                     (p.LastName.ToLower().StartsWith(FNTarget.ToLower()) || ((p.Alias != null) && (p.Alias == falias)))
                                     join a in entities.Address on
                                     p.AddressId equals a.Id
                                     join c in entities.Church on
                                     p.ChurchId equals c.Id into pc
                                     from pcn in pc.DefaultIfEmpty()
                                     select new { p.Id, p.FirstName, p.LastName, p.Ssnum, a.ZipCode, pcn.Name }).ToList();

                   var jsonPG = JsonConvert.SerializeObject(PInfoListF, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                   return jsonPG.ToString();
               }
           }

           return Person.FindPeople(entities, id);
        }

        // PUT: api/CStat/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerson(int id, Person person)
        {
            if (id != person.Id)
            {
                return BadRequest();
            }

            entities.Entry(person).State = EntityState.Modified;

            try
            {
                await entities.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PersonExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CStat
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Person>> PostPerson(Person person)
        {
            entities.Person.Add(person);
            await entities.SaveChangesAsync();

            return CreatedAtAction("GetPerson", new { id = person.Id }, person);
        }

        // DELETE: api/CStat/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Person>> DeletePerson(int id)
        {
            var person = await entities.Person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }

            entities.Person.Remove(person);
            await entities.SaveChangesAsync();

            return person;
        }

        private bool PersonExists(int id)
        {
            return entities.Person.Any(e => e.Id == id);
        }
    }

    public static class SqlQueryExtensions
    {
        public static IList<T> SqlQuery<T>(this DbContext db, string sql, params object[] parameters) where T : class
        {
            using (var db2 = new ContextForQueryType<T>((DbConnection)db.Database.GetDbConnection()))
            {
                return db2.Set<T>().FromSqlRaw(sql, parameters).ToList();
            }
        }

        private class ContextForQueryType<T> : DbContext where T : class
        {
            private readonly DbConnection connection;

            public ContextForQueryType(DbConnection connection)
            {
                this.connection = connection;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure());

                base.OnConfiguring(optionsBuilder);
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<T>().HasNoKey();
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
