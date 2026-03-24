using System;
using System.IO;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonMarketSnapshot
    {
        private const string UrlTemplate = @"https://api.massive.com/v2/snapshot/locale/us/markets/stocks/tickers";

        public static void Download()
        {
            var timeStamp = TimeHelper.GetTimeStamp();
            var folder = Settings.DataFolder + $@"Screener\PolygonMarketSnapshot\{timeStamp.Item2}";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var zipFileName = Path.Combine(folder, $"PolygonMS_{timeStamp.Item1:yyyyMMddHHmmss}.zip");
            // api key doesn't support FMV (Fair market value): var url = UrlTemplate + "?apiKey=" + PolygonCommon.GetRealTimeApiKey2003();
            var url = UrlTemplate + "?apiKey=" + PolygonCommon.GetApiKey2003();
            var o = WebClientExt.GetToBytes(url, true);
            if (o.Item3 != null)
                throw new Exception(
                    $"PolygonMarketSnapshot: Error while download from {url}. Error message: {o.Item3.Message}");

            var jsonFileName = Path.ChangeExtension(Path.GetFileName(zipFileName), "json");
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(jsonFileName, o.Item1) });

            Logger.AddMessage($"Finished!");
        }
    }
}
