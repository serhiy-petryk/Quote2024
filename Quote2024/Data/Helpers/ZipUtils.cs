using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpanJson.Resolvers;

namespace Data.Helpers
{
    public static class ZipUtils
    {
        public static T DeserializeJson<T>(ZipArchiveEntry entry)
        {
            var x = new SpanJsonOptions();
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
            {
                using (var memstream = new MemoryStream())
                {
                    reader.BaseStream.CopyTo(memstream);
                    var bytes = memstream.ToArray();
                    return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes);
                }
            }
        }

        public static T DeserializeJson<T>(string entryContent) => SpanJson.JsonSerializer.Generic.Utf16.Deserialize<T>(entryContent);

        public static void ZipVirtualFileEntries(string zipFileName, IEnumerable<VirtualFileEntry> entries)
        {
            using (var zipArchive = System.IO.Compression.ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                foreach (var entry in entries)
                {
                    var oldEntries = zipArchive.Entries.Where(a =>
                        string.Equals(a.FullName, entry.Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
                    foreach (var o in oldEntries)
                        o.Delete();

                    var zipEntry = zipArchive.CreateEntry(entry.Name);
                    using (var writer = new StreamWriter(zipEntry.Open()))
                        writer.Write(entry.Content);
                    zipEntry.LastWriteTime = new DateTimeOffset(entry.Timestamp);
                }
        }

        public static string GetContentOfZipEntry(this ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var reader = new StreamReader(entryStream, System.Text.Encoding.UTF8, true))
                return reader.ReadToEnd();
        }

    }
}
