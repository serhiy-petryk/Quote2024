using Data.Helpers;

namespace Data.Actions.Polygon
{
    class PolygonSynchronizeDayAndSymbols
    {
        public static void Start()
        {
            Logger.AddMessage($"Synchronize Polygon Day and Symbols data");
            DbUtils.RunProcedure("dbQ2024..pSynchronizePolygonDayAndSymbols");
            Logger.AddMessage($"!Finished.");
        }
    }
}
