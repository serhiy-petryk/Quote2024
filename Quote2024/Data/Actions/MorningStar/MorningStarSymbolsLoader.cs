using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.MorningStar
{
    public static class MorningStarSymbolsLoader
    {
        // Server error for: https://www.morningstar.com/stocks/xnas/vbnk/quote, https://www.morningstar.com/stocks/xnas/na/quote

        // https://www.morningstar.com/api/v2/stocks/xnas/naaa/quote
        // private const string UrlTemplateJson = @"https://www.morningstar.com/api/v2/stocks/{1}/{0}/quote";
        private const string UrlTemplateJson = @"https://www.morningstar.com/stocks/{1}/{0}/quote";
        private const string UrlTemplateHtml = @"https://www.morningstar.com/stocks/{1}/{0}/quote";
        private const string FileTemplateHtml = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240615\MSProfile_{1}_{0}.html";
        // private const string FileTemplateJson = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_{2}\MSProfile_{1}_{0}.json";
        private const string FileTemplateJson = @"E:\Quote\WebData\Symbols\MorningStar\Profile\Data\MSProfiles_{2}\MSProfile_{1}_{0}.html";
        // private const string FolderTemplateJson = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_{0}";
        private const string FolderTemplateJson = @"E:\Quote\WebData\Symbols\MorningStar\Profile\Data\MSProfiles_{0}";

        public static void Test()
        {
            var dbItems = new List<MorningStarScreenerLoader.DbItem>();
            var types = new Dictionary<string, int>();
            var folder = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240620";
            var files = Directory.GetFiles(folder, "*.json").OrderBy(a=>a).ToArray();
            var ss2 = Path.GetFileNameWithoutExtension(folder).Split('_');
            var dateKey = DateTime.ParseExact(ss2[ss2.Length - 1], "yyyyMMdd", CultureInfo.InvariantCulture);
            foreach (var file in files)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var symbol = ss[ss.Length - 1];
                var exchange = ss[ss.Length - 2];

                var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(file));
                if (string.Equals(symbol.Replace('^', 'p'), oo.page.ticker, StringComparison.InvariantCulture))
                {

                }
                else if (string.Equals(symbol.Replace("^", ".PR"), oo.page.ticker, StringComparison.InvariantCulture))
                {

                }
                else if (symbol.Contains('^') &&
                         oo.page.name.EndsWith(" Pref Share", StringComparison.InvariantCulture))
                {

                }
                else
                {

                }
                var type = oo.page?.investmentType ?? "";
                var oSector = oo.components?.profile?.payload?.dataPoints?.sector;
                var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                if (oSector == null)
                {
                    Debug.Print($"No sector: {exchange}:{symbol}. Type: {type}");
                }
                else if (string.IsNullOrEmpty(sector))
                {
                    Debug.Print($"Blank sector: {exchange}:{symbol}. Type: {type}");
                }
                else
                {
                    var dbItem = new MorningStarScreenerLoader.DbItem
                    {
                        Symbol = symbol, Exchange = exchange, Name = oo.page?.name, Sector = sector, Date = dateKey,
                        TimeStamp = File.GetLastWriteTime(file)
                    };
                    dbItems.Add(dbItem);

                    if (!types.ContainsKey(type))
                        types.Add(type, 0);
                    types[type]++;
                    if (type != "EQ")
                        Debug.Print($"Not equity: {exchange}:{symbol}. Type: {type}");
                }
            }

            // DbHelper.ClearAndSaveToDbTable(dbItems, "dbQ2023Others..Bfr_ScreenerMorningStar", "Symbol", "Date",
               // "Exchange", "Sector", "Name", "TimeStamp");

        }

        public static List<MsSymbolItem> CheckFiles(List<MsSymbolItem> originalItems)
        {
            var data = new List<MsSymbolItem>();
            foreach (var item in originalItems)
            {
                if (item.StatusCode == HttpStatusCode.BadGateway ||
                    item.StatusCode == HttpStatusCode.InternalServerError)
                    data.Add(item);
                else if (item.StatusCode == HttpStatusCode.NotFound)
                {
                    var secondMsSymbol = MorningStarCommon.GetSecondMorningStarProfileTicker(item.PolygonSymbol);
                    if (!string.IsNullOrEmpty(secondMsSymbol) && !string.Equals(item.MsSymbol, secondMsSymbol, StringComparison.CurrentCultureIgnoreCase) && 
                        item.Exchange==item.ExchangeForUrl)
                    {
                        item.MsSymbol = secondMsSymbol;
                        item.StatusCode = null;
                        data.Add(item);
                        continue;
                    }

                    if (item.Exchange == item.ExchangeForUrl && (item.Exchange == "XNAS" || item.Exchange == "XNYS"))
                    {
                        item.ExchangeForUrl = item.Exchange == "XNAS" ? "XNYS" : "XNAS";
                        item.StatusCode = null;
                        data.Add(item);
                    }
                    /*if (!string.IsNullOrEmpty(secondMsSymbol) && item.MsSymbol== secondMsSymbol && item.Exchange != item.ExchangeForUrl && (item.Exchange == "XNAS" || item.Exchange == "XNYS"))
                    {
                        item.ExchangeForUrl = item.Exchange == "XNAS" ? "XNYS" : "XNAS";
                        item.MsSymbol = MorningStarCommon.GetMorningStarProfileTicker(item.PolygonSymbol);
                        item.StatusCode = null;
                        data.Add(item);
                    }*/
                }
                else if (File.Exists(item.Filename))
                {
                    var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(item.Filename));
                    var oSector = oo.components?.profile?.payload?.dataPoints?.sector;
                    if (oSector == null)
                    {
                        File.Delete(item.Filename);
                        data.Add(item);
                    }
                }
                else
                {
                    data.Add(item);
                }
            }

            foreach (var item in data)
                item.StatusCode = null;

            return data;
        }

        public class MsSymbolItem : WebClientExt.IDownloadItem
        {
            public string PolygonSymbol;
            public string MsSymbol;
            public string Exchange;
            public string ExchangeForUrl;
            public string Url => string.Format(UrlTemplateJson, MsSymbol.ToLower(), ExchangeForUrl.ToLower());
            public string Filename => string.Format(FileTemplateJson, PolygonSymbol, Exchange, Date.ToString("yyyyMMdd"));
            public int DownloadAttempts { get; set; }
            public HttpStatusCode? StatusCode { get; set; }

            public string SymbolType;
            public cSector oSector;
            public string Sector => oSector?.value;
            public DateTime Date;
            public DateTime TimeStamp;

            public MsSymbolItem(string polygonSymbol, string exchange, DateTime dateKey)
            {
                PolygonSymbol = polygonSymbol;
                MsSymbol = MorningStarCommon.GetMorningStarProfileTicker(PolygonSymbol);
                Exchange = exchange;
                ExchangeForUrl = exchange;
                Date = dateKey;
                var dataFolder = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);
            }
        }

        private static async Task Download(ConcurrentDictionary<MsSymbolItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    var data = await kvp.Value;
                    /*var oo = ZipUtils.DeserializeBytes<cRoot>(data);
                    var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                    if (string.IsNullOrEmpty(sector))
                    {
                        var data2 = WebClientExt.GetToBytes(kvp.Key.Url, false);
                        if (data2.Item1 != null) data = data2.Item1;
                    }*/
                    File.WriteAllBytes(kvp.Key.Filename, data);
                    kvp.Key.StatusCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.NotFound ||
                         response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway))
                    {
                        kvp.Key.StatusCode = response.StatusCode;
                        continue;
                    }

                    throw new Exception(
                        $"MorningStarSymbolsLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }

        }

        public static async Task StartJson()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var symbolItems = GetSymbolsAndExchanges2(timeStamp.Item1.Date).ToList();
            await DownloadItems(symbolItems);

            var checkCount = int.MaxValue;
            while (true)
            {
                var badItems = CheckFiles(symbolItems);
                Debug.Print($"Bad item count: {badItems.Count}");
                if (badItems.Count == 0 || badItems.Count >= checkCount)
                    break;
                await DownloadItems(badItems);
                checkCount = badItems.Count;
            }

            foreach(var item in symbolItems.Where(a=>a.StatusCode == HttpStatusCode.NotFound))
                Debug.Print($"Not found:\t{item.Exchange}\t{item.PolygonSymbol}\t{item.MsSymbol}");

            var items = symbolItems.Count();
            var notFoundItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.NotFound);
            var badGatewayItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.BadGateway);
            var serverErrorItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.InternalServerError);
            Helpers.Logger.AddMessage($"Finished. Items: {items:N0}. Not found: {notFoundItems:N0}. Server errors: {serverErrorItems:N0}. Bad Gateway errors: {badGatewayItems:N0}");
        }

        public static async Task DownloadItems(List<MsSymbolItem> symbolItems)
        {
            var tasks = new ConcurrentDictionary<MsSymbolItem, Task<byte[]>>();
            var needToDownload = true;
            while (needToDownload)
            {
                needToDownload = false;

                foreach (var symbolItem in symbolItems)
                {
                    if (!File.Exists(symbolItem.Filename))
                    {
                        if (symbolItem.StatusCode != HttpStatusCode.NotFound)
                        {
                            var task = WebClientExt.DownloadToBytesAsync(symbolItem.Url);
                            tasks[symbolItem] = task;
                            symbolItem.StatusCode = null;
                        }
                    }
                    else
                        symbolItem.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 20)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {symbolItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {symbolItems.Count:N0}");
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
                Helpers.Logger.AddMessage($"Downloaded {symbolItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {symbolItems.Count:N0}");
            }
        }

        public static async void StartHttp()
        {
            const int chunkSize = 200;
            var symbolsAndExchanges = GetSymbolsAndExchanges();
            var sublist = symbolsAndExchanges.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize).Select(x => x.Select(v => v.Value).ToList()).ToList();

            var items = 0;
            var notFoundItems = 0;
            var serverError = 0;

            Logger.AddMessage($"Started");
            foreach (var listItem in sublist)
            {
                // System.Threading.Thread.Sleep(1000);
                var tasks = new ConcurrentDictionary<(string, string, string), Task<byte[]>>();
                foreach (var symbolAndExchange in listItem)
                {
                    var filename = string.Format(FileTemplateHtml, symbolAndExchange.Item1, symbolAndExchange.Item3);
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplateHtml, symbolAndExchange.Item2.ToLower(), symbolAndExchange.Item3.ToLower());
                        var task = WebClientExt.DownloadToBytesAsync(url);
                        tasks[symbolAndExchange] = task;
                    }
                    else
                        items++;
                }

                foreach (var kvp in tasks)
                {
                    if (items % 10 == 0)
                        Helpers.Logger.AddMessage($"Downloaded {items} items from {symbolsAndExchanges.Count}");
                    items++;

                    try
                    {
                        // release control to the caller until the task is done, which will be near immediate for each task following the first
                        var data = await kvp.Value;
                        var filename = string.Format(FileTemplateHtml, kvp.Key.Item1, kvp.Key.Item3);
                        File.WriteAllBytes(filename, data);
                    }
                    catch (Exception ex)
                    {
                        if (ex is WebException we && we.Response is HttpWebResponse webResponse &&
                            webResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            notFoundItems++;
                            continue;
                        }

                        if (ex is WebException we2 && we2.Response is HttpWebResponse webResponse2 &&
                            webResponse2.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            serverError++;
                            if (serverError < 24)
                                continue;
                        }

                        throw new Exception(
                            $"MorningStarSymbolsLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                    }
                }
            }

            /*items = 7000;
            foreach (var symbolAndExchange in symbolsAndExchanges.Skip(7000))
            {
                if (items % 10 == 0)
                    Helpers.Logger.AddMessage($"Downloaded {items} items");
                items++;

                var filename = string.Format(FileTemplate, symbolAndExchange.Item1, symbolAndExchange.Item2);
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, symbolAndExchange.Item1, symbolAndExchange.Item2);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                    {
                        if (o.Item3 is WebException we && we.Response is HttpWebResponse webResponse &&
                            webResponse.StatusCode == HttpStatusCode.NotFound)
                        {
                            notFoundItems++;
                            continue;
                        }

                        throw new Exception($"MorningStarSymbolsLoader: Error while download from {url}. Error message: {o.Item3.Message}");
                    }
                    File.WriteAllBytes(filename, o.Item1);
                }
            }*/

            Helpers.Logger.AddMessage($"Finished. Items: {items:N0}. Not found items: {notFoundItems:N0}. Server errors:{serverError}");
        }

        private static List<(string, string, string)> GetSymbolsAndExchanges()
        {
            var data = new List<(string, string, string)>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE [To] is null and IsTest is null and MyType not like 'ET%' " +
                                  // "and MyType not in ('FUND', 'RIGHT', 'WARRANT') and symbol in ('NA', 'VBNK','BSRR','CLEU','LDWY','LEDS','MGPI','MTRX','RVYL','SLAB','SRTS','TSVT','HGTY','TFPM')";
                                  "and MyType not in ('FUND', 'RIGHT', 'WARRANT')";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var symbol = (string)rdr["symbol"];
                        var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(symbol);
                        if (!string.IsNullOrEmpty(msSymbol))
                        {
                            data.Add((symbol, msSymbol, (string)rdr["exchange"]));
                        }
                    }
            }

            return data;
        }

        private static List<MsSymbolItem> GetSymbolsAndExchanges2(DateTime dateKey)
        {
            var data = new List<MsSymbolItem>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null "+
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') order by 2";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var item = new MsSymbolItem((string)rdr["symbol"], (string)rdr["exchange"], dateKey);
                        if (!string.IsNullOrEmpty(item.MsSymbol))
                            data.Add(item);
                    }
            }

            return data;
        }

        public static void ParseAll()
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(FileTemplateHtml), "*.html");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var i1 = content.IndexOf(">Sector</dt>", StringComparison.InvariantCulture);
                if (i1 < 0)
                {
                    Debug.Print($"Bad: {Path.GetFileName(file)}");
                    var newFileName = Path.GetDirectoryName(file) + @"\save\" + Path.GetFileName(file);
                    // File.Move(file, newFileName);
                    continue;
                }
                var i2 = content.IndexOf("</span>", i1 + 5, StringComparison.InvariantCulture);
                var s = content.Substring(i1, i2-i1);
                i1 = s.LastIndexOf(">", StringComparison.InvariantCulture);
                var sector = s.Substring(i1+1).Trim();
            }
            
        }

        #region ===========  Json SubClasses  ===========
        public class cRoot
        {
            public cPage page;
            public cComponents components;
        }

        public class cPage
        {
            public string ticker;
            public string name;
            public string investmentType;
        }
        public class cComponents
        {
            public cProfile profile;
        }
        public class cProfile
        {
            public cPayload payload;
        }
        public class cPayload
        {
            public cDataPoints dataPoints;
        }
        public class cDataPoints
        {
            public cSector sector;
        }
        public class cSector
        {
            public string name;
            public string value;
        }
        #endregion


    }
}
