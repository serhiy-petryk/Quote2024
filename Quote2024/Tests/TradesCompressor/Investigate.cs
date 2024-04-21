using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sewer56.BitStream;
using Sewer56.BitStream.ByteStreams;

namespace Tests.TradesCompressor
{
    public class Investigate
    {
        public static void Compress()    // 110 358 milliseconds (83 368 milliseconds - no actions)
        {
            var sw = new Stopwatch();
            sw.Start();

            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            var cnt1 = 0;
            var cnt2 = 0;
            var cnt3 = 0;
            var cnt4 = 0;
            var times = new Dictionary<int, int>();
            var prices = new Dictionary<int, int>();
            var volumes = new Dictionary<int, int>();
            foreach (var zipFileName in files.Take(1))
            {
                // Logger.AddMessage($"File: {zipFileName}");
                Debug.Print($"File: {zipFileName}");
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

                    var _data = new byte[1000 * 1000];
                    var sData = new StringBuilder();
                    var stream = new ArrayByteStream(_data);
                    var bitStream = new BitStream<ArrayByteStream>(stream, 0);

                    var itemCount = 0;
                    foreach (var o in results)
                    {
                        var time = Convert.ToInt32(o.sip_timestamp / 1000000000);
                        var price = Convert.ToInt32(o.price * 10000);
                        var volume = Convert.ToInt32(o.size);

                        if (price == lastPrice && volume == lastVolume && time == lastTime)
                        { // prefix = 0x110
                            cnt1++;
                            bitStream.Write8(0x06, 3);
                            sData.Append(",110");
                        }
                        else if (price == lastPrice && time == lastTime)
                        { // prefix = 0x0
                            cnt2++;
                            bitStream.Write8(0x0, 1);
                            sData.Append(",0");

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }
                        else if (time == lastTime)
                        { // prefix = 0x10
                            cnt3++;

                            sData.Append(",10");
                            bitStream.Write8(0x02, 2);

                            var priceKey = lastPrice - price;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey, 0);
                            prices[priceKey]++;
                            SavePrice(ref bitStream, priceKey, sData);

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }
                        else
                        {   // prefix = 0x111
                            cnt4++;
                            sData.Append(",111");
                            bitStream.Write8(0x07, 3);

                            var timeKey = lastTime - time;
                            if (!times.ContainsKey(timeKey))
                                times.Add(timeKey, 0);
                            times[timeKey]++;
                            // SaveTime(ref bitStream, timeKey, sData);

                            var priceKey = price - lastPrice;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey, 0);
                            prices[priceKey]++;
                            SavePrice(ref bitStream, priceKey, sData);

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }

                        lastTime = time;
                        lastPrice = price;
                        lastVolume = volume;
                        itemCount++;
                    }

                    var s = "";
                    for (var k = 0; k < bitStream.ByteOffset; k++)
                    {
                        s = s + Convert.ToString(_data[k], 2).PadLeft(8, '0');
                    }

                    if (bitStream.NextByteIndex != bitStream.ByteOffset)
                    {
                        var s1 = Convert.ToString(_data[bitStream.ByteOffset], 2).PadLeft(8, '0');
                        var s2 = s1.Substring(0, bitStream.BitOffset);
                        s = s + s2;
                    }

                    if (!string.Equals(sData.ToString().Replace(",", ""), s))
                    {

                    }

