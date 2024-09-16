using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Tests
{
    public static class RealTimeYahooMinuteTests
    {
        private const string Folder = Settings.DataFolder + @"RealTime\YahooMinute\2024-03-21";
        private const string Folder2 = Settings.DataFolder + @"RealTime\YahooMinute\2024-03-26";
        private const string MainFolder = Settings.DataFolder + @"RealTime\YahooMinute";

        public static void TestTradingPeriod()
        {
            var zipFileNames = Directory.GetFiles(Actions.Yahoo.YahooCommon.MinuteYahooDataFolder, "*.zip");
            var errroLog = new Dictionary<string, string>();
            foreach (var zipFileName in zipFileNames)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                foreach (var x in Actions.Yahoo.YahooCommon.GetMinuteDataFromZipFile(zipFileName, errroLog))
                {
                    var aa = x.Item2;
                }
            }
            Logger.AddMessage($"!!!Finished");
        }

        public static void CheckExchanges()
        {
            var exchanges = new Dictionary<string, int>();
            var zipFiles = Directory.GetFiles(Folder, "*.zip").OrderBy(a => a).ToArray();
            foreach (var zipFileName in zipFiles)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                        if (o.chart.result != null)
                        {
                            var exchange = o.chart.result[0].meta.exchangeName;
                            if (!exchanges.ContainsKey(exchange))
                                exchanges.Add(exchange, 0);
                            exchanges[exchange]++;
                        }
                    }
            }

        }

        public static void CorrectErrorProperty()
        {
            var exchanges = new Dictionary<string, int>();
            var zipFiles = Directory.GetFiles(MainFolder, "*.zip", SearchOption.AllDirectories).OrderBy(a => a)
                .ToArray();
            foreach (var zipFileName in zipFiles)
            {
                if (zipFileName.Contains("DayPolygon")) continue;

                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                var errors = new Dictionary<string, string>();
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        string content;
                        using (var entryStream = entry.Open())
                        using (var memstream = new MemoryStream())
                        {
                            entryStream.CopyTo(memstream);
                            var bytes = memstream.ToArray();
                            if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // contains BOM
                                content = System.Text.Encoding.UTF8.GetString(bytes.Skip(3).ToArray());
                            else
                                content = System.Text.Encoding.UTF8.GetString(bytes);
                        }

                        if (content.Contains("\"error\":\""))
                        {
                            Debug.Print($"File: {Path.GetFileName(zipFileName)}\tEntry: {entry.Name}\tContent: {content}");
                            errors.Add(entry.Name, content.Replace("\"error\":", "\"error\":{\"code\":\"C#\",\"description\":") + "}");
                        }
                        else
                        {
                            var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                            if (o.chart.error != null)
                            {
                                Debug.Print($"File: {Path.GetFileName(zipFileName)}\tEntry: {entry.Name}\tError: {o.chart.error.code}\t{o.chart.error.description}");
                            }
                        }
                    }

                if (errors.Count > 0)
                {
                    var d1 = File.GetCreationTime(zipFileName);
                    var d2 = File.GetLastWriteTime(zipFileName);
                    var d3 = File.GetLastAccessTime(zipFileName);

                    using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                        foreach (var kvp in errors)
                        {
                            var entry = zipArchive.GetEntry(kvp.Key);
                            var entryDate = entry.LastWriteTime;
                            entry.Delete();

                            var zipEntry = zipArchive.CreateEntry(entry.Name);
                            using (var writer = new StreamWriter(zipEntry.Open()))
                                writer.Write(kvp.Value);
                            zipEntry.LastWriteTime = entryDate;
                        }

                    File.SetCreationTime(zipFileName, d1);
                    File.SetLastWriteTime(zipFileName, d2);
                    File.SetLastAccessTime(zipFileName, d3);
                }
            }

        }

        public static void ErrorList()
        {
            var exchanges = new Dictionary<string, int>();
            var zipFiles = Directory.GetFiles(MainFolder, "*.zip", SearchOption.AllDirectories).OrderBy(a => a)
                .ToArray();
            foreach (var zipFileName in zipFiles)
            {
                if (zipFileName.Contains("DayPolygon")) continue;

                var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
                var utcFileDateTime =
                    DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToUniversalTime();
                var msecs = new DateTimeOffset(utcFileDateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
                var nyFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(msecs);

                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        string content;
                        using (var entryStream = entry.Open())
                        using (var memstream = new MemoryStream())
                        {
                            entryStream.CopyTo(memstream);
                            var bytes = memstream.ToArray();
                            var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                            if (o.chart.error != null)
                            {
                                Debug.Print($"File: {Path.GetFileName(zipFileName)}\tEntry: {entry.Name}\tError: {o.chart.error.code}\t{o.chart.error.description}");
                            }
                        }
                    }
            }
        }

        public static void RegularMarketTime()
        {
            var exchanges = new Dictionary<string, int>();
            var zipFiles = Directory.GetFiles(MainFolder, "*.zip", SearchOption.AllDirectories).OrderBy(a => a)
                .ToArray();
            foreach (var zipFileName in zipFiles)
            {
                if (zipFileName.Contains("DayPolygon")) continue;

                var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
                var utcFileDateTime =
                    DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToUniversalTime();
                var msecs = new DateTimeOffset(utcFileDateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
                var nyFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(msecs);

                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        string content;
                        using (var entryStream = entry.Open())
                        using (var memstream = new MemoryStream())
                        {
                            entryStream.CopyTo(memstream);
                            var bytes = memstream.ToArray();
                            var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                            if (o.chart.result != null && o.chart.result[0].indicators.quote.Length > 0)
                            {
                                var a1 = o.chart.result[0].meta.NyRegularMarketTime.Value;
                                Debug.Print($"regularMarketTime: {a1:yyyy-MM-dd HH:mm:ss}\tFileDateTime: {nyFileDateTime:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                    }
            }
        }

        public static void DefineDelay()
        {
            var lastSecondsLog = new Dictionary<int, int>();

            var zipFiles = Directory.GetFiles(Folder2, "*.zip", SearchOption.AllDirectories).OrderBy(a => a)
                .ToArray();
            foreach (var zipFileName in zipFiles)
            {
                if (zipFileName.Contains("DayPolygon")) continue;

                var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
                var utcFileDateTime =
                    DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture).ToUniversalTime();
                var msecs = new DateTimeOffset(utcFileDateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
                var nyFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(msecs);

                if (nyFileDateTime.TimeOfDay > new TimeSpan(16, 2, 0))
                    continue;

                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                        if (o.chart.result?[0].timestamp != null)
                        {
                            var times = o.chart.result[0].timestamp;
                            var times2 = times.Select(a => TimeHelper.GetEstDateTimeFromUnixMilliseconds(a * 1000)).ToArray();
                            var lastSeconds = Convert.ToInt32(times[times.Length - 1] - times[times.Length - 2]);
                            if (!lastSecondsLog.ContainsKey(lastSeconds))
                                lastSecondsLog.Add(lastSeconds,0);
                            lastSecondsLog[lastSeconds]++;

                            var a1 = o.chart.result[0].indicators.quote[0];
                            if (lastSeconds > 60)
                            {
                                Debug.Print($"Big last seconds: {entry.Name}. File: {Path.GetFileName(zipFileName)}. Volume: {a1.volume[times.Length-1]}. Time: {times2[times2.Length-1].TimeOfDay}. Last seconds:{lastSeconds}");
                                if (entry.Name == "APP.json")
                                {

                                }
                            }
                            if (lastSeconds == 60)
                            {
                                if (a1.volume[times.Length - 1] == null || a1.volume[times.Length - 1]==0)
                                {

                                }

                            }
                            else
                            {
                                if (a1.volume[times.Length - 1] != 0)
                                {

                                }
                            }

                            if (a1.volume[times.Length - 2] == null && a1.volume[times.Length - 1] != 0 && lastSeconds!=60)
                            {

                            }
                            else if (a1.volume[times.Length - 2] != null && a1.volume[times.Length - 1] == 0)
                            {

                            }
                            // Debug.Print($"regularMarketTime: {a1:yyyy-MM-dd HH:mm:ss}\tFileDateTime: {nyFileDateTime:yyyy-MM-dd HH:mm:ss}");
                        }
                    }
            }

            var aa1 = lastSecondsLog.OrderBy(a => a.Key).ToDictionary(a => a.Key, a => a.Value);
            foreach(var kvp in aa1)
                Debug.Print($"{kvp.Key}\t{kvp.Value}");
        }

        public static void Start()
        {
            var exchanges = new Dictionary<string, int>();
            var zipFiles = Directory.GetFiles(MainFolder, "*.zip", SearchOption.AllDirectories).OrderBy(a => a)
                .ToArray();
            foreach (var zipFileName in zipFiles)
            {
                if (zipFileName.Contains("DayPolygon")) continue;

                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        string content;
                        using (var entryStream = entry.Open())
                        using (var memstream = new MemoryStream())
                        {
                            entryStream.CopyTo(memstream);
                            var bytes = memstream.ToArray();
                            if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // contains BOM
                                content = System.Text.Encoding.UTF8.GetString(bytes.Skip(3).ToArray());
                            else
                                content = System.Text.Encoding.UTF8.GetString(bytes);
                        }

                        if (content.Contains("\"error\":\""))
                        {
                            Debug.Print($"File: {Path.GetFileName(zipFileName)}\tEntry: {entry.Name}\tContent: {content}");
                            throw new Exception("AAAA");
                        }
                        else
                        {
                            var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                            if (o.chart.error != null)
                            {
                                Debug.Print($"File: {Path.GetFileName(zipFileName)}\tEntry: {entry.Name}\tError: {o.chart.error.code}\t{o.chart.error.description}");
                            }
                        }
                    }
            }

        }

    }
}
