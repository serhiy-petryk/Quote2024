using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ProtoBuf;

namespace Data.Models
{
    [ProtoContract]
    public class PricingData
    {
        public static PricingData GetPricingData(string codedString)
        {
            var bb = Convert.FromBase64String(codedString);
            return Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)bb);
        }

        public static void RunTest()
        {
            // CgRBQVBMFXH9KEMYsN7m18JjKgNOTVMwCDgBRQoLKr9IlIq+M2WAo5C/2AEE
            var s = @"CgRBQVBMFXH9KEMYsN7m18JjKgNOTVMwCDgBRQoLKr9IlIq+M2WAo5C/2AEE";
            var bb = Convert.FromBase64String(s);
            string decodedString = System.Text.Encoding.UTF8.GetString(bb);
            var y = Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)bb);
        }

        public static void RunFileTest()
        {
            var lines = File.ReadAllLines(@"E:\Temp\WebSocket_20240306213522.txt");
            foreach (var line in lines)
            {
                var dt = DateTime.ParseExact(line.Substring(0, 12), "HH:mm:ss.fff", CultureInfo.InvariantCulture);
                var s = line.Substring(13);
                var bb = Convert.FromBase64String(s);
                var decodedString = System.Text.Encoding.UTF8.GetString(bb);
                var o = Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)bb);
                if (o.quoteType == QuoteType.EQUITY || o.quoteType == QuoteType.ETF)
                {
                    if (o.id == "AAPL")
                    {
                        var t = DateTimeOffset.FromUnixTimeMilliseconds(o.time / 2);
                        Debug.Print($"AAPL: {o.price}, {t:HH:mm:ss}");
                    }
                }
                else
                {

                }
            }
        }

        public enum QuoteType
        {
            NONE = 0,
            ALTSYMBOL = 5,
            HEARTBEAT = 7,
            EQUITY = 8,
            INDEX = 9,
            MUTUALFUND = 11,
            MONEYMARKET = 12,
            OPTION = 13,
            CURRENCY = 14,
            WARRANT = 15,
            BOND = 17,
            FUTURE = 18,
            ETF = 20,
            COMMODITY = 23,
            ECNQUOTE = 28,
            CRYPTOCURRENCY = 41,
            INDICATOR = 42,
            INDUSTRY = 1000
        };

        public enum OptionType
        {
            CALL = 0,
            PUT = 1
        };

        public enum MarketHoursType
        {
            PRE_MARKET = 0,
            REGULAR_MARKET = 1,
            POST_MARKET = 2,
            EXTENDED_HOURS_MARKET = 3,
        };


        [ProtoMember(1)]
        public string id { get; set; }
        [ProtoMember(2)]
        public float price { get; set; }
        [ProtoMember(3)]
        public long time { get; set; }
        [ProtoMember(4)]
        public string currency { get; set; }
        [ProtoMember(5)]
        public string exchange { get; set; }
        [ProtoMember(6)]
        public QuoteType quoteType { get; set; }
        [ProtoMember(7)]
        public MarketHoursType marketHours { get; set; }
        [ProtoMember(8)]
        public float changePercent { get; set; }
        [ProtoMember(9)]
        public long dayVolume { get; set; }
        [ProtoMember(10)]
        public float dayHigh { get; set; }
        [ProtoMember(11)]
        public float dayLow { get; set; }
        [ProtoMember(12)]
        public float change { get; set; }
        [ProtoMember(13)]
        public string shortName { get; set; }
        [ProtoMember(14)]
        public long expireDate { get; set; }
        [ProtoMember(15)]
        public float openPrice { get; set; }
        [ProtoMember(16)]
        public float previousClose { get; set; }
        [ProtoMember(17)]
        public float strikePrice { get; set; }
        [ProtoMember(18)]
        public string underlyingSymbol { get; set; }
        [ProtoMember(19)]
        public long openInterest { get; set; }
        [ProtoMember(20)]
        public OptionType optionsType { get; set; }
        [ProtoMember(21)]
        public long miniOption { get; set; }
        [ProtoMember(22)]
        public long lastSize { get; set; }
        [ProtoMember(23)]
        public float bid { get; set; }
        [ProtoMember(24)]
        public long bidSize { get; set; }
        [ProtoMember(25)]
        public float ask { get; set; }
        [ProtoMember(26)]
        public long askSize { get; set; }
        [ProtoMember(27)]
        public long priceHint { get; set; }
        [ProtoMember(28)]
        public long vol_24hr { get; set; }
        [ProtoMember(29)]
        public long volAllCurrencies { get; set; }
        [ProtoMember(30)]
        public string fromcurrency { get; set; }
        [ProtoMember(31)]
        public string lastMarket { get; set; }
        [ProtoMember(32)]
        public double circulatingSupply { get; set; }
        [ProtoMember(33)]
        public double marketcap { get; set; }
    }
}
