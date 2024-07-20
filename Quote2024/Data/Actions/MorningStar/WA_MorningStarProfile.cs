using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.MorningStar
{
    public static class WA_MorningStarProfile
    {
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://www.morningstar.com/stocks/{0}/{1}/quote&matchType=prefix&limit=100000&from=2019";
        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\MorningStar\WA_List";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\MorningStar\WA_Profile";

        public static async Task DownloadData()
        {
            Helpers.Logger.AddMessage("Prepare url list");
            var toDownload = new List<DownloadItem>();
            var fileCount = 0;
            var zipFileName = ListDataFolder + ".zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var items = entry.GetLinesOfZipEntry().Select(a => new JsonItem(a))
                        .Where(a => a.Type == "text/html" && a.Status == 200).OrderBy(a => a.TimeStamp).ToArray();
                    var exchange = ss[0];
                    var msSymbol = ss[1];
                    var lastDateKey = "";
                    foreach (var item in items)
                    {
                        var dateKey = item.TimeStamp.Substring(0, 8);
                        if (dateKey == lastDateKey) // skip the same day
                            continue;

                        var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                        var filename = Path.Combine(HtmlDataFolder, $"{exchange}_{msSymbol}_{item.TimeStamp}.html");
                        toDownload.Add(new DownloadItem(url, filename));
                        fileCount++;
                        lastDateKey = dateKey;
                    }
                }

            await WebClientExt.DownloadItemsToFiles(toDownload.Cast<WebClientExt.IDownloadToFileItem>().ToList(), 10);

            var badItems = 0;
            foreach (var item in toDownload.Where(a => a.StatusCode != HttpStatusCode.OK))
            {
                badItems++;
                Debug.Print($"{item.StatusCode}:\t{Path.GetFileNameWithoutExtension(item.Filename)}");
            }

            Helpers.Logger.AddMessage($"Downloaded {toDownload.Count:N0} items. Bad items (see debug window for details): {badItems:N0}");
        }

        public static void DownloadList()
        {
            Logger.AddMessage($"Started");
            var exchangeAndSymbols = GetExchangeAndSymbols();
            // var symbols = new Dictionary<string,string>{{"MSFT", "MSFT80"}};
            var count = 0;
            foreach (var exchangeAndSymbol in exchangeAndSymbols)
            {
                count++;
                Logger.AddMessage($"Process {count} symbols from {exchangeAndSymbols.Count}");
                var filename = Path.Combine($@"{ListDataFolder}", $"{exchangeAndSymbol.Key.Item1}_{exchangeAndSymbol.Key.Item2}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplate, exchangeAndSymbol.Key.Item1, exchangeAndSymbol.Key.Item2);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"WA_MorningStarProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                    File.WriteAllBytes(filename, o.Item1);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        private static Dictionary<(string, string), object> GetExchangeAndSymbols()
        {
            var data = new Dictionary<(string, string), object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') order by 2";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var exchange = ((string)rdr["exchange"]).ToLower();
                        var symbol = (string)rdr["symbol"];
                        var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(symbol).ToLower();
                        if (!string.IsNullOrEmpty(msSymbol))
                        {
                            var key = (exchange, msSymbol);
                            if (!data.ContainsKey(key))
                                data.Add(key, null);
                            var secondSymbol = (MorningStarCommon.GetSecondMorningStarProfileTicker(symbol) ?? "").ToLower();
                            if (!string.IsNullOrEmpty(secondSymbol))
                            {
                                var secondKey = (exchange, secondSymbol);
                                if (!data.ContainsKey(secondKey))
                                    data.Add(secondKey, null);
                            }
                        }
                    }
            }

            return data;
        }

        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string PolygonSymbol;
            public string YahooSymbol;
            public string Name;
            public string Sector;
            public DateTime Date;
            public DateTime? To;
            public DateTime LastUpdated;
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

        public class DownloadItem : WebClientExt.IDownloadToFileItem
        {
            public string Url { get; }
            public string Filename { get; set; }
            public HttpStatusCode? StatusCode { get; set; }

            public DownloadItem(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }
        }
        #endregion


    }
}
