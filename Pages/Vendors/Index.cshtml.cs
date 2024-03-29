﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Vendors
{
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<Business> Business { get;set; }

        public async System.Threading.Tasks.Task OnGetAsync()
        {
            Business = await _context.Business
                .Include(b => b.Address)
                .Include(b => b.Poc).ToListAsync();
        }
    }
}
