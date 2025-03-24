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
using System.IO;

namespace CStat.Common
{
    internal interface IScopedProcessingService
    {
        Task DoWork(CancellationToken stoppingToken);
    }
    internal class CSProcService : IScopedProcessingService
    {
        private static long CSPInstCnt = 0; 
        private readonly ILogger _logger;
        private readonly CStat.Models.CStatContext _context;
        private readonly IConfiguration _configuration;
        private CSSettings _csSettings;
        private readonly IWebHostEnvironment _hostEnv;
        private readonly UserManager<CStatUser> _userManager;

        public CSProcService(ILogger<CSProcService> logger, CStat.Models.CStatContext context, IConfiguration configuration, IWebHostEnvironment hostEnv, UserManager<CStatUser> userManager)
        {
            _hostEnv = hostEnv;
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _userManager = userManager;

            //var dr = new DateRange(new DateTime(2020, 1, 1), new DateTime(2022, 12, 1));
            //var list = CStat.Models.Task.GetAllTasks(context, dr, null, false);
            //_csSettings = new CSSettings(_configuration, userManager);
        }


        public async Task DoWork(CancellationToken stoppingToken)
        {
            var cl = new CSLogger();

            // Create Filewatches for Camera notications to trigger patrol
            try
            {
                string webRootPath = _hostEnv.WebRootPath;
                string ImgPath1 = Path.Combine(webRootPath, @"Camera1\Images");
                string ImgPath2 = Path.Combine(webRootPath, @"Camera2\Images");
                if (!Directory.Exists(ImgPath1))
                    Directory.CreateDirectory(ImgPath1);
                if (!Directory.Exists(ImgPath2))
                    Directory.CreateDirectory(ImgPath2);
                CSFileWatcher csFW1 = new CSFileWatcher(ImgPath1, "*.jpg", _hostEnv);
                CSFileWatcher csFW2 = new CSFileWatcher(ImgPath2, "*.jpg", _hostEnv);
            }
            catch (Exception e)
            {
                cl.Log("CSProc: DoWork() CSFileWatcher Initialization Exception: " + e.Message);
            }

            cl.Log("CSProc: DoWork() **** STARTED **** Inst=" + Interlocked.Increment(ref CSPInstCnt));

            // SET THE MINIMUM TIME BETWEEN PROCESSING RUNS IN MINUTES
            int MinWaitMins = 120; // 2 hrs

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    cl.Log("CSProc: Top of while Loop");

                    _csSettings = new CSSettings(_configuration, _userManager); // refresh settings every MinWaitMins.

                    DateTime enow = PropMgr.ESTNow;
                    DateTime lastW = (_csSettings.LastTaskUpdate != null) ? _csSettings.LastTaskUpdate : new DateTime(2020, 1, 1);

                    cl.Log("CSProc: DoWork() Now=" + PropMgr.ESTNowStr + " LastTaskUpdate=" + lastW.ToString());

                    // Has it been MinWait (minutes) or more since last run ?
#if DEBUG
                    if (true)
#else
                    if ((enow - lastW).TotalMinutes >= MinWaitMins)
#endif
                    {
                        // Check Stock for possibly needed items

                        cl.Log("CSProc: DoWork() Check Stock");

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
                        cl.Log("CSProc: DoWork() Check for new Tasks");

                        AutoGen ag = new AutoGen(_context);
                        _csSettings.LastTaskUpdate = PropMgr.ESTNow;
                        _csSettings.Save();
                        ag.GenTasks(_hostEnv);

                        // Notify users Tasks Due
                        CTask.NotifyUserTaskDue(_hostEnv, _configuration, _userManager, _context, 24, true); // Potentially clean log

                        // Persist Daily Reading and Notify if needed for Propane
                        PropaneMgr pmgr = new PropaneMgr(_hostEnv, _configuration, _userManager);
                        var pl = pmgr.GetTUTank(false);
                        pmgr.CheckValue(pl); // check

                        // Check for any unexpected propane usage (not potentially impacted from events) from recent daily usage
                        cl.Log("CSProc: DoWork() Check Propane");
                        var plList = pmgr.GetAll(3);
                        var plCnt = plList.Count;
                        if ((pl != null) && (plCnt > 1))
                        {
                            if (pl.ReadingTime > plList[plCnt-1].ReadingTime)
                            {
                                // Append new value to files and to this list of 3
                                pmgr.WritePropane(pl, cl);
                                plList.Add(pl);
                                plList.RemoveAt(0);
                            }

                            for (int i = 0; i < plCnt - 1; ++i)
                            {
                                var plStart = plList[i];
                                var plEnd = plList[i + 1];
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
                        else
                            cl.Log("CSProc: DoWork() Check Propane FAILED: pl=null?" + (pl == null) + " plCnt=" + plCnt);

                        // Check for new EMails to process / store
                        cl.Log("CSProc: DoWork() Check for new EMails");
                        CSEMail.ProcessEMails(_hostEnv, _configuration, _userManager, _csSettings, _context);

                        // Check/Truncate Size of Arduino file
                        cl.Log("CSProc: DoWork() Check/Truncate Arduino file");
                        ArdMgr amgr = new ArdMgr(_hostEnv, _configuration, _userManager);
                        amgr.GetAll(true);

                        // Clean/{Reset to full view} Camera
                        cl.Log("CSProc: DoWork() Clean/Reset Camera");

                        using (var ptz = new PtzCamera(Cam.Camera.Camera1))
                        {
                            ptz.Cleanup(_hostEnv);
                            ptz.EnableEMailAlerts(!Event.IsEventDay(_context, false, -8, 6)); // disable camera email alerts starting 8 hours of the first day of the non-banquet event up to 6 pm on the last day
                        }

                        // TBD ENABLE using (var ptz = new PtzCamera(CamOps.Camera.Camera2))
                        // TBD ENABLE {
                        // TBD ENABLE     ptz.Cleanup(_hostEnv);
                        // TBD ENABLE     ptz.EnableEMailAlerts(!Event.IsEventDay(_context, false, -8, 6)); // disable camera email alerts starting 8 hours of the first day of the non-banquet event up to 6 pm on the last day
                        // TBD ENABLE }


                        cl.Log($"CSProc: DoWork() Done!");
                        _logger.LogInformation($"CStat Daily Updates Completed at {PropMgr.ESTNow}");
                    }
                }
                catch (Exception e)
                {
                    cl.Log("CSProc: Exception : " + e.Message);
                }

                int MSecsToStart = MinWaitMins * 60000; // sleep for MinWaitMins then do work again

                cl.Log($"CSProc: DoWork() Waiting " + MSecsToStart + " msecs.");
                await(System.Threading.Tasks.Task.Delay(MSecsToStart, stoppingToken));
            }
            cl.Log("CSProc: DoWork() **** TERMINATED ****");
            Interlocked.Decrement(ref CSPInstCnt);
        }
    }
}
