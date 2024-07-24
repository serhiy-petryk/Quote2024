using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Data.Actions.MorningStar
{
    public static class MorningStarCommon
    {
        // Profile: https://www.morningstar.com/api/v2/etfs/xnas/vcit/quote

        public static string[] Sectors = new[]
        {
            "basic-materials-stocks", "communication-services-stocks", "consumer-cyclical-stocks",
            "consumer-defensive-stocks", "energy-stocks", "financial-services-stocks", "healthcare-stocks",
            "industrial-stocks", "real-estate-stocks", "technology-stocks", "utility-stocks"
        };

        public static string[] Exchanges = new[] { "ARCX", "BATS", "XASE", "XNAS", "XNYS" };

        public static string GetSectorName(string sectorId) =>
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sectorId.Replace("-stocks", "").Replace("-", " "));

        /* Need to adjust
         public static string GetWebMorningStarTicker(string myTicker, string exchange)
        {
            if (myTicker.Contains('^') && exchange == "XASE" && @"AULT^D,SACH^A,PCG^D,PCG^E,".IndexOf(myTicker+",", StringComparison.Ordinal) == -1)
            {
                myTicker = myTicker.Replace("^", ".PR");
            }
            return myTicker.Replace("^", "-p");
        }*/

        public static List<string> BadTickers = new List<string>();

        public static string GetMorningStarProfileTicker(string myTicker)
        {
            var morningStarTicker = myTicker;
            if (morningStarTicker.Contains("^"))
            {
                morningStarTicker = morningStarTicker.Replace("^", "-p");
            }
            else if (morningStarTicker.EndsWith(".U"))
            {

            }
            else if (morningStarTicker.EndsWith(".WS"))
            {
                morningStarTicker = "";
            }
            else if (morningStarTicker.EndsWith(".A") || morningStarTicker.EndsWith(".B") || morningStarTicker.EndsWith(".C"))
            {
            }
            else
            {
            }

            return morningStarTicker;
        }

        public static string GetSecondMorningStarProfileTicker(string myTicker) =>
            myTicker.Contains("^") ? myTicker.Replace("^", ".pr") : null;

        public static string GetMyTicker(string morningStarTicker)
            {
            var testSymbol = morningStarTicker;
            if (morningStarTicker == "PSA/pG")
            {
                morningStarTicker = "PSA^G";
                testSymbol = morningStarTicker;
            }
            else if (morningStarTicker == "PRIFPB" || morningStarTicker == "MTLPR" || morningStarTicker == "PPLPRU")
            {
                morningStarTicker = morningStarTicker.Replace("PR", "^");
                testSymbol = morningStarTicker;
            }
            else if (morningStarTicker.Contains('p'))
            {
                morningStarTicker = morningStarTicker.Replace('p', '^');
                testSymbol = morningStarTicker;
            }
            else if (morningStarTicker.EndsWith('w'))
            {
                morningStarTicker = morningStarTicker.Substring(0, morningStarTicker.Length-1) + ".WI";
                testSymbol = morningStarTicker.Substring(0, morningStarTicker.Length - 3);
            }
            else if (morningStarTicker.EndsWith(" U"))
            {
                morningStarTicker = morningStarTicker.Replace(" U", ".U");
                testSymbol = morningStarTicker.Substring(0, morningStarTicker.Length - 2);
            }
            else if (morningStarTicker.EndsWith(".U"))
            {
                testSymbol = morningStarTicker.Substring(0, morningStarTicker.Length - 2);
            }
            else if (morningStarTicker.EndsWith(".A") || morningStarTicker.EndsWith(".B") || morningStarTicker.EndsWith(".C")|| morningStarTicker.EndsWith(".V"))
            {
                testSymbol = morningStarTicker.Substring(0, morningStarTicker.Length - 2);
            }
            else if (morningStarTicker.Contains(".PR"))
            {
                morningStarTicker = morningStarTicker.Replace(".PR", "^");
                testSymbol = morningStarTicker;
            }

            var ok = testSymbol.All(c => (c >= 'A' && c <= 'Z') || c=='^');
            if (!ok)
                BadTickers.Add(morningStarTicker);

            return morningStarTicker;
        }
    }
}
