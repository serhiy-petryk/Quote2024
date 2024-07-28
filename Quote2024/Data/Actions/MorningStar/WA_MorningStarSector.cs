using System;
using System.Collections.Generic;
using System.Linq;
using Data.Helpers;

namespace Data.Actions.MorningStar
{
    public static class WA_MorningStarSector
    {
        public static void Start()
        {
            Logger.AddMessage($"Started");

            var data = new List<DbItem>();
            WA_MorningStarScreenerLoader.ParseHtmlFiles(data);
            WA_MorningStarProfile.ParseHtmlFiles(data);

            var dbData = new List<DbItem>();
            foreach (var item in data.OrderBy(a => a.MsSymbol).ThenByDescending(a => a.LastUpdated))
            {
                // Adjust name property
                if (item.Name.EndsWith("- Stock Quote", StringComparison.InvariantCulture))
                    item.Name = item.Name.Substring(0, item.Name.Length - 13).Trim();
                else if (item.Name.EndsWith(" Stock Quote", StringComparison.InvariantCulture))
                    item.Name = item.Name.Substring(0, item.Name.Length - 12).Trim();

                var lastDbItem = dbData.Count == 0 ? null : dbData[dbData.Count - 1];

                if (lastDbItem != null && item.MsSymbol == lastDbItem.MsSymbol && lastDbItem.Date == item.Date && lastDbItem.Sector != item.Sector)
                    throw new Exception("Check sector");
                
                if (lastDbItem == null || item.MsSymbol != lastDbItem.MsSymbol || CsUtils.MyCalculateSimilarity(item.Name, lastDbItem.Name) < 0.7 || item.Sector != lastDbItem.Sector)
                {
                    var newItem = new DbItem(item.MsSymbol, item.Name, item.Sector, item.LastUpdated);
                    dbData.Add(newItem);
                }
                else
                {
                    lastDbItem.Date = item.Date;
                }
            }

            DbHelper.ClearAndSaveToDbTable(dbData, "dbQ2024..SectorMorningStar_WA", "PolygonSymbol", "Date", "To",
                "Name", "Sector", "LastUpdated", "MsSymbol");

            Logger.AddMessage($"Finished!");

        }

        #region =========  SubClasses  ===========
        public class DbItem
        {
            public string PolygonSymbol => MorningStarCommon.GetMyTicker(MsSymbol);
            public string MsSymbol;
            public string Name;
            public string Sector;
            public DateTime Date;
            public DateTime To;
            public DateTime LastUpdated;
            public string source;

            public DbItem(string symbol, string name, string sector, DateTime timestamp)
            {
                MsSymbol = symbol;
                Name = name;
                Sector = sector;
                LastUpdated = timestamp;
                Date = LastUpdated.Date;
                To = LastUpdated.Date;
            }
        }
        #endregion
    }
}