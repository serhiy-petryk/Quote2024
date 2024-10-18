using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using Data.Actions.Polygon;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Scanners
{
    public static class DailyBy5Minutes
    {
        private static readonly TimeSpan BeforeNotInInterval = new TimeSpan(9, 0, 0);
        private static readonly TimeSpan AfterNotInInterval = new TimeSpan(16, 2, 0);

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
                            else if (!valid) continue;
                            else
                            {
                                var time = item.DateTime.TimeOfDay;
                                if (time < BeforeNotInInterval || time > AfterNotInInterval) continue;

                                foreach(var o in dbEntry.Intervals.Where(a=>time>=a.From && time<a.To))
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
                "O0930", "H0930", "L0930", "O0935", "H0935", "L0935",
                "O0940", "H0940", "L0940", "O0945", "H0945", "L0945",
                "O0950", "H0950", "L0950", "O0955", "H0955", "L0955",
                "O1000", "H1000", "L1000", "O1005", "H1005", "L1005",
                "O1010", "H1010", "L1010", "O1015", "H1015", "L1015",
                "O1020", "H1020", "L1020", "O1025", "H1025", "L1025",

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

            public float? O0935 => Intervals[1].Open;
            public float? H0935 => Intervals[1].High;
            public float? L0935 => Intervals[1].Low;
            
            public float? O0940 => Intervals[2].Open;
            public float? H0940 => Intervals[2].High;
            public float? L0940 => Intervals[2].Low;
            
            public float? O0945 => Intervals[3].Open;
            public float? H0945 => Intervals[3].High;
            public float? L0945 => Intervals[3].Low;

            public float? O0950 => Intervals[4].Open;
            public float? H0950 => Intervals[4].High;
            public float? L0950 => Intervals[4].Low;
            
            public float? O0955 => Intervals[5].Open;
            public float? H0955 => Intervals[5].High;
            public float? L0955 => Intervals[5].Low;

            public float? O1000 => Intervals[6].Open;
            public float? H1000 => Intervals[6].High;
            public float? L1000 => Intervals[6].Low;

            public float? O1005 => Intervals[7].Open;
            public float? H1005 => Intervals[7].High;
            public float? L1005 => Intervals[7].Low;

            public float? O1010 => Intervals[8].Open;
            public float? H1010 => Intervals[8].High;
            public float? L1010 => Intervals[8].Low;

            public float? O1015 => Intervals[9].Open;
            public float? H1015 => Intervals[9].High;
            public float? L1015 => Intervals[9].Low;

            public float? O1020 => Intervals[10].Open;
            public float? H1020 => Intervals[10].High;
            public float? L1020 => Intervals[10].Low;

            public float? O1025 => Intervals[11].Open;
            public float? H1025 => Intervals[11].High;
            public float? L1025 => Intervals[11].Low;

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
                new DbInterval(new TimeSpan(16, 0, 0), new TimeSpan(16, 2, 0)) //20
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

            public DbInterval(TimeSpan from)
            {
                From = from;
                To = new TimeSpan(15, 20, 0);
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
                if (Open.HasValue)
                {
                    if (High.Value < item.h) High = item.h;
                    if (Low.Value > item.l) Low = item.l;
                }
                else if (item.DateTime.TimeOfDay < ToLocal)
                {
                    Open = item.o;
                    High = item.h;
                    Low = item.l;
                }
            }
        }
        #endregion
    }
}
