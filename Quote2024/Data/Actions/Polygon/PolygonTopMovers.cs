using System;
using System.IO;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonTopMovers
    {
        private const string GainersUrlTemplate = @"https://api.massive.com/v2/snapshot/locale/us/markets/stocks/gainers";
        private const string LosersUrlTemplate = @"https://api.massive.com/v2/snapshot/locale/us/markets/stocks/losers";

        public static void Start()
        {
            var timeStamp = TimeHelper.GetTimeStamp();
            var folder = Settings.DataFolder + $@"Screener\PolygonTopMovers\{timeStamp.Item2}";
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var zipFileName = Path.Combine(folder, $"PolygonGainers_{timeStamp.Item1:yyyyMMddHHmmss}.zip");
            // api key doesn't support FMV (Fair market value): var url = UrlTemplate + "?apiKey=" + PolygonCommon.GetRealTimeApiKey2003();
            var url = GainersUrlTemplate + "?apiKey=" + PolygonCommon.GetApiKey2003();
            var o = WebClientExt.GetToBytes(url, true);
            if (o.Item3 != null)
                throw new Exception(
                    $"PolygonMarketMovers: Error while download from {url}. Error message: {o.Item3.Message}");

            var jsonFileName = Path.ChangeExtension(Path.GetFileName(zipFileName), "json");
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(jsonFileName, o.Item1) });

            zipFileName = Path.Combine(folder, $"PolygonLosers_{timeStamp.Item1:yyyyMMddHHmmss}.zip");
            // api key doesn't support FMV (Fair market value): var url = UrlTemplate + "?apiKey=" + PolygonCommon.GetRealTimeApiKey2003();
            url = LosersUrlTemplate + "?apiKey=" + PolygonCommon.GetApiKey2003();
            o = WebClientExt.GetToBytes(url, true);
            if (o.Item3 != null)
                throw new Exception(
                    $"PolygonMarketMovers: Error while download from {url}. Error message: {o.Item3.Message}");

            jsonFileName = Path.ChangeExtension(Path.GetFileName(zipFileName), "json");
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { new VirtualFileEntry(jsonFileName, o.Item1) });

            Logger.AddMessage($"Finished!");
        }
    }
}
