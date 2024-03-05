using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Helpers;

namespace Data.Scanners
{
    public class HourPolygon
    {
        private const float AveragePeriod = 21f;
        private const float AveragePeriodDelta = 2.6f;
        private static readonly DateTime FromDate = new DateTime(2022, 1, 1);
        private static readonly DateTime ToDate = new DateTime(2099, 12, 29);

        public static void StartMin5()
        {
            var aa = new List<TimeSpan>();
            var ts = new TimeSpan(9, 30, 0);
            while (ts <= Settings.MarketEndCommon)
            {
                aa.Add(ts);
                ts = ts.Add(new TimeSpan(0, 5, 0));
            }
            Start("dbQ2024Temp..Min5Polygon", aa.ToArray(), aa.Where(a => a <= Settings.MarketEndOfShortenedDay).ToArray());
        }

        public static void StartMin15()
        {
            var aa = new List<TimeSpan>();
            var ts = new TimeSpan(9, 30, 0);
            while (ts <= Settings.MarketEndCommon)
            {
                aa.Add(ts);
                ts = ts.Add(new TimeSpan(0, 15, 0));
            }
            Start("dbQ2024..Min15Polygon", aa.ToArray(), aa.Where(a => a <= Settings.MarketEndOfShortenedDay).ToArray());
        }

        public static void StartHourHalf()
        {
            var timeCommon = "09:30,10:00,10:30,11:00,11:30,12:00,12:30,13:00,13:30,14:00,14:30,15:00,15:30,16:00".Split(',').Select(TimeSpan.Parse).ToArray();
            var timeShortened = "09:30,10:00,10:30,11:00,11:30,12:00,12:30,13:00".Split(',').Select(TimeSpan.Parse).ToArray();
            Start("dbQ2024..HourHalfPolygon", timeCommon, timeShortened);
        }

        public static void StartHour()
        {
            var timeCommon = "09:30,10:00,11:00,12:00,13:00,14:00,15:00,15:45,16:00".Split(',').Select(TimeSpan.Parse).ToArray();
            var timeShortened = "09:30,10:00,11:00,12:00,12:45,13:00".Split(',').Select(TimeSpan.Parse).ToArray();
            Start("dbQ2024..HourPolygon2", timeCommon, timeShortened);
        }

