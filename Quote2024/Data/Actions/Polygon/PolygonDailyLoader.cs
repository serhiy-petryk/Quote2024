using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonDailyLoader
    {
        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Define the dates to download
            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 150;
                /*cmd.CommandText = "select a.Date from (select date from dbQ2024..TradingDays " +
                                  "where date >= isnull((select min(date) from dbQ2024..DayPolygon), '2003-09-01')) a " +
                                  "left join dbQ2024..DayPolygon b on a.Date = b.Date where b.Date is null order by 1";*/
                cmd.CommandText = "select a.Date from dbQ2024..TradingDays a " +
                                  "left join (select distinct date from dbQ2024..DayPolygon) b on a.Date = b.Date " +
                                  "where b.Date is null and a.Date >= '2003-09-10' order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime)rdr["Date"]);
            }

            // Download data
            var cnt = 0;
            var itemCount = 0;
            var filesCount = 0;
            var filesSize = 0;
            foreach (var date in dates)
            {
                filesCount++;
                Logger.AddMessage($"Downloaded {cnt++} files from {dates.Count}");
                var zipFileName = Path.Combine(PolygonCommon.DataFolderDaily, $"DayPolygon_{date:yyyyMMdd}.zip");
                if (!File.Exists(zipFileName))
                {
                    var url = $@"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date:yyyy-MM-dd}?adjusted=false&apiKey={PolygonCommon.GetApiKey2003()}";
                    var o = Download.DownloadToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception($"PolygonDailyLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    var jsonFileName = $"DayPolygon_{date:yyyyMMdd}.json";
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[] {new VirtualFileEntry(jsonFileName, o.Item1)});
                }

                itemCount += ParseAndSaveToDb(zipFileName);
                filesSize += CsUtils.GetFileSizeInKB(zipFileName);
            }

            Logger.AddMessage($"!Finished. Loaded quotes into DayPolygon table. Quotes: {itemCount:N0}. Number of files: {filesCount}. Size of files: {filesSize:N0}KB");
        }

        public static void ParseAndSaveToDbAllFiles()
        {
            var files = Directory.GetFiles(PolygonCommon.DataFolderDaily, "*.zip");
            var itemCnt = 0;
            var fileCnt = 0;
            foreach (var file in files)
            {
                Logger.AddMessage($"Parsed {fileCnt++} files from {files.Length}");
                itemCnt += ParseAndSaveToDb(file);
            }

            Logger.AddMessage($"Finished! Parsed {fileCnt++} files from {files.Length}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            Logger.AddMessage($"Parsed and saved to database {zipFileName}");

            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry().Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
                    var oo = ZipUtils.DeserializeString<cRoot>(content);

                    if (oo.status != "OK" || oo.count != oo.queryCount || oo.count != oo.resultsCount || oo.adjusted)
                        throw new Exception($"Bad file: {zipFileName}");
                    
                    if (oo.resultsCount == 0) continue; // No data

                    itemCount += oo.results.Length;

                    // Save data to buffer table of data server
                    DbHelper.SaveToDbTable(oo.results, "dbQ2024..DayPolygon", "Symbol", "Date", "Open", "High",
                        "Low", "Close", "Volume", "WeightedVolumePrice", "TradeCount");
                }

            return itemCount;
        }

        #region ===========  Json SubClasses  ===========
        public class cRoot
        {
            public int queryCount;
            public int resultsCount;
            public bool adjusted;
            public cItem[] results;
            public string status;
            public string request_id;
            public int count;
        }

        public class cItem
        {
            public string TT;
            public float v;
            public float vw;
            public float o;
            public float h;
            public float l;
            public float c;
            public long t;
            public int n;

            public string Symbol => PolygonCommon.GetMyTicker(TT);
            public DateTime Date => TimeHelper.GetEstDateTimeFromUnixMilliseconds(t);
            public float Open => o;
            public float High => h;
            public float Low => l;
            public float Close => c;
            public float Volume => v;
            public float WeightedVolumePrice => vw;
            public int TradeCount => n;
        }
        #endregion
    }
}
