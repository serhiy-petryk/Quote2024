using System;
using System.Collections.Concurrent;
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
            ParseZipAndSaveToDb(zipFileName);
        }

        public static void ParseZipAndSaveToDb(string zipFileName)
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
                            var dbItem = new DbItem()
                            {
                                PolygonSymbol = symbolXref[item.symbol],
                                Date = dateKey,
                                YahooSymbol = item.symbol,
                                Name = item.upsell?.companyName,
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

            DbHelper.ClearAndSaveToDbTable(data, "dbQ2024..Bfr_SectorYahoo", "PolygonSymbol", "Date", "Name", "Sector",
                "TimeStamp", "YahooSymbol");
            Logger.AddMessage($"Finished");
        }

        public static void Parse()
        {
            Logger.AddMessage($"Started");
            var symbolXref = GetYahooSymbolXref();
            var data = new List<DbItem>();
            var nameLen = 0;
            var sectorLen = 0;

            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_20240704";
            var files = Directory.GetFiles(folder, "*.json");
            foreach (var file in files)
            {
                var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(File.ReadAllBytes(file));
                var timeStamp = File.GetLastWriteTime(file);
                foreach (var item in o.finance.result)
                {
                    if (symbolXref.ContainsKey(item.symbol))
                    {
                        var dbItem = new DbItem()
                        {
                            PolygonSymbol = symbolXref[item.symbol],
                            YahooSymbol = item.symbol,
                            Name = item.upsell?.companyName,
                            Sector = item.instrumentInfo?.technicalEvents.sector ?? item.companySnapshot?.sectorInfo,
                            TimeStamp = timeStamp
                        };
                        if (string.IsNullOrEmpty(dbItem.Sector))
                        {
                            Debug.Print($"No sector:\t{dbItem.YahooSymbol}\t{dbItem.Name}");
                        }
                        else
                            data.Add(dbItem);

                        if (!string.IsNullOrEmpty(dbItem.Name) && dbItem.Name.Length > nameLen)
                            nameLen = dbItem.Name.Length;
                        if (!string.IsNullOrEmpty(dbItem.Sector) && dbItem.Sector.Length > sectorLen)
                            sectorLen = dbItem.Sector.Length;
                    }
                    else
                    {

                    }
                }
            }

            DbHelper.ClearAndSaveToDbTable(data, "dbQ2024..Bfr_SectorYahoo", "PolygonSymbol", "YahooSymbol", "Name",
                "Sector", "TimeStamp");

            Logger.AddMessage($"Finished");
        }

        public static async Task Start()
        {
            var sw = new Stopwatch();
            sw.Start();

            const int chunkSize = 20;
            const int DownloadBatchSize = 10;
            
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = string.Format(ZipFileNameTemplate, timeStamp.Item2);
            var entryFolder = Path.GetFileNameWithoutExtension(zipFileName);

            var symbolsToDownload = GetSymbolsToDownload2();
            var urls = symbolsToDownload.Select((a, index) => new { Value = a, Index = index }).GroupBy(a => a.Index / chunkSize)
                .Select(a => string.Format(UrlTemplate, string.Join(",", a.Select(a1 => a1.Value)))).ToArray();

            var itemsToDownload = urls.Select(a => new DownloadItem(a)).ToArray();

            await WebClientExt.DownloadItemsToMemory(itemsToDownload, DownloadBatchSize);

            var virtualFileEntries = itemsToDownload.Where(a => a.StatusCode == HttpStatusCode.OK).Select((a, index) =>
                    new VirtualFileEntry(
                        Path.Combine(entryFolder, string.Format(FileNameTemplate, timeStamp.Item2, DownloadBatchSize, index)), a.Data))
                .ToArray();
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            foreach (var item in itemsToDownload)
            {
                if (item.Data == null)
                    item.StatusCode = null;
            }



            // var urlAndFilenames = GetUrlAndFilenames(symbolsToDownload, timeStamp.Item2);
            // await WebClientExt.DownloadItemsToFiles(urlAndFilenames.Cast<WebClientExt.IDownloadItem>().ToList(), 10);

            // Save data to zip
            // var dataFolder = string.Format(Path.GetDirectoryName(FileNameTemplate), timeStamp.Item2);
            // var zipFileName = dataFolder + ".zip";
            // ZipUtils.CompressFolder(dataFolder, zipFileName);
            // Directory.Delete(dataFolder, true);// 

            // ParseZipAndSaveToDb(zipFileName);

            sw.Stop();
            Debug.Print($"SW: {sw.ElapsedMilliseconds:N0}");

            Helpers.Logger.AddMessage($"Finished. Items: {symbolsToDownload.Count:N0}. SW: {sw.ElapsedMilliseconds:N0}");
        }

        /*public static async Task XxStart()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var folder = string.Format(FolderTemplate, timeStamp.Item2);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var symbolsToDownload = GetSymbolsToDownload();
            var urlAndFilenames = GetUrlAndFilenames(symbolsToDownload, timeStamp.Item2);
            // await WebClientExt.DownloadItemsToFiles(urlAndFilenames.Cast<WebClientExt.IDownloadItem>().ToList(), 10);

            // Save data to zip
            var dataFolder = string.Format(Path.GetDirectoryName(FileNameTemplate), timeStamp.Item2);
            var zipFileName = dataFolder + ".zip";
            ZipUtils.CompressFolder(dataFolder, zipFileName);
            Directory.Delete(dataFolder, true);

            ParseZipAndSaveToDb(zipFileName);

            Helpers.Logger.AddMessage($"Finished. Items: {symbolsToDownload.Count:N0}");
        }*/

        /*public static async Task StartOld()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var symbolsToDownload = GetSymbolsToDownload();
            var urlAndFilenames = GetUrlAndFilenames(symbolsToDownload, timeStamp.Item2);
            await DownloadItems(urlAndFilenames);

            Helpers.Logger.AddMessage($"Finished. Items: {symbolsToDownload.Count:N0}");
        }*/

        public static void xxStart2()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var symbolsToDownload = GetSymbolsToDownload();

            xxDownloadItems(symbolsToDownload, timeStamp.Item2);

            Helpers.Logger.AddMessage($"Finished. Items: {symbolsToDownload.Count:N0}");
        }

        /*public static async Task DownloadItems(List<DownloadItem> urlAndFilenames)
        {
            var tasks = new ConcurrentDictionary<DownloadItem, Task<byte[]>>();
            var itemCount = 0;
            var needToDownload = true;
            while (needToDownload)
            {
                needToDownload = false;
                foreach (var urlAndFilename in urlAndFilenames)
                {
                    itemCount++;

                    if (!File.Exists(urlAndFilename.Filename))
                    {
                        var task = WebClientExt.DownloadToBytesAsync(urlAndFilename.Url);
                        tasks[urlAndFilename] = task;
                        urlAndFilename.StatusCode = null;
                    }
                    else
                        urlAndFilename.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 10)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {urlAndFilenames.Count:N0}");
                        tasks.Clear();
                        needToDownload = true;
                    }
                }

                if (tasks.Count > 0)
                {
                    await Download(tasks);
                    tasks.Clear();
                    needToDownload = true;
                }

                Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {urlAndFilenames.Count:N0}");
            }
        }*/

        /*private static async Task Download(ConcurrentDictionary<DownloadItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    var data = await kvp.Value;
                    File.WriteAllBytes(kvp.Key.Filename, data);
                    kvp.Key.StatusCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway))
                    {
                        kvp.Key.StatusCode = response.StatusCode;
                        continue;
                    }

                    throw new Exception($"YahooSectorLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }

        }*/

        /*public static List<DownloadItem> GetUrlAndFilenames(Dictionary<string, string> symbols, string dateKey)
        {
            const int chunkSize = 20;

            var urlAndFilenames = new List<DownloadItem>();
            var symbolsToDownload = new List<string>();
            var chunkCount = 0;
            foreach (var kvp in symbols)
            {
                symbolsToDownload.Add(kvp.Key);
                if (symbolsToDownload.Count == chunkSize)
                {
                    var filename = string.Format(FileNameTemplate, dateKey, chunkSize.ToString(), chunkCount.ToString());
                    var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                    urlAndFilenames.Add( new DownloadItem(url,filename));
                    symbolsToDownload.Clear();
                    chunkCount++;
                }
            }

            if (symbolsToDownload.Count > 0)
            {
                var filename = string.Format(FileNameTemplate, dateKey, chunkSize.ToString(), chunkCount.ToString());
                var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                urlAndFilenames.Add(new DownloadItem(url, filename));
                symbolsToDownload.Clear();
                chunkCount++;
            }

            return urlAndFilenames;
        }*/

        public static void xxDownloadItems(Dictionary<string,string> symbols, string dateKey)
        {
            const int chunkSize = 20;

            var tempFilename = string.Format(FileNameTemplate, dateKey, chunkSize.ToString(), "");
            var folder = Path.GetDirectoryName(tempFilename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var symbolsToDownload = new List<string>();
            var count = 0;
            var chunkCount = 0;
            foreach (var kvp in symbols)
            {
                count++;
                symbolsToDownload.Add(kvp.Key);
                if (symbolsToDownload.Count == chunkSize)
                {
                    var filename = string.Format(FileNameTemplate, dateKey, chunkSize.ToString(), chunkCount.ToString());
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                        var o = WebClientExt.GetToBytes(url, true);
                        if (o.Item3 != null)
                            throw new Exception(
                                $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                        File.WriteAllBytes(filename, o.Item1);
                    }

                    Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbols.Count:N0}");

                    symbolsToDownload.Clear();
                    chunkCount++;
                }
            }

            if (symbolsToDownload.Count > 0)
            {
                var filename = string.Format(FileNameTemplate, dateKey, chunkSize.ToString(), chunkCount.ToString());
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                    var o = WebClientExt.GetToBytes(url, true);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                    File.WriteAllBytes(filename, o.Item1);
                }

                Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbols.Count:N0}");

                symbolsToDownload.Clear();
                chunkCount++;
            }

        }

        private static Dictionary<string, string> GetSymbolsToDownload()
        { // return Key=YahooSymbol, Value=PolygonSymbol
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null";
                //                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        private static List<string> GetSymbolsToDownload2()
        { 
            var data = new List<string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
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

        public class DownloadItem2 : WebClientExt.IDownloadToFileItem
        {
            public string Url { get; }
            public string Filename { get; }
            public HttpStatusCode? StatusCode { get; set; }

            public DownloadItem2(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }
        }

        public class DownloadItem : WebClientExt.IDownloadToMemoryItem
        {
            public string Url { get; }
            public byte[] Data { get; set; }
            // public VirtualFileEntry VirtualEntry { get; set; }
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

        private class DbItem
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
