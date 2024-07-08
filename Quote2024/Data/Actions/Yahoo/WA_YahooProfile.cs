using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Data.Helpers;
using Microsoft.Data.SqlClient;

namespace Data.Actions.Yahoo
{
    public static class WA_YahooProfile
    {
        // private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}&matchType=prefix&limit=100000&from=2024";
        private const string ListUrlTemplate = "https://web.archive.org/cdx/search/cdx?url=https://finance.yahoo.com/quote/{0}/profile&matchType=prefix&limit=100000&from=2020";
        private const string ListDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_List";
        private const string HtmlDataFolder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data";

        public static void ParseHtml()
        {
            Logger.AddMessage($"Started");

            var folder = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data.Short";
            var files = Directory.GetFiles(folder, "*.html").OrderBy(a=>a).ToArray();
            var cnt = 0;
            var dict = new Dictionary<int, object[]>();
            foreach (var file in files)
            {
                cnt++;
                if (cnt%100==0)
                    Logger.AddMessage($"Parsed {cnt:N0} files from {files.Length:N0}. Last file: {Path.GetFileName(file)}");

                if (file.EndsWith("ACT_20230406035606.html") || file.EndsWith("AXP_20230410144059.html") ||
                    file.EndsWith("OXM_20230429222332.html") || file.EndsWith("SONO_20230422222307.html") ||
                    file.EndsWith("WAB_20230417233217.html") || file.EndsWith("ILMNV_20240624224841.html"))
                { // Bad files
                    continue;
                }

                var content = File.ReadAllText(file);

                var i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                if (i1 > 1)
                {
                    SaveToDict(9);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(1", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 1702, 2020-01-01, 2020-08-03
                { // AA_20200512043535.html
                    var k2 = content.IndexOf("<h1 class=\"D(ib) Fz(1", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector())
                    {

                    }
                    SaveToDict(1);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"svelte-", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 417, 2024-03-23, 2024-05-06
                { // AAON_20240503140454.html
                    var k2 = content.IndexOf("<h1 class=\"svelte-", i1 + 5, StringComparison.InvariantCulture);
                    if (k2 != -1)
                        throw new Exception("Check YahooProfile parser");

                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2())
                    {

                    }
                    SaveToDict(2);
                    continue;
                }

                /*i1 = content.IndexOf("<h1 class=\"D(ib) Fz(16px) Lh(18px)\" data-reactid=\"7\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 1702, 2020-01-01, 2020-08-03
                { // AA_20200512043535.html
                    var i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2==-1) i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector())
                    {

                    }
                    SaveToDict(1);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(18px)\" data-reactid=\"7\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 9240, 2020-03-08, 2022-01-21
                { // A_20201127012313.html
                    var i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector())
                    {

                    }
                    SaveToDict(2);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(18px)\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 9304, 2021-11-04, 2024-04-15
                { //A_20220124182731.html
                    var i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector())
                    {

                    }
                    SaveToDict(3);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"D(ib) Fz(16px) Lh(18px)\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 406, 2023-05-24, 2024-06-27
                { // AAN_20230609055307.html
                    var i2 = content.IndexOf(">Sector(s)<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1) i2 = content.IndexOf(">Sector<", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector3())
                    {

                    }
                    SaveToDict(4);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"svelte-1dqn7jb\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 5, 2023-11-12, 2023-12-30
                { // DISH_20231226205342.html
                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1)
                    {

                    }

                    SaveToDict(5);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"svelte-169tggr\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 10, 2024-01-29, 2024-03-16
                { // AI_20240211152301.html
                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1)
                    {

                    }
                    SaveToDict(6);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"svelte-ufs8hf\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 417, 2024-03-23, 2024-05-06
                { // AAON_20240503140454.html
                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2())
                    {

                    }
                    SaveToDict(7);
                    continue;
                }

                i1 = content.IndexOf("<h1 class=\"svelte-3a2v0c\">", StringComparison.InvariantCulture);
                if (i1 > -1) // Count, from, to: 897, 2024-05-06, 2024-07-05
                { // A_20240627204404.html
                    var i2 = content.IndexOf(">Sector: </dt>", i1, StringComparison.InvariantCulture);
                    if (i2 == -1 && CheckSector2())
                    { //ILMNV_20240624224841.html

                    }
                    SaveToDict(8);
                    continue;
                }*/

                if (file.EndsWith("ACT_20230406035606.html") || file.EndsWith("AXP_20230410144059.html") ||
                    file.EndsWith("OXM_20230429222332.html") || file.EndsWith("SONO_20230422222307.html") ||
                    file.EndsWith("WAB_20230417233217.html"))
                { // Bad files
                    SaveToDict(9);
                    continue;
                }

                if (i1 == -1)
                {
                }
                /*
                var i1 = content.IndexOf(">Sector(s)<", StringComparison.InvariantCulture);
                if (i1 == -1)
                {
                    i1 = content.IndexOf(">Sector: <", StringComparison.InvariantCulture);
                }

                if (i1 == -1)
                {
                    i1 = content.IndexOf(">Sector<", StringComparison.InvariantCulture);
                }

                if (i1 == -1)
                {
                    i1 = content.IndexOf("\"sector\":\"", StringComparison.InvariantCulture);
                }

                if (i1 == -1)
                {
                    i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                    if (i1 == -1)
                    {

                    }
                }*/
                void SaveToDict(int id)
                {
                    var ss = Path.GetFileNameWithoutExtension(file).Split('_');
                    var dateKey = DateTime.ParseExact(ss[ss.Length-1], "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
                    if (!dict.ContainsKey(id))
                    {
                        dict.Add(id, new object[]{1, dateKey, dateKey});
                        return;
                    }

                    var oldData = dict[id];
                    oldData[0] = (int)oldData[0] + 1;
                    var from = (DateTime)oldData[1];
                    if (dateKey < from) oldData[1] = dateKey;
                    var to = (DateTime)oldData[2];
                    if (dateKey > to) oldData[2] = dateKey;
                }

                bool HasSector()
                {
                    return content.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) != -1;
                }
                bool CheckSector()
                {
                    var content1 = content.Replace("https://finance.yahoo.com/sectors\" title=\"Sectors\">Sectors", "\">");
                    var a1 = content1.Length;
                    var a2 = content.Length;
                    content1 = content1.Replace("sector information, ", "");
                    content1 = content1.Replace("finance.yahoo.com/sector/", "x");
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;
                    if (content1.IndexOf(">Profile Information Not Available<",
                            StringComparison.InvariantCulture) != -1)
                    { // CNH_20240520210256.html
                        return false;
                    }

                    // if (content1.IndexOf("playback timings (ms):", StringComparison.InvariantCulture) != -1) return false;
                    return true;
                }

                bool CheckSector2()
                {
                    var content1 = content;
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;
                    if (content1.IndexOf(">Profile Information Not Available<",
                            StringComparison.InvariantCulture) != -1)
                    { // CNH_20240520210256.html
                        return false;
                    }
                    if (content1.IndexOf(">Profile data is currently not available for ",
                            StringComparison.InvariantCulture) != -1)
                    { // ENERW_20240422165327.html
                        return false;
                    }

                    // if (content1.IndexOf("playback timings (ms):", StringComparison.InvariantCulture) != -1) return false;
                    return true;
                }

                bool CheckSector3()
                {
                    var content1 = content.Replace("sector information, ", "");
                    if (content1.IndexOf("sector", StringComparison.InvariantCultureIgnoreCase) == -1)
                        return false;

                    // if (content1.IndexOf("playback timings (ms):", StringComparison.InvariantCulture) != -1) return false;
                    return true;
                }

            }
            Logger.AddMessage($"Finished");

            foreach(var kvp in dict.OrderBy(a=>a.Key))
                Debug.Print($"{kvp.Key}\t{kvp.Value[0]}\t{((DateTime)kvp.Value[1]):yyyy-MM-dd}\t{((DateTime)kvp.Value[2]):yyyy-MM-dd}");
            /*
0	144	2020-08-14	2024-03-05
1	1702	2020-01-01	2020-08-03
2	9240	2020-03-08	2022-01-21
3	9304	2021-11-04	2024-04-15
4	406	2023-05-24	2024-06-27
5	5	2023-11-12	2023-12-30
6	10	2024-01-29	2024-03-16
7	417	2024-03-23	2024-05-06
8	897	2024-05-06	2024-07-05
9	5	2023-04-06	2023-04-29
            */
        }

        public static void ParseHtmlZip()
        {
            var zipFileName = @"E:\Quote\WebData\Symbols\Yahoo\Profile\WA_Data.Short.zip";
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var content = entry.GetContentOfZipEntry();
                    var timeStamp = entry.LastWriteTime.DateTime;
                    //                    var items = new List<SplitModel>();
                    var i1 = content.IndexOf(">Sector(s)<", StringComparison.InvariantCulture);
                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Sector: <", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Sector<", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf("\"sector\":\"", StringComparison.InvariantCulture);
                    }

                    if (i1 == -1)
                    {
                        i1 = content.IndexOf(">Fund Overview<", StringComparison.InvariantCulture);
                        if (i1 == -1)
                        {

                        }
                    }
                }
        }

