using System;

namespace Data.Helpers
{
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        /// <summary>
        /// For date id in downloaded filenames
        /// </summary>
        /// <param name="hourOffset"></param>
        /// <returns></returns>
        public static (DateTime, string, string) GetTimeStamp(int hourOffset = -9) => (
            DateTime.Now.AddHours(hourOffset), DateTime.Now.AddHours(hourOffset).ToString("yyyyMMdd"),
            DateTime.Now.AddHours(0).ToString("yyyyMMddHHmmss"));

        /// <summary>
        /// Get NewYork time from Unix UTC milliseconds
        /// </summary>
        /// <param name="unixTimeInSeconds"></param>
        /// <returns></returns>
        public static DateTime GetEstDateTimeFromUnixMilliseconds(long unixTimeInSeconds)
        {
            var aa2 = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeInSeconds);
            return (aa2 + EstTimeZone.GetUtcOffset(aa2)).DateTime;
        }

        /// <summary>
        /// Get Unix UTC milliseconds from NewYork time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static long GetUnixMillisecondsFromEstDateTime(DateTime dt) =>
            Convert.ToInt64((new DateTimeOffset(dt, EstTimeZone.GetUtcOffset(dt))).ToUnixTimeMilliseconds());

        public const long UnixMillisecondsForOneDay = 86400000L;


    }
}
