using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using SpanJson.Formatters.Dynamic;

namespace Data.Actions.TradingView
{
    public class ObsoleteTvScreenerLoader
    {
        private const string URL = @"https://scanner.tradingview.com/america/scan";
        private const string parameters = @"{""filter"":[{""left"":""exchange"",""operation"":""in_range"",""right"":[""AMEX"",""NASDAQ"",""NYSE""]}],""options"":{""lang"":""en""},""markets"":[""america""],""symbols"":{""query"":{""types"":[]},""tickers"":[]},""columns"":[""minmov"",""name"",""close"",""change"",""change_abs"",""Recommend.All"",""volume"",""Value.Traded"",""market_cap_basic"",""price_earnings_ttm"",""earnings_per_share_basic_ttm"",""number_of_employees"",""sector"",""industry"",""description"",""type"",""subtype""],""sort"":{""sortBy"":""name"",""sortOrder"":""asc""},""range"":[0,20000]}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // Download
            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmm");
            var zipFileName = Settings.DataFolder + $@"Screener\TradingView\TVScreener_{timeStamp}.zip";

            Logger.AddMessage($"Download data from {URL}");
            var o = WebClientExt.PostToBytes(URL, parameters, true);
            if (o.Item3 is Exception ex)
                throw new Exception($"TvScreenerLoader: Error while download from {URL}. Error message: {ex.Message}");

            var entry = new VirtualFileEntry( $"{Path.GetFileNameWithoutExtension(zipFileName)}.json", o.Item1);
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
                    var o = ZipUtils.DeserializeZipEntry<JsonTvScreenerModel>(entry);
                    var items = o.data.Select(a => a.GetDbItem(entry.LastWriteTime.DateTime)).ToArray();

                    if (items.Length > 0)
                    {
                        DbHelper.ClearAndSaveToDbTable(items, "dbQ2023Others..Bfr_ScreenerTradingView", "Symbol", "Exchange", "Name",
                            "Type", "Subtype", "Sector", "Industry", "Close", "MarketCap", "Volume", "Recommend",
                            "TimeStamp");
                        DbHelper.RunProcedure("dbQ2023Others..pUpdateScreenerTradingView");
                    }

                    itemCount += items.Length;
                }

            return itemCount;
        }

        #region ========  Model classes  ==========

        public class JsonTvScreenerModel
        {
            public int totalCount;
            public ItemModel[] data;
        }

        public class ItemModel
        {
            public string s;
            public object[] d;

            public DbItemModel GetDbItem(DateTime timeStamp)
            {
                var symbol = GetString(d[1]);
                var ss = s.Split(':');
                if (!string.Equals(ss[1], symbol))
                    throw new Exception("Check 'Symbol' in ScreenerTradingView.Item.");

                var recommend = GetSingle(d[5]);
                var marketCap = GetSingle(d[8]);
                var item = new DbItemModel
                {
                    Exchange = ss[0],
                    Symbol = ss[1],
                    Name = GetString(d[14]),
                    Type = GetString(d[15]),
                    Subtype = GetString(d[16]),
                    Sector = GetString(d[12]),
                    Industry = GetString(d[13]),
                    Close = GetSingle(d[2]).Value,
                    Volume = Convert.ToSingle(Math.Round(GetSingle(d[6]).Value / 1000000.0, 3)),
                    MarketCap = marketCap.HasValue ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3)) : (float?)null,
                    Recommend = recommend.HasValue ? recommend.Value : (float?)null,
                    TimeStamp = timeStamp
                };
                return item;

                string GetString(object input)
                {
                    if (input == null) return null;

                    var span = (SpanJsonDynamic<Byte>)input;
                    var s = System.Text.Encoding.UTF8.GetString(span.Symbols);
                    if (!s.StartsWith('"') || !s.EndsWith('"'))
                        throw new Exception($"Invalid string in SpanJsonDynamic<Byte>: {input}");
                    s = s.Substring(1, s.Length - 2);
                    return string.IsNullOrWhiteSpace(s) ? null : s;
                }
                float? GetSingle(object input)
                {
                    if (input == null) return (float?)null;

                    var span = (SpanJsonDynamic<Byte>)input;
                    var s = System.Text.Encoding.UTF8.GetString(span.Symbols);
                    return string.IsNullOrWhiteSpace(s) ? (float?)null : float.Parse(s, CultureInfo.InvariantCulture);
                }
            }
        }

        public class DbItemModel
        {
            public string Exchange;
            public string Symbol;
            public string Name;
            public string Type;
            public string Subtype;
            public string Sector;
            public string Industry;
            public float Close;
            public float Volume;
            public float? MarketCap;
            public float? Recommend;
            public DateTime TimeStamp;
        }

        #endregion
    }
}
