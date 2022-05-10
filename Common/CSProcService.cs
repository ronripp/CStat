using CStat.Areas.Identity.Data;
using CStat.Models;
using CStat.Pages.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using CTask = CStat.Models.Task;

namespace CStat.Common
{
    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
    internal class CSProcService : IScopedProcessingService
    {
        private readonly ILogger _logger;
        private readonly CStat.Models.CStatContext _context;
        private readonly IConfiguration _configuration;
        private readonly CSSettings _csSettings;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly UserManager<CStatUser> _userManager;

        public CSProcService(ILogger<CSProcService> logger, CStat.Models.CStatContext context, IConfiguration configuration, IWebHostEnvironment hostEnv, UserManager<CStatUser> userManager)
        {
            _hostEnv = hostEnv;
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _csSettings = CSSettings.GetCSSettings(_configuration, userManager);
        }

        public async Task DoWork(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime enow = PropMgr.ESTNow;
                DateTime expected = new DateTime(enow.Year, enow.Month, enow.Day, 3, 0, 0);
                DateTime lastW = (_csSettings.LastTaskUpdate != null) ? _csSettings.LastTaskUpdate : new DateTime(2020, 1, 1);
#if DEBUG
                double MinWait = 0; // for testing
#else
                double MinWait = 720; // 12 hrs
#endif
                if (((enow - lastW).TotalMinutes >= MinWait) || (Math.Abs((enow - expected).TotalMinutes) < 65))  // done not more than twice a day and covers DST change with delay/offset
                {
                    // Check Stock for possibly needed items
                    OrderedEvents ordEvs = new OrderedEvents(_context);
                    IList<InventoryItem> InventoryItems = _context.InventoryItem.Include(i => i.Inventory).Include(i => i.Item).ToList();
                    foreach (var invIt in InventoryItems)
                    {
                        if (IndexInvModel.MayNeedItem(_context, invIt, ordEvs))
                        {
                            // Item likely needs to be ordered. Mark as needed.
                            if (invIt.State == IndexInvModel.STOCKED_STATE)
                            {
                                invIt.State = IndexInvModel.NEEDED_STATE;
                                _context.Attach(invIt).State = EntityState.Modified;

                                try
                                {
                                    _context.SaveChanges();
                                    _csSettings.LastStockUpdate = PropMgr.ESTNow;
                                    _csSettings.Save();
                                    await Task.Run(() => IndexInvModel.NotifyNeedAsync(_hostEnv, _configuration, _userManager, "CStat:Stock> Needed : " + invIt.Item.Name, true)); // potentially clean Message log
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    continue;
                                }
                            }
                        }
                    }

                    // Check for new Tasks
                    AutoGen ag = new AutoGen(_context);
                    _csSettings.LastTaskUpdate = PropMgr.ESTNow;
                    _csSettings.Save();
                    ag.GenTasks(_hostEnv);

                    // Notify users Tasks Due
                    CTask.NotifyUserTaskDue(_hostEnv, _configuration, _userManager, _context, 24, true); // Potentially clean log

                    // Persist Daily Reading and Notify if needed for Propane
                    PropaneMgr pmgr = new PropaneMgr(_hostEnv, _configuration, _userManager);
                    pmgr.CheckValue(pmgr.GetTUTank(true)); // get value, log to file and check

                    // Check for any unexpected propane usage (not potentially impacted from events) from recent daily usage
                    var plList = pmgr.GetAll(3);
                    var plCnt = plList.Count;
                    if (plCnt > 1)
                    {
                        for (int i = 0; i < plCnt-1; ++i)
                        {
                            var plStart = plList[i];
                            var plEnd = plList[i+1];
                            var dateRange = new DateRange(plStart.ReadingTime, plEnd.ReadingTime);
                             
                            var evList = ag.GetImpactingEvents(dateRange);
                            if (evList.Count == 0) // no impacting events
                            {
                                var totalHrs = dateRange.TotalHours();
                                if ((totalHrs > 0) && _csSettings.GetPropaneProperties(out double tankGals, out double pricePerGal))
                                {
                                    var totalGals = ((plStart.LevelPct - plEnd.LevelPct) / 100) * tankGals;
                                    if ((totalGals / totalHrs) > ((double)2 / 24)) // averaging more than 2 gals/day?
                                    {
                                        // Send a one time alert (allowResend = false)
                                        CSSMS sms = new CSSMS(_hostEnv, _configuration, _userManager);
                                        sms.NotifyUsers(CSSMS.NotifyType.EquipNT, "CStat:Equip> " + "Non Event Propane Usage was " + totalGals.ToString("0.##") + " gals. during " + totalHrs.ToString("0.#") + " hours starting " +
                                            plStart.ReadingTime.Month + "/" + plStart.ReadingTime.Day + "/" + plStart.ReadingTime.Year % 100, false, true); // potentially clean Message Log
                                    }
                                }
                            }
                        }
                    }

                    // Check for new EMails to store
                    CSEMail.SaveEMails(_configuration, _userManager);

                    // Check/Truncate Size of Arduino file
                    ArdMgr amgr = new ArdMgr(_hostEnv, _configuration, _userManager);
                    amgr.GetAll(true);

                    // Clean/{Reset to full view} Camera
                    using (var ptz = new PtzCamera())
                    {
                        ptz.Cleanup(_hostEnv);
                    }

                    _logger.LogInformation($"CStat Daily Updates Completed at {PropMgr.ESTNow}");
                }

                // Run again at 3:00 AM
                DateTime now = PropMgr.ESTNow;
                DateTime tom = now.AddDays(1);
                DateTime start = new DateTime(tom.Year, tom.Month, tom.Day, 3, 0, 0); // Restart at 3:00 AM tomorrow
                int MSecsToStart = (int)Math.Round((start - now).TotalMilliseconds);
                await System.Threading.Tasks.Task.Delay(MSecsToStart, stoppingToken);
            }
        }
    }
}
