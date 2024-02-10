using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Scanners
{
    public class HourPolygon
    {
        private const float AveragePeriod = 21f;
        private const float AveragePeriodDelta = 2.6f;

        public static void Start()
        {
            var timeCommon = "09:30,10:00,11:00,12:00,13:00,14:00,15:00,15:45,16:00".Split(',').Select(TimeSpan.Parse).ToArray();
            var timeRangeCommon = new List<(TimeSpan, TimeSpan)>();
            for (var k=0; k<timeCommon.Length-1;k++)
                timeRangeCommon.Add((timeCommon[k], timeCommon[k + 1]));

            var timeShortened = "09:30,10:00,11:00,12:00,12:45,13:00".Split(',').Select(TimeSpan.Parse).ToArray();

            var timeRangeShortened = new List<(TimeSpan, TimeSpan)>();
            for (var k = 0; k < timeShortened.Length - 1; k++)
                timeRangeShortened.Add((timeShortened[k], timeShortened[k + 1]));

            var allResults = new List<HourPolygon>();
            var resultsCount = 0;

            DbUtils.ClearAndSaveToDbTable(allResults, "dbQ2024..HourPolygon", "Symbol", "Date", "Time", "To", "Open", "High", "Low", "Close", "Volume", "Average", "TradeCount", "Count");

            var from = new DateTime(2022, 1, 1);
            var to = new DateTime(2099, 12, 31);
            foreach (var oo in Data.Actions.Polygon.PolygonMinuteScan.GetQuotes(from, to))
            {
                var symbol = oo.Item1;
                var date = oo.Item2;
                var ticksFrom = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.MarketStart)); // Unix ticks for 9:30
                var tickTo = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.GetMarketEndTime(date)));
                var quotes = oo.Item3.Where(a => a.t >= ticksFrom && a.t<tickTo).ToArray();

                var timeRange = Settings.ShortenedDays.ContainsKey(date) ? timeRangeShortened: timeRangeCommon;
                var results = new List<HourPolygon>();

                foreach (var o1 in timeRange)
                {
                    resultsCount += results.Count;
                    var result = new HourPolygon(symbol, date, o1);
                    results.Add(result);

                    var fromUnixTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(o1.Item1));
                    var toUnixTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(o1.Item2));
                    var countFull = 0;
                    foreach (var quote in quotes)
                    {
                        countFull++;
                        if (quote.t<fromUnixTicks)
                        {
                            if (countFull == 1)
                                result.Average = GetAveragePrice(quote);
                            else
                                result.Average = (result.Average * (AveragePeriod - AveragePeriodDelta) + AveragePeriodDelta * GetAveragePrice(quote)) / AveragePeriod;
                        }
                        else if (quote.t<toUnixTicks)
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
                            }
                            else
                            {
                                if (quote.h > result.High) result.High = quote.h;
                                if (quote.l < result.Low) result.Low = quote.l;
                                result.Close = quote.c;
                            }
                        }
                    }
                }

                allResults.AddRange(results);
                if (allResults.Count > 100000)
                {
                    DbUtils.SaveToDbTable(allResults, "dbQ2024..HourPolygon", "Symbol", "Date", "Time", "To", "Open", "High", "Low", "Close", "Volume", "Average", "TradeCount", "Count");
                    allResults.Clear();
                }
            }

            if (allResults.Count > 0)
            {
                DbUtils.SaveToDbTable(allResults, "dbQ2024..HourPolygon", "Symbol", "Date", "Time", "To", "Open", "High", "Low", "Close", "Volume", "Average", "TradeCount", "Count");
                allResults.Clear();
            }

            Debug.Print($"Results count: {resultsCount}");

            float GetAveragePrice(PolygonCommon.cMinuteItem q) => (q.o + q.h + q.l + q.c) / 4.0f;
        }

        #region =========  Instance  ==========

        public string Symbol;
        public DateTime Date;
        public TimeSpan Time;
        public TimeSpan To;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float Volume;
        public float Average;
        public int TradeCount;
        public byte Count;

        private long _fromUnixMilliseconds;
        private long _toUnixMilliseconds;

        public HourPolygon(string symbol, DateTime date, (TimeSpan,TimeSpan) FromTo)
        {
            Symbol = symbol;
            Date = date;
            Time = FromTo.Item1;
            To = FromTo.Item2;
            _fromUnixMilliseconds = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Time));
            _toUnixMilliseconds = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(To));
        }
        #endregion
    }
}
