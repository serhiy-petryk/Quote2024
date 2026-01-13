using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Data.Actions.Polygon
{
    public static class PolygonSplits
    {

        public static void ParseAllFiles()
        {
            var folder = @"E:\Quote\WebData\Splits\Polygon\AllSplitsPolygon_20251028";
            // var files = Directory.GetFiles(folder, "*.json").OrderBy(a => a).ToArray();
            var files = Directory.GetFiles(folder, "*.json").OrderByDescending(a => a).ToArray();
            var dbItems = new Dictionary<(DateTime, string), DbItem>();
            foreach (var file in files)
            {
                Parse(file, dbItems);
            }

            foreach (var item in dbItems.OrderBy(a => a.Key).Select(a=>a.Value))
            {
                Debug.Print($"{item.Symbol}\t{item.Date:yyyy-MM-dd}\t{item.Ratio}\t{item.K}");
            }

        }

        private static void Parse(string filename, Dictionary<(DateTime,string), DbItem> dbItems)
        {

            var bytes = File.ReadAllBytes(filename);
            var jsonData = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<cRoot>(bytes);

            var maxDate = File.GetLastWriteTime(filename).Date;
            var lastDate = jsonData.results[jsonData.results.Length - 1].Date;
            var items = jsonData.results.Where(a => a.Date!=lastDate && a.Date<maxDate).ToArray();

            foreach (var item in items)
            {
                var key = (item.Date, item.ticker);
                if (!dbItems.ContainsKey(key))
                    dbItems.Add(key, new DbItem(item));

                dbItems[key].ProcessItem(item);
            }

            /*var dictItems = new Dictionary<(DateTime, string), decimal[]>();
            var cnt = 0;
            foreach (var item in items)
            {
                var key = (item.Date, item.ticker);
                if (dictItems.ContainsKey(key))
                {
                    var dictItem = dictItems[key];
                    Debug.Print($"{cnt}\t{key.Date:yyyy-MM-dd}\t{key.ticker}\t{dictItem[0]}:{dictItem[1]}\t{item.split_from}:{item.split_to}");
                }
                else
                {
                    dictItems.Add(key, new[] { item.split_from, item.split_to });
                }

                cnt++;
            }*/
        }

        #region ===========  Json SubClasses  ===========
        public class cRoot
        {
            public cItem[] results;
            public string status;
            public string request_id;
            public string next_url;
        }

        public class cItem
        {
            public string execution_date;
            public string id;
            public decimal split_from;
            public decimal split_to;
            public string ticker;

            public DateTime Date => DateTime.ParseExact(execution_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            // public decimal From => decimal.Parse(split_from, CultureInfo.InvariantCulture);
        }

        public class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Ratio => string.Join(' ', Ratios);
            public float K => Convert.ToSingle(K1);
            public decimal K1 = 1M;
            public List<string> Ratios = new List<string>();

            public DbItem(cItem item)
            {
                Symbol = item.ticker;
                Date = item.Date;
            }

            public void ProcessItem(cItem item)
            {
                Ratios.Add(item.split_to.ToString(CultureInfo.InvariantCulture) + ":" +
                           item.split_from.ToString(CultureInfo.InvariantCulture));
                K1 = K1 * item.split_to / item.split_from;
            }
        }

        #endregion


    }
}
