using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class YahooSectorLoader
    {
        private const string UrlTemplate = @"https://query1.finance.yahoo.com/ws/insights/v3/finance/insights?disableRelatedReports=true&formatted=true&getAllResearchReports=false&reportsCount=0&ssl=true&symbols={0}&lang=en-US&region=US";
        private const string ZipFileNameTemplate = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_{0}.zip";
        private const string FileNameTemplate = @"YS_{1}_{2}_{0}.json";

        public static void TestParse()
        {
            var zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_20240704.zip";
            var data = ParseZip(zipFileName);

            DbHelper.ClearAndSaveToDbTable(data, "dbQ2024..Bfr_SectorYahoo", "PolygonSymbol", "Date", "Name", "Sector",
                "TimeStamp", "YahooSymbol");
            Logger.AddMessage($"Finished");

        }

        public static List<DbItem> ParseZip(string zipFileName)
        {
            Logger.AddMessage($"Started");

            var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
            var dateKey = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMdd", CultureInfo.InvariantCulture);
            var symbolXref = GetYahooSymbolXref();

            var data = new List<DbItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var timeStamp = entry.LastWriteTime.DateTime;
                    var o = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in o.finance.result)
                    {
                        if (symbolXref.ContainsKey(item.symbol))
                        {
                            var name = item.upsell?.companyName;
                            if (string.IsNullOrEmpty(name))
                                name = null;
                            else if (name.IndexOf('(', StringComparison.InvariantCultureIgnoreCase) != -1 && name.IndexOf(')', StringComparison.InvariantCultureIgnoreCase)==-1)
                            {
                                var i1 = name.IndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                                name = name.Substring(0, i1).Trim();
                            }
                            else if (name.IndexOf("(The)", StringComparison.InvariantCultureIgnoreCase) != -1)
                            {
                                var i1 = name.IndexOf("(The)", StringComparison.InvariantCultureIgnoreCase);
                                name = name.Substring(0, i1).Trim();
                            }
                            else if (name.IndexOf("(REIT)", StringComparison.InvariantCultureIgnoreCase) != -1)
                            {
                                var i1 = name.IndexOf("(REIT)", StringComparison.InvariantCultureIgnoreCase);
                                name = name.Substring(0, i1).Trim();
                            }

                            var dbItem = new DbItem()
                            {
                                PolygonSymbol = symbolXref[item.symbol],
                                Date = dateKey,
                                YahooSymbol = item.symbol,
                                Name = name,
                                Sector = item.instrumentInfo?.technicalEvents.sector ?? item.companySnapshot?.sectorInfo,
                                TimeStamp = timeStamp
                            };
                            if (string.IsNullOrEmpty(dbItem.Sector))
                            {
                                Debug.Print($"No sector:\t{dbItem.YahooSymbol}\t{dbItem.Name}");
                            }
                            else
                                data.Add(dbItem);
                        }
                        else
                        {
                            // some test/non-exists tickers
                        }
                    }

                }

            return data;
        }

        public static async Task Start()
        {
            var sw = new Stopwatch();
            sw.Start();

            const int chunkSize = 20;
            const int downloadBatchSize = 10; // 20 is faster ~10%
            
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = string.Format(ZipFileNameTemplate, timeStamp.Item2);
            var entryFolder = Path.GetFileNameWithoutExtension(zipFileName);

            var symbolsToDownload = GetSymbolsToDownload();
            var urls = symbolsToDownload.Select((a, index) => new { Value = a, Index = index }).GroupBy(a => a.Index / chunkSize)
                .Select(a => string.Format(UrlTemplate, string.Join(",", a.Select(a1 => a1.Value)))).ToArray();

            var itemsToDownload = urls.Select(a => new DownloadItem(a)).ToArray();

            await WebClientExt.DownloadItemsToMemory(itemsToDownload, downloadBatchSize);

            foreach (var item in itemsToDownload)
            { // Check on invalid items
                if (item.Data == null)
                    throw new Exception("Invalid data in YahooSectorLoader");
            }

            // Save data to zip
            var virtualFileEntries = itemsToDownload.Where(a => a.StatusCode == HttpStatusCode.OK).Select((a, index) =>
                    new VirtualFileEntry(
                        Path.Combine(entryFolder, string.Format(FileNameTemplate, timeStamp.Item2, downloadBatchSize, index)), a.Data))
                .ToArray();

            if (File.Exists(zipFileName))
                File.Delete(zipFileName);
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            sw.Stop();
            Debug.Print($"SW: {sw.ElapsedMilliseconds:N0}");

            Helpers.Logger.AddMessage($"Finished. Items: {symbolsToDownload.Count:N0}. SW: {sw.ElapsedMilliseconds:N0}");
        }

        private static List<string> GetSymbolsToDownload()
        { 
            var data = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                // Rights don't have sector info. 'FUND' and 'WARRANT' have.
                cmd.CommandText = "SELECT DISTINCT YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null";
                //                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"]);
            }

            return data;
        }

        private static Dictionary<string, string> GetYahooSymbolXref()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE IsTest is null and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        public class DownloadItem : WebClientExt.IDownloadToMemoryItem
        {
            public string Url { get; }
            public byte[] Data { get; set; }
            public HttpStatusCode? StatusCode { get; set; }

            public DownloadItem(string url)
            {
                Url = url;
            }
        }

        #region =======  Json classes  =========

        private class cRoot
        {
            public cFinance finance;
        }

        private class cFinance
        {
            public cResult[] result;
        }
        private class cResult
        {
            public string symbol;
            public cCompanySnapshot companySnapshot;
            public cInstrumentInfo instrumentInfo;
            public cUpsell upsell;
        }

        private class cCompanySnapshot
        {
            public string sectorInfo;
        }
        private class cInstrumentInfo
        {
            public cTechnicalEvents technicalEvents;
        }
        private class cTechnicalEvents
        {
            public string provider;
            public string sector;
        }

        private class cUpsell
        {
            public string companyName;
        }

        public class DbItem
        {
            public string PolygonSymbol;
            public string YahooSymbol;
            public DateTime Date;
            public string Name;
            public string Sector;
            public DateTime TimeStamp;
        }
        #endregion
    }
}
