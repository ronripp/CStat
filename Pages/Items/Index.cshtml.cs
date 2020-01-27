using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;
using Task = System.Threading.Tasks.Task;

namespace CStat
{
    public class IndexItemsModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexItemsModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<Item> Item { get;set; }

        public async Task OnGetAsync()
        {
            Item = await _context.Item
                .Include(i => i.Mfg).ToListAsync();
        }
    }
}
