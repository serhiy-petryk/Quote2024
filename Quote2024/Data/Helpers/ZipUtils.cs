using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using SevenZip;
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
