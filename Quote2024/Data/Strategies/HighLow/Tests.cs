using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Strategies.HighLow
{
    public static class Tests
    {
        private static DateTime _fromDate = new DateTime(2024, 04, 2);
        private static DateTime _toDate = new DateTime(2024, 04, 4);

        public static void StartNbbo()
        {
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(16, 0, 0);
            var zipFileName = Settings.DataFolder + @"Trades\PolygonNbbo\Data\2024-04-05\PolygonNbbo_20240405_Z.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                var results = new List<PolygonNbboLoader.cResult>();
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = ZipUtils.DeserializeZipEntry<PolygonNbboLoader.cRoot>(entry);
                    results.AddRange(oo.results.Where(a=>a.SipTime>=startTime && a.SipTime<endTime));
                }

                results = results.OrderBy(a=>a.sip_timestamp).ToList();

                var aa1 = results.Where(a => a.bid_price >= a.ask_price).ToArray();
                var aa12 = results.Where(a => a.bid_price > a.ask_price).ToArray();
                var aa2 = results.Where(a => a.bid_size == 0 || a.ask_size == 0).ToArray();
                var aa3 = results.Where(a => Math.Abs(a.bid_price - a.ask_price) < 0.00005).ToArray();

                var delta = results.Average(a => a.ask_price - a.bid_price);
                var askSize = results.Average(a => a.ask_size);
                var bidSize = results.Average(a => a.bid_size);
                var askPrice = results.Average(a => a.ask_price);
                var bidPrice = results.Average(a => a.bid_price);
                var diffPrice = (askPrice - bidPrice) * 10000;
                var cnt = results.Count() / (endTime.TotalMinutes - startTime.TotalMinutes);

                var start2 = new TimeSpan(10, 0, 0);
                var end2 = new TimeSpan(11, 0, 0);
                var askSize2 = results.Where(a=>a.SipTime>=start2 && a.SipTime<end2).Average(a => a.ask_size);
                var bidSize2 = results.Where(a => a.SipTime >= start2 && a.SipTime < end2).Average(a => a.bid_size);
                var askPrice2 = results.Where(a => a.SipTime >= start2 && a.SipTime < end2).Average(a => a.ask_price);
                var bidPrice2 = results.Where(a => a.SipTime >= start2 && a.SipTime < end2).Average(a => a.bid_price);
                var diffPrice2 = (askPrice2 - bidPrice2) * 10000;
                var cnt2 = results.Count(a => a.SipTime >= start2 && a.SipTime < end2) / 59.0;

                var start3 = new TimeSpan(11, 5, 0);
                var end3 = new TimeSpan(11, 7, 0);
                var askSize3 = results.Where(a => a.SipTime >= start3 && a.SipTime < end3).Average(a => a.ask_size);
                var bidSize3 = results.Where(a => a.SipTime >= start3 && a.SipTime < end3).Average(a => a.bid_size);
                var askPrice3 = results.Where(a => a.SipTime >= start3 && a.SipTime < end3).Average(a => a.ask_price);
                var bidPrice3 = results.Where(a => a.SipTime >= start3 && a.SipTime < end3).Average(a => a.bid_price);
                var diffPrice3 = (askPrice3 - bidPrice3) * 10000;
                var cnt3 = results.Count(a => a.SipTime >= start3 && a.SipTime < end3) / 2.0;

                var aa5 = results.GroupBy(a => Convert.ToInt32(a.SipTime.TotalMinutes))
                    .ToDictionary(a => TimeSpan.FromMinutes(a.Key), a => a.Count());
                var aa51 = results.GroupBy(a => Convert.ToInt32(a.SipTime.TotalMinutes))
                    .ToDictionary(a => TimeSpan.FromMinutes(a.Key), a => a.Average(x => (x.ask_price - x.bid_price) * 10000));
                var aa52 = results.Where(a => Math.Abs(a.bid_price - a.ask_price) < 0.00005)
                    .GroupBy(a => Convert.ToInt32(a.SipTime.TotalMinutes)).ToDictionary(
                        a => TimeSpan.FromMinutes(a.Key), a => a.Count());

                Debug.Print("=======  RESULTS:  =======");
                Debug.Print("Time\tCount\tPriceDiff\tAsk=Bid");
                var aa = new Dictionary<TimeSpan, (int, float?, int)>();
                foreach (var kvp in aa5)
                {
                    var priceDiff = aa51.ContainsKey(kvp.Key) ? aa51[kvp.Key] : (float?)null;
                    var askBidEqualCount = aa52.ContainsKey(kvp.Key) ? aa52[kvp.Key] : 0;
                    aa.Add(kvp.Key, (kvp.Value, priceDiff, askBidEqualCount));
                    Debug.Print($"{kvp.Key}\t{kvp.Value}\t{priceDiff}\t{askBidEqualCount}");
                }
            }
        }

        public static void Start()
        {
            var date = new DateTime(2024, 04, 5);
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(15, 30, 0);
            var scanTime = new TimeSpan(0, 30, 0);

            var tickerAndDateAndHighToLow = Actions.Polygon.PolygonCommon.GetSymbolsAndHighToLowForStrategies(new[] { date });
            var symbols = tickerAndDateAndHighToLow.Keys.ToArray();
            // var symbols = new List<string>{"A", "AA", "AAAU"};

            var zipFileName = Settings.DataFolder + @"Minute\Polygon2003\Data\MP2003_20240406.zip";
            var fromTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(date + startTime);
            var toTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(date + endTime);
            var quotes = new Dictionary<string, PolygonCommon.cMinuteItem[]>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0 && symbols.Contains(Path.GetFileNameWithoutExtension(a.Name).Split('_')[1], StringComparer.OrdinalIgnoreCase)))
                {
                    var symbol = Path.GetFileNameWithoutExtension(entry.Name).Split('_')[1];
                    var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                    quotes.Add(symbol, oo.results.Where(a => a.IsValid && a.t >= fromTicks && a.t < toTicks).ToArray());
                }

            var data = new Dictionary<(string, TimeSpan), (float,float,float)>();
            foreach (var kvp in quotes)
            {
                var time = startTime;
                var to = endTime - scanTime;
                while (time < to)
                {
                    var ticks1 = TimeHelper.GetUnixMillisecondsFromEstDateTime(date + time);
                    var ticks2 = TimeHelper.GetUnixMillisecondsFromEstDateTime(date + time + scanTime);
                    var high = kvp.Value.Where(a => a.t >= ticks1 && a.t < ticks2).Max(a => a.h);
                    var low = kvp.Value.Where(a => a.t >= ticks1 && a.t < ticks2).Min(a => a.l);
                    var count = kvp.Value.Count(a => a.t >= ticks1 && a.t < ticks2);
                    var highToLow = Convert.ToSingle(Math.Round((high - low) / (high + low) * 200f, 2));
                    var prevHighToLow = tickerAndDateAndHighToLow[kvp.Key][date];
                    if (low > 5.0f && high < 5000.0f && count >= 20 && highToLow/prevHighToLow > 1.0f)
                    {
                        var lastClose = kvp.Value.Last(a => a.t >= ticks1 && a.t < ticks2).c;
                        var level = (lastClose - low) / (high - low) * 100f;
                        data.Add((kvp.Key, time + scanTime), (highToLow, level, highToLow / prevHighToLow));
                    }

                    time = time.Add(TimeSpan.FromMinutes(1));
                }
            }

            var aa2 = data.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa3 = data.OrderBy(a=>a.Key.Item2).ThenBy(a=>a.Key.Item1).ToDictionary(a => a.Key, a => a.Value);

            Debug.Print("RESULTS:");
            Debug.Print("========");
            Debug.Print($"Time\tTicker\tHighToLow\tLevel\tK_HighToLow");
            foreach (var kvp in aa3)
            {
                Debug.Print($"{kvp.Key.Item2:hh\\:mm}\t{kvp.Key.Item1}\t{kvp.Value.Item1}\t{kvp.Value.Item2}\t{kvp.Value.Item3}");
            }
        }
    }
}
