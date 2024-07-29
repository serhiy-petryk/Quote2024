using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public class MorningStarJsonProfileLoader
    {
        private const string UrlTemplate = @"https://www.morningstar.com/api/v2/stocks/{1}/{0}/quote";
        private const string FileTemplate = @"E:\Quote\WebData\Symbols\MorningStar\Profile\Data.JSON\MSProfiles_{0}\MSProfile_{1}_{2}.json";
        private const string FolderTemplate = @"E:\Quote\WebData\Symbols\MorningStar\Profile\Data.JSON\MSProfiles_{0}";

        public static void ParseJsonZipAll(List<WA_MorningStarSector.DbItem> data)
        {
            var folder = @"E:\Quote\WebData\Symbols\MorningStar\Data";
            var files = Directory.GetFiles(folder, "*json_*.zip");
            foreach (var file in files)
                ParseJsonZip(file, data);
        }

        private static void ParseJsonZip(string zipFileName, List<WA_MorningStarSector.DbItem> data)
        {
            var cnt = 0;
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    if (++cnt % 100 == 0)
                        Logger.AddMessage(
                            $"Parse data from {Path.GetFileName(zipFileName)}. Parsed {cnt:N0} from {zip.Entries.Count:N0} items.");

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var polygonSymbol = ss[ss.Length - 1];
                    // if (data.ContainsKey(polygonSymbol)) // May be the same tickers if exchanges are different
                       // continue;

                    var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(polygonSymbol);
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                    var fileTicker = oo.page?.ticker;
                    if (string.IsNullOrEmpty(fileTicker) || string.IsNullOrEmpty(sector))
                        continue;

                    var item = new WA_MorningStarSector.DbItem(fileTicker, oo.page.DbName, sector, entry.LastWriteTime.DateTime);
                    if (item.PolygonSymbol.Contains('^'))
                    {

                    }
                    if (string.IsNullOrEmpty(item.Name))
                        throw new Exception("Check MorningStar profile json parser");
                    data.Add(item);
                }
        }

        /* =====  OBSOOLETE  =======
        #region ========  Subclasses  =========
        public class MsSymbolToFileItem : WebClientExt.IDownloadToFileItem
        {
            public string PolygonSymbol;
            public string MsSymbol;
            public string Exchange;
            public string ExchangeForUrl; // some nasdaq symbols may be nyse symbols and otherwise
            public string Url => string.Format(UrlTemplate, MsSymbol.ToLower(), ExchangeForUrl.ToLower());
            public string Filename => string.Format(FileTemplate, DateKey, Exchange, PolygonSymbol);
            public HttpStatusCode? StatusCode { get; set; }

            public string DateKey;
            public DateTime Date => DateTime.ParseExact(DateKey, "yyyyMMdd", CultureInfo.InvariantCulture);
            public DateTime TimeStamp;

            public MsSymbolToFileItem(string polygonSymbol, string exchange, string dateKey)
            {
                PolygonSymbol = polygonSymbol;
                MsSymbol = MorningStarCommon.GetMorningStarProfileTicker(PolygonSymbol);
                Exchange = exchange;
                ExchangeForUrl = exchange;
                DateKey = dateKey;
            }
        }

        public class DbItem
        {
            public string PolygonSymbol;
            public string MsSymbol;
            public DateTime Date;
            public string Name;
            public string Sector;
            public DateTime TimeStamp;
        }
        #endregion

        public static void TestParse()
        {
            var folder = @"E:\Quote\WebData\Symbols\MorningStar\Profile\Data.JSON\MSProfiles_20240710.zip";
            ParseJsonZipAndSaveToDb(folder);
            Logger.AddMessage($"Finished");
        }

        public static async Task Start()
        {
            // Duration: 558 016 msecs for parallelBatchSize = 50 (948 348 msecs  for parallelBatchSize = 20)
            var sw = new Stopwatch();
            sw.Start();

            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var dataFolder = string.Format(Path.GetDirectoryName(FileTemplate), timeStamp.Item2);
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var symbolItems = GetSymbolsAndExchanges(timeStamp.Item2);
            await WebClientExt.DownloadItemsToFiles(symbolItems.Cast<WebClientExt.IDownloadToFileItem>().ToList(), 50);

            CheckFiles(symbolItems);

            await WebClientExt.DownloadItemsToFiles(symbolItems.Cast<WebClientExt.IDownloadToFileItem>().ToList(), 50);

            foreach (var item in symbolItems.Where(a => a.StatusCode != HttpStatusCode.OK))
                Debug.Print($"{item.StatusCode}:\t{item.Exchange}\t{item.PolygonSymbol}\t{item.MsSymbol}");
            
            // Save data to zip
            var zipFileName = dataFolder + ".zip";
            ZipUtils.CompressFolder(dataFolder, zipFileName);
            Directory.Delete(dataFolder, true);

            ParseJsonZipAndSaveToDb(zipFileName);

            var okItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.OK);
            var notFoundItems = symbolItems.Count(a => a.StatusCode == HttpStatusCode.NotFound);
            var serverErrorItems = symbolItems.Count(a => a.StatusCode != HttpStatusCode.OK && a.StatusCode != HttpStatusCode.NotFound);
            sw.Stop();

            Debug.Print($"SW: {sw.ElapsedMilliseconds:N0}");
            Helpers.Logger.AddMessage($"Finished. Items: {symbolItems.Count:N0}. Ok: {okItems:N0}. Not found: {notFoundItems:N0}. Server errors: {serverErrorItems:N0}. Duration: {sw.ElapsedMilliseconds:N0} msecs");
        }

        private static void ParseJsonZipAndSaveToDb(string zipFileName)
        {
            var data = new Dictionary<string, DbItem>();
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var polygonSymbol = ss[ss.Length - 1];
                    if (data.ContainsKey(polygonSymbol)) // May be the same tickers if exchanges are different
                        continue;

                    var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(polygonSymbol);
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    var sector = oo.components?.profile?.payload?.dataPoints?.sector?.value;
                    var fileTicker = oo.page?.ticker;
                    // Check fileTicker;
                    if (string.IsNullOrEmpty(fileTicker) || string.IsNullOrEmpty(sector)) continue;
                    if (!string.Equals(fileTicker, msSymbol.Replace("-", "")))
                    {
                        var secondMsSymbol = MorningStarCommon.GetSecondMorningStarProfileTicker(polygonSymbol);
                        if (string.IsNullOrEmpty(secondMsSymbol) || !string.Equals(secondMsSymbol, fileTicker, StringComparison.InvariantCultureIgnoreCase))
                            continue;
                    }

                    var dbItem = new DbItem
                    {
                        PolygonSymbol = polygonSymbol,
                        MsSymbol = fileTicker,
                        Date = dateKey,
                        Name = oo.page.DbName,
                        Sector = sector,
                        TimeStamp = entry.LastWriteTime.DateTime
                    };
                    data.Add(polygonSymbol, dbItem);
                }

            DbHelper.ClearAndSaveToDbTable(data.Values, "dbQ2024..Bfr_SectorMorningStar", "PolygonSymbol", "Date",
                "Name", "Sector", "TimeStamp", "MsSymbol");

        }

        public static void CheckFiles(List<MsSymbolToFileItem> originalItems)
        {
            foreach (var item in originalItems)
            {
                var secondMsSymbol = MorningStarCommon.GetSecondMorningStarProfileTicker(item.PolygonSymbol);
                if (item.StatusCode == HttpStatusCode.NotFound)
                {
                    if (!string.IsNullOrEmpty(secondMsSymbol) && !string.Equals(item.MsSymbol, secondMsSymbol, StringComparison.CurrentCultureIgnoreCase) &&
                        item.Exchange == item.ExchangeForUrl)
                    {
                        item.MsSymbol = secondMsSymbol;
                        item.StatusCode = null;
                        continue;
                    }

                    if (item.Exchange == item.ExchangeForUrl && (item.Exchange == "XNAS" || item.Exchange == "XNYS"))
                    {
                        item.ExchangeForUrl = item.Exchange == "XNAS" ? "XNYS" : "XNAS";
                        item.StatusCode = null;
                    }
                    //if (!string.IsNullOrEmpty(secondMsSymbol) && item.MsSymbol== secondMsSymbol && item.Exchange != item.ExchangeForUrl && (item.Exchange == "XNAS" || item.Exchange == "XNYS"))
                    //{
                      //  item.ExchangeForUrl = item.Exchange == "XNAS" ? "XNYS" : "XNAS";
                        //item.MsSymbol = MorningStarCommon.GetMorningStarProfileTicker(item.PolygonSymbol);
//                        item.StatusCode = null;
  //                      data.Add(item);
    //                }
                }
                else if (File.Exists(item.Filename))
                {
                    var oo = ZipUtils.DeserializeBytes<cRoot>(File.ReadAllBytes(item.Filename));
                    var oSector = oo.components?.profile?.payload?.dataPoints?.sector;
                    if (oSector == null)
                    {
                        File.Delete(item.Filename);
                        item.StatusCode = null;
                    }
                    else
                    {
                        var ticker = oo.page?.ticker;
                        if (string.IsNullOrEmpty(ticker))
                        {

                        }
                        else if (ticker != item.MsSymbol.Replace("-", ""))
                        {
                            if (!string.IsNullOrEmpty(secondMsSymbol))
                            {
                                if (!string.Equals(ticker, secondMsSymbol, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    item.MsSymbol = secondMsSymbol;
                                    item.StatusCode = null;
                                    continue;
                                }
                                else
                                {
                                    // OK
                                }
                            } 
                            else
                            {
                                item.StatusCode = HttpStatusCode.NotFound;
                                continue;
                            }
                        }

                        item.TimeStamp = File.GetLastWriteTime(item.Filename);
                        item.StatusCode = HttpStatusCode.OK;
                    }
                }
                else
                {
                    item.StatusCode = null;
                }
            }
        }

        private static List<MsSymbolToFileItem> GetSymbolsAndExchanges(string dateKey)
        {
            var data = new List<MsSymbolToFileItem>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') order by 2";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var item = new MsSymbolToFileItem((string)rdr["symbol"], (string)rdr["exchange"], dateKey);
                        if (!string.IsNullOrEmpty(item.MsSymbol)) data.Add(item);
                    }
            }

            return data;
        }
        */

        #region ===========  Json SubClasses  ===========
        public class cRoot
        {
            public cPage page;
            public cComponents components;
        }

        public class cPage
        {
            public string exchange;
            public string ticker;
            public string name;
            public string investmentType;
            public string DbName => string.IsNullOrEmpty(name) ? null : name;
        }
        public class cComponents
        {
            public cProfile profile;
        }
        public class cProfile
        {
            public cPayload payload;
        }
        public class cPayload
        {
            public cDataPoints dataPoints;
        }
        public class cDataPoints
        {
            public cSector sector;
        }
        public class cSector
        {
            public string name;
            public string value;
        }
        #endregion
    }
}
