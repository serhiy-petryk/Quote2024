using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Nasdaq
{
    public static class NasdaqScreenerGithubLoader
    {
        private static string StartUrl = "https://github.com/rreichel3/US-Stock-Symbols/commits/main";

        // Github api: потрібно отримати апі ключ, дані дають по сторінкам - макс. 100 комітів на сторінку
        // private static string _githubApiCommits = "https://api.github.com/repos/rreichel3/US-Stock-Symbols/commits";
        // ?! (don't) use request: https://api.github.com/repos/rreichel3/US-Stock-Symbols/commits

        private const string HtmlFileNameTemplate = @"E:\Quote\WebData\Screener\NasdaqGithub\Html\NG_{0}.html";
        private const string JsonFileNameTemplate = @"NG_{1}_{0}.json";
        private const string ZipFileNameTemplate = @"E:\Quote\WebData\Screener\NasdaqGithub\Data\NG_{0}.zip";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var dataFolder = Path.GetDirectoryName(ZipFileNameTemplate);
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            var lastFile = Directory.GetFiles(dataFolder, "*.zip").OrderByDescending(a => a).FirstOrDefault();
            var lastDate = lastFile == null
                ? new DateTime(2000, 1, 1)
                : DateTime.ParseExact(Path.GetFileNameWithoutExtension(lastFile).Split('_')[1], "yyyyMMdd",
                    CultureInfo.InvariantCulture);

            var commits = new Dictionary<DateTime, (string, DateTime)>();

            GetCommitList(commits, lastDate);

            var actualCommits = commits.Where(a => a.Key > lastDate).OrderBy(a=>a.Key).ToArray();
            DownloadFilesAndSaveToDb(actualCommits);

            Logger.AddMessage($"Finished!!");
        }

        private static void ParseAndSaveAllZipFiles()
        {
            var zipFolder = Path.GetDirectoryName(ZipFileNameTemplate);
            var files = Directory.GetFiles(zipFolder, "*.zip").OrderBy(a=> a).ToArray();
            foreach (var file in files)
            {
                Logger.AddMessage($"File: {Path.GetFileName(file)}");
                ParseAndSaveToDbZipFile(file);
            }
        }

        private static void ParseAllHtmlFiles(Dictionary<DateTime, (string,DateTime)> commits)
        {
            Logger.AddMessage($"Prepare commit list");
            var folder = Path.GetDirectoryName(HtmlFileNameTemplate);
            var files = Directory.GetFiles(folder, "*.html");
            foreach(var file in files)
                ParseHtmlPage(File.ReadAllText(file), commits);
        }

        private static void GetCommitList(Dictionary<DateTime, (string, DateTime)> commits, DateTime lastDate)
        {
            Logger.AddMessage($"Prepare commit list");

            var url = StartUrl;
            while (url != null)
            {
                var o = Download.DownloadToBytes(url, false, true);
                if (o is Exception ex)
                    throw new Exception($"NasdaqScreenerGithubLoader: Error while download from {url}. Error message: {ex.Message}");

                /*
                var filename = string.Format(HtmlFileNameTemplate, cnt.ToString());
                if (File.Exists(filename))
                    File.Delete(filename);

                File.WriteAllBytes(filename, (byte[])o);*/

                var htmlContent = System.Text.Encoding.UTF8.GetString((byte[])o);
                url = ParseHtmlPage(htmlContent, commits);

                if (commits.Any(a => a.Key <= lastDate))
                    break;
            }
        }

        private static void DownloadFilesAndSaveToDb(KeyValuePair<DateTime, (string, DateTime)>[] commits)
        {
            var exchanges = new[] { "Amex", "Nasdaq", "Nyse" };
            var urlTemplate = "https://raw.githubusercontent.com/rreichel3/US-Stock-Symbols/{0}/{1}/{1}_full_tickers.json";
            foreach (var commit in commits)
            {
                Logger.AddMessage($"Download files for {commit.Key:yyyy-MM-dd}");

                var timeStamp = commit.Key.ToString("yyyyMMdd");
                var zipFileName = string.Format(ZipFileNameTemplate, timeStamp);
                if (!File.Exists(zipFileName))
                {
                    var virtualFileEntries = new List<VirtualFileEntry>();
                    foreach (var exchange in exchanges)
                    {
                        var url = string.Format(urlTemplate, commit.Value.Item1, exchange.ToLower());
                        // Can't check json format -> last symbol is '\n'
                        var o = Download.DownloadToBytes(url, false, true);
                        if (o is Exception ex)
                            throw new Exception(
                                $"NasdaqScreenerGithubLoader: Error while download from {url}. Error message: {ex.Message}");

                        var jsonFileName = Path.Combine(Path.GetFileNameWithoutExtension(zipFileName),
                            string.Format(JsonFileNameTemplate, timeStamp, exchange));
                        var entry = new VirtualFileEntry(jsonFileName, (byte[])o);
                        virtualFileEntries.Add(entry);
                    }

                    ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);
                    File.SetLastWriteTime(zipFileName, commit.Value.Item2);
                }
                ParseAndSaveToDbZipFile(zipFileName);
            }
        }

        private static void ParseAndSaveToDbZipFile(string zipFileName)
        {
            DbUtils.ClearAndSaveToDbTable(Array.Empty<object>(), "dbQ2023Others..Bfr_ScreenerNasdaqStock", "Symbol", "Exchange",
                "Name", "LastSale", "Volume", "NetChange", "Change", "MarketCap", "Country", "IpoYear",
                "Sector", "Industry", "TimeStamp", "Date");
            var timestamp = File.GetLastWriteTime(zipFileName);
            var dateKey = DateTime.ParseExact(Path.GetFileNameWithoutExtension(zipFileName).Split('_')[1], "yyyyMMdd",
                CultureInfo.InvariantCulture);
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var exchange = entry.Name.Split('_')[1].ToUpper();
                    var items = ZipUtils.DeserializeZipEntry<Nasdaq.NasdaqScreenerLoader.cStockRow[]>(entry);
                    var dbItems = items.Select(a =>
                        new NasdaqScreenerLoader.DbStockRow(exchange, timestamp, a, dateKey));

                    DbUtils.SaveToDbTable(dbItems, "dbQ2023Others..Bfr_ScreenerNasdaqStock", "Symbol", "Exchange",
                        "Name", "LastSale", "Volume", "NetChange", "Change", "MarketCap", "Country", "IpoYear",
                        "Sector", "Industry", "TimeStamp", "Date");
                }
                DbUtils.RunProcedure("dbQ2023Others..pUpdateScreenerNasdaqGithub");
            }
        }

        private static string ParseHtmlPage(string content, Dictionary<DateTime, (string, DateTime)> items)
        {
            var n1 = content.IndexOf("{\"payload\":{", StringComparison.InvariantCulture);
            var n2 = content.IndexOf("</script>", n1 + 20, StringComparison.InvariantCulture);
            var json = content.Substring(n1, n2 - n1);
            var oo = SpanJson.JsonSerializer.Generic.Utf16.Deserialize<cRoot>(json);

            foreach (var commitGroup in oo.payload.commitGroups)
            {
                var hasCommit = false;
                foreach (var commit in commitGroup.commits)
                {
                    if (!string.Equals(commit.shortMessage, "generated",
                            StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    var date = DateTime.Parse(commit.committedDate, CultureInfo.InvariantCulture);
                    var dateKey = date.AddHours(-12).Date;
                    var id = commit.oid;
                    items.Add(dateKey, (id,date));
                    hasCommit = true;
                    break;
                }

                if (!hasCommit)
                {
                    var commitGroupDate = DateTime.Parse(commitGroup.title, CultureInfo.InvariantCulture);
                    if (commitGroupDate > new DateTime(2021, 2, 1))
                        throw new Exception("Please, check html parser in NasdaqScreenerGithubLoader method");
                }
            }

            if (oo.payload.filters.pagination.hasNextPage)
                return @"https://github.com/rreichel3/US-Stock-Symbols/commits/main?after=" + oo.payload.filters.pagination.endCursor;//.Replace(" ", "+");

            return null;
        }

        #region =========  Json classes  ============
        private class cRoot
        {
            public cPayload payload;
        }
        private class cPayload
        {
            public cCommitGroup[] commitGroups;
            public cFilters filters;
        }
        private class cCommitGroup
        {
            public string title;
            public cCommit[] commits;
        }
        private class cCommit
        {
            public string oid;
            public string authoredDate;
            public string committedDate;
            public string shortMessage;
            public string shortMessageMarkdownLink;

        }

        private class cFilters
        {
            public cPagination pagination;
        }
        private class cPagination
        {
            public string startCursor;
            public string endCursor;
            public bool hasNextPage;
            public bool hasPreviousPage;
        }
        #endregion
    }
}
