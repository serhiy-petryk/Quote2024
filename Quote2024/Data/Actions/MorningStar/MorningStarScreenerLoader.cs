using System;
using System.Collections.Generic;
using System.IO;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public static class MorningStarScreenerLoader
    {
        public static string[] Sectors = new[]
        {
            "basic-materials-stocks", "communication-services-stocks", "consumer-cyclical-stocks",
            "consumer-defensive-stocks", "energy-stocks", "financial-services-stocks", "healthcare-stocks",
            "industrial-stocks", "real-estate-stocks", "technology-stocks", "utility-stocks"
        };

        // page numeration from 1
        private const string UrlTemplate = @"https://www.morningstar.com/api/v2/navigation-list/{0}?sort=marketCap:desc&page={1}&limit=50";
        private const string DataFolder = @"E:\Quote\WebData\Screener\MorningStar\Data";
        private const string JsonDataFolder = @"E:\Quote\WebData\Screener\MorningStar\Json";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            DownloadAndSaveToZip();

            Logger.AddMessage($"Finished!");
        }

        private static void DownloadAndSaveToZip()
        {
            var dateKey = DateTime.Now.AddHours(-8).Date.ToString("yyyyMMdd");
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = Path.Combine(DataFolder, $"MSS_{dateKey}.zip");
            var entryNameTemplate = Path.Combine(Path.GetFileNameWithoutExtension(zipFileName), $"MSS_{dateKey}_" + "{0}_{1}.json");

            foreach (var sector in Sectors)
            {
                Logger.AddMessage($"Process {sector} sector");

                byte[] bytes;
                var filename = JsonDataFolder + $@"\{sector}_0.json";
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, sector, 1);
                    var o = Helpers.Download.DownloadToBytes(url, true);
                    if (o is Exception ex)
                        throw new Exception(
                            $"MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                    bytes = (byte[])o;
                    File.WriteAllBytes(filename, bytes);
                }
                else
                    bytes = File.ReadAllBytes(filename);

                var entryName = string.Format(entryNameTemplate, sector, "0");
                var entry = new VirtualFileEntry(entryName, bytes);
                virtualFileEntries.Add(entry);

                var oo = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(bytes);

                for (var k = 1; k < oo.pagination.totalPages;k++)
                {
                    filename = JsonDataFolder + $@"\{sector}_{k}.json";
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, sector, k + 1);
                        var o = Helpers.Download.DownloadToBytes(url, true);
                        if (o is Exception ex)
                            throw new Exception(
                                $"MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                        bytes = (byte[])o;
                        File.WriteAllBytes(filename, bytes);
                    }
                    else
                        bytes = File.ReadAllBytes(filename);

                    entryName = string.Format(entryNameTemplate, sector, k.ToString());
                    entry = new VirtualFileEntry(entryName, bytes);
                    virtualFileEntries.Add(entry);
                }
            }

            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
        }

        #region ============  Json classes  ==============

        private class cRoot
        {
            public cPagination pagination;
            public cResult[] results;
        }

        private class cPagination
        {
            public int count;
            public string page;
            public int totalCount;
            public int totalPages;
        }

        private class cResult
        {
            public cMeta meta;
            public cFields fields;
        }

        private class cMeta
        {
            public string securityID;
            public string performanceID;
            public string companyID;
            public string universe;
            public string exchange;
            public string ticker;
        }

        private class cFields
        {
            public cStringValue name;
            public cStringValue ticker;
        }

        private class cStringValue
        {
            public string value;
        }
        #endregion
    }
}
