using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.RealTime
{
    public class RealTimeYahooMinutes
    {
        // Url from finance.yahoo.com: https://query2.finance.yahoo.com/v8/finance/chart/AA?period1=1710964800&period2=1711483200&interval=1m&includePrePost=true&events=div%7Csplit%7Cearn&useYfid=true&lang=en-US&region=US
        // private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&events=history";
        private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&includePrePost=true&events=history";
        private const string FileTemplate = @"{0}.json";

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, string[] tickers)
        {
            Logger.AddMessage($"Check Yahoo minute data", fnShowStatus);
            var from = new DateTimeOffset(DateTime.UtcNow.AddMinutes(-30)).ToUnixTimeSeconds();
            var to = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            // Rounded to 1 minute
            from = from / 60 * 60;
            // to = to / 60 * 60;
            to = (to + 59) / 60 * 60; // + current minute

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

            // Rounded to 1 minute
            from = from / 60 * 60;
            // to = to / 60 * 60;
            to = (to + 59) / 60 * 60; // + current minute

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
                    var text = "{\"chart\":{\"error\":{\"code\":\"C#\",\"description\":\"{0}\"}}}".Replace(@"{0}", e.Message.Replace(@"""", @"\"""));
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
