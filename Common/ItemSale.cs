using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
            {"homedepot", "Home Depot"},
            {"webstaurantstore", "WebstaurantStore"},

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

        public static async Task<string> GetPageContent (string url)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };

                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                string _ContentType = "application/json";
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));
                var _CredentialBase64 = "RWRnYXJTY2huaXR0ZW5maXR0aWNoOlJvY2taeno=";
                client.DefaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", _CredentialBase64));
                var _UserAgent = "CStat HttpClient";
                client.DefaultRequestHeaders.Add("User-Agent", _UserAgent);
                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
            }
            return "";
        }
    }
}
