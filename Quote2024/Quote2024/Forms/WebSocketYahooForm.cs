﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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

        private Dictionary<string, (string,List<string>)> GetSplittedTickersByOneTicker()
        {
            // 1 socket - 1 ticker
            var result = Tickers.OrderBy(a => a).ToDictionary(ticker => ticker,
                ticker => (Path.Combine(_dataFolder, $"YSocket_{ticker}_{{0}}.txt"), new List<string> { ticker }));
            return result;
        }

        private Dictionary<string, List<string>> GetSplittedTickers()
        {
            // For investigation: 44 socket from 1 to 44 tickers in socket.
            var indexTickers = new List<string>();
            var tickers = new List<string>();
            var splittedTickers = new Dictionary<string, List<string>>() { { Path.Combine(_dataFolder, "YSocket_0_{0}.txt"), indexTickers } };
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
            _sockets = GetSplittedTickersByOneTicker().Select(a =>
                new YahooSocket(a.Key, string.Format(a.Value.Item1, dateTimeKey), a.Value.Item2, OnDisconnect)).ToArray();

            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            foreach (var socket in _sockets)
            {
                socket.Start();
                await Task.Delay(300);
            }

            Logger.AddMessage($"YahooSockets were initialized: {DateTime.Now.TimeOfDay:hh\\:mm\\:ss}", ShowStatus);
            RefreshUI();
        }

        private void OnDisconnect(string message)
        {
            Debug.Print(message);
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

        private void ShowStatus(string message)
        {
            if (InvokeRequired)
                Invoke(new Action(() => ShowStatus(message)));
            else
                lblStatus.Text = message;
        }

        private void btnFlush_Click(object sender, EventArgs e)
        {
            if (_sockets != null)
            {
                foreach(var socket in _sockets)
                    socket.Flush();
            }
        }

        private void btnPrintSocketStatuses_Click(object sender, EventArgs e)
        {
            Debug.Print("=======  STATUS of SOCKETS =======");
            if (_sockets == null)
            {
                Debug.Print("No sockets");
                return;
            }

            var messageCountArray = _sockets.OrderByDescending(a => a.MessageCount).ThenBy(a => a.Id).ToArray();
            var messageCount = 0;
            foreach (var socket in messageCountArray)
            {
                Debug.Print($"Ticker: {socket.Id}.\tMessages:\t{socket.MessageCount}");
                messageCount += socket.MessageCount;
            }
            Debug.Print($"TOTAL messages: {messageCount:N0}");
            Debug.Print(Environment.NewLine);

            var notRunning = _sockets.Where(a => a.SocketClient != null && !a.SocketClient.IsRunning).Select(a => a.Id).ToArray();
            if (notRunning.Length > 0)
                Debug.Print($"Not running {notRunning.Length} sockets: " + string.Join(", ", notRunning));
            else
                Debug.Print("All sockets are running");

            var notStarted = _sockets.Where(a => a.SocketClient != null && !a.SocketClient.IsStarted).Select(a => a.Id).ToArray();
            if (notStarted.Length > 0)
                Debug.Print($"Not started {notRunning.Length} sockets: " + string.Join(", ", notRunning));
            else
                Debug.Print("All sockets are started");

        }
    }
}
