using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net;
using Data.Actions.Polygon;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Yahoo
{
    public class YahooCommon
    {
        public const string MinuteYahooDataFolder = Settings.DataFolder + @"Minute\Yahoo\Data\";
        public const string MinuteYahooLogFolder = Settings.DataFolder + @"Minute\Yahoo\Logs\";
        public const string MinuteYahooCorrectionFiles = MinuteYahooDataFolder + "YahooMinuteCorrections.txt";

        private static CookieCollection _yahooCookies = null;
        private static string _crumb;

        public static System.Net.CookieCollection GetYahooCookies(bool refresh)
        {
            if (_yahooCookies == null || refresh)
            {
                // var url = @"https://fc.yahoo.com/";
                // var url = @"https://finance.yahoo.com/quote/MSFT/profile/";
                // var url = @"https://finance.yahoo.com/screener/predefined/sec-ind_sec-largest-equities_basic-materials";
                var urlX2 = @"https://query1.finance.yahoo.com/ws/insights/v3/finance/insights?disableRelatedReports=true&formatted=true&getAllResearchReports=false&reportsCount=0&ssl=true&symbols=A&lang=en-US&region=US";
                var urlX1 = @"https://query1.finance.yahoo.com/ws/insights/v3/finance/insights?disableRelatedReports=true&formatted=true&getAllResearchReports=false&reportsCount=0&ssl=true&symbols=A,AA,MSFT,EZY&lang=en-US&region=US";

                var url = @"https://finance.yahoo.com";
                var o1 = WebClientExt.GetToBytes(url, false, false, new CookieCollection());
                var content1 = System.Text.Encoding.UTF8.GetString(o1.Item1);
                _yahooCookies = o1.Item2;

                /*var crambUrl = "https://query2.finance.yahoo.com/v1/test/getcrumb";
                var o2 = WebClientExt.GetToBytes(crambUrl, false, false, _yahooCookies);
                _crumb = System.Text.Encoding.UTF8.GetString(o2.Item1);*/

                // var o3 = WebClientExt.GetToBytes(urlX1, false, false, _yahooCookies);
                var o3 = WebClientExt.GetToBytes(urlX1, false);
                var sectorContent = System.Text.Encoding.UTF8.GetString(o3.Item1);
            }

            return _yahooCookies;
        }



        public static IEnumerable<(string, List<Quote>)> GetMinuteDataFromZipFile(string zipFileName, Dictionary<string, string> errorLog)
        {
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries)
                    if (entry.Length > 0)
                    {
                        if (!entry.Name.StartsWith("YMIN-", StringComparison.InvariantCultureIgnoreCase))
                            throw new Exception($"Bad file name in {zipFileName}: {entry.Name}. Must starts with 'yMin-'");

                        var symbol = entry.Name.Substring(5, entry.Name.Length - 9).ToUpper();
                        var o = ZipUtils.DeserializeZipEntry<Models.MinuteYahoo>(entry);
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
