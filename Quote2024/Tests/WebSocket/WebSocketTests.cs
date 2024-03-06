using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.WebSocket
{
    class WebSocketTests
    {
        private static DateTime _baseUnixDate = new DateTime(1970, 1, 1);

        public static void ParseFile()
        {
            var fn = @"E:\Temp\WebSocket - Copy.txt";
            var lines = File.ReadAllLines(fn);

            // delay 2-3 seconds + 15 minutes (To property)
            var msft_sec_minFrom = TimeSpan.MaxValue;
            var msft_sec_maxFrom = TimeSpan.MinValue;
            var a_sec_minFrom = TimeSpan.MaxValue;
            var a_sec_maxFrom = TimeSpan.MinValue;
            var msft_sec_minTo = TimeSpan.MaxValue;
            var msft_sec_maxTo = TimeSpan.MinValue;
            var a_sec_minTo = TimeSpan.MaxValue;
            var a_sec_maxTo = TimeSpan.MinValue;

            // delay 0.8-1.9 seconds + 15 minutes (To property)
            var msft_minute_minFrom = TimeSpan.MaxValue;
            var msft_minute_maxFrom = TimeSpan.MinValue;
            var a_minute_minFrom = TimeSpan.MaxValue;
            var a_minute_maxFrom = TimeSpan.MinValue;
            var msft_minute_minTo = TimeSpan.MaxValue;
            var msft_minute_maxTo = TimeSpan.MinValue;
            var a_minute_minTo = TimeSpan.MaxValue;
            var a_minute_maxTo = TimeSpan.MinValue;

            foreach (var line in lines)
            {
                var ss = line.Split(',');
                if (ss.Length == 15)
                {
                    var time = DateTime.ParseExact(ss[0], "HH:mm:ss.fff", CultureInfo.InvariantCulture).AddMinutes(-120).TimeOfDay;
                    var from = _baseUnixDate.AddMilliseconds(double.Parse(ss[ss.Length - 2].Split(':')[1])).TimeOfDay;
                    var to = _baseUnixDate.AddMilliseconds(double.Parse(ss[ss.Length - 1].Split(new []{':','}'})[1])).TimeOfDay;
                    var delayFrom = time - from;
                    var delayTo = time - to;
                    var isMsft = ss[2].EndsWith("T\"", StringComparison.Ordinal);
                    if (ss[1].EndsWith("A\"", StringComparison.Ordinal))
                    {
                        if (isMsft)
                        {
                            if (msft_sec_minFrom > delayFrom) msft_sec_minFrom = delayFrom;
                            if (msft_sec_maxFrom < delayFrom) msft_sec_maxFrom = delayFrom;
                            if (msft_sec_minTo > delayTo) msft_sec_minTo = delayTo;
                            if (msft_sec_maxTo < delayTo) msft_sec_maxTo = delayTo;
                        }
                        else
                        {
                            if (a_sec_minFrom > delayFrom) a_sec_minFrom = delayFrom;
                            if (a_sec_maxFrom < delayFrom) a_sec_maxFrom = delayFrom;
                            if (a_sec_minTo > delayTo) a_sec_minTo = delayTo;
                            if (a_sec_maxTo < delayTo) a_sec_maxTo = delayTo;
                        }
                    }
                    else
                    {
                        if (isMsft)
                        {
                            if (msft_minute_minFrom > delayFrom) msft_minute_minFrom = delayFrom;
                            if (msft_minute_maxFrom < delayFrom) msft_minute_maxFrom = delayFrom;
                            if (msft_minute_minTo > delayTo) msft_minute_minTo = delayTo;
                            if (msft_minute_maxTo < delayTo) msft_minute_maxTo = delayTo;
                        }
                        else
                        {
                            if (a_minute_minFrom > delayFrom) a_minute_minFrom = delayFrom;
                            if (a_minute_maxFrom < delayFrom) a_minute_maxFrom = delayFrom;
                            if (a_minute_minTo > delayTo) a_minute_minTo = delayTo;
                            if (a_minute_maxTo < delayTo) a_minute_maxTo = delayTo;
                        }
                    }
                }

            }
        }

        public static async Task DoClientWebSocket()
        {
            using (ClientWebSocket ws = new ClientWebSocket())
            {
                Uri serverUri = new Uri("wss://echo.websocket.org/");

                //Implementation of timeout of 5000 ms
                var source = new CancellationTokenSource();
                source.CancelAfter(15000);

                await ws.ConnectAsync(serverUri, source.Token);
                var iterationNo = 0;
                // restricted to 5 iteration only
                while (ws.State == WebSocketState.Open && iterationNo++ < 5)
                {
                    string msg = "hello0123456789123456789123456789123456789123456789123456789";
                    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg));
                    await ws.SendAsync(bytesToSend, WebSocketMessageType.Text, true, source.Token);
                    
                    //Receive buffer
                    var receiveBuffer = new byte[200];
                    
                    //Multipacket response
                    var offset = 0;
                    var dataPerPacket = 10; //Just for example
                    while (true)
                    {
                        ArraySegment<byte> bytesReceived = new ArraySegment<byte>(receiveBuffer, offset, dataPerPacket);
                        WebSocketReceiveResult result = await ws.ReceiveAsync(bytesReceived, source.Token);

                        //Partial data received
                        // Console.WriteLine("Data:{0}", Encoding.UTF8.GetString(receiveBuffer, offset, result.Count));
                        Debug.Print("Data:{0}", Encoding.UTF8.GetString(receiveBuffer, offset, result.Count));
                        offset += result.Count;
                        if (result.EndOfMessage)
                            break;
                    }

                    Debug.Print("Complete response: {0}", Encoding.UTF8.GetString(receiveBuffer, 0, offset));
                }
            }
        }

    }
}
