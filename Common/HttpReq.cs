﻿using System;
using System.IO;
using System.Net;
using System.Text;

namespace CStat.Common
{
    public class HttpReq
    {
        public void Open(string method, string url)
        {
            _request = (HttpWebRequest)WebRequest.Create(url); // Create a request using a URL that can receive a post.
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
            catch
            {
                return false;
            }

        }
        public HttpWebResponse Send(out string responseStr)
        {
            try
            {
                WebResponse webResp = _request.GetResponse();

                // Get the stream containing content returned by the server.
                // The using block ensures the stream is automatically closed.

                using (Stream dataStream = webResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(dataStream); // Open the stream using a StreamReader for easy access.
                    responseStr = reader.ReadToEnd(); // Read the content.
                }
                webResp.Close(); // Close the response.
                return (HttpWebResponse)webResp;
            }
            catch (Exception e)
            {
                _ = e;
                responseStr = "";
                return new HttpWebResponse();
            }
        }

        HttpWebRequest _request = null;
    }
}