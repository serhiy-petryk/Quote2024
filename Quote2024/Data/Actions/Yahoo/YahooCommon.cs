using System;
using System.Collections.Generic;
using System.IO.Compression;
using Data.Models;

namespace Data.Actions.Yahoo
{
    public class YahooCommon
    {
        public const string MinuteYahooDataFolder = @"D:\Quote\WebData\Minute\Yahoo\Data\";
        public const string MinuteYahooLogFolder = @"D:\Quote\WebData\Minute\Yahoo\Logs\";
        public const string MinuteYahooCorrectionFiles = MinuteYahooDataFolder + "YahooMinuteCorrections.txt";

        public static IEnumerable<(string, List<Quote>)> GetMinuteDataFromZipFile(string zipFileName, Dictionary<string, string> errorLog)
        {
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        if (!entry.Name.StartsWith("YMIN-", StringComparison.InvariantCultureIgnoreCase))
                            throw new Exception($"Bad file name in {zipFileName}: {entry.Name}. Must starts with 'yMin-'");

                        var symbol = entry.Name.Substring(5, entry.Name.Length - 9).ToUpper();
                        var o = Helpers.ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
                        if (o.chart.error != null)
                        {
                            errorLog.Add(symbol, o.chart.error.description ?? o.chart.error.code);
                            continue;
                        }

                        yield return (symbol, o.GetQuotes(symbol));
                    }
        }
    }
}
