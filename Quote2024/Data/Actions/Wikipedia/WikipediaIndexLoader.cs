using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;
using Data.Models;

namespace Data.Actions.Wikipedia
{
    public static class WikipediaIndexLoader
    {
        private static readonly (string,string)[] UrlsAndFilenames = new []
        {
            ("https://en.wikipedia.org/wiki/List_of_S%26P_500_companies", "Components_SP500_{0}.html"),
            ("https://en.wikipedia.org/wiki/List_of_S%26P_400_companies", "Components_SP400_{0}.html"),
            ("https://en.wikipedia.org/wiki/List_of_S%26P_600_companies", "Components_SP600_{0}.html"),
            ("https://en.wikipedia.org/wiki/Nasdaq-100", "Components_Nasdaq100_{0}.html")
            // old: https://en.wikipedia.org/wiki/List_of_NASDAQ-100_companies
        };

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var zipFileName =
                $@"E:\Quote\WebData\Indices\Wikipedia\IndexComponents\IndexComponents_{timeStamp.Item2}.zip";
            var virtualFileEntries = new List<VirtualFileEntry>();

            // Download data
            foreach (var oo in UrlsAndFilenames)
            {
                var o = Download.DownloadToBytes(oo.Item1, false);
                if (o is Exception ex)
                    throw new Exception($"WikipediaIndexLoader: Error while download from {oo.Item1}. Error message: {ex.Message}");

                var entry = new VirtualFileEntry(string.Format(oo.Item2, timeStamp.Item2), (byte[]) o);
                virtualFileEntries.Add(entry);
            }

