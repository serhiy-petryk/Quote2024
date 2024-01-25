using System;

namespace Data.Actions.Eoddata
{
    public static class EoddataCommon
    {
        public static Func<System.Net.CookieContainer> FnGetEoddataCookies;

        public static System.Net.CookieContainer GetEoddataCookies()
        {
            var cookieContainer = FnGetEoddataCookies();

            if (cookieContainer.Count == 0)
                throw new Exception("Check login to www.eoddata.com in Chrome browser");

            return cookieContainer;
        }
    }
}
