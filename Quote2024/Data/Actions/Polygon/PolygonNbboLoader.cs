using System;
using System.Collections.Generic;
using System.IO;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonNbboLoader
    {
        private const string UrlTemplate = @"https://api.polygon.io/v3/quotes/{0}?timestamp={1}&limit=50000";
        private const string DataFolder = @"E:\Quote\WebData\Trades\PolygonNbbo\Data";
        private static readonly string ZipFileNameTemplate = Path.Combine(DataFolder, @"{2}\PolygonNbbo_{1}_{0}.zip");

        public static void Start()
        {
            var date = new DateTime(2024, 04, 5);
            var tickerAndDateAndHighToLow = Actions.Polygon.PolygonCommon.GetSymbolsAndHighToLowForStrategies(new [] {date});

            foreach (var symbol in tickerAndDateAndHighToLow.Keys)
                Download(symbol, date);

            Logger.AddMessage($"Finished!");
        }

        public static void Download(string symbol, DateTime date)
        {
            var zipFileName = string.Format(ZipFileNameTemplate, symbol, date.ToString("yyyyMMdd"),
                date.ToString("yyyy-MM-dd"));
            var folder = Path.GetDirectoryName(zipFileName);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var cnt = 0;
            var url = string.Format(UrlTemplate, symbol, date.ToString("yyyy-MM-dd"));
            var virtualFileEntries = new List<VirtualFileEntry>();
            if (!File.Exists(zipFileName))
            {
                while (url != null)
                {
                    Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(zipFileName)} for {symbol}/{date:yyyy-MM-dd}");

                    url = url + "&apiKey=" + PolygonCommon.GetApiKey2003();

                    var o = Data.Helpers.Download.DownloadToBytes(url, true);
                    if (o.Item2 != null)
                        throw new Exception(
                            $"PolygonNbboLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    var entry = new VirtualFileEntry($@"NbboPolygon_{cnt:D2}_{date:yyyyMMdd}.json", o.Item1);
                    virtualFileEntries.Add(entry);

                    var oo = ZipUtils.DeserializeBytes<cRoot>(entry.Content);

                    if (oo.status != "OK")
                        throw new Exception($"Wrong status of response! Status is '{oo.status}'");

                    url = oo.next_url;
                    cnt++;
                }

                // Save to zip file
                ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
            }
        }

        public class cRoot
        {
            public cResult[] results;
            public string status;
            // public string request_id;
            public string next_url;
        }

        public class cResult
        {
            public byte ask_exchange;
            public float ask_price;
            public double ask_size;
            public byte bid_exchange;
            public float bid_price;
            public double bid_size;

            public byte[] conditions;
            public ushort[] indicators; // ?? ushort
            public long participant_timestamp;
            public int sequence_number;
            public long sip_timestamp; // ??? for trades: syncronized with sequence_number
            public byte tape;

            public long trf_timestamp; // ??
            public DateTime SipDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000);
            public TimeSpan SipTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000).TimeOfDay;

            /*            public DateTime ParticipantDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000);
                        public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);
                        public TimeSpan ParticipantTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000).TimeOfDay;
                        // public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);*/
        }
    }
}
