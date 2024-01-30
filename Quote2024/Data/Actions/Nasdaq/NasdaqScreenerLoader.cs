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
        // Nasdaq screener history (from Feb 2, 2021): https://github.com/rreichel3/US-Stock-Symbols

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

                    var stockItems = new List<DbStockRow>();
                    var etfItems = new List<DbEtfRow>();

                    if (Path.GetExtension(entry.FullName) == ".json" && entry.Name.IndexOf("Etf", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        var oEtf = ZipUtils.DeserializeJson<cEtfRoot>(entry);
                        etfItems = oEtf.data.data.rows.Select(a=>new DbEtfRow(entry.LastWriteTime.DateTime, a)).ToList();
                    }
                    else if (Path.GetExtension(entry.FullName) == ".json" && entry.Name.IndexOf("Stock", StringComparison.InvariantCultureIgnoreCase) != -1)
                    {
                        var content = entry.GetContentOfZipEntry();

                        var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                        var exchange = ss[ss.Length - 2];
                        if (content.StartsWith("[")) // Github version
                        {
                            var oo = ZipUtils.DeserializeJson<cStockRow[]>(content);
                            stockItems.AddRange(oo.Select(a => new DbStockRow(exchange, entry.LastWriteTime.DateTime, a)));
                        }
                        else
                        {
                            var oo = ZipUtils.DeserializeJson<cStockRoot>(content);
                            stockItems.AddRange(oo.data.rows.Select(a=>new DbStockRow(exchange, entry.LastWriteTime.DateTime, a)));
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
                        DbUtils.ClearAndSaveToDbTable(stockItems, "dbQ2023Others..Bfr_ScreenerNasdaqStock", "Symbol", "Exchange",
                            "Name", "LastSale", "Volume", "NetChange", "Change", "MarketCap", "Country", "IpoYear",
                            "Sector", "Industry", "TimeStamp");
                        DbUtils.RunProcedure("dbQ2023Others..pUpdateScreenerNasdaqStock");
                    }

                    if (etfItems.Count > 0)
                    {
                        DbUtils.ClearAndSaveToDbTable(etfItems, "dbQ2023Others..Bfr_ScreenerNasdaqEtf", "Symbol", "Name",
                            "LastSale", "NetChange", "Change", "TimeStamp");
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
            public string symbol;
            public string name;
            public string lastsale;
            public string volume;
            public string netchange;
            public string pctchange;
            public string marketCap;
            public string country;
            public string ipoyear;
            public string sector;
            public string industry;
        }

        private class DbStockRow
        {
            // "Symbol", "Exchange", "Name", "LastSale", "Volume", "NetChange", "Change", "MarketCap", "Country", "IpoYear", "Sector", "Industry", "TimeStamp"
            private cStockRow Row;
            public string Symbol => Row.symbol;
            public string Exchange;
            public string Name => NullCheck(Row.name);
            public float? LastSale => Row.lastsale == "NA" ? (float?)null : float.Parse(Row.lastsale, NumberStyles.Any, culture);
            public float Volume => Convert.ToSingle(Math.Round(float.Parse(Row.volume, NumberStyles.Any, culture) / 1000000.0, 3));
            public float? NetChange => string.IsNullOrEmpty(Row.netchange) ? (float?)null : float.Parse(Row.netchange, NumberStyles.Any, culture);
            public float? Change => string.IsNullOrEmpty(Row.pctchange) ? (float?)null : float.Parse(Row.pctchange.Replace("%", ""), NumberStyles.Any, culture);
            public float? MarketCap => string.IsNullOrEmpty(Row.marketCap) ? (float?)null : Convert.ToSingle(Math.Round(float.Parse(Row.marketCap, NumberStyles.Any, culture) / 1000000.0, 3));
            public string Country => NullCheck(Row.country);
            public short? IpoYear => string.IsNullOrEmpty(Row.ipoyear) ? (short?)null : short.Parse(Row.ipoyear, NumberStyles.Any, culture);
            public string Sector => NullCheck(Row.sector);
            public string Industry => NullCheck(Row.industry);
            public DateTime TimeStamp;

            public DbStockRow(string exchange, DateTime timeStamp, cStockRow row)
            {
                Exchange = exchange;
                TimeStamp = timeStamp;
                Row = row;
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
            public string symbol;
            public string companyName;
            public string lastSalePrice;
            public string netChange;
            public string percentageChange;
        }

        private class DbEtfRow
        {
            // "symbol", "name", "LastSale", "netChange", "Change", "TimeStamp"
            private cEtfRow Row;
            public string Symbol => Row.symbol;
            public string Name=> NullCheck(Row.companyName);
            public float LastSale => float.Parse(Row.lastSalePrice, NumberStyles.Any, culture);
            public float NetChange => float.Parse(Row.netChange, NumberStyles.Any, culture);
            public float? Change => string.IsNullOrEmpty(Row.percentageChange) ? (float?)null : float.Parse(Row.percentageChange.Replace("%", ""), culture);
            public DateTime TimeStamp;

            public DbEtfRow(DateTime timeStamp, cEtfRow row)
            {
                Row = row;
                TimeStamp = timeStamp;
            }
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
