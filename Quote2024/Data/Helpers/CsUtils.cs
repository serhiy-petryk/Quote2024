using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Data.Helpers
{
    public static class CsUtils
    {
        public static string CurrentArchitecture = IntPtr.Size == 4 ? "x86" : "x64";

        public static bool IsInDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public static string[] GetApiKeys(string dataProvider)
        {
            const string filename = @"E:\Quote\WebData\ApiKeys.txt";
            var keys = File.ReadAllLines(filename)
                .Where(a => a.StartsWith(dataProvider, StringComparison.InvariantCultureIgnoreCase))
                .Select(a => a.Split('\t')[1].Trim()).ToArray();
            return keys;
        }

        private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
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

        /// <summary>
        /// For date id in downloaded filenames
        /// </summary>
        /// <param name="hourOffset"></param>
        /// <returns></returns>
        public static (DateTime, string, string) GetTimeStamp(int hourOffset = -9) => (
            DateTime.Now.AddHours(hourOffset), DateTime.Now.AddHours(hourOffset).ToString("yyyyMMdd"),
            DateTime.Now.AddHours(0).ToString("yyyyMMddHHmmss"));

        public static int GetFileSizeInKB(string filename) => Convert.ToInt32(new FileInfo(filename).Length / 1024.0);

        public static long MemoryUsedInBytes
        {
            get
            {
                // clear memory
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return GC.GetTotalMemory(true);
            }
        }

    }
}
