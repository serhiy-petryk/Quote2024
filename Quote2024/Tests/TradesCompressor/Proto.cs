using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Tests.TradesCompressor
{
    class Proto
    {
        public static void RunProtobuf()    //76 274 milliseconds
        {
            var sw = new Stopwatch();
            sw.Start();

            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            long byteCount = 0;
            long recCount = 0;
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

                    var lastPrice = 0;
                    var lastSecond = int.MaxValue;
                    foreach (var o in results)
                    {
                        var seconds = Convert.ToInt32(o.sip_timestamp / 1000000000);
                        var price = Convert.ToInt32(o.price * 1000);
                        o.Price2 = price - lastPrice;
                        o.Size2 = Convert.ToUInt32(o.size);

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

                    // var fn = zipFileName.Replace("2024-", "P2024-").Replace(".zip", ".buf");
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, results);
                        var bytes = ms.ToArray();
                        byteCount += bytes.Length;
                        // File.WriteAllBytes(fn, bytes);
                    }

                    // var oo2 = ProtoBuf.Serializer.Deserialize<PolygonTradesLoader.cResult[]>((ReadOnlySpan<byte>)bytes);
                }
            }

            sw.Stop();

            Debug.Print($"Duration:\t{sw.ElapsedMilliseconds:N0} msecs");
            Debug.Print($"Bytes:\t{byteCount:N0}");
            Debug.Print($"Records:\t{recCount:N0}");

            // Logger.AddMessage($"Finished!");
            Debug.Print("Finished");
        }

    }
}