        public static async Task DownloadData()
        {
            // Get url and file list
            var listFiles = Directory.GetFiles(ListDataFolder, "*.txt");
            var toDownload = new List<DownloadItem>();
            var fileCount = 0;
            foreach (var listFile in listFiles)
            {
                var items = File.ReadAllLines(listFile).Select(a => new JsonItem(a))
                    .Where(a => a.Type == "text/html" && a.Status == 200).OrderBy(a => a.TimeStamp).ToArray();
                var polygonSymbol = Path.GetFileNameWithoutExtension(listFile);
                var lastDateKey = "";
                foreach (var item in items)
                {
                    var dateKey = item.TimeStamp.Substring(0, 8);
                    if (dateKey == lastDateKey) // skeep the same day
                        continue;

                    var url = $"https://web.archive.org/web/{item.TimeStamp}/{item.Url}";
                    var filename = Path.Combine(HtmlDataFolder, $"{polygonSymbol}_{item.TimeStamp}.html");
                    toDownload.Add(new DownloadItem(url, filename));
                    fileCount++;
                    lastDateKey = dateKey;
                }
            }

            // Download data
            var tasks = new ConcurrentDictionary<DownloadItem, Task<byte[]>>();
            var itemCount = 0;
            var needToDownload = true;
            while (needToDownload)
            {
                needToDownload = false;
                foreach (var urlAndFilename in toDownload)
                {
                    itemCount++;

                    if (!File.Exists(urlAndFilename.Filename))
                    {
                        var task = WebClientExt.DownloadToBytesAsync(urlAndFilename.Url);
                        tasks[urlAndFilename] = task;
                        urlAndFilename.StatusCode = null;
                    }
                    else
                        urlAndFilename.StatusCode = HttpStatusCode.OK;

                    if (tasks.Count >= 10)
                    {
                        await Download(tasks);
                        Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
                        tasks.Clear();
                        needToDownload = true;
                    }
                }

                if (tasks.Count > 0)
                {
                    await Download(tasks);
                    tasks.Clear();
                    needToDownload = true;
                }

                Helpers.Logger.AddMessage($"Downloaded {itemCount} url from {toDownload.Count:N0}");
            }

            Helpers.Logger.AddMessage($"Downloaded {toDownload.Count:N0} items");
        }

