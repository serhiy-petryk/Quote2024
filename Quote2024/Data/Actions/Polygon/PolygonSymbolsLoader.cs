using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonSymbolsLoader
    {
        // private const string UrlTemplate = "https://api.polygon.io/v3/reference/tickers?market=stocks&date={0}&active=true&limit=1000";
        private const string UrlTemplate = "https://api.polygon.io/v3/reference/tickers?date={0}&active=true&limit=1000";
        private static readonly string ZipFileNameTemplate = Path.Combine(PolygonCommon.DataFolderSymbols, "SymbolsPolygon_{0}.zip");

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Define list of dates
            var dates = new List<DateTime>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = "select date from dbQ2024..TradingDays where date>=(select min(date) from dbQ2024..DayPolygon) order by 1";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        dates.Add((DateTime)rdr["Date"]);
            }

            var itemCount = 0;
            var fileCount = 0;
            foreach (var date in dates)
            {
                var zipFileName = string.Format(ZipFileNameTemplate, date.ToString("yyyyMMdd"));
                if (!File.Exists(zipFileName))
                {
                    fileCount++;
                    var folder = zipFileName.Substring(0, zipFileName.Length - 4);
                    var url = string.Format(UrlTemplate, date.ToString("yyyy-MM-dd"));
                    var cnt = 0;
                    var virtualFileEntries = new List<VirtualFileEntry>();
                    while (url != null)
                    {
                        url = url + "&apiKey=" + PolygonCommon.GetApiKey2003();
                        var filename = folder + $@"\SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json";
                        Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(filename)} for {date:yyyy-MM-dd}");

                        var o = Download.DownloadToBytes(url, true);
                        if (o is Exception ex)
                            throw new Exception($"PolygonSymbolsLoader: Error while download from {url}. Error message: {ex.Message}");

                        var entry = new VirtualFileEntry(Path.Combine(Path.GetFileName(folder), $@"SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json"), (byte[])o);
                        virtualFileEntries.Add(entry);

                        var oo = ZipUtils.DeserializeBytes<cRoot>(entry.Content);
                        /*File.WriteAllText(Path.Combine(folder, $@"SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json"), (byte[])o);

                        var oo = JsonConvert.DeserializeObject<cRoot>((string)o);*/
                        if (oo.status != "OK")
                            throw new Exception($"Wrong status of response! Status is '{oo.status}'");

                        url = oo.next_url;
                        cnt++;
                    }

                    ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
                    itemCount += ParseAndSaveToDb(zipFileName);
                }
            }

            Logger.AddMessage($"!Finished. Processed {fileCount} files with {itemCount:N0} items");
        }

        public static void ParseAndSaveAllZip()
        {
            var files = Directory.GetFiles( PolygonCommon.DataFolderSymbols, "*.zip").OrderBy(a => a).ToArray();
            var itemCount = 0;
            foreach (var zipFileName in files)
                itemCount += ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Processed {files.Length} files with {itemCount:N0} items");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            Logger.AddMessage($"Parsing and saving to database: {Path.GetFileName(zipFileName)}");

            var items = new List<cItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var date = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMdd", CultureInfo.InstalledUICulture);
                    foreach (var item in oo.results)
                    {
                        item.pDate = date;
                        item.pTimeStamp = entry.LastWriteTime.DateTime;
                    }

                    items.AddRange(oo.results.Where(a => string.Equals(a.market, "stocks")));
                }

            DbUtils.ClearAndSaveToDbTable(items, "dbQ2024..Bfr_SymbolsPolygon", "pSymbol", "pDate", "pExchange",
                "pName", "type", "cik", "composite_figi", "share_class_figi", "last_updated_utc", "pTimeStamp");

            DbUtils.RunProcedure("dbQ2024..pUpdateSymbolsPolygon");

            return items.Count;
        }

        #region ===========  Json SubClasses  ===========

        private class cRoot
        {
            public int count;
            public string next_url;
            public string request_id;
            public cItem[] results;
            public string status;
        }
        private class cItem
        {
            public string ticker;
            public string name;
            public string market; // stocks, otc, indices, fx, crypto
            public string locale;
            public string primary_exchange;
            public string type;
            public bool active;
            public string currency_name;
            public string cik;
            public string composite_figi;
            public string share_class_figi;
            public DateTime last_updated_utc;
            public DateTime delisted_utc;

            public string pSymbol => PolygonCommon.GetMyTicker(ticker);
            public DateTime pDate;
            public string pName => string.IsNullOrEmpty(name) ? null : name;
            public string pExchange => string.IsNullOrEmpty(primary_exchange) ? "No" : primary_exchange;

            public DateTime pTimeStamp;
        }
        #endregion

    }
}
