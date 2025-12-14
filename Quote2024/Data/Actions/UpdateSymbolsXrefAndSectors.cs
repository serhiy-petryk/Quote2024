using Data.Helpers;

namespace Data.Actions
{
    public static class UpdateSymbolsXrefAndSectors
    {
        public static void Start()
        {
            Logger.AddMessage($"Run sql UpdateSymbolsXref procedures");
            DbHelper.RunProcedure("dbQ2024..pUpdateSymbolsXrefAndSectorsOfPolygon");
            DbHelper.RunProcedure("dbQ2024..pUpdateSymbolsXrefOfEoddata");

            Logger.AddMessage($"!Finished.");
        }

    }
}
