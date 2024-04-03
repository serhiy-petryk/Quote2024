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
    public class MinuteFinageRealTime
    {
        private const string UrlTemplate = @"https://api.finage.co.uk/agg/stock/{0}/1/minute/{1}/{1}?limit=50000&apikey={2}";
        private const string UrlTimerTemplate = @"https://api.finage.co.uk/agg/stock/{0}/1/minute/{1}/{1}?dbt_filter=true&st={3}&limit=1000&apikey={2}";
        private const string FileTemplate = @"{0}.json";

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, IEnumerable<string> tickers)
        {
            Logger.AddMessage($"Check Finage minute data", fnShowStatus);

            var dateKey = TimeHelper.GetCurrentEstDateTime().Date.ToString("yyyy-MM-dd");
            var apiKey = CsUtils.GetApiKeys("finage.co.uk")[0];

            var sw = new Stopwatch();
            sw.Start();

            var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
            foreach (var ticker in tickers)
            {
                var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker, dateKey, apiKey));
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
                $"Finage minute data checked. {invalidTickers.Count} invalid tickers, {validTickers.Count} valid tickers", fnShowStatus);

            return (validTickers, invalidTickers);
        }

        public static void SaveResult(Dictionary<string, byte[]> results, string dataFolder, DateTime methodStarted)
        {
            var filename = Path.Combine(dataFolder, $@"FinageMinutes_{DateTime.Now:yyyyMMddHHmmss}.zip");
            var virtualFileEntries = results.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
            ZipUtils.ZipVirtualFileEntries(filename, virtualFileEntries);
            File.SetCreationTime(filename, methodStarted);
        }

        public static async Task<Dictionary<string, byte[]>> Run(Action<string> fnShowStatus, string[] tickers, string dataFolder, Action<string, Exception> onError)
        {

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
                var task = Download.DownloadToBytesAsync(string.Format(UrlTimerTemplate, ticker, dateKey, apiKey, startTimeKey));
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
