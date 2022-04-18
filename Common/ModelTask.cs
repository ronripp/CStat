using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Models
{
    public partial class Task
    {
        public static int RemoveActiveChildTasks(CStat.Models.CStatContext context, int parentTaskId)
        {
            // Delete only those task that are unabiguously not done
            int NumDeleted = 0;
            var oldTasks = context.Task.AsNoTracking().Where(t => t.ParentTaskId.HasValue &&
                                                             (t.ParentTaskId.Value == parentTaskId) && !t.ActualDoneDate.HasValue &&
                                                             ((t.Status & (int)Task.eTaskStatus.Completed) == 0));
            if (oldTasks != null)
            {
                foreach (var t in oldTasks)
                {
                    try
                    {
                        context.Task.Remove(t);
                        context.SaveChangesAsync();
                        ++NumDeleted;
                    }
                    catch
                    {
                        // TBD : log these errors
                    }
                }
            }
            return NumDeleted;
        }

    }
}