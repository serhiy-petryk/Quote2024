using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonTopMovers
    {
        private const string GainersUrlTemplate = @"https://api.massive.com/v2/snapshot/locale/us/markets/stocks/gainers";
        private const string LosersUrlTemplate = @"https://api.massive.com/v2/snapshot/locale/us/markets/stocks/losers";

        public static void Download()
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

        public static void Parse()
        {
            var zipFileName =
                @"E:\Quote\WebData\Screener\PolygonTopMovers\20260320\PolygonGainers_20260320132310.zip";
            Debug.Print($"ticker\tChange\tfmv\tLastPrice\tVolume\tTrades");
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry().Replace("{\"P\":", "{\"p1\":").Replace(",\"S\":", ",\"s1\":"); // remove AmbiguousMatchException for original 'P/St' and 'p/s' properties names
                    var oo = ZipUtils.DeserializeString<cRoot>(content);
                    foreach (var item in oo.tickers)
                    {
                        Debug.Print($"{item.ticker}\t{item.todaysChange}\t{item.fmv}\t{item.min.c}\t{item.min.v}\t{item.min.n}");
                    }
                }
        }
        public static void ParseFolder()
        {
            // Result for 2026-03-20 for Losers: 7 tickers with last price>5.0 and trades/minute>60 = 3 go down + 4 not go done
            var folder = @"E:\Quote\WebData\Screener\PolygonTopMovers\20260320";
            // var files = Directory.GetFiles(folder, "PolygonGainers_*.zip")
            var files = Directory.GetFiles(folder, "PolygonLos*.zip")
                .OrderBy(Path.GetFileNameWithoutExtension).ToArray();

            var data = new Dictionary<string, Dictionary<TimeSpan, List<cTicker>>>();
            foreach (var file in files)
            {
                var filename = Path.GetFileNameWithoutExtension(file);
                var sTime = filename.Substring(filename.Length - 6);
                var time = TimeSpan.ParseExact(sTime, "hhmmss", CultureInfo.InvariantCulture);
                if (time<new TimeSpan(9,40,0) || time>new TimeSpan(15,30,0)) continue;

                using (var zip = ZipFile.Open(file, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var content = entry.GetContentOfZipEntry().Replace("{\"P\":", "{\"p1\":").Replace(",\"S\":", ",\"s1\":"); // remove AmbiguousMatchException for original 'P/St' and 'p/s' properties names
                        var oo = ZipUtils.DeserializeString<cRoot>(content);
                        foreach (var item in oo.tickers)
                        {
                            if (!data.ContainsKey(item.ticker))
                                data.Add(item.ticker, new Dictionary<TimeSpan, List<cTicker>>());

                            var timeData = data[item.ticker];
                            if (!timeData.ContainsKey(time))
                                timeData.Add(time, new List<cTicker>());
                            timeData[time].Add(item);

                            // Debug.Print($"{item.ticker}\t{item.todaysChange}\t{item.fmv}\t{item.min.c}\t{item.min.v}\t{item.min.n}");
                        }
                    }
            }

            Debug.Print($"ticker\tTime\tChange\tfmv\tLastPrice\tVolume\tTrades");

            foreach (var kvp1 in data)
            {
                var minTime = kvp1.Value.Min(a => a.Key);
                var item = kvp1.Value[minTime][0];
                Debug.Print($"{item.ticker}\t{minTime:hh\\:mm}\t{item.todaysChange}\t{item.fmv}\t{item.min.c}\t{item.min.v}\t{item.min.n}");
            }
        }

        public class cRoot
        {
            public cTicker[] tickers;
            public string status;
            public string request_id;
        }
        public class cTicker
        {
            public string ticker;
            public float todaysChangePerc;
            public float todaysChange;
            public long updated;
            public float fmv;
            public cDay day;
            public cLastQuote lastQuote;
            public cLastTrade lastTrade;
            public cLastMinute min;
            public cPrevDay prevDay;
            public DateTime dUpdated => TimeHelper.GetEstDateTimeFromUnixMilliseconds(updated/1000000);
        }

        public class cDay
        {
            public string dv;
            public float o;
            public float h;
            public float l;
            public float c;
            public float v;
            public float vw;
        }
        public class cLastQuote
        {
            public float p1;
            public int s1;
            public float p;
            public int s;
            public long t;
            public DateTime dTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(t / 1000000);
        }
        public class cLastTrade
        {
            public string i;
            public float p;
            public int s;
            public long t;
            public int x;
            public string ds;
            public DateTime dTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(t / 1000000);
        }
        public class cLastMinute
        {
            public string dv;
            public string dav;
            public long av;
            public long t;
            public int n;
            public float o;
            public float h;
            public float l;
            public float c;
            public float v;
            public float vw;
            public DateTime dTime => TimeHelper.GetEstDateTimeFromUnixMilliseconds(t);
        }
        public class cPrevDay
        {
            public float o;
            public float h;
            public float l;
            public float c;
            public float v;
            public float vw;
        }
    }
}
