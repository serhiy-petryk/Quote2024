using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;

//using Newtonsoft.Json;
//using JsonSerializer = Utf8Json.JsonSerializer;

namespace Tests.Json
{
    public class Utf8JsonTests
    {
        private static Dictionary<string, string> decoder = new Dictionary<string, string>
        {
            {"{action:\"", "{\"action\":\""}, {"{title:\"", "{\"title\":\""}, {"{date:\"", "{\"date\":\""},
            {",type:\"", ",\"type\":\""},{",url:\"", ",\"url\":\""},{",id:\"", ",\"id\":\""},{",format:\"", ",\"format\":\""},
            {",classes:\"", ",\"classes\":\""},{",symbol:\"", ",\"symbol\":\""},{",name:\"", ",\"name\":\""},{",other:\"", ",\"other\":\""},{",text:\"", ",\"text\":\""},
            {",heading:\"",",\"heading\":\""},{",description:\"",",\"description\":\""},{",fileName:\"",",\"fileName\":\""},{",fullCount:",",\"fullCount\":"},
            {",props:{", ",\"props\":{"},{",columns:[", ",\"columns\":["}, {",data:[", ",\"data\":["}

        };
        public static void Start()
        {
            var fn = @"E:\Quote\WebData\Splits\StockAnalysis\StockAnalysisActions_20240127.html";
            var content = File.ReadAllText(fn);
                    var items = new List<ActionStockAnalysis>();
                    Parse(content, items, DateTime.Now);
//            var p2 = JsonSerializer.Deserialize<Person>(result);
        }

        private static void Parse(string content, List<ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            if (!TryToParseAsJson(content, items, fileTimeStamp))
                ParseAsHtml(content, items, fileTimeStamp);
        }

        private static bool TryToParseAsJson(string content, List<ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            var i1 = content.IndexOf("const data =", StringComparison.InvariantCulture);
            if (i1 == -1) return false;
            var i2 = content.IndexOf("}}]", i1 + 12, StringComparison.InvariantCulture);
            var s = content.Substring(i1 + 12, i2 - i1 - 12 + 3).Trim();
            var i12 = s.IndexOf("{\"type\":", StringComparison.InvariantCulture);
            i12 = s.IndexOf("{\"type\":", i12 + 8, StringComparison.InvariantCulture);
            var s2 = s.Substring(i12, s.Length - i12 - 1);

            var s3 = s2;
            foreach (var kvp in decoder)
            {
                s3 = s3.Replace(kvp.Key, kvp.Value);
            }
            var oo3 = System.Text.Json.JsonSerializer.Deserialize<cRoot>(s3);
            var oo4 = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<cRoot>(s3);

            // var oo = JsonSerializer.Deserialize<cRoot>(s2);
            var oo = Newtonsoft.Json.JsonConvert.DeserializeObject<cRoot>(s2);
            // var oo = System.Text.Json.JsonSerializer.Deserialize<cRoot>(s2, options);
            // var oo = NetJSON.NetJSON.Deserialize<cRoot>(s2); // NetJSON.Des System.Text.Json.JsonSerializer.Deserialize<cRoot>(s2, options);
            foreach (var item in oo.data.data)
              items.Add(new ActionStockAnalysis(item, fileTimeStamp));

            return true;
        }

        private static void ParseAsHtml(string content, List<ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            var i1 = content.IndexOf(">Action</th>", StringComparison.InvariantCultureIgnoreCase);
            if (i1 == -1)
                throw new Exception("Check StockAnalysis.WebArchiveActions parser");
            var i2 = content.IndexOf("</tbody>", i1 + 12, StringComparison.InvariantCultureIgnoreCase);

            var rows = content.Substring(i1 + 12, i2 - i1 - 12).Replace("</thead>", "").Replace("<tbody>", "")
                .Replace("</tr>", "").Replace("<!-- HTML_TAG_START -->", "").Replace("<!-- HTML_TAG_END -->", "")
                .Split(new[] { "<tr>", "<tr " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row)) continue;
                var item = new ActionStockAnalysis(row.Trim(), fileTimeStamp);
                items.Add(item);
            }
        }

        #region =======  Json subclasses  =============
        private class cRoot
        {
            public string type;
            public cData data;
        }
        private class cData
        {
            public string action;
            public string type;
            public object props;
            public cItem[] data;
            public int fullCount;
        }
        public class cItem
        {
            public string date;
            public string type;
            public string symbol;
            public string name;
            public string other;
            public string text;

            public DateTime pDate => DateTime.Parse(date, CultureInfo.InvariantCulture);
            public string pSymbol => symbol.StartsWith("$") ? symbol.Substring(1) : symbol;
            public string pOther => other == "N/A" || string.IsNullOrEmpty(other) ? null : other;
            public string pName => string.IsNullOrEmpty(name) ? null : name;

            public override string ToString()
            {
                return $"{pDate:yyyy-MM-dd}, {pSymbol}, {type}, {pOther}, {text}";
            }
        }
        #endregion

    }
}
