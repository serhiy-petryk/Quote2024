using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Actions.Yahoo;
using Data.Helpers;

namespace Data.Models
{
    public class MinuteYahoo
    {
        private class QuoteCorrection
        {
            public float[] PriceValues;
            // public int? VolumeFactor;
            public double? Split;
            public bool Remove = false;
            public bool PriceChecked = false;
            public bool SplitChecked = false;
        }

        private static Dictionary<string, Dictionary<DateTime, QuoteCorrection>> _allCorrections = null;

        public static bool IsQuotePriceChecked(Quote q) => _allCorrections.ContainsKey(q.Symbol) &&
                                                           _allCorrections[q.Symbol].ContainsKey(q.Timed) &&
                                                           _allCorrections[q.Symbol][q.Timed].PriceChecked;
        public static bool IsQuoteSplitChecked(Quote q) => _allCorrections.ContainsKey(q.Symbol) &&
                                                           _allCorrections[q.Symbol].ContainsKey(q.Timed) &&
                                                           _allCorrections[q.Symbol][q.Timed].SplitChecked;

        public static void ClearCorrections() => _allCorrections = null;
        private static Dictionary<DateTime, QuoteCorrection> GetCorrections(string symbol)
        {
            if (_allCorrections == null)
            {
                _allCorrections=new Dictionary<string, Dictionary<DateTime, QuoteCorrection>>();
                var lines = File.ReadAllLines(YahooCommon.MinuteYahooCorrectionFiles)
                    .Where(a => !string.IsNullOrWhiteSpace(a) && !a.Trim().StartsWith("#"));
                foreach (var line in lines)
                {
                    var ss = line.Split('\t');
                    var symbolKey = ss[0].Trim().ToUpper();
                    if (!_allCorrections.ContainsKey(symbolKey))
                        _allCorrections.Add(symbolKey, new Dictionary<DateTime, QuoteCorrection>());

                    var a1 = _allCorrections[symbolKey];
                    var corr = new QuoteCorrection();
                    DateTime dateKey;
                    switch (ss[2].Trim().ToUpper())
                    {
                        case "REMOVE":
                        case "DELETE":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.Remove = true;
                            break;
                        case "PRICE":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.PriceValues = new float[4];
                            for (var k = 0; k < 4; k++)
                                corr.PriceValues[k] = float.Parse(ss[k + 3].Trim(), CultureInfo.InvariantCulture);
                            break;
                        case "SPLIT":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            var f1 = double.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
                            var f2 = double.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
                            corr.Split = f1 / f2;
                            break;
                        case "PRICECHECKED":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                            corr.PriceChecked = true;
                            break;
                        case "SPLITCHECKED":
                            dateKey = DateTime.ParseExact(ss[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            corr.SplitChecked = true;
                            break;
                        /*case "VOLUME":
                            corr.VolumeFactor = int.Parse(ss[3].Trim());
                            break;*/
                        default:
                            throw new Exception($"Check MinuteYahoo correction file: {YahooCommon.MinuteYahooCorrectionFiles}. '{ss[2]}' is invalid action");
                    }

                    a1.Add(dateKey, corr);
                }
            }

            return _allCorrections.ContainsKey(symbol) ? _allCorrections[symbol] : null;
        }

        private string _metaSymbol => chart.result[0].meta.symbol;
        private Dictionary<DateTime, QuoteCorrection> _corrections;
        public List<Quote> GetQuotes(string symbol)
        {
            if (_metaSymbol != symbol)
                throw new Exception($"MinuteYahoo error. Different symbol. Filename symbol is '{symbol}, file context symbol is '{_metaSymbol}'");

            var quotes = new List<Quote>();
            _corrections = GetCorrections(symbol);
            if (chart.result[0].timestamp == null)
            {
                if (chart.result[0].indicators.quote[0].close != null)
                    throw new Exception("Check Normilize procedure in  in Quote2022.Models.MinuteYahoo");
                return quotes;
            }

            for (var k = 0; k < chart.result[0].timestamp.Length; k++)
            {
                if (chart.result[0].indicators.quote[0].open[k].HasValue &&
                    chart.result[0].indicators.quote[0].high[k].HasValue &&
                    chart.result[0].indicators.quote[0].low[k].HasValue &&
                    chart.result[0].indicators.quote[0].close[k].HasValue &&
                    chart.result[0].indicators.quote[0].volume[k].HasValue)
                {
                    var nyTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(chart.result[0].timestamp[k] * 1000);
                    var q = GetQuote(nyTime, chart.result[0].indicators.quote[0], k);
                    if (q != null)
                        quotes.Add(q);
                }
                else if (!chart.result[0].indicators.quote[0].open[k].HasValue &&
                         !chart.result[0].indicators.quote[0].high[k].HasValue &&
                         !chart.result[0].indicators.quote[0].low[k].HasValue &&
                         !chart.result[0].indicators.quote[0].close[k].HasValue &&
                         !chart.result[0].indicators.quote[0].volume[k].HasValue)
                {
                }
                else
                    throw new Exception($"Please, check quote data for {chart.result[0].timestamp[k]} timestamp (k={k})");
            }

            if (quotes.Count > 0 && quotes[quotes.Count - 1].Timed.TimeOfDay == new TimeSpan(16, 0, 0))
                quotes.RemoveAt(quotes.Count - 1);

            return quotes;
        }

        private Quote GetQuote(DateTime timed, cQuote fileQuote, int quoteNo)
        {
            var qCorr = _corrections != null && _corrections.ContainsKey(timed) ? _corrections[timed] : new QuoteCorrection();
            var split = _corrections != null && _corrections.ContainsKey(timed.Date) ? _corrections[timed.Date].Split : null;

            if (qCorr.Remove) return null;

            var q = new Quote() {Symbol = _metaSymbol, Timed = timed, Volume = fileQuote.volume[quoteNo].Value};
            if (qCorr.PriceValues == null)
            {
                if (split.HasValue)
                {
                    q.Open = Convert.ToSingle(Math.Round(fileQuote.open[quoteNo].Value * split.Value, 4));
                    q.High = Convert.ToSingle(Math.Round(fileQuote.high[quoteNo].Value * split.Value, 4));
                    q.Low = Convert.ToSingle(Math.Round(fileQuote.low[quoteNo].Value * split.Value, 4));
                    q.Close = Convert.ToSingle(Math.Round(fileQuote.close[quoteNo].Value * split.Value, 4));
                }
                else
                {
                    q.Open = Convert.ToSingle(fileQuote.open[quoteNo].Value);
                    q.High = Convert.ToSingle(fileQuote.high[quoteNo].Value);
                    q.Low = Convert.ToSingle(fileQuote.low[quoteNo].Value);
                    q.Close = Convert.ToSingle(fileQuote.close[quoteNo].Value);
                }
            }
            else
            {
                if (split.HasValue)
                {
                    q.Open = Convert.ToSingle(qCorr.PriceValues[0] * split.Value);
                    q.High = Convert.ToSingle(qCorr.PriceValues[1] * split.Value);
                    q.Low = Convert.ToSingle(qCorr.PriceValues[2] * split.Value);
                    q.Close = Convert.ToSingle(qCorr.PriceValues[3] * split.Value);
                }
                else
                {
                    q.Open = Convert.ToSingle(qCorr.PriceValues[0]);
                    q.High = Convert.ToSingle(qCorr.PriceValues[1]);
                    q.Low = Convert.ToSingle(qCorr.PriceValues[2]);
                    q.Close = Convert.ToSingle(qCorr.PriceValues[3]);
                }
            }

            return q;
        }

        #region ===============  Json property and SubClasses  ==================
        public cChart chart { get; set; }

        public class cChart
        {
            public cResult[] result { get; set; }
            public cError error { get; set; }
        }

        public class cError
        {
            public string code { get; set; }
            public string description { get; set; }
        }

        public class cResult
        {
            public cMeta meta { get; set; }
            public long[] timestamp { get; set; }
            public cIndicators indicators { get; set; }
        }

        public class cMeta
        {
            public string currency { get; set; }
            public string symbol { get; set; }
            public string exchangeName { get; set; }
            public string instrumentType { get; set; }
            public long? regularMarketTime { get; set; } // Last transaction DateTime
            public DateTime? NyRegularMarketTime => regularMarketTime.HasValue
                ? TimeHelper.GetEstDateTimeFromUnixMilliseconds(regularMarketTime.Value * 1000)
                : null;
        }

        public class cIndicators
        {
            public cQuote[] quote { get; set; }
        }

        public class cQuote
        {
          public float?[] open { get; set; }
          public float?[] high { get; set; }
          public float?[] low { get; set; }
          public float?[] close { get; set; }
          public long?[] volume { get; set; }
        }
        #endregion
    }
}
