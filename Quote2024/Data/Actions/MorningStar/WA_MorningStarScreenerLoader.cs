using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

            var data = new Dictionary<string, List<DbItem>>();

            // DownloadList();
            // DownloadData();
            ParseHtmlFiles(data);
            SaveToDb(data);

            Logger.AddMessage($"Finished!");
        }

        public static void SaveToDb(Dictionary<string, List<DbItem>> data)
        {
            var items = data.SelectMany(a => a.Value).OrderBy(a=>a.Symbol).ToArray();
            DbUtils.ClearAndSaveToDbTable(items, "dbQ2023Others..HScreenerMorningStar", "Symbol", "Date", "LastDate",
                "Exchange", "Sector", "Name");
            
            DbUtils.ExecuteSql("UPDATE a SET Sector=b.CorrectSectorName FROM dbQ2023Others..HScreenerMorningStar a INNER JOIN dbQ2023Others..SectorXref b on a.Sector = b.BadSectorName");
        }

        public static void ParseHtmlFiles(Dictionary<string, List<DbItem>> dictData)
        {
            var htmlFiles = Directory.GetFiles(HtmlDataFolder, "*.html").OrderBy(a=>a).ToArray();
            var data = new List<(string, string, string, string, string)>();
            var data2 = new List<(string, string, string, string, string)>();
            foreach (var htmlFile in htmlFiles)
            {
                var ss = Path.GetFileNameWithoutExtension(htmlFile).Split('_');
                var timeKey = ss[1];
                var sector = MorningStarCommon.GetSectorName(ss[0]);

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
                    if (!MorningStarCommon.Exchanges.Any(exchange.Contains)) continue;

                    var k3 = s.IndexOf('/', k2+1);
                    var symbol = s.Substring(k2+1, k3-k2-1).ToUpper();
                    var k4 = s.IndexOf('>');
                    var k5 = s.IndexOf("</a>", StringComparison.Ordinal);
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
                                dictData.Add(symbol, new List<DbItem>());
                                var newDbItem = new DbItem()
                                {
                                    Symbol = symbol,
                                    sDate = timeKey,
                                    sLastDate = timeKey,
                                    Exchange = exchange,
                                    Name = name,
                                    Sector = sector
                                };
                                dictData[symbol].Add(newDbItem);
                            }
                            else
                            {
                                var lastItem = dictData[symbol][dictData[symbol].Count - 1];
                                if (lastItem.Sector != sector || lastItem.Exchange != exchange || lastItem.Name!= name)
                                {
                                    var newDbItem = new DbItem()
                                    {
                                        Symbol = symbol, sDate = timeKey, sLastDate = timeKey, Exchange = exchange,
                                        Name = name, Sector = sector
                                    };

                                    if (lastItem.LastDate != newDbItem.Date || lastItem.Name.Length < newDbItem.Name.Length)
                                        dictData[symbol].Add(newDbItem);
                                }
                                else
                                {
                                    // only change [LastDate] timeKey
                                    dictData[symbol][dictData[symbol].Count - 1].sLastDate = timeKey;
                                }
                            }
                        }
                    }
                    else
                    {

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
                    Logger.AddMessage($"Downloaded {cnt++} from {items.Length} pages for {MorningStarCommon.GetSectorName(sectorKey)}");
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
            foreach (var sector in MorningStarCommon.Sectors)
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

                var i1 = content.IndexOf(">LAST:<", StringComparison.Ordinal);
                if (i1 == -1)
                {
                    IsBad = true;
                    return;
                }

                var i22 = content.LastIndexOf("<table ", i1, StringComparison.Ordinal);
                var i32 = content.IndexOf("</table>", i22, StringComparison.Ordinal);
                var rows2 = content.Substring(i22 + 7, i32 - i22 - 7).Trim().Split(new[] { "</tr>" }, StringSplitOptions.RemoveEmptyEntries);

                var i21 = content.LastIndexOf("<table ", i22 - 7, StringComparison.Ordinal);
                var i31 = content.IndexOf("</table>", i21, StringComparison.Ordinal);
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

        public class DbItem
        {
            public string Symbol;
            public string sDate;
            public string sLastDate;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime Date => DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
            public DateTime LastDate => DateTime.ParseExact(sLastDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
        }

        private class JsonItem
        {
            public string Url;
            public string TimeStamp;
            public string Type;
            public int Status;

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
