using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        // https://www.morningstar.com/api/v2/stocks/xnas/naaa/quote
        private const string UrlTemplateApi = @"https://www.morningstar.com/api/v2/stocks/{1}/{0}/quote";
        private const string UrlTemplate = @"https://www.morningstar.com/stocks/{1}/{0}/quote";
        private const string FileTemplate = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240615\MSProfile_{1}_{0}.html";
        private const string FileTemplateApi = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240617.3\MSProfile_{1}_{0}.json";

        public static void Test()
        {
            var types = new Dictionary<string, int>();
            var folder = @"E:\Quote\WebData\Symbols\MorningStar\Data\MSProfiles_20240617.3";
            var files = Directory.GetFiles(folder, "*.json").OrderBy(a=>a).ToArray();
            foreach (var file in files)
            {
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var symbol = ss[ss.Length - 1];
                var exchange = ss[ss.Length - 2];

                var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(file));
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
                    if (!types.ContainsKey(type))
                        types.Add(type, 0);
                    types[type]++;
                    if (type != "EQ")
                        Debug.Print($"Not equity: {exchange}:{symbol}. Type: {type}");
                }
            }
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
                }
                else if (File.Exists(item.Filename))
                {
                    var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(item.Filename));
                    var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                    if (string.IsNullOrEmpty(sector))
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

        public static List<MsSymbolItem> CheckFilesX(List<MsSymbolItem> originalItems)
        {
            var data = new List<MsSymbolItem>();
            var files = Directory.GetFiles(Path.GetDirectoryName(FileTemplateApi), "*.json");
            foreach (var file in files)
            {
                var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(file));
                var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                if (string.IsNullOrEmpty(sector))
                {
                    var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                    var symbol = ss[ss.Length - 1];
                    var exchange = ss[ss.Length - 2];
                    data.AddRange(originalItems.Where(a =>
                        string.Equals(a.PolygonSymbol, symbol, StringComparison.InvariantCultureIgnoreCase) &&
                        string.Equals(a.Exchange, exchange, StringComparison.InvariantCultureIgnoreCase)));
                    Debug.Print($"No sector: {Path.GetFileName(file)}");
                }
            }
            return data;
        }

        public class MsSymbolItem
        {
            public string PolygonSymbol;
            public string MsSymbol;
            public string Exchange;
            public string Url;
            public string Filename;
            public HttpStatusCode? StatusCode;

            public MsSymbolItem(string polygonSymbol, string exchange)
            {
                PolygonSymbol = polygonSymbol;
                MsSymbol = MorningStarCommon.GetMorningStarProfileTicker(PolygonSymbol);
                Exchange = exchange;
                Url = string.Format(UrlTemplateApi, MsSymbol.ToLower(), Exchange.ToLower());
                Filename = string.Format(FileTemplateApi, PolygonSymbol, Exchange);
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

        public static async void StartJson()
        {
            WebClientExt.CheckVpnConnection();

            Logger.AddMessage($"Started");
            var symbolItems = GetSymbolsAndExchanges2();
            await DownloadItems(symbolItems);

            var checkCount = 0;
            while (true)
            {
                var badItems = CheckFiles(symbolItems);
                if (badItems.Count == checkCount)
                    break;
                await DownloadItems(badItems);
                checkCount = badItems.Count;
            }

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
                var doneItems = symbolItems.Count(a => (a.StatusCode == HttpStatusCode.OK || a.StatusCode == HttpStatusCode.NotFound));

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

                    if (tasks.Count >= 100)
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

            var items = symbolItems.Count();
            var notFoundItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.NotFound);
            var badGatewayItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.BadGateway);
            var serverErrorItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.InternalServerError);
            Helpers.Logger.AddMessage($"Finished. Items: {items:N0}. Not found: {notFoundItems:N0}. Server errors: {serverErrorItems:N0}. Bad Gateway errors: {badGatewayItems:N0}");
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
                    var filename = string.Format(FileTemplate, symbolAndExchange.Item1, symbolAndExchange.Item3);
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, symbolAndExchange.Item2.ToLower(), symbolAndExchange.Item3.ToLower());
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
                        var filename = string.Format(FileTemplate, kvp.Key.Item1, kvp.Key.Item3);
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
                                  "and MyType not in ('FUND', 'RIGHT', 'WARRANT') and symbol in ('NA', 'VBNK','BSRR','CLEU','LDWY','LEDS','MGPI','MTRX','RVYL','SLAB','SRTS','TSVT','HGTY','TFPM')";
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

        private static List<MsSymbolItem> GetSymbolsAndExchanges2()
        {
            var data = new List<MsSymbolItem>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE [To] is null and IsTest is null and MyType not like 'ET%' " +
                                  "and MyType not in ('FUND', 'RIGHT', 'WARRANT')";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var item = new MsSymbolItem((string)rdr["symbol"], (string)rdr["exchange"]);
                        if (!string.IsNullOrEmpty(item.MsSymbol))
                            data.Add(item);
                    }
            }

            return data;
        }

        public static void ParseAll()
        {
            var files = Directory.GetFiles(Path.GetDirectoryName(FileTemplate), "*.html");
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
