using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Data.Helpers
{
    public class WebClientExt : WebClient
    {
        private enum Method {Get, Post}

        #region =========  IP helpers ============
        private class cIpCountry
        {
            public string ip;
            public string country;
        }
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
        #endregion

        #region ======  BatchDownload  ========
        public interface IDownloadToFileItem: IBaseDownloadItem
        {
            public string Filename { get; }
        }

        public interface IDownloadToMemoryItem: IBaseDownloadItem
        {
            public byte[] Data { get; set; }
        }

        public interface IBaseDownloadItem
        {
            public string Url { get; }
            public HttpStatusCode? StatusCode { get; set; }
        }

        public static async Task DownloadItemsToMemory(ICollection<IDownloadToMemoryItem> downloadItems, int parallelBatchSize)
        {
            var tasks = new List<(IBaseDownloadItem, Task<byte[]>)>();
            foreach (var downloadItem in downloadItems)
            {
                if (downloadItem.Data != null && downloadItem.StatusCode != HttpStatusCode.OK)
                    downloadItem.StatusCode = HttpStatusCode.OK;

                if (!(downloadItem.StatusCode == HttpStatusCode.NotFound || downloadItem.StatusCode == HttpStatusCode.OK))
                {
                    var task = WebClientExt.DownloadToBytesAsync(downloadItem.Url);
                    tasks.Add((downloadItem, task));
                    downloadItem.StatusCode = null;
                }

                if (tasks.Count >= parallelBatchSize)
                {
                    await Download(tasks);
                    Helpers.Logger.AddMessage($"Downloaded {downloadItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {downloadItems.Count:N0}");
                    tasks.Clear();
                }
            }

            if (tasks.Count > 0)
            {
                await Download(tasks);
                tasks.Clear();
            }
            Helpers.Logger.AddMessage($"Downloaded {downloadItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {downloadItems.Count:N0}");
        }

        public static async Task DownloadItemsToFiles(ICollection<IDownloadToFileItem> downloadItems, int parallelBatchSize)
        {
            // var tasks = new ConcurrentDictionary<IDownloadItem, Task<byte[]>>();
            var tasks = new List<(IBaseDownloadItem, Task<byte[]>)>();
            foreach (var downloadItem in downloadItems)
            {
                if (File.Exists(downloadItem.Filename) && downloadItem.StatusCode != HttpStatusCode.OK)
                    downloadItem.StatusCode = HttpStatusCode.OK;

                if (!(downloadItem.StatusCode == HttpStatusCode.NotFound || downloadItem.StatusCode == HttpStatusCode.OK))
                {
                    var task = WebClientExt.DownloadToBytesAsync(downloadItem.Url);
                    tasks.Add((downloadItem, task));
                    downloadItem.StatusCode = null;
                }

                if (tasks.Count >= parallelBatchSize)
                {
                    await Download(tasks);
                    Helpers.Logger.AddMessage($"Downloaded {downloadItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {downloadItems.Count:N0}");
                    tasks.Clear();
                }
            }

            if (tasks.Count > 0)
            {
                await Download(tasks);
                tasks.Clear();
            }
            Helpers.Logger.AddMessage($"Downloaded {downloadItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {downloadItems.Count:N0}");
        }

        private static async Task Download(List<(IBaseDownloadItem, Task<byte[]>)> tasks)
        {
            foreach (var item in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    var data = await item.Item2;
                    item.Item1.StatusCode= HttpStatusCode.OK;

                    if (item.Item1 is IDownloadToMemoryItem memoryItem)
                        memoryItem.Data = data;
                    else if (item.Item1 is IDownloadToFileItem fileItem)
                        await File.WriteAllBytesAsync(fileItem.Filename, data);
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.NotFound ||
                         response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway ||
                         response.StatusCode == HttpStatusCode.ServiceUnavailable))
                    {
                        item.Item1.StatusCode = response.StatusCode;
                        if (response.StatusCode != HttpStatusCode.NotFound)
                            Debug.Print($"Status code: {response.StatusCode} for {item.Item1.Url}");
                        continue;
                    }

                    throw new Exception($"WebClientExt.Download: Error while download from {item.Item1.Url}. Error message: {ex.Message}");
                }
            }

        }
        #endregion

        #region ====  Static region  ====
        public static Task<byte[]> DownloadToBytesAsync(string url, bool isXmlHttpRequest = false, bool noProxy = false, CookieCollection cookies = null)
        {
            using (var wc = new WebClientExt())
            {
                if (noProxy) wc.Proxy = null;
                wc.Encoding = System.Text.Encoding.UTF8;
                wc._isXmlHttpRequest = isXmlHttpRequest;
                wc.Headers.Add(HttpRequestHeader.Referer, new Uri(url).Host);
                if (cookies != null)
                {
                    wc._cookies = new CookieContainer();
                    wc._cookies.Add(cookies);
                }
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
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-US,en;q=0.9,uk;q=0.8,ru;q=0.7"); // for https://api.nasdaq.com/api/quote/INDU/historical?assetclass=index&fromdate=2024-08-30&limit=9999&todate=2024-09-07
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Accept = "text/html,*/*"; // text/html,*/* - for auth in finance.yahoo

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
