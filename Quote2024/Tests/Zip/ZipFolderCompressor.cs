using System.Diagnostics;
using System.IO;
using SevenZip;

namespace Tests.Zip
{
    public static class ZipFolderCompressor
    {
        public static void Start()
        {
            SevenZipCompressor.SetLibraryPath(@"7z2400\x64-dll\7z.dll");
            // SevenZipCompressor.SetLibraryPath(@"7z1900\x64-dll\7z.dll");

            var sw = new Stopwatch();
            sw.Start();

            var folder = @"E:\Temp\Exchange\xx";
            var zipFileName = folder + ".zip";
            if (File.Exists(zipFileName))
                File.Delete(zipFileName);

            var tmp = new SevenZipCompressor();
            tmp.ArchiveFormat = OutArchiveFormat.Zip;
            tmp.CompressDirectory(folder, zipFileName);

            // CreateZip(folder, zipFileName);

            /*using (var stream = File.OpenWrite(zipFileName))
            {
                tmp.CompressDirectory(folder, stream);
            }*/

            // System.IO.Compression.ZipFile.CreateFromDirectory(folder, zipFileName, System.IO.Compression.CompressionLevel.Optimal, true);

            // =============  Old tests  ===========

            /*var files = Directory.GetFiles(folder, "*.json");
            var tmp = new SevenZipCompressor();
            tmp.ArchiveFormat = OutArchiveFormat.Zip;
            var dict = new Dictionary<string, Stream>();
            var zipStream = File.OpenWrite(zipFileName);*/
            /*foreach (var file in files)
            {
                dict.Clear();
                dict.Add(Path.GetFileName(file), File.OpenRead(file));
                tmp.CompressStreamDictionary(dict, zipStream);
            }*/


            /*var files2 =files.Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 64)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
            foreach (var file2 in files2)
            {
                dict.Clear();
                foreach (var file21 in file2)
                    dict.Add(Path.GetFileName(file21), File.OpenRead(file21));
                tmp.CompressStreamDictionary(dict, zipStream);
            }*/

            /*var files = Directory.GetFiles(folder, "*.json");
            foreach (var file in files)
            {
                var s = File.ReadAllText(file);
                // CreateZipFromFile(file, zipFileName);
                // break;
            }*/

            // ZipFolder(folder);

            sw.Stop();
            var d1 = sw.ElapsedMilliseconds;
            Debug.Print($"Duration: {sw.ElapsedMilliseconds:N0} msecs");
            /*using (ArchiveFile archiveFile = new ArchiveFile(zipFile, "7za.exe"))
            {
                foreach (Entry entry in archiveFile.Entries)
                {
                    Console.WriteLine(entry.FileName);

                    // extract to file
                    // entry.Extract(entry.FileName);

                    // extract to stream
                    byte[] bytes;
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        entry.Extract(memoryStream);
                        bytes = memoryStream.ToArray();
                    }
                }
            }*/

        }

        private  static void CreateZip(string sourceName, string targetZipFileName)
        {
            var p = new ProcessStartInfo();
            // p.FileName = "7za-x64.exe";
            // p.FileName = @"7z1900\x64\7za.exe";

            // p.FileName = @"7z2301\x64-exe\7z.exe";
            p.FileName = @"7z2301\x64-extra\7za.exe";
            // p.FileName = @"7z1900\x64-extra\7za.exe";

            p.Arguments = $"a \"{targetZipFileName}\" \"{sourceName}\"";
            p.WindowStyle = ProcessWindowStyle.Hidden;
            p.CreateNoWindow = true;
            Process x = Process.Start(p);
            x.WaitForExit();
        }

        private static void CreateZipFromFile(string sourceFileName, string targetZipFileName)
        {
            var sourceName = Path.GetFileName(sourceFileName);

            var psi = new ProcessStartInfo();
            psi.FileName = "7za-x64.exe";
            psi.Arguments = $"a \"{targetZipFileName}\" -si\"{sourceName}\"";
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = true;

            Process x = Process.Start(psi);

            x.StandardInput.Write(File.ReadAllText(sourceFileName));
            //x.StandardInput.Flush();
            x.StandardInput.Close();

            // x.WaitForExit();

            /*using (Process p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = @"C:\Windows\System32\diskpart.exe";
                p.StartInfo.Verb = "runas";
                //p.StartInfo.Arguments = "/c diskpart";
                p.StartInfo.RedirectStandardInput = true;

                p.Start();

                StreamWriter myStreamWriter = p.StandardInput;
                myStreamWriter.WriteLine("LIST DISK");
                myStreamWriter.Close();
                p.WaitForExitAsync();
            }*/
        }

        private static string ZipFolder(string folderName)
        {
            var zipFn = (folderName.EndsWith("\\") || folderName.EndsWith("/")
                            ? folderName.Substring(0, folderName.Length - 1)
                            : folderName) + ".zip";
            // var zipFn = Path.GetDirectoryName(folderName) + @"\" + Path.GetFileNameWithoutExtension(folderName) + ".zip";
            if (File.Exists(zipFn))
                File.Delete(zipFn);

            System.IO.Compression.ZipFile.CreateFromDirectory(folderName, zipFn, System.IO.Compression.CompressionLevel.Optimal, true);
            return zipFn;
        }
    }
}
