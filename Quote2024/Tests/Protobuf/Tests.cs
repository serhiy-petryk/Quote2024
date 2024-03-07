using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ProtoBuf;

namespace Tests.Protobuf
{
    public class Tests
    {
        /* Results:
Unzip + Deserialize Proto:	38.4 secs
Unzip + Deserialize Json:	81.7 secs
Unzip + Deserialize Csv:	115.4 secs

Unzip + SaveToJson:	173.9 secs	5.92GB; Zip: 2:44 min, 1018 GB
Unzip + SaveToProto:	161.2 secs	2.74GB; Zip: 1:06 min, 1164 GB
Unzip + SaveToCsv:	238.7 secs	3.65GB; Zip: 1:46 min, 819 GB
         */

        private const string TestFile = @"E:\Temp\Exchange\MP2003_20231230.zip";
        private const string ProtoFile = @"E:\Temp\Exchange\ProtoMP2003_20231230.zip";
        private const string JsonFile = @"E:\Temp\Exchange\JsonMP2003_20231230.zip";
        private const string CsvFile = @"E:\Temp\Exchange\CsvMP2003_20231230.zip";

        public static void ReadFromCsv()
        {
            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(CsvFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                    {
                        var itemCount = int.Parse(reader.ReadLine());
                        var oo = new List<cMinuteItem>();
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            oo.Add(new cMinuteItem(line));
                        }
                        count += oo.Count;
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        public static void ReadFromProto()
        {
            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(ProtoFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var bytes = memstream.ToArray();
                        var o = Serializer.Deserialize<cMinuteRoot>((ReadOnlyMemory<byte>)bytes);
                        // var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cMinuteRoot>(bytes);
                        count += o.count == 0 ? 0 : o.results.Length;
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        public static void ReadFromZip()
        {
            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(TestFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var bytes = memstream.ToArray();
                        var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cMinuteRoot>(bytes);
                        count += o.count == 0 ? 0 : o.results.Length;
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        public static void SaveToCsv()
        {
            if (File.Exists(CsvFile))
                File.Delete(CsvFile);
            var folder = Path.Combine(Path.GetDirectoryName(CsvFile), Path.GetFileNameWithoutExtension(CsvFile));
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(TestFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var bytes = memstream.ToArray();
                        var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cMinuteRoot>(bytes);
                        count += o.count == 0 ? 0 : o.results.Length;
                        var newFN = Path.Combine(folder, Path.ChangeExtension(entry.Name, ".csv"));
                        File.AppendAllText(newFN, o.count.ToString()+Environment.NewLine);
                        if (o.count != 0)
                        {
                            File.AppendAllLines(newFN, o.results.Select(a=>a.ToString()));
                        }
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        public static void SaveToJson()
        {
            if (File.Exists(JsonFile))
                File.Delete(JsonFile);
            var folder = Path.Combine(Path.GetDirectoryName(JsonFile), Path.GetFileNameWithoutExtension(JsonFile));
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(TestFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var bytes = memstream.ToArray();
                        var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cMinuteRoot>(bytes);
                        count += o.count == 0 ? 0 : o.results.Length;
                        var oo = SpanJson.JsonSerializer.Generic.Utf8.Serialize(o);
                        var newFN = Path.Combine(folder, entry.Name);
                        File.WriteAllBytes(newFN, oo);
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        public static void SaveToProtobuf()
        {
            if (File.Exists(ProtoFile))
                File.Delete(ProtoFile);
            var folder = Path.Combine(Path.GetDirectoryName(ProtoFile), Path.GetFileNameWithoutExtension(ProtoFile));
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);

            var sw = new Stopwatch();
            sw.Start();
            var count = 0;
            using (var zip = ZipFile.Open(TestFile, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var bytes = memstream.ToArray();
                        var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cMinuteRoot>(bytes);
                        count += o.count == 0 ? 0 : o.results.Length;
                        using (var stream = new MemoryStream())
                        {
                            Serializer.Serialize(stream, o);
                            var newFN = Path.Combine(folder, Path.ChangeExtension(entry.Name, ".protobuf"));
                            File.WriteAllBytes(newFN, stream.ToArray());
                        }
                    }
                }
            sw.Stop();
            Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} milliseconds.Items: {count:N0}");
        }

        #region ========  Json classes  ===========
        private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        public static DateTime GetEstDateTimeFromUnixMilliseconds(long unixTimeInSeconds)
        {
            var aa2 = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeInSeconds);
            return (aa2 + EstTimeZone.GetUtcOffset(aa2)).DateTime;
        }

        public static string GetMyTicker(string polygonTicker)
        {
            if (polygonTicker.EndsWith("pw"))
                polygonTicker = polygonTicker.Replace("pw", "^^W");
            else if (polygonTicker.EndsWith("pAw"))
                polygonTicker = polygonTicker.Replace("pAw", "^^AW");
            else if (polygonTicker.EndsWith("pEw"))
                polygonTicker = polygonTicker.Replace("pEw", "^^EW");
            else if (polygonTicker.Contains("p"))
                polygonTicker = polygonTicker.Replace("p", "^");
            else if (polygonTicker.Contains("rw"))
                polygonTicker = polygonTicker.Replace("rw", ".RTW");
            else if (polygonTicker.Contains("r"))
                polygonTicker = polygonTicker.Replace("r", ".RT");
            else if (polygonTicker.Contains("w"))
                polygonTicker = polygonTicker.Replace("w", ".WI");

            if (polygonTicker.Any(char.IsLower))
                throw new Exception($"Check PolygonCommon.GetMyTicker method for '{polygonTicker}' ticker");

            return polygonTicker;
        }


        [ProtoContract]
        public class cMinuteRoot
        {
            [ProtoMember(1)]
            public string ticker { get; set; }
            [ProtoMember(2)]
            public int queryCount { get; set; }
            [ProtoMember(3)]
            public int resultsCount { get; set; }
            [ProtoMember(4)]
            public int count { get; set; }
            [ProtoMember(5)]
            public bool adjusted { get; set; }
            [ProtoMember(6)]
            public string status { get; set; }
            [ProtoMember(7)]
            public string next_url { get; set; }
            [ProtoMember(8)]
            public string request_id { get; set; }
            [ProtoMember(9)]
            public cMinuteItem[] results { get; set; }

            public string Symbol => GetMyTicker(ticker);
        }

        [ProtoContract]
        public class cMinuteItem
        {
            public cMinuteItem(string csvLine)
            {
                var ss = csvLine.Split(',');
                t = long.Parse(ss[0]);
                o = float.Parse(ss[1], CultureInfo.InvariantCulture);
                h = float.Parse(ss[2], CultureInfo.InvariantCulture);
                l = float.Parse(ss[3], CultureInfo.InvariantCulture);
                c = float.Parse(ss[4], CultureInfo.InvariantCulture);
                v = float.Parse(ss[5], CultureInfo.InvariantCulture);
                vw = float.Parse(ss[6], CultureInfo.InvariantCulture);
                n = int.Parse(ss[7]);
            }

            [ProtoMember(1)]
            public long t { get; set; } // unix utc milliseconds
            [ProtoMember(2)]
            public float o { get; set; }
            [ProtoMember(3)]
            public float h { get; set; }
            [ProtoMember(4)]
            public float l { get; set; }
            [ProtoMember(5)]
            public float c { get; set; }
            [ProtoMember(6)]
            public float v { get; set; }
            [ProtoMember(7)]
            public float vw { get; set; }
            [ProtoMember(8)]
            public int n { get; set; }

            // All not valid quotes have volume=0 except one quote of 2004 year where open<low
            // public bool IsValid => !(o > h || o < l || c > h || c < l || v < 0.5f || n == 0);

            // public DateTime DateTime => GetEstDateTimeFromUnixMilliseconds(t);

            public override string ToString()
            {
                return $"{t},{GetString(o)},{GetString(h)},{GetString(l)},{GetString(c)},{GetString(v)},{GetString(vw)},{n}";
                string GetString(float f) => f.ToString(CultureInfo.InvariantCulture);
            }
        }
        #endregion

    }
}
