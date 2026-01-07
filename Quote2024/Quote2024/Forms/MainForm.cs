using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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

            // Data.Actions.Polygon.PolygonSplits.ParseAllFiles();

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
                Data.Actions.Yahoo.YahooMinuteChecks.CheckOnBadQuoteAndSplit(files, ShowStatus);
            
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

        private async void btnDailyBy5Minutes_Click(object sender, EventArgs e)
        {
            btnDailyBy5Minutes.Enabled = false;

            var sw = new Stopwatch();
            sw.Start();

            await Task.Factory.StartNew(Data.Scanners.DailyBy5Minutes.Start);

            sw.Stop();
            var d1 = sw.ElapsedMilliseconds / 1000;
            Debug.Print($"Duration: {d1:N0} seconds");

            btnDailyBy5Minutes.Enabled = true;
        }

        private async void btnIntradayBy5Minutes_Click(object sender, EventArgs e)
        {
            btnIntradayBy5Minutes.Enabled = false;

            var sw = new Stopwatch();
            sw.Start();

            await Task.Factory.StartNew(Data.Scanners.IntradayBy5Minutes.Start);

            sw.Stop();
            var d1 = sw.ElapsedMilliseconds/1000;
            Debug.Print($"Duration: {d1:N0} seconds");

            btnIntradayBy5Minutes.Enabled = true;
        }
    }
}
