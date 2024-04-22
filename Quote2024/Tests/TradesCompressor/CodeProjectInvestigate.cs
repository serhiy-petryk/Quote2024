using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Tests.TradesCompressor
{
    public static class CodeProjectInvestigate
    {
        public static void Test()
        {
            var stream = new CodeProjectBitStream();
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
            SaveVolume(ref stream, 100);
        }

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
            var volumes = new Dictionary<ulong, int>();
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
                    var lastVolume = Convert.ToUInt64(results[0].size);

                    // var _data = new byte[10000 * 1000];
                    // var sData = new StringBuilder();
                    var bitStream = new CodeProjectBitStream(results.Count * 24);

                    var itemCount = 0;
                    foreach (var o in results)
                    {
                        var time = Convert.ToInt32(o.sip_timestamp / 1000000000);
                        var price = Convert.ToInt32(o.price * 10000);
                        var volume = Convert.ToUInt64(o.size);

                        if (price == lastPrice && volume == lastVolume && time == lastTime)
                        { // prefix = 0x110
                            cnt1++;

                            bitStream.Write(0x06, 0, 3);
                            // sData.Append(",110");
                        }
                        else if (price == lastPrice && time == lastTime)
                        { // prefix = 0x0
                            cnt2++;

                            bitStream.Write(0x0, 0, 1);
                            // sData.Append(",0");

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                            SaveVolume(ref bitStream, volume);
                        }
                        else if (time == lastTime)
                        { // prefix = 0x10
                            cnt3++;

                            // sData.Append(",10");
                            bitStream.Write(0x02, 0, 2);

                            var priceKey = lastPrice - price;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey, 0);
                            prices[priceKey]++;
                            SavePrice(ref bitStream, priceKey);

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                            SaveVolume(ref bitStream, volume);
                        }
                        else
                        {   // prefix = 0x111
                            cnt4++;

                            // sData.Append(",111");
                            bitStream.Write(0x07, 0, 3);

                            var timeKey = lastTime - time;
                            if (!times.ContainsKey(timeKey))
                                times.Add(timeKey, 0);
                            times[timeKey]++;
                            SaveTime(ref bitStream, timeKey);

                            var priceKey = price - lastPrice;
                            if (!prices.ContainsKey(priceKey))
                                prices.Add(priceKey, 0);
                            prices[priceKey]++;
                            SavePrice(ref bitStream, priceKey);

                            if (!volumes.ContainsKey(volume))
                                volumes.Add(volume, 0);
                            volumes[volume]++;
                            SaveVolume(ref bitStream, volume);
                        }

                        lastTime = time;
                        lastPrice = price;
                        lastVolume = volume;
                        itemCount++;
                    }

                    // end of stream
                    bitStream.Write(0x1, 0, 3);

                    byteCount += bitStream.Length8;

                    /*var fn = zipFileName.Replace("2024-04", "New2024-04").Replace(".zip", ".bin");
                    var data2 = new byte[bitStream.ByteOffset];
                    Array.Copy(_data, data2, bitStream.ByteOffset);
                    File.WriteAllBytes(fn, data2);*/

                    // sData.Append("001");


                    /* var s = "";
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

                    }*/
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

        private static void SaveTime(ref CodeProjectBitStream stream, int time, StringBuilder sb = null)
        {
            // return;

            if (time == 0)
            {
                stream.Write(0x0, 0,1);
                sb?.Append(",0");
            }
            else if (time == 1)
            {
                stream.Write(0x4, 0, 3);
                sb?.Append(",100");
            }
            else if (time == 2)
            {
                stream.Write(0x5, 0, 3);
                sb?.Append(",101");
            }
            else if (time == 3)
            {
                stream.Write(0x6, 0, 3);
                sb?.Append(",110");
            }
            else
            {
                stream.Write(0x7, 0, 3);
                sb?.Append(",111");
                if (time <= byte.MaxValue)
                {
                    stream.Write(0x0, 0, 1);
                    sb?.Append("0");
                    // stream.Write(Convert.ToByte(time));
                    // stream.Write32((uint)time, 8);
                    stream.Write(Convert.ToByte(time), 0, 8);
                    var a = Convert.ToString(time, 2).PadLeft(8, '0');
                    sb?.Append(a);
                }
                else
                {
                    stream.Write(0x1, 0, 1);
                    sb?.Append("1");
                    stream.Write(Convert.ToUInt32(time), 0, 32);
                    sb?.Append(Convert.ToString(time, 2).PadLeft(32, '0'));
                }
            }
        }

        private static void SavePrice(ref CodeProjectBitStream stream, int diffPrice, StringBuilder sb = null)
        {
            // return;

            if (diffPrice < 0)
            {
                stream.Write(0x1, 0 ,1);
                sb?.Append("1");
                diffPrice = -diffPrice;
            }
            else
            {
                stream.Write(0x0, 0, 1);
                sb?.Append("0");
            }

            if (diffPrice % 50 == 0)
            {
                stream.Write(0x1, 0, 1);
                sb?.Append("1");
                diffPrice = diffPrice / 50;
            }
            else
            {
                stream.Write(0x0, 0, 1);
                sb?.Append("0");
            }

            if (diffPrice == 0)
            {
                stream.Write(0, 0, 2);
                sb?.Append("00");
            }
            else if (diffPrice == 1)
            {
                stream.Write(0x1, 0, 2);
                sb?.Append("01");
            }
            else if (diffPrice == 2)
            {
                stream.Write(0x2, 0, 2);
                sb?.Append("10");
            }
            else
            {
                stream.Write(0x3, 0, 2);
                sb?.Append("11");
                if (diffPrice <= Byte.MaxValue)
                {
                    stream.Write(Convert.ToByte(diffPrice), 0, 8);
                    sb?.Append(Convert.ToString(diffPrice, 2).PadLeft(8, '0'));
                }
                else if (diffPrice <= UInt16.MaxValue)
                {
                    stream.Write(0x0, 0, 8);
                    sb?.Append("00000000");
                    stream.Write(Convert.ToUInt16(diffPrice), 0, 16);
                    sb?.Append(Convert.ToString(diffPrice, 2).PadLeft(16, '0'));
                }
                else
                {
                    stream.Write(0x1, 0, 8);
                    sb?.Append("00000001");
                    stream.Write((uint)diffPrice, 0, 32);
                    sb?.Append(Convert.ToString(diffPrice, 2).PadLeft(32, '0'));
                }
            }
        }

        private static void SaveVolume(ref CodeProjectBitStream stream, ulong volume, StringBuilder sb = null)
        {
            // return;

            if (volume % 100 == 0)
            {
                stream.Write(0x1, 0,1 );
                sb?.Append("1");
                volume = volume / 100;
            }
            else
            {
                stream.Write(0x0, 0,1);
                sb?.Append("0");
            }

            if (volume == 1)
            {
                stream.Write(0x0, 0, 1);
                sb?.Append("0");
            }
            else
            {
                stream.Write(0x1, 0, 1);
                sb?.Append("1");
                if (volume <= byte.MaxValue)
                {
                    stream.Write(Convert.ToByte(volume), 0, 8);
                    sb?.Append(Convert.ToString((byte)volume, 2).PadLeft(8, '0'));
                }
                else
                {
                    stream.Write((byte)0x0, 0, 8);
                    sb?.Append("00000000");
                    stream.Write(volume, 0, 64);
                    sb?.Append(Convert.ToString((long)volume, 2).PadLeft(64, '0'));
                }
            }
        }


    }
}
