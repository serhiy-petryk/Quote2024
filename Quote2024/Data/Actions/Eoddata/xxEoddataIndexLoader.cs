using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Eoddata
{
    // ?? Download empty data.
    // Temporary for replace YahooIndexLoader
    public class xxEoddataIndexLoader
    {
        private const string FILE_FOLDER = @"E:\Quote\WebData\Daily\Eoddata\";
        private static string[] _indexes = new string[] { "DJI", "SP500"};
        private const string URL_HOME = "https://www.eoddata.com/download.aspx";
        private const string URL_TEMPLATE = "https://eoddata.com/data/filedownload.aspx?e=INDEX&s={0}&sd={1}&ed={2}&d=9&p=0&o=d&k={3}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var fromDate = new DateTime(2000, 1, 1);
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "select max([date]) MaxDate from dbQ2024..TradingDays";
                var o1 = cmd.ExecuteScalar();
                if (o1 is DateTime) fromDate = (DateTime)o1;
            }

            var sFromDate = fromDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var sToDate = DateTime.Today.AddDays(-2).ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            // Prepare cookies
            var cookies = EoddataCommon.GetEoddataCookies();

            // Get k parameter of eoddata url
            var o = WebClientExt.GetToBytes(URL_HOME, false, false, cookies);
            if (o.Item3 != null)
                throw new Exception($"EoddataDailyLoader: Error while download from {URL_HOME}. Error message: {o.Item3.Message}");

            var s = System.Text.Encoding.UTF8.GetString(o.Item1);
            var i1 = s.IndexOf("/data/filedownload.aspx?e=", StringComparison.InvariantCulture);
            var i2 = s.IndexOf("\"", i1 + 20, StringComparison.InvariantCulture);
            var kParameter = s.Substring(i1 + 20, i2 - i1 - 20).Split('&').FirstOrDefault(a => a.StartsWith("k="));
            if (kParameter == null)
                throw new Exception("Can not define 'k' parameter of url request for www.eoddata.com");

            foreach (var index in _indexes)
            {
                Logger.AddMessage($"Download Eoddata daily data for {index} index");
                var url = string.Format(URL_TEMPLATE, index, sFromDate, sToDate, kParameter.Substring(2));
                o = WebClientExt.GetToBytes(url, false, false, cookies);
                if (o.Item3 != null)
                    throw new Exception($"EoddataDailyLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                var zipFileName = $"{FILE_FOLDER}{index}_{sToDate}.zip";
                var entry = new VirtualFileEntry($"{Path.GetFileNameWithoutExtension(zipFileName)}.txt", o.Item1);
                ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { entry });
            }
            /*var fromUnixSeconds = TimeHelper.GetUnixMillisecondsFromEstDateTime(maxDate.AddDays(-30)) / 1000;
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

            Logger.AddMessage($"!Finished. Last trade date: {data.Max(a => a.Date):yyyy-MM-dd}");*/
        }


        public static void StartOld()
        {
            Logger.AddMessage($"Started");

            var tradingDays = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT [date] FROM dbQ2024..TradingDays WHERE [date] > DATEADD(day,-30, GETDATE())";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        tradingDays.Add((DateTime) rdr["Date"]);
            }

            var missingFiles = new List<(string, string)>();
            /*foreach (var date in tradingDays)
            {
                var fileTimeStamp = date.ToString("yyyyMMdd", CultureInfo.InstalledUICulture);
                foreach (var exchange in _exchanges)
                {
                    var filename = $"{FILE_FOLDER}{exchange}_{fileTimeStamp}.zip";
                    if (!File.Exists(filename))
                        missingFiles.Add((exchange, fileTimeStamp));
                }
            }*/

            if (missingFiles.Count > 0)
            {
                // Prepare cookies
                var cookies = EoddataCommon.GetEoddataCookies();

                // Get k parameter of eoddata url
                var o = WebClientExt.GetToBytes(URL_HOME, false, false, cookies);
                if (o.Item3 != null)
                    throw new Exception($"EoddataDailyLoader: Error while download from {URL_HOME}. Error message: {o.Item3.Message}");

                var s = System.Text.Encoding.UTF8.GetString(o.Item1);
                var i1 = s.IndexOf("/data/filedownload.aspx?e=", StringComparison.InvariantCulture);
                var i2 = s.IndexOf("\"", i1 + 20, StringComparison.InvariantCulture);
                var kParameter = s.Substring(i1 + 20, i2 - i1 - 20).Split('&').FirstOrDefault(a => a.StartsWith("k="));
                if (kParameter == null)
                    throw new Exception("Can not define 'k' parameter of url request for www.eoddata.com");

                // Download and zip missing files
                foreach (var fileId in missingFiles)
                {
                    Logger.AddMessage($"Download Eoddata daily data for {fileId.Item1} and {fileId.Item2}");
                    var url = string.Format(URL_TEMPLATE, fileId.Item1, fileId.Item2, kParameter.Substring(2));
                    o = WebClientExt.GetToBytes(url, false, false, cookies);
                    if (o.Item3 != null)
                        throw new Exception($"EoddataDailyLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                    var zipFileName = $"{FILE_FOLDER}{fileId.Item1}_{fileId.Item2}.zip";
                    var entry = new VirtualFileEntry($"{Path.GetFileNameWithoutExtension(zipFileName)}.txt", o.Item1);
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[]{entry});
                }

                Logger.AddMessage($"!Downloaded {missingFiles.Count} files");
            }

            // Get missing quotes in database
            Logger.AddMessage($"Get existing data in database");
            var existingQuotes = new Dictionary<string, object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = "SELECT distinct Exchange, date from dbQ2024..DayEoddata";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            existingQuotes.Add($"{FILE_FOLDER}{(string)rdr["Exchange"]}_{(DateTime)rdr["Date"]:yyyyMMdd}.zip", null);
                }
            }

            // Parse and save new items to database
            var files = Directory.GetFiles(FILE_FOLDER, "*.zip", SearchOption.TopDirectoryOnly);
            var newFileCount = 0;
            var itemCount = 0;
            var fileSize = 0;
            foreach (var file in files)
            {
                if (!existingQuotes.ContainsKey(file))
                {
                    newFileCount++;
                    Logger.AddMessage($"Save to database quotes from file {Path.GetFileName(file)}");
                    itemCount += ParseAndSaveToDb(file);
                    fileSize += CsUtils.GetFileSizeInKB(file);
                }
            }

            if (newFileCount > 0)
            {
                Logger.AddMessage($"Update data in database ('pUpdateDayEoddata' procedure)");
                DbHelper.RunProcedure("dbQ2024..pUpdateDayEoddata");
            }

            Logger.AddMessage($"!Finished. Loaded quotes into DayEoddata table. Quotes: {itemCount:N0}. Number of files: {newFileCount}. Size of files: {fileSize:N0}KB");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var exchange = Path.GetFileNameWithoutExtension(zipFileName).Split('_')[0].Trim().ToUpper();
            var quotes = new List<DayEoddata>();
            string[] lines = null;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                if (zip.Entries.Count != 1)
                    throw new Exception($"Error in zip file structure: {zipFileName}");
                lines = zip.Entries[0].GetLinesOfZipEntry().ToArray();
            }

            var itemCount = 0;
            string prevLine = null; // To ignore duplicates
            foreach (var line in lines)
            {
                if (!string.Equals(line, prevLine))
                {
                    itemCount++;
                    prevLine = line;
                    quotes.Add(new DayEoddata(exchange, line.Split(',')));
                }
            }

            DbHelper.SaveToDbTable(quotes, "dbQ2024..DayEoddata", "Symbol", "Exchange", "Date", "Open", "High", "Low", "Close", "Volume");
            return itemCount;
        }

        #region ===========  DayEodata models  ============

        public class DayEoddata
        {
            public string Symbol;
            public string Exchange;
            public DateTime Date;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public float Volume;

            public DayEoddata(string exchange, string[] ss)
            {
                Symbol = ss[0];
                Exchange = exchange.Trim().ToUpper();
                Date = DateTime.ParseExact(ss[1], "yyyyMMdd", CultureInfo.InvariantCulture);
                Open = float.Parse(ss[2].Trim(), CultureInfo.InvariantCulture);
                High = float.Parse(ss[3].Trim(), CultureInfo.InvariantCulture);
                Low = float.Parse(ss[4].Trim(), CultureInfo.InvariantCulture);
                Close = float.Parse(ss[5].Trim(), CultureInfo.InvariantCulture);
                Volume = float.Parse(ss[6].Trim(), CultureInfo.InvariantCulture);
            }

            public DayEoddata(DbDataReader rdr)
            {
                Symbol = (string)rdr["Symbol"];
                Exchange = (string)rdr["Exchange"];
                Date = (DateTime)rdr["Date"];
                Open = (float)rdr["Open"]; ;
                High = (float)rdr["High"]; ;
                Low = (float)rdr["Low"]; ;
                Close = (float)rdr["Close"]; ;
                Volume = (float)rdr["Volume"]; ;
            }
        }
        #endregion
    }
}
