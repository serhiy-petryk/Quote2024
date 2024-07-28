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
        private const string ListDataFolder = @"E:\Quote\WebData\Screener\MorningStar\WA_List.2024-07";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Screener\MorningStar\WA_Data";

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
                    var sector =
                        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                            ss[0].Replace("-stocks", "").Replace('-', ' '));
                    if (sector == "Industrial") sector = "Industrials";
                    else if (sector == "Utility") sector = "Utilities";

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

    }
}
