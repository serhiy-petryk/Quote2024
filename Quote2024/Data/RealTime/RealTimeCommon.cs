using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.RealTime
{
    public class RealTimeCommon
    {
        private const string DayPolygonDataFolder = @"E:\Quote\WebData\RealTime\YahooMinute\DayPolygon";

        public static List<string> GetTickerList(Action<string> fnShowStatus, int tradingDays, float minTradeValue,
            float maxTradeValue, int minTradeCount, float minClose, float maxClose)
        {
            Logger.AddMessage($"Get ticker list", fnShowStatus);

            var dates = Actions.Yahoo.YahooIndicesLoader.GetTradingDays(fnShowStatus,
                TimeHelper.GetCurrentEstDateTime().Date.AddDays(-1), tradingDays * 2 + 10);
            var sqls = new string[tradingDays];
            var cnt = 0;
            var data = new List<PolygonDailyLoader.cItem>();
            for (var k = 0; k < tradingDays; k++)
            {
                var date = dates[k];
                var zipFileName = Path.Combine(DayPolygonDataFolder, $"DayPolygon_{date:yyyyMMdd}.zip");
                if (!File.Exists(zipFileName))
                {
                    var url =
                        $@"https://api.polygon.io/v2/aggs/grouped/locale/us/market/stocks/{date:yyyy-MM-dd}?adjusted=false&apiKey={PolygonCommon.GetApiKey2003()}";
                    var o = Download.GetToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception(
                            $"PolygonDailyLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    var jsonFileName = $"DayPolygon_{date:yyyyMMdd}.json";
                    ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(jsonFileName, o.Item1) });
                }

                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var content = zip.Entries[0].GetContentOfZipEntry().Replace("{\"T\":\"", "{\"TT\":\""); // remove AmbiguousMatchException for original 't' and 'T' property names
                    var oo = ZipUtils.DeserializeString<PolygonDailyLoader.cRoot>(content);

                    if (oo.status != "OK" || oo.count != oo.queryCount || oo.count != oo.resultsCount || oo.adjusted || oo.resultsCount == 0)
                        throw new Exception($"Bad file: {zipFileName}");

                    var minTradeValue2 = (Settings.IsInMarketTime(date) ? minTradeValue / 2.0f : minTradeValue) * 1000000f;
                    var maxTradeValue2 = (Settings.IsInMarketTime(date) ? maxTradeValue / 2.0f : maxTradeValue) * 1000000f;
                    var minTradeCount2 = Settings.IsInMarketTime(date) ? minTradeCount / 2.0 : minTradeCount;

                    data.AddRange(oo.results.Where(a => a.c >= minClose && a.c <= maxClose && a.n >= minTradeCount && a.c * a.v >= minTradeValue2 && a.c * a.v <= maxTradeValue2));
                }
            }

            // Order by TradeCount DESC -> for YahooStreamer

            var tickers = data.GroupBy(a => a.TT).Where(a => a.Count() == tradingDays)
                .OrderByDescending(a => a.ToArray().Average(a1 => a1.TradeCount)).Select(a => a.Key).ToList();

            return tickers;
        }


    }
}
