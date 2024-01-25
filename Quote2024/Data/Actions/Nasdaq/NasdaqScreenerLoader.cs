using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Nasdaq
{
    public class NasdaqScreenerLoader
    {
        // Exchanges: 'AMEX','BAT','DUAL LISTED','NASDAQ','NASDAQ-CM','NASDAQ-GM','NASDAQ-GS','NYSE','PSE'
        // 'NASDAQ-CM','NASDAQ-GM','NASDAQ-GS' are part of 'NASDAQ'
        // 'PSE' is empty
        // private static readonly string[] Exchanges = new string[] { "AMEX", "NASDAQ", "NYSE", "BAT", "DUAL LISTED" };
        private static readonly string[] Exchanges = new string[] { "AMEX", "NASDAQ", "NYSE", "BAT" };

        private const string StockUrlTemplate = @"https://api.nasdaq.com/api/screener/stocks?tableonly=true&exchange={0}&download=true";
        private const string EtfUrl = @"https://api.nasdaq.com/api/screener/etf?tableonly=true&download=true";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var virtualFileEntries = new Dictionary<string, VirtualFileEntry>();

            // Download data
            foreach (var exchange in Exchanges)
            {
                var stockUrl = string.Format(StockUrlTemplate, exchange);
                Logger.AddMessage($"Download STOCK data for {exchange} from {stockUrl}");
                var o = Download.DownloadToString(stockUrl, true);
                if (o is Exception ex)
                    throw new Exception($"NasdaqScreenerLoader: Error while download from {StockUrlTemplate}. Error message: {ex.Message}");

                var entry = new VirtualFileEntry($"StockScreener_{exchange}_{timeStamp.Item2}.json", (string)o);
                virtualFileEntries.Add(entry.Name, entry);
            }

            Logger.AddMessage($"Download ETF data from {EtfUrl}");
            var o2 = Download.DownloadToString(EtfUrl, true);
            if (o2 is Exception ex2)
                throw new Exception($"NasdaqScreenerLoader: Error while download from {EtfUrl}. Error message: {ex2.Message}");

            var entry2 = new VirtualFileEntry($"EtfScreener_{timeStamp.Item2}.json", (string)o2);
            virtualFileEntries.Add(entry2.Name, entry2);

            // Zip data
            var zipFileName = $@"E:\Quote\WebData\Screener\Nasdaq\NasdaqScreener_{timeStamp.Item2}.zip";
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries.Values);

            // Parse and save data to database
            Logger.AddMessage($"Parse and save files to database");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    if (entry.LastWriteTime.DateTime < new DateTime(2021, 2, 6)) // 3 very old files have old symbol format => don't save in database
                        continue;

                    var stockItems = new List<cStockRow>();
                    var etfItems = new List<cEtfRow>();

                    if (Path.GetExtension(entry.FullName) == ".json" && entry.Name.IndexOf("Etf", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        var oEtf = ZipUtils.DeserializeJson<cEtfRoot>(entry);
                        etfItems = oEtf.data.data.rows.ToList();

                        foreach (var item in etfItems)
                            item.TimeStamp = entry.LastWriteTime.DateTime;
                    }
                    else if (Path.GetExtension(entry.FullName) == ".json" && entry.Name.IndexOf("Stock", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        var content = entry.GetContentOfZipEntry();

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var exchange = ss[ss.Length - 2];
                        if (content.StartsWith("[")) // Github version
                            stockItems = ZipUtils.DeserializeJson<cStockRow[]>(content).ToList();
                        else
                        {
                            var oo = ZipUtils.DeserializeJson<cStockRoot>(content);
                            stockItems.AddRange(oo.data.rows);
                        }

                        foreach (var item in stockItems)
                        {
                            item.Exchange = exchange;
                            item.TimeStamp = entry.LastWriteTime.DateTime;
                        }
                    }
                    else
                        throw new Exception($"NasdaqScreenerLoader.Parse. '{entry.FullName}' file is invalid in {zipFileName} zip file");

                    if (stockItems.Count > 0)
                    {
                        DbUtils.ClearAndSaveToDbTable(stockItems, "dbQ2023Others..Bfr_ScreenerNasdaqStock", "symbol", "Exchange",
                            "Name", "LastSale", "Volume", "netChange", "Change", "MarketCap", "Country", "ipoYear",
                            "Sector", "Industry", "TimeStamp");
                        DbUtils.RunProcedure("dbQ2023Others..pUpdateScreenerNasdaqStock");
                    }

                    if (etfItems.Count > 0)
                    {
                        DbUtils.ClearAndSaveToDbTable(etfItems, "dbQ2023Others..Bfr_ScreenerNasdaqEtf", "symbol", "Name",
                            "LastSale", "netChange", "Change", "TimeStamp");
                        DbUtils.RunProcedure("dbQ2023Others..pUpdateScreenerNasdaqEtf");
                    }
                    itemCount += stockItems.Count + etfItems.Count;

                }

            return itemCount;
        }

        #region =========  Json classes  ============
        //========================
        private static CultureInfo culture = new CultureInfo("en-US");
        private class cStockRoot
        {
            public cStockData data;
            public object message;
            public cStatus status;
        }
        private class cStockData
        {
            public cStockRow[] rows;
        }
        private class cStockRow
        {
            // "symbol", "name", "LastSale", "Volume", "netChange", "Change", "MarketCap", "country", "ipoYear", "sector", "industry", "timeStamp"
            // github: symbol, name, lastsale, volume, netchange, pctchange, marketCap, country, ipoyear, industry, sector, url
            public string symbol;
            public string name;
            public string lastSale;
            public long volume;
            public float? netChange;
            public string pctChange;
            public float? marketCap;
            public string country;
            public short? ipoYear;
            public string sector;
            public string industry;
            public DateTime TimeStamp;

            public string Exchange;
            public string Name => NullCheck(name);
            public float? LastSale => lastSale == "NA" ? (float?)null : float.Parse(lastSale, NumberStyles.Any, culture);
            public float Volume => Convert.ToSingle(Math.Round(volume / 1000000.0, 3));
            public float? Change => string.IsNullOrEmpty(pctChange) ? (float?)null : float.Parse(pctChange.Replace("%", ""), culture);
            public float? MarketCap => marketCap.HasValue
                ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3))
                : (float?)null;
            public string Country => NullCheck(country);
            public string Sector => NullCheck(sector);
            public string Industry => NullCheck(industry);

            public cStockRow() { }

            public cStockRow(string fileLine)
            {
                var ss = fileLine.Split(',');
                symbol = NullCheck(ss[0]);
                name = ss[1];
                lastSale = ss[2];
                marketCap = NullCheck(ss[5]) == null ? (long?)null : Convert.ToInt64(decimal.Parse(ss[5], culture));
                country = ss[6];
                ipoYear = NullCheck(ss[7]) == null ? (short?)null : short.Parse(ss[7]);
                volume = long.Parse(ss[8], culture);
                sector = ss[9];
                industry = ss[10];
            }
        }

        private static string NullCheck(string s) => string.IsNullOrEmpty(s) ? null : s.Trim();

        //========================
        private class cEtfRoot
        {
            public cEtfData data;
            public object message;
            public cStatus status;
        }
        private class cEtfData
        {
            public string dataAsOf;
            public cEtfData2 data;
        }
        private class cEtfData2
        {
            public cEtfRow[] rows;
        }
        private class cEtfRow
        {
            // "symbol", "name", "LastSale", "netChange", "Change", "TimeStamp"
            public string symbol;
            public string companyName;
            public string lastSalePrice;
            public float netChange;
            public string percentageChange;
            public DateTime TimeStamp;
            public string Name => NullCheck(companyName);
            public float LastSale => float.Parse(lastSalePrice, NumberStyles.Any, culture);
            public float? Change => string.IsNullOrEmpty(percentageChange) ? (float?)null : float.Parse(percentageChange.Replace("%", ""), culture);
        }

        //=====================
        private class cStatus
        {
            public int rCode;
            public object bCodeMessage;
            public string developerMessage;
        }

        #endregion
    }
}
