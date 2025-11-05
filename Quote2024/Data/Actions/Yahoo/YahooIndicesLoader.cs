using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Data.Helpers;

namespace Data.Actions.Yahoo
{
    public static class YahooIndicesLoader
    {
        // Valid time for period is (date + 09:30 of NewYork time zone)
        private const string UrlTemplate = "https://query1.finance.yahoo.com/v7/finance/download/{2}?period1={0}&period2={1}&interval=1d&events=history&includeAdjustedClose=true";
        // actual url (may be error: Unauthorized): https://query1.finance.yahoo.com/v7/finance/download/AA?period1=1679112201&period2=1710734601&interval=1d&events=history&includeAdjustedClose=true

        // UrlTemplateForTradingDaysOnly is more reliable
        private const string UrlTemplateForTradingDaysOnly = "https://query2.finance.yahoo.com/v8/finance/chart/{2}?period1={0}&period2={1}&interval=1d&events=history&includePrePost=false";
        private static readonly string[] Symbols = new[] { "^DJI", "^GSPC" };

        // https://api.nasdaq.com/api/quote/INDU/historical?assetclass=index&fromdate=2024-08-08&limit=9999&todate=2024-09-08
        private static readonly string NasdaqUrlTemplate = @"https://api.nasdaq.com/api/quote/{0}/historical?assetclass=index&fromdate={1}&limit=9999&todate={2}";
        private static readonly Dictionary<string, string> NasdaqSymbols = new Dictionary<string, string> { { "INDU", "^DJI" }, { "SPX", "^GSPC" } };

        // https://query2.finance.yahoo.com/v8/finance/chart/%5EGSPC?period1=1725321600&period2=1725667200&interval=1d&events=history
        private const string YahooMinuteUrlTemplate = "https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1d&events=history";

        #region  =========  RealTime  ==========
        public static DateTime[] GetTradingDays(Action<string> fnShowStatus, DateTime toDate, int days)
        {
            Logger.AddMessage($"Started", fnShowStatus);

            var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(toDate.Date.AddDays(-days + 1)) / 1000;
            var toUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(toDate.Date.AddHours(23)) / 1000;
            var dates = new List<DateTime>();
            RealTimeDownloadData(Symbols[0], fromUnixSeconds, toUnixSeconds, dates);

            Logger.AddMessage($"!Finished", fnShowStatus);

            return dates.Select(a => a.Date).OrderByDescending(a => a).ToArray();
        }

        private static void RealTimeDownloadData(string symbol, long fromUnixSeconds, long toUnixSeconds, List<DateTime> dates)
        {
            // Download data
            Logger.AddMessage($"Download data for {symbol}");
            var url = string.Format(UrlTemplateForTradingDaysOnly, fromUnixSeconds, toUnixSeconds, symbol);
            var o = WebClientExt.GetToBytes(url, false);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            var content = Encoding.UTF8.GetString(o.Item1).Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
            var oo = ZipUtils.DeserializeString<Models.MinuteYahoo>(content);

            var aa1 = oo.chart.result[0].timestamp.Select(a => TimeHelper.GetEstDateTimeFromUnixMilliseconds(a * 1000));
            dates.AddRange(aa1);
        }
        #endregion

        #region ========  Yahoo Minute  ========
        public static void YahooMinuteStart()
        {
            var maxDate = GetStartDate();
            var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(maxDate) / 1000;
            var toUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(DateTime.Today) / 1000 - 1; // last second of previous day
            var data = new List<DayYahoo>();
            foreach (var symbol in Symbols)
                YahooMinuteDownloadData(symbol, fromUnixSeconds, toUnixSeconds, data);
            SaveData(data);
        }

