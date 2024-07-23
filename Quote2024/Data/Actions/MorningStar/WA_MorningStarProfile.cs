using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
        private const string ListUrlTemplate =
            "https://web.archive.org/cdx/search/cdx?url=https://www.morningstar.com/stocks/{0}/{1}/quote&matchType=prefix&limit=100000&from=2019";

        private const string ListUrlTemplateByExchange =
            "https://web.archive.org/cdx/search/cdx?url=https://www.morningstar.com/stocks/{0}&matchType=prefix&limit=300000";

        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\MorningStar\WA_List";
        private const string ListDataFolderByExchange = @"E:\Quote\WebData\Symbols\MorningStar\WA_ListByExchange";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\MorningStar\WA_Profile";

        private static string[] _exchanges = new[] { "arcx", "bats", "xase", "xnas", "xnys" };

        public static void ParseFiles()
        {
            var badFiles = new Dictionary<string, object>
            {
                { "xnas_tsla_20230404182642.html", null }, { "xnas_tsla_20230404190240.html", null },
                { "xnas_tsla_20230404202431.html", null }
            };

            var zipFileName = HtmlDataFolder + ".Short.zip";
            var cnt = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    if (++cnt % 100 == 0)
                        Logger.AddMessage(
                            $"Parse data from {Path.GetFileName(zipFileName)}. Parsed {cnt:N0} from {zip.Entries.Count:N0} items.");

                    if (badFiles.ContainsKey(entry.Name.ToLower()))
                        continue;

                    var content1 = entry.GetContentOfZipEntry().Replace("\n", "").Replace("\t", "")
                        .Replace(" <!---->", "");

                    // Remove footer
                    var i21 = content1.IndexOf(">Related</h2>", StringComparison.InvariantCulture);
                    if (i21 == -1)
                        i21 = content1.IndexOf(">Stocks by Morningstar Classification</",
                            StringComparison.InvariantCulture);
                    if (i21 == -1) i21 = content1.IndexOf(">Sponsor Center</", StringComparison.InvariantCulture);
                    string content;
                    if (i21 == -1)
                    {
                        content = content1;
                        Debug.Print($"No footer:\t{entry.Name}");
                    }
                    else
                    {
                        content = content1.Substring(0, i21);
                        var i11 = content1.IndexOf(">Sector<", i21, StringComparison.InvariantCultureIgnoreCase);
                        if (i11 != -1)
                            Debug.Print($"There is '>Sector<' in footer:\t{entry.Name}");
                    }

                    var i1 = content.IndexOf(">Company Profile</h2>", StringComparison.InvariantCulture);
                    if (i1 == -1) i1 = content.IndexOf(" Company Profile</h2>", StringComparison.InvariantCulture);
                    if (i1 == -1) i1 = content.IndexOf(">Company Profile<", StringComparison.InvariantCulture);
                    if (i1 == -1) i1 = content.IndexOf(">Company Profile <", StringComparison.InvariantCulture);
                    if (i1 == -1)
                        i1 = content.IndexOf(">Company Profile -" /*plus symbol*/, StringComparison.InvariantCulture);
                    if (i1 == -1)
                    {
                        var i12 = content.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase);
                        if (i12 != -1)
                        {
                            Debug.Print($"No 'Company Profile' but is 'sector':\t{entry.Name}");
                        }

                        var i11 = content1.IndexOf(">Sector<", StringComparison.InvariantCultureIgnoreCase);
                        if (i11 != -1)
                        {
                            Debug.Print($"No 'Company Profile' but is '>Sector<' in footer:\t{entry.Name}");
                        }
                    }
                    else
                    {
                        var i11 = content.IndexOf(">Sector<", i1 + 10, StringComparison.InvariantCultureIgnoreCase);
                        if (i11 == -1)
                        {
                            Debug.Print($"There is 'Company Profile' but no '>Sector<':\t{entry.Name}");
                        }
                    }
                }
        }

        public static async Task DownloadDataByExchange()
        {
            Helpers.Logger.AddMessage("Prepare url list");

            var zipFileName = ListDataFolderByExchange + ".zip";
            var exchangeAndSymbols = GetExchangeAndSymbols(true);
            var toDownload = new List<DownloadItem>();
            /*var existingItems = new Dictionary<string, object>();
            var existingFiles = Directory.GetFiles(HtmlDataFolder, "*.html");
            foreach (var file in existingFiles)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var key = $"{ss[0]}_{ss[1]}_{ss[2].Substring(0, 8)}";
                if (!existingItems.con)
            }*/
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var items = entry.GetLinesOfZipEntry().Select(a => new JsonItem(a))
                        .Where(a => a.Type == "text/html" && a.Status == 200).OrderBy(a => a.TimeStamp).ToArray();
                    foreach (var item in items)
                    {
                        var itemUrl = item.Url;
                        var i1 = itemUrl.IndexOf('?', StringComparison.InvariantCulture);
                        if (i1 > 0)
                            itemUrl = itemUrl.Substring(0, i1);

                        if (itemUrl.EndsWith("/quote"))
                        {
                            var ss = itemUrl.Split('/');
                            var exchange = ss[ss.Length - 3].ToLower();
                            var msSymbol = ss[ss.Length - 2].ToLower();
                            var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                            var shortFileName = $"{exchange}_{msSymbol}_{item.TimeStamp}.html";
                            var filename = Path.Combine(HtmlDataFolder, shortFileName);
                            if (!File.Exists(filename))
                                toDownload.Add(new DownloadItem(url, filename));
                            /*if (!existingItems.ContainsKey(shortFileName))
                            {
                                existingItems.Add(shortFileName, null);
                                var filename = Path.Combine(HtmlDataFolder, shortFileName);
                                if (!File.Exists(filename))
                                    toDownload.Add(new DownloadItem(url, filename));
                            }*/
                        }
                        else if (itemUrl.EndsWith("/quote.html")) { }
                        else if (item.Url.EndsWith("/quoteprint.html")) {}
                        else {}
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

        public static void DownloadListByExchange()
        {
            Logger.AddMessage($"Started");
            foreach (var exchange in _exchanges)
            {
                Logger.AddMessage($"Download url list for {exchange}");
                var filename = Path.Combine($@"{ListDataFolderByExchange}", $"{exchange}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplateByExchange, exchange);
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

        public static async Task xxDownloadData()
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

        public static void xxDownloadList()
        {
            Logger.AddMessage($"Started");
            var exchangeAndSymbols = GetExchangeAndSymbols(false);
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

        private static Dictionary<(string, string), object> GetExchangeAndSymbols(bool allTickers)
        {
            var data = new Dictionary<(string, string), object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                var filter = allTickers ? "" : " and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') ";
                cmd.CommandText = "SELECT DISTINCT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  $"WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null {filter} order by 2";
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
