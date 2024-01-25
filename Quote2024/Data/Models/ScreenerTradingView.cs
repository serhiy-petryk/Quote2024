using System;
using System.Globalization;
using SpanJson.Formatters.Dynamic;

namespace Data.Models
{
    public class ScreenerTradingView
    {
        public int totalCount;
        public Item[] data;

        public class Item
        {
            public string s;
            public object[] d;

            public DbItem GetDbItem(DateTime timeStamp)
            {
                var symbol = GetString(d[1]);
                var ss = s.Split(':');
                if (!string.Equals(ss[1], symbol))
                    throw new Exception("Check 'Symbol' in ScreenerTradingView.Item.");

                var recommend = GetSingle(d[5]);
                var marketCap = GetSingle(d[8]);
                var item = new DbItem
                {
                    Exchange = ss[0],
                    Symbol = ss[1],
                    Name = GetString(d[14]),
                    Type = GetString(d[15]),
                    Subtype = GetString(d[16]),
                    Sector = GetString(d[12]),
                    Industry = GetString(d[13]),
                    Close = GetSingle(d[2]).Value,
                    Volume = Convert.ToSingle(Math.Round(GetSingle(d[6]).Value / 1000000.0, 3)),
                    MarketCap = marketCap.HasValue ? Convert.ToSingle(Math.Round(marketCap.Value / 1000000.0, 3)) : (float?)null ,
                    Recommend = recommend.HasValue ? recommend.Value : (float?) null,
                    TimeStamp = timeStamp
                };
                return item;

                string GetString(object input)
                {
                    if (input == null) return null;

                    var span = (SpanJsonDynamic<Byte>)input;
                    var s = System.Text.Encoding.UTF8.GetString(span.Symbols);
                    if (!s.StartsWith('"') || !s.EndsWith('"'))
                        throw new Exception($"Invalid string in SpanJsonDynamic<Byte>: {input}");
                    s = s.Substring(1, s.Length - 2);
                    return string.IsNullOrEmpty(s) ? null : s;
                }
                float? GetSingle(object input)
                {
                    if (input == null) return (float?) null;

                    var span = (SpanJsonDynamic<Byte>)input;
                    var s = System.Text.Encoding.UTF8.GetString(span.Symbols);
                    return string.IsNullOrEmpty(s) ? (float?)null: float.Parse(s, CultureInfo.InvariantCulture);
                }
            }
        }

        public class DbItem
        {
            public string Exchange;
            public string Symbol;
            public string Name;
            public string Type;
            public string Subtype;
            public string Sector;
            public string Industry;
            public float Close;
            public float Volume;
            public float? MarketCap;
            public float? Recommend;
            public DateTime TimeStamp;
        }
    }
}
