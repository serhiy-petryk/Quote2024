using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace Tests.RealTime
{
    public static class RealTimeYahooMinute
    {
        private const string Folder = @"E:\Quote\WebData\RealTime\YahooMinute\2024-03-21";
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
                            var oo1 = JsonConvert.DeserializeObject<Models.MinuteYahoo>(json);
                            var oo2 = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<Models.MinuteYahoo>(json);
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
