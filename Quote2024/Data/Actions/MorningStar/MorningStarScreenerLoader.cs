using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public static class MorningStarScreenerLoader
    {
        // page numeration from 1
        // private const string UrlTemplate = @"https://www.morningstar.com/api/v2/navigation-list/{0}?sort=marketCap:desc&page={1}&limit=50";
        private const string UrlTemplate = @"https://www.morningstar.com/api/v2/navigation-list/{0}?sort=ticker&page={1}&limit=50";
        private const string DataFolder = @"E:\Quote\WebData\Screener\MorningStar\Data";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var zipFileName = DownloadAndSaveToZip();
            ParseJsonAndSaveToDb(zipFileName);

            Logger.AddMessage($"Finished!");
        }

        public static void ParseJsonAndSaveToDbAllFiles()
        {
            var files = Directory.GetFiles(DataFolder, "*.zip").OrderBy(a => a).ToArray();
            foreach(var file in files)
                ParseJsonAndSaveToDb(file);
        }

        private static void ParseJsonAndSaveToDb(string zipFileName)
        {
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var data = new Dictionary<string, Dictionary<string, DbItem>>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var sector = MorningStarCommon.GetSectorName(ss[ss.Length - 2]);
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in oo.results)
                    {
                        var exchange = item.meta.exchange;
                        var originalSymbol = item.meta.ticker;
                        var name = item.fields.name.value;

                        var symbol = originalSymbol;
                        if (string.IsNullOrWhiteSpace(symbol) && name == "Nano Labs Ltd Ordinary Shares - Class A")
                            symbol = "NA";

                        if (!MorningStarCommon.Exchanges.Any(exchange.Contains)) continue;
                        if (symbol == "SANP1") continue;

                        if (string.IsNullOrWhiteSpace(symbol))
                            throw new Exception($"No ticker code. Please, check. Security name is {name}. File: {entry.Name}");
                        if (string.IsNullOrWhiteSpace(name))
                            throw new Exception($"Name of '{originalSymbol}' ticker is blank. Please, adjust structure (Bfr_)ScreenerMorningStar tables in database. File: {entry.Name}");
                        symbol = MorningStarCommon.GetMyTicker(symbol);

                        if (!data.ContainsKey(exchange))
                            data.Add(exchange, new Dictionary<string, DbItem>());
                        var dbItems = data[exchange];
                        if (!dbItems.ContainsKey(symbol))
                        {
                            var dbItem = new DbItem()
                            {
                                Symbol = symbol,
                                Date = dateKey,
                                Exchange = exchange,
                                Sector = sector,
                                Name = name,
                                TimeStamp = DateTime.Now
                            };
                            dbItems.Add(symbol, dbItem);
                        }
                        else
                            throw new Exception($"'{originalSymbol}' Ticker is repeating. Please, check source data in {entry.Name}");
                    }
                }

            var badTickers = MorningStarCommon.BadTickers;
            if (badTickers.Count > 0)
                throw new Exception($"There are {badTickers.Count} bad morning star tickers. Please, check MorningStarMethod.GetMyTicker");

            var itemsToSave = data.Values.SelectMany(a => a.Values).ToArray();
            DbHelper.ClearAndSaveToDbTable(itemsToSave, "dbQ2023Others..Bfr_ScreenerMorningStar", "Symbol", "Date",
                "Exchange", "Sector", "Name", "TimeStamp");

            DbHelper.RunProcedure("dbQ2023Others..pUpdateScreenerMorningStar");
        }

        private static string DownloadAndSaveToZip()
        {
            var dateKey = DateTime.Now.AddHours(-8).Date.ToString("yyyyMMdd");
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = Path.Combine(DataFolder, $"MSS_{dateKey}.zip");
            var entryNameTemplate = Path.Combine(Path.GetFileNameWithoutExtension(zipFileName), $"MSS_{dateKey}_" + "{0}_{1}.json");

            foreach (var sector in MorningStarCommon.Sectors)
            {
                Logger.AddMessage($"Process {sector} sector");

                var url = string.Format(UrlTemplate, sector, 1);
                var o = Helpers.Download.DownloadToBytes(url, true);
                if (o.Item2 != null)
                    throw new Exception($"MorningStarScreenerLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                var entryName = string.Format(entryNameTemplate, sector, "0");
                var entry = new VirtualFileEntry(entryName, o.Item1);
                virtualFileEntries.Add(entry);

                var oo = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(o.Item1);

                for (var k = 1; k < oo.pagination.totalPages; k++)
                {
                    if (k % 10 == 0)
                        Logger.AddMessage($"Process {sector} sector. Downloaded {k} pages from {oo.pagination.totalPages}");

                    url = string.Format(UrlTemplate, sector, k + 1);
                    o = Helpers.Download.DownloadToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception($"MorningStarScreenerLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    entryName = string.Format(entryNameTemplate, sector, k.ToString());
                    entry = new VirtualFileEntry(entryName, o.Item1);
                    virtualFileEntries.Add(entry);
                }
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
            return zipFileName;
        }

        #region ============  Json classes  ==============

        private class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime TimeStamp;
        }

        private class cRoot
        {
            public cPagination pagination;
            public cResult[] results;
        }

        private class cPagination
        {
            public int count;
            public string page;
            public int totalCount;
            public int totalPages;
        }

        private class cResult
        {
            public cMeta meta;
            public cFields fields;
        }

        private class cMeta
        {
            public string securityID;
            public string performanceID;
            public string companyID;
            public string universe;
            public string exchange;
            public string ticker;
        }

        private class cFields
        {
            public cStringValue name;
            public cStringValue ticker;
        }

        private class cStringValue
        {
            public string value;
        }
        #endregion
    }
}
