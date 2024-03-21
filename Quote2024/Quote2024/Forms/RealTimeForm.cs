using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Data.Helpers;

namespace Quote2024.Forms
{
    public partial class RealTimeForm : Form
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        
        private Dictionary<string, byte[]> _validSymbols;
        private Dictionary<string, Exception> _invalidSymbols;

        private int _tickCount;
        private int TickCount
        {
            get => _tickCount;
            set
            {
                _tickCount = value;
                this.BeginInvoke((Action)(() => lblTickCount.Text = _tickCount.ToString()));
            }
        }


        private string[] Tickers => txtTickerList.Text.Split('\n').Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();

        private int _errorCount;
        private readonly string _baseFolder = @"E:\Quote\WebData\RealTime\YahooMinute";
        private string _dataFolder;

        public RealTimeForm()
        {
            InitializeComponent();

            _timer.Interval = 2000;
            _timer.Elapsed += _timer_Elapsed;
            lblTickCount.Text = lblStatus.Text = "";
            RefreshUI();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            TickCount = 0;
            _errorCount = 0;

            Debug.Print($"RealTime start: {DateTime.Now.TimeOfDay}");

            var tickers = await Data.RealTime.YahooMinutes.CheckTickers(ShowStatus, Tickers);

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

            Data.RealTime.YahooMinutes.SaveResult(tickers.Item1, _dataFolder);

            _timer.Start();
            RefreshUI();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            RefreshUI();
        }

        private void RefreshUI()
        {
            btnStart.Enabled = !_timer.Enabled;
            btnStop.Enabled = _timer.Enabled;
        }

        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Debug.Print($"Tick: {DateTime.Now.TimeOfDay}");
            TickCount++;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _timer.Elapsed -= _timer_Elapsed;
            _timer.Dispose();
            _frmTickerListParameter?.Dispose();
        }

        private TickerListParameterForm _frmTickerListParameter;
        private async void btnUpdateTickerList_Click(object sender, EventArgs e)
        {
            if (_frmTickerListParameter == null)
            {
                _frmTickerListParameter = new TickerListParameterForm(Tickers);
                _frmTickerListParameter.StartPosition = FormStartPosition.CenterScreen;
            }

            _frmTickerListParameter.ShowDialog();
            if (_frmTickerListParameter.DialogResult == DialogResult.OK)
                txtTickerList.Text = string.Join('\n', _frmTickerListParameter.Tickers);
        }

        private void txtTickerList_TextChanged(object sender, EventArgs e) => lblTickerList.Text = $@"Tickers ({Tickers.Length} items):";
        
        private void ShowStatus(string message) => lblStatus.Text = message;
    }
}
