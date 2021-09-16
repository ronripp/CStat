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
            {"filtersfast", "FiltersFast.com"},
            {"costco", "Costco"}
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

                /***************** OLD **************************/
                //HttpClient client = new HttpClient(handler);
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                //string _ContentType = "application/json";
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));
                //var _CredentialBase64 = "RWRnYXJTY2huaXR0ZW5maXR0aWNoOlJvY2taeno=";
                //client.DefaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", _CredentialBase64));
                ////var _UserAgent = "CStat HttpClient";
                //var _UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                //client.DefaultRequestHeaders.Add("User-Agent", _UserAgent);
                ////client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                ////client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                //return await client.GetStringAsync(url);

                /***************** NEW **************************/
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                
                //string _ContentType = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
                string _ContentType = "application/json";
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));

                //var _CredentialBase64 = "RWRnYXJTY2huaXR0ZW5maXR0aWNoOlJvY2taeno=";
                //client.DefaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", _CredentialBase64));

                //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36
                //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9
                //Sec-Fetch-Site: none
                //Sec-Fetch-Mode: navigate
                //Sec-Fetch-User: ?1
                //Sec-Fetch-Dest: document
                //Accept-Encoding: gzip, deflate, br
                //Accept-Language: en-US,en;q=0.9

                //var _UserAgent = "CStat HttpClient";
                //var _UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                var _UserAgent = "Mozilla/5.0 (x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                client.DefaultRequestHeaders.Add("User-Agent", _UserAgent);

                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                return await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
            }
            return "";
        }
    }
}
