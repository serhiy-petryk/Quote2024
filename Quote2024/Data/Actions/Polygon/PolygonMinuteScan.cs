using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteScan
    {
        private static DateTime From = new DateTime(2023, 1, 1);
        private static DateTime To = new DateTime(2023, 3, 1);
        private const int MinTurnover = 50;
        private const int MinTradeCount = 5000;

        private const string DataFolder = @"E:\Quote\WebData\Minute\Polygon2003\Data\";

        public static void Start()
        {
            var sw = new Stopwatch();
            sw.Start();

            Logger.AddMessage($"Started");

            var ids = new Dictionary<string, Dictionary<string, List<DateTime>>>();
            var counter = 0;
            var counter1 = 0;
            var recs = 0;
            var byteCount = 0;
            Stopwatch sw1 = null;
            ZipArchive zip = null;
            string lastFolder = null;
            string lastSymbol = null;
            PolygonCommon.cMinuteRoot jsonData = null;

            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = $"SELECT * FROM dbQ2024Minute..MinutePolygonLog WHERE RowStatus IN (2, 3, 4, 5) " +
                                  $"AND Date BETWEEN '{From:yyyy-MM-dd}' AND '{To:yyyy-MM-dd}' AND " +
                                  $"[Close]*[Volume]>={MinTurnover * 1000000} AND TradeCount>={MinTradeCount}" +
                                  $"ORDER BY Folder, Symbol, Date";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        counter++;
                        if (sw1 == null)
                        {
                            sw1 = new Stopwatch();
                            sw1.Start();
                        }
                        var folder = ((string)rdr["Folder"]).ToUpper();
                        var symbol = ((string)rdr["Symbol"]).ToUpper();
                        var date = (DateTime)rdr["Date"];
                        //var fromTicks = CsUtils.GetUnixSecondsFromEstDateTime(date)*1000;
                        //var toTicks = CsUtils.GetUnixSecondsFromEstDateTime(date.AddDays(1))*1000;

                        if (!string.Equals(folder, lastFolder))
                        {
                            Logger.AddMessage($"Data: {symbol}/{date:yyyy-MM-dd}.");
                            zip?.Dispose();
                            lastSymbol = null;
                            var zipFileName = DataFolder + folder + ".zip";
                            zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read);
                        }

                        if (!string.Equals(symbol, lastSymbol))
                        {
                            counter1++;
                            var entry = zip.Entries.First(a =>a.Name.Contains("_" + symbol + "_20", StringComparison.InvariantCultureIgnoreCase));

                            using (var entryStream = entry.Open())
                            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                            {
                                using (var memstream = new MemoryStream())
                                {
                                    reader.BaseStream.CopyTo(memstream);
                                    var bytes = memstream.ToArray();
                                    jsonData = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<PolygonCommon.cMinuteRoot>(bytes);
                                    byteCount += bytes.Length;
                                }
                            }
                            if (jsonData.adjusted || jsonData.status != "OK")
                                    throw new Exception("Check parser");
                        }

                        // var data = jsonData.results.Where(a => a.Date == date).ToArray();
                        //var data = jsonData.results.Where(a => a.t>=fromTicks && a.t<toTicks).ToArray();
                        // recs += data.Length;
                        /*var data = new List<PolygonCommon.cMinuteItem2>();
                        foreach(var item in jsonData.results)
                            if (item.DateTime.Date == date)
                                data.Add(item);*/

                        lastFolder = folder;
                        lastSymbol = symbol;
                    }
            }

            zip?.Dispose();

            sw.Stop();
            sw1.Stop();
            Logger.AddMessage($"Finished!!! {sw.Elapsed.TotalSeconds:N0} seconds. {sw1.Elapsed.TotalSeconds:N0} seconds. Items: {counter}. Entries: {counter1}. Recs: {recs:N0}. Bytes: {byteCount:N0}");
        }
    }
}
