using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Investing
{
    public class InvestingSplitsLoader
    {
        private const string URL = @"https://www.investing.com/stock-split-calendar/Service/getCalendarFilteredData";
        private const string POST_DATA_TEMPLATE = @"country%5B%5D=5&dateFrom={0}&dateTo={1}&currentTab=custom&limit_from=0";

        public static void Start()
        {
            var timeStamp = TimeHelper.GetTimeStamp(-9 - 24);
            var postData = string.Format(POST_DATA_TEMPLATE,
                timeStamp.Item1.AddDays(-30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                timeStamp.Item1.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            var zipFileName = $@"E:\Quote\WebData\Splits\Investing\InvestingSplits_{timeStamp.Item2}.zip";

            // Download data
            Logger.AddMessage($"Download data from {URL}");
            var o = Download.PostToBytes(URL, postData, true, true);
            if (o is Exception ex)
                throw new Exception($"InvestingSplitsLoader: Error while download from {URL}. Error message: {ex.Message}");

            var entry = new VirtualFileEntry($"{Path.GetFileNameWithoutExtension(zipFileName)}.json", (byte[])o);
            ZipUtils.ZipVirtualFileEntries(zipFileName, new []{entry});

            // Parse and save to database
            Logger.AddMessage($"Parse and save to database");
            var itemCount = ParseZipAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseTextFileAndSaveToDb(string txtFileName)
        {
            var content = File.ReadAllText(txtFileName);
            var timeStamp = File.GetCreationTime(txtFileName);
            var items = new List<SplitModel>();

            ParseTxt(Path.GetFileName(txtFileName), content, timeStamp, items);

            if (items.Count > 0)
                SaveToDb(items.Where(a => a.Date <= a.TimeStamp));

            return items.Count;
        }

        public static int ParseZipAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry();
                    var timeStamp = entry.LastWriteTime.DateTime;
                    var items = new List<SplitModel>();

                    if (Path.GetExtension(entry.FullName) == ".json")
                        ParseJson(entry, timeStamp, items);
                    else if (Path.GetExtension(entry.FullName) == ".txt")
                        ParseTxt(entry.FullName, content, timeStamp, items);
                    else
                        throw new Exception($"InvestingSplitsLoader. Parse don't defined for '{Path.GetExtension(entry.FullName)}' file type");

                    if (items.Count > 0)
                        SaveToDb(items.Where(a => a.Date <= a.TimeStamp));

                    itemCount += items.Count;
                }

            return itemCount;
        }

        private static void ParseJson(ZipArchiveEntry entry, DateTime timeStamp, List<SplitModel> items)
        {
            var o = ZipUtils.DeserializeZipEntry<cRoot>(entry);
            var rows = o.data.Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

            string lastDate = null;
            for (var k = 0; k < rows.Length; k++)
            {
                var row = rows[k];
                var cells = row.Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                var sDateOriginal = GetCellValue(cells[0]);
                var sDate = string.IsNullOrEmpty(sDateOriginal) ? lastDate : sDateOriginal;
                var date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);

                cells[1] = System.Net.WebUtility.HtmlDecode(cells[1]).Trim();
                if (cells[1].EndsWith(")"))
                    cells[1] = cells[1].Substring(0, cells[1].Length - 1);
                else
                    throw new Exception("Check InvestingSplitsLoader parser");
                var symbol = GetCellValue(cells[1]);

                var i2 = cells[1].LastIndexOf("</span>", StringComparison.InvariantCulture);
                var i1 = cells[1].LastIndexOf(">", i2 - 7, StringComparison.InvariantCulture);
                var name = cells[1].Substring(i1 + 1, i2 - i1 - 1).Trim();

                var ratio = GetCellValue(cells[2]);

                var item = new SplitModel(symbol, date, name, ratio, null, timeStamp);
                items.Add(item);

                lastDate = sDate;
            }

            string GetCellValue(string cell)
            {
                var s = cell.Replace("</a>", "").Trim();
                var i1 = s.LastIndexOf('>');
                s = System.Net.WebUtility.HtmlDecode(s.Substring(i1 + 1)).Trim();
                return s;
            }
        }

        private static void ParseTxt(string txtFileName, string content, DateTime timeStamp, List<SplitModel> items)
        {
            // var lines = File.ReadAllLines(txtFileName, System.Text.Encoding.Default); // or Encoding.UTF7
            var lines = content.Split(new [] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0 || !Equals(lines[0], "Split date\tCompany\tSplit ratio"))
                throw new Exception($"Invalid Investing.com split file structure! {txtFileName}");

            var splits = new Dictionary<string, SplitModel>();
            DateTime? lastDate = null;
            for (var k = 1; k < lines.Length; k++)
            {
                var ss = lines[k].Split('\t');
                if (ss.Length != 3)
                    throw new Exception($"Invalid line of Investing.com split file! Line: {lines[k]}, file: {txtFileName}");
                if (string.IsNullOrEmpty(ss[0]) && !lastDate.HasValue)
                    throw new Exception($"Error: Empty first date in Investing.com split file! Line: {lines[k]}, file: {txtFileName}");
                var date = string.IsNullOrEmpty(ss[0]) ? lastDate.Value : DateTime.ParseExact(ss[0], "MMM dd, yyyy", CultureInfo.InvariantCulture);
                var s = ss[1].Trim();
                if (!s.EndsWith(")"))
                    throw new Exception($"Error: The line must be ended with ')'! Line: {lines[k]}, file: {txtFileName}");
                var i = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                var symbol = s.Substring(i + 1, s.Length - i - 2).Trim().ToUpper();
                var name = s.Substring(0, i).Trim();

                if (String.Equals(symbol, "US90274X5529=UBSS") && string.Equals(name, "Whiting Petroleum"))
                    symbol = "WLL";

                var item = new SplitModel(symbol, date, name, ss[2].Trim(), null, timeStamp);
                if (!splits.ContainsKey(item.Key))
                    splits.Add(item.Key, item);
                lastDate = date;
            }

            items.AddRange(splits.Values);
        }

        private static void SaveToDb(IEnumerable<SplitModel> items)
        {
            DbUtils.ClearAndSaveToDbTable(items.Where(a => a.Date <= a.TimeStamp), "dbQ2023Others..Bfr_SplitInvesting",
                "Symbol", "Date", "Name", "Ratio", "K", "TimeStamp");

            DbUtils.ExecuteSql("INSERT INTO dbQ2023Others..SplitInvesting (Symbol,[Date],Name,Ratio,K,[TimeStamp]) " +
                               "SELECT a.Symbol, a.[Date], a.Name, a.Ratio, a.K, a.[TimeStamp] FROM dbQ2023Others..Bfr_SplitInvesting a " +
                               "LEFT JOIN dbQ2023Others..SplitInvesting b ON a.Symbol = b.Symbol AND a.Date = b.Date " +
                               "WHERE b.Symbol IS NULL");
        }

        #region =======  Json subclasses  =============
        private class cRoot
        {
            public string data;
            public int rows_num;
            public string last_time_scope;
            public DateTime From => new DateTime(1970, 1, 1).AddSeconds(long.Parse(last_time_scope));
        }
        #endregion
    }
}
