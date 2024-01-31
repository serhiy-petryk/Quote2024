using System;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Data.Helpers
{
    public static class Download
    {
        public static object DownloadToBytes(string url, bool isXmlHttpRequest = false, CookieContainer cookies = null)
        {
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Cookies = cookies;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                try
                {
                    return wc.DownloadData(url);
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                        return ex;
                    }
                    else
                        throw ex;
                }
            }
        }

        public static object DownloadToString(string url, bool isXmlHttpRequest = false, CookieContainer cookies = null)
        {
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Cookies = cookies;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                try
                {
                    var bb = wc.DownloadData(url);
                    var response = Encoding.UTF8.GetString(bb);
                    return response;
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                        return ex;
                    }
                    else
                        throw ex;
                }
            }
        }

        public static object PostToString(string url, string parameters, bool isXmlHttpRequest = false)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded"); // for post

                try
                {
                    var response = wc.UploadString(url, "POST", parameters);
                    return response;
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                        return ex;
                    }
                    else
                        throw ex;
                }
            }
        }

        public class WebClientEx : WebClient
        {
            public int? TimeoutInMilliseconds;
            public CookieContainer Cookies;
            public bool IsXmlHttpRequest;

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest)base.GetWebRequest(address);
                request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
                request.AllowAutoRedirect = true;
                request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                /*request.ContentType = "application/json";
                request.MediaType = "application/json";
                request.Accept = "application/json";
                request.Method = "POST";*/

                if (IsXmlHttpRequest)
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                if (Cookies != null)
                    request.CookieContainer = Cookies;

                if (TimeoutInMilliseconds.HasValue)
                    request.Timeout = TimeoutInMilliseconds.Value;
                return request;
            }
        }


    }
}
