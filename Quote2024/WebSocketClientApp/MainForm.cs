using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Windows.Forms;
using Websocket.Client;

namespace WebSocketClientApp
{
    public partial class MainForm : Form
    {
        private WebsocketClient _client;
        private string _lastSendMessage;
        private List<string> _fileLogBuffer;
        private readonly object _fileLocker = new object();

        public MainForm()
        {
            InitializeComponent();

            txtLogFileName.Text = $@"E:\Temp\WebSocket_{DateTime.Now:yyyyMMddHHmmss}.txt";
            UpdateUI();
        }

        private void InitClient(string host)
        {
            _client?.Dispose();
            _fileLogBuffer = new List<string>();

            _client = new WebsocketClient(new Uri(host));

            _client.ReconnectTimeout = TimeSpan.FromSeconds(3);
            _client.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial)
                    _lastSendMessage = null;
                else if (info.Type == ReconnectionType.Lost || info.Type== ReconnectionType.NoMessageReceived)
                    SendMessage(_lastSendMessage);

                SaveLog($"{DateTime.Now.TimeOfDay} Reconnection happened, type {info.Type}");
            });

            _client.DisconnectionHappened.Subscribe(info =>
            {
                //if (info.Type != DisconnectionType.Exit && info.Type!= DisconnectionType.ByUser)
                //  _client.Reconnect();
                SaveLog($"{DateTime.Now.TimeOfDay} Disconnection happened, type {info.Type}");
                if (info.Type == DisconnectionType.NoMessageReceived && _lastSendMessage != null)
                {
                    Debug.Print("Resend message");
                    SendMessage(_lastSendMessage);
                    SaveLog("Send last message after disconnection happened");
                }
            });

            _client.MessageReceived.Subscribe(msg =>
            {
                var messText = $"{DateTime.Now:HH:mm:ss.fff},{msg}";
                if (cbSaveLogToFile.Checked && !string.IsNullOrWhiteSpace(txtLogFileName.Text))
                {
                    lock (_fileLocker)
                    {
                        _fileLogBuffer.Add(messText);
                        if (_fileLogBuffer.Count > 5000)
                        {
                            var buffer = _fileLogBuffer;
                            _fileLogBuffer = new List<string>();
                            File.AppendAllLinesAsync(txtLogFileName.Text, buffer);
                        }
                    }
                }
                if (cbLogMessages.Checked)
                    SaveLog(messText);
            });

            _client.Start();

            UpdateUI();
        }

        private void SaveLog(string text)
        {
            if (cbLogMessages.Checked)
                BeginInvoke((Action)(() => listBox1.Items.Insert(0, text)));
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(WebServerUri.Text))
                InitClient(WebServerUri.Text);
        }

        private void DisconnectButton_Click(object sender, EventArgs e)  {
            StopSocket();
            UpdateUI();
        }

        private void SendButton_Click(object sender, EventArgs e) => SendMessage(SendMessageText.Text);

        private void SendMessage(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                _client?.Send(message.Trim());
                _lastSendMessage = message;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            StopSocket();
        }

        private void ClearLogButton_Click(object sender, EventArgs e) => listBox1.Items.Clear();

        private void UpdateUI()
        {
            ConnectButton.Enabled = _client == null;
            DisconnectButton.Enabled = _client != null;
            SendButton.Enabled = _client != null;
        }

        private void StopSocket()
        {
            _client?.Stop(WebSocketCloseStatus.Empty, String.Empty);
            _client?.Dispose();
            _client = null;
            if (_fileLogBuffer != null && _fileLogBuffer.Count > 0)
                File.AppendAllLines(txtLogFileName.Text, _fileLogBuffer);
            _fileLogBuffer = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var frm = new MainForm();
            frm.Show();
        }
    }
}
