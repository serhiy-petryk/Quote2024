using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using SevenZip;

namespace Data.Helpers
{
    public static class ZipUtils
    {
        // 2.75 times faster than non-parallel execution
        public static void ProcessZipByParallel(string zipFileName, Func<ZipArchiveEntry, bool> entryFilter,
            Action<ZipArchiveEntry> action)
        {
            var threads = Math.Max(1, Environment.ProcessorCount - 1);
            string[] entries;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                entries = zip.Entries.Where(a => entryFilter == null || entryFilter(a)).Select(a => a.FullName)
                    .ToArray();

            var size = entries.Length / threads;
            var chunks = entries
                .Select((s, i) => new { Value = s, Index = i })
                .GroupBy(x => x.Index / size)
                .Select(grp => grp.ToDictionary(a => a.Value, a => (object)null)).ToArray();

            var numbers = Enumerable.Range(0, chunks.Length).ToArray();
            Parallel.ForEach(numbers, number =>
            {
                using (var fs = File.Open(zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, false))
                {
                    var thisEntryNames = chunks[number];
                    var thisEntries = archive.Entries.Where(a => thisEntryNames.ContainsKey(a.FullName));
                    foreach (var entry in thisEntries)
                        action(entry);
                }
            });
        }

        public static T DeserializeZipEntry<T>(ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var memstream = new MemoryStream())
            {
                entryStream.CopyTo(memstream);
                var bytes = memstream.ToArray();
                if (bytes.Length>2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // contains BOM
                    return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes.Skip(3).ToArray());
                return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes);
            }
        }

        public static T DeserializeBytes<T>(byte[] bytes ) => SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes);
        public static T DeserializeString<T>(string entryContent) => SpanJson.JsonSerializer.Generic.Utf16.Deserialize<T>(entryContent);

        public static void CompressFolder(string folder, string zipFileName)
        {
            var tmp = new SevenZipCompressor();
            tmp.ArchiveFormat = OutArchiveFormat.Zip;
            tmp.CompressDirectory(folder, zipFileName);
        }

        public static void ZipVirtualFileEntries(string zipFileName, IEnumerable<VirtualFileEntry> entries)
        {
            // -- Very slowly: another way for 7za -> use Process/ProcessStartInfo class
            // see https://stackoverflow.com/questions/71343454/how-do-i-use-redirectstandardinput-when-running-a-process-with-administrator-pri

            var tmp = new SevenZipCompressor {ArchiveFormat = OutArchiveFormat.Zip};
            var dict = entries.ToDictionary(a=>a.Name, a=> a.Stream);
            tmp.CompressStreamDictionary(dict, zipFileName);

            foreach (var item in entries) item?.Dispose();
        }

        #region =========  Extensions for ZipArchiveEntry  ===========
        public static IEnumerable<string> GetLinesOfZipEntry(this ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    yield return line;
            }
        }
        public static string GetContentOfZipEntry(this ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                return reader.ReadToEnd();
        }
        #endregion
    }
}
