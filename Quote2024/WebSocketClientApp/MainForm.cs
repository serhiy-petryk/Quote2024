using System;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Forms;
using Websocket.Client;

namespace WebSocketClientApp
{
    public partial class MainForm : Form
    {
        private Websocket.Client.WebsocketClient _client;

        public MainForm()
        {
            InitializeComponent();

            txtLogFileName.Text = $@"E:\Temp\WebSocket_{DateTime.Now:yyyyMMddHHmmss}.txt";
            UpdateUI();
        }

        private void InitClient(string host)
        {
            _client?.Dispose();

            _client = new WebsocketClient(new Uri(host));

            // _client.ReconnectTimeout = TimeSpan.FromSeconds(500)
            _client.ReconnectionHappened.Subscribe(info => SaveLog("Reconnection happened, type: " + info.Type));
            _client.DisconnectionHappened.Subscribe(info => SaveLog("Disconnection happened, type: " + info.Type));
            _client.MessageReceived.Subscribe(msg =>
            {
                var messText = $"{DateTime.Now:HH:mm:ss.fff},{msg}";
                if (cbSaveLogToFile.Checked && !string.IsNullOrEmpty(txtLogFileName.Text))
                    File.AppendAllText(txtLogFileName.Text, messText + Environment.NewLine);
                if (cbLogMessages.Checked)
                    SaveLog(messText);
            });

            _client.Start();

            UpdateUI();
        }

        private void SaveLog(string text)
        {
            if (cbLogMessages.Checked)
                BeginInvoke((Action)(() => listBox1.Items.Add(text)));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(WebServerUri.Text))
                InitClient(WebServerUri.Text);
        }

        private void DisconnectButton_Click(object sender, EventArgs e)  {
            _client?.Stop(WebSocketCloseStatus.Empty, String.Empty);
            _client.Dispose();
            _client = null;

            UpdateUI();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SendMessageText.Text))
                _client?.Send(SendMessageText.Text.Trim());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _client?.Dispose();
        }

        private void ClearLogButton_Click(object sender, EventArgs e) => listBox1.Items.Clear();

        private void UpdateUI()
        {
            ConnectButton.Enabled = _client == null;
            DisconnectButton.Enabled = _client != null;
            SendButton.Enabled = _client != null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new MainForm();
            frm.Show();
        }
    }
}
