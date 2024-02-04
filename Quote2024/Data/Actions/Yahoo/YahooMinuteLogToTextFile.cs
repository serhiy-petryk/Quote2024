using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Yahoo
{
    public static class YahooMinuteLogToTextFile
    {
        public static void YahooMinuteLogSaveToTextFile(string[] zipFileNames, Action<string> showStatusAction)
        {
            if (zipFileNames == null)
                zipFileNames = Directory.GetFiles(YahooCommon.MinuteYahooLogFolder, "*.zip");
            Array.Sort(zipFileNames);

            var cnt = 0;
            foreach (var zipFileName in zipFileNames)
            {
                showStatusAction($"YahooMinuteLogSaveToTextFile is working with {Path.GetFileName(zipFileName)}");

                var errorLog = new Dictionary<string, string>();
                var log = new Dictionary<string, Dictionary<DateTime, object[]>>();

                var dates = new Dictionary<DateTime, object[]>();
                log.Add(Path.GetFileName(zipFileName), dates);

                foreach (var data in YahooCommon.GetMinuteDataFromZipFile(zipFileName, errorLog))
                {
                    cnt++;
                    if ((cnt % 100) == 0)
                        showStatusAction($"YahooMinuteLogSaveToTextFile is working for {Path.GetFileName(zipFileName)}. Total file processed: {cnt:N0}");

                    var quotes = data.Item2;
                    var groups = quotes.GroupBy(a => a.Timed.Date);
                    foreach (var group in groups)
                    {
                        var groupByTime = @group
                            .GroupBy(a => a.Timed.RoundDown(new TimeSpan(0, 30, 0)).TimeOfDay).ToArray();
                        var groupByTimeCount = new int[13];
                        foreach (var gKey in groupByTime)
                        {
                            if (gKey.Key == new TimeSpan(9, 30, 0))
                                groupByTimeCount[0] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(10, 00, 0))
                                groupByTimeCount[1] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(10, 30, 0))
                                groupByTimeCount[2] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(11, 00, 0))
                                groupByTimeCount[3] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(11, 30, 0))
                                groupByTimeCount[4] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(12, 00, 0))
                                groupByTimeCount[5] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(12, 30, 0))
                                groupByTimeCount[6] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(13, 00, 0))
                                groupByTimeCount[7] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(13, 30, 0))
                                groupByTimeCount[8] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(14, 00, 0))
                                groupByTimeCount[9] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(14, 30, 0))
                                groupByTimeCount[10] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(15, 00, 0))
                                groupByTimeCount[11] = gKey.Count();
                            else if (gKey.Key == new TimeSpan(15, 30, 0))
                                groupByTimeCount[12] = gKey.Count();
                            else
                                throw new Exception($"MinuteYahoo_SaveLog. Check groupByTimeCount array");
                        }

                        if (!dates.ContainsKey(@group.Key))
                            dates.Add(@group.Key,
                                new object[]
                                {
                                    @group.Min(a => a.Timed.TimeOfDay), @group.Max(a => a.Timed.TimeOfDay), 1,
                                    @group.Count(), groupByTimeCount
                                });
                        else
                        {
                            var oo = dates[@group.Key];
                            var minTime = @group.Min(a => a.Timed.TimeOfDay);
                            var maxTime = @group.Max(a => a.Timed.TimeOfDay);
                            if (minTime < (TimeSpan) oo[0]) oo[0] = minTime;
                            if (maxTime > (TimeSpan) oo[1]) oo[1] = maxTime;
                            oo[2] = (int) oo[2] + 1;
                            oo[3] = (int) oo[3] + @group.Count();
                            var gg = (int[]) oo[4];
                            for (var k = 0; k < groupByTimeCount.Length; k++)
                                gg[k] += groupByTimeCount[k];
                        }
                    }

                }

                var ss = Path.GetFileNameWithoutExtension(zipFileName).Split('_');
                var folderTimeStamp = ss[ss.Length - 1];
                var logFileName = Path.Combine(YahooCommon.MinuteYahooLogFolder, $"log_{folderTimeStamp}.txt");
                if (File.Exists(logFileName))
                    File.Delete(logFileName);

                if (errorLog.Count > 0)
                {
                    File.AppendAllLines(logFileName, new[] { "Errors:" });
                    File.AppendAllLines(logFileName, errorLog.Select(a => a.Key + "\t" + a.Value));
                }

                File.AppendAllLines(logFileName, new[] { "File\tDate\tMinTime\tMaxTime\tSymbolCount\tQuoteCount\t09:30\t10:00\t10:30\t11:00\t11:30\t12:00\t12:30\t13:00\t13:30\t14:00\t14:30\t15:00\t15:30" });

                foreach (var kvp in log)
                {
                    File.AppendAllLines(logFileName,
                        kvp.Value.Select(a =>
                            kvp.Key + "\t" + Helpers.CsUtils.GetString(a.Key) + "\t" + a.Value[0] + "\t" + a.Value[1] + "\t" +
                            a.Value[2] + "\t" + a.Value[3] + "\t" +
                            string.Join("\t", ((int[])a.Value[4]).Select(a1 => a1.ToString()))));
                }
            }

            showStatusAction($"MinuteYahoo_SaveLog FINISHED! See log(s) in folder: {YahooCommon.MinuteYahooLogFolder}");
        }

    }
}
