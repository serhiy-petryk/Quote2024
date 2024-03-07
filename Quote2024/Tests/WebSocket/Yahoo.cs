using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Tests.WebSocket
{
    public class Yahoo
    {
        #region =======  Json subclasses  ==========
        public class oRoot
        {
            public oLog log;
        }
        public class oLog
        {
            public oEntry[] entries;
        }

        public class oEntry
        {
            public oRequest request;
            public oWebSocketMessage[] _webSocketMessages;
        }

        public class oRequest
        {
            public string method;
            public string url;
        }
        public class oWebSocketMessage
        {
            public string type;
            public double time;
            public int opcode;
            public string data;

        }
        #endregion

        public static void TestHar()
        {
            var fn = @"E:\Quote\Request\finance.yahoo.com.1.har";
            var jsonData = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<oRoot>(File.ReadAllBytes(fn));
            foreach (var o in jsonData.log.entries)
            {
                Debug.Print($"{o.request.method}, {o.request.url}");
            }
            return;
            var aa1 = jsonData.log.entries.Where(a => a._webSocketMessages != null).ToArray();
            foreach (var a in aa1)
            foreach (var m in a._webSocketMessages.Where(x=>x.type=="receive"))
            {
                    // var s = line.Substring(13);
                    var bb = Convert.FromBase64String(m.data);
                var decodedString = System.Text.Encoding.UTF8.GetString(bb);
                var o = Serializer.Deserialize<PricingData>((ReadOnlySpan<byte>)bb);
                if (o.quoteType == PricingData.QuoteType.EQUITY || o.quoteType == PricingData.QuoteType.ETF)
                {
                    if (o.Id == "AAPL")
                    {
                        var t = DateTimeOffset.FromUnixTimeMilliseconds(o.Time / 2);
                        Debug.Print($"AAPL: {o.Price}, {t:HH:mm:ss};x");
                    }
                }
                else
                {

                }

            }
        }

        [ProtoContract]
        public class PricingData
        {
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
                        if (o.Id == "AAPL")
                        {
                            var t = DateTimeOffset.FromUnixTimeMilliseconds(o.Time/2);
                            Debug.Print($"AAPL: {o.Price}, {t:HH:mm:ss}");
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
            public string Id { get; set; }
            [ProtoMember(2)]
            public float Price { get; set; }
            [ProtoMember(3)]
            public long Time { get; set; }
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
}
