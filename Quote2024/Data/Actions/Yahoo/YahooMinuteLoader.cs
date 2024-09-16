using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading;
using Data.Helpers;

namespace Data.Actions.Yahoo
{
    public static class YahooMinuteLoader
    {
        private const string UrlTemplate = @"https://query2.finance.yahoo.com/v8/finance/chart/{0}?period1={1}&period2={2}&interval=1m&events=history";

        public static void Start()
        {
            var from = DateTime.MaxValue;
            var to = DateTime.MinValue;
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                cmd.CommandText = "SELECT min(date) MinDate, max(date) MaxDate FROM dbQ2024..TradingDays where Date >= DATEADD(day, -7, GetDate())";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        from = (DateTime)rdr["MinDate"];
                        to = (DateTime)rdr["MaxDate"];
                    }
            }

            if (from < DateTime.MaxValue)
                Start(from, to.AddDays(1), GetDefaultYahooSymbolList());
            else
                Logger.AddMessage($"!Finished. No data");
        }

        public static void Start(DateTime from, DateTime to, ICollection<string> yahooSymbols)
        {
            Logger.AddMessage($"Started");

            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = Settings.DataFolder + $@"Minute\Yahoo\Data\YahooMinute_{timeStamp.Item2}.zip";
            var errorFileName = $@"{Path.GetDirectoryName(zipFileName)}\DownloadErrors_{timeStamp.Item2}.txt";

            var fromInSeconds = new DateTimeOffset(from, TimeSpan.Zero).ToUnixTimeSeconds();
            var toInSeconds = new DateTimeOffset(to, TimeSpan.Zero).ToUnixTimeSeconds();

            var cnt = 0;
            var downloadErrors = new List<string>();
            var virtualFileEntries = new List<VirtualFileEntry>();
            foreach (var symbol in yahooSymbols)
            {
                cnt++;
                if (cnt % 10 == 0)
                    Logger.AddMessage($@"Downloaded {cnt:N0} files from {yahooSymbols.Count:N0}");

                var url = string.Format(UrlTemplate, symbol, fromInSeconds, toInSeconds);
                var o = WebClientExt.GetToBytes(url, false);
                if (o.Item3 != null)
                    downloadErrors.Add($"{symbol}\t{o.Item3.Message}");
                else
                {
                    var entry = new VirtualFileEntry($@"YahooMinute_{timeStamp.Item2}\yMin-{symbol}.txt", o.Item1);
                    virtualFileEntries.Add(entry);
                }

                Thread.Sleep(300);
            }

            if (downloadErrors.Count > 0)
                File.WriteAllLines(errorFileName, downloadErrors);

            // Zip data
            Logger.AddMessage($@"Zip data. {virtualFileEntries.Count:N0} entries");
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            if (downloadErrors.Count > 0)
                Logger.AddMessage($"!Finished. Found {downloadErrors.Count} ERRORS. Error file: {errorFileName}. Items: {yahooSymbols.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
            else
                Logger.AddMessage($"!Finished. No errors. Items: {yahooSymbols.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        private static List<string> GetDefaultYahooSymbolList()
        {
            // var symbols = new List<string> { "YS", "FORLU", "BLACR", "CANB", "BLACW", "YSBPW", "BLAC", "CLCO" };
            var symbols = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                /*cmd.CommandText = "SELECT '^DJI' Symbol UNION SELECT '^GSPC' UNION " +
                                  "SELECT b.YahooSymbol FROM dbQ2024..DayEoddata a " +
                                  "INNER JOIN dbQ2024..SymbolsEoddata b on a.Exchange = b.Exchange and a.Symbol = b.Symbol " +
                                  "WHERE b.YahooSymbol is not null AND a.volume* a.[close]>= 5000000 and a.date >= DATEADD(day, -30, GetDate()) UNION " +
                                  "SELECT b.Symbol from dbQ2024..SymbolsEoddata a " +
                                  "RIGHT JOIN(SELECT * from dbQ2023Others..ScreenerNasdaqStock " +
                                  "WHERE Deleted is null or Deleted > DATEADD(day, -30, GetDate())) b " +
                                  "ON a.NasdaqSymbol = b.Symbol WHERE a.Symbol is null AND b.MaxTradeValue > 5 ORDER BY 1";*/
                cmd.CommandText = "SELECT '^DJI' Symbol UNION SELECT '^GSPC' UNION " +
                                  "SELECT b.YahooSymbol FROM dbQ2024..DayEoddata a " +
                                  "INNER JOIN dbQ2024..SymbolsEoddata b on a.Exchange = b.Exchange and a.Symbol = b.Symbol " +
                                  "WHERE b.YahooSymbol is not null AND a.volume* a.[close]>= 5000000 and a.date >= DATEADD(day, -30, GetDate()) " +
                                  "ORDER BY 1";

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbols.Add((string)rdr["Symbol"]);
            }

            return symbols;

        }
    }
}
