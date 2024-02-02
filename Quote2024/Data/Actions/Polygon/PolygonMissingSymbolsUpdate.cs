using Data.Helpers;

namespace Data.Actions.Polygon
{
    class PolygonMissingSymbolsUpdate
    {
        public static void Start()
        {
            Logger.AddMessage($"Update missing symbols from DayPolygon in SymbolsPolygon...");
            DbUtils.RunProcedure("dbQ2024..pUpdateDayPolygon");
            Logger.AddMessage($"!Finished.");
        }
    }
}
