using System.Collections.Generic;
using System.Data;
using FastMember;
using Microsoft.Data.SqlClient;

namespace Data.Helpers
{
    public static class DbUtils
    {
        public static void ClearAndSaveToDbTable<T>(IEnumerable<T> items, string destinationTable, params string[] properties) =>
            SaveToDbTable(items, destinationTable, true, properties);

        public static void SaveToDbTable<T>(IEnumerable<T> items, string destinationTable, params string[] properties) =>
            SaveToDbTable(items, destinationTable, false, properties);

        public static void SaveToDbTable<T>(IEnumerable<T> items, string destinationTable, bool clearTable, params string[] properties)
        {
            using (var reader = ObjectReader.Create(items, properties))
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            {
                if (conn.State != ConnectionState.Open) conn.Open();
                if (clearTable)
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandTimeout = 150;
                        cmd.CommandText = $"truncate table {destinationTable}";
                        cmd.ExecuteNonQuery();
                    }

                using (var bcp = new SqlBulkCopy(conn))
                {
                    bcp.BulkCopyTimeout = 300;
                    bcp.DestinationTableName = destinationTable;
                    bcp.WriteToServer(reader);
                }
            }
        }

        public static void RunProcedure(string procedureName, Dictionary<string, object> paramaters = null)
        {
            using (var conn = new SqlConnection(Settings.DbConnectionString))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = procedureName;
                cmd.CommandTimeout = 15 * 60;
                cmd.CommandType = CommandType.StoredProcedure;
                if (paramaters != null)
                {
                    foreach (var kvp in paramaters)
                        cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }
                cmd.ExecuteNonQuery();
            }
        }
    }
}
