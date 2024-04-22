using System;

namespace Tests.TradesCompressor
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
    }
}
