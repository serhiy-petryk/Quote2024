using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Data.Scanners
{
    public class TradesPerMinute
    {
        public static void Start()
        {
            var result = new Dictionary<TimeSpan, int[]>();
            foreach (var oo in Data.Actions.Polygon.PolygonMinuteScan.GetQuotes(new DateTime(2023,1,1), new DateTime(2024,1,1)))
            foreach (var quote in oo.Item3.Where(a => a.o >= 5.0f && Settings.IsInMarketTime(a.DateTime)))
            {
                var time = quote.DateTime.TimeOfDay;
                if (!result.ContainsKey(time))
                {
                    result.Add(time, new int[2]);
                }

                result[time][0]++;
                result[time][1] += quote.n;
            }

            foreach (var kvp in result.OrderBy(a => a.Key))
                Debug.Print($"{kvp.Key}\t{kvp.Value[0]}\t{kvp.Value[1]/kvp.Value[0]}");
        }
    }
}