namespace Data.Models
{
    public class MinuteYahoo2
    {
        public cChart chart { get; set; }

        public class cChart
        {
            public cResult[] result { get; set; }
            public cError error { get; set; }
        }

        public class cError
        {
            public string code { get; set; }
            public string description { get; set; }
        }

        public class cResult
        {
            public cMeta meta { get; set; }
            public long[] timestamp { get; set; }
            public cIndicators indicators { get; set; }
        }

        public class cMeta
        {
            public string currency { get; set; }
            public string symbol { get; set; }
            public string exchangeName { get; set; }
            public string instrumentType { get; set; }
            public cTradingPeriod tradingPeriods { get; set; }
        }

        public class cIndicators
        {
            public cQuote[] quote { get; set; }
        }

        public class cQuote
        {
            public double?[] open { get; set; }
            public double?[] high { get; set; }
            public double?[] low { get; set; }
            public double?[] close { get; set; }
            public long?[] volume { get; set; }
        }

        public class cTradingPeriod
        {
            public cTradingPeriodItem[,] pre { get; set; }
            public cTradingPeriodItem[,] post { get; set; }
            public cTradingPeriodItem[,] regular { get; set; }
            public string dataGranularity { get; set; }
        }

        public class cTradingPeriodItem
        {
            public string timezone { get; set; }
            public long start { get; set; }
            public long end { get; set; }
            public long gmtoffset { get; set; }
        }
    }
}
