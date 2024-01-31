using SevenZip;

namespace Data
{
    public static class Settings
    {
        internal const string DbConnectionString = "Data Source=localhost;Initial Catalog=dbQuote2022;Integrated Security=True;Connect Timeout=150;Encrypt=false";

        public static void SetZipLibrary() => SevenZipBase.SetLibraryPath(@"7z1900\x64-dll\7z.dll");

        // private const string BaseFolder = @"E:\Quote\";
    }
}
