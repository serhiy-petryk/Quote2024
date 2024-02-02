using System;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.StockAnaysis
{
    public class StockAnalysisIPOs
    {
        private const string Url = @"https://stockanalysis.com/api/screener/s/f?m=ipoDate&s=desc&c=ipoDate,s,n,ipoPrice,ippc,exchange,sector,industry,employees&cn=5000&i=histip";
        private const string Folder = @"E:\Quote\WebData\Splits\StockAnalysis\IPOs\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = CsUtils.GetTimeStamp();
            var zipFileName = Folder + $"StockAnalysisIPOs_{timeStamp.Item2}.zip";

            // Download html data
            var o = Download.DownloadToBytes(Url, true);
            if (o is Exception ex)
                throw new Exception($"StockAnalysisIPOs: Error while download from {Url}. Error message: {ex.Message}");

            // Save html data to zip
            var entry = new VirtualFileEntry($"StockAnalysisIPOs_{timeStamp.Item2}.json", (byte[])o);
            ZipUtils.ZipVirtualFileEntries(zipFileName, new[] { entry });

            // Parse and save to database
            var itemCount = ParseAndSaveToDb(zipFileName);

            Logger.AddMessage($"!Finished. Items: {itemCount:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public static int ParseAndSaveToDb(string zipFileName)
        {
            var itemCount = 0;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                {
                    var oo = ZipUtils.DeserializeJson<cRoot>(entry);
                    foreach (var item in oo.data.data)
                        item.TimeStamp = entry.LastWriteTime.DateTime;
                    itemCount += oo.data.data.Length;

                    // Save data to database
                    if (oo.data.data.Length > 0)
                    {
                        DbUtils.ClearAndSaveToDbTable(oo.data.data, "dbQ2023Others..Bfr_IpoStockAnalysis", "Symbol", "Date", "Exchange",
                            "Name", "IpoPrice", "CurrentPrice", "Sector", "Industry", "employees", "TimeStamp");

                        DbUtils.RunProcedure("dbQ2023Others..pUpdateIpoStockAnalysis");
                    }
                }

            return itemCount;
        }

        #region =======  Json subclasses  =============
        private class cRoot
        {
            public int status;
            public cData data;
        }

        private class cData
        {
            public int resultsCount;
            public cItem[] data;
        }
        private class cItem
        {
            public string s;
            public DateTime ipoDate;
            public string exchange;
            public string n;
            public float ipoPrice;
            public float ippc;
            public string sector;
            public string industry;
            public int? employees;

            public string Symbol => s;
            public DateTime Date => ipoDate;
            public string Exchange => exchange;
            public string Name => n;
            public float IpoPrice => ipoPrice;
            public float CurrentPrice => ippc;
            public string Sector => string.IsNullOrEmpty(sector) ? null : sector;
            public string Industry => string.IsNullOrEmpty(industry) ? null : industry;
            public DateTime TimeStamp;
        }
        #endregion
    }
}
