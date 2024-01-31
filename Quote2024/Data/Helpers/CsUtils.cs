using System;
using System.Collections.Generic;
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

        private static readonly DateTime OffsetWebTime = new DateTime(1970, 1, 1);
        private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        // private static readonly TimeZoneInfo EstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        public static DateTime GetEstDateTimeFromUnixSeconds(long unixTimeInSeconds)
        {
            var aa2 = DateTimeOffset.FromUnixTimeSeconds(unixTimeInSeconds);
            var aa3 = aa2 + EstTimeZone.GetUtcOffset(aa2);
            return aa3.DateTime;
        }

        public static long GetUnixSecondsFromEtcDateTime(DateTime dt)
        {
            var seconds = (dt - OffsetWebTime).TotalSeconds + (EstTimeZone.GetUtcOffset(dt)).TotalSeconds;
            var a1 = (new DateTimeOffset(dt, EstTimeZone.GetUtcOffset(dt))).ToUnixTimeSeconds();
            return Convert.ToInt64(seconds);
        }

        public static long GetUnixSecondsFromEstDateTime(DateTime dt)
        {
            // var seconds = (dt - OffsetWebTime).TotalSeconds + (EstTimeZone.GetUtcOffset(dt)).TotalSeconds;
            var a1 = (new DateTimeOffset(dt, EstTimeZone.GetUtcOffset(dt))).ToUnixTimeSeconds();
            return Convert.ToInt64(a1);
        }

        public static Tuple<DateTime, string, string> GetTimeStamp(int hourOffset = -9) => Tuple.Create(
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
