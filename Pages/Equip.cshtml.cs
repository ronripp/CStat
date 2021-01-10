using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using CTask = CStat.Models.Task;
using Task = System.Threading.Tasks.Task;
using CStat.Common;
using Microsoft.AspNetCore.Hosting;

namespace CStat.Pages
{
    public class EquipModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private IWebHostEnvironment _hostEnv;
        private readonly ArdMgr _ardMgr;
        private ArdRecord _ar;
        private PropaneLevel _pl;

        public EquipModel(CStat.Models.CStatContext context, IWebHostEnvironment hostEnv)
        {
            _context = context;
            _hostEnv = hostEnv;
            _ardMgr = new ArdMgr(_hostEnv);
            _ar = _ardMgr.GetLast();
            if (_ar == null)
                _ar = new ArdRecord(-40, -40, -40, 0, PropMgr.ESTNow);
            _pl = PropaneMgr.GetTUTank();
            if (_pl == null)
                _pl = new PropaneLevel(0, PropMgr.ESTNow, -40);
        }

        public IList<CTask> Task { get;set; }

        public async Task OnGetAsync()
        {
            Task = await _context.Task
                .Include(t => t.Blocking1)
                .Include(t => t.Blocking2)
                .Include(t => t.Church)
                .Include(t => t.ParentTask)
                .Include(t => t.Person)
                .Include(t => t.Worker1)
                .Include(t => t.Worker2)
                .Include(t => t.Worker3).ToListAsync();
        }

        public ArdRecord GetLastArd()
        {
            _ar = _ardMgr.GetLast();
            if (_ar == null)
                _ar = new ArdRecord(-40, -40, -40, 0, PropMgr.ESTNow);
            return _ar;
        }

        public PropaneLevel GetLastTUTank()
        {
            return _pl;
        }
        public string GetColorClass(string propStr)
        {
            return _ar.GetColor(propStr, true, _pl);
        }

    }
}
