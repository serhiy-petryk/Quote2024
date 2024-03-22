using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Data.Actions.Polygon;
using Data.Actions.Yahoo;
using Data.Helpers;

namespace Data.RealTime
{
    public class YahooMinutes
    {
        // private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&events=history";
        private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&includePrePost=true&events=history";
        private const string FileTemplate = @"{0}.json";
        private const string DayPolygonDataFolder = @"E:\Quote\WebData\RealTime\YahooMinute\DayPolygon";

        public static List<string> GetTickerList(Action<string> fnShowStatus, int tradingDays, float minTradeValue,
            float maxTradeValue, int minTradeCount, float minClose, float maxClose)
        {
            Logger.AddMessage($"Get ticker list", fnShowStatus);

            var dates = Actions.Yahoo.YahooIndicesLoader.GetTradingDays(
                TimeHelper.GetCurrentEstDateTime().Date.AddDays(-1), tradingDays * 2 + 10);
            var sqls = new string[tradingDays];
            var cnt = 0;
            var data = new List<PolygonDailyLoader.cItem>();
            for (var k = 0; k < tradingDays; k++)
            {
                var date = dates[k];
                var zipFileName = Path.Combine(DayPolygonDataFolder, $"DayPolygon_{date:yyyyMMdd}.zip");
                if (!File.Exists(zipFileName))
                {
                    var url =
                        $@"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date:yyyy-MM-dd}?adjusted=false&apiKey={PolygonCommon.GetApiKey2003()}";
                    var o = Download.DownloadToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception(
                            $"PolygonDailyLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    var jsonFileName = $"DayPolygon_{date:yyyyMMdd}.json";
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(jsonFileName, o.Item1) });
                }

                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var content = zip.Entries[0].GetContentOfZipEntry().Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
                    var oo = ZipUtils.DeserializeString<PolygonDailyLoader.cRoot>(content);

                    if (oo.status != "OK" || oo.count != oo.queryCount || oo.count != oo.resultsCount || oo.adjusted || oo.resultsCount == 0)
                        throw new Exception($"Bad file: {zipFileName}");

                    var minTradeValue2 = (Settings.IsInMarketTime(date) ? minTradeValue / 2.0f : minTradeValue) * 1000000f;
                    var maxTradeValue2 = (Settings.IsInMarketTime(date) ? maxTradeValue / 2.0f : maxTradeValue) * 1000000f;
                    var minTradeCount2 = Settings.IsInMarketTime(date) ? minTradeCount / 2.0 : minTradeCount;

                    data.AddRange(oo.results.Where(a => a.c >= minClose && a.c <= maxClose && a.n >= minTradeCount && a.c * a.v >= minTradeValue2 && a.c * a.v <= maxTradeValue2));
                }
            }

            var tickers = data.GroupBy(a => a.TT).Where(a => a.Count() == tradingDays)
                .Select(a => YahooCommon.GetYahooTickerFromPolygonTicker(a.Key)).ToList();

            return tickers;
        }

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, string[] tickers)
        {
            Logger.AddMessage($"Check Yahoo minute data", fnShowStatus);
            var from = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-30)).ToUnixTimeSeconds();
            var to = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            var urlsAndFileNames =
                tickers.Select(s => (string.Format(UrlTemplate, s, from, to), string.Format(FileTemplate, s)));

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker, from, to));
                tasks[ticker] = task;
            }

            var validTickers = new Dictionary<string, byte[]>();
            var invalidTickers = new Dictionary<string, Exception>();
            foreach (var kvp in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    validTickers.Add(kvp.Key, await kvp.Value);
                }
                catch (Exception ex)
                {
                    invalidTickers.Add(kvp.Key, ex);
                    Debug.Print($"{DateTime.Now.TimeOfDay}. Ticker: {kvp.Key}. Error: {ex.Message}");
                }
            }

            sw.Stop();
            Debug.Print($"SW: {sw.ElapsedMilliseconds}. From: {from}. To: {to}");

            Logger.AddMessage(
                $"Yahoo minute data checked. {invalidTickers.Count} invalid tickers, {validTickers.Count} valid tickers", fnShowStatus);

            return (validTickers, invalidTickers);
        }

        public static async Task<Dictionary<string, byte[]>> Run(Action<string> fnShowStatus, string[] symbols, string dataFolder, Action<string, Exception> onError)
        {
            const int minutes = 30;
            Logger.AddMessage($"Download Yahoo minute data from {DateTime.UtcNow.AddMinutes(-minutes):HH:mm:ss} to {DateTime.UtcNow:HH:mm:ss} UTC", fnShowStatus);

            var utcNow = DateTime.UtcNow;
            var from = new DateTimeOffset(utcNow.AddMinutes(-minutes)).ToUnixTimeSeconds();
            var to = new DateTimeOffset(utcNow).ToUnixTimeSeconds();

            var urlsAndFileNames =
                symbols.Select(s => (string.Format(UrlTemplate, s, from, to), string.Format(FileTemplate, s)));

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var symbol in symbols)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, symbol, from, to));
                tasks[symbol] = task;
            }

            // can't process error: await Task.WhenAll(tasks.Values);
            var results = new Dictionary<string, byte[]>();
            foreach (var kvp in tasks)
            {
                try
                {
                    // release control to the caller until the task is done, which will be near immediate for each task following the first
                    results.Add(kvp.Key, await kvp.Value);
                }
                catch (Exception e)
                {
                    var text = @"{""chart"":{""error"":""{0}""}}".Replace(@"{0}", e.Message.Replace(@"""", @"\"""));
                    results.Add(kvp.Key, System.Text.Encoding.UTF8.GetBytes(text));
                    onError?.Invoke(kvp.Key, e);
                }
            }

            sw.Stop();
            Logger.AddMessage(
                $"Yahoo minute data from {utcNow.AddMinutes(-minutes):HH:mm:ss} to {utcNow:HH:mm:ss} UTC was downloaded. Duration: {sw.ElapsedMilliseconds:N0} milliseconds",
                fnShowStatus);

            if (dataFolder != null)
                SaveResult(results, dataFolder);

            return results;
        }

        public static void SaveResult(Dictionary<string, byte[]> results, string dataFolder)
        {
            var virtualFileEntries = results.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
            ZipUtils.ZipVirtualFileEntries(Path.Combine(dataFolder, $@"RTYahooMinutes_{DateTime.Now:yyyyMMddHHmmss}.zip"), virtualFileEntries);
        }
    }
}
