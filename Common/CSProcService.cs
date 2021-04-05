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
                ag.GenTasks();

                // Notify users Tasks Due
                CTask.NotifyUserTaskDue(_hostEnv, _configuration, _userManager, _context, 24, true);

                // Persist Daily Reading and Notify if needed for Propane
                PropaneMgr pmgr = new PropaneMgr(_hostEnv, _configuration, _userManager);
                pmgr.CheckValue(pmgr.GetTUTank(true)); // get value, log to file and check

                // Check/Truncate Size of Arduino file
                ArdMgr amgr = new ArdMgr(_hostEnv, _configuration, _userManager);
                amgr.GetAll(true);

                _logger.LogInformation($"CStat Daily Updates Completed at {PropMgr.ESTNow}");

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
