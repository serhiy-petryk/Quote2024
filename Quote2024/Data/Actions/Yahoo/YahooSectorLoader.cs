using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class YahooSectorLoader
    {
        private const string UrlTemplate = @"https://query1.finance.yahoo.com/ws/insights/v3/finance/insights?disableRelatedReports=true&formatted=true&getAllResearchReports=false&reportsCount=0&ssl=true&symbols={0}&lang=en-US&region=US";
        private const string FileNameTemplate = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_{1}\YS_{0}_{2}_{1}.json";

        public static void Parse()
        {
            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Sectors\Data\YS_20240701";
            var files = Directory.GetFiles(folder, "*.json");
            foreach (var file in files)
            {
                var o = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(File.ReadAllBytes(file));
            }

        }

        public static void Start()
        {
            Logger.AddMessage($"Started");
            var timeStamp = TimeHelper.GetTimeStamp();
            var symbolItems = GetSymbolsAndExchanges().OrderBy(a => a.YahooSymbol).ToList();
            DownloadItems(symbolItems, timeStamp.Item2);

            Helpers.Logger.AddMessage($"Finished. Items: {symbolItems.Count:N0}");
        }

        public static void DownloadItems(List<YahooSectorItem> symbolItems, string dateKey)
        {
            const int chunkSize = 20;

            var tempFilename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, "");
            var folder = Path.GetDirectoryName(tempFilename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var symbolsToDownload = new List<YahooSectorItem>();
            var count = 0;
            var chunkCount = 0;
            foreach (var item in symbolItems)
            {
                count++;
                symbolsToDownload.Add(item);
                if (symbolsToDownload.Count == chunkSize)
                {
                    var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload.Select(a => a.YahooSymbol)));
                        var o = WebClientExt.GetToBytes(url, true);
                        if (o.Item3 != null)
                            throw new Exception(
                                $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                        File.WriteAllBytes(filename, o.Item1);
                    }

                    Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbolItems.Count:N0}");

                    symbolsToDownload.Clear();
                    chunkCount++;
                }
            }

            if (symbolsToDownload.Count > 0)
            {
                var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload.Select(a => a.YahooSymbol)));
                    var o = WebClientExt.GetToBytes(url, true);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                    File.WriteAllBytes(filename, o.Item1);
                }

                Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbolItems.Count:N0}");

                symbolsToDownload.Clear();
                chunkCount++;
            }

        }

        private static List<YahooSectorItem> GetSymbolsAndExchanges()
        {
            var data = new List<YahooSectorItem>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var item = new YahooSectorItem((string)rdr["symbol"], (string)rdr["YahooSymbol"]);
                        if (!string.IsNullOrEmpty(item.YahooSymbol))
                            data.Add(item);
                    }
            }

            return data;
        }

        public class YahooSectorItem
        {
            public string PolygonSymbol;
            public string YahooSymbol;

            public YahooSectorItem(string polygonSymbol, string yahooSymbol)
            {
                PolygonSymbol = polygonSymbol;
                YahooSymbol = yahooSymbol;
            }
        }

        #region =======  Json classes  =========

        private class cRoot
        {
            public cFinance finance;
        }

        private class cFinance
        {
            public cResult[] result;
        }
        private class cResult
        {
            public string symbol;
            public cCompanySnapshot companySnapshot;
            public cUpsell upsell;
        }

        private class cCompanySnapshot
        {
            public string sectorInfo;
        }

        private class cUpsell
        {
            public string companyName;
        }
        #endregion
    }
}
