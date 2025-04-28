using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Eoddata
{
    public class EoddataSymbolsLoader
    {
        private static readonly string[] Exchanges = new string[] {"AMEX", "NASDAQ", "NYSE", "OTCBB"};
        private const string UrlTemplate = @"https://www.eoddata.com/Data/symbollist.aspx?e={0}";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = TimeHelper.GetTimeStamp();
            var virtualFileEntries = new List<VirtualFileEntry>();
            var zipFileName = Settings.DataFolder + $@"Symbols\Eoddata\SymbolsEoddata_{timeStamp.Item2}.zip";

            // Prepare cookies
            var cookies = EoddataCommon.GetEoddataCookies();

            // Download data
            foreach (var exchange in Exchanges)
            {
                Logger.AddMessage($"Download Symbols data for {exchange}");
                var url = string.Format(UrlTemplate, exchange);
                var o = WebClientExt.GetToBytes(url, false, false, cookies);
                if (o.Item3 != null)
                    throw new Exception($"EoddataSymbolsLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                var entry = new VirtualFileEntry($@"{Path.GetFileNameWithoutExtension(zipFileName)}\{exchange}_{timeStamp.Item2}.txt", o.Item1);
                virtualFileEntries.Add(entry);
            }

            // Save to zip file
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            // Parse and save data to database
            Logger.AddMessage($"'{zipFileName}' file is parsing");
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"Run sql procedure: pUpdateSymbolsXref");
            DbHelper.RunProcedure("dbQ2024..pUpdateSymbolsXref");

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var lines = entry.GetLinesOfZipEntry().ToArray();

                    var ss = Path.GetFileNameWithoutExtension(entry.Name).Split('_');
                    var exchange = ss[0].Trim().ToUpper();
                    var date = entry.LastWriteTime.DateTime;

                    if (lines.Length == 0 || !string.Equals(lines[0], "Symbol\tDescription"))
                        throw new Exception($"SymbolsEoddata_Parse error! Please, check the first line of {entry.Name} file in {zipFileName}");

                    var items = lines.Skip(1).Select(line => new SymbolsEoddata(exchange, date, line.Split('\t')))
                        .ToArray();
                    var filteredItems = new List<SymbolsEoddata>();
                    foreach (var item in items)
                    {
                        item.TimeStamp = entry.LastWriteTime.DateTime;
                        if (filteredItems.Count>0 && string.Equals(filteredItems[filteredItems.Count - 1].Symbol, item.Symbol))
                        {
                            if (!string.Equals(filteredItems[filteredItems.Count - 1].Name, item.Name))
                            {
                                if (string.IsNullOrEmpty(item.Name))
                                    continue;

                                if (item.Symbol == "DVS" && item.Name == "Diversified Security Solutions Inc.")
                                    continue;
                                if (item.Symbol == "MBBC" && item.Name == "Monterey Bay Bancorp Inc.")
                                    continue;

                                continue;
                                throw new Exception(
                                    $"Duplicate '{item.Symbol}' in {entry.Name} entry of {Path.GetFileName(zipFileName)}");
                            }
                        }
                        else
                            filteredItems.Add(item);
                    }

                    itemCount += items.Length;

                    // Save data to buffer table of data server
                    DbHelper.ClearAndSaveToDbTable(filteredItems, "dbQ2024..Bfr_SymbolsEoddata", "Symbol", "Exchange", "Name",
                        "TimeStamp");
                    DbHelper.RunProcedure("dbQ2024..pUpdateSymbolsEoddata");
                }

            return itemCount;
        }

        #region ========  SymbolsEoddata Model  =========
        public class SymbolsEoddata
        {
            public string Symbol;
            public string Exchange;
            public string Name;
            public DateTime TimeStamp;

            public SymbolsEoddata(string exchange, DateTime timeStamp, string[] ss)
            {
                this.Exchange = exchange.ToUpper();
                Symbol = ss[0].Trim().ToUpper();
                Name = ss[1].Trim();
                if (string.IsNullOrWhiteSpace(Name)) Name = null;
                TimeStamp = timeStamp;
            }

            // public override string ToString() => this.Symbol + "\t" + this.Exchange + "\t" + this.Name + "\t" + CsUtils.GetString(this.Created);
        }
        #endregion
    }
}
