using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Data.Helpers;

namespace Data.Actions.Chartmill
{
    public class ObsoleteChartmillScreenerLoader
    {
        private static CookieCollection cookies;

        private const string Url = @"https://www.chartmill.com/chartmill-rest/screener";
        // private const string parameters2 = @"{""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":null,""sectors"":""12"",""loadWatchListId"":null,""start"":20}";
        // private const string parameters = @"{""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":null,""sectors"":""1,12,36,79,124,146,165,193,216,234,247"",""loadWatchListId"":null,""start"":40}";
        // at date: {"sort":"ticker","sortDir":"ASC","tickers":null,"conditions":null,"exchanges":"125,119,3","indexes":null,"types":null,"date":"2021-03-05","sectors":"1,12,36,79,124,146,165,193,216,234,247","loadWatchListId":null,"start":20}
        // all us tickers(10816): {""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":null,""sectors"":null,""loadWatchListId"":null,""start"":20}
        // with date: @"{""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":""2023-01-06"",""sectors"":null,""loadWatchListId"":null,""start"":{0}}";
        private const string ParameterTemplate = @"{""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":null,""sectors"":null,""loadWatchListId"":null,""start"":{0}}";
        private const string ParameterTemplateWithDate = @"{""sort"":""ticker"",""sortDir"":""ASC"",""tickers"":null,""conditions"":null,""exchanges"":""125,119,3"",""indexes"":null,""types"":null,""date"":""{1}"",""sectors"":null,""loadWatchListId"":null,""start"":{0}}";
        // 2020-01-03,2020-04-03,2020-07-10,2020-10-02,2021-01-08,2021-04-09,2021-07-02,2021-10-01,2022-01-07,2022-01-07,2022-04-01,2022-07-01,2022-10-07,2023-01-06,2023-03-31,2023-07-07,2023-10-02,2024-01-02,

        private const string DataFolder = Settings.DataFolder + @"Screener\Chartmill\Data";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            Logger.AddMessage($"Get cookies ..");
            if (cookies == null)
            {
                var url = @"https://www.chartmill.com/chartmill-rest/auth/login";
                var userAndPassword = Data.Helpers.CsUtils.GetApiKeys("chartmill.com")[0].Split('^');
                var parameters = $"{{\"username\":\"{userAndPassword[0]}\",\"password\":\"{userAndPassword[1]}\"}}";
                var o = WebClientExt.PostToBytes(url, parameters, false, false, "application/json");
                if (o.Item3 is Exception ex)
                    throw new Exception(
                        $"ChartmillScreenerLoader: Can't get cookies from {url}. Error message: {o.Item3.Message}");
                cookies = o.Item2;
            }

            Logger.AddMessage($"Data is downloading ..");
            var zipFileName = DownloadAndSaveToZip(null);

            // Parse and save data to database
            Logger.AddMessage($"Parse and save to database");
            var itemCount = ParseJsonAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static void DownloadAndSaveToZipHistory()
        {
            Logger.AddMessage($"Started");

            var dates =
                "2020-01-03,2020-04-03,2020-07-10,2020-10-02,2021-01-08,2021-04-09,2021-07-02,2021-10-01,2022-01-07,2022-04-01,2022-07-01,2022-10-07,2023-01-06,2023-03-31,2023-07-07,2023-10-02,2024-01-02"
                    .Split(',').Select(a => DateTime.ParseExact(a, "yyyy-MM-dd", CultureInfo.InvariantCulture)).ToArray();

            foreach (var date in dates)
            {
                Logger.AddMessage($"Data is downloading for {date:yyyy-MM-dd} ..");
                var zipFileName = DownloadAndSaveToZip(date);
            }
            Logger.AddMessage($"!Finished.");
        }

