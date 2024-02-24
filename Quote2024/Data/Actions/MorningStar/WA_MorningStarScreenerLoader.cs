using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public static class WA_MorningStarScreenerLoader
    {
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=morningstar.com/{0}&matchType=prefix&limit=100000";
        private const string ListDataFolder = @"E:\Quote\WebData\Screener\MorningStar\WA_Data\List";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Screener\MorningStar\WA_Data";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            // DownloadList();
            // DownloadData();
            ParseHtmlFiles();

            Logger.AddMessage($"Finished!");
        }

        public static void ParseHtmlFiles()
        {
            var htmlFiles = Directory.GetFiles(HtmlDataFolder, "*.html").OrderBy(a=>a).ToArray();
            var data = new List<(string, string, string, string, string)>();
            var data2 = new List<(string, string, string, string, string)>();
            var dictData = new Dictionary<string, List<(string, string, string, string, string, string)>>();
            foreach (var htmlFile in htmlFiles)
            {
                var ss = Path.GetFileNameWithoutExtension(htmlFile).Split('_');
                var timeKey = ss[1];
                var sector = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ss[0]
                    .Replace("-stocks", "", StringComparison.InvariantCultureIgnoreCase)
                    .Replace("-", " ", StringComparison.InvariantCultureIgnoreCase));

                var countBefore = data.Count;
                var countBefore2 = data2.Count;
                var content = File.ReadAllText(htmlFile);
                var ss1 = content.Split("<a href=\"/web/" + timeKey+"/https://www.morningstar.com/stocks/");
                for (var k1 = 1; k1 < ss1.Length; k1++)
                {
                    var s = ss1[k1];
                    var k2 = s.IndexOf('/');
                    if (k2 > 10) continue;

                    var exchange = s.Substring(0,k2).ToUpper();
                    var k3 = s.IndexOf('/', k2+1);
                    var symbol = s.Substring(k2+1, k3-k2-1).ToUpper();
                    var k4 = s.IndexOf('>');
                    var k5 = s.IndexOf("</a>", StringComparison.InvariantCulture);
                    var name = s.Substring(k4 + 1, k5 - k4 - 1).Trim();
                    if (name.EndsWith("</span>"))
                    {
                        var k6 = name.Substring(0, name.Length - 1).LastIndexOf('>');
                        name = name.Substring(k6 + 1, name.Length - k6 - 8).Trim();
                    }
                    if (!string.Equals(symbol, name))
                    {
                        name = System.Net.WebUtility.HtmlDecode(name);
                        if (ss1[0].Contains("Gainers, Losers, Actives"))
                            data2.Add((sector,timeKey, symbol, exchange, name));
                        else
                        {
                            var item = (sector, timeKey, symbol, exchange, name);
                            data.Add(item);
                            if (!dictData.ContainsKey(symbol))
                            {
                                dictData.Add(symbol, new List<(string, string, string, string, string, string)>());
                                dictData[symbol].Add((sector, timeKey, timeKey, symbol, exchange, name));
                            }
                            else
                            {
                                var lastItem = dictData[symbol][dictData[symbol].Count - 1];
                                if (lastItem.Item1 != sector || lastItem.Item5 != exchange || lastItem.Item6 != name)
                                {
                                    var newItem = (sector, timeKey, timeKey, symbol, exchange, name);
                                    dictData[symbol].Add(newItem);
                                }
                                else
                                {
                                    // only change [To] timeKey
                                    var newItem = (lastItem.Item1, lastItem.Item2, timeKey, lastItem.Item4, lastItem.Item5, lastItem.Item6);
                                    dictData[symbol][dictData[symbol].Count - 1] = newItem;
                                }
                            }
                        }
                    }
                }
                Debug.Print((data.Count - countBefore) + "\t" + (data2.Count - countBefore2) + "\t" + ss1.Length + "\t" + Path.GetFileNameWithoutExtension(htmlFile));
            }
        }

        public static void DownloadData()
        {
            var listFiles = Directory.GetFiles(ListDataFolder, "*.txt");
            foreach (var listFile in listFiles)
            {
                var items = File.ReadAllLines(listFile).Select(a => new JsonItem(a)).Where(a => a.Type == "text/html" && a.Status == 200).ToArray();
                var sectorKey = Path.GetFileNameWithoutExtension(listFile);
                var cnt = 0;
                Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 8 }, item =>
                {
                    Logger.AddMessage($"Downloaded {cnt++} from {items.Length} pages for {item.Sector}");
                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var filename = Path.Combine(HtmlDataFolder, $"{sectorKey}_{item.TimeStamp}.html");
                    if (!File.Exists(filename))
                    {
                        var o = Download.DownloadToBytes(url, false);
                        if (o is Exception ex)
                            throw new Exception(
                                $"WA_MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                        File.WriteAllBytes(filename, (byte[])o);
                        if (!File.Exists(filename))
                        {

                        }
                    }
                });
            }
        }

        public static void DownloadList()
        {
            foreach (var sector in MorningStarScreenerLoader.Sectors)
            {
                Logger.AddMessage($"Process {sector} sector");
                var url = string.Format(ListUrlTemplate, sector);
                var filename = Path.Combine($@"{ListDataFolder}", $"{sector}.txt");
                var o = Helpers.Download.DownloadToBytes(url, false);
                if (o is Exception ex)
                    throw new Exception(
                        $"WA_MorningStarScreenerLoader: Error while download from {url}. Error message: {ex.Message}");
                File.WriteAllBytes(filename, (byte[])o);


            }
        }

        #region =========  SubClasses  ===========

        /*private class DbItem
        {
            public string Symbol;
            public DateTime Date;
            public string Source => "quote";
            public string Exchange;
            public string Name;
            public float Open;
            public float High;
            public float Low;
            public float Last;
            public long Volume;
            public DateTime TimeStamp;

            public bool IsBad;

            public DbItem(string exchange, DateTime timeStamp, string content)
            {
                Exchange = exchange;
                TimeStamp = timeStamp;

                var i1 = content.IndexOf(">LAST:<", StringComparison.InvariantCulture);
                if (i1 == -1)
                {
                    IsBad = true;
                    return;
                }

                var i22 = content.LastIndexOf("<table ", i1, StringComparison.InvariantCulture);
                var i32 = content.IndexOf("</table>", i22, StringComparison.InvariantCulture);
                var rows2 = content.Substring(i22 + 7, i32 - i22 - 7).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

                var i21 = content.LastIndexOf("<table ", i22 - 7, StringComparison.InvariantCulture);
                var i31 = content.IndexOf("</table>", i21, StringComparison.InvariantCulture);
                var rows1 = content.Substring(i21 + 7, i31 - i21 - 7).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

                var cells = rows1[0].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Symbol = GetCellContent(cells[0]);
                Name = GetCellContent(cells[1]);
                var sDate = GetCellContent(cells[2]);
                Date = DateTime.Parse(sDate, CultureInfo.InvariantCulture);

                cells = rows2[0].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Last = float.Parse(GetCellContent(cells[0]), NumberStyles.Any, CultureInfo.InvariantCulture);
                Open = float.Parse(GetCellContent(cells[2]), NumberStyles.Any, CultureInfo.InvariantCulture);
                High = float.Parse(GetCellContent(cells[3]), NumberStyles.Any, CultureInfo.InvariantCulture);
                Volume = long.Parse(GetCellContent(cells[5]), NumberStyles.Any, CultureInfo.InvariantCulture);

                cells = rows2[1].Trim().Split(new[] { "</td>" }, StringSplitOptions.RemoveEmptyEntries);
                Low = float.Parse(GetCellContent(cells[2]), NumberStyles.Any, CultureInfo.InvariantCulture);
            }
        }*/

        private class JsonItem
        {
            public string Url;
            public string TimeStamp;
            public string Type;
            public int Status;

            public string Sector => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Path
                .GetFileNameWithoutExtension(Url.Split('?')[0])
                .Replace("-stocks", "", StringComparison.InvariantCultureIgnoreCase)
                .Replace("-", " ", StringComparison.InvariantCultureIgnoreCase));

            public JsonItem(string line)
            {
                var ss = line.Split(' ');
                TimeStamp = ss[1];
                Url = ss[2];
                Type = ss[3];
                Status = int.Parse(ss[4]);
            }
        }
        #endregion

    }
}