        public static void Start(string tableName, TimeSpan[] timeCommon, TimeSpan[] timeShortened)
        {
            var timeRangeCommon = new List<(TimeSpan, TimeSpan)>();
            for (var k = 0; k < timeCommon.Length - 1; k++)
                timeRangeCommon.Add((timeCommon[k], timeCommon[k + 1]));

            var timeRangeShortened = new List<(TimeSpan, TimeSpan)>();
            for (var k = 0; k < timeShortened.Length - 1; k++)
                timeRangeShortened.Add((timeShortened[k], timeShortened[k + 1]));

            var allResults = new List<QuoteScanner>();
            var resultsCount = 0;

            DbUtils.ClearAndSaveToDbTable(allResults, tableName, "Symbol", "Date", "Time", "To", "Open", "High", "Low",
                "Close", "Volume", "TradeCount", "Final", "FinalDelayInMinutes", "Count",
                "OpenNextDelayInMinutes", "OpenNext", "HighNext", "LowNext",
                "Prev2Wma_10", "PrevWma_10", "Wma_10", "CloseAtWma_10",
                "Prev2Wma_20", "PrevWma_20", "Wma_20", "CloseAtWma_20",
                "Prev2Wma_30", "PrevWma_30", "Wma_30", "CloseAtWma_30",
                "PrevEma_10", "PrevEma_10", "Ema_10", "CloseAtEma_10",
                "PrevEma_20", "PrevEma_20", "Ema_20", "CloseAtEma_20",
                "Prev2Ema_30", "PrevEma_30", "Ema_30", "CloseAtEma_30",
                "Prev2Ema2_10", "PrevEma2_10", "Ema2_10", "CloseAtEma2_10",
                "Prev2Ema2_20", "PrevEma2_20", "Ema2_20", "CloseAtEma2_20",
                "Prev2Ema2_30", "PrevEma2_30", "Ema2_30", "CloseAtEma2_30");

            foreach (var oo in Data.Actions.Polygon.PolygonMinuteScan.GetQuotes(FromDate, ToDate))
            {
                var symbol = oo.Item1;
                var date = oo.Item2;
                var ticksFrom = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.MarketStart)); // Unix ticks for 9:30
                var tickTo = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.GetMarketEndTime(date)));
                var quotes = oo.Item3.Where(a => a.t >= ticksFrom && a.t < tickTo && a.IsValid).ToArray();
                var closes = quotes.Select(q => q.c).ToArray();
                var wma_10 = StatMethods.Wma(closes, 10);
                var wma_20 = StatMethods.Wma(closes, 20);
                var wma_30 = StatMethods.Wma(closes, 30);
                var ema_10 = StatMethods.Ema(closes, 10);
                var ema_20 = StatMethods.Ema(closes, 20);
                var ema_30 = StatMethods.Ema(closes, 30);
                var ema2_10 = StatMethods.Ema(closes, 10);
                var ema2_20 = StatMethods.Ema2(closes, 20);
                var ema2_30 = StatMethods.Ema2(closes, 30);

                var timeRange = Settings.ShortenedDays.ContainsKey(date) ? timeRangeShortened : timeRangeCommon;
                var results = new List<QuoteScanner>();

                foreach (var o1 in timeRange)
                {
                    resultsCount += results.Count;
                    var result = new QuoteScanner(symbol, date, o1);
                    results.Add(result);

                    var fromUnixTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(o1.Item1));
                    var toUnixTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(o1.Item2));
                    var countFull = 0;
                    bool? wma_10_Up = null;
                    bool? wma_20_Up = null;
                    bool? wma_30_Up = null;
                    bool? ema_10_Up = null;
                    bool? ema_20_Up = null;
                    bool? ema_30_Up = null;
                    bool? ema2_10_Up = null;
                    bool? ema2_20_Up = null;
                    bool? ema2_30_Up = null;
                    foreach (var quote in quotes)
                    {
                        if (quote.t < fromUnixTicks)
                        {
                            result.Prev2Wma_10 = result.PrevWma_10;
                            result.Prev2Wma_20 = result.PrevWma_20;
                            result.Prev2Wma_30 = result.PrevWma_30;
                            result.Prev2Ema_10 = result.PrevEma_10;
                            result.Prev2Ema_20 = result.PrevEma_20;
                            result.Prev2Ema_30 = result.PrevEma_30;
                            result.Prev2Ema2_10 = result.PrevEma2_10;
                            result.Prev2Ema2_20 = result.PrevEma2_20;
                            result.Prev2Ema2_30 = result.PrevEma2_30;

                            result.PrevWma_10 = wma_10[countFull];
                            result.PrevWma_20 = wma_20[countFull];
                            result.PrevWma_30 = wma_30[countFull];
                            result.PrevEma_10 = ema_10[countFull];
                            result.PrevEma_20 = ema_20[countFull];
                            result.PrevEma_30 = ema_30[countFull];
                            result.PrevEma2_10 = ema2_10[countFull];
                            result.PrevEma2_20 = ema2_20[countFull];
                            result.PrevEma2_30 = ema2_30[countFull];
                        }
                        else if (quote.t < toUnixTicks)
                        {
                            result.Count++;
                            result.TradeCount += quote.n;
                            result.Volume += quote.v;

                            if (result.Count == 1)
                            {
                                result.Open = quote.o;
                                result.High = quote.h;
                                result.Low = quote.l;
                                result.Close = quote.c;

                                result.Wma_10 = wma_10[countFull];
                                result.Wma_20 = wma_20[countFull];
                                result.Wma_30 = wma_30[countFull];
                                result.Ema_10 = ema_10[countFull];
                                result.Ema_20 = ema_20[countFull];
                                result.Ema_30 = ema_30[countFull];
                                result.Ema2_10 = ema2_10[countFull];
                                result.Ema2_20 = ema2_20[countFull];
                                result.Ema2_30 = ema2_30[countFull];

                                wma_10_Up = GetAverageUpDown(wma_10[countFull]);
                                wma_20_Up = GetAverageUpDown(wma_20[countFull]);
                                wma_30_Up = GetAverageUpDown(wma_30[countFull]);
                                ema_10_Up = GetAverageUpDown(ema_10[countFull]);
                                ema_20_Up = GetAverageUpDown(ema_20[countFull]);
                                ema_30_Up = GetAverageUpDown(ema_30[countFull]);
                                ema2_10_Up = GetAverageUpDown(ema2_10[countFull]);
                                ema2_20_Up = GetAverageUpDown(ema2_20[countFull]);
                                ema2_30_Up = GetAverageUpDown(ema2_30[countFull]);

                                bool? GetAverageUpDown(float aver)
                                {
                                    if (aver > 0 && (result.Open - 0.01f) > aver) return true;
                                    if (aver > 0 && (result.Open + 0.01f) < aver) return false;
                                    return null;
                                }
                            }
                            else
                            {
                                if (result.Count == 2)
                                {
                                    result.OpenNext = quote.o;
                                    result.HighNext = quote.h;
                                    result.LowNext = quote.l;
                                    result.OpenNextDelayInMinutes = Convert.ToByte((quote.t - fromUnixTicks) / 60000);
                                }

                                if (quote.h > result.HighNext) result.HighNext = quote.h;
                                if (quote.l < result.LowNext) result.LowNext = quote.l;

                                if (quote.h > result.High) result.High = quote.h;
                                if (quote.l < result.Low) result.Low = quote.l;
                                result.Close = quote.c;
                            }

                            result.CloseAtWma_10 ??= GetCloseAtAver(wma_10[countFull], wma_10_Up);
                            result.CloseAtWma_20 ??= GetCloseAtAver(wma_20[countFull], wma_20_Up);
                            result.CloseAtWma_30 ??= GetCloseAtAver(wma_30[countFull], wma_30_Up);
                            result.CloseAtEma_10 ??= GetCloseAtAver(ema_10[countFull], ema_10_Up);
                            result.CloseAtEma_20 ??= GetCloseAtAver(ema_20[countFull], ema_20_Up);
                            result.CloseAtEma_30 ??= GetCloseAtAver(ema_30[countFull], ema_30_Up);
                            result.CloseAtEma2_10 ??= GetCloseAtAver(ema2_10[countFull], ema2_10_Up);
                            result.CloseAtEma2_20 ??= GetCloseAtAver(ema2_20[countFull], ema2_20_Up);
                            result.CloseAtEma2_30 ??= GetCloseAtAver(ema2_30[countFull], ema2_30_Up);

                            float? GetCloseAtAver(float aver, bool? averUp)
                            {
                                if (averUp == true && quote.l <= aver)
                                    return quote.l;
                                if (averUp == false && quote.h >= aver)
                                    return quote.h;
                                return null;
                            }
                        }
                        else
                        {
                            if (!result.Final.HasValue)
                            {
                                result.Final = quote.o;
                                result.FinalDelayInMinutes = Convert.ToInt16((quote.t - toUnixTicks) / 60000);
                            }
                        }

                        countFull++;
                    }
                }

                allResults.AddRange(results);
                if (allResults.Count > 100000)
                {
                    DbUtils.SaveToDbTable(allResults, tableName, "Symbol", "Date", "Time", "To", "Open", "High", "Low",
                        "Close", "Volume", "TradeCount", "Final", "FinalDelayInMinutes", "Count",
                        "OpenNextDelayInMinutes", "OpenNext", "HighNext", "LowNext",
                        "Prev2Wma_10", "PrevWma_10", "Wma_10", "CloseAtWma_10",
                        "Prev2Wma_20", "PrevWma_20", "Wma_20", "CloseAtWma_20",
                        "Prev2Wma_30", "PrevWma_30", "Wma_30", "CloseAtWma_30",
                        "PrevEma_10", "PrevEma_10", "Ema_10", "CloseAtEma_10",
                        "PrevEma_20", "PrevEma_20", "Ema_20", "CloseAtEma_20",
                        "Prev2Ema_30", "PrevEma_30", "Ema_30", "CloseAtEma_30",
                        "Prev2Ema2_10", "PrevEma2_10", "Ema2_10", "CloseAtEma2_10",
                        "Prev2Ema2_20", "PrevEma2_20", "Ema2_20", "CloseAtEma2_20",
                        "Prev2Ema2_30", "PrevEma2_30", "Ema2_30", "CloseAtEma2_30");
                    allResults.Clear();
                }
            }

            if (allResults.Count > 0)
            {
                DbUtils.SaveToDbTable(allResults, tableName, "Symbol", "Date", "Time", "To", "Open", "High", "Low",
                    "Close", "Volume", "TradeCount", "Final", "FinalDelayInMinutes", "Count",
                    "OpenNextDelayInMinutes", "OpenNext", "HighNext", "LowNext",
                    "Prev2Wma_10", "PrevWma_10", "Wma_10", "CloseAtWma_10",
                    "Prev2Wma_20", "PrevWma_20", "Wma_20", "CloseAtWma_20",
                    "Prev2Wma_30", "PrevWma_30", "Wma_30", "CloseAtWma_30",
                    "PrevEma_10", "PrevEma_10", "Ema_10", "CloseAtEma_10",
                    "PrevEma_20", "PrevEma_20", "Ema_20", "CloseAtEma_20",
                    "Prev2Ema_30", "PrevEma_30", "Ema_30", "CloseAtEma_30",
                    "Prev2Ema2_10", "PrevEma2_10", "Ema2_10", "CloseAtEma2_10",
                    "Prev2Ema2_20", "PrevEma2_20", "Ema2_20", "CloseAtEma2_20",
                    "Prev2Ema2_30", "PrevEma2_30", "Ema2_30", "CloseAtEma2_30");
                allResults.Clear();
            }

            Debug.Print($"Results count: {resultsCount}");
        }
    }
}