        public static void ParseAllZipFiles()
        {
            Logger.AddMessage($"Started");

            var zipFileNames = Directory.GetFiles(DataFolder, "*.zip");
            foreach (var zipFileName in zipFileNames)
            {
                Logger.AddMessage($"Parse {Path.GetFileName(zipFileName)}");
                ParseJsonAndSaveToDb(zipFileName);
            }
            Logger.AddMessage($"!Finished.");
        }

        private static int ParseJsonAndSaveToDb(string zipFileName)
        {
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            var data = new Dictionary<string, DbItem>();
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in oo.result)
                    {
                        var dbItem = new DbItem()
                        {
                            Symbol = item.ticker,
                            Date = dateKey,
                            Exchange = item.exchange.shortName,
                            Sector = item.gicsSubIndustry?.sector.name,
                            Industry = item.gicsSubIndustry?.name,
                            Name = string.IsNullOrWhiteSpace(item.name) ? null: item.name,
                            ISIN = string.IsNullOrWhiteSpace(item.isin) ? null : item.isin,
                            Country = item.country,
                            Type = item.type,
                            Etf = item.etf ? true : null
                        };
                        data.Add(dbItem.Symbol, dbItem);
                    }
                }

            DbHelper.ClearAndSaveToDbTable(data.Values, "dbQ2023Others..Bfr_ScreenerChartmill", "Symbol", "Date",
                "Exchange", "Sector", "Industry", "Name", "ISIN", "Country", "Type", "Etf");

            DbHelper.RunProcedure("dbQ2023Others..pUpdateScreenerChartmill");

            return data.Count;
        }

        private static string DownloadAndSaveToZip(DateTime? date)
        {
            var dateKey = (date ?? DateTime.Now.AddHours(-8).Date).ToString("yyyyMMdd");
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = Path.Combine(DataFolder, $"Chartmill_{dateKey}.zip");
            var entryNameTemplate = Path.Combine(Path.GetFileNameWithoutExtension(zipFileName), $"Chartmill_{dateKey}_" + "{0}.json");

            var from = 0;
            var rows = 0;
            var count = 0;
            while (from == 0 || from < rows)
            {
                if (count % 10 == 0 && count != 0)
                    Logger.AddMessage($"Downloaded {from} rows from {rows}");

                var parameters = date.HasValue
                    ? ParameterTemplateWithDate.Replace("{0}", from.ToString())
                        .Replace("{1}", date.Value.ToString("yyyy-MM-dd"))
                    : ParameterTemplate.Replace("{0}", from.ToString());

                var o = WebClientExt.PostToBytes(Url, parameters, false, false, "application/json", cookies);
                if (o.Item3 is Exception ex)
                    throw new Exception(
                        $"ChartmillScreenerLoader: Error while download from {Url}. Error message: {ex.Message}");

                var entryName = string.Format(entryNameTemplate, from.ToString());
                var entry = new VirtualFileEntry(entryName, o.Item1);
                virtualFileEntries.Add(entry);

                var oo = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(o.Item1);
                rows = oo.total;
                from = oo.from + oo.result.Length;

                if (rows == 0)
                    throw new Exception("ChartmillScreenerLoader. No data downloaded");
                count++;
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
            return zipFileName;
        }


        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Exchange;
            public string Sector;
            public string Industry;
            public string Name;
            public string ISIN;
            public byte? Country;
            public byte? Type;
            public bool? Etf;
        }

        private class cRoot
        {
            public int total;
            public int from;
            public string error;
            public cResult[] result;
        }

        private class cResult
        {
            public int id;
            public string isin;
            public cExchange exchange;
            public string ticker;
            public string name;
            public byte? country;
            public cGicsSubIndustry gicsSubIndustry;
            public bool etf;
            public byte? type; // ?1-ADR, 3-
        }

        private class cExchange
        {
            public string shortName;
        }

        public class cGicsSubIndustry
        {
            public string name;
            public cSector sector;
        }
        public class cSector
        {
            public string name;
        }
        #endregion

    }
}
