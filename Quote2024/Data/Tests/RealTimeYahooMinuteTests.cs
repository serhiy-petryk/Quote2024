using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Tests
{
    public static class RealTimeYahooMinuteTests
    {
        private const string Folder = @"E:\Quote\WebData\RealTime\YahooMinute\2024-03-21";
        private const string MainFolder = @"E:\Quote\WebData\RealTime\YahooMinute";

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
