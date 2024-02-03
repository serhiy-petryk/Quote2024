using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Polygon
{
    public static class PolygonMinuteLogSaveToDb
    {
        public static void Start()
        {
            var zipFiles = Directory.GetFiles(PolygonCommon.DataFolderMinute, "*.zip").ToList();

            var loggedFolders = new Dictionary<string, object>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandTimeout = 300;
                cmd.CommandText = $"SELECT DISTINCT Folder FROM dbQ2024Minute..MinutePolygonLog";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        loggedFolders.Add((string) rdr["Folder"], null);
            }

            foreach(var zipFileName in zipFiles.ToArray())
                if (loggedFolders.ContainsKey(Path.GetFileNameWithoutExtension(zipFileName).ToUpper()))
                    zipFiles.Remove(zipFileName);

            var totalErrors = 0;
            foreach (var zipFileName in zipFiles)
            {
                var folderId = Path.GetFileNameWithoutExtension(zipFileName);

                Logger.AddMessage($"Started. Delete old log in database for {folderId} folder");
                DbUtils.ExecuteSql($"DELETE dbQ2024Minute..MinutePolygonLog WHERE folder='{folderId}'");
                DbUtils.ExecuteSql($"DELETE dbQ2024Minute..MinutePolygonLog_BlankFiles WHERE folder='{folderId}'");

                var errorLog = new List<string>();
                ProcessZipFile(zipFileName, errorLog);

                if (errorLog.Count > 0)
                {
                    totalErrors += errorLog.Count;
                    // Save errors to file
                    var errorFolder = Path.GetDirectoryName(zipFileName) + @"\Errors\";
                    if (!Directory.Exists(errorFolder))
                        Directory.CreateDirectory(errorFolder);
                    var errorFileName = errorFolder + Path.GetFileNameWithoutExtension(zipFileName) + ".txt";
                    if (File.Exists(errorFileName))
                        File.Delete(errorFileName);
                    File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
                    File.AppendAllLines(errorFileName, errorLog);

                    Logger.AddMessage($"!Finished. Found {errorLog.Count} errors. Error filename: {errorFileName}");
                }
            }

            if (totalErrors > 0)
                Logger.AddMessage($"!Finished. Files: {zipFiles.Count}. Found {totalErrors} errors");
            else
                Logger.AddMessage($"!Finished. No errors. Files: {zipFiles.Count}.");
        }

        public static void StartForZipFile(string zipFileName)
        {
            var folderId = Path.GetFileNameWithoutExtension(zipFileName);

            Logger.AddMessage($"Started. Delete old log in database.");
            DbUtils.ExecuteSql($"DELETE dbQ2024Minute..MinutePolygonLog WHERE folder='{folderId}'");
            DbUtils.ExecuteSql($"DELETE dbQ2024Minute..MinutePolygonLog_BlankFiles WHERE folder='{folderId}'");

            var errorLog = new List<string>();
            ProcessZipFile(zipFileName, errorLog);

            if (errorLog.Count > 0)
            {
                // Save errors to file
                var errorFolder = Path.GetDirectoryName(zipFileName) + @"\Errors\";
                if (!Directory.Exists(errorFolder))
                    Directory.CreateDirectory(errorFolder);
                var errorFileName = errorFolder + Path.GetFileNameWithoutExtension(zipFileName) + ".txt";
                if (File.Exists(errorFileName))
                    File.Delete(errorFileName);
                File.AppendAllText(errorFileName, $"File\tMessage\tContent{Environment.NewLine}");
                File.AppendAllLines(errorFileName, errorLog);

                Logger.AddMessage($"!Finished. Found {errorLog.Count} errors. Error filename: {errorFileName}");
            }
            else
                Logger.AddMessage($"!Finished. No errors.");
        }

        private static void ProcessZipFile(string zipFileName, List<string> errorLog)
        {
            var folderId = Path.GetFileNameWithoutExtension(zipFileName);
            var log = new List<LogEntry>();
            var blankFiles = new List<BlankFile>();

            var cnt = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Name.EndsWith(".json", StringComparison.InvariantCultureIgnoreCase)).ToArray())
                {
                    cnt++;
                    if (cnt % 100 == 0)
                        Logger.AddMessage($"Processed {cnt} from {zip.Entries.Count} entries in {Path.GetFileName(zipFileName)}");

                    var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                    if (oo.adjusted || oo.status != "OK")
                        throw new Exception("Check parser");

                    if (!string.IsNullOrEmpty(oo.next_url))
                    {
                        Debug.Print($"NEXT URL:\t{entry.Name}\t{oo.next_url}");
                        errorLog.Add($"{entry.Name}\tPartial downloading\tNext url: {oo.next_url}");
                    }

                    if (oo.count == 0 && (oo.results == null || oo.results.Length == 0))
                    {
                        blankFiles.Add(new BlankFile
                        {
                            Folder = folderId,
                            FileName = entry.Name,
                            FileCreated = entry.LastWriteTime.DateTime,
                            Symbol = oo.Symbol
                        });
                        continue;
                    }

                    LogEntry logEntry = null;
                    var lastDate = DateTime.MinValue;
                    var endTrading = TimeSpan.Zero;
                    var endTradingIn = TimeSpan.Zero;
                    for (var k = 0; k < oo.results.Length; k++)
                    {
                        var item = oo.results[k];

                        if (item.DateTime.Date != lastDate)
                        {
                            if (log.Count > 100000)
                                SaveToDb(log, blankFiles);

                            var position = (logEntry == null ? (byte)2 : (byte)3); // First, Middle
                            logEntry = new LogEntry
                            {
                                Folder = folderId,
                                FileName = entry.Name,
                                Symbol = oo.Symbol,
                                Date = item.DateTime.Date,
                                Position = position,
                                // Status = oo.status,
                                Created = entry.LastWriteTime.DateTime
                            };
                            log.Add(logEntry);
                            logEntry.MinTime = item.DateTime.TimeOfDay;

                            lastDate = item.DateTime.Date;
                            endTrading = Data.Settings.GetMarketEndTime(logEntry.Date);
                            endTradingIn = endTrading - new TimeSpan(0, 0, 15, 0);
                        }

                        // check item
                        if (item.o > item.h || item.o < item.l || item.c > item.h || item.c < item.l || item.l < 0)
                        {
                            errorLog.Add($"{entry.Name}\tBad prices in {k} item: {item.ToString()}");
                            logEntry._errors++;
                        }
                        if (item.v < 0)
                        {
                            errorLog.Add($"{entry.Name}\tBad volume in {k} item: {item.ToString()}");
                            logEntry._errors++;
                        }
                        if (item.v == 0 && item.h != item.l)
                        {
                            errorLog.Add($"{entry.Name}\tPrices are not equal when volume=0 in {k} line. Item: {item.ToString()}");
                            logEntry._errors++;
                        }

                        logEntry.CountFull++;
                        logEntry.VolumeFull += item.v;
                        logEntry.TradeCountFull += item.n;
                        logEntry.MaxTime = item.DateTime.TimeOfDay;

                        if (item.DateTime.TimeOfDay >= Settings.MarketStart && item.DateTime.TimeOfDay < endTrading)
                        {
                            logEntry.Count++;
                            logEntry.Volume += item.v;
                            logEntry.TradeCount += item.n;
                            if (logEntry.Count == 1)
                            {
                                logEntry.Open = item.o;
                                logEntry.High = item.h;
                                logEntry.Low = item.l;
                                logEntry.Close = item.c;
                            }
                            else
                            {
                                logEntry.Close = item.c;
                                if (item.h > logEntry.High) logEntry.High = item.h;
                                if (item.l < logEntry.Low) logEntry.Low = item.l;
                            }
                        }
                        if (item.DateTime.TimeOfDay >= Settings.MarketStart && item.DateTime.TimeOfDay < Settings.MarketStartIn)
                        {
                            logEntry.CountBefore++;
                            logEntry.VolumeBefore += item.v;
                            logEntry.TradeCountBefore += item.n;
                            if (logEntry.CountBefore == 1)
                            {
                                logEntry.HighBefore = item.h;
                                logEntry.LowBefore = item.l;
                            }
                            else
                            {
                                if (item.h > logEntry.HighBefore) logEntry.HighBefore = item.h;
                                if (item.l < logEntry.LowBefore) logEntry.LowBefore = item.l;
                            }
                        }
                        if (item.DateTime.TimeOfDay >= Settings.MarketStartIn && item.DateTime.TimeOfDay < endTradingIn)
                        {
                            logEntry.CountIn++;
                            logEntry.VolumeIn += item.v;
                            logEntry.TradeCountIn += item.n;
                            if (logEntry.CountIn == 1)
                            {
                                logEntry.OpenIn = item.o;
                                logEntry.HighIn = item.h;
                                logEntry.LowIn = item.l;
                                logEntry.CloseIn = item.c;
                            }
                            else
                            {
                                logEntry.CloseIn = item.c;
                                if (item.h > logEntry.HighIn) logEntry.HighIn = item.h;
                                if (item.l < logEntry.LowIn) logEntry.LowIn = item.l;
                            }
                        }
                        if (item.DateTime.TimeOfDay >= endTradingIn && item.DateTime.TimeOfDay < endTrading && !logEntry.FinalIn.HasValue)
                            logEntry.FinalIn = item.o;
                    }

                    if (logEntry != null)
                    {
                        if (oo.next_url != null)
                            throw new Exception($"Partial loading in {Path.GetFileName(zipFileName)}.{entry.Name}. Please, decrease date range while downloading.");
                        logEntry.Position = 4;
                    }
                }

            if (log.Count > 0 || blankFiles.Count > 0)
                SaveToDb(log, blankFiles);
        }

        private static void SaveToDb(List<LogEntry> log, List<BlankFile> blankFiles)
        {
            Logger.AddMessage($"Save data to database ...");
            DbUtils.SaveToDbTable(log, "dbQ2024Minute..MinutePolygonLog", "Folder", "Symbol", "Date",
                "MinTime", "MaxTime", "Count", "CountFull", "Open", "High", "Low", "Close", "Volume", "VolumeFull",
                "TradeCount", "TradeCountFull", "HighBefore", "LowBefore", "VolumeBefore", "CountBefore",
                "TradeCountBefore", "OpenIn", "HighIn", "LowIn", "CloseIn", "FinalIn", "VolumeIn", "CountIn",
                "TradeCountIn", "Errors", "Position", "RowStatus", "Created", "TimeStamp");

            DbUtils.SaveToDbTable(blankFiles, "dbQ2024Minute..MinutePolygonLog_BlankFiles", "Folder", "FileName",
                "FileCreated", "Symbol");

            log.Clear();
            blankFiles.Clear();
        }

        #region ========  SubClasses  =========
        private class BlankFile
        {
            public string Folder;
            public string FileName;
            public DateTime FileCreated;
            public string Symbol;
        }

        private class LogEntry
        {
            internal byte _errors;

            public string Folder;
            public string FileName;
            public string Symbol;
            public DateTime Date;
            public TimeSpan MinTime;
            public TimeSpan MaxTime;
            public short Count;
            public short CountFull;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public float Volume;
            public float VolumeFull;
            public int TradeCount;
            public int TradeCountFull;
            public float HighBefore;
            public float LowBefore;
            public float VolumeBefore;
            public byte CountBefore;
            public int TradeCountBefore;
            public float OpenIn;
            public float HighIn;
            public float LowIn;
            public float CloseIn;
            public float? FinalIn;
            public float VolumeIn;
            public short CountIn;
            public int TradeCountIn;
            public byte? Errors => _errors == 0 ? (byte?)null : _errors;
            public byte Position;
            public byte RowStatus => 0; // not defined
            public DateTime Created;
            public DateTime TimeStamp => DateTime.Now;
        }
        #endregion

    }
}