        private static void YahooMinuteDownloadData(string symbol, long fromUnixSeconds, long toUnixSeconds, List<DayYahoo> data)
        {
            Logger.AddMessage($"Download data for {symbol}");
            var fileName = $@"E:\Quote\WebData\Indices\Yahoo\{symbol}.2025-05-10.json";
            // var o = (File.ReadAllBytes(fileName), (CookieCollection)null, (Exception)null);
            var url = string.Format(YahooMinuteUrlTemplate, symbol, fromUnixSeconds, toUnixSeconds);
            var o = WebClientExt.GetToBytes(url, false);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            var oo = ZipUtils.DeserializeBytes<cYahooMinuteRoot>(o.Item1);

            var quotes = oo.chart.result[0].indicators.quote[0];
            for (var k = 0; k < oo.chart.result[0].timestamp.Length; k++)
            {
                var dataSymbol = oo.chart.result[0].meta.symbol;
                var date = TimeHelper.GetEstDateTimeFromUnixMilliseconds(oo.chart.result[0].timestamp[k] * 1000).Date;
                var open = quotes.open[k];
                var high = quotes.high[k];
                var low = quotes.low[k];
                var close = quotes.close[k];
                var volume = quotes.volume[k];
                var item = new DayYahoo(symbol, date, open, high, low, close, volume);
                data.Add(item);
            }
        }
        #endregion

        #region =========  Nasdaq =========
        public static void NasdaqStart()
        {
            var fromDate = GetStartDate();
            var data = new List<DayYahoo>();
            foreach (var symbol in NasdaqSymbols.Keys)
                NasdaqDownloadData(symbol, fromDate, DateTime.Today.AddDays(-1),  data);
            SaveData(data);
        }

        private static void NasdaqDownloadData(string symbol, DateTime fromDate, DateTime toDate, List<DayYahoo> data)
        {
            Logger.AddMessage($"Download data for {symbol}");
            var url = string.Format(NasdaqUrlTemplate, symbol, fromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                toDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            var o = WebClientExt.GetToBytes(url, false, true);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            var oo = ZipUtils.DeserializeBytes<cNasdaqRoot>(o.Item1);

            foreach (var row in oo.data.tradesTable.rows)
            {
                var item = new DayYahoo(NasdaqSymbols[symbol], row);
                data.Add(item);
            }
        }
        #endregion

        public static void Start()
        {
            var maxDate = GetStartDate();
            var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(maxDate.AddDays(-30)) / 1000;
            var toUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(DateTime.Today) / 1000 - 1;
            var data = new List<DayYahoo>();
            foreach (var symbol in Symbols)
                DownloadData(symbol, fromUnixSeconds, toUnixSeconds, data);
            SaveData(data);
        }

        private static void DownloadData(string symbol, long fromUnixSeconds, long toUnixSeconds, List<DayYahoo> data)
        {
            Logger.AddMessage($"Download data for {symbol}");
            var url = string.Format(UrlTemplate, fromUnixSeconds, toUnixSeconds, symbol);
            var o = WebClientExt.GetToBytes(url, false);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            var lines = Encoding.UTF8.GetString(o.Item1).Split('\n');
            if (lines.Length == 0)
                throw new Exception($"Invalid Day Yahoo quote file (no text lines): {o}");
            if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                throw new Exception($"Invalid YahooIndex quotes file (check file header): {lines[0]}");

            for (var k = 1; k < lines.Length; k++)
            {
                if (!string.IsNullOrWhiteSpace(lines[k]))
                {
                    if (lines[k].Contains("null"))
                        Debug.Print($"{symbol}, {lines[k]}");
                    else
                    {
                        var ss = lines[k].Split(',');
                        var item = new DayYahoo(symbol, ss);
                        data.Add(item);
                    }
                }
            }
        }

        private static DateTime GetStartDate()
        {
            Logger.AddMessage($"Started");

            var maxDate = new DateTime(2000, 1, 1);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select max([date]) MaxDate from dbQ2024..TradingDays";
                var o = cmd.ExecuteScalar();
                if (o is DateTime) maxDate = (DateTime)o;
            }

            return maxDate;
        }

