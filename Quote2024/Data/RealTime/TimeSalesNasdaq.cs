using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.RealTime
{
    public class TimeSalesNasdaq
    {
        private const string UrlTemplate = @"https://api.nasdaq.com/api/quote/{0}/realtime-trades?&limit=200000&fromTime={1}";
        private const string FileTemplate = @"{0}.json";

        public static async Task<(Dictionary<string, byte[]>, Dictionary<string, Exception>)> CheckTickers(Action<string> fnShowStatus, string[] tickers)
        {
            Logger.AddMessage($"Check Nasdaq timesales data", fnShowStatus);
            var from = TimeHelper.GetCurrentEstDateTime().TimeOfDay;
            from = new TimeSpan(0, Convert.ToInt32(from.TotalMinutes) / 30 * 30, 0);
            // var fromKey = from.ToString(@"hh\:mm");
            var fromKey = "15:30";

            var sw = new Stopwatch();
            sw.Start();

            var tickers2 = tickers.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 24)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToArray();

            var validTickers = new Dictionary<string, byte[]>();
            var invalidTickers = new Dictionary<string, Exception>();
            for (var k = 0; k < tickers2.Length; k++)
            {
                var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
                foreach (var ticker in tickers2[k])
                {
                    var task = Download.DownloadToBytesAsync(string.Format(UrlTemplate, ticker, fromKey), true, true);
                    tasks[ticker] = task;
                }

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

                if (k == tickers2.Length - 1)
                    break;
                Thread.Sleep(20);
            }

            sw.Stop();
            Debug.Print($"TimeSalesNasdaq.CheckTickers. {sw.ElapsedMilliseconds:N0} milliseconds. From: {fromKey}");

            Logger.AddMessage(
                $"Nasdaq timesales data checked. {invalidTickers.Count} invalid tickers, {validTickers.Count} valid tickers", fnShowStatus);

            return (validTickers, invalidTickers);
        }

        public static async Task<Dictionary<string, byte[]>> Run(Action<string> fnShowStatus, string[] symbols, string dataFolder, Action<string, Exception> onError)
        {
            fnShowStatus("!!! Function not ready!!!");
            return new Dictionary<string, byte[]>();
        }

        public static void SaveResult(Dictionary<string, byte[]> results, string dataFolder)
        {
            var virtualFileEntries = results.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
            ZipUtils.ZipVirtualFileEntries(Path.Combine(dataFolder, $@"TSNasdaq_{DateTime.Now:yyyyMMddHHmmss}.zip"), virtualFileEntries);
        }

    }
}
