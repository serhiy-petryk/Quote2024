using System;
using System.Collections.Generic;
using System.IO;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonTradesLoader
    {
        private const string UrlTemplate = @"https://api.polygon.io/v3/trades/{0}?timestamp={1}&limit=5000";
        private const string DataFolder = @"E:\Quote\WebData\Trades\Polygon\Data";
        private static readonly string ZipFileNameTemplate = Path.Combine(DataFolder, "TradesPolygon_{1}_{0}.zip");

        public static void Download(string symbol, DateTime date)
        {
            var zipFileName = string.Format(ZipFileNameTemplate, symbol, date.ToString("yyyyMMdd"));
            var cnt = 0;
            var url = string.Format(UrlTemplate, date.ToString("yyyy-MM-dd"));
            var virtualFileEntries = new List<VirtualFileEntry>();
            while (url != null)
            {
                /*url = url + "&apiKey=" + PolygonCommon.GetApiKey2003();
                // var filename = folder + $@"\SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json";
                // Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(filename)} for {date:yyyy-MM-dd}");

                var o = Data.Helpers.Download.DownloadToBytes(url, true);
                if (o.Item2 != null)
                    throw new Exception($"PolygonSymbolsLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                var entry = new VirtualFileEntry(Path.Combine(Path.GetFileName(folder), $@"SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json"), o.Item1);
                virtualFileEntries.Add(entry);

                var oo = ZipUtils.DeserializeBytes<cRoot>(entry.Content);
                //File.WriteAllText(Path.Combine(folder, $@"SymbolsPolygon_{cnt:D2}_{date:yyyyMMdd}.json"), (byte[])o);

                //var oo = JsonConvert.DeserializeObject<cRoot>((string)o);
                if (oo.status != "OK")
                    throw new Exception($"Wrong status of response! Status is '{oo.status}'");

                url = oo.next_url;
                cnt++;*/
            }

        }

        private class cRoot
        {
            public cResult[] results;
            public string status;
            // public string request_id;
            public string next_url;
        }

        private class cResult
        {
            public byte[] conditions;
            public byte exchange;
            public long id;
            public long participant_timestamp;
            public float price;
            public int sequence_number;
            public long sip_timestamp;
            public int size;
            public byte type;
            public int trf_id;
            public long trf_timestamp;
        }
    }
}
