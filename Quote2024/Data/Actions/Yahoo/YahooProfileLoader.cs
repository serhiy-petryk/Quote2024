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

namespace Data.Actions.Yahoo
{
    public static class YahooProfileLoader
    {
        private const string UrlTemplate = @"https://finance.yahoo.com/quote/{0}/profile/";
        private const string FileNameTemplate = Settings.DataFolder + @"Symbols\Yahoo\ProfileTest\Data\YP_{1}\YP_{0}_{1}.html";


        public static void RemoveNotFound()
        {
            var timeStamp = TimeHelper.GetTimeStamp();
            var filename = string.Format(FileNameTemplate, "", timeStamp.Item2);
            var dataFolder = Path.GetDirectoryName(filename);
            var newDataFolder = Path.Combine(dataFolder, "NotFound");
            if (!Directory.Exists(newDataFolder))
                Directory.CreateDirectory(newDataFolder);

            var files = Directory.GetFiles(dataFolder, "*.html");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                if (content.Contains(">Symbols similar to ", StringComparison.InvariantCulture) ||
                    content.Contains(">We are experiencing some temporary issues", StringComparison.InvariantCulture))
                {
                    var newFileName = Path.Combine(newDataFolder, Path.GetFileName(file));
                    File.Move(file, newFileName, true);
                }
                    // File.Delete(file);
            }
        }

        // Download only ~100 items with USA VPN. After than, download 'Not found' page. Cookies don't help.
        public static async Task Start()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var symbolItems = GetSymbolsAndExchanges(timeStamp.Item1.Date).OrderBy(a=>a.YahooSymbol).Take(555).ToList();
            await DownloadItems(symbolItems);

            /*var checkCount = int.MaxValue;
            while (true)
            {
                var badItems = CheckFiles(symbolItems);
                Debug.Print($"Bad item count: {badItems.Count}");
                if (badItems.Count == 0 || badItems.Count >= checkCount)
                    break;
                await DownloadItems(badItems);
                checkCount = badItems.Count;
            }*/

            foreach (var item in symbolItems.Where(a => a.StatusCode == HttpStatusCode.NotFound))
                Debug.Print($"Not found:\t{item.PolygonSymbol}\t{item.YahooSymbol}");


            var items = symbolItems.Count();
            var notFoundItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.NotFound);
            var badGatewayItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.BadGateway);
            var serverErrorItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.InternalServerError);
            Helpers.Logger.AddMessage($"Finished. Items: {items:N0}. Not found: {notFoundItems:N0}. Server errors: {serverErrorItems:N0}. Bad Gateway errors: {badGatewayItems:N0}");
        }

        public static async Task DownloadItems(List<YahooSymbolItem> symbolItems)
        {
            var tasks = new ConcurrentDictionary<YahooSymbolItem, Task<byte[]>>();
            var needToDownload = true;
            var cookies = YahooCommon.GetYahooCookies(true);
            while (needToDownload)
            {
                needToDownload = false;

                foreach (var symbolItem in symbolItems)
                {
                    if (!File.Exists(symbolItem.Filename))
                    {
                        if (symbolItem.StatusCode != HttpStatusCode.NotFound)
                        {
                            var task = WebClientExt.DownloadToBytesAsync(symbolItem.Url, false, false);
                            tasks[symbolItem] = task;
                            symbolItem.StatusCode = null;
                        }
                    }
                    else
                        symbolItem.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 1)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {symbolItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {symbolItems.Count:N0}");
                        tasks.Clear();
                        // cookies = YahooCommon.GetYahooCookies(true);

                        needToDownload = true;

                        await Task.Delay(350);
                    }
                }

                if (tasks.Count > 0)
                {
                    await Download(tasks);
                    tasks.Clear();
                    // cookies = YahooCommon.GetYahooCookies(true);
                    needToDownload = true;
                    await Task.Delay(350);
                }
                Helpers.Logger.AddMessage($"Downloaded {symbolItems.Count(a => a.StatusCode == HttpStatusCode.OK):N0} items from {symbolItems.Count:N0}");
            }
        }

        private static async Task Download(ConcurrentDictionary<YahooSymbolItem, Task<byte[]>> tasks)
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
                        $"YahooProfileLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }

        }


        public class YahooSymbolItem
        {
            public string Url => string.Format(UrlTemplate, YahooSymbol);
            public string PolygonSymbol;
            public string YahooSymbol;
            public string Filename => string.Format(FileNameTemplate, PolygonSymbol, Date.ToString("yyyyMMdd"));
            public DateTime Date;
            public HttpStatusCode? StatusCode;

            public YahooSymbolItem(string polygonSymbol, string yahooSymbol, DateTime dateKey)
            {
                PolygonSymbol = polygonSymbol;
                YahooSymbol = yahooSymbol;
                Date = dateKey;
                var dataFolder = Path.GetDirectoryName(Filename);
                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);
            }
        }

        private static List<YahooSymbolItem> GetSymbolsAndExchanges(DateTime dateKey)
        {
            var data = new List<YahooSymbolItem>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var item = new YahooSymbolItem((string)rdr["symbol"], (string)rdr["YahooSymbol"], dateKey);
                        if (!string.IsNullOrEmpty(item.YahooSymbol))
                            data.Add(item);
                    }
            }

            return data;
        }


    }
}
