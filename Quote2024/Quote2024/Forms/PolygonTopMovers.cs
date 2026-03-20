using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quote2024.Forms
{
    public partial class PolygonTopMovers : Form
    {
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();

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

            lblTickCount.Text = text;
        }

        public PolygonTopMovers()
        {
            InitializeComponent();

            _timer.Interval = Convert.ToDouble(numInterval.Value * 1000);
            _timer.Elapsed += _timer_Elapsed;
            lblTickCount.Text = lblStatus.Text = "";
            RefreshUI();
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            TickCount = 0;
            ErrorCount = 0;
            _timer.Interval = Convert.ToDouble(numInterval.Value * 1000);
            btnStart.Enabled = false;

            Debug.Print($"PolygonTopMoversForm started: {DateTime.Now.TimeOfDay}");

            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonTopMovers.Start);

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
            _timer.Interval = Convert.ToDouble(numInterval.Value * 1000);
            await Task.Factory.StartNew(Data.Actions.Polygon.PolygonTopMovers.Start);
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

        private void ShowStatus(string message) => lblStatus.Text = message;
    }
}
