using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TradesCompressor
{
    public static class ZipUtils
    {
        public static T DeserializeZipEntry<T>(ZipArchiveEntry entry)
        {
            using (var entryStream = entry.Open())
            using (var memstream = new MemoryStream())
            {
                entryStream.CopyTo(memstream);
                var bytes = memstream.ToArray();
                if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) // contains BOM
                    return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes.Skip(3).ToArray());
                return SpanJson.JsonSerializer.Generic.Utf8.Deserialize<T>(bytes);
            }
        }
    }
}
