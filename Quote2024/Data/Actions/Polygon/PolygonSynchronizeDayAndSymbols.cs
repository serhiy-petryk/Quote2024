using Data.Helpers;

namespace Data.Actions.Polygon
{
    class PolygonSynchronizeDayAndSymbols
    {
        public static void Start()
        {
            Logger.AddMessage($"Synchronize Polygon Day and Symbols data");
            DbHelper.RunProcedure("dbQ2024..pSynchronizePolygonDayAndSymbols");

            Logger.AddMessage($"Update DayPolygonSummary data");
            DbHelper.RunProcedure("dbQ2024..pUpdateDayPolygonSummary");
            
            Logger.AddMessage($"!Finished.");
        }
    }
}
