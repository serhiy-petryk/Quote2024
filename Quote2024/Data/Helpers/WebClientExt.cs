﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data.Helpers
{
    public class WebClientExt : WebClient
    {
        private enum Method {Get, Post}

        private class cIpCountry
        {
            public string ip;
            public string country;
        }
        #region ====  Static region  ====

        public static void CheckVpnConnection()
        {
            var country = WebClientExt.GetMyIpCountry();
            if (string.IsNullOrEmpty(country) || string.Equals(country, "UA", StringComparison.InvariantCultureIgnoreCase))
                MessageBox.Show("Please, check VPN connection");
        }

        private static string GetMyIpCountry()
        {
            var response = WebClientExt.GetToBytes($"https://ipapi.co/json/", false);
            if (response.Item1 != null)
            {
                var oo = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cIpCountry>(response.Item1);
                return oo.country;
            }
            return null;
        }

        public static Task<byte[]> DownloadToBytesAsync(string url, bool isXmlHttpRequest = false, bool noProxy = false)
        {
            using (var wc = new WebClientExt())
            {
                if (noProxy) wc.Proxy = null;
                wc.Encoding = System.Text.Encoding.UTF8;
                wc._isXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                return wc.DownloadDataTaskAsync(url);
            }
        }

        public static (byte[], CookieCollection, Exception) GetToBytes(string url, bool isJson,
            bool isXmlHttpRequest = false, CookieCollection cookies = null) =>
            DoRequest(Method.Get, url, null, isJson, isXmlHttpRequest, null, cookies);

        public static (byte[], CookieCollection, Exception) PostToBytes(string url, string parameters, bool isJson,
            bool isXmlHttpRequest = false, string contentType = null, CookieCollection cookies = null) =>
            DoRequest(Method.Post, url, parameters, isJson, isXmlHttpRequest, contentType, cookies);

        private static (byte[], CookieCollection, Exception) DoRequest(Method method, string url, string postParameters, bool isJson, bool isXmlHttpRequest = false, string contentType = null, CookieCollection cookies = null)
        {
            // see https://stackoverflow.com/questions/5401501/how-to-post-data-to-specific-url-using-webclient-in-c-sharp
            using (var wc = new WebClientExt())
            {
                wc.Encoding = System.Text.Encoding.UTF8;
                wc._isXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                if (method == Method.Post)
                    wc.Headers.Add(HttpRequestHeader.ContentType, contentType ?? "application/x-www-form-urlencoded"); // very important for Investing.Splits
                if (cookies != null)
                {
                    wc._cookies = new CookieContainer();
                    wc._cookies.Add(cookies);
                }
                // Very slowly
                //wc._cookies = new CookieContainer();
                //wc._cookies.Add(cookies ?? new CookieCollection());

                try
                {
                    var response = method == Method.Post
                        ? wc.UploadData(url, "POST", wc.Encoding.GetBytes(postParameters))
                        : wc.DownloadData(url);
                    if (isJson && (response.Length < 2 || response[0] != 0x7b || response[response.Length - 1] != 0x7d))
                        throw new Exception($"Downloaded content is not in JSON format");
                    return (response, wc._responseCookies, null);
                }
                catch (Exception ex)
                {
                    if (ex is WebException)
                    {
                        Debug.Print($"{DateTime.Now}. Web Exception: {url}. Message: {ex.Message}");
                        return (null, wc._responseCookies, ex);
                    }
                    else
                        throw ex;
                }
            }
        }
        #endregion

        #region ====  Instance region  =====

        private int? _timeoutInMilliseconds;
        private CookieContainer _cookies;
        private bool _isXmlHttpRequest;
        private CookieCollection _responseCookies;

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            var response = base.GetWebResponse(request);
            _responseCookies = ((HttpWebResponse)response).Cookies;
            return response;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36";
            request.AllowAutoRedirect = true;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //request.ContentType = "application/json";
            //request.MediaType = "application/json";
            //request.Accept = "application/json";
            //request.Method = "POST";

            if (_isXmlHttpRequest)
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

            if (_cookies != null)
                request.CookieContainer = _cookies;

            if (_timeoutInMilliseconds.HasValue)
                request.Timeout = _timeoutInMilliseconds.Value;
            return request;
        }
        #endregion
    }
}
