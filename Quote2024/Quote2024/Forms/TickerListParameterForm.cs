﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data.RealTime;

namespace Quote2024.Forms
{
    public partial class TickerListParameterForm : Form
    {
        public string[] Tickers => txtTickerList.Text.Split('\t').ToArray();
        private Func<string, string> _fnConvertTicker;

        public TickerListParameterForm(string[] tickers, Func<string, string> fnConvertTicker)
        {
            InitializeComponent();

            _fnConvertTicker = fnConvertTicker;
            lblStatus.Text = "";
            txtTickerList.Text = string.Join('\t', tickers);
        }

        private async void btnUpdateList_Click(object sender, EventArgs e)
        {
            btnUpdateList.Enabled = false;
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
                        tickerList.AddRange(new [] { "^DJI", "^GSPC" });
                    tickerList.AddRange(tickers.OrderBy(a => a));
                    txtTickerList.Text = string.Join('\t', tickerList);
                    btnUpdateList.Enabled = true;
                }));
            }));

        }

        private void txtTickerList_TextChanged(object sender, EventArgs e)
        {
            var itemCount = txtTickerList.Text.Split('\t').Count(a => !string.IsNullOrWhiteSpace(a));
            lblTickerList.Text = $@"Tickers: ({itemCount} items)";
        }

        private void ShowStatus(string message) => lblStatus.Text = message;
    }
}
