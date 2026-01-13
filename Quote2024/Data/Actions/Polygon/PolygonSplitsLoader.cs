using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public static class PolygonSplitsLoader
    {
        private const string UrlTemplate = @"https://api.massive.com/stocks/v1/splits?execution_date.gte={0}&execution_date.lte={1}&limit=5000&sort=execution_date.asc";
        private const string DataFolder = Settings.DataFolder + @"Splits\Polygon\Data";

        public static void Start()
        {
            var oLastDate = DbHelper.ExecuteScalarSql("SELECT max([Date]) as MaxDate FROM dbQ2024..SplitPolygon");
            var lastDate = oLastDate is DBNull ? new DateTime(2004, 1, 1) : ((DateTime)oLastDate).AddDays(-6);
            var timeStamp = TimeHelper.GetTimeStamp();
            var zipFileName = Path.Combine(DataFolder, $@"PolygonSplits_{timeStamp.Item2}.zip");

            var cnt = 0;
            var url = string.Format(UrlTemplate, lastDate.ToString("yyyy-MM-dd"),
                timeStamp.Item1.ToString("yyyy-MM-dd"));
            var virtualFileEntries = new List<VirtualFileEntry>();
            var dbItems = new List<cResult>();
            if (File.Exists(zipFileName))
                throw new Exception($"File already exist. File name: {zipFileName}");

            while (url != null)
            {
                Logger.AddMessage($"Downloading {cnt} data chunk into {Path.GetFileName(zipFileName)} from {lastDate:yyyy-MM-dd} to {timeStamp.Item1:yyyy-MM-dd}");

                url = url + "&apiKey=" + PolygonCommon.GetApiKey2003();

                var o = WebClientExt.GetToBytes(url, true);
                if (o.Item3 != null)
                    throw new Exception(
                        $"PolygonSplitsLoader: Error while download from {url}. Error message: {o.Item3.Message}");

                var entry = new VirtualFileEntry($@"PolygonSplits_{cnt:D2}_{timeStamp.Item2}.json", o.Item1);
                virtualFileEntries.Add(entry);

                var oo = ZipUtils.DeserializeBytes<cRoot>(entry.Content);
                dbItems.AddRange(oo.results);

                if (oo.status != "OK")
                    throw new Exception($"Wrong status of response! Status is '{oo.status}'");

                url = oo.next_url;
                cnt++;
            }

            // Save to zip file
            ZipUtils.ZipVirtualFileEntries(zipFileName, virtualFileEntries);

            if (dbItems.Count > 0)
            {
                DbHelper.ClearAndSaveToDbTable(dbItems, "dbQ2024..Bfr_SplitPolygon", "id", "ticker", "Date",
                    "split_from", "split_to", "adjustment_type");

                DbHelper.RunProcedure("dbQ2024..pUpdateSplitPolygon");
            }

            Logger.AddMessage($"!Finished. Items: {dbItems.Count:N0}. Zip file size: {CsUtils.GetFileSizeInKB(zipFileName):N0}KB. Filename: {zipFileName}");
        }

        public class cRoot
        {
            public cResult[] results;
            public string status;
            // public string request_id;
            public string next_url;
        }

        public class cResult
        {
            public string id;
            public string execution_date;
            public float split_from;
            public float split_to;
            public string ticker;
            public string adjustment_type;

            public DateTime Date => DateTime.ParseExact(execution_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
