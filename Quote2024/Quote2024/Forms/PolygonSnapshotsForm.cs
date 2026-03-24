using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quote2024.Forms
{
    public partial class PolygonSnapshotsForm : Form
    {
        private readonly System.Timers.Timer _marketSnapshotTimer = new System.Timers.Timer();
        private readonly System.Timers.Timer _topMoversTimer = new System.Timers.Timer();

        private int _marketSnapshotTickCount;
        private int MarketSnapshotTickCount
        {
            get => _marketSnapshotTickCount;
            set
            {
                _marketSnapshotTickCount = value;
                BeginInvoke((Action)UpdateMarketSnapshotTickLabel);
            }
        }

        private int _topMoversTickCount;
        private int TopMoversTickCount
        {
            get => _topMoversTickCount;
            set
            {
                _topMoversTickCount = value;
                BeginInvoke((Action)UpdateTopMoversTickLabel);
            }
        }

        private int _marketSnapshotErrorCount;
        private int MarketSnapshotErrorCount
        {
            get => _marketSnapshotErrorCount;
            set
            {
                _marketSnapshotErrorCount = value;
                this.BeginInvoke((Action)(UpdateMarketSnapshotTickLabel));
            }
        }

        private int _topMoversErrorCount;
        private int TopMoversErrorCount
        {
            get => _topMoversErrorCount;
            set
            {
                _topMoversErrorCount = value;
                this.BeginInvoke((Action)(UpdateTopMoversTickLabel));
            }
        }

        private void UpdateMarketSnapshotTickLabel()
        {
            lblMarketSnapshotTickCount.ForeColor = MarketSnapshotErrorCount == 0 ? Color.Black : Color.DarkRed;
            var text = $@"Ticks: {MarketSnapshotTickCount:N0}";
            if (MarketSnapshotErrorCount != 0)
                text += $@". Errors: {MarketSnapshotErrorCount:N0}";

            lblMarketSnapshotTickCount.Text = text;
        }

        private void UpdateTopMoversTickLabel()
        {
            lblTopMoversTickCount.ForeColor = TopMoversErrorCount == 0 ? Color.Black : Color.DarkRed;
            var text = $@"Ticks: {TopMoversTickCount:N0}";
            if (TopMoversErrorCount != 0)
                text += $@". Errors: {TopMoversErrorCount:N0}";

            lblTopMoversTickCount.Text = text;
        }

        public PolygonSnapshotsForm()
        {
            InitializeComponent();

            _marketSnapshotTimer.Interval = Convert.ToDouble(numMarketSnapshotInterval.Value * 1000);
            _marketSnapshotTimer.Elapsed += _marketSnapshotTimer_Elapsed;
            // lblMarketSnapshotTickCount.Text = lblMarketSnapshotStatus.Text = "";
            lblMarketSnapshotTickCount.Text = "";
            MarketSnapshotRefreshUI();

            _topMoversTimer.Interval = Convert.ToDouble(numTopMoversInterval.Value * 1000);
            _topMoversTimer.Elapsed += _topMoversTimer_Elapsed;
            // lblTopMoversTickCount.Text = lblTopMoversStatus.Text = "";
            lblTopMoversTickCount.Text = "";
            TopMoversRefreshUI();

        }

        private async void _marketSnapshotTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MarketSnapshotTickCount++;
            _marketSnapshotTimer.Interval = Convert.ToDouble(numMarketSnapshotInterval.Value * 1000);
            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonMarketSnapshot.Download);
        }

        private async void _topMoversTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TopMoversTickCount++;
            _topMoversTimer.Interval = Convert.ToDouble(numTopMoversInterval.Value * 1000);
            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonTopMovers.Download);
        }


        private async void btnMarketSnapshotStart_Click(object sender, EventArgs e)
        {
            MarketSnapshotErrorCount = 0;
            _marketSnapshotTimer.Interval = Convert.ToDouble(numMarketSnapshotInterval.Value * 1000);
            btnMarketSnapshotStart.Enabled = false;

            Debug.Print($"PolygonMarketSnapshot started: {DateTime.Now.TimeOfDay}");

            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonMarketSnapshot.Download);
            MarketSnapshotTickCount = 1;

            _marketSnapshotTimer.Start();
            MarketSnapshotRefreshUI();
        }

        private async void btnTopMoversStart_Click(object sender, EventArgs e)
        {
            TopMoversErrorCount = 0;
            _topMoversTimer.Interval = Convert.ToDouble(numTopMoversInterval.Value * 1000);
            btnTopMoversStart.Enabled = false;

            Debug.Print($"PolygonTopMovers started: {DateTime.Now.TimeOfDay}");

            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonTopMovers.Download);
            TopMoversTickCount = 1;

            _topMoversTimer.Start();
            TopMoversRefreshUI();
        }

        private void MarketSnapshotRefreshUI()
        {
            if (lblMarketSnapshotTickCount.InvokeRequired)
                Invoke(new MethodInvoker(MarketSnapshotRefreshUI));
            else
            {
                btnMarketSnapshotStart.Enabled = !_marketSnapshotTimer.Enabled;
                btnMarketSnapshotStop.Enabled = _marketSnapshotTimer.Enabled;
            }
        }

        private void TopMoversRefreshUI()
        {
            if (lblTopMoversTickCount.InvokeRequired)
                Invoke(new MethodInvoker(TopMoversRefreshUI));
            else
            {
                btnTopMoversStart.Enabled = !_topMoversTimer.Enabled;
                btnTopMoversStop.Enabled = _topMoversTimer.Enabled;
            }
        }

        private void btnMarketSnapshotStop_Click(object sender, EventArgs e)
        {
            _marketSnapshotTimer.Stop();
            MarketSnapshotRefreshUI();

        }
        private void btnTopMoversStop_Click(object sender, EventArgs e)
        {
            _topMoversTimer.Stop();
            TopMoversRefreshUI();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _marketSnapshotTimer.Elapsed -= _marketSnapshotTimer_Elapsed;
            _marketSnapshotTimer.Dispose();
            _topMoversTimer.Elapsed -= _topMoversTimer_Elapsed;
            _topMoversTimer.Dispose();
        }
    }
}
