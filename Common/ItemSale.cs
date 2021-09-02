using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class ItemSale
    {
        public static Dictionary<string, string> VendorDict = new Dictionary<string, string>
        {
            {"bjs", "BJ's" },
            {"restaurantdepot", "Restaurant Depot"},
            {"therdstore", "Restaurant Depot"},
            {"monogramcleanforce", "US Foods"},
            {"monogram", "US Foods"},
            {"homedepot", "Home Depot"}
        };

        public static string ResolveVendor (string host)
        {
            var hst = host.Trim().ToLower();
            if (VendorDict.TryGetValue(hst, out string vendorName))
            {
                return vendorName;
            }
            return (hst.Length >= 2) ? char.ToUpper(hst[0]) + hst.Substring(1) : hst.ToUpper();
        }
    }
}