                    byteCount += bitStream.ByteOffset;
                }
            }

            sw.Stop();

            Debug.Print($"Duration:\t{sw.ElapsedMilliseconds:N0} msecs");
            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");
            // Debug.Print($"Cnt1:\t{cnt1:N0}\tCnt21:\t{cnt21:N0}\tCnt22:\t{cnt22:N0}\tCnt23:\t{cnt23:N0}\tCnt31:\t{cnt31:N0}\tCnt32:\t{cnt32:N0}\tCnt33:\t{cnt33:N0}");

            // Logger.AddMessage($"Finished!");

            var aa1 = times.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa2 = prices.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa3 = volumes.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }

        public static void InvestigateNewCompressor()    // 110 358 milliseconds (83 368 milliseconds - no actions)
        {
            var sw = new Stopwatch();
            sw.Start();

            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
            var cnt1 = 0;
            var cnt2 = 0;
            var cnt3 = 0;
            var cnt4 = 0;
            var times = new Dictionary<int, int>();
            var diffPrices = new Dictionary<int, int>();
            var diffPrices2 = new Dictionary<int, int>();
            var volumes = new Dictionary<int, int>();
            foreach (var zipFileName in files)
            {
                // Logger.AddMessage($"File: {zipFileName}");
                Debug.Print($"File: {zipFileName}");
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
                            if (!diffPrices.ContainsKey(priceKey))
                                diffPrices.Add(priceKey, 0);
                            diffPrices[priceKey]++;

                            var priceKey2 = Math.Abs(priceKey);
                            if (priceKey2 % 100 == 0)
                                priceKey2 = priceKey2 / 100;
                            if (!diffPrices2.ContainsKey(priceKey2))
                                diffPrices2.Add(priceKey2, 0);
                            diffPrices2[priceKey2]++;

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                        }
                        else
                        {   // prefix = 0x111
                            cnt4++;

                            var timeKey = lastTime - time;
                            if (!times.ContainsKey(timeKey))
                                times.Add(timeKey, 0);
                            times[timeKey]++;

                            var priceKey = price - lastPrice;
                            if (!diffPrices.ContainsKey(priceKey))
                                diffPrices.Add(priceKey, 0);
                            diffPrices[priceKey]++;

                            var priceKey2 = Math.Abs(priceKey);
                            if (priceKey2 % 100 == 0)
                                priceKey2 = priceKey2 / 100;
                            if (!diffPrices2.ContainsKey(priceKey2))
                                diffPrices2.Add(priceKey2, 0);
                            diffPrices2[priceKey2]++;

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

            sw.Stop();

            Debug.Print($"Duration:\t{sw.ElapsedMilliseconds:N0} msecs");
            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");
            // Debug.Print($"Cnt1:\t{cnt1:N0}\tCnt21:\t{cnt21:N0}\tCnt22:\t{cnt22:N0}\tCnt23:\t{cnt23:N0}\tCnt31:\t{cnt31:N0}\tCnt32:\t{cnt32:N0}\tCnt33:\t{cnt33:N0}");

            // Logger.AddMessage($"Finished!");

            var aa1 = times.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa21 = diffPrices.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa22 = diffPrices2.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
            var aa3 = volumes.OrderByDescending(a => a.Value).ToDictionary(a => a.Key, a => a.Value);
        }


        private static void SaveTime(ref BitStream<ArrayByteStream> stream, int time, StringBuilder sb)
        {
            if (time == 0)
            {
                stream.WriteBit(0x0);
                sb.Append(",0");
                return;
            }
            else if (time == 1)
            {
                stream.Write8(0x4, 3);
                sb.Append(",100");
                return;
            }
            else if (time == 2)
            {
                stream.Write8(0x5, 3);
                sb.Append(",101");
                return;
            }
            else if (time == 3)
            {
                stream.Write8(0x6, 3);
                sb.Append(",110");
                return;
            }

            stream.Write8(0x7, 3);
            sb.Append(",111");
            if (time <= byte.MaxValue)
            {
                stream.WriteBit(0x0);
                sb.Append("0");
                // stream.Write(Convert.ToByte(time));
                // stream.Write32((uint)time, 8);
                stream.Write8(Convert.ToByte(time), 8);
                var a = Convert.ToString(time, 2).PadLeft(8, '0');
                sb.Append(a);
                return;
            }
            stream.WriteBit(0x1);
            sb.Append("1");
            stream.Write32(Convert.ToUInt32(time), 32);
            var a1 = Convert.ToString(time, 2).PadLeft(32, '0');
            sb.Append(a1);
        }

        private static void SavePrice(ref BitStream<ArrayByteStream> stream, int diffPrice, StringBuilder sb)
        {
            if (diffPrice < 0)
            {
                stream.WriteBit(0x1);
                sb.Append("1");
                diffPrice = -diffPrice;
            }
            else
            {
                stream.WriteBit(0x0);
                sb.Append("0");
            }

            if (diffPrice % 50 == 0)
            {
                stream.WriteBit(0x1);
                sb.Append("1");
                diffPrice = diffPrice / 50;
            }
            else
            {
                stream.WriteBit(0x0);
                sb.Append("0");
            }

            if (diffPrice == 0)
            {
                stream.Write8(0, 2);
                sb.Append("00");
            }
            else if (diffPrice == 1)
            {
                stream.Write8(0x1, 2);
                sb.Append("01");
            }
            else if (diffPrice == 2)
            {
                stream.Write8(0x2, 2);
                sb.Append("10");
            }
            else
            {
                stream.Write8(0x3, 2);
                sb.Append("11");
                if (diffPrice <= Byte.MaxValue)
                {
                    stream.Write8(Convert.ToByte(diffPrice), 8);
                    sb.Append(Convert.ToString(diffPrice, 2).PadLeft(8, '0'));
                }
                else if (diffPrice <= UInt16.MaxValue)
                {
                    stream.Write8(0x0, 8);
                    sb.Append("00000000");
                    stream.Write16(Convert.ToUInt16(diffPrice), 16);
                    sb.Append(Convert.ToString(diffPrice, 2).PadLeft(16, '0'));
                }
                else
                {
                    stream.Write8(0x1, 8);
                    sb.Append("00000001");
                    stream.Write32((uint)diffPrice, 32);
                    sb.Append(Convert.ToString(diffPrice, 2).PadLeft(32, '0'));
                }
            }
        }
    }
}
