using System;
using System.Net;
using System.Net.Sockets;

namespace Data.Helpers
{
    public static class TimeHelper
    {
        // New York time zone
        public static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        /// <summary>
        /// For date id in downloaded filenames
        /// </summary>
        /// <returns></returns>
        public static (DateTime, string, string) GetTimeStamp()
        {
            var newYorkDateTime = GetCurrentEstDateTime();
            return (newYorkDateTime, newYorkDateTime.ToString("yyyyMMdd"), DateTime.Now.ToString("yyyyMMddHHmmss"));
        }

        /// <summary>
        /// Get NewYork time from Unix UTC milliseconds
        /// </summary>
        /// <param name="unixTimeInMilliseconds"></param>
        /// <returns></returns>
        public static DateTime GetEstDateTimeFromUnixMilliseconds(long unixTimeInMilliseconds)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTimeOffset.FromUnixTimeMilliseconds(unixTimeInMilliseconds).DateTime, EstTimeZone);
        }

        /// <summary>
        /// Get Unix UTC milliseconds from NewYork time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetUnixMillisecondsFromEstDateTime(DateTime dt) =>
            Convert.ToInt64((new DateTimeOffset(dt.Ticks, EstTimeZone.GetUtcOffset(dt))).ToUnixTimeMilliseconds());

        public static DateTime GetCurrentEstDateTime()
        {
            var msecs = (new DateTimeOffset(DateTime.Now)).ToUnixTimeMilliseconds();
            var newYorkDateTime = GetEstDateTimeFromUnixMilliseconds(msecs);
            return newYorkDateTime;
        }

        public const long UnixMillisecondsForOneDay = 86400000L;


        #region =========  GetNetworkTime ============
        // see https://stackoverflow.com/questions/1193955/how-to-query-an-ntp-server-using-c
        // doesn't work: var response = await client.GetStringAsync("https://worldtimeapi.org/api/timezone/America/New_York");
        // https://time.is/ - show difference between Internet time and PC time
        // GetNetworkTimeFromNtpServer is more faster than GetNetworkTime
        public static DateTime GetNetworkTimeFromNtpServer()
        {
            //default Windows time server
            const string ntpServer = "time.windows.com";

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var utcDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return utcDateTime;
            // return utcDateTime.ToLocalTime();

            // stackoverflow.com/a/3294698/162671
            static uint SwapEndianness(ulong x)
            {
                return (uint)(((x & 0x000000ff) << 24) +
                              ((x & 0x0000ff00) << 8) +
                              ((x & 0x00ff0000) >> 8) +
                              ((x & 0xff000000) >> 24));
            }
        }

        public static DateTime GetNetworkTime()
        {
            var url = "http://worldclockapi.com/api/json/utc/now";
            var o = WebClientExt.GetToBytes(url, false, false);
            if (o.Item3 != null)
                throw new Exception($"GetNetworkTime: Error while download from {url}. Error message: {o.Item3.Message}");

            var s = System.Text.Encoding.UTF8.GetString(o.Item1);
            var oResponse = SpanJson.JsonSerializer.Generic.Utf8.Deserialize<worldclockapi_json>(o.Item1);

            var utcDateTime = DateTime.FromFileTimeUtc(oResponse.currentFileTime);
            return utcDateTime;
        }

        public class worldclockapi_json
        {
            public long currentFileTime;
        }
        #endregion
    }

}
