using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CStat.Models;

namespace CStat.Pages.Churches
{
    public class IndexModel : PageModel
    {
        private readonly CStat.Models.CStatContext _context;
        private int _filter=1;

        public IndexModel(CStat.Models.CStatContext context)
        {
            _context = context;
        }

        public IList<Church> Church { get;set; }

        public async System.Threading.Tasks.Task OnGetAsync(int filter = 1)
        {
            _filter = filter;
            switch (filter)
            {
                case 1: // All
                    Church = await _context.Church
                        .Include(c => c.Address)
                        .Include(c => c.Alternate1)
                        .Include(c => c.Alternate2)
                        .Include(c => c.Alternate3)
                        .Include(c => c.Elder1)
                        .Include(c => c.Elder2)
                        .Include(c => c.Elder3)
                        .Include(c => c.Elder4)
                        .Include(c => c.Elder5)
                        .Include(c => c.SeniorMinister)
                        .Include(c => c.Trustee1)
                        .Include(c => c.Trustee2)
                        .Include(c => c.Trustee3)
                        .Include(c => c.YouthMinister).ToListAsync();
                    break;

                case 2: //Member Good Standing
                    Church = await _context.Church.Where(c => c.MembershipStatus == 1)
                    .Include(c => c.Address)
                    .Include(c => c.Alternate1)
                    .Include(c => c.Alternate2)
                    .Include(c => c.Alternate3)
                    .Include(c => c.Elder1)
                    .Include(c => c.Elder2)
                    .Include(c => c.Elder3)
                    .Include(c => c.Elder4)
                    .Include(c => c.Elder5)
                    .Include(c => c.SeniorMinister)
                    .Include(c => c.Trustee1)
                    .Include(c => c.Trustee2)
                    .Include(c => c.Trustee3)
                    .Include(c => c.YouthMinister).ToListAsync();
                    break;


                case 3: //Members
                    Church = await _context.Church.Where(c => (c.MembershipStatus == 1) || (c.MembershipStatus == 2))
                    .Include(c => c.Address)
                    .Include(c => c.Alternate1)
                    .Include(c => c.Alternate2)
                    .Include(c => c.Alternate3)
                    .Include(c => c.Elder1)
                    .Include(c => c.Elder2)
                    .Include(c => c.Elder3)
                    .Include(c => c.Elder4)
                    .Include(c => c.Elder5)
                    .Include(c => c.SeniorMinister)
                    .Include(c => c.Trustee1)
                    .Include(c => c.Trustee2)
                    .Include(c => c.Trustee3)
                    .Include(c => c.YouthMinister).ToListAsync();
                    break;

                case 4: //IC_COC
                    Church = await _context.Church.Where(c => (c.Affiliation == "ICCOC"))
                    .Include(c => c.Address)
                    .Include(c => c.Alternate1)
                    .Include(c => c.Alternate2)
                    .Include(c => c.Alternate3)
                    .Include(c => c.Elder1)
                    .Include(c => c.Elder2)
                    .Include(c => c.Elder3)
                    .Include(c => c.Elder4)
                    .Include(c => c.Elder5)
                    .Include(c => c.SeniorMinister)
                    .Include(c => c.Trustee1)
                    .Include(c => c.Trustee2)
                    .Include(c => c.Trustee3)
                    .Include(c => c.YouthMinister).ToListAsync();
                    break;

                case 5: // Attends Metro
                    Church = await _context.Church.Where(c => c.StatusDetails.Contains("@Metro") || c.StatusDetails.Contains("@metro") || c.StatusDetails.Contains("@METRO"))
                    .Include(c => c.Address)
                    .Include(c => c.Alternate1)
                    .Include(c => c.Alternate2)
                    .Include(c => c.Alternate3)
                    .Include(c => c.Elder1)
                    .Include(c => c.Elder2)
                    .Include(c => c.Elder3)
                    .Include(c => c.Elder4)
                    .Include(c => c.Elder5)
                    .Include(c => c.SeniorMinister)
                    .Include(c => c.Trustee1)
                    .Include(c => c.Trustee2)
                    .Include(c => c.Trustee3)
                    .Include(c => c.YouthMinister).ToListAsync();
                    break;
            }
        }

        public string GetBkColor (int filter)
        {
            return (filter==_filter) ? "#7CFC00" : "#D3D3D3";
        }

        public static string GetPOCMinister(Church church)
        {
            return Person.GetPOCMinister(church);
        }
        public static string GetPhone(Church church)
        {
            if ((church.Address != null) && (!string.IsNullOrEmpty(church.Address.Phone)))
                return "<a href=\"tel:" + church.Address.Phone + "\">" + Person.FixPhone(church.Address.Phone) + "</a>";
            return "---";
        }
        public static string GetMembershipStatus(Church church)
        {
            if ((church.MembershipStatus < 0) || (church.MembershipStatus >= CStat.Models.Church.ChurchLists.MemberStatList.Length))
                return "---";

            return CStat.Models.Church.ChurchLists.MemberStatList[church.MembershipStatus].name;
        }
        public static string GetAffiliation(Church church)
        {
            return string.IsNullOrEmpty(church.Affiliation) ? "" : church.Affiliation;
        }
    }
}
