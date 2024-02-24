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
        private const string JsonDataFolder = @"E:\Quote\WebData\Screener\MorningStar\Json";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // DownloadAndSaveToZip();
            ParseJson(@"E:\Quote\WebData\Screener\MorningStar\Data\MSS_20240224.zip");

            Logger.AddMessage($"Finished!");
        }

        private static void ParseJson(string zipFileName)
        {
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var data = new Dictionary<string, Dictionary<string, DbItem>>();
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var sector = MorningStarCommon.GetSectorName(ss[ss.Length - 2]);
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in oo.results)
                    {
                        var exchange = item.meta.exchange;
                        var symbol = item.meta.ticker;
                        var name = item.fields.name.value;

                        if (string.IsNullOrEmpty(symbol) && name == "Nano Labs Ltd Ordinary Shares - Class A")
                            symbol = "NA";

                        if (!MorningStarCommon.Exchanges.Any(exchange.Contains)) continue;
                        if (string.IsNullOrEmpty(symbol))
                            throw new Exception($"No ticker code. Please, check. Security name is {name}. File: {entry.Name}");

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
                        {

                        }
                    }
                }

            var itemsToSave = data.Values.SelectMany(a => a.Values).ToArray();
            DbUtils.ClearAndSaveToDbTable(itemsToSave, "dbQ2023Others..Bfr_ScreenerMorningStar", "Symbol", "Date",
                "Exchange", "Sector", "Name", "TimeStamp");
        }

        private static void DownloadAndSaveToZip()
        {
            var dateKey = DateTime.Now.AddHours(-8).Date.ToString("yyyyMMdd");
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = Path.Combine(DataFolder, $"MSS_{dateKey}.zip");
            var entryNameTemplate = Path.Combine(Path.GetFileNameWithoutExtension(zipFileName), $"MSS_{dateKey}_" + "{0}_{1}.json");

            foreach (var sector in MorningStarCommon.Sectors)
            {
                Logger.AddMessage($"Process {sector} sector");

                byte[] bytes;
                var filename = JsonDataFolder + $@"\{sector}_0.json";
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, sector, 1);
                    var o = Helpers.Download.DownloadToBytes(url, true);
                    if (o is Exception ex)
                        throw new Exception(
                            $"MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                    bytes = (byte[])o;
                    File.WriteAllBytes(filename, bytes);
                }
                else
                    bytes = File.ReadAllBytes(filename);

                var entryName = string.Format(entryNameTemplate, sector, "0");
                var entry = new VirtualFileEntry(entryName, bytes);
                virtualFileEntries.Add(entry);

                var oo = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(bytes);

                for (var k = 1; k < oo.pagination.totalPages; k++)
                {
                    filename = JsonDataFolder + $@"\{sector}_{k}.json";
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, sector, k + 1);
                        var o = Helpers.Download.DownloadToBytes(url, true);
                        if (o is Exception ex)
                            throw new Exception(
                                $"MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                        bytes = (byte[])o;
                        File.WriteAllBytes(filename, bytes);
                    }
                    else
                        bytes = File.ReadAllBytes(filename);

                    entryName = string.Format(entryNameTemplate, sector, k.ToString());
                    entry = new VirtualFileEntry(entryName, bytes);
                    virtualFileEntries.Add(entry);
                }
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
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
