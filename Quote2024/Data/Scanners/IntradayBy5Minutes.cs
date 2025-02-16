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
    public static class IntradayBy5Minutes
    {
        private static readonly TimeSpan StartTime = new TimeSpan(9, 30, 0);
        private static readonly TimeSpan EndTime = new TimeSpan(16, 0, 0);
        private const float MinTurnover = 0.5f;
        private const int MinTradeCount = 100;

        public static void Start()
        {
            Logger.AddMessage($"Started. Select list of symbols and dates");
            var folderAndSymbolAndDates = new Dictionary<string, Dictionary<(string, DateTime), object>>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = $"SELECT Folder, Symbol, a.Date "+
                                  "FROM [dbQ2024Minute].[dbo].[MinutePolygonLog] a "+
                                  "inner join dbQ2024..TradingDays b on a.Date = b.Date "+
                                  "WHERE RowStatus in (2, 5) AND year(a.date)>= 2010 and "+
                                  "high>=5.0 and (High-Low)/(High+Low)*200>=5.0 and "+
                                  $"Volume*High>={MinTurnover*1000000f} and TradeCount>={MinTradeCount} and "+
                                  "b.IsShortened is null and MinTime < '12:00'";

                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                    {
                        var folder = (string)rdr["Folder"];
                        var symbolAndDate = ((string)rdr["Symbol"], (DateTime)rdr["Date"]);
                        if (!folderAndSymbolAndDates.ContainsKey(folder))
                            folderAndSymbolAndDates.Add(folder, new Dictionary<(string, DateTime), object>());
                        folderAndSymbolAndDates[folder].Add(symbolAndDate, null);
                    }
            }

            DbHelper.ExecuteSql($"DELETE dbQ2024MinuteScanner..IntradayBy5Minutes");

            var data = new List<DbEntry>();
            var fileCnt = 0;
            var zipFiles = Directory.GetFiles(PolygonCommon.DataFolderMinute, "*.zip").OrderBy(a => a).ToList();

            foreach (var zipFileName in zipFiles)
            {
                fileCnt++;
                if (!folderAndSymbolAndDates.ContainsKey(Path.GetFileNameWithoutExtension(zipFileName))) continue;

                var symbolAndDates = folderAndSymbolAndDates[Path.GetFileNameWithoutExtension(zipFileName)];
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
                                valid = symbolAndDates.ContainsKey(key);
                                if (valid)
                                {
                                    if (data.Count > 10000)
                                    {
                                        SaveToDb(data);
                                        data.Clear();
                                    }

                                    dbEntry = new DbEntry { Symbol = symbol, Date = lastDate };
                                    data.Add(dbEntry);
                                }
                                else
                                    dbEntry = null;
                            }

                            if (!valid) continue;
                            else
                            {
                                var minutes = Convert.ToInt32((item.DateTime.TimeOfDay - StartTime).TotalMinutes);
                                var index = minutes / 5;
                                if (minutes >= 0 && index < dbEntry.Intervals.Count)
                                    dbEntry.Intervals[index].ProcessQuote(item);

                                /*if (minutes >= 0 && symbol == "AA" && item.DateTime.Date == new DateTime(2010, 1, 12))
                                {
                                    var x1 = dbEntry.MaxTurnover;
                                }*/
                            }
                        }
                    }
            }

            SaveToDb(data);
            Logger.AddMessage($"!Finished. Files: {zipFiles.Count}");
        }

        private static void SaveToDb(IEnumerable<DbEntry> data)
        {
            Logger.AddMessage($"Save data to database ...");
            DbHelper.SaveToDbTable(data.Where(a=>a.MaxTurnover>=MinTurnover && a.MaxTradeCount>=MinTradeCount),
                "dbQ2024MinuteScanner..IntradayBy5Minutes", "Symbol", "Date", "MaxTurnover", "MaxTradeCount",
                "O0930", "H0930", "L0930", "C0930", "V0930", "N0930", "O0935", "H0935", "L0935", "C0935", "V0935", "N0935",
                "O0940", "H0940", "L0940", "C0940", "V0940", "N0940", "O0945", "H0945", "L0945", "C0945", "V0945", "N0945",
                "O0950", "H0950", "L0950", "C0950", "V0950", "N0950", "O0955", "H0955", "L0955", "C0955", "V0955", "N0955",
                "O1000", "H1000", "L1000", "C1000", "V1000", "N1000", "O1005", "H1005", "L1005", "C1005", "V1005", "N1005",
                "O1010", "H1010", "L1010", "C1010", "V1010", "N1010", "O1015", "H1015", "L1015", "C1015", "V1015", "N1015",
                "O1020", "H1020", "L1020", "C1020", "V1020", "N1020", "O1025", "H1025", "L1025", "C1025", "V1025", "N1025",
                "O1030", "H1030", "L1030", "C1030", "V1030", "N1030", "O1035", "H1035", "L1035", "C1035", "V1035", "N1035",
                "O1040", "H1040", "L1040", "C1040", "V1040", "N1040", "O1045", "H1045", "L1045", "C1045", "V1045", "N1045",
                "O1050", "H1050", "L1050", "C1050", "V1050", "N1050", "O1055", "H1055", "L1055", "C1055", "V1055", "N1055",
                "O1100", "H1100", "L1100", "C1100", "V1100", "N1100", "O1105", "H1105", "L1105", "C1105", "V1105", "N1105",
                "O1110", "H1110", "L1110", "C1110", "V1110", "N1110", "O1115", "H1115", "L1115", "C1115", "V1115", "N1115",
                "O1120", "H1120", "L1120", "C1120", "V1120", "N1120", "O1125", "H1125", "L1125", "C1125", "V1125", "N1125",
                "O1130", "H1130", "L1130", "C1130", "V1130", "N1130", "O1135", "H1135", "L1135", "C1135", "V1135", "N1135",
                "O1140", "H1140", "L1140", "C1140", "V1140", "N1140", "O1145", "H1145", "L1145", "C1145", "V1145", "N1145",
                "O1150", "H1150", "L1150", "C1150", "V1150", "N1150", "O1155", "H1155", "L1155", "C1155", "V1155", "N1155",
                "O1200", "H1200", "L1200", "C1200", "V1200", "N1200", "O1205", "H1205", "L1205", "C1205", "V1205", "N1205",
                "O1210", "H1210", "L1210", "C1210", "V1210", "N1210", "O1215", "H1215", "L1215", "C1215", "V1215", "N1215",
                "O1220", "H1220", "L1220", "C1220", "V1220", "N1220", "O1225", "H1225", "L1225", "C1225", "V1225", "N1225",
                "O1230", "H1230", "L1230", "C1230", "V1230", "N1230", "O1235", "H1235", "L1235", "C1235", "V1235", "N1235",
                "O1240", "H1240", "L1240", "C1240", "V1240", "N1240", "O1245", "H1245", "L1245", "C1245", "V1245", "N1245",
                "O1250", "H1250", "L1250", "C1250", "V1250", "N1250", "O1255", "H1255", "L1255", "C1255", "V1255", "N1255",
                "O1300", "H1300", "L1300", "C1300", "V1300", "N1300", "O1305", "H1305", "L1305", "C1305", "V1305", "N1305",
                "O1310", "H1310", "L1310", "C1310", "V1310", "N1310", "O1315", "H1315", "L1315", "C1315", "V1315", "N1315",
                "O1320", "H1320", "L1320", "C1320", "V1320", "N1320", "O1325", "H1325", "L1325", "C1325", "V1325", "N1325",
                "O1330", "H1330", "L1330", "C1330", "V1330", "N1330", "O1335", "H1335", "L1335", "C1335", "V1335", "N1335",
                "O1340", "H1340", "L1340", "C1340", "V1340", "N1340", "O1345", "H1345", "L1345", "C1345", "V1345", "N1345",
                "O1350", "H1350", "L1350", "C1350", "V1350", "N1350", "O1355", "H1355", "L1355", "C1355", "V1355", "N1355",
                "O1400", "H1400", "L1400", "C1400", "V1400", "N1400", "O1405", "H1405", "L1405", "C1405", "V1405", "N1405",
                "O1410", "H1410", "L1410", "C1410", "V1410", "N1410", "O1415", "H1415", "L1415", "C1415", "V1415", "N1415",
                "O1420", "H1420", "L1420", "C1420", "V1420", "N1420", "O1425", "H1425", "L1425", "C1425", "V1425", "N1425",
                "O1430", "H1430", "L1430", "C1430", "V1430", "N1430", "O1435", "H1435", "L1435", "C1435", "V1435", "N1435",
                "O1440", "H1440", "L1440", "C1440", "V1440", "N1440", "O1445", "H1445", "L1445", "C1445", "V1445", "N1445",
                "O1450", "H1450", "L1450", "C1450", "V1450", "N1450", "O1455", "H1455", "L1455", "C1455", "V1455", "N1455",
                "O1500", "H1500", "L1500", "C1500", "V1500", "N1500", "O1505", "H1505", "L1505", "C1505", "V1505", "N1505",
                "O1510", "H1510", "L1510", "C1510", "V1510", "N1510", "O1515", "H1515", "L1515", "C1515", "V1515", "N1515",
                "O1520", "H1520", "L1520", "C1520", "V1520", "N1520", "O1525", "H1525", "L1525", "C1525", "V1525", "N1525",
                "O1530", "H1530", "L1530", "C1530", "V1530", "N1530", "O1535", "H1535", "L1535", "C1535", "V1535", "N1535",
                "O1540", "H1540", "L1540", "C1540", "V1540", "N1540", "O1545", "H1545", "L1545", "C1545", "V1545", "N1545",
                "O1550", "H1550", "L1550", "C1550", "V1550", "N1550", "O1555", "H1555", "L1555", "C1555", "V1555", "N1555");
        }

        #region ========  SubClasses  =========

        private class DbEntry
        {
            public string Symbol;
            public DateTime Date;
            public float MaxTurnover => Intervals.Max(a => a.Volume * ((a.Low ?? 0.0f) + (a.High ?? 0.0f)) / 2000000f);
            public int MaxTradeCount => Intervals.Max(a => a.Trades);

            public float? O0930 => Intervals[0].Open;
            public float? H0930 => Intervals[0].High;
            public float? L0930 => Intervals[0].Low;
            public float? C0930 => Intervals[0].Close;
            public float V0930 => Intervals[0].Volume;
            public int N0930 => Intervals[0].Trades;
            public float? O0935 => Intervals[1].Open;
            public float? H0935 => Intervals[1].High;
            public float? L0935 => Intervals[1].Low;
            public float? C0935 => Intervals[1].Close;
            public float V0935 => Intervals[1].Volume;
            public int N0935 => Intervals[1].Trades;
            public float? O0940 => Intervals[2].Open;
            public float? H0940 => Intervals[2].High;
            public float? L0940 => Intervals[2].Low;
            public float? C0940 => Intervals[2].Close;
            public float V0940 => Intervals[2].Volume;
            public int N0940 => Intervals[2].Trades;
            public float? O0945 => Intervals[3].Open;
            public float? H0945 => Intervals[3].High;
            public float? L0945 => Intervals[3].Low;
            public float? C0945 => Intervals[3].Close;
            public float V0945 => Intervals[3].Volume;
            public int N0945 => Intervals[3].Trades;
            public float? O0950 => Intervals[4].Open;
            public float? H0950 => Intervals[4].High;
            public float? L0950 => Intervals[4].Low;
            public float? C0950 => Intervals[4].Close;
            public float V0950 => Intervals[4].Volume;
            public int N0950 => Intervals[4].Trades;
            public float? O0955 => Intervals[5].Open;
            public float? H0955 => Intervals[5].High;
            public float? L0955 => Intervals[5].Low;
            public float? C0955 => Intervals[5].Close;
            public float V0955 => Intervals[5].Volume;
            public int N0955 => Intervals[5].Trades;
            public float? O1000 => Intervals[6].Open;
            public float? H1000 => Intervals[6].High;
            public float? L1000 => Intervals[6].Low;
            public float? C1000 => Intervals[6].Close;
            public float V1000 => Intervals[6].Volume;
            public int N1000 => Intervals[6].Trades;
            public float? O1005 => Intervals[7].Open;
            public float? H1005 => Intervals[7].High;
            public float? L1005 => Intervals[7].Low;
            public float? C1005 => Intervals[7].Close;
            public float V1005 => Intervals[7].Volume;
            public int N1005 => Intervals[7].Trades;
            public float? O1010 => Intervals[8].Open;
            public float? H1010 => Intervals[8].High;
            public float? L1010 => Intervals[8].Low;
            public float? C1010 => Intervals[8].Close;
            public float V1010 => Intervals[8].Volume;
            public int N1010 => Intervals[8].Trades;
            public float? O1015 => Intervals[9].Open;
            public float? H1015 => Intervals[9].High;
            public float? L1015 => Intervals[9].Low;
            public float? C1015 => Intervals[9].Close;
            public float V1015 => Intervals[9].Volume;
            public int N1015 => Intervals[9].Trades;
            public float? O1020 => Intervals[10].Open;
            public float? H1020 => Intervals[10].High;
            public float? L1020 => Intervals[10].Low;
            public float? C1020 => Intervals[10].Close;
            public float V1020 => Intervals[10].Volume;
            public int N1020 => Intervals[10].Trades;
            public float? O1025 => Intervals[11].Open;
            public float? H1025 => Intervals[11].High;
            public float? L1025 => Intervals[11].Low;
            public float? C1025 => Intervals[11].Close;
            public float V1025 => Intervals[11].Volume;
            public int N1025 => Intervals[11].Trades;
            public float? O1030 => Intervals[12].Open;
            public float? H1030 => Intervals[12].High;
            public float? L1030 => Intervals[12].Low;
            public float? C1030 => Intervals[12].Close;
            public float V1030 => Intervals[12].Volume;
            public int N1030 => Intervals[12].Trades;
            public float? O1035 => Intervals[13].Open;
            public float? H1035 => Intervals[13].High;
            public float? L1035 => Intervals[13].Low;
            public float? C1035 => Intervals[13].Close;
            public float V1035 => Intervals[13].Volume;
            public int N1035 => Intervals[13].Trades;
            public float? O1040 => Intervals[14].Open;
            public float? H1040 => Intervals[14].High;
            public float? L1040 => Intervals[14].Low;
            public float? C1040 => Intervals[14].Close;
            public float V1040 => Intervals[14].Volume;
            public int N1040 => Intervals[14].Trades;
            public float? O1045 => Intervals[15].Open;
            public float? H1045 => Intervals[15].High;
            public float? L1045 => Intervals[15].Low;
            public float? C1045 => Intervals[15].Close;
            public float V1045 => Intervals[15].Volume;
            public int N1045 => Intervals[15].Trades;
            public float? O1050 => Intervals[16].Open;
            public float? H1050 => Intervals[16].High;
            public float? L1050 => Intervals[16].Low;
            public float? C1050 => Intervals[16].Close;
            public float V1050 => Intervals[16].Volume;
            public int N1050 => Intervals[16].Trades;
            public float? O1055 => Intervals[17].Open;
            public float? H1055 => Intervals[17].High;
            public float? L1055 => Intervals[17].Low;
            public float? C1055 => Intervals[17].Close;
            public float V1055 => Intervals[17].Volume;
            public int N1055 => Intervals[17].Trades;
            public float? O1100 => Intervals[18].Open;
            public float? H1100 => Intervals[18].High;
            public float? L1100 => Intervals[18].Low;
            public float? C1100 => Intervals[18].Close;
            public float V1100 => Intervals[18].Volume;
            public int N1100 => Intervals[18].Trades;
            public float? O1105 => Intervals[19].Open;
            public float? H1105 => Intervals[19].High;
            public float? L1105 => Intervals[19].Low;
            public float? C1105 => Intervals[19].Close;
            public float V1105 => Intervals[19].Volume;
            public int N1105 => Intervals[19].Trades;
            public float? O1110 => Intervals[20].Open;
            public float? H1110 => Intervals[20].High;
            public float? L1110 => Intervals[20].Low;
            public float? C1110 => Intervals[20].Close;
            public float V1110 => Intervals[20].Volume;
            public int N1110 => Intervals[20].Trades;
            public float? O1115 => Intervals[21].Open;
            public float? H1115 => Intervals[21].High;
            public float? L1115 => Intervals[21].Low;
            public float? C1115 => Intervals[21].Close;
            public float V1115 => Intervals[21].Volume;
            public int N1115 => Intervals[21].Trades;
            public float? O1120 => Intervals[22].Open;
            public float? H1120 => Intervals[22].High;
            public float? L1120 => Intervals[22].Low;
            public float? C1120 => Intervals[22].Close;
            public float V1120 => Intervals[22].Volume;
            public int N1120 => Intervals[22].Trades;
            public float? O1125 => Intervals[23].Open;
            public float? H1125 => Intervals[23].High;
            public float? L1125 => Intervals[23].Low;
            public float? C1125 => Intervals[23].Close;
            public float V1125 => Intervals[23].Volume;
            public int N1125 => Intervals[23].Trades;
            public float? O1130 => Intervals[24].Open;
            public float? H1130 => Intervals[24].High;
            public float? L1130 => Intervals[24].Low;
            public float? C1130 => Intervals[24].Close;
            public float V1130 => Intervals[24].Volume;
            public int N1130 => Intervals[24].Trades;
            public float? O1135 => Intervals[25].Open;
            public float? H1135 => Intervals[25].High;
            public float? L1135 => Intervals[25].Low;
            public float? C1135 => Intervals[25].Close;
            public float V1135 => Intervals[25].Volume;
            public int N1135 => Intervals[25].Trades;
            public float? O1140 => Intervals[26].Open;
            public float? H1140 => Intervals[26].High;
            public float? L1140 => Intervals[26].Low;
            public float? C1140 => Intervals[26].Close;
            public float V1140 => Intervals[26].Volume;
            public int N1140 => Intervals[26].Trades;
            public float? O1145 => Intervals[27].Open;
            public float? H1145 => Intervals[27].High;
            public float? L1145 => Intervals[27].Low;
            public float? C1145 => Intervals[27].Close;
            public float V1145 => Intervals[27].Volume;
            public int N1145 => Intervals[27].Trades;
            public float? O1150 => Intervals[28].Open;
            public float? H1150 => Intervals[28].High;
            public float? L1150 => Intervals[28].Low;
            public float? C1150 => Intervals[28].Close;
            public float V1150 => Intervals[28].Volume;
            public int N1150 => Intervals[28].Trades;
            public float? O1155 => Intervals[29].Open;
            public float? H1155 => Intervals[29].High;
            public float? L1155 => Intervals[29].Low;
            public float? C1155 => Intervals[29].Close;
            public float V1155 => Intervals[29].Volume;
            public int N1155 => Intervals[29].Trades;
            public float? O1200 => Intervals[30].Open;
            public float? H1200 => Intervals[30].High;
            public float? L1200 => Intervals[30].Low;
            public float? C1200 => Intervals[30].Close;
            public float V1200 => Intervals[30].Volume;
            public int N1200 => Intervals[30].Trades;
            public float? O1205 => Intervals[31].Open;
            public float? H1205 => Intervals[31].High;
            public float? L1205 => Intervals[31].Low;
            public float? C1205 => Intervals[31].Close;
            public float V1205 => Intervals[31].Volume;
            public int N1205 => Intervals[31].Trades;
            public float? O1210 => Intervals[32].Open;
            public float? H1210 => Intervals[32].High;
            public float? L1210 => Intervals[32].Low;
            public float? C1210 => Intervals[32].Close;
            public float V1210 => Intervals[32].Volume;
            public int N1210 => Intervals[32].Trades;
            public float? O1215 => Intervals[33].Open;
            public float? H1215 => Intervals[33].High;
            public float? L1215 => Intervals[33].Low;
            public float? C1215 => Intervals[33].Close;
            public float V1215 => Intervals[33].Volume;
            public int N1215 => Intervals[33].Trades;
            public float? O1220 => Intervals[34].Open;
            public float? H1220 => Intervals[34].High;
            public float? L1220 => Intervals[34].Low;
            public float? C1220 => Intervals[34].Close;
            public float V1220 => Intervals[34].Volume;
            public int N1220 => Intervals[34].Trades;
            public float? O1225 => Intervals[35].Open;
            public float? H1225 => Intervals[35].High;
            public float? L1225 => Intervals[35].Low;
            public float? C1225 => Intervals[35].Close;
            public float V1225 => Intervals[35].Volume;
            public int N1225 => Intervals[35].Trades;
            public float? O1230 => Intervals[36].Open;
            public float? H1230 => Intervals[36].High;
            public float? L1230 => Intervals[36].Low;
            public float? C1230 => Intervals[36].Close;
            public float V1230 => Intervals[36].Volume;
            public int N1230 => Intervals[36].Trades;
            public float? O1235 => Intervals[37].Open;
            public float? H1235 => Intervals[37].High;
            public float? L1235 => Intervals[37].Low;
            public float? C1235 => Intervals[37].Close;
            public float V1235 => Intervals[37].Volume;
            public int N1235 => Intervals[37].Trades;
            public float? O1240 => Intervals[38].Open;
            public float? H1240 => Intervals[38].High;
            public float? L1240 => Intervals[38].Low;
            public float? C1240 => Intervals[38].Close;
            public float V1240 => Intervals[38].Volume;
            public int N1240 => Intervals[38].Trades;
            public float? O1245 => Intervals[39].Open;
            public float? H1245 => Intervals[39].High;
            public float? L1245 => Intervals[39].Low;
            public float? C1245 => Intervals[39].Close;
            public float V1245 => Intervals[39].Volume;
            public int N1245 => Intervals[39].Trades;
            public float? O1250 => Intervals[40].Open;
            public float? H1250 => Intervals[40].High;
            public float? L1250 => Intervals[40].Low;
            public float? C1250 => Intervals[40].Close;
            public float V1250 => Intervals[40].Volume;
            public int N1250 => Intervals[40].Trades;
            public float? O1255 => Intervals[41].Open;
            public float? H1255 => Intervals[41].High;
            public float? L1255 => Intervals[41].Low;
            public float? C1255 => Intervals[41].Close;
            public float V1255 => Intervals[41].Volume;
            public int N1255 => Intervals[41].Trades;
            public float? O1300 => Intervals[42].Open;
            public float? H1300 => Intervals[42].High;
            public float? L1300 => Intervals[42].Low;
            public float? C1300 => Intervals[42].Close;
            public float V1300 => Intervals[42].Volume;
            public int N1300 => Intervals[42].Trades;
            public float? O1305 => Intervals[43].Open;
            public float? H1305 => Intervals[43].High;
            public float? L1305 => Intervals[43].Low;
            public float? C1305 => Intervals[43].Close;
            public float V1305 => Intervals[43].Volume;
            public int N1305 => Intervals[43].Trades;
            public float? O1310 => Intervals[44].Open;
            public float? H1310 => Intervals[44].High;
            public float? L1310 => Intervals[44].Low;
            public float? C1310 => Intervals[44].Close;
            public float V1310 => Intervals[44].Volume;
            public int N1310 => Intervals[44].Trades;
            public float? O1315 => Intervals[45].Open;
            public float? H1315 => Intervals[45].High;
            public float? L1315 => Intervals[45].Low;
            public float? C1315 => Intervals[45].Close;
            public float V1315 => Intervals[45].Volume;
            public int N1315 => Intervals[45].Trades;
            public float? O1320 => Intervals[46].Open;
            public float? H1320 => Intervals[46].High;
            public float? L1320 => Intervals[46].Low;
            public float? C1320 => Intervals[46].Close;
            public float V1320 => Intervals[46].Volume;
            public int N1320 => Intervals[46].Trades;
            public float? O1325 => Intervals[47].Open;
            public float? H1325 => Intervals[47].High;
            public float? L1325 => Intervals[47].Low;
            public float? C1325 => Intervals[47].Close;
            public float V1325 => Intervals[47].Volume;
            public int N1325 => Intervals[47].Trades;
            public float? O1330 => Intervals[48].Open;
            public float? H1330 => Intervals[48].High;
            public float? L1330 => Intervals[48].Low;
            public float? C1330 => Intervals[48].Close;
            public float V1330 => Intervals[48].Volume;
            public int N1330 => Intervals[48].Trades;
            public float? O1335 => Intervals[49].Open;
            public float? H1335 => Intervals[49].High;
            public float? L1335 => Intervals[49].Low;
            public float? C1335 => Intervals[49].Close;
            public float V1335 => Intervals[49].Volume;
            public int N1335 => Intervals[49].Trades;
            public float? O1340 => Intervals[50].Open;
            public float? H1340 => Intervals[50].High;
            public float? L1340 => Intervals[50].Low;
            public float? C1340 => Intervals[50].Close;
            public float V1340 => Intervals[50].Volume;
            public int N1340 => Intervals[50].Trades;
            public float? O1345 => Intervals[51].Open;
            public float? H1345 => Intervals[51].High;
            public float? L1345 => Intervals[51].Low;
            public float? C1345 => Intervals[51].Close;
            public float V1345 => Intervals[51].Volume;
            public int N1345 => Intervals[51].Trades;
            public float? O1350 => Intervals[52].Open;
            public float? H1350 => Intervals[52].High;
            public float? L1350 => Intervals[52].Low;
            public float? C1350 => Intervals[52].Close;
            public float V1350 => Intervals[52].Volume;
            public int N1350 => Intervals[52].Trades;
            public float? O1355 => Intervals[53].Open;
            public float? H1355 => Intervals[53].High;
            public float? L1355 => Intervals[53].Low;
            public float? C1355 => Intervals[53].Close;
            public float V1355 => Intervals[53].Volume;
            public int N1355 => Intervals[53].Trades;
            public float? O1400 => Intervals[54].Open;
            public float? H1400 => Intervals[54].High;
            public float? L1400 => Intervals[54].Low;
            public float? C1400 => Intervals[54].Close;
            public float V1400 => Intervals[54].Volume;
            public int N1400 => Intervals[54].Trades;
            public float? O1405 => Intervals[55].Open;
            public float? H1405 => Intervals[55].High;
            public float? L1405 => Intervals[55].Low;
            public float? C1405 => Intervals[55].Close;
            public float V1405 => Intervals[55].Volume;
            public int N1405 => Intervals[55].Trades;
            public float? O1410 => Intervals[56].Open;
            public float? H1410 => Intervals[56].High;
            public float? L1410 => Intervals[56].Low;
            public float? C1410 => Intervals[56].Close;
            public float V1410 => Intervals[56].Volume;
            public int N1410 => Intervals[56].Trades;
            public float? O1415 => Intervals[57].Open;
            public float? H1415 => Intervals[57].High;
            public float? L1415 => Intervals[57].Low;
            public float? C1415 => Intervals[57].Close;
            public float V1415 => Intervals[57].Volume;
            public int N1415 => Intervals[57].Trades;
            public float? O1420 => Intervals[58].Open;
            public float? H1420 => Intervals[58].High;
            public float? L1420 => Intervals[58].Low;
            public float? C1420 => Intervals[58].Close;
            public float V1420 => Intervals[58].Volume;
            public int N1420 => Intervals[58].Trades;
            public float? O1425 => Intervals[59].Open;
            public float? H1425 => Intervals[59].High;
            public float? L1425 => Intervals[59].Low;
            public float? C1425 => Intervals[59].Close;
            public float V1425 => Intervals[59].Volume;
            public int N1425 => Intervals[59].Trades;
            public float? O1430 => Intervals[60].Open;
            public float? H1430 => Intervals[60].High;
            public float? L1430 => Intervals[60].Low;
            public float? C1430 => Intervals[60].Close;
            public float V1430 => Intervals[60].Volume;
            public int N1430 => Intervals[60].Trades;
            public float? O1435 => Intervals[61].Open;
            public float? H1435 => Intervals[61].High;
            public float? L1435 => Intervals[61].Low;
            public float? C1435 => Intervals[61].Close;
            public float V1435 => Intervals[61].Volume;
            public int N1435 => Intervals[61].Trades;
            public float? O1440 => Intervals[62].Open;
            public float? H1440 => Intervals[62].High;
            public float? L1440 => Intervals[62].Low;
            public float? C1440 => Intervals[62].Close;
            public float V1440 => Intervals[62].Volume;
            public int N1440 => Intervals[62].Trades;
            public float? O1445 => Intervals[63].Open;
            public float? H1445 => Intervals[63].High;
            public float? L1445 => Intervals[63].Low;
            public float? C1445 => Intervals[63].Close;
            public float V1445 => Intervals[63].Volume;
            public int N1445 => Intervals[63].Trades;
            public float? O1450 => Intervals[64].Open;
            public float? H1450 => Intervals[64].High;
            public float? L1450 => Intervals[64].Low;
            public float? C1450 => Intervals[64].Close;
            public float V1450 => Intervals[64].Volume;
            public int N1450 => Intervals[64].Trades;
            public float? O1455 => Intervals[65].Open;
            public float? H1455 => Intervals[65].High;
            public float? L1455 => Intervals[65].Low;
            public float? C1455 => Intervals[65].Close;
            public float V1455 => Intervals[65].Volume;
            public int N1455 => Intervals[65].Trades;
            public float? O1500 => Intervals[66].Open;
            public float? H1500 => Intervals[66].High;
            public float? L1500 => Intervals[66].Low;
            public float? C1500 => Intervals[66].Close;
            public float V1500 => Intervals[66].Volume;
            public int N1500 => Intervals[66].Trades;
            public float? O1505 => Intervals[67].Open;
            public float? H1505 => Intervals[67].High;
            public float? L1505 => Intervals[67].Low;
            public float? C1505 => Intervals[67].Close;
            public float V1505 => Intervals[67].Volume;
            public int N1505 => Intervals[67].Trades;
            public float? O1510 => Intervals[68].Open;
            public float? H1510 => Intervals[68].High;
            public float? L1510 => Intervals[68].Low;
            public float? C1510 => Intervals[68].Close;
            public float V1510 => Intervals[68].Volume;
            public int N1510 => Intervals[68].Trades;
            public float? O1515 => Intervals[69].Open;
            public float? H1515 => Intervals[69].High;
            public float? L1515 => Intervals[69].Low;
            public float? C1515 => Intervals[69].Close;
            public float V1515 => Intervals[69].Volume;
            public int N1515 => Intervals[69].Trades;
            public float? O1520 => Intervals[70].Open;
            public float? H1520 => Intervals[70].High;
            public float? L1520 => Intervals[70].Low;
            public float? C1520 => Intervals[70].Close;
            public float V1520 => Intervals[70].Volume;
            public int N1520 => Intervals[70].Trades;
            public float? O1525 => Intervals[71].Open;
            public float? H1525 => Intervals[71].High;
            public float? L1525 => Intervals[71].Low;
            public float? C1525 => Intervals[71].Close;
            public float V1525 => Intervals[71].Volume;
            public int N1525 => Intervals[71].Trades;
            public float? O1530 => Intervals[72].Open;
            public float? H1530 => Intervals[72].High;
            public float? L1530 => Intervals[72].Low;
            public float? C1530 => Intervals[72].Close;
            public float V1530 => Intervals[72].Volume;
            public int N1530 => Intervals[72].Trades;
            public float? O1535 => Intervals[73].Open;
            public float? H1535 => Intervals[73].High;
            public float? L1535 => Intervals[73].Low;
            public float? C1535 => Intervals[73].Close;
            public float V1535 => Intervals[73].Volume;
            public int N1535 => Intervals[73].Trades;
            public float? O1540 => Intervals[74].Open;
            public float? H1540 => Intervals[74].High;
            public float? L1540 => Intervals[74].Low;
            public float? C1540 => Intervals[74].Close;
            public float V1540 => Intervals[74].Volume;
            public int N1540 => Intervals[74].Trades;
            public float? O1545 => Intervals[75].Open;
            public float? H1545 => Intervals[75].High;
            public float? L1545 => Intervals[75].Low;
            public float? C1545 => Intervals[75].Close;
            public float V1545 => Intervals[75].Volume;
            public int N1545 => Intervals[75].Trades;
            public float? O1550 => Intervals[76].Open;
            public float? H1550 => Intervals[76].High;
            public float? L1550 => Intervals[76].Low;
            public float? C1550 => Intervals[76].Close;
            public float V1550 => Intervals[76].Volume;
            public int N1550 => Intervals[76].Trades;
            public float? O1555 => Intervals[77].Open;
            public float? H1555 => Intervals[77].High;
            public float? L1555 => Intervals[77].Low;
            public float? C1555 => Intervals[77].Close;
            public float V1555 => Intervals[77].Volume;
            public int N1555 => Intervals[77].Trades;

            public List<DbInterval> Intervals = new List<DbInterval>();

            public DbEntry()
            {
                var ts = StartTime;
                while (ts < EndTime)
                {
                    var nextTime = ts.Add(new TimeSpan(0, 5, 0));
                    Intervals.Add(new DbInterval(ts, nextTime));
                    ts = nextTime;
                }
            }
        }

        public class DbInterval
        {
            public TimeSpan From;
            public TimeSpan To;
            public float? Open;
            public float? High;
            public float? Low;
            public float? Close;
            public float Volume;
            public int Trades;

            public DbInterval(TimeSpan from, TimeSpan to)
            {
                From = from;
                To = to;
            }

            public void ProcessQuote(PolygonCommon.cMinuteItem item)
            {
                if (item.DateTime.TimeOfDay >= From && item.DateTime.TimeOfDay < To)
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
                else throw new Exception("Check");
            }
    }
        #endregion
    }
}
