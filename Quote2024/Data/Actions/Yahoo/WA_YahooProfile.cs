using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class WA_YahooProfile
    {
        // private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}&matchType=prefix&limit=100000&from=2024";
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}/profile&matchType=prefix&limit=100000&from=2020";
        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data\List";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data";

        public static void ParseHtml()
        {
            var zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry();
                    var timeStamp = entry.LastWriteTime.DateTime;
//                    var items = new List<SplitModel>();
                }
        }

        public static async Task DownloadData()
        {
            // Get url and file list
            var listFiles = Directory.GetFiles(ListDataFolder, "*.txt");
            var toDownload = new List<DownloadItem>();
            var fileCount = 0;
            foreach (var listFile in listFiles)
            {
                var items = File.ReadAllLines(listFile).Select(a => new JsonItem(a))
                    .Where(a => a.Type == "text/html" && a.Status == 200).OrderBy(a => a.TimeStamp).ToArray();
                var polygonSymbol = Path.GetFileNameWithoutExtension(listFile);
                var lastDateKey = "";
                foreach (var item in items)
                {
                    var dateKey = item.TimeStamp.Substring(0, 8);
                    if (dateKey == lastDateKey) // skeep the same day
                        continue;

                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var filename = Path.Combine(HtmlDataFolder, $"{polygonSymbol}_{item.TimeStamp}.html");
                    toDownload.Add(new DownloadItem(url, filename));
                    fileCount++;
                    lastDateKey = dateKey;
                }
            }

            // Download data
            var tasks = new ConcurrentDictionary<DownloadItem, Task<byte[]>>();
            var itemCount = 0;
            var needToDownload = true;
            while (needToDownload)
            {
                needToDownload = false;
                foreach (var urlAndFilename in toDownload)
                {
                    itemCount++;

                    if (!File.Exists(urlAndFilename.Filename))
                    {
                        var task = WebClientExt.DownloadToBytesAsync(urlAndFilename.Url);
                        tasks[urlAndFilename] = task;
                        urlAndFilename.StatusCode = null;
                    }
                    else
                        urlAndFilename.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 10)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
                        tasks.Clear();
                        needToDownload = true;
                    }
                }

                if (tasks.Count > 0)
                {
                    await Download(tasks);
                    tasks.Clear();
                    needToDownload = true;
                }

                Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
            }

            Helpers.Logger.AddMessage($"Downloaded {toDownload.Count:N0} items");
        }

        private static async Task Download(ConcurrentDictionary<DownloadItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    var data = await kvp.Value;
                    var htmlContent = HtmlHelper.RemoveUselessTags(System.Text.Encoding.UTF8.GetString(data));
                    File.WriteAllText(kvp.Key.Filename, htmlContent);
                    kvp.Key.StatusCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway))
                    {
                        kvp.Key.StatusCode = response.StatusCode;
                        continue;
                    }

                    throw new Exception($"YahooSectorLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }
        }

        public static void DownloadList()
        {
            Logger.AddMessage($"Started");
            var symbols = GetSymbolXref();
            // var symbols = new Dictionary<string,string>{{"MSFT", "MSFT80"}};
            var count = 0;
            foreach (var symbol in symbols)
            {
                count++;
                    Logger.AddMessage($"Process {count} symbols from {symbols.Count}");
                var filename = Path.Combine($@"{ListDataFolder}", $"{symbol.Value}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplate, symbol.Key);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"WA_YahooProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                    File.WriteAllBytes(filename, o.Item1);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        private static Dictionary<string, string> GetSymbolXref()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null order by 1";
                //                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string Symbol;
            public string sDate;
            public string sLastUpdated;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime Date => DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
            public DateTime LastUpdated => DateTime.ParseExact(sLastUpdated, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
        }

        private class JsonItem
        {
            public string Url;
            public string TimeStamp;
            public string Type;
            public int Status;

            public JsonItem(string line)
            {
                var ss = line.Split(' ');
                TimeStamp = ss[1];
                Url = ss[2];
                Type = ss[3];
                if (ss[4] != "-")
                    Status = int.Parse(ss[4]);
            }
        }

        public class DownloadItem
        {
            public string Url;
            public string Filename;
            public HttpStatusCode? StatusCode;

            public DownloadItem(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }
        }
        #endregion


    }
}
