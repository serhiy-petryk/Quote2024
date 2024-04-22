using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using ProtoBuf;

namespace Data.Actions.Polygon
{
    public static class PolygonTradesLoader
    {
        private const string UrlTemplate = @"https://api.polygon.io/v3/trades/{0}?timestamp={1}&limit=50000";
        private const string DataFolder = @"E:\Quote\WebData\Trades\Polygon\Data";
        private static readonly string ZipFileNameTemplate = Path.Combine(DataFolder, @"{2}\TradesPolygon_{1}_{0}.zip");

        public static void Test()
        {
            var items = 0;
            var seqCount = 0;
            var seqCount1 = 0;
            var seqCount2 = 0;
            var sizes = new Dictionary<int, int>();
            var conditions = new Dictionary<byte, int> { { 0, 0 } };
            var sConditions = new Dictionary<string, int> { { "", 0 } };
            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                //var zipFileName = @"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05\TradesPolygon_20240405_AA.zip";
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);

                        items += oo.results.Length;
                        var time = oo.results.Max(a => a.sip_timestamp - a.participant_timestamp);
                        var time1 = oo.results.Min(a => a.sip_timestamp - a.participant_timestamp);

                        var lastSeq = int.MaxValue;
                        foreach (var item in oo.results)
                        {
                            if (lastSeq < item.sequence_number)
                            {
                                seqCount1++;
                            }

                            lastSeq = item.sequence_number;

                            if (item.conditions == null)
                            {
                              conditions[0]++;
                              sConditions[""]++;

                            }
                            else
                            {
                                foreach (var c in item.conditions)
                                {
                                    if (!conditions.ContainsKey(c))
                                        conditions.Add(c, 0);
                                    conditions[c]++;
                                }

                                var key = $"{item.conditions.Length};" + string.Join(',', item.conditions.OrderBy(a=>a).Select(a=>a.ToString()));
                                if (!sConditions.ContainsKey(key))
                                    sConditions.Add(key, 0);
                                sConditions[key]++;
                            }
                        }

                        /*var oo1 = oo.results
                            .Where(a => a.ParticipantTime > new TimeSpan(9, 30, 0) &&
                                        a.ParticipantTime < new TimeSpan(16, 00, 0))
                            .OrderByDescending(a => a.sip_timestamp - a.participant_timestamp).ToArray();*/
                        var oo2 = oo.results.OrderBy(a => a.sequence_number).ToArray();

                        foreach (var item in oo2)
                        {
                            var size = Convert.ToInt32(item.size);
                            if (!sizes.ContainsKey(size))
                                sizes.Add(size, 0);
                            sizes[size]++;
                        }

                        var lastSize = 0;
                        long lastTimestamp = 0;
                        foreach (var item in oo2)
                        {
                            if (item.sip_timestamp < lastTimestamp)
                            {
                                seqCount++;
                                Debug.Print($"Bad seq: {item.SipTime}");
                            }

                            if (Convert.ToInt32(item.size) == lastSize && lastSize!=100 && lastSize!=1)
                            {
                                seqCount2++;
                            }

                            lastSize = Convert.ToInt32(item.size);
                            lastTimestamp = item.sip_timestamp;
                        }
                    }
            }

            var aa1 = sizes.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa21 = conditions.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa22 = sConditions.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

        public static void Start()
        {
            var date = new DateTime(2024, 04, 5);
            var symbols = Actions.Polygon.PolygonCommon.GetSymbolsForStrategies(date);

            foreach (var symbol in symbols)
                Download(symbol, new DateTime(2024, 4, 5));

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
                            $"PolygonTradesLoader: Error while download from {url}. Error message: {o.Item2.Message}");

                    var entry = new VirtualFileEntry($@"PolygonTrades_{cnt:D2}_{date:yyyyMMdd}.json", o.Item1);
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

        [ProtoContract]
        public class cResult
        {
            [ProtoMember(3)]
            public int Price2;

            [ProtoMember(2)]
            public int Size2;

            [ProtoMember(1)]
            public ushort Seconds2;

            [ProtoMember(4)]
            public byte[] conditions;
            public byte exchange;
            public string id;
            public long participant_timestamp;
            public float price;
            public int sequence_number;
            public long sip_timestamp; // syncronized with sequence_number
            public double size;
            public byte type;
            public int trf_id;
            public long trf_timestamp;

            public DateTime ParticipantDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000);
            public DateTime SipDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000);
            public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);
            public TimeSpan ParticipantTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(participant_timestamp / 1000000).TimeOfDay;
            public TimeSpan SipTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(sip_timestamp / 1000000).TimeOfDay;
            // public DateTime TrfDt => TimeHelper.GetEstDateTimeFromUnixMilliseconds(trf_timestamp / 1000000);
        }
    }
}
