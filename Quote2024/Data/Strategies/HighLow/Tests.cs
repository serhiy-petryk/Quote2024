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

        public static void Start()
        {
            var date = new DateTime(2024, 04, 5);
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(15, 30, 0);
            var scanTime = new TimeSpan(0, 30, 0);

            var symbols = Actions.Polygon.PolygonCommon.GetSymbolsForStrategies(date);
            // var symbols = new List<string>{"A", "AA", "AAAU"};

            var zipFileName = @"E:\Quote\WebData\Minute\Polygon2003\Data\MP2003_20240406.zip";
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

            var data = new Dictionary<(string, TimeSpan), (float,float)>();
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
                    if (low > 5.0f && high < 5000.0f && count >= 20 && highToLow > 7.0f)
                    {
                        var lastClose = kvp.Value.Last(a => a.t >= ticks1 && a.t < ticks2).c;
                        var level = (lastClose - low) / (high - low) * 100f;
                        data.Add((kvp.Key, time + scanTime), (highToLow, level));
                    }

                    time = time.Add(TimeSpan.FromMinutes(1));
                }
            }

            var aa2 = data.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa3 = data.OrderBy(a=>a.Key.Item2).ThenBy(a=>a.Key.Item1).ToDictionary(a => a.Key, a => a.Value);

            Debug.Print("RESULTS:");
            Debug.Print("========");
            Debug.Print($"Time\tTicker\tHighToLow\tLevel");
            foreach (var kvp in aa3)
            {
                Debug.Print($"{kvp.Key.Item2:hh\\:mm}\t{kvp.Key.Item1}\t{kvp.Value.Item1}\t{kvp.Value.Item2}");
            }
        }
    }
}
