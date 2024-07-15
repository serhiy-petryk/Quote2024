using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data.Helpers;

namespace Quote2024.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            if (CsUtils.IsInDesignMode) return;

            // dataGridView1.Height = 18 * Data.Models.LoaderItem.DataGridLoaderItems.Count + 2;
            dataGridView1.Paint += new PaintEventHandler(dataGridView1_Paint);
            dataGridView1.DataSource = Data.Models.LoaderItem.DataGridLoaderItems;

            //=========================
            StatusLabel.Text = "";
            Data.Helpers.Logger.DefaultShowStatus = ShowStatus;

            /*clbIntradayDataList.Items.AddRange(IntradayResults.ActionList.Select(a => a.Key).ToArray());
            cbIntradayStopInPercent_CheckedChanged(null, null);
            for (var item = 0; item < clbIntradayDataList.Items.Count; item++)
            {
                clbIntradayDataList.SetItemChecked(item, true);
            }*/

            StartImageAnimation();
        }

        #region ===========  Image Animation  ============
        // taken from https://social.msdn.microsoft.com/Forums/windows/en-US/0d9e790e-6816-40e7-96fe-bbf333a4abc0/show-animated-gif-in-datagridview?forum=winformsdatacontrols
        void dataGridView1_Paint(object sender, PaintEventArgs e)
        {
            //Update the frames. The cell would paint the next frame of the image late on.
            ImageAnimator.UpdateFrames();
        }
        private void StartImageAnimation()
        {
            var image = Data.Models.LoaderItem.GetAnimatedImage();
            ImageAnimator.Animate(image, new EventHandler(this.OnFrameChanged));
        }
        private void OnFrameChanged(object o, EventArgs e)
        {
            if (dataGridView1.Columns.Count > 4)
            {
                //Force a call to the Paint event handler.
                dataGridView1.InvalidateColumn(1);
                dataGridView1.InvalidateColumn(4);
            }
        }
        #endregion

        #region ==========  EventHandlers of controls  ===========
        private void dataGridView1_SelectionChanged(object sender, EventArgs e) => dataGridView1.ClearSelection();
        #endregion

        private async void btnRunMultiItemsLoader_Click(object sender, EventArgs e)
        {
            ((Control)sender).Enabled = false;
            dataGridView1.ReadOnly = true;

            foreach (var item in Data.Models.LoaderItem.DataGridLoaderItems)
                item.Reset();

            foreach (var item in Data.Models.LoaderItem.DataGridLoaderItems.Where(a => a.Checked))
                await item.Start();

            dataGridView1.ReadOnly = false;
            ((Control)sender).Enabled = true;
        }

        private async void btnTest_Click(object sender, EventArgs e)
        {
            btnTest.Enabled = false;

            // Data.Helpers.StatMethods.Tests();
            var sw = new Stopwatch();
            sw.Start();

            // var s = Data.Actions.StockAnalysis.StockAnalysisActions.GetJsonContent();

            // await Task.Factory.StartNew(Data.Helpers.HtmlHelper.ProcessFolder);
            // await Task.Factory.StartNew(Data.Helpers.HtmlHelper.TestFile);

            await Task.Factory.StartNew(Data.Actions.Yahoo.WA_YahooProfile.ParseAndSaveToDb);
            // await Task.Factory.StartNew(Data.Actions.Yahoo.WA_YahooProfile.DownloadList);
            // await Data.Actions.Yahoo.WA_YahooProfile.DownloadData();
            // await Task.Factory.StartNew(Data.Actions.Yahoo.WA_YahooProfile.ParseHtml);

            // await Data.Actions.Yahoo.YahooSectorLoader.Start();
            // await Task.Factory.StartNew(Data.Actions.Yahoo.YahooSectorLoader.Parse);

            // await Task.Factory.StartNew(Data.Actions.Yahoo.YahooSectorLoader.TestParse);

            // await Data.Actions.Yahoo.YahooProfileLoader.Start();
            // await Task.Factory.StartNew(Data.Actions.Yahoo.YahooProfileLoader.RemoveNotFound);

            // await Task.Factory.StartNew(Data.Actions.MorningStar.MorningStarJsonProfileLoader.TestParse);
            // await Data.Actions.MorningStar.MorningStarJsonProfileLoader.Start();

            // await Task.Factory.StartNew(Data.Actions.MorningStar.MorningStarSymbolsLoader.Test);
            // await Data.Actions.MorningStar.MorningStarSymbolsLoader.StartJson();
            // await Task.Factory.StartNew(Data.Actions.MorningStar.MorningStarSymbolsLoader.ParseAllJson);
            // await Task.Factory.StartNew(Data.Scanners.CheckQuotes.Start);

            // await Task.Factory.StartNew(Data.Actions.Polygon.PolygonTradesLoader.Start);
            // await Task.Factory.StartNew(Data.Actions.Polygon.PolygonNbboLoader.Start);
            // await Task.Factory.StartNew(Data.Strategies.BigTickWithBigTradeCount.StartDown);

            // await Task.Factory.StartNew(Data.Tests.DBQ.DbqTest.InvestigateNewCompressor);

            // await Task.Factory.StartNew(Data.Actions.Wikipedia.WikipediaIndexLoader.ParseAndSaveToDbAllFiles);

            // await Task.Factory.StartNew(Data.Actions.Chartmill.ChartmillScreenerLoader.ParseAllZipFiles);

            // await Task.Factory.StartNew(Data.Scanners.QuoteScanner.StartHourHalf);
            // await Task.Factory.StartNew(Data.Tests.ScannerIntraday.Test);
            // await Task.Factory.StartNew(Data.Tests.ScannerIntraday.Test2);
            // await Task.Factory.StartNew(Data.Scanners.HourPolygon.StartHour);
            // await Task.Factory.StartNew(Data.Scanners.QuoteScanner.StartHour);

            // await Task.Factory.StartNew(Data.Tests.RealTimeYahooMinuteTests.DefineDelay);
            // await Task.Factory.StartNew(Data.Tests.WebSocketFiles.YahooDelayRun);
            // await Task.Factory.StartNew((() => Data.RealTime.YahooMinutes.GetTickerList(1)));
            // await Task.Factory.StartNew(Data.RealTime.YahooMinutes.InitTimer);

            // await Task.Factory.StartNew(Data.Tests.WebSocketFiles.YahooDelayRun);
            // await Task.Factory.StartNew(Data.Tests.Twelvedata.TestComplexCall);

            // await Task.Factory.StartNew(Data.Actions.Nasdaq.NasdaqScreenerGithubLoader.Start);
            // await Task.Factory.StartNew(Data.Actions.MorningStar.WA_MorningStarScreenerLoader.Start);

            // await Task.Factory.StartNew(()=>Data.Actions.Wikipedia.WikipediaIndexLoader.ParseAndSaveToDb(@"E:\Quote\WebData\Indices\Wikipedia\IndexComponents\WebArchive.Wikipedia.Indices.zip"));

            // await Task.Factory.StartNew(TradesPerMinute.Start);
            // await Task.Factory.StartNew(Data.Actions.Polygon.PolygonSymbolsLoader.ParseAndSaveAllZip);

            sw.Stop();
            var d1 = sw.ElapsedMilliseconds;
            Debug.Print($"Test duration: {d1:N0} milliseconds");

            btnTest.Enabled = true;
        }

        private void ShowStatus(string message)
        {
            if (statusStrip1.InvokeRequired)
                Invoke(new MethodInvoker(delegate { ShowStatus(message); }));
            else
                StatusLabel.Text = message;

            Application.DoEvents();
        }

        private void btnMinuteYahooLog_Click(object sender, EventArgs e)
        {
            btnMinuteYahooLog.Enabled = false;
            if (CsUtils.OpenZipFileDialog(Data.Actions.Yahoo.YahooCommon.MinuteYahooDataFolder) is string fn &&
                !string.IsNullOrWhiteSpace(fn))
                Data.Actions.Yahoo.YahooMinuteLogToTextFile.YahooMinuteLogSaveToTextFile(new[] { fn }, ShowStatus);
            btnMinuteYahooLog.Enabled = true;
        }

        private void btnMinuteYahooErrorCheck_Click(object sender, EventArgs e)
        {
            btnMinuteYahooErrorCheck.Enabled = false;

            if (CsUtils.OpenFileDialogMultiselect(Data.Actions.Yahoo.YahooCommon.MinuteYahooDataFolder,
                    @"zip files (*.zip)|*.zip") is string[] files && files.Length > 0)
                Data.Actions.Yahoo.YahooMinuteChecks.CheckOnBadQuoteAndsplit(files, ShowStatus);
            
            btnMinuteYahooErrorCheck.Enabled = true;
        }

        private void btnMinuteYahooSaveLogToDb_Click(object sender, EventArgs e)
        {
            btnMinuteYahooSaveLogToDb.Enabled = false;

            if (CsUtils.OpenFileDialogMultiselect(Data.Actions.Yahoo.YahooCommon.MinuteYahooDataFolder, @"zip files (*.zip)|*.zip") is string[] files && files.Length > 0)
                Data.Actions.Yahoo.YahooMinuteSaveLogToDb.Start(files, ShowStatus);

            btnMinuteYahooSaveLogToDb.Enabled = true;
        }

        private void btnOpenWebSocket_Click(object sender, EventArgs e)
        {
            var form = new WebSocketClientApp.MainForm();
            form.Show();
        }

        private void btnOpenTimeSalesNasdaq_Click(object sender, EventArgs e)
        {
            var form = new TimeSalesNasdaqForm();
            form.Show();
        }

        private void btnOpenTestForm_Click(object sender, EventArgs e)
        {
            var form = new TestForm();
            form.Show();
        }

        private void btnOpenRealTimeYahoo_Click(object sender, EventArgs e)
        {
            var form = new RealTimeYahooForm();
            form.Show();
        }

        private void btnFinageMinuteForm_Click(object sender, EventArgs e)
        {
            var form = new MinuteFinageForm();
            form.Show();
        }

        private void btnFinageTradesForm_Click(object sender, EventArgs e)
        {
            var form = new TradesFinageForm();
            form.Show();
        }

        private void btnYahooWebSocketForm_Click(object sender, EventArgs e)
        {
            var form = new WebSocketYahooForm();
            form.Show();
        }
    }
}
