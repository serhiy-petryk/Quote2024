using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Strategies
{
    public static class BigTickWithBigTradeCount
    {
        public static void StartDown()
        {
            var date = new DateTime(2024, 04, 5);
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(15, 30, 0);
            var beforeTickCount = 10;
            var beforePriceRangePercent = 1f;
            var highToLowPercent = 0.5f;

            Logger.AddMessage($"Select tickers");
            var zipFileName = Settings.DataFolder + @"Minute\Polygon2003\Data\MP2003_20240406.zip";
            Dictionary<string, Dictionary<DateTime, float>> tickerAndDateAndHighToLow;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                var entry = zip.Entries.First(a => a.Name.Contains("_AA_"));
                var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                tickerAndDateAndHighToLow = PolygonCommon.GetSymbolsAndHighToLowForStrategies(oo.results.Select(a => a.DateTime.Date)
                    .Distinct().OrderBy(a => a).ToArray());
            }

            Logger.AddMessage($"Calculate data");
            Debug.Print($"BeforeTicks: {beforeTickCount}\tPrevPriceRange,%: {beforePriceRangePercent}\tHighToLow,%: {highToLowPercent}");
            Debug.Print("Ticker\tDateTime\tDailyHighToLow\tGap\tGap,%\tHighLow,%\t K trades\tPrevClose\tOpen\tHigh\tLow\tClose\tTrades\tProfit,%");
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a =>
                             a.Length > 0 &&
                             tickerAndDateAndHighToLow.ContainsKey(
                                 Path.GetFileNameWithoutExtension(a.Name).Split('_')[1])))
                {
                    var ticker = Path.GetFileNameWithoutExtension(entry.Name).Split('_')[1];
                    var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                    foreach (var kvp in tickerAndDateAndHighToLow[ticker])
                    {
                        var fromTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(kvp.Key + startTime);
                        var toTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(kvp.Key + endTime);
                        var data = oo.results.Where(a => a.t >= fromTicks && a.t <= toTicks).OrderBy(a => a.t).ToArray();
                        for (var k = beforeTickCount; k < (data.Length - 1); k++)
                        {
                            var item = data[k];
                            var minutesBefore = item.DateTime.TimeOfDay.TotalMinutes - data[k - beforeTickCount].DateTime.TimeOfDay.TotalMinutes;
                            if ((beforeTickCount - minutesBefore) > 0.5) continue; // there are missing ticks
                            if (item.l < 5f) continue; // tick price < $5.0
                            if (item.o < (item.h + item.l) / 2 || item.c > (item.h + item.l) / 2) continue; // not down tick

                            var currentHighToLowPercent = (item.h - item.l) / (item.h + item.l) * 200f;
                            if (currentHighToLowPercent < highToLowPercent) continue; // high-low < 1%
                            if ((item.o - item.c) < (item.h - item.l) / 2) continue; // small close-open

                            var diffOpenClose = Math.Abs(item.o - data[k - 1].c);
                            var diffOpenClosePercent = diffOpenClose / (item.o + data[k - 1].c) * 200f;
                            if (!(diffOpenClose < 0.015 || diffOpenClosePercent < 0.1)) continue; // big split between close of previous minute and open

                            var closeOpenDiff = 0f;
                            var beforeHigh = float.MinValue;
                            var beforeLow = float.MaxValue;
                            var beforeTrades = 0;
                            for (var k1 = 1; k1 <= beforeTickCount; k1++)
                            {
                                if (k1 != beforeTickCount)
                                    closeOpenDiff += Convert.ToSingle(Math.Round(Math.Abs(data[k - k1].o - data[k - k1 - 1].c), 4));
                                if (data[k - k1].h > beforeHigh) beforeHigh = data[k - k1].h;
                                if (data[k - k1].h < beforeLow) beforeLow = data[k - k1].l;
                                beforeTrades += data[k - k1].n;
                            }

                            if (5.0 * beforeTrades / beforeTickCount > item.n) continue; // no big trade count
                            if (!(closeOpenDiff / beforeTickCount < 0.015 || closeOpenDiff / beforeTickCount / item.o * 200 < 0.1))
                                continue; // big split between close and open

                            if ((beforeHigh - beforeLow) / (beforeHigh + beforeLow) * 200 > beforePriceRangePercent) continue; // big price range before main tick

                            var nextTick = data[k + 1];
                            Debug.Print($"{ticker}\t{item.DateTime}\t{kvp.Value}\t{closeOpenDiff/beforeTickCount}\t{closeOpenDiff / beforeTickCount / item.o * 100}\t{currentHighToLowPercent}\t{item.n / (1.0 * beforeTrades / beforeTickCount) }\t{item.c}\t{nextTick.o}\t{nextTick.h}\t{nextTick.l}\t{nextTick.c}\t{nextTick.n}\t{(nextTick.o - nextTick.c) / nextTick.o * 100}");
                        }

                    }
                }

            Logger.AddMessage($"Finished");
        }

        public static void StartUp()
        {
            var date = new DateTime(2024, 04, 5);
            var startTime = new TimeSpan(9, 30, 0);
            var endTime = new TimeSpan(15, 30, 0);
            var beforeTickCount = 10;
            var beforePriceRangePercent = 1.0f;
            var highToLowPercent = 0.5f;

            Logger.AddMessage($"Define dates");
            var zipFileName = Settings.DataFolder + @"Minute\Polygon2003\Data\MP2003_20240406.zip";
            Dictionary<string, Dictionary<DateTime, float>> tickerAndDateAndHighToLow;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                var entry = zip.Entries.First(a => a.Name.Contains("_AA_"));
                var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                tickerAndDateAndHighToLow = PolygonCommon.GetSymbolsAndHighToLowForStrategies(oo.results.Select(a => a.DateTime.Date)
                    .Distinct().OrderBy(a => a).ToArray());
            }

            Debug.Print($"BeforeTicks: {beforeTickCount}\tPrevPriceRange,%: {beforePriceRangePercent}\tHighToLow,%: {highToLowPercent}");
            Debug.Print("Ticker\tDateTime\tDailyHighToLow\tGap\tGap,%\tHighLow,%\t K trades\tPrevClose\tOpen\tHigh\tLow\tClose\tTrades\tProfit,%");
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a =>
                             a.Length > 0 &&
                             tickerAndDateAndHighToLow.ContainsKey(
                                 Path.GetFileNameWithoutExtension(a.Name).Split('_')[1])))
                {
                    var ticker = Path.GetFileNameWithoutExtension(entry.Name).Split('_')[1];
                    var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                    foreach (var kvp in tickerAndDateAndHighToLow[ticker])
                    {
                        var fromTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(kvp.Key + startTime);
                        var toTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(kvp.Key + endTime);
                        var data = oo.results.Where(a => a.t >= fromTicks && a.t <= toTicks).OrderBy(a => a.t).ToArray();
                        for (var k = beforeTickCount; k < (data.Length - 1); k++)
                        {
                            var item = data[k];
                            var minutesBefore = item.DateTime.TimeOfDay.TotalMinutes - data[k - beforeTickCount].DateTime.TimeOfDay.TotalMinutes;
                            if ((beforeTickCount - minutesBefore) > 0.5) continue; // there are missing ticks
                            if (item.l < 5f) continue; // tick price < $5.0
                            if (item.o > (item.h + item.l) / 2 || item.c < (item.h + item.l) / 2) continue; // not up tick

                            var currentHighToLowPercent = (item.h - item.l) / (item.h + item.l) * 200f;
                            if (currentHighToLowPercent < highToLowPercent) continue; // high-low < 1%
                            if ((item.c - item.o) < (item.h - item.l) / 2) continue; // small close-open

                            var diffOpenClose = Math.Abs(item.o - data[k - 1].c);
                            var diffOpenClosePercent = diffOpenClose / (item.o + data[k - 1].c) * 200f;
                            if (!(diffOpenClose < 0.015 || diffOpenClosePercent < 0.1)) continue; // big split between close of previous minute and open

                            var closeOpenDiff = 0f;
                            var beforeHigh = float.MinValue;
                            var beforeLow = float.MaxValue;
                            var beforeTrades = 0;
                            for (var k1 = 1; k1 <= beforeTickCount; k1++)
                            {
                                if (k1 != beforeTickCount)
                                    closeOpenDiff += Convert.ToSingle(Math.Round(Math.Abs(data[k - k1].o - data[k - k1 - 1].c), 4));
                                if (data[k - k1].h > beforeHigh) beforeHigh = data[k - k1].h;
                                if (data[k - k1].h < beforeLow) beforeLow = data[k - k1].l;
                                beforeTrades += data[k - k1].n;
                            }

                            if (5.0 * beforeTrades / beforeTickCount > item.n) continue; // no big trade count
                            if (!(closeOpenDiff / beforeTickCount < 0.015 || closeOpenDiff / beforeTickCount / item.o * 200 < 0.1))
                                continue; // big split between close and open

                            if ((beforeHigh - beforeLow) / (beforeHigh + beforeLow) * 200 > beforePriceRangePercent) continue; // big price range before main tick

                            var nextTick = data[k + 1];
                            Debug.Print($"{ticker}\t{item.DateTime}\t{kvp.Value}\t{closeOpenDiff / beforeTickCount}\t{closeOpenDiff / beforeTickCount/item.o*100}\t{currentHighToLowPercent}\t{item.n / (1.0 * beforeTrades / beforeTickCount) }\t{item.c}\t{nextTick.o}\t{nextTick.h}\t{nextTick.l}\t{nextTick.c}\t{nextTick.n}\t{(nextTick.c - nextTick.o) / nextTick.o * 100}");
                        }

                    }
                }

            Logger.AddMessage($"Finished");
        }
    }
}
