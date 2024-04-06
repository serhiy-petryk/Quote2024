using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Tests
{
    public class WebSocketFiles
    {
        private const string folderOld = @"E:\Quote\WebData\RealTime\WebSockets";
        private const string folder = @"E:\Quote\WebData\RealTime\YahooSocket\Data\2024-04-05";
        public static void DecodePolygonRun()
        {
            // Result for Yahoo: min - 7 minutes 8 seconds
            var evs = new Dictionary<string, int>();
            var files = Directory.GetFiles(folder, "WebSocket_*.txt");
            foreach (var file in files)
            {
                // var items = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<oPolygon[]>(File.ReadAllBytes(file));
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var items = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<oPolygon[]>(line.Substring(13));
                    foreach (var item in items)
                    {
                        string key;
                        if (item.ev == "status")
                            key = item.status;
                        else
                            key = item.ev;
                        if (!evs.ContainsKey(key))
                            evs.Add(key, 0);
                        evs[key]++;
                        if (item.otc)
                        {

                        }
                    }
                }
            }

            foreach (var ev in evs)
                Debug.Print($"{ev.Key}\t{ev.Value}");
        }

        public static void YahooCheckSequence()
        {
            // Result is ~ 0.05% bad rows. Average time difference is ~ 2.0-3.5 secs
            var files = Directory.GetFiles(folder, "WebSocketYahoo_*.txt");
            var cnt = 0;
            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var fileDateTime = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var fileDate = fileDateTime.Date;
                var fileTime = fileDateTime.TimeOfDay;

                var a1 = new DateTimeOffset(fileDateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds();
                var estFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(a1);
                var estTimeOffset = fileDateTime - estFileDateTime;

                var sequences = new Dictionary<string, long>();
                var allCount = 0;
                var badCount = 0;
                var totalDifference = 0.0;
                foreach (var line in lines)
                {
                    cnt++;
                    var data = PricingData.GetPricingData(line.Substring(13));
                    if (data.marketHours != PricingData.MarketHoursType.REGULAR_MARKET) continue;

                    allCount++;
                    if (!sequences.ContainsKey(data.id))
                        sequences.Add(data.id, data.time);
                    else
                    {
                        if (data.time < sequences[data.id])
                        {
                            // var a2 = sequences[data.id] - data.time;
                            var date1 = TimeHelper.GetEstDateTimeFromUnixMilliseconds(data.time / 2);
                            var date2 = TimeHelper.GetEstDateTimeFromUnixMilliseconds(sequences[data.id] / 2);
                            badCount++;
                            totalDifference += (date2 - date1).TotalMilliseconds;
                        }
                        else
                            sequences[data.id] = data.time;
                    }
                }

                if (badCount == 0)
                    Debug.Print($"{Path.GetFileName(file)}\t{allCount}\t{badCount}");
                else
                    Debug.Print($"{Path.GetFileName(file)}\t{allCount}\t{badCount}\t{Convert.ToInt32(totalDifference / badCount)}");
            }
        }

        public static void YahooDelayRun()
        {
            // Result for Yahoo: 0.6% of item have > 10 seconds delay; average delay: 2 seconds (9 biggest (by TradeValue) symbols)
            //var files = Directory.GetFiles(folder, "YSocket_*.txt")
              //  .OrderBy(a => int.Parse(Path.GetFileNameWithoutExtension(a).Split('_')[1])).ToArray();
            var files = Directory.GetFiles(folder, "YSocket_*.txt").OrderBy(a => a).ToArray();
            var cnt = 0;
            foreach (var file in files)
            {
                var diffValues = new Dictionary<int, int>();
                var marketHoursTypes = new Dictionary<PricingData.MarketHoursType, int>();
                var optionTypes = new Dictionary<PricingData.OptionType, int>();
                // var allData = new List<PricingData>();
                var dataByMinutes = new Dictionary<string, Dictionary<DateTime, (float, float, long, int)>>();

                var lines = File.ReadAllLines(file);
                var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                var fileDateTime = DateTime.ParseExact(ss[ss.Length - 1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                var fileDate = fileDateTime.Date;
                var fileTime = fileDateTime.TimeOfDay;

                var a1 = new DateTimeOffset(fileDateTime.ToUniversalTime(), TimeSpan.Zero).ToUnixTimeMilliseconds();
                var estFileDateTime = TimeHelper.GetEstDateTimeFromUnixMilliseconds(a1);
                var estTimeOffset = fileDateTime - estFileDateTime;

                var delayTotal = 0.0;
                var itemCount = 0;
                var symbols = new Dictionary<string, int>();
                var badRecordDayCount = 0;
                var maxBadRecordMilliseconds = 0;
                foreach (var line in lines)
                {
                    cnt++;
                    var data = PricingData.GetPricingData(line.Substring(13));
                    if (!marketHoursTypes.ContainsKey(data.marketHours))
                        marketHoursTypes.Add(data.marketHours, 0);
                    marketHoursTypes[data.marketHours]++;

                    if (data.marketHours != PricingData.MarketHoursType.REGULAR_MARKET) continue;

                    var etcDataDate = TimeHelper.GetEstDateTimeFromUnixMilliseconds(data.time / 2);
                    var recordTime = TimeSpan.ParseExact(line.Substring(0, 12), @"hh\:mm\:ss\.FFF", CultureInfo.InvariantCulture);
                    var recordDate = ((recordTime < fileTime ? fileDate.AddDays(1) : fileDate) + recordTime);
                    var etcRecordDate = recordDate.Subtract(estTimeOffset);

                    if (data.marketHours == PricingData.MarketHoursType.REGULAR_MARKET &&
                        etcRecordDate.TimeOfDay >= Settings.MarketEndCommon)
                        continue;

                    itemCount++;
                    // allData.Add(data);

                    if (!dataByMinutes.ContainsKey(data.id))
                        dataByMinutes.Add(data.id, new Dictionary<DateTime, (float, float, long, int)>());
                    var minuteData = dataByMinutes[data.id];
                    var dataUnixMinuteMilliseconds = (data.time/120000) * 60000; // rounded to 1 minute
                    var dataMinuteKey = TimeHelper.GetEstDateTimeFromUnixMilliseconds(dataUnixMinuteMilliseconds);
                    if (!minuteData.ContainsKey(dataMinuteKey))
                    {
                        minuteData.Add(dataMinuteKey, (data.price, data.price, data.lastSize,1));
                    }
                    else
                    {
                        var minPrice = Math.Min(minuteData[dataMinuteKey].Item1, data.price);
                        var maxPrice = Math.Max(minuteData[dataMinuteKey].Item2, data.price);
                        var volume = minuteData[dataMinuteKey].Item3 + data.lastSize;
                        var count = minuteData[dataMinuteKey].Item4 + 1;
                        minuteData[dataMinuteKey] = (minPrice, maxPrice, volume, count);
                    }

                    if (!optionTypes.ContainsKey(data.optionsType))
                        optionTypes.Add(data.optionsType, 0);
                    optionTypes[data.optionsType]++;

                    if (etcRecordDate < etcDataDate.AddMilliseconds(-1000))
                    {
                        Debug.Print($"Big bad record date\t{Path.GetFileName(file)}. Ticker: {data.id}. Time: {etcRecordDate.TimeOfDay}. Difference: \t{(etcDataDate - etcRecordDate).TotalMilliseconds}");
                    }
                    if (etcRecordDate < etcDataDate)
                    {
                        badRecordDayCount++;
                        var badRecordDayDiff = Convert.ToInt32((etcDataDate - etcRecordDate).TotalMilliseconds);
                        if (badRecordDayDiff > maxBadRecordMilliseconds) maxBadRecordMilliseconds = badRecordDayDiff;
                        // Debug.Print($"Bad record date\t{Path.GetFileName(file)}. Ticker: {data.id}. Time: {etcRecordDate.TimeOfDay}. Difference: \t{(etcDataDate-etcRecordDate).TotalMilliseconds}");
                    }
                    var difference = etcRecordDate - etcDataDate;
                    delayTotal += difference.TotalSeconds;
                    var diffValue = Convert.ToInt32(Math.Round(difference.TotalSeconds, 0));
                    if (!diffValues.ContainsKey(diffValue))
                        diffValues.Add(diffValue, 0);
                    diffValues[diffValue]++;

                    if (!symbols.ContainsKey(data.id))
                        symbols.Add(data.id, 0);
                }

                var minDelay = diffValues.Keys.Min();
                var maxDelay = diffValues.Keys.Max();
                Debug.Print($"{Path.GetFileName(file)}\t{itemCount}\t{Math.Round(delayTotal / itemCount, 1)}\t{symbols.Count}\t{badRecordDayCount}\t{maxBadRecordMilliseconds}\t{minDelay}\t{maxDelay}");
                // foreach (var key in diffValues.Keys.OrderBy(a => a))
                //  Debug.Print($"Delay:\t{key}\t{diffValues[key]}");
            }
        }

        public static void YahooTimeRangesRun()
        {
            // Result for Yahoo: min - 7 minutes 8 seconds
            var times = new List<(string, TimeSpan, TimeSpan, TimeSpan)>();
            var files = Directory.GetFiles(folder, "*.txt");
            foreach (var file in files)
            {
                var from = TimeSpan.Zero;
                var to = TimeSpan.Zero;

                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    var timeSpan = TimeSpan.ParseExact(line.Substring(0, 12), @"hh\:mm\:ss\.FFF", CultureInfo.InvariantCulture);
                    if (from == TimeSpan.Zero)
                    {
                        from = timeSpan;
                        to = timeSpan;
                    }
                    else
                    {
                        if (timeSpan < to)
                            timeSpan = timeSpan.Add(new TimeSpan(1, 0, 0, 0, 0));

                        if ((timeSpan - to) < new TimeSpan(0, 1, 0))
                        {
                            to = timeSpan;
                        }
                        else
                        {
                            times.Add((Path.GetFileName(file), from, to, to - from));
                            from = timeSpan;
                            to = timeSpan;
                        }
                    }
                }

                if (from != TimeSpan.Zero)
                    times.Add((Path.GetFileName(file), from, to, to - from));
            }

            foreach (var item in times)
                Debug.Print($"{item.Item1}\t{item.Item2}\t{item.Item3}\t{item.Item4}");
        }

        public class oPolygon
        {
            public string ev;

            public string status;
            public string message;

            public string sym;
            public int v;
            public int av;
            public float op;
            public float vw;
            public float o;
            public float c;
            public float h;
            public float l;
            public float a;
            public double z;
            public long s;
            public long e;
            public bool otc;
        }
    }
}
