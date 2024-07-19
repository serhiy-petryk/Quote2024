using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.MorningStar
{
    public static class WA_MorningStarProfile
    {
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://www.morningstar.com/stocks/{0}/{1}/quote&matchType=prefix&limit=100000&from=2019";
        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\MorningStar\WA_List";

        public static void DownloadList()
        {
            Logger.AddMessage($"Started");
            var exchangeAndSymbols = GetExchangeAndSymbols();
            // var symbols = new Dictionary<string,string>{{"MSFT", "MSFT80"}};
            var count = 0;
            foreach (var exchangeAndSymbol in exchangeAndSymbols)
            {
                count++;
                Logger.AddMessage($"Process {count} symbols from {exchangeAndSymbols.Count}");
                var filename = Path.Combine($@"{ListDataFolder}", $"{exchangeAndSymbol.Key.Item1}_{exchangeAndSymbol.Key.Item2}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplate, exchangeAndSymbol.Key.Item1, exchangeAndSymbol.Key.Item2);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"WA_MorningStarProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                    File.WriteAllBytes(filename, o.Item1);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        //                    var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(polygonSymbol);
        private static Dictionary<(string, string), object> GetExchangeAndSymbols()
        {
            var data = new Dictionary<(string, string), object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT exchange, symbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') order by 2";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var exchange = ((string)rdr["exchange"]).ToLower();
                        var symbol = (string)rdr["symbol"];
                        var msSymbol = MorningStarCommon.GetMorningStarProfileTicker(symbol).ToLower();
                        if (!string.IsNullOrEmpty(msSymbol))
                        {
                            var key = (exchange, msSymbol);
                            if (!data.ContainsKey(key))
                                data.Add(key, null);
                            var secondSymbol = (MorningStarCommon.GetSecondMorningStarProfileTicker(symbol) ?? "").ToLower();
                            if (!string.IsNullOrEmpty(secondSymbol))
                            {
                                var secondKey = (exchange, secondSymbol);
                                if (!data.ContainsKey(secondKey))
                                    data.Add(secondKey, null);
                            }
                        }
                    }
            }

            return data;
        }


    }
}
