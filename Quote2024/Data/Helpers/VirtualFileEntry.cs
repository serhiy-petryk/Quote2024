using System;

namespace Data.Helpers
{
    public class VirtualFileEntry
    {
        public readonly string Name;
        public readonly string Content;
        public readonly DateTime Timestamp = DateTime.Now;

        public VirtualFileEntry(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}
