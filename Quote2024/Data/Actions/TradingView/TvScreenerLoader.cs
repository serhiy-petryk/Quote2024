using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;
// using Newtonsoft.Json;

namespace Data.Actions.TradingView
{
    public class TvScreenerLoader
    {
        private const string URL = @"https://scanner.tradingview.com/america/scan";
        private const string parameters = @"{""filter"":[{""left"":""exchange"",""operation"":""in_range"",""right"":[""AMEX"",""NASDAQ"",""NYSE""]}],""options"":{""lang"":""en""},""markets"":[""america""],""symbols"":{""query"":{""types"":[]},""tickers"":[]},""columns"":[""minmov"",""name"",""close"",""change"",""change_abs"",""Recommend.All"",""volume"",""Value.Traded"",""market_cap_basic"",""price_earnings_ttm"",""earnings_per_share_basic_ttm"",""number_of_employees"",""sector"",""industry"",""description"",""type"",""subtype""],""sort"":{""sortBy"":""name"",""sortOrder"":""asc""},""range"":[0,20000]}";

       /* public static void Start()
        {
            Logger.AddMessage($"Started");

            // Download
            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmm");
            var zipFileName = $@"E:\Quote\WebData\Screener\TradingView\TVScreener_{timeStamp}.zip";

            Logger.AddMessage($"Download data from {URL}");
            var o = Download.PostToString(URL, parameters);
            if (o is Exception ex)
                throw new Exception($"TvScreenerLoader: Error while download from {URL}. Error message: {ex.Message}");

            var entry = new VirtualFileEntry( $"{Path.GetFileNameWithoutExtension(zipFileName)}.json", (string)o);
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { entry });
            
            // Parse and save data to database
            Logger.AddMessage($"Parse and save to database");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var o = JsonConvert.DeserializeObject<ScreenerTradingView>(entry.GetContentOfZipEntry());
                    var items = o.data.Select(a => a.GetDbItem(entry.LastWriteTime.DateTime)).ToArray();

                    if (items.Length > 0)
                    {
                        DbUtils.ClearAndSaveToDbTable(items, "dbQ2023Others..Bfr_ScreenerTradingView", "Symbol", "Exchange", "Name",
                            "Type", "Subtype", "Sector", "Industry", "Close", "MarketCap", "Volume", "Recommend",
                            "TimeStamp");
                        DbUtils.RunProcedure("dbQ2023Others..pUpdateScreenerTradingView");
                    }

                    itemCount += items.Length;
                }

            return itemCount;
        }*/
    }
}
