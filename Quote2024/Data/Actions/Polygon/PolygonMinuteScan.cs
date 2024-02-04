using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteScan
    {
        // private static readonly DateTime From = new DateTime(2023, 7, 1);
        // private static readonly DateTime To = new DateTime(2023, 12, 29);
        private static readonly DateTime From = new DateTime(2023, 1, 1);
        private static readonly DateTime To = new DateTime(2023, 3, 1);
        private const int MinTurnover = 50;
        private const int MinTradeCount = 5000;

        public static IEnumerable<(string, DateTime, PolygonCommon.cMinuteItem[])> GetQuotes()
        {
            var sw = new Stopwatch();
            sw.Start();

            var itemCount = 0;
            long byteCount = 0;

            Logger.AddMessage($"Started");

            foreach (var oo in GetData())
            {
                if (itemCount % 100 == 0)
                    Logger.AddMessage($"Items: {itemCount}");
                itemCount++;
                foreach (var item in oo.Item2)
                {
                    // byteCount += item.Item2.Length;
                    yield return (oo.Item1, item.Item1, item.Item2);
                }
            }
            sw.Stop();

            Logger.AddMessage($"Finished!!! {sw.Elapsed.TotalSeconds:N0} seconds. Items: {itemCount:N0}. Bytes: {byteCount:N0}");
        }

        public static void Start()
        {
            var sw = new Stopwatch();
            sw.Start();

            var itemCount = 0;
            long byteCount = 0;

            Logger.AddMessage($"Started");

            foreach (var oo in GetData())
            {
                if (itemCount % 100 == 0)
                    Logger.AddMessage($"Items: {itemCount}");
                itemCount++;
                foreach (var item in oo.Item2)
                    byteCount += item.Item2.Length;
            }
            sw.Stop();

            Logger.AddMessage($"Finished!!! {sw.Elapsed.TotalSeconds:N0} seconds. Items: {itemCount:N0}. Bytes: {byteCount:N0}");
        }

        public static IEnumerable<(string, List<(DateTime, PolygonCommon.cMinuteItem[])>)> GetData()
        {
            Task<(string, List<(DateTime, PolygonCommon.cMinuteItem[])>)> task = null;
            foreach (var oo in GetBytes_ZipFile())
            {
                if (task != null)
                    yield return task.Result;

                task = Task.Run(() =>
                {
                    var jsonData = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<PolygonCommon.cMinuteRoot>(oo.Item2);
                    var result = new List<(DateTime, PolygonCommon.cMinuteItem[])>();
                    foreach (var date in oo.Item3)
                    {
                        var fromTicks = CsUtils.GetUnixMillisecondsFromEstDateTime(date);
                        var toTicks = fromTicks + CsUtils.UnixMillisecondsForOneDay; // next day
                        var data = jsonData.results.Where(a => a.t >= fromTicks && a.t < toTicks).ToArray();
                        result.Add((date, data));
                    }

                    return (oo.Item1, result);
                });
            }

            if (task != null)
                yield return task.Result;
        }

        public static IEnumerable<(string, byte[], List<DateTime>)> GetBytes_ZipFile()
        {
            // var foldersAndSymbolsAndDates = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            string lastFolder = null;
            string lastSymbol = null;
            byte[] lastBytes = null;
            List<DateTime> lastDates = null;
            ZipArchive zip = null;

            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = $"SELECT * FROM dbQ2024Minute..MinutePolygonLog WHERE RowStatus IN (2, 5) " +
                                  $"AND Date BETWEEN '{From:yyyy-MM-dd}' AND '{To:yyyy-MM-dd}' AND " +
                                  $"[Close]*[Volume]>={MinTurnover * 1000000} AND TradeCount>={MinTradeCount} " +
                                  "ORDER BY Folder, Symbol, Date";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var folder = ((string)rdr["Folder"]).ToUpper();
                        var symbol = ((string)rdr["Symbol"]).ToUpper();
                        var date = (DateTime)rdr["Date"];

                        if (!string.Equals(lastFolder, folder))
                        {
                            if (lastBytes != null)
                                yield return GetResults();

                            lastSymbol = null;
                            lastFolder = folder;
                            lastBytes = null;

                            var zipFileName = Path.Combine(PolygonCommon.DataFolderMinute, folder + ".zip");
                            zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read);
                        }

                        if (!string.Equals(lastSymbol, symbol))
                        {
                            if (lastBytes != null)
                                yield return GetResults();

                            lastSymbol = symbol;
                            lastDates = new List<DateTime>();
                            var entry = zip.Entries.First(a => a.Name.Contains("_" + symbol + "_20", StringComparison.InvariantCultureIgnoreCase));
                            using (var entryStream = entry.Open())
                            using (var memstream = new MemoryStream())
                            {
                                entryStream.CopyTo(memstream);
                                lastBytes = memstream.ToArray();
                            }
                        }

                        lastDates.Add(date);
                    }
            }

            if (lastBytes != null)
                yield return GetResults();

            (string, byte[], List<DateTime>) GetResults() => (lastSymbol, lastBytes, lastDates);
        }

    }
}
