using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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

        public static IEnumerable<(string, DateTime, PolygonCommon.cMinuteItem[])> GetMinuteQuotesByDate(string sqlWithSymbolAndDate)
        {
            var sql = $"SELECT a.Folder, a.Symbol, a.Date FROM dbQ2024Minute..MinutePolygonLog a inner join " +
                                        $"({sqlWithSymbolAndDate}) b on a.Symbol = b.Symbol and a.Date = b.Date " +
                                        "WHERE RowStatus IN (2, 5) ORDER BY a.Folder, a.Symbol, a.Date";

            return GetMinuteQuotes(sql);
        }

        public static IEnumerable<(string, DateTime, PolygonCommon.cMinuteItem[])> GetMinuteQuotesByDate(DateTime from, DateTime to, float minTradeValue, int minTradeCount)
        {
            var sql = $"SELECT a.Folder, a.Symbol, a.Date FROM dbQ2024Minute..MinutePolygonLog a " +
                                        "inner join dbQ2024..DayPolygon b on a.Symbol=b.Symbol and a.Date=b.Date " +
                                        "WHERE b.IsTest IS NULL AND a.RowStatus IN (2, 5) " +
                                        $"AND a.Date BETWEEN '{from:yyyy-MM-dd}' AND '{to:yyyy-MM-dd}' AND " +
                                        $"a.[Close]*a.[Volume]>={(minTradeValue * 1000000).ToString(CultureInfo.InvariantCulture)} AND a.TradeCount>={minTradeCount} " +
                                        "ORDER BY a.Folder, a.Symbol, a.Date";

            return GetMinuteQuotes(sql);
        }

        private static IEnumerable<(string, DateTime, PolygonCommon.cMinuteItem[])> GetMinuteQuotes(string sql)
        {
            var sw = new Stopwatch();
            sw.Start();

            var itemCount = 0;
            long byteCount = 0;

            Logger.AddMessage($"Started");

            Task<(string, List<(DateTime, PolygonCommon.cMinuteItem[])>)> task = null;
            foreach (var oo in GetDatesAndBytes(sql))
            {
                if (itemCount % 100 == 0)
                    Logger.AddMessage($"Items: {itemCount}");
                itemCount++;

                if (task != null)
                {
                    foreach (var oo1 in task.Result.Item2)
                        yield return (task.Result.Item1, oo1.Item1, oo1.Item2);
                }

                task = Task.Run(() =>
                {
                    var jsonData = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<PolygonCommon.cMinuteRoot>(oo.Item3);
                    var result = new List<(DateTime, PolygonCommon.cMinuteItem[])>();
                    foreach (var date in oo.Item2)
                    {
                        var fromTicks = TimeHelper.GetUnixMillisecondsFromEstDateTime(date);
                        var toTicks = fromTicks + TimeHelper.UnixMillisecondsForOneDay; // next day
                        var data = jsonData.results.Where(a => a.t >= fromTicks && a.t < toTicks).ToArray();
                        result.Add((date, data));
                    }

                    return (oo.Item1, result);
                });
            }

            if (task != null)
            {
                foreach (var oo1 in task.Result.Item2)
                    yield return (task.Result.Item1, oo1.Item1, oo1.Item2);
            }

            sw.Stop();

            Logger.AddMessage($"Finished!!! {sw.Elapsed.TotalSeconds:N0} seconds. Items: {itemCount:N0}. Bytes: {byteCount:N0}");
        }

        private static IEnumerable<(string, List<DateTime>, byte[])> GetDatesAndBytes(string sql)
        {
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
                cmd.CommandText = sql;
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

            (string, List<DateTime>, byte[]) GetResults() => (lastSymbol, lastDates, lastBytes);
        }
    }
}
