using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace CStat.Common
{
    public class HttpReq
    {
        public static CSLogger gLog = null;

        public HttpReq ()
        {
            if (gLog == null)
                gLog = new CSLogger();
        }

        private HttpWebRequest _request = null;

        public void Open(string method, string url)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback +=
            delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                                    System.Security.Cryptography.X509Certificates.X509Chain chain,
                                    System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true; // **** Always accept
            };

            //ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            _request = (HttpWebRequest)WebRequest.Create(url); // Create a request using a URL that can receive a post.

            _request.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            //_request.ServerCertificateValidationCallback += delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
            //            System.Security.Cryptography.X509Certificates.X509Chain chain,
            //            System.Net.Security.SslPolicyErrors sslPolicyErrors)
            //{
            //    return true; // **** Always accept
            //};

            _request.AutomaticDecompression = DecompressionMethods.All;
            _request.Method = method; // Set the Method property of the request.
        }

        public void AddHeaderProp(string prop)
        {
            _request.Headers.Add(prop);
        }

        public bool AddBody(string postData, string contentType = "application/x-www-form-urlencoded")
        {
            try
            {
                // Get the request stream.
                byte[] byteArray = Encoding.UTF8.GetBytes(postData); // Create POST data and convert it to a byte array.
                _request.ContentType = contentType; // Set the ContentType property of the WebRequest.
                _request.ContentLength = byteArray.Length; // Set the ContentLength property of the WebRequest.

                Stream dataStream = _request.GetRequestStream(); // Get the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length); // Write the data to the request stream.
                dataStream.Close(); // Close the Stream object.
                return true;
            }
            catch( Exception e)
            {
                gLog.Log("HttpReq:AddBody e=" + e.Message);
                return false;
            }

        }
        public HttpWebResponse Send(out string responseStr)
        {
            try
            {
                WebResponse webResp = _request.GetResponse();
                HttpWebResponse httpWebResp = (HttpWebResponse)webResp;

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.

                using (Stream dataStream = webResp.GetResponseStream())
                {
                    Stream dataStream2;
                    var cenc = httpWebResp.ContentEncoding;
                    var cencStr = string.IsNullOrEmpty(cenc) ? " " : cenc.ToLower();
                    
                    if (cencStr.Contains("gzip"))
                        dataStream2 = new GZipStream(dataStream, CompressionMode.Decompress);
                    else if (cencStr.Contains("deflate"))
                        dataStream2 = new DeflateStream(dataStream, CompressionMode.Decompress);
                    else
                        dataStream2 = dataStream;
                    StreamReader Reader = new StreamReader(dataStream2, Encoding.Default);
                    responseStr = Reader.ReadToEnd(); // Read the content.
                    dataStream2.Close();
                }
                webResp.Close(); // Close the response.
                return (HttpWebResponse)webResp;
            }
            catch (Exception e)
            {
                gLog.Log("HttpReq:Send e=" + e.Message);
                responseStr = "";
                return new HttpWebResponse();
            }
        }
    }
}
