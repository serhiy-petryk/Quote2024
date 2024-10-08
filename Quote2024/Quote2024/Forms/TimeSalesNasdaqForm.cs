﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data;
using Data.Helpers;
using Data.RealTime;

namespace Quote2024.Forms
{
    public partial class TimeSalesNasdaqForm : Form
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();

        private Dictionary<string, byte[]> _validTickers;
        private Dictionary<string, Exception> _invalidTickers;

        private string[] Tickers => txtTickerList.Text.Split(new []{"\t", " ", "\n"}, StringSplitOptions.RemoveEmptyEntries).Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim()).ToArray();

        private readonly string _baseFolder = Settings.DataFolder + @"Minute\NasdaqTimeSales";
        private string _dataFolder;

        private int _tickCount;
        private int TickCount
        {
            get => _tickCount;
            set
            {
                _tickCount = value;
                this.BeginInvoke((Action)(UpdateTickLabel));
            }
        }

        private int _errorCount;
        private int ErrorCount
        {
            get => _errorCount;
            set
            {
                _errorCount = value;
                this.BeginInvoke((Action)(UpdateTickLabel));
            }
        }

        private void UpdateTickLabel()
        {
            lblTickCount.ForeColor = ErrorCount == 0 ? Color.Black : Color.DarkRed;
            var text = $@"Ticks: {TickCount:N0}";
            if (ErrorCount != 0)
                text += $@". Errors: {ErrorCount:N0}";
            if (_invalidTickers != null && _invalidTickers.Count != 0)
                text += $@". Invalid tickers: {_invalidTickers.Count:N0}";

            lblTickCount.Text = text;
        }

        public TimeSalesNasdaqForm()
        {
            InitializeComponent();

            _timer.Interval = 61000;
            _timer.Elapsed += _timer_Elapsed;
            lblTickCount.Text = lblStatus.Text = "";
            RefreshUI();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            TickCount = 0;
            ErrorCount = 0;
            btnStart.Enabled = false;

            Debug.Print($"Nasdaq TimeSales started: {DateTime.Now.TimeOfDay}");

            var tickers = await Data.RealTime.TimeSalesNasdaq.CheckTickers(ShowStatus, Tickers);

            _validTickers = tickers.Item1;
            _invalidTickers = tickers.Item2;

            if (tickers.Item2.Count > 0)
            {
                if (MessageBox.Show($@"There are some invalid tickers: {string.Join(", ", tickers.Item2.Keys)}. Continue?",
                        null, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            }

            if (tickers.Item1.Count == 0)
            {
                MessageBox.Show(@"No valid tickers", null, MessageBoxButtons.OK);
                return;
            }

            _dataFolder = Path.Combine(_baseFolder, TimeHelper.GetCurrentEstDateTime().ToString("yyyy-MM-dd"));
            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            TimeSalesNasdaq.SaveResult(tickers.Item1, _dataFolder);

            //_timer.Start();
            RefreshUI();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (statusStrip1.InvokeRequired)
                Invoke(new MethodInvoker(RefreshUI));
            else
            {
                btnStart.Enabled = !_timer.Enabled;
                btnStop.Enabled = _timer.Enabled;
            }
        }

        private async void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TickCount++;
            var nyTime = TimeHelper.GetCurrentEstDateTime();
            if (nyTime.AddMinutes(-15).TimeOfDay > Settings.MarketEndCommon)
            {
                btnStop_Click(sender, EventArgs.Empty);
                return;
            }
            else if (nyTime.AddMinutes(15).TimeOfDay > Settings.MarketStart)
            {
                await TimeSalesNasdaq.Run(ShowStatus, Tickers, _dataFolder, OnError);
            }
        }

        private void OnError(string arg1, Exception arg2)
        {
            ErrorCount++;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();
        }

        private void txtTickerList_TextChanged(object sender, EventArgs e) => lblTickerList.Text = $@"Tickers (divided by space or tab): {Tickers.Length} items";

        private async void btnUpdateList_Click(object sender, EventArgs e)
        {
            btnUpdateList.Enabled = false;
            txtTickerList.Text = "";
            await Task.Factory.StartNew((() =>
            {
                var tickers = RealTimeCommon.GetTickerList(ShowStatus, Convert.ToInt32(numPreviousDays.Value),
                    Convert.ToSingle(numMinTradeValue.Value), Convert.ToSingle(numMaxTradeValue.Value),
                    Convert.ToInt32(numMinTradeCount.Value), Convert.ToSingle(numMinClose.Value),
                    Convert.ToSingle(numMaxClose.Value)).OrderBy(a => a);

                this.BeginInvoke((Action)(() =>
                {
                    txtTickerList.Text = string.Join('\t', tickers);
                    btnUpdateList.Enabled = true;
                }));
            }));
        }

        private void ShowStatus(string message) => lblStatus.Text = message;

        private async void btnDownloadForAllDay_Click(object sender, EventArgs e)
        {
            btnDownloadForAllDay.Enabled = false;
            await Data.RealTime.TimeSalesNasdaq.DownloadAllData(ShowStatus, Tickers);
            btnDownloadForAllDay.Enabled = true;
        }
    }
}
