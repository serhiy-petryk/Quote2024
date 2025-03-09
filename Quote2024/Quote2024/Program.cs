using System;
using System.Net;
using System.Windows.Forms;
using Quote2024.Forms;

namespace Quote2024
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            HttpWebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            //      HttpWebRequest.DefaultWebProxy.als = CredentialCache.DefaultCredentials;
            FtpWebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            System.Net.ServicePointManager.DefaultConnectionLimit = 20000;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Data.Settings.SetZipLibrary();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Data.Tests.DailySql_2024_10.Start();
            // Data.Actions.StockAnalysis.StockAnalysisIPOs.ParseAndSaveToDb(@"E:\Quote\WebData\Splits\StockAnalysis\IPOs\StockAnalysisIPOs_20250208.zip");
            // Data.Actions.StockAnalysis.StockAnalysisIPOs.ParseAndSaveToDb(@"E:\Quote\WebData\Splits\StockAnalysis\IPOs\StockAnalysisIPOs_20250308.zip");

            Data.Helpers.WebClientExt.CheckVpnConnection();

            Application.Run(new MainForm());
        }
    }
}
