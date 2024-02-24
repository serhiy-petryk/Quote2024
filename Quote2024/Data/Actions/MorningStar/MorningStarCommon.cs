using System.Globalization;

namespace Data.Actions.MorningStar
{
    public static class MorningStarCommon
    {
        public static string[] Sectors = new[]
        {
            "basic-materials-stocks", "communication-services-stocks", "consumer-cyclical-stocks",
            "consumer-defensive-stocks", "energy-stocks", "financial-services-stocks", "healthcare-stocks",
            "industrial-stocks", "real-estate-stocks", "technology-stocks", "utility-stocks"
        };

        public static string[] Exchanges = new[] { "ARCX", "BATS", "XASE", "XNAS", "XNYS" };

        public static string GetSectorName(string sectorId) =>
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sectorId.Replace("-stocks", "").Replace("-", " "));

        public static string GetMorningStarTicker(string myTicker) => myTicker.Replace("^", "-P");
        public static string GetMyTicker(string morningStarTicker) => morningStarTicker.Replace("p", "^");
    }
}
