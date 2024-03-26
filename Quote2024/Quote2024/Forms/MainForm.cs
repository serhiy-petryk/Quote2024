using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Data.Helpers;

namespace Quote2024.Forms
{
    public partial class MainForm : Form
    {
        private ChromiumWebBrowser browser;// = new ChromiumWebBrowser("www.eoddata.com");
        private System.Net.CookieContainer eoddataCookies = new System.Net.CookieContainer();

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

            browser = new ChromiumWebBrowser("www.eoddata.com");
            browser.FrameLoadEnd += Browser_FrameLoadEnd;
        }

        private async void Browser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            if (e.Frame.IsMain)
            {
                var isLogged = await IsLogged();
                if (isLogged)
                {
                    // MessageBox.Show("OK");
                    var cookieManager = Cef.GetGlobalCookieManager();
                    var visitor = new Quote2024.Helpers.CookieCollector();

                    cookieManager.VisitUrlCookies(browser.Address, true, visitor);

                    var cookies = await visitor.Task; // AWAIT !!!!!!!!!
                    eoddataCookies = new System.Net.CookieContainer();
                    foreach (var cookie in cookies)
                    {
                        eoddataCookies.Add(new System.Net.Cookie()
                        {
                            Name = cookie.Name,
                            Value = cookie.Value,
                            Domain = cookie.Domain
                        });
                    }

                    Data.Actions.Eoddata.EoddataCommon.FnGetEoddataCookies = () => { return eoddataCookies; };
                    Invoke(new Action(() =>
                    {
                        lblEoddataLogged.Text = "Logged in eoddata.com";
                        lblEoddataLogged.BackColor = System.Drawing.Color.Green;
                    }));

                    // var cookieHeader = CookieCollector.GetCookieHeader(cookies);
                    return;
                }

                var userAndPassword = Data.Helpers.CsUtils.GetApiKeys("eoddata.com")[0].Split('^');
                var script = $"document.getElementById('ctl00_cph1_lg1_txtEmail').value='{userAndPassword[0]}';" +
                    $"document.getElementById('ctl00_cph1_lg1_txtPassword').value='{userAndPassword[1]}';" +
                    "document.getElementById('ctl00_cph1_lg1_chkRemember').checked = true;" +
                    "document.getElementById('ctl00_cph1_lg1_btnLogin').click();";
                browser.ExecuteScriptAsync(script);
            }
        }

        private async Task<bool> IsLogged()
        {
            const string script = @"(function()
{
  return document.getElementById('ctl00_cph1_lg1_lblName') == undefined ? null : document.getElementById('ctl00_cph1_lg1_lblName').innerHTML;
  })();";

            var response = await browser.EvaluateScriptAsync(script);
            var logged = response.Success && Equals(response.Result, "Sergei Petrik");
            Invoke(new Action(() =>
            {
                if (logged)
                {
                    lblEoddataLogged.Text = "Logged in eoddata.com";
                    lblEoddataLogged.BackColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblEoddataLogged.Text = "Not logged in eoddata.com";
                    lblEoddataLogged.BackColor = System.Drawing.Color.Red;
                }
            }));

            return logged;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            browser?.Dispose();
            CefSharp.Cef.Shutdown();
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

            // await Task.Factory.StartNew(Data.Scanners.CheckQuotes.Start);

            // await Task.Factory.StartNew(Data.Actions.Wikipedia.WikipediaIndexLoader.ParseAndSaveToDbAllFiles);

            // await Task.Factory.StartNew(Data.Actions.Chartmill.ChartmillScreenerLoader.ParseAllZipFiles);

            // await Task.Factory.StartNew(Data.Scanners.QuoteScanner.StartHourHalf);
            // await Task.Factory.StartNew(Data.Scanners.HourPolygon.StartHour);
            // await Task.Factory.StartNew(Data.Scanners.QuoteScanner.StartHour);

            // await Task.Factory.StartNew(Data.Tests.RealTimeYahooMinuteTests.RegularMarketTime);
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

        private void btnOpenRealTime_Click(object sender, EventArgs e)
        {
            var form = new RealTimeForm();
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
    }
}
