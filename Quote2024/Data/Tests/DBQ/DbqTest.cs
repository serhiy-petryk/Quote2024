using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Tests.DBQ
{
    public static class DbqTest
    {
        public static void InvestigateNewCompressor()    // 110 358 milliseconds (83 368 milliseconds - no actions)
        {
            var files = Directory.GetFiles(Settings.DataFolder + @"Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            var cnt1 = 0;
            var cnt2 = 0;
            var cnt3 = 0;
            var cnt4 = 0;
            var times = new Dictionary<int, int>();
            var prices = new Dictionary<int, int>();
            var volumes = new Dictionary<int, int>();
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var results = new List<PolygonTradesLoader.cResult>();
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonTradesLoader.cRoot>(entry);
                        results.AddRange(oo.results);
                    }

                    recCount += results.Count;

                    var lastTime = Convert.ToInt32(results[0].sip_timestamp / 1000000000);
                    var lastPrice = Convert.ToInt32(results[0].price * 10000);
                    var lastVolume = Convert.ToInt32(results[0].size);

                    var itemCount = 0;
                    foreach (var o in results)
                    {
                        var time = Convert.ToInt32(o.sip_timestamp / 1000000000);
                        var price = Convert.ToInt32(o.price * 10000);
                        var volume = Convert.ToInt32(o.size);

                        if (price == lastPrice && volume == lastVolume && time == lastTime)
                        { // prefix = 0x110
                            cnt1++;
                        }
                        else if (price == lastPrice && time == lastTime)
                        { // prefix = 0x0
                            cnt2++;

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }
                        else if (time == lastTime)
                        { // prefix = 0x10
                            cnt3++;

                            var priceKey = lastPrice - price;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey,0);
                            prices[priceKey]++;

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }
                        else
                        {
                            cnt4++;

                            var timeKey = lastTime - time;
                            if (timeKey < 0)
                            {

                            }
                            if(!times.ContainsKey(timeKey))
                                times.Add(timeKey,0);
                            times[timeKey]++;

                            var priceKey = price - lastPrice;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey, 0);
                            prices[priceKey]++;
                            
                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }

                        lastTime = time;
                        lastPrice = price;
                        lastVolume = volume;
                        itemCount++;
                    }
                }
            }

            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");
            // Debug.Print($"Cnt1:\t{cnt1:N0}\tCnt21:\t{cnt21:N0}\tCnt22:\t{cnt22:N0}\tCnt23:\t{cnt23:N0}\tCnt31:\t{cnt31:N0}\tCnt32:\t{cnt32:N0}\tCnt33:\t{cnt33:N0}");

            Logger.AddMessage($"Finished!");

            var aa1 = times.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa2 = prices.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa3 = volumes.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

        public static void RunStatistics()    // 110 358 milliseconds (83 368 milliseconds - no actions)
        {
            var files = Directory.GetFiles(Settings.DataFolder + @"Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            var cnt1 = 0;
            var cnt21 = 0;
            var cnt22 = 0;
            var cnt23 = 0;
            var cnt31 = 0;
            var cnt32 = 0;
            var cnt33 = 0;
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var results = new List<PolygonTradesLoader.cResult>();
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonTradesLoader.cRoot>(entry);
                        results.AddRange(oo.results);
                    }

                    recCount += results.Count;

                    var lastPrice = 0;
                    var lastTime = 0;
                    var lastVolume = 0;

                    foreach (var o in results)
                    {
                        var price = Convert.ToInt32(o.price * 10000);
                        var volume = Convert.ToInt32(o.size);
                        var time = Convert.ToInt32(o.sip_timestamp / 1000000000);

                        if (price == lastPrice && volume == lastVolume && time == lastTime)
                        {
                            cnt1++;
                        }
                        else if (price == lastPrice && volume == lastVolume)
                            cnt21++;
                        else if (price == lastPrice && time == lastTime)
                            cnt22++;
                        else if (volume == lastVolume && time == lastTime)
                            cnt23++;
                        else
                        {
                            if (price == lastPrice)
                                cnt31++;
                            if (volume == lastVolume)
                                cnt32++;
                            if (time == lastTime)
                                cnt33++;
                        }
                        lastPrice = price;
                        lastTime = time;
                        lastVolume = volume;
                    }
                }
            }

            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");
            Debug.Print($"Cnt1:\t{cnt1:N0}\tCnt21:\t{cnt21:N0}\tCnt22:\t{cnt22:N0}\tCnt23:\t{cnt23:N0}\tCnt31:\t{cnt31:N0}\tCnt32:\t{cnt32:N0}\tCnt33:\t{cnt33:N0}");

            Logger.AddMessage($"Finished!");
        }

        public static void RunDbq()    //76 274 milliseconds
        {
            var files = Directory.GetFiles(Settings.DataFolder + @"Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    // var results = new List<PolygonTradesLoader.cResult>();
                    var results = new List<MbtTickHttp>();
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonTradesLoader.cRoot>(entry);
                        results.AddRange(oo.results.Where(a => a.size > 0.1).Select(a =>
                            new MbtTickHttp(TimeHelper.GetEstDateTimeFromUnixMilliseconds(a.sip_timestamp / 1000000),
                                a.price, Convert.ToInt64(a.size), 0, 0)));
                    }

                    recCount += results.Count;

                    int records;
                    var writer = new DbqWriter();
                    var bytes = writer.GetBytes(results, "", new DateTime(2024, 4, 5), out records);
                    byteCount += bytes.Length;

                    // var fn = zipFileName.Replace("2024-", "DBQ2024-").Replace(".zip", ".dbq");
                    // File.WriteAllBytes(fn, bytes);
                }
            }

            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");

            Logger.AddMessage($"Finished!");
        }

        public static void RunProtobuf()    //76 274 milliseconds
        {
            var files = Directory.GetFiles(Settings.DataFolder + @"Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    var results = new List<PolygonTradesLoader.cResult>();
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonTradesLoader.cRoot>(entry);
                        results.AddRange(oo.results);
                    }

                    recCount += results.Count;

                    var lastPrice = 0;
                    var lastSecond = int.MaxValue;
                    foreach (var o in results)
                    {
                        var seconds = Convert.ToInt32(o.sip_timestamp / 1000000000);
                        var price = Convert.ToInt32(o.price * 1000);
                        o.Price2 = price - lastPrice;
                        o.Size2 = Convert.ToInt32(o.size);

                        if (lastSecond == int.MaxValue)
                        {
                            // o.Seconds2 = o.Seconds;
                        }
                        else
                        {
                            o.Seconds2 = Convert.ToUInt16(lastSecond - seconds);
                        }


                        lastPrice = price;
                        lastSecond = seconds;
                    }

                    var fn = zipFileName.Replace("2024-", "P2024-").Replace(".zip", ".buf");
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, results);
                        var bytes = ms.ToArray();
                        byteCount += bytes.Length;
                        File.WriteAllBytes(fn, bytes);
                    }

                    // var oo2 = ProtoBuf.Serializer.Deserialize<PolygonTradesLoader.cResult[]>((ReadOnlySpan<byte>)bytes);
                }
            }

            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");

            Logger.AddMessage($"Finished!");
        }

        public static void Run()    //76 274 milliseconds
        {
            var files = Directory.GetFiles(Settings.DataFolder + @"Trades\Polygon\Data\2024-04-05", "*.zip");
            foreach (var zipFileName in files.Take(3))
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonTradesLoader.cRoot>(entry);
                    }
            }

        }
    }
}