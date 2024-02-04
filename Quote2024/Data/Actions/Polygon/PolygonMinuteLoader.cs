using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteLoader
    {
        private const string UrlTemplate = "https://api.polygon.io/v2/aggs/ticker/{0}/range/1/minute/{1}/{2}?adjusted=false&sort=asc&limit=50000&apiKey={3}";
        private static string ZipFileNameTemplate = Path.Combine(PolygonCommon.DataFolderMinute, "MP2003_{0}.zip");
        private static string FileNameTemplate = Path.Combine(PolygonCommon.DataFolderMinute, @"MP2003_{0}\MP2003_{2}_{1}.json");

        public static void Start()
        {
            Logger.AddMessage($"Started");

            Logger.AddMessage($"Define symbols to download ...");
            var symbols = new List<string>();
            var from = DateTime.MaxValue;
            var to = DateTime.MinValue;
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = "select a.Symbol, Min(a.Date) MinDate, MAX(a.Date) MaxDate from dbQ2024..DayPolygon a "+
                                  "inner join(select MinDate, iif(DATEADD(Day,70,MinDate)< GetDate(), DATEADD(Day, 70, MinDate), GetDate()) MaxDate "+
                                  "from (select DATEADD(day, -5, max(date)) MinDate from dbQ2024Minute..MinutePolygonLog) x) b "+
                                  "on a.Date between b.MinDate and b.MaxDate GROUP BY a.Symbol order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        symbols.Add((string)rdr["Symbol"]);
                        var minDate = (DateTime)rdr["MinDate"];
                        if (minDate < from) from = minDate;
                        var maxDate = (DateTime)rdr["MaxDate"];
                        if (maxDate > to) to = maxDate;
                    }
            }

            Start(symbols, from, to);

            Logger.AddMessage($"Finished!");
        }

        public static void StartAll()
        {
            Logger.AddMessage($"Started");

            var startDate=new DateTime(2003,9,6);
            var endDate = DateTime.Today.AddDays(-70);
            var dates = new List<DateTime>();

            while (startDate <= endDate)
            {
                dates.Add(startDate);
                startDate = startDate.AddDays(+70);
            }

            foreach (var from in dates)
            {
                var to = from.AddDays(70 - 1);
                var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
                if (File.Exists(zipFileName)) continue;

                var mySymbols = new List<string>();

                Logger.AddMessage($"Define symbols to download from {from:yyyy-MM-dd} to {to:yyyy-MM-dd} ...");

                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandTimeout = 150;
                    cmd.CommandText = $"SELECT DISTINCT Symbol FROM dbQ2024..DayPolygon WHERE Date between '{from:yyyy-MM-dd}' AND '{to:yyyy-MM-dd}' ORDER BY 1";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            mySymbols.Add((string) rdr["Symbol"]);
                }

                Start(mySymbols, from, to);
            }

            Logger.AddMessage($"Finished!");
        }

        public static void Start(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var cnt = 0;
            var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            if (File.Exists(zipFileName))
                return;

            var folder = Path.GetDirectoryName(string.Format(FileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"), "", ""));
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            Task task = null;
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}. Dates from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.");

                var symbol = PolygonCommon.GetPolygonTicker(mySymbol);
                var fileName = string.Format(FileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"), from.ToString("yyyyMMdd"), mySymbol);
                if (File.Exists(fileName))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, symbol, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey2003());
                var o = Download.DownloadToBytes(url, true);
                if (o is Exception ex)
                    throw new Exception($"PolygonMinuteLoader: Error while download from {url}. Error message: {ex.Message}");

                task?.Wait();
                task = File.WriteAllTextAsync(fileName, System.Text.Encoding.UTF8.GetString((byte[])o));
            }

            task?.Wait();

            Logger.AddMessage($"Create zip file: {Path.GetFileName(zipFileName)}");
            ZipUtils.CompressFolder(folder, zipFileName);
            Directory.Delete(folder, true);

            Logger.AddMessage($"!Finished. No errors. {mySymbols.Count} symbols. Folder: {folder}");
        }

        public static void StartZip(List<string> mySymbols, DateTime from, DateTime to)
        {
            Logger.AddMessage($"Started");

            var cnt = 0;
            var zipFileName = string.Format(ZipFileNameTemplate, to.AddDays(1).ToString("yyyyMMdd"));
            var zipEntries = new Dictionary<string, object>();
            if (File.Exists(zipFileName))
            {
                using (var zipArchive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    zipEntries = zipArchive.Entries.ToDictionary(a => a.FullName.ToLower(), a => (object)null);
            }

            var virtualFileEntries = new List<VirtualFileEntry>();
            foreach (var mySymbol in mySymbols)
            {
                Logger.AddMessage($"Downloaded {cnt++} tickers from {mySymbols.Count}. Dates from {from:yyyy-MM-dd} to {to:yyyy-MM-dd}.");

                var entryName = $@"{Path.GetFileNameWithoutExtension(zipFileName)}\pMin_{mySymbol}_{from:yyyyMMdd}.json";
                var urlTicker = PolygonCommon.GetPolygonTicker(mySymbol);
                if (zipEntries.ContainsKey(entryName.ToLower()))
                    continue;

                // var url = $"https://api.polygon.io/v2/aggs/ticker/{urlTicker}/range/1/minute/{from:yyyy-MM-dd}/{to:yyyy-MM-dd}?adjusted=false&sort=asc&limit=50000&apiKey={PolygonCommon.GetApiKey()}";
                var url = string.Format(UrlTemplate, urlTicker, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), PolygonCommon.GetApiKey2003());
                var o = Download.DownloadToBytes(url, true);
                if (o is Exception ex)
                    throw new Exception($"PolygonMinuteLoader: Error while download from {url}. Error message: {ex.Message}");

                ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(entryName, (byte[])o) });
            }

            Logger.AddMessage($"!Finished. No errors. {mySymbols.Count} symbols. Zip file name: {zipFileName}");
        }
    }
}
