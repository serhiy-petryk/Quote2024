using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Scanners
{
    public static class DailyBy5Minutes
    {
        private static readonly TimeSpan StartTime = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan EndTime = new TimeSpan(16, 2, 0);

        public static void Start()
        {
            int GetYear(string zipFileName) =>
                int.Parse(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1].Substring(0, 4));

            var zipFiles = Directory.GetFiles(PolygonCommon.DataFolderMinute, "*.zip")
                .Where(a => GetYear(a)>=2010).OrderBy(a => a).ToList();

            Logger.AddMessage($"Started. Select list of symbols and dates");
            var symbolAndDates = new Dictionary<(string, DateTime), DateTime?>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = $"select a.Symbol, a.Date PrevDate, b.Date NextDate " +
                                  "from dbQ2024..DayPolygon a " +
                                  "inner join dbQ2024..TradingDays b on b.Prev1 = a.Date " +
                                  "where year(a.Date)>=2010 and a.IsTest is null and a.Volume*a.[Close]>= 50000000 and a.TradeCount >= 10000";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        symbolAndDates.Add(((string)rdr["Symbol"], (DateTime)rdr["NextDate"]), (DateTime)rdr["PrevDate"]);
            }

            DbHelper.ExecuteSql($"DELETE dbQ2024MinuteScanner..DailyBy5Minutes");

            var data = new Dictionary<(string, DateTime), DbEntry>();
            var fileCnt = 0;
            foreach (var zipFileName in zipFiles)
            {
                fileCnt++;
                var cnt = 0;
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries
                                 .Where(a => a.Name.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase))
                                 .ToArray())
                    {
                        cnt++;
                        if (cnt % 100 == 0)
                            Logger.AddMessage(
                                $"Processed {cnt} from {zip.Entries.Count} entries in {Path.GetFileName(zipFileName)}. File {fileCnt} from {zipFiles.Count}");

                        var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                        if (oo.adjusted || oo.status != "OK")
                            throw new Exception("Check parser");

                        if (!string.IsNullOrWhiteSpace(oo.next_url))
                            throw new Exception("Check next_url");

                        if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                            continue;

                        var symbol = oo.Symbol;

                        var lastDate = DateTime.MinValue;
                        var valid = false;
                        DbEntry dbEntry = null;
                        for (var k = 0; k < oo.results.Length; k++)
                        {
                            var item = oo.results[k];

                            if (item.DateTime.Date != lastDate)
                            {
                                lastDate = item.DateTime.Date;
                                var key = (symbol, lastDate);
                                valid = !data.ContainsKey(key) && symbolAndDates.ContainsKey(key) && symbolAndDates[key].HasValue;
                                if (valid)
                                {
                                    if (data.Count > 10000)
                                    {
                                        SaveToDb(data.Values);
                                        data.Clear();
                                    }

                                    dbEntry = new DbEntry()
                                        { Symbol = symbol, NextDate = lastDate, PrevDate = symbolAndDates[key].Value };
                                    symbolAndDates[key] = null;

                                    data.Add(key, dbEntry);
                                }
                                else
                                    dbEntry = null;
                            }

                            if (!valid) continue;
                            else
                            {
                                var time = item.DateTime.TimeOfDay;
                                if (time < StartTime || time > EndTime) continue;

                                foreach (var o in dbEntry.Intervals.Where(a => time >= a.From && time < a.To))
                                    o.ProcessQuote(item);
                            }
                        }
                    }
            }

            SaveToDb(data.Values);
            Logger.AddMessage($"!Finished. Files: {zipFiles.Count}");
        }

        private static void SaveToDb(IEnumerable<DbEntry> data)
        {
            Logger.AddMessage($"Save data to database ...");
            DbHelper.SaveToDbTable(data, "dbQ2024MinuteScanner..DailyBy5Minutes", "Symbol", "PrevDate", "NextDate",
                "O0930", "H0930", "L0930", "C0930", "V0930", "N0930", "HG0930", "LG0930",
                "O0931", "H0931", "L0931", "C0931", "V0931", "N0931", "HG0931", "LG0931",
                "O0932", "H0932", "L0932", "C0932", "V0932", "N0932", "HG0932", "LG0932",
                "O0933", "H0933", "L0933", "C0933", "V0933", "N0933", "HG0933", "LG0933",
                "O0934", "H0934", "L0934", "C0934", "V0934", "N0934", "HG0934", "LG0934",
                "O0935", "H0935", "L0935", "C0935", "V0935", "N0935", "HG0935", "LG0935",
                "O0940", "H0940", "L0940", "C0940", "V0940", "N0940", "HG0940", "LG0940",
                "O0945", "H0945", "L0945", "C0945", "V0945", "N0945", "HG0945", "LG0945",
                "O0950", "H0950", "L0950", "C0950", "V0950", "N0950", "HG0950", "LG0950",
                "O0955", "H0955", "L0955", "C0955", "V0955", "N0955", "HG0955", "LG0955",
                "O1000", "H1000", "L1000", "C1000", "V1000", "N1000", "HG1000", "LG1000",
                "O1005", "H1005", "L1005", "C1005", "V1000", "N1005", "HG1005", "LG1005",
                "O1010", "H1010", "L1010", "C1010", "V1010", "N1010", "HG1010", "LG1010",
                "O1015", "H1015", "L1015", "C1015", "V1015", "N1015", "HG1015", "LG1015",
                "O1020", "H1020", "L1020", "C1020", "V1020", "N1020", "HG1020", "LG1020",
                "O1025", "H1025", "L1025", "C1025", "V1025", "N1025", "HG1025", "LG1025",
                "O1520", "H1520", "L1520", "O1525", "H1525", "L1525",
                "O1530", "H1530", "L1530", "O1535", "H1535", "L1535",
                "O1540", "H1540", "L1540", "O1545", "H1545", "L1545",
                "O1550", "H1550", "L1550", "O1555", "H1555", "L1555",
                "O1600", "H1600", "L1600");
        }

        #region ========  SubClasses  =========
        private class DbEntry
        {
            public string Symbol;
            public DateTime PrevDate;
            public DateTime NextDate;
            public float? O0930 => Intervals[0].Open;
            public float? H0930 => Intervals[0].High;
            public float? L0930 => Intervals[0].Low;
            public float? C0930 => Intervals[0].Close;
            public float? HG0930 => Intervals[0].HighGlobal;
            public float? LG0930 => Intervals[0].LowGlobal;
            public float V0930 => Intervals[0].Volume;
            public int N0930 => Intervals[0].Trades;

            public float? O0931 => Intervals[21].Open;
            public float? H0931 => Intervals[21].High;
            public float? L0931 => Intervals[21].Low;
            public float? C0931 => Intervals[21].Close;
            public float? HG0931 => Intervals[21].HighGlobal;
            public float? LG0931 => Intervals[21].LowGlobal;
            public float V0931 => Intervals[21].Volume;
            public int N0931 => Intervals[21].Trades;

            public float? O0932 => Intervals[22].Open;
            public float? H0932 => Intervals[22].High;
            public float? L0932 => Intervals[22].Low;
            public float? C0932 => Intervals[22].Close;
            public float? HG0932 => Intervals[22].HighGlobal;
            public float? LG0932 => Intervals[22].LowGlobal;
            public float V0932 => Intervals[22].Volume;
            public int N0932 => Intervals[22].Trades;

            public float? O0933 => Intervals[23].Open;
            public float? H0933 => Intervals[23].High;
            public float? L0933 => Intervals[23].Low;
            public float? C0933 => Intervals[23].Close;
            public float? HG0933 => Intervals[23].HighGlobal;
            public float? LG0933 => Intervals[23].LowGlobal;
            public float V0933 => Intervals[23].Volume;
            public int N0933 => Intervals[23].Trades;

            public float? O0934 => Intervals[24].Open;
            public float? H0934 => Intervals[24].High;
            public float? L0934 => Intervals[24].Low;
            public float? C0934 => Intervals[24].Close;
            public float? HG0934 => Intervals[24].HighGlobal;
            public float? LG0934 => Intervals[24].LowGlobal;
            public float V0934 => Intervals[24].Volume;
            public int N0934 => Intervals[24].Trades;

            public float? O0935 => Intervals[1].Open;
            public float? H0935 => Intervals[1].High;
            public float? L0935 => Intervals[1].Low;
            public float? C0935 => Intervals[1].Close;
            public float? HG0935 => Intervals[1].HighGlobal;
            public float? LG0935 => Intervals[1].LowGlobal;
            public float V0935 => Intervals[1].Volume;
            public int N0935 => Intervals[1].Trades;

            public float? O0940 => Intervals[2].Open;
            public float? H0940 => Intervals[2].High;
            public float? L0940 => Intervals[2].Low;
            public float? C0940 => Intervals[2].Close;
            public float? HG0940 => Intervals[2].HighGlobal;
            public float? LG0940 => Intervals[2].LowGlobal;
            public float V0940 => Intervals[2].Volume;
            public int N0940 => Intervals[2].Trades;

            public float? O0945 => Intervals[3].Open;
            public float? H0945 => Intervals[3].High;
            public float? L0945 => Intervals[3].Low;
            public float? C0945 => Intervals[3].Close;
            public float? HG0945 => Intervals[3].HighGlobal;
            public float? LG0945 => Intervals[3].LowGlobal;
            public float V0945 => Intervals[3].Volume;
            public int N0945 => Intervals[3].Trades;

            public float? O0950 => Intervals[4].Open;
            public float? H0950 => Intervals[4].High;
            public float? L0950 => Intervals[4].Low;
            public float? C0950 => Intervals[4].Close;
            public float? HG0950 => Intervals[4].HighGlobal;
            public float? LG0950 => Intervals[4].LowGlobal;
            public float V0950 => Intervals[4].Volume;
            public int N0950 => Intervals[4].Trades;

            public float? O0955 => Intervals[5].Open;
            public float? H0955 => Intervals[5].High;
            public float? L0955 => Intervals[5].Low;
            public float? C0955 => Intervals[5].Close;
            public float? HG0955 => Intervals[5].HighGlobal;
            public float? LG0955 => Intervals[5].LowGlobal;
            public float V0955 => Intervals[5].Volume;
            public int N0955 => Intervals[5].Trades;

            public float? O1000 => Intervals[6].Open;
            public float? H1000 => Intervals[6].High;
            public float? L1000 => Intervals[6].Low;
            public float? C1000 => Intervals[6].Close;
            public float? HG1000 => Intervals[6].HighGlobal;
            public float? LG1000 => Intervals[6].LowGlobal;
            public float V1000 => Intervals[6].Volume;
            public int N1000 => Intervals[6].Trades;

            public float? O1005 => Intervals[7].Open;
            public float? H1005 => Intervals[7].High;
            public float? L1005 => Intervals[7].Low;
            public float? C1005 => Intervals[7].Close;
            public float? HG1005 => Intervals[7].HighGlobal;
            public float? LG1005 => Intervals[7].LowGlobal;
            public float V1005 => Intervals[7].Volume;
            public int N1005 => Intervals[7].Trades;

            public float? O1010 => Intervals[8].Open;
            public float? H1010 => Intervals[8].High;
            public float? L1010 => Intervals[8].Low;
            public float? C1010 => Intervals[8].Close;
            public float? HG1010 => Intervals[8].HighGlobal;
            public float? LG1010 => Intervals[8].LowGlobal;
            public float V1010 => Intervals[8].Volume;
            public int N1010 => Intervals[8].Trades;

            public float? O1015 => Intervals[9].Open;
            public float? H1015 => Intervals[9].High;
            public float? L1015 => Intervals[9].Low;
            public float? C1015 => Intervals[9].Close;
            public float? HG1015 => Intervals[9].HighGlobal;
            public float? LG1015 => Intervals[9].LowGlobal;
            public float V1015 => Intervals[9].Volume;
            public int N1015 => Intervals[9].Trades;

            public float? O1020 => Intervals[10].Open;
            public float? H1020 => Intervals[10].High;
            public float? L1020 => Intervals[10].Low;
            public float? C1020 => Intervals[10].Close;
            public float? HG1020 => Intervals[10].HighGlobal;
            public float? LG1020 => Intervals[10].LowGlobal;
            public float V1020 => Intervals[10].Volume;
            public int N1020 => Intervals[10].Trades;

            public float? O1025 => Intervals[11].Open;
            public float? H1025 => Intervals[11].High;
            public float? L1025 => Intervals[11].Low;
            public float? C1025 => Intervals[11].Close;
            public float? HG1025 => Intervals[11].HighGlobal;
            public float? LG1025 => Intervals[11].LowGlobal;
            public float V1025 => Intervals[11].Volume;
            public int N1025 => Intervals[11].Trades;

            // =================
            public float? O1520 => Intervals[12].Open;
            public float? H1520 => Intervals[12].High;
            public float? L1520 => Intervals[12].Low;

            public float? O1525 => Intervals[13].Open;
            public float? H1525 => Intervals[13].High;
            public float? L1525 => Intervals[13].Low;

            public float? O1530 => Intervals[14].Open;
            public float? H1530 => Intervals[14].High;
            public float? L1530 => Intervals[14].Low;

            public float? O1535 => Intervals[15].Open;
            public float? H1535 => Intervals[15].High;
            public float? L1535 => Intervals[15].Low;

            public float? O1540 => Intervals[16].Open;
            public float? H1540 => Intervals[16].High;
            public float? L1540 => Intervals[16].Low;

            public float? O1545 => Intervals[17].Open;
            public float? H1545 => Intervals[17].High;
            public float? L1545 => Intervals[17].Low;

            public float? O1550 => Intervals[18].Open;
            public float? H1550 => Intervals[18].High;
            public float? L1550 => Intervals[18].Low;

            public float? O1555 => Intervals[19].Open;
            public float? H1555 => Intervals[19].High;
            public float? L1555 => Intervals[19].Low;

            public float? O1600 => Intervals[20].Open;
            public float? H1600 => Intervals[20].High;
            public float? L1600 => Intervals[20].Low;

            public DbInterval[] Intervals =
            {
                new DbInterval(new TimeSpan(9, 30, 0)), new DbInterval(new TimeSpan(9, 35, 0)), // 0-1
                new DbInterval(new TimeSpan(9, 40, 0)), new DbInterval(new TimeSpan(9, 45, 0)), // 2-3
                new DbInterval(new TimeSpan(9, 50, 0)), new DbInterval(new TimeSpan(9, 55, 0)), // 4-5
                new DbInterval(new TimeSpan(10, 0, 0)), new DbInterval(new TimeSpan(10, 5, 0)), // 6-7
                new DbInterval(new TimeSpan(10, 10, 0)), new DbInterval(new TimeSpan(10, 15, 0)), // 8-9
                new DbInterval(new TimeSpan(10, 20, 0)), new DbInterval(new TimeSpan(10, 25, 0)), // 10-11
                new DbInterval(new TimeSpan(15, 20, 0), new TimeSpan(15, 25, 0)), //12
                new DbInterval(new TimeSpan(15, 25, 0), new TimeSpan(15, 30, 0)), //13
                new DbInterval(new TimeSpan(15, 30, 0), new TimeSpan(15, 35, 0)), //14
                new DbInterval(new TimeSpan(15, 35, 0), new TimeSpan(15, 40, 0)), //15
                new DbInterval(new TimeSpan(15, 40, 0), new TimeSpan(15, 45, 0)), //16
                new DbInterval(new TimeSpan(15, 45, 0), new TimeSpan(15, 50, 0)), //17
                new DbInterval(new TimeSpan(15, 50, 0), new TimeSpan(15, 55, 0)), //18
                new DbInterval(new TimeSpan(15, 55, 0), new TimeSpan(16, 0, 0)), //19
                new DbInterval(new TimeSpan(16, 0, 0), new TimeSpan(16, 2, 0)), //20
                new DbInterval(new TimeSpan(9, 31, 0)), new DbInterval(new TimeSpan(9, 32, 0)), // 21-22
                new DbInterval(new TimeSpan(9, 33, 0)), new DbInterval(new TimeSpan(9, 34, 0)) // 23-24
            };
        }

        public class DbInterval
        {
            public TimeSpan From;
            public TimeSpan To;
            public TimeSpan ToLocal;
            public float? Open;
            public float? High;
            public float? Low;
            public float? Close;
            public float Volume;
            public int Trades;
            public float? HighGlobal;
            public float? LowGlobal;

            public DbInterval(TimeSpan from)
            {
                From = from;
                To = new TimeSpan(15, 20, 0);
                if (from < new TimeSpan(9, 35, 0))
                    ToLocal = From.Add(new TimeSpan(0, 1, 0));
                else
                    ToLocal = From.Add(new TimeSpan(0, 5, 0));
            }
            public DbInterval(TimeSpan from, TimeSpan to)
            {
                From = from;
                To = to;
                ToLocal = From.Add(new TimeSpan(0, 5, 0));
            }

            public void ProcessQuote(PolygonCommon.cMinuteItem item)
            {
                if (item.DateTime.TimeOfDay < ToLocal)
                {
                    Volume += item.v;
                    Trades += item.n;

                    if (Open.HasValue)
                    {
                        if (High < item.h) High = item.h;
                        if (Low > item.l) Low = item.l;
                        Close = item.c;
                    }
                    else
                    {
                        Open = item.o;
                        High = item.h;
                        Low = item.l;
                        Close = item.c;
                    }
                }

                if (!HighGlobal.HasValue)
                {
                    HighGlobal = item.h;
                    LowGlobal = item.l;
                }
                else
                {
                    if (HighGlobal.Value < item.h) HighGlobal = item.h;
                    if (LowGlobal.Value > item.l) LowGlobal = item.l;
                }
            }
    }
        #endregion
    }
}
