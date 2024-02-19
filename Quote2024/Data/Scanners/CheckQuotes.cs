using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Data.Actions.Polygon;
using Data.Helpers;

namespace Data.Scanners
{
    public class CheckQuotes
    {
        public static void Start()
        {
            var folder = @"E:\Quote\WebData\Minute\Polygon2003\Data";
            var files = Directory.GetFiles(folder, "*.zip");
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");

                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                        if (oo.count == 0) continue;

                        foreach (var q in oo.results.Where(o=>!o.IsValid))
                        {
                            Debug.Print($"File: {Path.GetFileName(zipFileName)}. Entry: {entry.Name}. Quote: {q}");
                        }
                    }
            }
            Logger.AddMessage($"Finished!");
        }
    }
}
