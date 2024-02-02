using System;
using System.Windows.Forms;

namespace Tests
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Zip.ZipFolderCompressor.Start();
            Json.Utf8JsonTests.Start();

            Application.Run(new TestMainForm());
        }
    }
}
