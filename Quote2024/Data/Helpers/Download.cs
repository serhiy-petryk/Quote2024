using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Data.Helpers
{
    public static class Download
    {
        public static Task<byte[]> DownloadToBytesAsync(string url, bool isXmlHttpRequest = false, bool noProxy = false)
        {
            using (var wc = new WebClientEx())
            {
                if (noProxy) wc.Proxy = null;
                wc.Encoding = System.Text.Encoding.UTF8;
                // wc.Cookies = cookies;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                return wc.DownloadDataTaskAsync(url);
            }
        }

        public static (byte[], Exception) DownloadToBytes(string url, bool isJson, bool isXmlHttpRequest = false, CookieContainer cookies = null)
        {
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.Cookies = cookies;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                try
                {
                    var response = wc.DownloadData(url);
                    if (isJson && !IsJsonFormat(response))
                        throw new Exception($"Downloaded content is not in JSON format");
                    return (response, null);
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                        return (null, ex);
                    }
                    else
                        throw ex;
                }
            }
        }

        public static object PostToBytes(string url, string parameters, bool isJson, bool isXmlHttpRequest = false, string contentType = null)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            using (var wc = new WebClientEx())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc.IsXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                wc.Headers.Add(HttpRequestHeader.ContentType, contentType ?? "application/x-www-form-urlencoded"); // very important for Investing.Splits

                try
                {
                    var response = wc.UploadData(url, "POST", wc.Encoding.GetBytes(parameters));
                    if (isJson && (response.Length < 2 || response[0] != 0x7b || response[response.Length - 1] != 0x7d))
                        throw new Exception($"Downloaded content is not in JSON format");
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

        private static bool IsJsonFormat(byte[] response) => response.Length > 1 &&
            ((response[0] == '{' && response[response.Length - 1] == '}') ||
             (response[0] == '[' && response[response.Length - 1] == ']'));

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
