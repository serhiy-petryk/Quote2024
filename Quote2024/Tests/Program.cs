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
            // Json.Utf8JsonTests.Start();
            // Task.Factory.StartNew(WebSocket.WebSocketTests.DoClientWebSocket);

            WebSocket.WebSocketTests.ParseFile();

            Application.Run(new TestMainForm());
        }
    }
}
