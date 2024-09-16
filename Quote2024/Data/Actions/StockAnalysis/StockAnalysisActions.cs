using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.StockAnalysis
{
    public class StockAnalysisActions
    {
        private const string Url = @"https://stockanalysis.com/actions/";
        private const string Folder = Settings.DataFolder + @"Splits\StockAnalysis\Actions\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = Folder + $"StockAnalysisActions_{timeStamp.Item2}.zip";

            // Download html data
            var o = WebClientExt.GetToBytes(Url, false);
            if (o.Item3 != null)
                throw new Exception($"StockAnalysisActions: Error while download from {Url}. Error message: {o.Item3.Message}");

            // Save html data to zip
            var entry = new VirtualFileEntry($"StockAnalysisActions_{timeStamp.Item2}.html", o.Item1);
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { entry });

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName, false);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static void ParseAllFiles()
        {
            var files = Directory.GetFiles(Folder, "*.zip");
            var itemCount = 0;
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File {Path.GetFileName(zipFileName)}. ItemCount: {itemCount}");
                var count = ParseAndSaveToDb(zipFileName, true);
                if (count < 100)
                    throw new Exception("Trap!!!");
                itemCount += count;
            }
        }

        public static int ParseAndSaveToDb(string zipFileName, bool onlyCheck)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var items = new List<Models.ActionStockAnalysis>();
                    Parse(entry.GetContentOfZipEntry(), items, entry.LastWriteTime.DateTime);
                    itemCount += items.Count;

                    if (onlyCheck) continue;

                    // Save data to database
                    if (items.Count > 0)
                    {
                        DbHelper.ClearAndSaveToDbTable(items.Where(a => !a.IsBad), "dbQ2024..Bfr_ActionsStockAnalysis",
                            "Date", "Type", "Symbol", "OtherSymbolOrName", "Name", "Description", "SplitRatio", "SplitK",
                            "TimeStamp");

                        DbHelper.ClearAndSaveToDbTable(items.Where(a => a.IsBad),
                            "dbQ2024..Bfr_ActionsStockAnalysisError", "Date", "Type", "Symbol", "OtherSymbolOrName",
                            "Description", "TimeStamp");

                        DbHelper.RunProcedure("dbQ2024..pUpdateActionsStockAnalysis");
                    }
                }

            return itemCount;
        }

        #region ==========  Private section  ==========
        private static void Parse(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            if (!TryToParseAsJson(content, items, fileTimeStamp))
                ParseAsHtml(content, items, fileTimeStamp);
        }

        private static Dictionary<string, string> _decoder = new Dictionary<string, string>
        {
            {"{action:\"", "{\"action\":\""}, {"{title:\"", "{\"title\":\""}, {"{date:\"", "{\"date\":\""},
            {",type:\"", ",\"type\":\""},{",url:\"", ",\"url\":\""},{",id:\"", ",\"id\":\""},{",format:\"", ",\"format\":\""},
            {",classes:\"", ",\"classes\":\""},{",symbol:\"", ",\"symbol\":\""},{",name:\"", ",\"name\":\""},{",other:\"", ",\"other\":\""},{",text:\"", ",\"text\":\""},
            {",heading:\"",",\"heading\":\""},{",description:\"",",\"description\":\""},{",fileName:\"",",\"fileName\":\""},{",fullCount:",",\"fullCount\":"},
            {",props:{", ",\"props\":{"},{",columns:[", ",\"columns\":["}, {",data:[", ",\"data\":["}

        };

        public static string GetJsonContent()
        {
            var fileContent = File.ReadAllText(Settings.DataFolder + @"Splits\StockAnalysis\StockAnalysisActions_20240127.html");
            var i1 = fileContent.IndexOf("const data =", StringComparison.InvariantCulture);
            if (i1 == -1) return null;

            var i2 = fileContent.IndexOf("}}]", i1 + 12, StringComparison.InvariantCulture);
            var s = fileContent.Substring(i1 + 12, i2 - i1 - 12 + 3).Trim();
            var i12 = s.IndexOf("{\"type\":", StringComparison.InvariantCulture);
            i12 = s.IndexOf("{\"type\":", i12 + 8, StringComparison.InvariantCulture);
            var s2 = s.Substring(i12, s.Length - i12 - 1);
            return s2;
        }

        private static bool TryToParseAsJson(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
        {
            var i1 = content.IndexOf("const data =", StringComparison.InvariantCulture);
            if (i1 == -1) return false;
            var i2 = content.IndexOf("}}]", i1 + 12, StringComparison.InvariantCulture);
            var s = content.Substring(i1 + 12, i2 - i1 - 12 + 3).Trim();
            var i12 = s.IndexOf("{\"type\":", StringComparison.InvariantCulture);
            i12 = s.IndexOf("{\"type\":", i12 + 8, StringComparison.InvariantCulture);
            var s2 = s.Substring(i12, s.Length - i12 - 1);

            var s3 = s2;
            foreach (var kvp in _decoder)
                s3 = s3.Replace(kvp.Key, kvp.Value);

            var oo = ZipUtils.DeserializeString<cRoot>(s3);
            if (oo.data.fullCount != oo.data.data.Length)
                throw new Exception("Check data Deserializator for StockAnalysisActions");

            foreach (var item in oo.data.data)
                items.Add(new ActionStockAnalysis(item, fileTimeStamp));

            return true;
        }

        private static void ParseAsHtml(string content, List<Models.ActionStockAnalysis> items, DateTime fileTimeStamp)
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
                var item = new Data.Models.ActionStockAnalysis(row.Trim(), fileTimeStamp);
                items.Add(item);
            }
        }
        #endregion

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
            public string pOther => other == "N/A" || string.IsNullOrWhiteSpace(other) ? null : other;
            public string pName => string.IsNullOrWhiteSpace(name) ? null : name;

            public override string ToString() => $"{pDate:yyyy-MM-dd}, {pSymbol}, {type}, {pOther}, {text}";
        }
        #endregion
    }
}
