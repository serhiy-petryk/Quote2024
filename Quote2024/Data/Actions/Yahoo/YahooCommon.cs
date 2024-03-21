using System;
using System.Collections.Generic;
using System.IO.Compression;
using Data.Actions.Polygon;
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

        public static string GetYahooTickerFromPolygonTicker(string originalPolygonSymbol)
        {
            var s = PolygonCommon.GetMyTicker(originalPolygonSymbol);
            if (s.Contains('^')) s = s.Replace("^", "-P", StringComparison.Ordinal);
            if (s.EndsWith(".U")) s = s.Substring(0, s.Length - 2) + "-UN";
            if (s.EndsWith(".WS")) s = s.Substring(0, s.Length - 3) + "-WT";
            if (s.EndsWith(".WI")) s = s.Substring(0, s.Length - 3) + "-WI";
            if (s.EndsWith(".RT")) s = s.Substring(0, s.Length - 3) + "-RI";
            if (s.EndsWith(".RTW")) s = s.Substring(0, s.Length - 4) + "-RW";
            if (s.Contains(".WS.")) s = s.Replace(".WS.", "-WT", StringComparison.Ordinal);
            if (s.Length >= 3 && s[s.Length - 2] == '.')
            {
                var cc = s.ToCharArray();
                cc[s.Length - 2] = '-';
                s = new string(cc);
            }

            if (s.Contains('.'))
                throw new Exception("Error in GetYahooSymbolFromPolygonSymbol method. Please, check method");

            return s;
        }
    }
}
