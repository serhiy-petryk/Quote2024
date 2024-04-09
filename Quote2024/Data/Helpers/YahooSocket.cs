using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using Websocket.Client;

namespace Data.Helpers
{
    public class YahooSocket: IDisposable
    {
        private const string Host = @"wss://streamer.finance.yahoo.com";

        private readonly string _logFileName;
        private readonly string _sendMessage;
        private readonly int _flushCount;
        private readonly object _fileLocker = new object();
        private readonly Action<string> _onDisconnect;

        private List<string> _fileLogBuffer;

        public WebsocketClient SocketClient;
        public readonly string Id;
        public int MessageCount;
        public List<string> Log;

        public YahooSocket(string id, string logFileName, IList<string> tickers, Action<string> onDisconnect)
        {
            Id = id;
            _logFileName = logFileName;
            if (tickers != null && tickers.Count > 0)
            {
                _sendMessage = "{\"subscribe\":[\"" + string.Join("\",\"", tickers) + "\"]}";
                _flushCount = Math.Min(5000, tickers.Count * 120);
            }
            _onDisconnect = onDisconnect;
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(_sendMessage)) return;

            SocketClient?.Dispose();
            lock (_fileLocker)
            {
                _fileLogBuffer = new List<string>();
            }

            SocketClient = new WebsocketClient(new Uri(Host));

            MessageCount = 0;
            Log = new List<string>();
            // _client.ReconnectTimeout = TimeSpan.FromSeconds(300);
            SocketClient.ReconnectTimeout = null;
            SocketClient.ReconnectionHappened.Subscribe(info =>
            {
                if (info.Type == ReconnectionType.Initial || info.Type == ReconnectionType.Lost || info.Type == ReconnectionType.NoMessageReceived)
                    SocketClient?.Send(_sendMessage);

                SaveLog($"{DateTime.Now.TimeOfDay} Reconnection happened, type {info.Type}");
            });

            SocketClient.DisconnectionHappened.Subscribe(info =>
            {
                var message = $"{DateTime.Now.TimeOfDay} Disconnection happened, type {info.Type}";
                SaveLog(message);
                if (SocketClient != null)
                    this?._onDisconnect(Path.GetFileNameWithoutExtension(_logFileName) + " " + message);
            });

            SocketClient.MessageReceived.Subscribe(msg =>
            {
                MessageCount++;
                var messText = $"{DateTime.Now:HH:mm:ss.fff},{msg}";
                lock (_fileLocker)
                {
                    _fileLogBuffer.Add(messText);
                    if (_fileLogBuffer.Count >= _flushCount)
                    {
                        var buffer = _fileLogBuffer;
                        _fileLogBuffer = new List<string>();
                        File.AppendAllLinesAsync(_logFileName, buffer);
                    }
                }
                // SaveLog(messText);
            });

            SocketClient.Start();
        }

        public void Flush()
        {
            lock (_fileLocker)
            {
                if (_fileLogBuffer!= null && _fileLogBuffer.Count > 0)
                {
                    var buffer = _fileLogBuffer;
                    _fileLogBuffer = new List<string>();
                    File.AppendAllLinesAsync(_logFileName, buffer);
                }
            }
        }

        private void SaveLog(string message)
        {
            Log.Add(message);
            // Debug.Print($"YSocketLog: {Path.GetFileNameWithoutExtension(_logFileName)}\t{message}");
        }

        public void Dispose()
        {
            SocketClient?.Stop(WebSocketCloseStatus.Empty, String.Empty);
            SocketClient?.Dispose();
            SocketClient = null;

            Flush();
            _fileLogBuffer = null;
        }
    }
}
