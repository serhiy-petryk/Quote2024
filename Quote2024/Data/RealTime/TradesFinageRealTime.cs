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
    public class TradesFinageRealTime
    {
        private const string UrlTemplate2 = @"https://api.finage.co.uk/ticks/trade/stock/{0}?t={3}&date={1}&limit=30000&apikey={2}";
        private const string UrlTemplate = @"https://api.finage.co.uk/ticks/trade/stock/{0}?t={1}&limit=30000&apikey={2}";
        private const string FileTemplate = @"{0}.json";

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, IEnumerable<string> tickers)
        {
            Logger.AddMessage($"Check Finage trades data", fnShowStatus);
            var dateKey = TimeHelper.GetCurrentEstDateTime().Date.ToString("yyyy-MM-dd");
            var fromTimestamp = (new DateTimeOffset(DateTime.UtcNow.AddMinutes(-3), TimeSpan.Zero))
                .ToUnixTimeMilliseconds().ToString() + "000000";
            var apiKey = CsUtils.GetApiKeys("finage.co.uk")[0];

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker, fromTimestamp, apiKey, fromTimestamp));
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
            Debug.Print($"SW: {sw.ElapsedMilliseconds}. Date: {dateKey}");

            Logger.AddMessage(
                $"Finage trades data checked. {invalidTickers.Count} invalid tickers, {validTickers.Count} valid tickers", fnShowStatus);

            return (validTickers, invalidTickers);
        }

        public static async Task<Dictionary<string, byte[]>> Run(Action<string> fnShowStatus, string[] tickers, string dataFolder, Action<string, Exception> onError)
        {
            Logger.AddMessage($"Download Finage trades data", fnShowStatus);

            var dateKey = TimeHelper.GetCurrentEstDateTime().Date.ToString("yyyy-MM-dd");
            var fromTimestamp = (new DateTimeOffset(DateTime.UtcNow.AddMinutes(-3), TimeSpan.Zero))
                .ToUnixTimeMilliseconds().ToString() + "000000";
            var apiKey = CsUtils.GetApiKeys("finage.co.uk")[0];

            var methodStarted = DateTime.Now;

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker, fromTimestamp, apiKey));
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
                    var text = "{\"error\": \"{0}\", \"symbol\":\"{1}\"}".Replace(@"{0}", e.Message.Replace(@"""", @"\"""))
                        .Replace("{1}", kvp.Key);
                    results.Add(kvp.Key, System.Text.Encoding.UTF8.GetBytes(text));
                    onError?.Invoke(kvp.Key, e);
                }
            }

            sw.Stop();
            Logger.AddMessage(
                $"Finage trades data for {dateKey} was downloaded. Duration: {sw.ElapsedMilliseconds:N0} milliseconds", fnShowStatus);

            if (dataFolder != null)
                SaveResult(results, dataFolder, methodStarted);

            return results;
        }

        public static void SaveResult(Dictionary<string, byte[]> results, string dataFolder, DateTime methodStarted)
        {
            var filename = Path.Combine(dataFolder, $@"FinageTrades_{DateTime.Now:yyyyMMddHHmmss}.zip");
            var virtualFileEntries = results.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
            ZipUtils.ZipVirtualFileEntries(filename, virtualFileEntries);
            File.SetCreationTime(filename, methodStarted);
        }

        public static async Task DownloadAllData(Action<string> fnShowStatus, string[] tickers)
        {
            throw new Exception("Not ready!");
        }

    }
}
