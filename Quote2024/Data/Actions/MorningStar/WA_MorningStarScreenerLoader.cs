using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public static class WA_MorningStarScreenerLoader
    {
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=morningstar.com/{0}&matchType=prefix&limit=100000";
        private const string ListDataFolder = Settings.DataFolder + @"Screener\MorningStar\WA_List.2024-07";
        private const string HtmlDataFolder = Settings.DataFolder + @"Screener\MorningStar\WA_Data";
        private const string JsonDataFolder2024 = Settings.DataFolder + @"Screener\MorningStar\Data";

        public static void ParseJson2024AllFiles(List<WA_MorningStarSector.DbItem> data)
        {
            var files = Directory.GetFiles(JsonDataFolder2024, "*.zip").OrderBy(a => a).ToArray();
            foreach (var file in files)
                ParseJson2024Files(file, data);
        }

        private static void ParseJson2024Files(string zipFileName, List<WA_MorningStarSector.DbItem> data)
        {
            var cnt = 0;
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            // var data = new Dictionary<string, Dictionary<string, WA_MorningStarSector.DbItem>>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    if (++cnt % 100 == 0)
                        Logger.AddMessage(
                            $"Parse data from {Path.GetFileName(zipFileName)}. Parsed {cnt:N0} from {zip.Entries.Count:N0} items.");

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var sector = MorningStarCommon.GetSectorName(ss[ss.Length - 2]);

                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in oo.results)
                    {
                        var exchange = item.meta.exchange;
                        var originalSymbol = item.meta.ticker;
                        var name = item.fields.name.value;

                        var symbol = originalSymbol;
                        if (string.IsNullOrWhiteSpace(symbol) && name == "Nano Labs Ltd Ordinary Shares - Class A")
                            symbol = "NA";

                        if (!MorningStarCommon.Exchanges.Any(exchange.Contains)) continue;
                        if (symbol == "SANP1") continue;

                        if (string.IsNullOrWhiteSpace(symbol))
                            throw new Exception($"No ticker code. Please, check. Security name is {name}. File: {entry.Name}");
                        if (string.IsNullOrWhiteSpace(name))
                            throw new Exception($"Name of '{originalSymbol}' ticker is blank. Please, adjust structure (Bfr_)ScreenerMorningStar tables in database. File: {entry.Name}");

                        var dbItem = new WA_MorningStarSector.DbItem(symbol, name, sector, entry.LastWriteTime.DateTime);
                        data.Add(dbItem);
                    }
                }

            var badTickers = MorningStarCommon.BadTickers;
            if (badTickers.Count > 0)
                throw new Exception($"There are {badTickers.Count} bad morning star tickers. Please, check MorningStarMethod.GetMyTicker");
        }

        public static void ParseHtmlFiles(List<WA_MorningStarSector.DbItem> data)
        {
            var zipFileName = HtmlDataFolder + ".Short.zip";
            var cnt = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    if (++cnt % 100 == 0)
                        Logger.AddMessage(
                            $"Parse data from {Path.GetFileName(zipFileName)}. Parsed {cnt:N0} from {zip.Entries.Count:N0} items.");

                    var content = entry.GetContentOfZipEntry();

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var timeKey = ss[1];
                    var timestamp = DateTime.ParseExact(timeKey, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                    var sector = MorningStarCommon.GetSectorName(ss[0]);

                    var rows = 0;
                    var ss1 = content.Split("<a href=\"/web/" + timeKey + "/https://www.morningstar.com/stocks/");
                    for (var k1 = 1; k1 < ss1.Length; k1++)
                    {
                        var s = ss1[k1];
                        var k2 = s.IndexOf('/');
                        if (k2 > 10) continue;

                        var exchange = s.Substring(0, k2).ToUpper();
                        if (!MorningStarCommon.Exchanges.Any(exchange.Contains)) continue;

                        var k3 = s.IndexOf('/', k2 + 1);
                        var symbol = s.Substring(k2 + 1, k3 - k2 - 1).ToUpper();

                        var k4 = s.IndexOf('>');
                        var k5 = s.IndexOf("</a>", StringComparison.Ordinal);
                        var name = s.Substring(k4 + 1, k5 - k4 - 1).Trim();
                        if (name.EndsWith("</span>"))
                        {
                            var k6 = name.Substring(0, name.Length - 1).LastIndexOf('>');
                            name = name.Substring(k6 + 1, name.Length - k6 - 8).Trim();
                        }

                        if (symbol.Contains("-P")) symbol = symbol.Replace("-P", "p");
                        else if (symbol.EndsWith("-A") || symbol.EndsWith("-B") || symbol.EndsWith("-C"))
                            symbol = symbol.Replace('-', '.');
                        else if (symbol.EndsWith("-W"))
                            symbol = symbol.Replace("-W", "w");

                        if (string.Equals(symbol, name))
                        {
                            // link in ticker column
                        }
                        else
                        {
                            // link in name column
                            name = System.Net.WebUtility.HtmlDecode(name);
                            if (ss1[0].Contains("Gainers, Losers, Actives"))
                            {
                                // Invalid html area
                            }
                            else
                            {
                                rows++;
                                var dbItem = new WA_MorningStarSector.DbItem(symbol, name, sector, timestamp);
                                data.Add(dbItem);
                            }
                        }
                    }

                    if (rows == 0 && content.Contains("Market Return", StringComparison.InvariantCulture))
                        throw new Exception("Check");
                    else if (rows != 0 && !content.Contains("Market Return", StringComparison.InvariantCulture))
                        throw new Exception("Check");
                }
        }

        public static async Task DownloadData()
        {
            var toDownload = new List<DownloadItem>();
            var listFiles = Directory.GetFiles(ListDataFolder, "*.txt");
            foreach (var listFile in listFiles)
            {
                var items = File.ReadAllLines(listFile).Select(a => new JsonItem(a)).Where(a => a.Type == "text/html" && a.Status == 200).ToArray();
                var sectorKey = Path.GetFileNameWithoutExtension(listFile);
                foreach (var item in items)
                {
                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var filename = Path.Combine(HtmlDataFolder, $"{sectorKey}_{item.TimeStamp}.html");
                    if (!File.Exists(filename))
                        toDownload.Add(new DownloadItem(url, filename));
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
            foreach (var sector in MorningStarCommon.Sectors)
            {
                Logger.AddMessage($"Process {sector} sector");
                var url = string.Format(ListUrlTemplate, sector);
                var filename = Path.Combine($@"{ListDataFolder}", $"{sector}.txt");
                var o = Helpers.WebClientExt.GetToBytes(url, false);
                if (o.Item3 != null)
                    throw new Exception($"WA_MorningStarScreenerLoader: Error while download from {url}. Error message: {o.Item3.Message}");
                File.WriteAllBytes(filename, o.Item1);
            }
        }

        #region =========  SubClasses  ===========
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

        #region ============  Json classes  ==============

        private class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime TimeStamp;
        }

        private class cRoot
        {
            public cPagination pagination;
            public cResult[] results;
        }

        private class cPagination
        {
            public int count;
            public string page;
            public int totalCount;
            public int totalPages;
        }

        private class cResult
        {
            public cMeta meta;
            public cFields fields;
        }

        private class cMeta
        {
            public string securityID;
            public string performanceID;
            public string companyID;
            public string universe;
            public string exchange;
            public string ticker;
        }

        private class cFields
        {
            public cStringValue name;
            public cStringValue ticker;
        }

        private class cStringValue
        {
            public string value;
        }
        #endregion
    }
}
