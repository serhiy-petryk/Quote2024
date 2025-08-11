using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Data.Helpers;

namespace Data.Actions.Eoddata
{
    public static class EoddataCommon
    {
        private static CookieCollection _eoddataCookies = null;
        private const string url = @"https://www.eoddata.com/Login.aspx";

        public static CookieCollection GetEoddataCookies()
        {
            if (_eoddataCookies == null)
            {
                var o1 = WebClientExt.GetToBytes(url, false, false, null);
                var content = System.Text.Encoding.UTF8.GetString(o1.Item1);
                var ss = content.Split("input type=\"hidden\"");

                var userAndPassword = CsUtils.GetApiKeys("eoddata.com")[0].Split('^');
                var cookies = new NameValueCollection
                {
                    { "ctl00$Menu1$s2$txtSearch", "" },
                    { "ctl00$cph1$Login1$txtEmail", userAndPassword[0] },
                    { "ctl00$cph1$Login1$txtPassword", userAndPassword[1] },
                    { "ctl00$cph1$Login1$chkRemember", "off" },
                    { "ctl00$cph1$ql1$panelWidth", "173" },
                    { "__EVENTTARGET", "ctl00$cph1$Login1$btnLogin" }
                };
                for (var k = 1; k < ss.Length; k++)
                {
                    var s = ss[k].Substring(0, ss[k].IndexOf("/>", StringComparison.InvariantCulture));
                    var k1 = s.IndexOf("name=\"", StringComparison.InvariantCulture);
                    var k2 = s.IndexOf("\"", k1 + 6, StringComparison.InvariantCulture);
                    var name = s.Substring(k1 + 6, k2 - k1 - 6);
                    if (!cookies.AllKeys.Contains(name))
                    {
                        k1 = s.IndexOf("value=\"", StringComparison.InvariantCulture);
                        k2 = s.IndexOf("\"", k1 + 7, StringComparison.InvariantCulture);
                        var value = s.Substring(k1 + 7, k2 - k1 - 7);
                        cookies.Add(name, value);
                    }
                }

                var parameters = string.Join("&",
                    cookies.AllKeys.Select(a => WebUtility.UrlEncode(a) + "=" + WebUtility.UrlEncode(cookies[a])));

                var o2 = WebClientExt.PostToBytes(url, parameters, false, false, "application/x-www-form-urlencoded",
                    o1.Item2);
                _eoddataCookies = o2.Item2;

                if (_eoddataCookies["ASP.NET_SessionId"] == null)
                    throw new Exception("No 'ASP.NET_SessionId' cookie");
                if (_eoddataCookies["EODDataLogin"] == null)
                    throw new Exception("No 'EODDataLogin' cookie");
                if (_eoddataCookies[".ASPXAUTH"] == null)
                    throw new Exception("No '.ASPXAUTH' cookie");
            }

            return _eoddataCookies;
        }

        public static CookieCollection GetEoddataCookies_before_2025_08_08()
        {
            if (_eoddataCookies == null)
            {
                var userAndPassword = Data.Helpers.CsUtils.GetApiKeys("eoddata.com")[0].Split('^');
                var cookies = new NameValueCollection
                {
                    { "ctl00$Menu1$s1$txtSearch", "" },
                    { "ctl00$cph1$lg1$txtEmail", userAndPassword[0] },
                    { "ctl00$cph1$lg1$txtPassword", userAndPassword[1] },
                    { "ctl00$cph1$lg1$chkRemember", "off" },
                    { "ctl00$cph1$lg1$btnLogin", "Login" }
                };

                var urlOld = @"https://www.eoddata.com";
                var o1 = WebClientExt.GetToBytes(urlOld, false, false, null);
                var content = System.Text.Encoding.UTF8.GetString(o1.Item1);
                var ss = content.Split("input type=\"hidden\"");
                for (var k = 1; k < ss.Length; k++)
                {
                    var s = ss[k].Substring(0, ss[k].IndexOf("/>", StringComparison.InvariantCulture));
                    var k1 = s.IndexOf("name=\"", StringComparison.InvariantCulture);
                    var k2 = s.IndexOf("\"", k1 + 6, StringComparison.InvariantCulture);
                    var name = s.Substring(k1 + 6, k2 - k1 - 6);
                    k1 = s.IndexOf("value=\"", StringComparison.InvariantCulture);
                    k2 = s.IndexOf("\"", k1 + 7, StringComparison.InvariantCulture);
                    var value = s.Substring(k1 + 7, k2 - k1 - 7);
                    cookies.Add(name, value);
                }

                var parameters = string.Join("&",
                    cookies.AllKeys.Select(a =>
                        System.Net.WebUtility.UrlEncode(a) + "=" + System.Net.WebUtility.UrlEncode(cookies[a])));

                var o2 = WebClientExt.PostToBytes(urlOld, parameters, false, false, "application/x-www-form-urlencoded",
                    o1.Item2);
                _eoddataCookies = o2.Item2;

                if (_eoddataCookies["ASP.NET_SessionId"] == null)
                    throw new Exception("No 'ASP.NET_SessionId' cookie");
                //if (_eoddataCookies["EODDataAdmin"] == null)
                //  throw new Exception("No 'EODDataAdmin' cookie");
                //if (_eoddataCookies["EODDataLogin"] == null)
                //  throw new Exception("No 'EODDataLogin' cookie");
            }

            return _eoddataCookies;
        }

    }
}
