using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CStat.Models;
using CStat.Pages.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CStat.Common
{
    public class Worker : IWorker
    {
        private readonly CStatContext context;
        public Worker (CStat.Models.CStatContext context)
        {
            this.context = context;
        }

        public async System.Threading.Tasks.Task DoWork(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                // Check Stock for possibly needed items
                OrderedEvents ordEvs = new OrderedEvents(context);
                IList<InventoryItem> InventoryItems = context.InventoryItem.Include(i => i.Inventory).Include(i => i.Item).ToList();
                foreach (var invIt in InventoryItems)
                {
                    if (IndexInvModel.MayNeedItem(context, invIt, ordEvs))
                    {
                        // Item likely needs to be ordered. Mark as needed.
                        if (invIt.State == IndexInvModel.STOCKED_STATE)
                        {
                            invIt.State = IndexInvModel.NEEDED_STATE;
                            context.Attach(invIt).State = EntityState.Modified;

                            try
                            {
                                context.SaveChanges();
                            }
                            catch (DbUpdateConcurrencyException)
                            {
                                continue;
                            }
                        }
                    }
                }

                // Check for new Tasks
                AutoGen ag = new AutoGen(context);
                ag.GenTasks();

                // Run again at 3:00 AM
                DateTime now = DateTime.Now;
                DateTime tom = now.AddDays(1);
                DateTime start = new DateTime(tom.Year, tom.Month, tom.Day, 3, 0, 0); // Restart at 3:00 AM tomorrow
                int MSecsToStart = (int)Math.Round((start - now).TotalMilliseconds);
                await System.Threading.Tasks.Task.Delay(MSecsToStart);
            }
        }
    }
}
