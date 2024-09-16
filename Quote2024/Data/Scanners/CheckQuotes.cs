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
            var lockObj = new object();
            var folder = Settings.DataFolder + @"Minute\Polygon2003\Data";
            var files = Directory.GetFiles(folder, "MP2003_20230325.zip");
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");

                long itemCount = 0;
                long nullItems = 0;
                int entryCount = 0;
                var sw = new Stopwatch();
                sw.Start();
                ZipUtils.ProcessZipByParallel(zipFileName, entry => entry.Length > 0, entry =>
                {
                    entryCount++;
                    var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                    if (oo.count == 0)
                    {
                        nullItems++;
                        return;
                    }

                    lock (lockObj)
                    {
                        itemCount += oo.count;
                    }

                    foreach (var q in oo.results.Where(o => !o.IsValid))
                    {
                        Debug.Print(
                            $"File: {Path.GetFileName(zipFileName)}. Entry: {entry.Name}. Quote: {q}");
                    }

                });
                sw.Stop();
                Debug.Print($"Time: {sw.ElapsedMilliseconds:N0} msecs.\tEntries: {entryCount:N0}.\tNull items: {nullItems:N0}.\tItems: {itemCount:N0}.");
            }

            Logger.AddMessage($"Finished!");
        }

        public static void StartNonParallel()
        {
            var folder = Settings.DataFolder + @"Minute\Polygon2003\Data";
            var files = Directory.GetFiles(folder, "MP2003_20230325.zip");
            long itemCount = 0;
            var sw = new Stopwatch();
            sw.Start();
            foreach (var zipFileName in files)
            {
                Logger.AddMessage($"File: {Path.GetFileName(zipFileName)}");

                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                    foreach (var entry in zip.Entries.Where(a => a.Length > 0))
                    {
                        var oo = ZipUtils.DeserializeZipEntry<PolygonCommon.cMinuteRoot>(entry);
                        if (oo.count == 0) continue;

                        itemCount += oo.count;
                        foreach (var q in oo.results.Where(o => !o.IsValid))
                        {
                            Debug.Print($"File: {Path.GetFileName(zipFileName)}. Entry: {entry.Name}. Quote: {q}");
                        }
                    }
            }

            Debug.Print($"Total: {sw.ElapsedMilliseconds:N0}. Items: {itemCount:N0}");
            Logger.AddMessage($"Finished!");
        }
    }
}
