using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SevenZip;

namespace Data
{
    public static class Settings
    {
        internal const string DbConnectionString = "Data Source=localhost;Initial Catalog=dbQuote2022;Integrated Security=True;Connect Timeout=150;Encrypt=false";

        public static void SetZipLibrary() => SevenZipBase.SetLibraryPath(@"7z1900\x64-dll\7z.dll");

        // private const string BaseFolder = @"E:\Quote\";

        // see https://www.quantconnect.com/docs/v2/writing-algorithms/securities/asset-classes/us-equity/market-hours
        private const string ShortenedDaysList = "1999-11-26,2000-07-03,2000-11-24,2001-07-03,2001-11-23,2001-12-24,2002-07-05,2002-11-29,2002-12-24," +
                                               "2003-07-03,2003-11-28,2003-12-24,2003-12-26,2004-11-26,2005-11-25,2006-07-03,2006-11-24,2007-07-03," +
                                               "2007-11-23,2007-12-24,2008-07-03,2008-11-28,2008-12-24,2009-11-27,2009-12-24,2010-11-26,2011-11-25," +
                                               "2012-07-03,2012-11-23,2012-12-24,2013-07-03,2013-11-29,2013-12-24,2014-07-03,2014-11-28,2014-12-24," +
                                               "2015-11-27,2015-12-24,2016-11-25,2017-07-03,2017-11-24,2017-12-24,2018-07-03,2018-11-23,2018-12-24," +
                                               "2019-07-03,2019-11-29,2019-12-24,2020-11-27,2020-12-24,2021-11-26,2022-11-25,2023-07-03,2023-11-24," +
                                               "2024-07-03,2024-11-29,2024-12-24,2025-07-03,2025-11-28,2025-12-24,2026-11-27,2026-12-24";

        public static Dictionary<DateTime, object> ShortenedDays = ShortenedDaysList.Split(',')
            .Select(a => DateTime.ParseExact(a, "yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToDictionary(a => a, a => (object)null);

        private static readonly TimeSpan MarketEndCommon = new TimeSpan(16, 0, 0);
        private static readonly TimeSpan MarketEndOfShortenedDay = new TimeSpan(13, 0, 0);

        public static TimeSpan MarketStart = new TimeSpan(9, 30, 0); // time=marketStart: inside market
        public static TimeSpan MarketStartIn = new TimeSpan(9, 54, 0);
        public static bool IsInMarketTime(DateTime date) => date.TimeOfDay >= MarketStart && date.TimeOfDay < GetMarketEndTime(date);
        public static TimeSpan GetMarketEndTime(DateTime date) => ShortenedDays.ContainsKey(date.Date) ? MarketEndOfShortenedDay : MarketEndCommon;

    }
}
