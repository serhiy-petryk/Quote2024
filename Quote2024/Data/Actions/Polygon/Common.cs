using System;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.Polygon
{
    public class PolygonCommon
    {

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
            public long t;
            public float o;
            public float h;
            public float l;
            public float c;
            public double v;
            public float vw;
            public int n;

            /*private DateTime? _date;

            public DateTime Date
            {
                get
                {
                    if (!_date.HasValue)
                        _date = CsUtils.GetEstDateTimeFromUnixSeconds(t / 1000).Date;
                    return _date.Value;
                }
            }
            */
            public DateTime DateTime => CsUtils.GetEstDateTimeFromUnixSeconds(t / 1000);
        }
        #endregion
    }
}
