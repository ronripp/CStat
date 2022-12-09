using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CStat.Common
{
    public class HttpReq2
    {
        // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
        protected internal static HttpClientHandler _httpClientHandler;
        protected internal static HttpClient _client;

        private class HttpReq2Helper
        {
            HttpReq2Helper()
            {
                _httpClientHandler = new HttpClientHandler();
                _httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                if (_httpClientHandler.SupportsAutomaticDecompression)
                    _httpClientHandler.AutomaticDecompression = DecompressionMethods.All; 

                _client = new HttpClient(_httpClientHandler);
            }
        }
        private static HttpReq2Helper _HttpReq2Helper;
        protected internal static CSLogger _gLog = new CSLogger();;

        private HttpRequestMessage _httpReq = null;

        public HttpReq2()
        {
        }

        public void Open(HttpMethod method, string url)
        {
            _httpReq = new HttpRequestMessage(method, url);
        }

        public void AddHeaderProp(string propName, string propVal)
        {
            _httpReq.Headers.Add(propName, propVal);
        }

        public bool AddBody(string postData, string contentType = "application/x-www-form-urlencoded")
        {
            _httpReq.Content = new StringContent(postData,
                                                Encoding.UTF8,
                                                contentType);//CONTENT-TYPE header
            return _httpReq.Content != null;
        }

        public Stream SendForStream()
        {
            var httpResp = _client.SendAsync(_httpReq).Result;

            try
            {
                // In this case we'll expect our caller to handle a HttpRequestException
                // if this request was not successful.
                httpResp.EnsureSuccessStatusCode();

                if (httpResp.Content is object)
                {
                    return httpResp.Content.ReadAsStreamAsync().Result;
                }
            }
            catch (Exception e)
            {
                _gLog.Log("HttpReq:Send e=" + e.Message);

            }
            return new MemoryStream();
        }

        public String SendForString()
        {
            var httpResp = _client.SendAsync(_httpReq).Result;

            try
            {
                // In this case we'll expect our caller to handle a HttpRequestException
                // if this request was not successful.
                httpResp.EnsureSuccessStatusCode();

                if (httpResp.Content is object)
                {
                    return httpResp.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception e)
            {
                _gLog.Log("HttpReq:Send e=" + e.Message);

            }
            return "";
        }
    }
}

