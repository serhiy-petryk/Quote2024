﻿using System;
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

        public static DateTime[] GetTradingDays(Action<string> fnShowStatus, DateTime toDate, int days)
        {
            Logger.AddMessage($"Started", fnShowStatus);

            var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(toDate.Date.AddDays(-days + 1)) / 1000;
            var toUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(toDate.Date.AddHours(23)) / 1000;
            var dates = new List<DateTime>();
            DownloadData(Symbols[0], fromUnixSeconds, toUnixSeconds, dates);

            Logger.AddMessage($"!Finished", fnShowStatus);

            return dates.Select(a => a.Date).OrderByDescending(a => a).ToArray();
        }

        public static void Start()
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

            var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(maxDate.AddDays(-30))/1000;
            var toUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(DateTime.Today) / 1000 - 1;
            var data = new List<DayYahoo>();
            foreach (var symbol in Symbols)
                DownloadData(symbol, fromUnixSeconds, toUnixSeconds, data);

            if (data.Count > 0)
            {
                DbHelper.ClearAndSaveToDbTable(data, "dbQ2023Others..Bfr_DayYahooIndexes", "Symbol", "Date", "Open", "High", "Low",
                    "Close", "Volume", "AdjClose");
                DbHelper.ExecuteSql("INSERT into dbQ2023Others..DayYahooIndexes (Symbol, Date, [Open], High, Low, [Close], Volume, AdjClose) " +
                                   "SELECT a.Symbol, a.Date, a.[Open], a.High, a.Low, a.[Close], a.Volume, a.AdjClose " +
                                   "from dbQ2023Others..Bfr_DayYahooIndexes a " +
                                   "left join dbQ2023Others..DayYahooIndexes b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                   "where b.Symbol is null");

                // Logger.AddMessage($"Update trading days in dbQ2023Others database");
                // DbHelper.RunProcedure("dbQ2023Others..pRefreshTradingDays");
                
                Logger.AddMessage($"Update trading days in dbQ2024 database");
                DbHelper.RunProcedure("dbQ2024..pRefreshTradingDays");
            }

            Logger.AddMessage($"!Finished. Last trade date: {data.Max(a => a.Date):yyyy-MM-dd}");
        }

        private static void DownloadData(string symbol, long fromUnixSeconds, long toUnixSeconds, List<DayYahoo> data)
        {
            // Download data
            Logger.AddMessage($"Download data for {symbol}");
            var url = string.Format(UrlTemplate, fromUnixSeconds, toUnixSeconds, symbol);
            var o = WebClientExt.GetToBytes(url, false);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            /*var content = Encoding.UTF8.GetString(o.Item1).Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
            var oo = ZipUtils.DeserializeString<Models.MinuteYahoo>(content);*/


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

        private static void DownloadData(string symbol, long fromUnixSeconds, long toUnixSeconds, List<DateTime> dates)
        {
            // Download data
            Logger.AddMessage($"Download data for {symbol}");
            var url = string.Format(UrlTemplateForTradingDaysOnly, fromUnixSeconds, toUnixSeconds, symbol);
            var o = WebClientExt.GetToBytes(url, false);
            if (o.Item3 != null)
                throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item3.Message}");

            var content = Encoding.UTF8.GetString(o.Item1).Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
            var oo = ZipUtils.DeserializeString<Models.MinuteYahoo>(content);

            var aa1 = oo.chart.result[0].timestamp.Select(a=>TimeHelper.GetEstDateTimeFromUnixMilliseconds(a*1000));
            dates.AddRange(aa1);
        }
    }

    #region ===========  DayYahoo Model  ===============
    public class DayYahoo
    {
        public string Symbol;
        public DateTime Date;
        public decimal Open;
        public decimal High;
        public decimal Low;
        public decimal Close;
        public long Volume;
        public decimal AdjClose;

        public DayYahoo(string symbol, string[] ss)
        {
            Symbol = symbol;
            Date = DateTime.Parse(ss[0].Trim(), CultureInfo.InvariantCulture);
            Open = Decimal.Parse(ss[1].Trim(), CultureInfo.InvariantCulture);
            High = Decimal.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);
            Low = Decimal.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
            Close = Decimal.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
            Volume = long.Parse(ss[6].Trim(), CultureInfo.InvariantCulture);
            AdjClose = Decimal.Parse(ss[5].Trim(), CultureInfo.InvariantCulture);
        }
    }

    #endregion
}