        private static void SaveData(List<DayYahoo> data)
        {
            if (data.Count > 0)
            {
                DbHelper.ClearAndSaveToDbTable(data, "dbQ2024..Bfr_DayYahooIndexes", "Symbol", "Date", "Open", "High", "Low",
                    "Close", "Volume", "AdjClose");
                DbHelper.ExecuteSql("INSERT into dbQ2024..DayYahooIndexes (Symbol, Date, [Open], High, Low, [Close], Volume, AdjClose) " +
                                    "SELECT a.Symbol, a.Date, a.[Open], a.High, a.Low, a.[Close], a.Volume, a.AdjClose " +
                                    "from dbQ2024..Bfr_DayYahooIndexes a " +
                                    "left join dbQ2024..DayYahooIndexes b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                    "where b.Symbol is null");

                Logger.AddMessage($"Update trading days in dbQ2024 database");
                DbHelper.RunProcedure("dbQ2024..pRefreshTradingDays");
            }

            Logger.AddMessage($"!Finished. Last trade date: {data.Max(a => a.Date):yyyy-MM-dd}");
        }
    }

    #region ===========  DayYahoo Model  ===============
    public class DayYahoo
    {
        public string Symbol;
        public DateTime Date;
        public float Open;
        public float High;
        public float Low;
        public float Close;
        public long Volume;
        public float AdjClose => Close;

        public DayYahoo(string symbol, DateTime date, float open, float high, float low, float close, long volume)
        {
            Symbol = symbol;
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }

        public DayYahoo(string symbol, string[] ss)
        {
            Symbol = symbol;
            Date = DateTime.Parse(ss[0].Trim(), CultureInfo.InvariantCulture);
            Open = Single.Parse(ss[1].Trim(), CultureInfo.InvariantCulture);
            High = Single.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);
            Low = Single.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
            Close = Single.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
            Volume = long.Parse(ss[6].Trim(), CultureInfo.InvariantCulture);
        }
        internal DayYahoo(string symbol, cNasdaqRow row)
        {
            Symbol = symbol;
            Date = DateTime.Parse(row.date.Trim(), CultureInfo.InvariantCulture);
            Open = Single.Parse(row.open, CultureInfo.InvariantCulture);
            High = Single.Parse(row.high, CultureInfo.InvariantCulture);
            Low = Single.Parse(row.low, CultureInfo.InvariantCulture);
            Close = Single.Parse(row.close, CultureInfo.InvariantCulture);
            // AdjClose = Decimal.Parse(row.close, CultureInfo.InvariantCulture);

            if (row.volume != "--")
                Volume = long.Parse(row.volume, NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        }
    }

    #endregion

    #region =========  Nasdaq Json classes  ============
    internal class cNasdaqRoot
    {
        public cNasdaqData data;
        public object message;
        public cNasdaqStatus status;
    }

    internal class cNasdaqStatus
    {
        public int rCode;
        public object bCodeMessage;
        public string developerMessage;
    }

    internal class cNasdaqData
    {
        public string symbol;
        public int totalRecords;
        public cMasdaqTradesTable tradesTable;
    }

    internal class cMasdaqTradesTable
    {
        public cNasdaqRow[] rows;
    }

    internal class cNasdaqRow
    {
        public string close;
        public string date;
        public string high;
        public string low;
        public string open;
        public string volume;
    }

    #endregion

    #region ===============  YahooMinute json classes  ==================
    public class cYahooMinuteRoot
    {
        public cYahooMinuteChart chart { get; set; }
    }

    public class cYahooMinuteChart
    {
        public cYahooMinuteResult[] result { get; set; }
    }

    public class cYahooMinuteResult
    {
        public cYahooMinuteMeta meta { get; set; }
        public long[] timestamp { get; set; }
        public cYahooMinuteIndicators indicators { get; set; }
    }

    public class cYahooMinuteMeta
    {
        public string currency { get; set; }
        public string symbol { get; set; }
        public string exchangeName { get; set; }
        public string instrumentType { get; set; }
        // public cTradingPeriod[,] tradingPeriods { get; set; }
    }

    public class cYahooMinuteIndicators
    {
        public cYahooMinuteQuote[] quote { get; set; }
    }

    public class cYahooMinuteQuote
    {
        public float[] open { get; set; }
        public float[] high { get; set; }
        public float[] low { get; set; }
        public float[] close { get; set; }
        public long[] volume { get; set; }
    }
    #endregion
}

