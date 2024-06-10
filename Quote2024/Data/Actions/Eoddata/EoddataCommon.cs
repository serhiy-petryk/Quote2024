using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Data.Helpers;

namespace Data.Actions.Eoddata
{
    public static class EoddataCommon
    {
        private static CookieCollection EoddataCookies = null;

        public static System.Net.CookieCollection GetEoddataCookies()
        {
            if (EoddataCookies == null)
            {
                var userAndPassword = Data.Helpers.CsUtils.GetApiKeys("eoddata.com")[0].Split('^');
                var cookies = new NameValueCollection
                {
                    { "ctl00$Menu1$s1$txtSearch", "" },
                    { "ctl00$cph1$lg1$txtEmail", userAndPassword[0] },
                    { "ctl00$cph1$lg1$txtPassword", userAndPassword[1] },
                    { "ctl00$cph1$lg1$chkRemember", "on" },
                    { "ctl00$cph1$lg1$btnLogin", "Login" }
                };

                var url = @"https://www.eoddata.com";
                var o = Download.GetToBytes(url, false, false, null);
                var content = System.Text.Encoding.UTF8.GetString(o.Item1);
                var ss = content.Split("input type=\"hidden\"");
                for (var k = 1; k < ss.Length; k++)
                {
                    var s1 = ss[k];
                    var s2 = s1.Substring(0, s1.IndexOf("/>", StringComparison.InvariantCulture));
                    var k1 = s2.IndexOf("id=\"", StringComparison.InvariantCulture);
                    var k2 = s2.IndexOf("\"", k1 + 4, StringComparison.InvariantCulture);
                    var id = s2.Substring(k1 + 4, k2 - k1 - 4);
                    k1 = s2.IndexOf("value=\"", StringComparison.InvariantCulture);
                    k2 = s2.IndexOf("\"", k1 + 7, StringComparison.InvariantCulture);
                    var value = s2.Substring(k1 + 7, k2 - k1 - 7);
                    cookies.Add(id, value);
                }

                var parameters = System.Net.WebUtility.HtmlDecode(string.Join("&",
                    cookies.AllKeys.Select(a =>
                        System.Net.WebUtility.UrlEncode(a) + "=" + System.Net.WebUtility.UrlEncode(cookies[a]))));

                var o2 = Download.PostToBytes(url, parameters, false, false, "application/x-www-form-urlencoded",
                    o.Item2);
                EoddataCookies = o2.Item2;
            }

            return EoddataCookies;
        }

    }
}
