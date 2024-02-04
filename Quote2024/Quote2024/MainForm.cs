using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Data.Helpers;
using Quote2024.Helpers;

namespace Quote2024
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
            /*clbIntradayDataList.Items.AddRange(IntradayResults.ActionList.Select(a => a.Key).ToArray());
            cbIntradayStopInPercent_CheckedChanged(null, null);
            for (var item = 0; item < clbIntradayDataList.Items.Count; item++)
            {
                clbIntradayDataList.SetItemChecked(item, true);
            }*/

            StartImageAnimation();

            // Logger.MessageAdded += (sender, args) => StatusLabel.Text = args.FullMessage;
            Data.Helpers.Logger.MessageAdded += (sender, args) => this.BeginInvoke((Action)(() => StatusLabel.Text = args.FullMessage));

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
                    var visitor = new CookieCollector();

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

            var sw = new Stopwatch();
            sw.Start();

            /*var task1 = Task.Run((() =>
            {
                var data = Data.Actions.Polygon.PolygonMinuteScan.GetQuotes().ToArray();
            }));
            await task1;*/

            await Task.Factory.StartNew(Data.Actions.StockAnalysis.StockAnalysisActions.ParseAllFiles);
            // await Task.Factory.StartNew(Data.Scaners.TheFirstScanner.Start);

            // await Task.Factory.StartNew(Data.Actions.Polygon.PolygonMinuteScan.Start);
            // Data.Actions.Nasdaq.NasdaqScreenerLoader.ParseAndSaveToDb(@"E:\Quote\WebData\Screener\Nasdaq\GithubNasdaqStockScreener.zip");

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
            if (CsUtils.OpenZipFileDialog(Data.Actions.Yahoo.YahooCommon.MinuteYahooDataFolder) is string fn && !string.IsNullOrEmpty(fn))
                Data.Actions.Yahoo.YahooMinuteLogToTextFile.YahooMinuteLogSaveToTextFile(new[] { fn }, ShowStatus);

            btnMinuteYahooLog.Enabled = true;
        }
    }
}
