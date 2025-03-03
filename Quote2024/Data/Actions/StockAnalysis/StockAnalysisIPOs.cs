using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.StockAnalysis
{
    public class StockAnalysisIPOs
    {
        private const string Url = @"https://stockanalysis.com/api/screener/s/f?m=ipoDate&s=desc&c=ipoDate,s,n,ipoPrice,ippc,exchange,sector,industry,employees&cn=5000&i=histip";
        private const string Folder = Settings.DataFolder + @"Splits\StockAnalysis\IPOs\";

        public static void Start()
        {
            Logger.AddMessage($"Started");

            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = Folder + $"StockAnalysisIPOs_{timeStamp.Item2}.zip";

            // Download html data
            var o = WebClientExt.GetToBytes(Url, true);
            if (o.Item3 != null)
                throw new Exception($"StockAnalysisIPOs: Error while download from {Url}. Error message: {o.Item3.Message}");

            // Save html data to zip
            var entry = new VirtualFileEntry($"StockAnalysisIPOs_{timeStamp.Item2}.json", o.Item1);
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
                    cRoot oo;

                    // Data files store 'ipoPrice' in number format before 2023-03 and store it in string format after 2023-03
                    // Convert 'ipoPrice' from string format to number
                    using (var entryStream = entry.Open())
                    using (var memstream = new MemoryStream())
                    {
                        entryStream.CopyTo(memstream);
                        var s = System.Text.Encoding.UTF8.GetString(memstream.ToArray());
                        var i1 = s.IndexOf("\"ipoPrice\":\"", StringComparison.InvariantCultureIgnoreCase);
                        while (i1 != -1)
                        {
                            var i2 = s.IndexOf("\"", i1 + 12, StringComparison.InvariantCulture);
                            var s1 = s.Substring(0, i1 + 11);
                            var value = s.Substring(i1 + 12,i2-i1-12 );
                            var s3 = s.Substring(i2+1);

                            s = s1 + value + s3;
                            i1 = s.IndexOf("\"ipoPrice\":\"", StringComparison.InvariantCultureIgnoreCase);
                        }

                        oo = ZipUtils.DeserializeString<cRoot>(s);
                    }

                    // var oo = ZipUtils.DeserializeZipEntry<cRoot>(entry);
                    foreach (var item in oo.data.data)
                        item.TimeStamp = entry.LastWriteTime.DateTime;
                    itemCount += oo.data.data.Length;

                    // Save data to database
                    if (oo.data.data.Length > 0)
                    {
                        DbHelper.ClearAndSaveToDbTable(oo.data.data, "dbQ2024..Bfr_IpoStockAnalysis", "pSymbol", "pDate", "pExchange",
                            "pName", "pIpoPrice", "pCurrentPrice", "pSector", "pIndustry", "IEmployees", "TimeStamp");

                        DbHelper.RunProcedure("dbQ2024..pUpdateIpoStockAnalysis");
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
            public string resultsCount;
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
            public float? employees;
            public int IEmployees => Convert.ToInt32(employees);

            public string pSymbol => s;
            public DateTime pDate => ipoDate;
            public string pExchange => exchange;
            public string pName => n;
            public float pIpoPrice => ipoPrice;
            public float pCurrentPrice => ippc;
            public string pSector => string.IsNullOrWhiteSpace(sector) ? null : sector;
            public string pIndustry => string.IsNullOrWhiteSpace(industry) ? null : industry;
            public DateTime TimeStamp;
        }
        #endregion
    }
}
