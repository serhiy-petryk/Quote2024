using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Helpers;

namespace Data.Actions
{
    public static class UpdateSymbolsXrefAndSectors
    {
        public static void Start()
        {
            Logger.AddMessage($"Run sql procedure: pUpdateSymbolsXrefAndSectors");
            DbHelper.RunProcedure("dbQ2024..pUpdateSymbolsXrefAndSectors");

            Logger.AddMessage($"!Finished.");
        }

    }
}
