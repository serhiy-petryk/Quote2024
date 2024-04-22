using System;

namespace Data.Strategies.HighLow
{
    public static class Tests
    {
        private static DateTime _fromDate = new DateTime(2024, 04, 2);
        private static DateTime _toDate = new DateTime(2024, 04, 4);

        public static void Start()
        {
            var date = new DateTime(2024, 04, 5);
            var symbols = Actions.Polygon.PolygonCommon.GetSymbolsForStrategies(date);
        }
    }
}
