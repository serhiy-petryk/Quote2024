using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Tests
{
    public static class RealTimeYahooMinuteTests
    {
        private const string Folder = @"E:\Quote\WebData\RealTime\YahooMinute\2024-03-21";

        public static void TestTradingPeriod()
        {
            var zipFileNames = Directory.GetFiles(Actions.Yahoo.YahooCommon.MinuteYahooDataFolder, "*.zip");
            var errroLog = new Dictionary<string, string>();
            foreach (var zipFileName in zipFileNames)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");
                foreach (var x in Actions.Yahoo.YahooCommon.GetMinuteDataFromZipFile(zipFileName, errroLog).Take(1))
                {
                    var aa = x.Item2;
                }
            }
            Logger.AddMessage($"!!!Finished");
        }

        public static void Start()
        {
            var zipFiles = Directory.GetFiles(Folder, "*.zip").OrderBy(a=>a).ToArray();
            foreach (var zipFileName in zipFiles)
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries)
                    {
                        using (var sr = new StreamReader(entry.Open()))
                        {
                            var json = sr.ReadToEnd();
                            // var oo1 = JsonConvert.DeserializeObject<Models.MinuteYahoo>(json);
                            var oo2 = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<Models.MinuteYahoo2>(json);
                            //   Console.WriteLine(sr.ReadToEnd());
                        }
                        /*// var symbol = entry.Name.Substring(5, entry.Name.Length - 9).ToUpper();
                        // var oo = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                         if (oo.chart.error != null)
                         {
                             //errorLog.Add(symbol, o.chart.error.description ?? o.chart.error.code);
                             // continue;
                         }

                        // yield return (symbol, o.GetQuotes(symbol));*/
                    }

        }
    }
}
