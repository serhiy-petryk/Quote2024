using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Models;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class YahooMinuteChecks
    {
        public static void CheckOnBadQuoteAndSplit(string[] zipFileNames, Action<string> showStatusAction)
        {
            Array.Sort(zipFileNames);

            var priceErrorCount = 0;
            var splitErrorCount = 0;
            var volumeErrorCount = 0;
            foreach (var zipFileName in zipFileNames)
            {
                var priceErrors = new List<string>(new string[]
                {
                    "Price errors:", "\tPrevious price\tSymbol\tTime\tOpen\tHigh\tLow\tClose\tVolume\tDifference factor"
                });
                var volumeErrors = new List<string>(new string[] { null, "Volume errors:" });
                var splitErrors = new List<string>(new string[]
                {
                    "Split errors:",
                    "Symbol\tDate\tHigh\tDbHigh\tLow\tDbLow\tHighFactor\tLowFactor\tDbSplit\t\tDbRatio1\tDbRatio2"
                });

                string lastSymbol = null;
                var lastDate = DateTime.MinValue;
                var lastPrice = 0F;

                MinuteYahoo.ClearCorrections();
                //var quotes = QuoteLoader.MinuteYahoo_GetQuotesFromZipFiles(showStatusAction, new[] { zipFileName }, false, false);
                var cnt = 0;
                var quotes = new List<Quote>();
                var errorLog = new Dictionary<string, string>();
                foreach (var data in YahooCommon.GetMinuteDataFromZipFile(zipFileName, errorLog))
                {
                    cnt++;
                    if ((cnt % 100) == 0)
                        showStatusAction($"CheckOnBadQuoteAndsplit is working for {Path.GetFileName(zipFileName)}. Total file processed: {cnt:N0}");
                    quotes.AddRange(data.Item2);
                }

                var lastQuotes = new List<Quote>();
                Dictionary<Tuple<string, DateTime>, Quote> dbQuotesForWeek = null;
                var minDbQuotesDate = DateTime.MinValue;
                var maxDbQuotesDate = DateTime.MinValue;
                foreach (var q in quotes)
                {
                    if (!string.Equals(q.Symbol, lastSymbol) || !Equals(lastDate, q.Timed.Date))
                    {
                        if (dbQuotesForWeek == null || q.Timed.Date < minDbQuotesDate || q.Timed.Date > maxDbQuotesDate)
                        {
                            dbQuotesForWeek = GetQuoteForWeek(q.Timed.Date);
                            minDbQuotesDate = dbQuotesForWeek.Min(a => a.Key.Item2);
                            maxDbQuotesDate = dbQuotesForWeek.Max(a => a.Key.Item2);
                        }

                        CheckVolume();
                        CheckSplit();
                        lastQuotes.Clear();

                        lastPrice = q.Open;
                        lastSymbol = q.Symbol;
                        lastDate = q.Timed.Date;
                    }

                    if (q.Open > q.High || q.Open < q.Low || q.Close > q.High || q.Close < q.Low || q.Low < 0.0F ||
                        q.Volume < 0)
                        priceErrors.Add(
                            $"Invalid prices or volume.\t{q.Symbol}\t{q.Timed:yyyy-MM-dd HH:mm}\t{q.Open}\t{q.High}\t{q.Low}\t{q.Close}\t{q.Volume}");

                    var ff = CheckDecimalPlaces(q.Open, lastPrice) ?? CheckDecimalPlaces(q.High, lastPrice) ??
                             CheckDecimalPlaces(q.Low, lastPrice) ?? CheckDecimalPlaces(q.Close, lastPrice);
                    if (ff.HasValue)
                    {
                        var qKey = Tuple.Create(lastSymbol, lastDate);
                        var q1 = dbQuotesForWeek.ContainsKey(qKey) ? dbQuotesForWeek[qKey] : null;
                        var max = (new[] { lastPrice, q.Open, q.High, q.Low, q.Close }).Max() - 0.00001;
                        var min = (new[] { lastPrice, q.Open, q.High, q.Low, q.Close }).Min() + 0.00001;
                        if (q1 == null || q1.High <= max || q1.Low >= min)
                        {
                            if (!MinuteYahoo.IsQuotePriceChecked(q))
                            {
                                priceErrors.Add(
                                    $"Unusual price\t{lastPrice}\t{q.Symbol}\t{q.Timed:yyyy-MM-dd HH:mm}\t{q.Open}\t{q.High}\t{q.Low}\t{q.Close}\t{q.Volume}\t{Math.Round(ff.Value, 2)}");
                                priceErrors.Add(
                                    $"Quote from database\t\t{q1?.Symbol}\t{q1?.Timed:yyyy-MM-dd}\t{q1?.Open}\t{q1?.High}\t{q1?.Low}\t{q1?.Close}\t{q1?.Volume}");
                            }
                        }
                    }

                    lastPrice = q.Close;
                    lastQuotes.Add(q);
                }

                CheckVolume();
                CheckSplit();
                lastQuotes.Clear();

                var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
                var folderTimeStamp = ss[ss.Length - 1];
                var priceErrorFileName = Path.Combine(YahooCommon.MinuteYahooLogFolder, $"errorsPrice_{folderTimeStamp}.txt");
                if (priceErrors.Count > 0)
                {
                    if (File.Exists(priceErrorFileName))
                        File.Delete(priceErrorFileName);
                    File.AppendAllLines(priceErrorFileName, priceErrors);
                }

                var volumeErrorFileName = Path.Combine(YahooCommon.MinuteYahooLogFolder, $"errorsVolume_{folderTimeStamp}.txt");
                if (volumeErrors.Count > 0)
                {
                    if (File.Exists(volumeErrorFileName))
                        File.Delete(volumeErrorFileName);
                    File.AppendAllLines(volumeErrorFileName, volumeErrors);
                }

                var splitErrorFileName = Path.Combine(YahooCommon.MinuteYahooLogFolder, $"errorsSplit_{folderTimeStamp}.txt");
                if (splitErrors.Count > 0)
                {
                    if (File.Exists(splitErrorFileName))
                        File.Delete(splitErrorFileName);
                    File.AppendAllLines(splitErrorFileName, splitErrors);
                }

                priceErrorCount += priceErrors.Count;
                splitErrorCount += splitErrors.Count;
                volumeErrorCount += volumeErrors.Count;

                //============================
                void CheckSplit()
                {
                    if (string.IsNullOrWhiteSpace(lastSymbol)) return;
                    if (MinuteYahoo.IsQuoteSplitChecked(new Quote { Symbol = lastSymbol, Timed = lastDate }))
                        return;

                    var qKey = Tuple.Create(lastSymbol, lastDate);
                    var quote = dbQuotesForWeek.ContainsKey(qKey) ? dbQuotesForWeek[qKey] : (Quote)null;
                    if (quote == null)
                        return;
                    var high = lastQuotes.Max(a => a.High);
                    var low = lastQuotes.Min(a => a.Low);
                    if (high <= 0.1) return;
                    var highK = quote.High / high;
                    var lowK = quote.Low / low;
                    if (highK > 1.1F || highK < 0.9F || lowK > 1.1F || lowK < 0.9F)
                    {
                        var dbSplit = GetSplitsForWeek(lastSymbol, lastDate);
                        if (dbSplit != null)
                        {
                            var aa1 = dbSplit.Item1.Split(':');
                            splitErrors.Add(
                                $"{quote.Symbol}\t{quote.Timed.Date:yyyy-MM-dd}\t{high}\t{quote.High}\t{low}\t{quote.Low}\t{highK}\t{lowK}\t{dbSplit.Item2}\tSPLIT\t{aa1[0]}\t{aa1[1]}");
                        }
                        else
                            splitErrors.Add($"{quote.Symbol}\t{quote.Timed.Date:yyyy-MM-dd}\t{high}\t{quote.High}\t{low}\t{quote.Low}\t{highK}\t{lowK}");
                    }
                }

                void CheckVolume()
                {
                    // return; // Not active because there are a lot of corrections
                    if (string.IsNullOrWhiteSpace(lastSymbol)) return;

                    var volume = lastQuotes.Sum(a => a.Volume);
                    var qKey = Tuple.Create(lastSymbol, lastDate);
                    var lastQuote = dbQuotesForWeek.ContainsKey(qKey) ? dbQuotesForWeek[qKey] : (Quote)null;

                    if (lastSymbol == "NOVA" && lastDate == new DateTime(2022, 11, 10))
                    {

                    }
                    if (volume < 300000 || (lastQuote != null && lastQuote.Volume >= volume))
                        return;

                    var vQuotes = lastQuotes.Where(a => a.Volume >= 100000).OrderBy(a => a.Volume).ToArray();
                    for (var k = 1; k < vQuotes.Length; k++)
                    {
                        var volumeK = 1.0 * vQuotes[k].Volume / vQuotes[k - 1].Volume;
                        if (volumeK > 1.7)
                        {
                            volumeErrors.Add(
                                $"Unusual volume. Previous volume is {vQuotes[k - 1].Volume}.\t{vQuotes[k].Symbol}\t{vQuotes[k].Timed:yyyy-MM-dd HH:mm}\t{vQuotes[k].Open}\t{vQuotes[k].High}\t{vQuotes[k].Low}\t{vQuotes[k].Close}\t{vQuotes[k].Volume}\tIntraday\\EodVolume:\t{volume}\t{lastQuote?.Volume}\t{Math.Round(volumeK, 2)}");
                            for (var k2 = k + 1; k2 < vQuotes.Length; k2++)
                            {
                                volumeErrors.Add(
                                    $"Unusual volume quote list.\t{vQuotes[k2].Symbol}\t{vQuotes[k2].Timed:yyyy-MM-dd HH:mm}\t{vQuotes[k2].Open}\t{vQuotes[k2].High}\t{vQuotes[k2].Low}\t{vQuotes[k2].Close}\t{vQuotes[k2].Volume}\tIntraday\\EodVolume:\t{volume}\t{lastQuote?.Volume}");
                            }

                            break;
                        }
                    }
                }
            }

            showStatusAction($"MinuteYahoo_ErrorCheck FINISHED! Found {(priceErrorCount - 2) / 2}, {splitErrorCount - 2}, {volumeErrorCount - 2} errors. Error logs in folder: {YahooCommon.MinuteYahooLogFolder}");

            #region ===========  Check methods  ===========
            float? CheckDecimalPlaces(float price, float prevPrice)
            {
                var a1 = price / prevPrice;
                if (price <= 0.1001F && prevPrice <= 0.1001F)
                    return null;

                if (price <= 0.5F || prevPrice <= 0.5F)
                {
                    if (a1 > 0.15F && a1 < 7F) return null;
                    else if (a1 < 1F) return 1 / a1;
                    else return a1;
                }

                if (a1 > 0.5F && a1 < 2F) return null;
                else if (a1 <= 1F) return 1 / a1;
                else return a1;
            }

            #endregion

            #region ===========  Db methods  ===========
            Tuple<string, float> GetSplitsForWeek(string symbol, DateTime fromDate)
            {
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT Ratio, K from dbQ2024..Splits WHERE symbol='{symbol}' and  date >='{fromDate:yyyy-MM-dd}' and date<'{fromDate.AddDays(7):yyyy-MM-dd}'";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                            return Tuple.Create((string)rdr["Ratio"], Convert.ToSingle(rdr["K"]));
                }
                return null;
            }
            Dictionary<Tuple<string, DateTime>, Quote> GetQuoteForWeek(DateTime fromDate)
            {
                var quotes2 = new Dictionary<Tuple<string, DateTime>, Quote>();
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT isnull(b.YahooSymbol, a.Symbol) Symbol, a.Date, a.[Open], a.High, a.Low, a.[Close], a.Volume " +
                                      $"from dbQ2024..DayPolygon a left join dbQ2024..SymbolsPolygon b on a.Symbol = b.Symbol and "+
                                      "a.Date between b.Date and isnull(b.[To], '2099-12-31') " +
                                      $"WHERE a.date >= '{fromDate:yyyy-MM-dd}' and a.date<'{fromDate.AddDays(7):yyyy-MM-dd}'";
                    using (var rdr = cmd.ExecuteReader())
                        while (rdr.Read())
                        {
                            var q = new Quote
                            {
                                Symbol = (string)rdr["Symbol"],
                                Timed = (DateTime)rdr["Date"],
                                Open = (float)rdr["Open"],
                                High = (float)rdr["High"],
                                Low = (float)rdr["Low"],
                                Close = (float)rdr["Close"],
                                Volume = Convert.ToInt64((float)rdr["Volume"])
                            };
                            quotes2.Add(Tuple.Create(q.Symbol, q.Timed), q);
                        }
                }
                return quotes2;
            }
            #endregion
        }

    }
}
