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
        private static readonly string[] Symbols = new[] { "^DJI", "^GSPC" };

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var maxDate = new DateTime(2000, 1, 1);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select max([date]) MaxDate from dbQ2023Others..TradingDays";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        maxDate = (DateTime)rdr["MaxDate"];
                        break;
                    }
            }

            var from = TimeHelper.GetUnixMillisecondsFromEstDateTime(maxDate.AddDays(-30))/1000;
            var to = TimeHelper.GetUnixMillisecondsFromEstDateTime(DateTime.Today) / 1000 - 1;

            var timeStamp = TimeHelper.GetTimeStamp();
            var entries = new List<VirtualFileEntry>();
            var data = new List<DayYahoo>();

            // Download data
            foreach (var symbol in Symbols)
            {
                Logger.AddMessage($"Download data for {symbol}");
                var url = string.Format(UrlTemplate, from, to, symbol);
                var o = Download.DownloadToBytes(url, false);
                if (o.Item2 != null)
                    throw new Exception($"YahooIndicesLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                var lines = Encoding.UTF8.GetString(o.Item1).Split('\n');
                if (lines.Length == 0)
                    throw new Exception($"Invalid Day Yahoo quote file (no text lines): {o}");
                if (lines[0] != "Date,Open,High,Low,Close,Adj Close,Volume")
                    throw new Exception($"Invalid YahooIndex quotes file (check file header): {lines[0]}");

                for (var k = 1; k < lines.Length; k++)
                {
                    if (!string.IsNullOrEmpty(lines[k].Trim()))
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

            if (data.Count > 0)
            {
                DbHelper.ClearAndSaveToDbTable(data, "dbQ2023Others..Bfr_DayYahooIndexes", "Symbol", "Date", "Open", "High", "Low",
                    "Close", "Volume", "AdjClose");
                DbHelper.ExecuteSql("INSERT into dbQ2023Others..DayYahooIndexes (Symbol, Date, [Open], High, Low, [Close], Volume, AdjClose) " +
                                   "SELECT a.Symbol, a.Date, a.[Open], a.High, a.Low, a.[Close], a.Volume, a.AdjClose " +
                                   "from dbQ2023Others..Bfr_DayYahooIndexes a " +
                                   "left join dbQ2023Others..DayYahooIndexes b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                   "where b.Symbol is null");

                Logger.AddMessage($"Update trading days in dbQ2023Others database");
                DbHelper.RunProcedure("dbQ2023Others..pRefreshTradingDays");
                
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

