using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Tests
{
    public class WebSocketFiles
    {
        private const string folderOld = @"E:\Quote\WebData\RealTime\WebSockets";
        private const string folder = @"E:\Quote\WebData\RealTime\YahooSocket\Data\2024-04-08";

        public class DelayStat
        {
            public string Id;
            public string Type;
            public int MessageCount;
            public long TotalDelayMsecs;
            public int MinDelayMsecs;
            public int MaxDelayMsecs;
            public int BadRecordDayCount;
            public int MaxBadRecordMsecs;

            public DelayStat(string id, string type)
            {
                Id = id;
                Type = type;
            }

            public void AddMessageData(DateTime etcDataDate, DateTime etcRecordDate)
            {
                MessageCount++;
                var delayMsecs = Convert.ToInt32((etcRecordDate - etcDataDate).TotalMilliseconds);
                TotalDelayMsecs += delayMsecs;
                if (delayMsecs < MinDelayMsecs) MinDelayMsecs = delayMsecs;
                if (delayMsecs > MaxDelayMsecs) MaxDelayMsecs = delayMsecs;
                if (delayMsecs < 0)
                {
                    BadRecordDayCount++;
                    if (-delayMsecs > MaxBadRecordMsecs) MaxBadRecordMsecs = -delayMsecs;
                }
            }

            // public override string ToString() =>
               // $"{Type}\t{Id}\t{MessageCount}\t{Math.Round(0.001 * TotalDelayMsecs / MessageCount, 1)}\t{BadRecordDayCount}\t{Math.Round(0.001 * MaxBadRecordMsecs, 1)}\t{Math.Round(0.001 * MinDelayMsecs, 1)}\t{Math.Round(0.001 * MaxDelayMsecs, 1)}";
            public override string ToString() =>
                $"{Type}\t{Id}\t{MessageCount}\t{Math.Round(0.001 * TotalDelayMsecs / MessageCount, 1)}\t{BadRecordDayCount}\t{Math.Round(0.001 * MinDelayMsecs, 2)}\t{Math.Round(0.001 * MaxDelayMsecs, 1)}";
        }

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

        public static void YahooCheckSequence()
        {
            // Result is ~ 0.05% bad rows. Average time difference is ~ 2.0-3.5 secs
            var files = Directory.GetFiles(folder, "WebSocketYahoo_*.txt");
            var cnt = 0;
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var fileDateTime = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var fileDate = fileDateTime.Date;
                var fileTime = fileDateTime.TimeOfDay;

                var a1 = new DateTimeOffset(fileDateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds();
                var estFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(a1);
                var estTimeOffset = fileDateTime - estFileDateTime;

                var sequences = new Dictionary<string, long>();
                var allCount = 0;
                var badCount = 0;
                var totalDifference = 0.0;
                foreach (var line in lines)
                {
                    cnt++;
                    var data = PricingData.GetPricingData(line.Substring(13));
                    if (data.marketHours != PricingData.MarketHoursType.REGULAR_MARKET) continue;

                    allCount++;
                    if (!sequences.ContainsKey(data.id))
                        sequences.Add(data.id, data.time);
                    else
                    {
                        if (data.time < sequences[data.id])
                        {
                            // var a2 = sequences[data.id] - data.time;
                            var date1 = TimeHelper.GetEstDateTimeFromUnixMilliseconds(data.time / 2);
                            var date2 = TimeHelper.GetEstDateTimeFromUnixMilliseconds(sequences[data.id] / 2);
                            badCount++;
                            totalDifference += (date2 - date1).TotalMilliseconds;
                        }
                        else
                            sequences[data.id] = data.time;
                    }
                }

                if (badCount == 0)
                    Debug.Print($"{Path.GetFileName(file)}\t{allCount}\t{badCount}");
                else
                    Debug.Print($"{Path.GetFileName(file)}\t{allCount}\t{badCount}\t{Convert.ToInt32(totalDifference / badCount)}");
            }
        }

        public static void YahooDelayRun()
        {
            // Result for Yahoo (9 biggest (by TradeValue) symbols): 0.6% of item have > 10 seconds delay; average delay: 2 seconds 
            // Result for Yahoo (~1000 symbols: average delay is ~10-11 seconds; 25% of item have < 5 seconds delay; 50% - < 10 seconds delay, 75% - < 20 seconds delay; ~ 20 tickers have > 100 seconds delays
            //var files = Directory.GetFiles(folder, "YSocket_*.txt")
            //  .OrderBy(a => int.Parse(Path.GetFileNameWithoutExtension(a).Split('_')[1])).ToArray();
            var files = Directory.GetFiles(folder, "YSocket_*.txt").OrderBy(a => a).ToArray();
            var cnt = 0;
            var folderStat = new DelayStat(Path.GetFileName(folder), "Folder");
            var tickerCount = 0;
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var fileDateTime = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var fileDate = fileDateTime.Date;
                var fileTime = fileDateTime.TimeOfDay;

                var a1 = new DateTimeOffset(fileDateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds();
                var estFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(a1);
                var estTimeOffset = fileDateTime - estFileDateTime;

                var fileStat = new DelayStat(Path.GetFileName(file), "File");
                var tickerStats = new Dictionary<string, DelayStat>();
                foreach (var line in lines)
                {
                    cnt++;
                    var data = PricingData.GetPricingData(line.Substring(13));
                    if (data.marketHours != PricingData.MarketHoursType.REGULAR_MARKET) continue;

                    var etcDataDate = TimeHelper.GetEstDateTimeFromUnixMilliseconds(data.time / 2);
                    var recordTime = TimeSpan.ParseExact(line.Substring(0, 12), @"hh\:mm\:ss\.FFF", CultureInfo.InvariantCulture);
                    var recordDate = ((recordTime < fileTime ? fileDate.AddDays(1) : fileDate) + recordTime);
                    var etcRecordDate = recordDate.Subtract(estTimeOffset);

                    if (data.marketHours == PricingData.MarketHoursType.REGULAR_MARKET &&
                        etcRecordDate.TimeOfDay >= Settings.MarketEndCommon)
                        continue;

                    /*if (etcRecordDate.TimeOfDay < new TimeSpan(13, 0, 0) ||
                        etcRecordDate.TimeOfDay > new TimeSpan(15, 0, 0))
                        continue;*/

                    folderStat.AddMessageData(etcDataDate, etcRecordDate);
                    fileStat.AddMessageData(etcDataDate, etcRecordDate);
                    var ticker = data.id;
                    if (!tickerStats.ContainsKey(ticker))
                        tickerStats.Add(ticker, new DelayStat(ticker, "Ticker"));
                    tickerStats[ticker].AddMessageData(etcDataDate, etcRecordDate);
                }

                tickerCount += tickerStats.Count;

                Debug.Print(fileStat.ToString()+$"\t{tickerStats.Count}");
                foreach(var item in tickerStats)
                    Debug.Print(item.Value.ToString());
            }

            Debug.Print(folderStat.ToString() + $"\t{tickerCount}");
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
