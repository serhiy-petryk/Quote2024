using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data;
using Data.Actions.Yahoo;
using Data.Helpers;
using Data.RealTime;

namespace Quote2024.Forms
{
    public partial class WebSocketYahooForm: Form
    {
        private string[] Tickers => txtTickerList.Text.Split(new[] { "\t", " ", "\n" }, StringSplitOptions.RemoveEmptyEntries).Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim()).ToArray();

        private readonly string _dataFolder = Path.Combine(@"E:\Quote\WebData\RealTime\YahooSocket\Data",
            TimeHelper.GetCurrentEstDateTime().ToString("yyyy-MM-dd"));

        private YahooSocket[] _sockets;

        private int _disconnectionCount;
        private int DisconnectionCount
        {
            get => _disconnectionCount;
            set
            {
                _disconnectionCount = value;
                this.BeginInvoke((Action)(UpdateTickLabel));
            }
        }

        private void UpdateTickLabel()
        {
            lblTickCount.ForeColor = DisconnectionCount == 0 ? Color.Black : Color.DarkRed;
            var text = $@"Disconnections: {DisconnectionCount:N0}";

            lblTickCount.Text = text;
        }

        public WebSocketYahooForm()
        {
            InitializeComponent();

            lblTickCount.Text = lblStatus.Text = "";
            RefreshUI();
            btnUpdateList_Click(null, null);
        }

        private Dictionary<string, List<string>> GetSplittedTickers()
        {
            var indexTickers = new List<string>();
            var tickers = new List<string>();
            var splittedTickers = new Dictionary<string, List<string>>() { {Path.Combine(_dataFolder, "YWS_0_{0}.txt"), indexTickers} };
            foreach (var ticker in Tickers)
            {
                if (ticker.StartsWith('^'))
                {
                    indexTickers.Add(ticker);
                    continue;
                }
                tickers.Add(ticker);
                if (tickers.Count >= splittedTickers.Count)
                {
                    splittedTickers.Add(Path.Combine(_dataFolder, $"YSocket_{splittedTickers.Count}_{{0}}.txt"), tickers);
                    tickers = new List<string>();
                }
            }
            if (tickers.Count > 0)
                splittedTickers.Add(Path.Combine(_dataFolder, $"YSocket_{splittedTickers.Count}_{{0}}.txt"), tickers);

            return splittedTickers;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            DisconnectionCount = 0;
            btnStart.Enabled = false;

            Logger.AddMessage($"Initializing YahooSockets: {DateTime.Now.TimeOfDay:hh\\:mm\\:ss}", ShowStatus);

            var dateTimeKey = DateTime.Now.ToString("yyyyMMddHHmmss");
            _sockets = GetSplittedTickers()
                .Select(a => new YahooSocket(string.Format(a.Key, dateTimeKey), a.Value, OnDisconnect)).ToArray();

            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            foreach (var socket in _sockets.Skip(2).Take(1))
            {
                socket.Start();
                await Task.Delay(50);
            }

            Logger.AddMessage($"YahooSockets were initialized: {DateTime.Now.TimeOfDay:hh\\:mm\\:ss}", ShowStatus);
            RefreshUI();
        }

        private void OnDisconnect(string message)
        {

            Logger.AddMessage($"Last disconnection: {message}", ShowStatus);
            DisconnectionCount++;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Stop();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (statusStrip1.InvokeRequired)
                Invoke(new MethodInvoker(RefreshUI));
            else
            {
                btnStart.Enabled = _sockets == null;
                btnStop.Enabled = _sockets != null;
                btnFlush.Enabled = btnStop.Enabled;
            }
        }

        private void Stop()
        {
            if (_sockets != null)
            {
                foreach (var socket in _sockets)
                    socket.Dispose();
                _sockets = null;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Stop();

            base.OnClosed(e);
        }

        private void txtTickerList_TextChanged(object sender, EventArgs e) => lblTickerList.Text = $@"Tickers (divided by space or tab): {Tickers.Length} items";

        private async void btnUpdateList_Click(object sender, EventArgs e)
        {
            btnUpdateList.Enabled = false;
            btnStart.Enabled = false;
            txtTickerList.Text = "";
            await Task.Factory.StartNew((() =>
            {
                var tickers = RealTimeCommon.GetTickerList(ShowStatus, Convert.ToInt32(numPreviousDays.Value),
                    Convert.ToSingle(numMinTradeValue.Value), Convert.ToSingle(numMaxTradeValue.Value),
                    Convert.ToInt32(numMinTradeCount.Value), Convert.ToSingle(numMinClose.Value),
                    Convert.ToSingle(numMaxClose.Value));

                this.BeginInvoke((Action)(() =>
                {
                    var tickerList = new List<string>();
                    if (cbIncludeIndices.Checked)
                        tickerList.AddRange(new[] { "^DJI", "^GSPC" });
                    tickerList.AddRange(tickers.Select(YahooCommon.GetYahooTickerFromPolygonTicker));
                    txtTickerList.Text = string.Join('\t', tickerList);
                    btnUpdateList.Enabled = true;
                    RefreshUI();
                }));
            }));
        }

        private void ShowStatus(string message) => lblStatus.Text = message;

        private void btnFlush_Click(object sender, EventArgs e)
        {
            if (_sockets != null)
            {
                foreach(var socket in _sockets)
                    socket.Flush();
            }
        }
    }
}
