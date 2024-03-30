using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.RealTime
{
    public class TimeSalesNasdaq
    {
        private const string UrlTemplate = @"https://api.nasdaq.com/api/quote/{0}/realtime-trades?&limit=200000&fromTime={1}";
        private const string FileTemplate = @"{0}.json";

        private static TimeSpan TSInterval = TimeSpan.FromMilliseconds(4800);
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
                .GroupBy(x => x.Index / 170)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToArray();

            var validTickers = new Dictionary<string, byte[]>();
            var invalidTickers = new Dictionary<string, Exception>();
            for (var k = 0; k < tickers2.Length; k++)
            {
                var dt1 = DateTime.Now.TimeOfDay;
                Debug.Print($"TSNasdaq Started. Time: {dt1}. K={k}");
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

                var dt2 = DateTime.Now.TimeOfDay;
                Debug.Print($"TSNasdaq Ended. Time: {dt2}. K={k}");

                if (k == tickers2.Length - 1)
                    break;

                var a1 = TSInterval - (dt2 - dt1);
                if (a1 > TimeSpan.Zero)
                    await Task.Delay(Convert.ToInt32(a1.TotalMilliseconds));
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

        public static async Task DownloadAllData(Action<string> fnShowStatus, string[] tickers)
        {
            Logger.AddMessage($"Download all Nasdaq timesales data", fnShowStatus);

            var downloadInterval = TimeSpan.FromMilliseconds(8500);
            var fromKeys = new string[]
            {
                "09:30", "10:00", "10:30", "11:00", "11:30", "12:00", "12:30", "13:00", "13:30", "14:00", "14:30",
                "15:00", "15:30"
            };

            const string baseFolder = @"D:\Quote\WebData\Minute\Nasdaq2";
            var dateKey = TimeHelper.GetCurrentEstDateTime();
            var dataFolder = Path.Combine(baseFolder, dateKey.ToString("yyyy-MM-dd"));
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var validCount = 0;
            var invalidCount = 0;
            var totalCount = tickers.Length * fromKeys.Length;
            var groupedTickers = tickers.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 180)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToArray(); 
            foreach (var fromKey in fromKeys)
            {
                var zipFileName = Path.Combine(dataFolder, $"TSAllNasdaq_{dateKey:yyyyMMdd}{fromKey.Replace(":", "")}.zip");
                if (!File.Exists(zipFileName))
                {
                    var validTickers = new Dictionary<string, byte[]>();
                    var invalidTickers = new Dictionary<string, Exception>();
                    for (var k = 0; k < groupedTickers.Length; k++)
                    {
                        var tickerGroup = groupedTickers[k];
                        var dt1 = DateTime.Now.TimeOfDay;
                        Debug.Print($"TSAllNasdaq Started. Time: {dt1}. K={k}");
                        var tasks = new ConcurrentDictionary<string, Task<byte[]>>();
                        foreach (var ticker in tickerGroup)
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
                                validCount++;
                            }
                            catch (Exception ex)
                            {
                                invalidTickers.Add(kvp.Key, ex);
                                Debug.Print($"{DateTime.Now.TimeOfDay}. FromKey: {fromKey}. Ticker: {kvp.Key}. Error: {ex.Message}");
                                invalidCount++;
                                if (ex is System.Net.WebException webEx &&
                                    webEx.Response is System.Net.HttpWebResponse webResponse &&
                                    webResponse.StatusCode == HttpStatusCode.Forbidden)
                                {
                                    Logger.AddMessage($"Forbidden error!!! All Nasdaq timesales data. {invalidCount} invalid items, {validCount} valid items", fnShowStatus);
                                    return;
                                }
                            }
                        }

                        var dt2 = DateTime.Now.TimeOfDay;
                        Debug.Print($"TSAllNasdaq ended. Time: {dt2}. K={k}");
                        Logger.AddMessage($"Downloaded {validCount:N0} from {totalCount:N0}. From key: {fromKey}", fnShowStatus);

                        var a1 = downloadInterval - (dt2 - dt1);
                        if (a1>TimeSpan.Zero)
                            await Task.Delay(Convert.ToInt32(a1.TotalMilliseconds));
                        //if (a1<TimeSpan.FromMilliseconds(1000)) a1= TimeSpan.FromMilliseconds(1000);
                        //await Task.Delay(Convert.ToInt32(a1.TotalMilliseconds));
                        // await Task.Delay(downloadInterval);
                    }

                    var virtualFileEntries = validTickers.Select(kvp => new VirtualFileEntry($"{kvp.Key}.json", kvp.Value));
                    ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
                }
            }

            Logger.AddMessage($"All Nasdaq timesales data finished. {invalidCount} invalid items, {validCount} valid items", fnShowStatus);
        }

    }
}