        private static async Task Download(ConcurrentDictionary<DownloadItem, Task<byte[]>> tasks)
        {
            foreach (var kvp in tasks)
            {
                try
                {
                    var data = await kvp.Value;
                    // var htmlContent = HtmlHelper.RemoveUselessTags(System.Text.Encoding.UTF8.GetString(data));
                    // File.WriteAllText(kvp.Key.Filename, htmlContent);
                    File.WriteAllText(kvp.Key.Filename, System.Text.Encoding.UTF8.GetString(data));
                    kvp.Key.StatusCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    if (ex is WebException exc && exc.Response is HttpWebResponse response &&
                        (response.StatusCode == HttpStatusCode.InternalServerError ||
                         response.StatusCode == HttpStatusCode.BadGateway))
                    {
                        kvp.Key.StatusCode = response.StatusCode;
                        continue;
                    }

                    throw new Exception($"YahooSectorLoader: Error while download from {kvp.Key}. Error message: {ex.Message}");
                }
            }
        }

        public static void DownloadList()
        {
            Logger.AddMessage($"Started");
            var symbols = GetSymbolXref();
            // var symbols = new Dictionary<string,string>{{"MSFT", "MSFT80"}};
            var count = 0;
            foreach (var symbol in symbols)
            {
                count++;
                    Logger.AddMessage($"Process {count} symbols from {symbols.Count}");
                var filename = Path.Combine($@"{ListDataFolder}", $"{symbol.Value}.txt");
                if (!File.Exists(filename))
                {
                    var url = string.Format(ListUrlTemplate, symbol.Key);
                    var o = Helpers.WebClientExt.GetToBytes(url, false);
                    if (o.Item3 != null)
                        throw new Exception(
                            $"WA_YahooProfile: Error while download from {url}. Error message: {o.Item3.Message}");
                    File.WriteAllBytes(filename, o.Item1);
                    System.Threading.Thread.Sleep(200);
                }
            }
            Logger.AddMessage($"Finished");
        }

        private static Dictionary<string, string> GetSymbolXref()
        {
            var data = new Dictionary<string, string>();
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT DISTINCT symbol, YahooSymbol FROM dbQ2024..SymbolsPolygon " +
                                  "WHERE isnull([To],'2099-12-31')>'2020-01-01' and IsTest is null " +
                                  "and MyType not like 'ET%' and MyType not in ('RIGHT') and YahooSymbol is not null order by 1";
                //                "and MyType not like 'ET%' and MyType not in ('FUND', 'RIGHT', 'WARRANT') and YahooSymbol is not null";
                using (var rdr = cmd.ExecuteReader())
                    while (rdr.Read())
                        data.Add((string)rdr["YahooSymbol"], (string)rdr["symbol"]);
            }

            return data;
        }

        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string Symbol;
            public string sDate;
            public string sLastUpdated;
            public string Exchange;
            public string Sector;
            public string Name;
            public DateTime Date => DateTime.ParseExact(sDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
            public DateTime LastUpdated => DateTime.ParseExact(sLastUpdated, "yyyyMMddHHmmss", CultureInfo.InvariantCulture).Date;
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
                if (ss[4] != "-")
                    Status = int.Parse(ss[4]);
            }
        }

        public class DownloadItem
        {
            public string Url;
            public string Filename;
            public HttpStatusCode? StatusCode;

            public DownloadItem(string url, string filename)
            {
                Url = url;
                Filename = filename;
            }
        }
        #endregion


    }
}
