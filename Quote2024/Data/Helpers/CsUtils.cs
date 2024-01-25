using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Helpers
{
    public static class CsUtils
    {
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
    }
}
