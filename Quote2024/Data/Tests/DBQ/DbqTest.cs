using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Actions.Polygon;
using Data.Helpers;
using ProtoBuf;

namespace Data.Tests.DBQ
{
    public static class DbqTest
    {
        public static void RunProtobuf()    //76 274 milliseconds
        {
            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
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
                        recCount += oo.results.Length;
                    }

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
                    byte[] bytes;
                    using (var ms = new MemoryStream())
                    {
                        ProtoBuf.Serializer.Serialize(ms, results);
                        bytes = ms.ToArray();
                        byteCount += bytes.Length;
                        // File.WriteAllBytes(fn, bytes);
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
            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
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