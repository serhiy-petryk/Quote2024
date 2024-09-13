using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class YahooMinuteSaveLogToDb
    {
        public class LogEntry
        {
            public string File;
            public string Symbol;
            public DateTime Date;
            public TimeSpan MinTime;
            public TimeSpan MaxTime;
            public short Count;
            public float Open;
            public float High;
            public float Low;
            public float Close;
            public float Volume;
        }

        public static void Start(string[] zipFiles, Action<string> showStatusAction)
        {
            var cnt = 0;
            foreach (var zipFileName in zipFiles)
            {
                showStatusAction($"YahooMinuteSaveLogToDb is working for {Path.GetFileName(zipFileName)}");

                var log = new List<LogEntry>();

                // delete old log in database
                using (var conn = new SqlConnection(Settings.DbConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText =
                        $"DELETE dbQ2024..FileLogMinuteYahoo WHERE [file]='{Path.GetFileNameWithoutExtension(zipFileName)}'";
                    cmd.ExecuteNonQuery();
                }

                var errorLog = new Dictionary<string, string>();
                foreach (var data in YahooCommon.GetMinuteDataFromZipFile(zipFileName, errorLog))
                {
                    cnt++;
                    if ((cnt % 100) == 0)
                        showStatusAction(
                            $"YahooMinuteSaveLogToDb is working with {Path.GetFileName(zipFileName)}. Total file processed: {cnt:N0}");

                    var symbol = data.Item1;
                    var minuteQuotes = data.Item2;
                    var groups = minuteQuotes.GroupBy(a => a.Timed.Date);
                    foreach (var group in groups)
                    {
                        var entry = new LogEntry { File = Path.GetFileNameWithoutExtension(zipFileName), Symbol = symbol };
                        log.Add(entry);
                        foreach (var q in group.OrderBy(a => a.Timed))
                        {
                            entry.Count++;
                            entry.Volume += q.Volume;
                            entry.Close = q.Close;
                            if (entry.Count == 1)
                            {
                                entry.Date = q.Timed.Date;
                                entry.MinTime = q.Timed.TimeOfDay;
                                entry.Open = q.Open;
                                entry.High = q.High;
                                entry.Low = q.Low;
                            }
                            else
                            {
                                entry.MaxTime = q.Timed.TimeOfDay;
                                if (entry.High < q.High) entry.High = q.High;
                                if (entry.Low > q.Low) entry.Low = q.Low;
                            }
                        }
                    }
                }

                showStatusAction($"YahooMinuteSaveLogToDb. Save data to database ...");
                // Save items to database table
                Helpers.DbHelper.SaveToDbTable(log, " dbQ2024..FileLogMinuteYahoo", "File", "Symbol", "Date", "MinTime", "MaxTime", "Count", "Open", "High", "Low", "Close", "Volume");

                showStatusAction($"YahooMinuteSaveLogToDb. Finished!");
            }
        }
    }
}

