using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using SpanJson.Formatters;

namespace Data.Scaners
{
    public class TheFirstScanner
    {
        public static void Start()
        {
            var startFrom = new TimeSpan(9, 50, 0);
            var startTo = new TimeSpan(10, 30, 0);

            var tempTS = startFrom;
            var startTimeSpans = new List<TimeSpan>();
            while (tempTS <= startTo)
            {
                startTimeSpans.Add(tempTS);
                tempTS = tempTS.Add(new TimeSpan(0, 1, 0));
            }

            var endTimeSpansCommon = "11:00,12:00,13:00,14:00,15:00,15:45".Split(',').Select(TimeSpan.Parse).ToArray();
            var endTimeSpansShortened = "11:00,12:00,12:45".Split(',').Select(TimeSpan.Parse).ToArray();

            var allResults = new List<TheFirstScanner>();
            var resultsCount = 0;

            DbUtils.ClearAndSaveToDbTable(allResults, "dbQ2023Others..xx_scanner", "Symbol", "Date", "From", "To",
                "Count", "TradeCount", "HighBefore", "LowBefore", "Average", "Open", "High", "Low", "Close", "Final", "FinalTime");
            
            foreach (var oo in Data.Actions.Polygon.PolygonMinuteScan.GetQuotes())
            {
                var symbol = oo.Item1;
                var date = oo.Item2;

                var endTimeSpans = Settings.ShortenedDays.ContainsKey(date) ? endTimeSpansShortened : endTimeSpansCommon;
                var endMarketDateTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.GetMarketEndTime(date)));

                var results = new List<TheFirstScanner>();
                foreach (var o1 in startTimeSpans)
                foreach (var o2 in endTimeSpans)
                    results.Add(new TheFirstScanner(symbol, date, o1, o2));

                var ticksFrom = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(Settings.MarketStart)); // Unix ticks for 9:30
                var quotes = oo.Item3.Where(a => a.t >= ticksFrom).ToArray();

                resultsCount += results.Count;
                foreach (var quote in quotes)
                {
                    if (quote.t >= endMarketDateTicks)
                        break;

                    foreach (var result in results)
                    {
                        if (result.Final.HasValue)
                            break;

                        result.CountFull++;

                        if (quote.t < result._fromUnixMilliseconds) // Before start
                        {
                            if (result.CountFull == 1)
                            {
                                result.HighBefore = quote.h;
                                result.LowBefore = quote.l;
                            }
                            else
                            {
                                if (quote.h > result.HighBefore) result.HighBefore = quote.h;
                                if (quote.l < result.LowBefore) result.LowBefore = quote.l;
                            }
                        }
                        else if (quote.t < result._toUnixMilliseconds)
                        {
                            result.Count++;
                            result.TradeCount += quote.n;
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
                        else
                        {
                            result.Final = quote.o;
                            result.FinalTime = CsUtils.GetEstDateTimeFromUnixMilliseconds(quote.t).TimeOfDay;
                        }
                    }
                }

                allResults.AddRange(results);
                if (allResults.Count > 100000)
                {
                    DbUtils.SaveToDbTable(allResults, "dbQ2023Others..xx_scanner", "Symbol", "Date", "From", "To",
                        "Count", "TradeCount", "HighBefore", "LowBefore", "Average", "Open", "High", "Low", "Close", "Final", "FinalTime");
                    allResults.Clear();
                }
            }

            if (allResults.Count > 0)
            {
                DbUtils.SaveToDbTable(allResults, "dbQ2023Others..xx_scanner", "Symbol", "Date", "From", "To",
                    "Count", "TradeCount", "HighBefore", "LowBefore", "Average", "Open", "High", "Low", "Close", "Final", "FinalTime");
                allResults.Clear();
            }

            Debug.Print($"Results count: {resultsCount}");
        }

        #region =========  Instance  ==========

        public string Symbol;
        public DateTime Date;
        public TimeSpan From;
        public TimeSpan To;
        public short Count;
        public int TradeCount;
        public float HighBefore;
        public float LowBefore;
        public float Average;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public float? Final;
        public TimeSpan? FinalTime;

        private long _fromUnixMilliseconds;
        private long _toUnixMilliseconds;
        private short CountFull;

        public TheFirstScanner(string symbol, DateTime date, TimeSpan from, TimeSpan to)
        {
            Symbol = symbol;
            Date = date;
            From = from;
            To = to;
            _fromUnixMilliseconds = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(from));
            _toUnixMilliseconds = CsUtils.GetUnixMillisecondsFromEstDateTime(date.Add(to));
        }
        #endregion
    }
}
