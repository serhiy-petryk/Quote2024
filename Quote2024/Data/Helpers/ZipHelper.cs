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
                .Select(grp => grp.Select(x => x.Value).ToArray())
                .ToArray();

            var numbers = Enumerable.Range(0, chunks.GetLength(0)).ToArray();

            Parallel.ForEach(numbers, number =>
                {
                    using (FileStream fs = File.Open(zipFileName, FileMode.Open, FileAccess.Read,
                               FileShare.Read))
                    {
                        using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, false))
                        {
                            var thisEntryNames = chunks[number];

                            var thisEntries = archive.Entries.Where(a => thisEntryNames.Contains(a.FullName));
                            foreach (var entry in thisEntries)
                                action(entry);
                        }
                    }
                });
        }

        public static IEnumerable<ZipArchiveEntry> GetEntriesByParallel(string zipFileName,
            Func<ZipArchiveEntry, bool> filter)
        {
            const int threads = 8;
            string[] entries;
            using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                entries = zip.Entries.Where(a => a.Length > 0).Select(a => a.FullName).ToArray();

            var size = entries.Length / threads;
            var chunks = entries
                .Select((s, i) => new { Value = s, Index = i })
                .GroupBy(x => x.Index / size)
                .Select(grp => grp.Select(x => x.Value).ToArray())
                .ToArray();

            var numbers = Enumerable.Range(0, chunks.GetLength(0)).ToArray();
            var aa = numbers.AsParallel().Select<int, IEnumerable<ZipArchiveEntry>>(a =>
            {
                using (FileStream fs = File.Open(zipFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, true))
                    {
                        var thisEntryNames = chunks[a];
                        // var thisEntries = archive.Entries.Where(a => thisEntryNames.Contains(a.FullName)).ToArray();
                        // foreach (var entry in archive.Entries.Where(a => thisEntryNames.Contains(a.FullName)))
                        return archive.Entries.Where(a1 => thisEntryNames.Contains(a1.FullName));
                    }
                }
            });

            foreach (var item in aa.SelectMany(a=>a))
                yield return item;
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
