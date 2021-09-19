using System;
using System.Collections.Generic;
using System.IO;
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

        public static bool IsVendor(string lhost, string vendor)
        {
            return lhost.Contains(" " + vendor) || (lhost == vendor);
        }
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
            var uri = new Uri(url);
            var hostStrs = uri.Host.Split('.'); 
            var lhost = (hostStrs.Length >= 2) ? ItemSale.ResolveVendor(hostStrs[hostStrs.Length-2]).ToLower().Trim() : "";
            string pageStr = "";
            try
            {
                HttpClientHandler handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
                };

                if (lhost == "ZZZ")
                {
                    using (HttpClient client = new HttpClient(handler))
                    {
                        /***************** OLD **************************/
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                        string _ContentType = "application/json";
                        //string _ContentType = "text/html";
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_ContentType));
                        //var _CredentialBase64 = "RWRnYXJTY2huaXR0ZW5maXR0aWNoOlJvY2taeno=";
                        //client.DefaultRequestHeaders.Add("Authorization", String.Format("Basic {0}", _CredentialBase64));
                        //var _UserAgent = "CStat HttpClient";
                        var _UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                        //var _UserAgent = "Chrome/93.0.4577.63 Safari/537.36";
                        //var _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows Phone OS 7.5; Trident/5.0; IEMobile/9.0)";
                        client.DefaultRequestHeaders.Add("User-Agent", _UserAgent);
                        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        pageStr = await client.GetStringAsync(url);
                    }
                }
                else
                {
                    /***************** NEW **************************/
                    using (HttpClient client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                        client.DefaultRequestHeaders.Add("Keep-Alive", "600");
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
                        var _UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                        //var _UserAgent = "Mozilla/5.0 (x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36";
                        client.DefaultRequestHeaders.Add("User-Agent", _UserAgent);

                        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                        client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");

                        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                        return await client.GetStringAsync(url);
                    }

                    /**************** EDGE *************************
                    //_http.DefaultRequestHeaders.Add("Connection", "Keep-Alive");

                    //Connection: keep-alive
                    //sec-ch-ua: "Microsoft Edge";v="93", " Not;A Brand";v="99", "Chromium";v="93"
                    //sec-ch-ua-mobile: ?0
                    //sec-ch-ua-platform: "Windows"
                    //Upgrade-Insecure-Requests: 1
                    //User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.63 Safari/537.36 Edg/93.0.961.47
                    //Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/
                    //* // ;q=0.8,application/signed-exchange;v=b3;q=0.9
                    //Sec-Fetch-Site: none
                    //Sec-Fetch-Mode: navigate
                    //Sec-Fetch-User: ?1
                    //Sec-Fetch-Dest: document
                    //Accept-Encoding: gzip, deflate, br
                    //Accept-Language: en-US,en;q=0.9
                    //**********************************************
                }
            }

            catch (TaskCanceledException ex)
            {
                return "PSLEN=TIMEOUT:" + ex.Message;
            }
            catch (Exception ex)
            {
                return "PSLEN=" + ex.Message;
            }

            if (pageStr.Trim().Length < 150)
            {
                // Try using Web Client
                pageStr = "";
                try
                {
                    WebClient myWebClient = new WebClient(); // Create a new WebClient instance.
                    // Download home page data, open a stream to point to the data stream coming from the Web resource.
                    using (Stream webStream = myWebClient.OpenRead(url))
                    {
                        StreamReader sr = new StreamReader(webStream);
                        pageStr = sr.ReadToEnd();
                        webStream.Close(); // Close the stream (may be done by exiting using also)
                    }
                }
                catch (Exception ex)
                {
                    pageStr = "";
                }
            }
            return pageStr;
        }
    }
}
