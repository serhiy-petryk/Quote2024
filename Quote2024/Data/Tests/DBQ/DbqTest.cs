using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Tests.DBQ
{
    public static class DbqTest
    {
        public static void Run()    //76 274 milliseconds
        {
            var files = Directory.GetFiles(@"E:\Quote\WebData\Trades\Polygon\Data\2024-04-05", "*.zip");
            foreach (var zipFileName in files.Take(3))
            {
                Logger.AddMessage($"File: {zipFileName}");
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonDailyLoader.cRoot>(entry);
                    }
            }

        }
    }
}