            // Zip data
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static void ParseAndSaveToDbAllFiles()
        {
            var allZipFiles = new List<string>
                { @"E:\Quote\WebData\Indices\Wikipedia\IndexComponents\WebArchive.Wikipedia.Indices.zip" };

            const string folder = @"E:\Quote\WebData\Indices\Wikipedia\IndexComponents";
            var files = Directory.GetFiles(folder, "*_202*.zip").OrderBy(a=>a);
            allZipFiles.AddRange(files);
            foreach (var zipFileName in allZipFiles)
            {
                ParseAndSaveToDb(zipFileName);
            }
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            var prevTimeStamp = DateTime.MinValue;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0).OrderBy(a=>a.Name))
                {
                    var content = entry.GetContentOfZipEntry();

                    var items = new Dictionary<string, Models.IndexDbItem>();
                    var changes = new List<Models.IndexDbChangeItem>();

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var indexName = ss[ss.Length - 2];

                    var timeStamp = entry.LastWriteTime.DateTime;
                    if (prevTimeStamp.Date == timeStamp.Date) continue;
                    prevTimeStamp = timeStamp;

                    var i1 = content.IndexOf("id=\"constituents\"", StringComparison.InvariantCultureIgnoreCase);
                    if (i1 > 0)
                    {
                        var i2 = content.IndexOf("</table", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                        var table = content.Substring(i1 + 17, i2 - i1 - 17);
                        ParseTable(indexName, timeStamp, table, items);
                        Debug.Print($"table: {items.Count}, {entry.Name}");
                    }
                    i1 = content.IndexOf(">stock symbol<", StringComparison.InvariantCultureIgnoreCase);
                    if (items.Count == 0 && i1 > 0)
                    {
                        var i2 = content.IndexOf("</ul>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                        var list = content.Substring(i1 + 17, i2 - i1 - 17);
                        ParseList(indexName, timeStamp, list, items);
                        Debug.Print($"list: {items.Count}, {entry.Name}");
                    }

                    i1 = content.IndexOf("==Components==", StringComparison.InvariantCultureIgnoreCase);
                    if (items.Count == 0 && i1 > 0)
                    {
                        var i2 = content.IndexOf("==External links==", i1, StringComparison.InvariantCultureIgnoreCase);
                        var links = content.Substring(i1 + 14, i2 - i1 - 14);
                        ParseLinks(indexName, timeStamp, links, items);
                        Debug.Print($"==Components==: {entry.Name}");
                    }

                    i1 = content.IndexOf(">External links</a>", StringComparison.InvariantCultureIgnoreCase);
                    if (items.Count == 0 && i1 > 0)
                    {
                        var i2 = content.IndexOf("</ol>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                        if (i2 == -1)
                            i2 = content.IndexOf("</ul>", i1 + 17, StringComparison.InvariantCultureIgnoreCase);
                        var list = content.Substring(i1 + 17, i2 - i1 - 17);
                        ParseList(indexName, timeStamp, list, items);
                        Debug.Print($"External link: {items.Count}, {entry.Name}");
                    }

                    i1 = content.IndexOf(">Ticker Symbol</a></th>", StringComparison.InvariantCultureIgnoreCase);
                    if (items.Count == 0 && i1 > 0)
                    {
                        items.Clear();
                        var i2 = content.Substring(0, i1).LastIndexOf("<table", StringComparison.InvariantCultureIgnoreCase);
                        var i3 = content.IndexOf("</table", i2, StringComparison.InvariantCultureIgnoreCase);
                        var table = content.Substring(i2, i3 - i2);
                        ParseTable(indexName, timeStamp, table, items);
                        Debug.Print($"Ticker Symbol: {items.Count}, {entry.Name}");
                    }

                    if (items.Count == 0 && timeStamp < new DateTime(2010, 1, 1))
                    {
                        Debug.Print($"??????: {entry.Name}");
                        continue;
                    }

                    if (items.Count == 0)
                        throw new Exception("Check parser");

                    IndexDbItem.SaveToDb(items.Values);

                    // Changes
                    i1 = content.IndexOf(" changes to the list of ", StringComparison.InvariantCultureIgnoreCase);
                    if (i1 > 0)
                    {
                        Debug.Print($"Changes: {entry.Name}");
                        i1 = content.IndexOf(">Reason", StringComparison.InvariantCultureIgnoreCase);
                        var i2 = content.IndexOf("</table>", i1 + 7, StringComparison.InvariantCultureIgnoreCase);
                        var table = content.Substring(i1, i2 - i1);

                        var lastDate = DateTime.MinValue;
                        var rows = table.Split(new[] { "</tr>" }, StringSplitOptions.None);
                        for (var k = 0; k < rows.Length; k++)
                        {
                            var cells = rows[k].Trim().Split(new[] { "</td>" }, StringSplitOptions.None);

                            if (cells.Length > 4)
                            {
                                var offset = 0;
                                var date = lastDate;
                                var firstCell = GetCellValue(cells[0]);
                                if (firstCell.Contains(' ') && (firstCell.Contains("19") || firstCell.Contains("20")))
                                {
                                    var sDate = GetCellValue(cells[0]).Replace("th, ", ", ");
                                    if (sDate == "Apr/May 2011") sDate = "April 27, 2011";

                                    date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);
                                    lastDate = date;
                                    offset = 1;
                                }
                                var addedSymbol = GetCellValue(cells[0 + offset]);
                                var addedName = GetCellValue(cells[1 + offset]);
                                var removedSymbol = GetCellValue(cells[2 + offset]);
                                var removedName = GetCellValue(cells[3 + offset]);
                                var addedSymbols = new[] {addedSymbol};
                                if (addedSymbol != null)
                                    addedSymbols = addedSymbol.Split(',');
                                foreach (var aSymbol in addedSymbols.Where(a=>!string.IsNullOrEmpty(a)))
                                {
                                    var aSymbol2 = string.IsNullOrEmpty(aSymbol) ? null : aSymbol.Trim();
                                    var item = new Models.IndexDbChangeItem
                                    {
                                        Index = indexName,
                                        Date = date,
                                        AddedSymbol = aSymbol2,
                                        AddedName = addedName,
                                        RemovedSymbol = removedSymbol,
                                        RemovedName = removedName,
                                        TimeStamp = timeStamp
                                    };
                                    changes.Add(item);
                                }
                            }
                        }

                        IndexDbChangeItem.SaveToDb(changes);
                    }
                    else if (indexName.StartsWith("SP") && timeStamp > new DateTime(2016, 1, 1))
                        throw new Exception($"File should have a symbol change section. Check symbol change parser for {entry.Name} in '{zipFileName}'");

                    itemCount += items.Count + changes.Count;
                }

            return itemCount;
        }

        private static void ParseTable(string indexName, DateTime timeStamp, string table, Dictionary<string, Models.IndexDbItem> items)
        {
            var rows = table.Replace("</tbody", "").Split(new[] { "</tr>" }, StringSplitOptions.None);

            // Header
            var symbolNo = -1;
            var nameNo = -1;
            var sectorNo = -1;
            var industryNo = -1;
            var cells = rows[0].Split(new[] { "</td>", "</th>" }, StringSplitOptions.None);
            for (var k = 0; k < cells.Length; k++)
            {
                var cell = GetCellValue(cells[k]);
                if (cell == null) continue;

                if (cell.IndexOf("symbol", StringComparison.InvariantCultureIgnoreCase) != -1)
                    symbolNo = k;
                else if (cell.IndexOf("ticker", StringComparison.InvariantCultureIgnoreCase) != -1)
                    symbolNo = k;
                else if (cell.IndexOf("security", StringComparison.InvariantCultureIgnoreCase) != -1)
                    nameNo = k;
                else if (cell.IndexOf("company", StringComparison.InvariantCultureIgnoreCase) != -1)
                    nameNo = k;
                if (cell.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) != -1)
                    sectorNo = k;
                if (cell.IndexOf("industry", StringComparison.InvariantCultureIgnoreCase) != -1)
                    industryNo = k;
            }

            for (var k = 1; k < rows.Length; k++)
            {
                cells = rows[k].Split(new[] { "</td>" }, StringSplitOptions.None);
                if (cells.Length > 1)
                {
                    var symbol = GetCellValue(cells[symbolNo]);
                    var name = GetCellValue(cells[nameNo]);
                    var sector = sectorNo == -1 ? null : GetCellValue(cells[sectorNo]);
                    var industry = industryNo == -1 ? null : GetCellValue(cells[industryNo]);
                    var item = new Models.IndexDbItem
                    { Index = indexName, Symbol = symbol, Name = name, Sector = sector, Industry = industry, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
            }
        }

        private static void ParseList(string indexName, DateTime timeStamp, string list, Dictionary<string, Models.IndexDbItem> items)
        {
            var rows = list.Split(new[] { "</li>" }, StringSplitOptions.None);
            for (var k = 0; k < rows.Length; k++)
            {
                var s = GetCellValue(rows[k])?.Trim();
                if (s == null) continue;

                if (s.EndsWith(")"))
                {
                    var i1 = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                    var symbol = s.Substring(i1 + 1, s.Length - i1 - 2).Trim();
                    var name = s.Substring(0, i1).Trim();
                    var item = new Models.IndexDbItem { Index = indexName, Symbol = symbol, Name = name, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
                else
                    throw new Exception("Check parser");
            }
        }

        private static void ParseLinks(string indexName, DateTime timeStamp, string list, Dictionary<string, Models.IndexDbItem> items)
        {
            //            var rows = list.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            var rows = list.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (var k = 0; k < rows.Length; k++)
            {
                var s = GetCellValue(rows[k])?.Trim();
                if (s == null) continue;

                if (s.EndsWith(")"))
                {
                    var i1 = s.LastIndexOf("(", StringComparison.InvariantCultureIgnoreCase);
                    var symbol = s.Substring(i1 + 1, s.Length - i1 - 2).Trim();
                    var name = s.Substring(0, i1).Trim();
                    var item = new Models.IndexDbItem { Index = indexName, Symbol = symbol, Name = name, TimeStamp = timeStamp };
                    if (!items.ContainsKey(symbol))
                        items.Add(symbol, item);
                }
                else if (s == "External Links section.''") { }
                else
                    throw new Exception("Check parser");
            }
        }

        private static string GetCellValue(string cell)
        {
            var c1 = cell.Replace("</a>", "").Trim();
            if (c1.EndsWith("</sup>"))
            {
                var i2 = c1.LastIndexOf("<sup", StringComparison.InvariantCultureIgnoreCase);
                c1 = c1.Substring(0, i2);
            }
            var i1 = c1.LastIndexOf(">", StringComparison.InvariantCultureIgnoreCase);
            var ss = c1.Split('>');
            if (ss[ss.Length - 1].EndsWith(")") && ss[ss.Length - 1].IndexOf('(') == -1)
            {
                var i2 = ss[ss.Length - 2].IndexOf('(');
                var val = ss[ss.Length - 2].Substring(0, i2 + 1) + ss[ss.Length - 1];
                return val;
            }
            var value = c1.Substring(i1 + 1);
            if (string.IsNullOrWhiteSpace(value))
                value = null;
            return System.Net.WebUtility.HtmlDecode(value);
        }
    }
}
