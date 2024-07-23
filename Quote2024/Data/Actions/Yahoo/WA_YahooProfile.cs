using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const string ListUrlTemplateByLetter = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}&matchType=prefix&limit=300000&filter=statuscode:200";
        private const string ListDataFolderByLetter = @"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_ListByLetter";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_Data";
        private const string FirstYahooSectorJsonFileName = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_20240704.zip";

        public static Dictionary<string, object> _validSectors = new Dictionary<string, object>
        {
            { "Basic Materials", null }, { "Communication Services", null }, { "Consumer Cyclical", null },
            { "Consumer Defensive", null }, { "Energy", null }, { "Financial Services", null }, { "Healthcare", null },
            { "Industrials", null }, { "Real Estate", null }, { "Technology", null }, { "Utilities", null }
        };

        private static Dictionary<string, int> _exchanges = new Dictionary<string, int> { { "", 0 } };

        public static void TestWAFiles()
        {
            var symbolXref = GetSymbolXref(false).ToDictionary(a=>a.Value, a=>a.Key);
            var folder = @"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_Data.Short";
            var files = Directory.GetFiles(folder, "*.html");
            foreach (var file in files)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var yahooSymbol = ss[0];
                if (!symbolXref.ContainsKey(yahooSymbol))
                {
                    Debug.Print($"Bad file: {yahooSymbol}\t{Path.GetFileName(file)}");
                }
            }
        }

        public static void ParseAndSaveToDb()
        {
            var maxNameLen = 0;
            var maxSectorLen = 0;

            Logger.AddMessage($"Started");
            var firstYahooSectorJsonData = YahooSectorLoader.ParseZip(FirstYahooSectorJsonFileName)
                .ToDictionary(a => a.YahooSymbol, a => a);

            var yahooSymbolXref = GetSymbolXref(false);
            var polygonSymbolXref = yahooSymbolXref.ToDictionary(a => a.Value, a => a.Key);

            var data = new List<(string, string, string, DateTime)>();
            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\Data";
            var zipFiles = Directory.GetFiles(folder, "*.zip");
            foreach (var zipFileName in zipFiles)
            {
                Logger.AddMessage($"Parse data from {Path.GetFileName(zipFileName)}");
                ParseZip(zipFileName, null);
            }

            var wa_zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_Data.Short.zip";
            Logger.AddMessage($"Parse data from {Path.GetFileName(wa_zipFileName)}");
            ParseZip(wa_zipFileName, polygonSymbolXref, true);

            Logger.AddMessage($"Save data to database");
            var dbData = new List<DbItem>();
            DbItem lastDbItem = null;
            foreach (var item in data.OrderBy(a => a.Item1).ThenByDescending(a => a.Item4))
            {
                var symbol = item.Item1;
                var name = item.Item2;
                if (string.IsNullOrEmpty(name))
                    name = null;
                else if (name.EndsWith("(REIT)", StringComparison.InvariantCulture))
                    name = name.Substring(0, name.Length - 6).Trim();
                else if (name != null && name.EndsWith("(delisted)", StringComparison.InvariantCulture))
                    name = name.Substring(0, name.Length - 10).Trim();

                var sector = item.Item3;
                if (string.IsNullOrEmpty(sector))
                    throw new Exception("Check");

                // Set blank name in lastDbItem
                if (lastDbItem != null && string.IsNullOrEmpty(lastDbItem.Name) && lastDbItem.YahooSymbol == symbol &&
                    lastDbItem.Sector == sector)
                    lastDbItem.Name = name;

                // Set blank name from lastDbItem value
                if (string.IsNullOrEmpty(name) && lastDbItem != null && lastDbItem.YahooSymbol == symbol &&
                    lastDbItem.Sector == sector)
                    name = lastDbItem.Name;

                if (lastDbItem == null || lastDbItem.YahooSymbol != symbol ||
                    !string.Equals(lastDbItem.Name, name, StringComparison.InvariantCultureIgnoreCase) ||
                    lastDbItem.Sector != sector)
                {
                    var to = (DateTime?)item.Item4;

                    if (lastDbItem == null || (firstYahooSectorJsonData.ContainsKey(symbol) && lastDbItem.YahooSymbol != symbol))
                    {
                        var firstItem = firstYahooSectorJsonData[symbol];
                        if (sector == firstItem.Sector && (string.IsNullOrEmpty(firstItem.Name) ||
                                                           string.IsNullOrEmpty(name) || string.Equals(firstItem.Name,
                                                               name, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            to = null;
                        }
                    }

                    if (lastDbItem != null && lastDbItem.YahooSymbol == symbol &&
                        _validSectors.ContainsKey(lastDbItem.Sector) && !_validSectors.ContainsKey(sector) &&
                        CsUtils.MyCalculateSimilarity(name, lastDbItem.Name)>=0.7)
                    {
                        lastDbItem.Date = item.Item4.Date;
                    }
                    else if (lastDbItem != null && lastDbItem.YahooSymbol == symbol &&
                        !_validSectors.ContainsKey(lastDbItem.Sector) && _validSectors.ContainsKey(sector) &&
                        CsUtils.MyCalculateSimilarity(name, lastDbItem.Name) >= 0.7)
                    {
                        lastDbItem.Sector = sector;
                        lastDbItem.Date = item.Item4.Date;
                    }
                    else
                    {
                        if (lastDbItem != null)
                            dbData.Add(lastDbItem);
                        lastDbItem = new DbItem()
                        {
                            PolygonSymbol = yahooSymbolXref[symbol], YahooSymbol = symbol, Name = name, Sector = sector,
                            Date = item.Item4.Date, To = to, LastUpdated = item.Item4
                        };
                    }

                    if (!string.IsNullOrEmpty(lastDbItem.Name) && maxNameLen < lastDbItem.Name.Length)
                        maxNameLen = lastDbItem.Name.Length;
                    if (maxSectorLen < lastDbItem.Sector.Length) maxSectorLen = lastDbItem.Sector.Length;
                }
                else
                {
                    lastDbItem.Date = item.Item4.Date;
                }
            }
            dbData.Add(lastDbItem);

            DbHelper.ClearAndSaveToDbTable(dbData, "dbQ2024..SectorYahoo", "PolygonSymbol", "Date", "To",
                "Name", "Sector", "LastUpdated", "YahooSymbol");

            Logger.AddMessage($"Finished");


            void ParseZip(string zipFileName, Dictionary<string, string> symbolXref, bool ignoreIfNotExistsInSymbolXref = false)
            {
                var cnt = 0;
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        if (++cnt % 100 == 0)
                            Logger.AddMessage($"Parse data from {Path.GetFileName(zipFileName)}. Parsed {cnt:N0} from {zip.Entries.Count:N0} items.");

                        if (entry.Name.EndsWith("ACT_20230406035606.html") || entry.Name.EndsWith("AXP_20230410144059.html") ||
                            entry.Name.EndsWith("OXM_20230429222332.html") || entry.Name.EndsWith("SONO_20230422222307.html") ||
                            entry.Name.EndsWith("WAB_20230417233217.html") || entry.Name.EndsWith("ILMNV_20240624224841.html") ||
                            entry.Name.EndsWith("THRX_20170509210723.html"))
                        { // Bad files
                            continue;
                        }

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var polygonSymbol = ss[ss.Length - 2];
                        if (ignoreIfNotExistsInSymbolXref && !symbolXref.ContainsKey(polygonSymbol))
                            continue;

                        var yahooSymbol = symbolXref == null ? polygonSymbol : symbolXref[polygonSymbol];
                        var dateKey = DateTime.ParseExact(ss[ss.Length - 1],
                            ss[ss.Length - 1].Length == 14 ? "yyyyMMddHHmmss" : "yyyyMMdd",
                            CultureInfo.InvariantCulture);
                        var content = entry.GetContentOfZipEntry();
                        var tempItem = ParseHtmlContent(content, yahooSymbol);
                        if (!string.IsNullOrEmpty(tempItem.Item4) && !(tempItem.Item3 ?? "").StartsWith("Other"))
                            data.Add((tempItem.Item1, tempItem.Item2, tempItem.Item4, dateKey));
                    }
            }
        }

        private static (string, string, string, string) ParseHtmlContent(string content, string yahooSymbol)
        {
            var i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
            if (i1 > 1)
            {
                return (null, null, null, null);
            }

            // 2020 year format
            i1 = content.IndexOf("<h1 class=\"D(ib) Fz(1", StringComparison.InvariantCulture);
            if (i1 > -1) // Count, from, to: 20652, 2020-01-01, 2024-06-27
            { // AA_20200512043535.html
                var k2 = content.IndexOf("<h1 class=\"D(ib) Fz(1", i1 + 5, StringComparison.InvariantCulture);
                if (k2 != -1)
                    throw new Exception("Check YahooProfile parser");

                var i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                if (i2 == -1) i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                if (i2 == -1 && CheckSectorWord2020())
                    throw new Exception("Check YahooProfile parser");

                var symbolAndName = GetSymbolAndName2020();
                if (symbolAndName[0] != yahooSymbol)
                    throw new Exception($"Invalid yahoo symbol: {symbolAndName[0]}. Must be: {yahooSymbol}");
                var sector = GetSector2020(i2);
                return ApplyCorrections(symbolAndName[0], symbolAndName[1], symbolAndName[2], sector);
            }

            // 2024 year format
            i1 = content.IndexOf("<h1 class=\"svelte-", StringComparison.InvariantCulture);
            if (i1 > -1) // Count, from, to: 1328, 2023-11-12, 2024-07-05
            { // AAON_20240503140454.html
                var k2 = content.IndexOf("<h1 class=\"svelte-", i1 + 5, StringComparison.InvariantCulture);
                if (k2 != -1)
                    throw new Exception("Check YahooProfile parser");

                var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                if (i2 == -1 && CheckSectorWord2024())
                    throw new Exception("Check YahooProfile parser");

                var symbolAndName = GetSymbolAndName2023();
                if (symbolAndName[0] != yahooSymbol)
                    throw new Exception($"Invalid yahoo symbol: {symbolAndName[0]}. Must be: {yahooSymbol}");
                var sector = GetSector2023(i2);
                return ApplyCorrections(symbolAndName[0], symbolAndName[1], symbolAndName[2], sector);
            }

            i1 = content.IndexOf(">Symbols similar to ", StringComparison.InvariantCulture);
            if (i1 > -1) // Not found
                return (null, null, null, null);

            throw new Exception("Check YahooProfile parser");

            bool CheckSectorWord2020()
            {
                var content1 = content.Replace("https://finance.yahoo.com/sectors\" title=\"Sectors\">Sectors", "\">");
                content1 = content1.Replace("sector information, ", "");
                content1 = content1.Replace("finance.yahoo.com/sector/", "x");
                if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                    return false;

                return true;
            }

            bool CheckSectorWord2024()
            {
                var content1 = content;
                if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                    return false;
                if (content1.IndexOf(">Profile Information Not Available<",
                        StringComparison.InvariantCulture) != -1)
                { // CNH_20240520210256.html
                    return false;
                }
                if (content1.IndexOf(">Profile data is currently not available for ", StringComparison.InvariantCulture) != -1)
                { // ENERW_20240422165327.html
                    return false;
                }

                return true;
            }

            string[] GetSymbolAndName2020()
            {
                var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                if (i2 == -1 || i3 < i2)
                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");

                var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                if (s1.StartsWith("REIT "))
                    s1 = s1.Substring(5);
                i2 = s1.LastIndexOf('(');
                if (s1.EndsWith(')') && i2 >= 0)
                {
                    var name = s1.Substring(0, i2).Trim();
                    var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                    var exchange = GetExchange2020(i3);
                    if (symbol == "REIT")
                        s1 = s1.Substring(0, s1.Length - 6).Trim(); // format: symbol - name (REIT)
                    else if (symbol.Contains(',')) {} // Format: symbol - name (xxx,xx)
                    else
                        return new[] { symbol, name, exchange };
                }

                i2 = s1.IndexOf('-');
                if (i2 > 0 && s1.Length > (i2 + 1) && s1[i2 + 1] != ' ') // Symbol like XX-PA
                    i2 = s1.IndexOf('-', i2 + 1);
                if (i2 >= 0)
                {
                    var symbol = s1.Substring(0, i2 - 1).Trim();
                    var name = s1.Substring(i2 + 1).Trim();
                    var exchange = GetExchange2020(i3);
                    return new[] { symbol, name, exchange };
                }

                throw new Exception("Check YahooProfile parser (GetSymbolAndName2020 method)");
            }

            string[] GetSymbolAndName2023()
            {
                var i2 = content.IndexOf(">", i1 + 5, StringComparison.InvariantCulture);
                var i3 = content.IndexOf("</h1", i1 + 5, StringComparison.InvariantCulture);
                if (i2 == -1 || i3 < i2)
                    throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");

                var s1 = content.Substring(i2 + 1, i3 - i2 - 1);
                s1 = System.Net.WebUtility.HtmlDecode(s1).Trim();
                i2 = s1.LastIndexOf('(');
                if (s1.EndsWith(')') && i2 >= 0)
                {
                    var name = s1.Substring(0, i2).Trim();
                    var symbol = s1.Substring(i2 + 1, s1.Length - i2 - 2).Trim();
                    var exchange = GetExchange2023();
                    return new[] { symbol, name, exchange };
                }

                i2 = s1.IndexOf('-');
                if (i2 >= 0)
                {
                    var symbol = s1.Substring(0, i2 - 1).Trim();
                    var name = s1.Substring(i2 + 1).Trim();
                    var exchange = GetExchange2023();
                    return new[] { symbol, name, exchange };
                }

                throw new Exception("Check YahooProfile parser (GetSymbolAndName2023 method)");
            }

            string GetExchange2020(int startIndex)
            {
                var i2 = content.IndexOf("</span>", startIndex, StringComparison.InvariantCulture);
                var i1 = content.LastIndexOf(">", i2 - 7, StringComparison.InvariantCulture);
                var s = content.Substring(startIndex, i2 - startIndex);
                var s1 = content.Substring(i1 + 1, i2 - i1 - 1);
                var i3 = s1.IndexOf('-', StringComparison.InvariantCulture);
                if (i3 == -1)
                {
                    Debug.Print($"No exchange: {yahooSymbol}");
                    _exchanges[""]++;
                    return null;
                }
                var exchange = s1.Substring(0, i3).Trim();
                if (!_exchanges.ContainsKey(exchange))
                    _exchanges.Add(exchange, 0);
                _exchanges[exchange]++;
                return exchange;
            }

            string GetExchange2023()
            {
                var i21 = content.LastIndexOf("exchange svelte-", i1, StringComparison.InvariantCulture);
                var exchange = GetExchange2020(i21);
                return exchange;
            }

            string GetSector2020(int sectorKeyPosition)
            {
                if (sectorKeyPosition == -1) return null;
                var i2 = content.IndexOf("<span", sectorKeyPosition, StringComparison.InvariantCulture);
                var i3 = content.IndexOf(">", i2, StringComparison.InvariantCulture);
                var i4 = content.IndexOf("</span>", i3, StringComparison.InvariantCulture);
                if (i4 > i3 && i3 > i2 && i2 > 0)
                {
                    var sector = content.Substring(i3 + 1, i4 - i3 - 1).Trim();
                    return sector;
                }

                throw new Exception("Check YahooProfile parser (GetSector2020 method)");
            }

            string GetSector2023(int sectorKeyPosition)
            {
                if (sectorKeyPosition == -1) return null;
                var i3 = content.IndexOf("</a>", sectorKeyPosition, StringComparison.InvariantCulture);
                var i2 = content.LastIndexOf(">", i3, StringComparison.InvariantCulture);
                if (i3 > i2 && i2 > 0)
                {
                    var sector = content.Substring(i2 + 1, i3 - i2 - 1).Trim();
                    return sector;
                }

                throw new Exception("Check YahooProfile parser (GetSector2020 method)");
            }

            (string, string, string, string) ApplyCorrections(string symbol, string name, string exchange, string sector)
            {
                if (string.IsNullOrEmpty(sector)) return (null, null, null, null);

                /*// Correction of non-standard sectors
                if (sector == "Industrial Goods" && (symbol == "BLDR" || symbol == "ECOL" || symbol == "GE" || symbol == "UTX")) sector = "Industrials";

                if (sector == "Financial" && symbol == "MFA") sector = "Real Estate";
                else if (sector == "Financial" && (symbol == "ALL" || symbol == "CBSH" || symbol == "ISTR" ||
                                                   symbol == "NAVI" || symbol == "SFE" ||
                                                   symbol == "ZTR")) sector = "Financial Services";

                if (sector == "Services" && (symbol == "EAT" || symbol == "PZZA")) sector = "Consumer Cyclical";
                else if (sector == "Services" && (symbol == "EROS" || symbol == "NFLX")) sector = "Communication Services";
                else if (sector == "Services" && (symbol == "NSC" || symbol == "TTEK" || symbol == "ZNH")) sector = "Industrials";
                else if (sector == "Consumer Goods" && symbol == "KNDI") sector = "Consumer Cyclical";
                else if (sector == "Consumer Goods" && symbol == "AAPL") sector = "Technology";
                else if (sector == "Consumer Goods" && (symbol == "BGS" || symbol == "HSY")) sector = "Consumer Defensive";*/

                if (string.IsNullOrEmpty(name)) name = null;
                if (string.IsNullOrEmpty(symbol))
                    throw new Exception("Check YahooProfile parser (AddSector method)");

                return (symbol, name, exchange, sector);
            }
        }

        public static async Task DownloadDataByLetter()
        {
            // Get url and file list
            var symbolXref = GetSymbolXref(true);
            var zipFileName = ListDataFolderByLetter + ".zip";
            var lastSymbol = "";
            var lastDateKey = "";
            var toDownload = new List<DownloadItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var lines = entry.GetLinesOfZipEntry().ToArray();
                    var items = entry.GetLinesOfZipEntry().Select(a => new JsonItem(a))
                        .Where(a => a.Url.Contains("profile", StringComparison.InvariantCultureIgnoreCase) &&
                                    a.Type == "text/html" && a.Status == 200 && a.IsOk).OrderBy(a => a.Symbol)
                        .ThenBy(a => a.TimeStamp).ToArray();
                    foreach (var item in items)
                    {
                        if (!symbolXref.ContainsKey(item.Symbol))
                            continue;

                        var dateKey = item.TimeStamp.Substring(0, 8);
                        if (dateKey == lastDateKey && string.Equals(lastSymbol, item.Symbol)) // skip the same symbol/day
                            continue;

                        var date = DateTime.ParseExact(item.TimeStamp, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
                        if (date >= new DateTime(2017, 5, 1) && date <= new DateTime(2024, 7, 5))
                        {
                            var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                            var polygonSymbol = symbolXref[item.Symbol];
                            var filename = Path.Combine(HtmlDataFolder, $"{polygonSymbol}_{item.TimeStamp}.html");
                            var checkFilename =
                                Path.Combine(@"E:\Quote\WebData\Symbols\Yahoo\WA_Profile\WA_Data.Short.2020",
                                    $"{polygonSymbol}_{item.TimeStamp}.html");
                            if (!File.Exists(checkFilename))
                                toDownload.Add(new DownloadItem(url, filename));
                        }

                        lastSymbol = item.Symbol;
                        lastDateKey = dateKey;
                    }
                }

            // Download data
            await WebClientExt.DownloadItemsToFiles(toDownload.Cast<WebClientExt.IDownloadToFileItem>().ToList(), 10);

            Helpers.Logger.AddMessage($"Downloaded {toDownload.Count:N0} items");
        }

        private static async Task Download(ConcurrentDictionary<DownloadItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    var data = await kvp.Value;
                    // var htmlContent = HtmlHelper.RemoveUselessTags(System.Text.Encoding.UTF8.GetString(data));
                    // File.WriteAllText(kvp.Key.Filename, htmlContent);
                    File.WriteAllText(kvp.Key.Filename, System.Text.Encoding.UTF8.GetString(data));
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

        public static void DownloadListByLetter()
        {
            Logger.AddMessage($"Started");
            var letters = Enumerable.Range('A', 26).Select(a => Convert.ToChar(a).ToString()).ToArray();
            var count = 0;
            foreach (var letter in letters)
            {
                count++;
                Logger.AddMessage($"Process urls which start with {letter} letter");
                var filename = Path.Combine($@"{ListDataFolderByLetter}", $"{letter}.txt");
                if (!File.Exists(filename))
                {
                    byte[] data = null;
                    var errorCnt = 0;
                    while (data == null)
                    {
                        var url = string.Format(ListUrlTemplateByLetter, letter);
                        var o = Helpers.WebClientExt.GetToBytes(url, false);
                        //if (o.Item3 != null)
                        //  throw new Exception($"WA_YahooProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                        if (o.Item3 != null)
                        {
                            errorCnt++;
                            Logger.AddMessage($"{errorCnt} errors. Last: {o.Item3}");
                            System.Threading.Thread.Sleep(200);
                        }
                        data = o.Item1;
                    }

                    File.WriteAllBytes(filename, data);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        private static Dictionary<string, string> GetSymbolXrefForUrlList()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        private static Dictionary<string, string> GetSymbolXref(bool notEtf)
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                var etfPart = notEtf ? "and MyType not like 'ET%'" : "";
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  $"and YahooSymbol is not null {etfPart} order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
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
            public bool IsOk;
            public string Symbol;

            public JsonItem(string line)
            {
                var ss = line.Split(' ');
                TimeStamp = ss[1];
                Url = ss[2];
                Type = ss[3];
                if (ss[4] != "-")
                    Status = int.Parse(ss[4]);
                IsOk = Url.EndsWith("/profile/", StringComparison.CurrentCultureIgnoreCase) ||
                              Url.EndsWith("/profile", StringComparison.InvariantCultureIgnoreCase) ||
                              Url.Contains("/profile/?", StringComparison.InvariantCultureIgnoreCase) ||
                              Url.Contains("/profile?", StringComparison.InvariantCultureIgnoreCase);

                if (IsOk)
                {
                    var i2 = Url.IndexOf("/profile", StringComparison.InvariantCultureIgnoreCase);
                    var i1 = Url.LastIndexOf("/", i2 - 1, StringComparison.InvariantCulture);
                    Symbol = Url.Substring(i1 + 1, i2 - i1 - 1).Trim().ToUpper();
                }
            }
        }

        public class DownloadItem: WebClientExt.IDownloadToFileItem
        {
            public string Url { get; }
            public string Filename { get; }
            public HttpStatusCode? StatusCode { get; set; }

            public DownloadItem(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }

            public override string ToString() => Path.GetFileNameWithoutExtension(Filename);
        }
        #endregion

    }
}
