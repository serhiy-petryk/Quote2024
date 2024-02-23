using System;
using System.IO;

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

        public VirtualFileEntry(string name, byte[] content)
        {
            Name = name;
            Content = content;
        }

        public void Dispose() => _stream?.Dispose();
    }
}
