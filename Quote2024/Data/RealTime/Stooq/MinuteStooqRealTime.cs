using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.RealTime.Stooq
{
    public class MinuteStooqRealTime
    {
        private const string UrlTemplate = @"https://stooq.com/q/a2/d/?s={0}&i=1";

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, IEnumerable<string> tickers)
        {
            Logger.AddMessage($"Check Stooq minute data", fnShowStatus);

            var dateKey = TimeHelper.GetCurrentEstDateTime().Date.ToString("yyyy-MM-dd");

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var urlTicker = ticker.StartsWith('^') ? ticker : ticker + ".us";
                // var aa = Download.DownloadToBytes(string.Format(UrlTemplate, urlTicker.ToLower()), false);
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, urlTicker.ToLower()));
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
                    if (validTickers[kvp.Key].Length == 0)
                    {
                        var urlTicker = kvp.Key.StartsWith('^') ? kvp.Key : kvp.Key + ".us";
                        // var aa = await Download.DownloadToBytesAsync(string.Format(UrlTemplate, urlTicker.ToLower()));
                        validTickers[kvp.Key] = await Download.DownloadToBytesAsync(string.Format(UrlTemplate, urlTicker.ToLower()));
                    }
                }
                catch (Exception ex)
                {
                    invalidTickers.Add(kvp.Key, ex);
                    Debug.Print($"{DateTime.Now.TimeOfDay}. Ticker: {kvp.Key}. Error: {ex.Message}");
                }
            }

            sw.Stop();
            Debug.Print($"SW: {sw.ElapsedMilliseconds}. Date: {dateKey}");

            Logger.AddMessage(
                $"Stooq minute data checked. {invalidTickers.Count} invalid tickers, {validTickers.Count} valid tickers", fnShowStatus);

            return (validTickers, invalidTickers);
        }

        public static void SaveResult(Dictionary<string, byte[]> results, string dataFolder, DateTime methodStarted)
        {
            var filename = Path.Combine(dataFolder, $@"StooqMinutes_{DateTime.Now:yyyyMMddHHmmss}.zip");
            var virtualFileEntries = results.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
            ZipUtils.ZipVirtualFileEntries(filename, virtualFileEntries);
            File.SetCreationTime(filename, methodStarted);
        }

        public static async Task<Dictionary<string, byte[]>> Run(Action<string> fnShowStatus, string[] tickers, string dataFolder, Action<string, Exception> onError)
        {
            throw new Exception("Not ready!!!");
            Logger.AddMessage($"Download Finage minute data", fnShowStatus);

            var startTimeKey = DateTime.Now.ToUniversalTime().AddMinutes(-5).TimeOfDay.ToString(@"hh\:mm");
            var nyTime = TimeHelper.GetCurrentEstDateTime();
            var dateKey = nyTime.Date.ToString("yyyy-MM-dd");
            var apiKey = CsUtils.GetApiKeys("finage.co.uk")[0];

            var methodStarted = DateTime.Now;

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker));
                tasks[ticker] = task;
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
                $"Finage minute data for {dateKey} was downloaded. NY time: {nyTime:HH:mm:ss} Duration: {sw.ElapsedMilliseconds:N0} milliseconds",
                fnShowStatus);

            if (dataFolder != null)
                SaveResult(results, dataFolder, methodStarted);

            return results;
        }

    }
}
