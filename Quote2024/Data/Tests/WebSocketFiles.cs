using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Models;

namespace Data.Tests
{
    public class WebSocketFiles
    {
        private const string folder = @"E:\Quote\WebData\RealTime\WebSockets";
        public static void DecodePolygonRun()
        {
            // Result for Yahoo: min - 7 minutes 8 seconds
            var evs = new Dictionary<string, int>();
            var files = Directory.GetFiles(folder, "WebSocket_*.txt");
            foreach (var file in files)
            {
                // var items = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<oPolygon[]>(File.ReadAllBytes(file));
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var items = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<oPolygon[]>(line.Substring(13));
                    foreach (var item in items)
                    {
                        string key;
                        if (item.ev == "status")
                            key = item.status;
                        else
                            key = item.ev;
                        if (!evs.ContainsKey(key))
                            evs.Add(key, 0);
                        evs[key]++;
                        if (item.otc)
                        {

                        }
                    }
                }
            }

            foreach (var ev in evs)
                Debug.Print($"{ev.Key}\t{ev.Value}");
        }

        public static void YahooDelayRun()
        {
            // Result for Yahoo: 1% of item have >2 min delay, 10% of item have >22 seconds delay; average delay: 7-12 seconds (96-98 symbols)
            var diffValues = new Dictionary<double, int>();
            var files = Directory.GetFiles(folder, "*Yahoo*.txt");
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                // File creation time can be late for part of second (need to add 'AddSeconds(-10)' method)
                var fileDate = File.GetCreationTime(file).AddSeconds(-10).Date;
                var fileTime = File.GetCreationTime(file).AddSeconds(-10).TimeOfDay;
                var delayTotal = 0.0;
                var itemCount = 0;
                var symbols = new Dictionary<string, int>();
                foreach (var line in lines)
                {
                    itemCount++;
                    var data = PricingData.GetPricingData(line.Substring(13));
                    var dataDate = DateTimeOffset.FromUnixTimeMilliseconds(data.time / 2);
                    var etcDataDate = TimeZoneInfo.ConvertTimeFromUtc(dataDate.DateTime, Settings.NewYorkTimeZone);

                    var timeSpan = TimeSpan.ParseExact(line.Substring(0, 12), @"hh\:mm\:ss\.FFF", CultureInfo.InvariantCulture);
                    var recordDate = fileDate.Add(timeSpan);
                    if (timeSpan < fileTime)
                        recordDate = recordDate.AddDays(1);
                    var etcRecordDate = TimeZoneInfo.ConvertTimeFromUtc(recordDate.ToUniversalTime(), Settings.NewYorkTimeZone);

                    var difference = etcRecordDate - etcDataDate;
                    delayTotal += difference.TotalSeconds;
                    var diffValue = Math.Round(difference.TotalSeconds, 0);
                    if (diffValue >1000)
                    {

                    }
                    if (!diffValues.ContainsKey(diffValue))
                        diffValues.Add(diffValue, 0);
                    diffValues[diffValue]++;

                    if (!symbols.ContainsKey(data.id))
                        symbols.Add(data.id, 0);
                }
                Debug.Print($"{Path.GetFileName(file)}\t{itemCount}\t{delayTotal/itemCount}\t{symbols.Count}");
            }

            foreach (var key in diffValues.Keys.OrderBy(a=>a))
                Debug.Print($"{key}\t{diffValues[key]}");
        }

        public static void YahooTimeRangesRun()
        {
            // Result for Yahoo: min - 7 minutes 8 seconds
            var times = new List<(string, TimeSpan, TimeSpan, TimeSpan)>();
            var files = Directory.GetFiles(folder, "*.txt");
            foreach (var file in files)
            {
                var from = TimeSpan.Zero;
                var to = TimeSpan.Zero;

                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var timeSpan = TimeSpan.ParseExact(line.Substring(0, 12), @"hh\:mm\:ss\.FFF", CultureInfo.InvariantCulture);
                    if (from == TimeSpan.Zero)
                    {
                        from = timeSpan;
                        to = timeSpan;
                    }
                    else
                    {
                        if (timeSpan < to)
                            timeSpan = timeSpan.Add(new TimeSpan(1, 0, 0, 0, 0));

                        if ((timeSpan - to) < new TimeSpan(0, 1, 0))
                        {
                            to = timeSpan;
                        }
                        else
                        {
                            times.Add((Path.GetFileName(file), from, to, to - from));
                            from = timeSpan;
                            to = timeSpan;
                        }
                    }
                }

                if (from != TimeSpan.Zero)
                    times.Add((Path.GetFileName(file), from, to, to - from));
            }

            foreach (var item in times)
                Debug.Print($"{item.Item1}\t{item.Item2}\t{item.Item3}\t{item.Item4}");
        }

        public class oPolygon
        {
            public string ev;

            public string status;
            public string message;

            public string sym;
            public int v;
            public int av;
            public float op;
            public float vw;
            public float o;
            public float c;
            public float h;
            public float l;
            public float a;
            public double z;
            public long s;
            public long e;
            public bool otc;
        }
    }
}
