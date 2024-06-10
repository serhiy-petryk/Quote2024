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

        public static System.Net.CookieCollection GetEoddataCookies()
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

                var url = @"https://www.eoddata.com";
                var o = Download.GetToBytes(url, false, false, null);
                var content = System.Text.Encoding.UTF8.GetString(o.Item1);
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

                var parameters = System.Net.WebUtility.HtmlDecode(string.Join("&",
                    cookies.AllKeys.Select(a =>
                        System.Net.WebUtility.UrlEncode(a) + "=" + System.Net.WebUtility.UrlEncode(cookies[a]))));

                var o2 = Download.PostToBytes(url, parameters, false, false, "application/x-www-form-urlencoded",
                    o.Item2);
                _eoddataCookies = o2.Item2;
            }

            return _eoddataCookies;
        }

    }
}
