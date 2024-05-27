using System;
using System.Diagnostics;
using System.Globalization;

namespace Data.Tests
{
    public static class ScannerIntraday
    {
        private const float MinTradeValue = 5.0f;
        private const int MinTradeCount = 500;

        public static void Test()
        {
            var itemCount = 0;
            var sw = new Stopwatch();
            sw.Start();
            var fromDate = new DateTime(2023, 1, 1);
            var toDate = new DateTime(2023, 12, 31);

            foreach (var oo in Data.Actions.Polygon.PolygonMinuteScan.GetMinuteQuotesByDate(fromDate, toDate, MinTradeValue, MinTradeCount))
            {
                var symbol = oo.Item1;
                var date = oo.Item2;
                var data = oo.Item3;
                itemCount += data.Length;
            }

            sw.Stop();
            Debug.Print($"Test3. Items: {itemCount:N0}. Duration: {sw.ElapsedMilliseconds:N0}");
        }

    }
}
