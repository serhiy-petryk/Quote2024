﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var symbolXref = GetSymbolXref();
            DownloadItems(symbolXref, timeStamp.Item2);

            Helpers.Logger.AddMessage($"Finished. Items: {symbolXref.Count:N0}");
        }

        public static List<(string,string)> GetUrlAndFilenames(List<YahooSectorItem> symbolItems, string dateKey)
        {
            const int chunkSize = 20;
            var tempFilename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, "");
            var folder = Path.GetDirectoryName(tempFilename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var urlAndFilenames = new List<(string, string)>();
            var symbolsToDownload = new List<YahooSectorItem>();
            var chunkCount = 0;
            foreach (var item in symbolItems)
            {
                symbolsToDownload.Add(item);
                if (symbolsToDownload.Count == chunkSize)
                {
                    var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                    var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload.Select(a => a.YahooSymbol)));
                    urlAndFilenames.Add((url,filename));
                    symbolsToDownload.Clear();
                    chunkCount++;
                }
            }

            if (symbolsToDownload.Count > 0)
            {
                var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload.Select(a => a.YahooSymbol)));
                urlAndFilenames.Add((url, filename));
                symbolsToDownload.Clear();
                chunkCount++;
            }

            return urlAndFilenames;
        }

        public static void DownloadItems(Dictionary<string,string> symbolXref, string dateKey)
        {
            const int chunkSize = 20;

            var tempFilename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, "");
            var folder = Path.GetDirectoryName(tempFilename);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var symbolsToDownload = new List<string>();
            var count = 0;
            var chunkCount = 0;
            foreach (var kvp in symbolXref)
            {
                count++;
                symbolsToDownload.Add(kvp.Key);
                if (symbolsToDownload.Count == chunkSize)
                {
                    var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                    if (!File.Exists(filename))
                    {
                        var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                        var o = WebClientExt.GetToBytes(url, true);
                        if (o.Item3 != null)
                            throw new Exception(
                                $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                        File.WriteAllBytes(filename, o.Item1);
                    }

                    Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbolXref.Count:N0}");

                    symbolsToDownload.Clear();
                    chunkCount++;
                }
            }

            if (symbolsToDownload.Count > 0)
            {
                var filename = string.Format(FileNameTemplate, chunkSize.ToString(), dateKey, chunkCount.ToString());
                if (!File.Exists(filename))
                {
                    var url = string.Format(UrlTemplate, string.Join(',', symbolsToDownload));
                    var o = WebClientExt.GetToBytes(url, true);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"YahooSectorLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                    File.WriteAllBytes(filename, o.Item1);
                }

                Helpers.Logger.AddMessage($"Downloaded sectors for {count} items from {symbolXref.Count:N0}");

                symbolsToDownload.Clear();
                chunkCount++;
            }

        }

        private static Dictionary<string, string> GetSymbolXref()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>=dateadd(month, -1, GetDate()) and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null";
//                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
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
