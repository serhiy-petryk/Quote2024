using System;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public class PolygonCommon
    {
        public const string DataFolderDaily = @"E:\Quote\WebData\Daily\Polygon2003\Data\";
        public const string DataFolderMinute = @"E:\Quote\WebData\Minute\Polygon2003\Data\";
        public const string DataFolderSymbols = @"E:\Quote\WebData\Symbols\Polygon2003\Data\";

        public static string GetApiKey() => CsUtils.GetApiKeys("polygon.io")[1];
        public static string GetApiKey2003() => CsUtils.GetApiKeys("polygon.io.2003")[0];

        public static string GetMyTicker(string polygonTicker)
        {
            if (polygonTicker.EndsWith("pw"))
                polygonTicker = polygonTicker.Replace("pw", "^^W");
            else if (polygonTicker.EndsWith("pAw"))
                polygonTicker = polygonTicker.Replace("pAw", "^^AW");
            else if (polygonTicker.EndsWith("pEw"))
                polygonTicker = polygonTicker.Replace("pEw", "^^EW");
            else if (polygonTicker.Contains("p"))
                polygonTicker = polygonTicker.Replace("p", "^");
            else if (polygonTicker.Contains("rw"))
                polygonTicker = polygonTicker.Replace("rw", ".RTW");
            else if (polygonTicker.Contains("r"))
                polygonTicker = polygonTicker.Replace("r", ".RT");
            else if (polygonTicker.Contains("w"))
                polygonTicker = polygonTicker.Replace("w", ".WI");

            if (polygonTicker.Any(char.IsLower))
                throw new Exception($"Check PolygonCommon.GetMyTicker method for '{polygonTicker}' ticker");

            return polygonTicker;
        }

        public static string GetPolygonTicker(string myTicker)
        {
            if (myTicker.EndsWith("^^W"))
                myTicker = myTicker.Replace("^^W", "pw");
            else if (myTicker.EndsWith("^^AW"))
                myTicker = myTicker.Replace("^^AW", "pAw");
            else if (myTicker.EndsWith("^^EW"))
                myTicker = myTicker.Replace("^^EW", "pEw");
            else if (myTicker.Contains("^"))
                myTicker = myTicker.Replace("^", "p");
            else if (myTicker.Contains(".RTW"))
                myTicker = myTicker.Replace(".RTW", "rw");
            else if (myTicker.Contains(".RT"))
                myTicker = myTicker.Replace(".RT", "r");
            else if (myTicker.Contains(".WI"))
                myTicker = myTicker.Replace(".WI", "w");

            return myTicker;
        }

        #region ========  Json classes  ===========
        public class cMinuteRoot
        {
            public string ticker;
            public int queryCount;
            public int resultsCount;
            public int count;
            public bool adjusted;
            public string status;
            public string next_url;
            public string request_id;
            public cMinuteItem[] results;
            public string Symbol => GetMyTicker(ticker);
        }

        public class cMinuteItem
        {
            /*private const long MillisecondsInDay = 24 * 3600 * 1000;
            private static readonly DateTime OffsetDateTime = new DateTime(1970, 1, 1);

            private long _t;
            public DateTime _dateTime;
            public short _date; // day from 1/1/1970
            public short _time; // time offset in minutes

            public long t
            {
                get => _t;
                set
                {
                    _t = value;
                    _dateTime = CsUtils.GetEstDateTimeFromUnixSeconds(value / 1000);
                    var ticks = (_dateTime - OffsetDateTime).Ticks;
                    _date = Convert.ToInt16(ticks / TimeSpan.TicksPerDay);
                    _time = Convert.ToInt16((ticks - _date * TimeSpan.TicksPerDay) / TimeSpan.TicksPerMinute);
                }
            }*/

            public long t; // unix utc milliseconds
            public float o;
            public float h;
            public float l;
            public float c;
            public float v;
            public float vw;
            public int n;

            public DateTime DateTime => CsUtils.GetEstDateTimeFromUnixMilliseconds(t);
            /*public short Date => _date; // day from 1/1/1970
            public short Time => _time; // time offset in minutes
            public DateTime DateTime => _dateTime;*/
        }
        #endregion
    }
}
