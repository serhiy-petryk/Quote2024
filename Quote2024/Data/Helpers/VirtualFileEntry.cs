using System;
using System.IO;
using System.Text;

namespace Data.Helpers
{
    public class VirtualFileEntry: IDisposable
    {
        public readonly string Name;
        public readonly byte[] Content;

        private Stream _stream;
        public Stream Stream
        {
            get
            {
                if (_stream == null) _stream = new MemoryStream(Content);
                return _stream;
            }
        }

        public readonly DateTime Timestamp = DateTime.Now;

        public VirtualFileEntry(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public void Dispose() => _stream?.Dispose();
    }

    public class VirtualFileEntryOld
    {
        public readonly string Name;
        public readonly string Content;

        public Stream Stream
        {
            get
            {
                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(Content)))
                    return stream;
            }
        }

        public readonly DateTime Timestamp = DateTime.Now;

        public VirtualFileEntryOld(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
