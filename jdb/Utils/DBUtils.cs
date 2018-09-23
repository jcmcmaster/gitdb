using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using jdb.Models;
using Microsoft.Win32;

namespace jdb.Utils
{
    /// <summary>
    /// Database utilities.
    /// </summary>
    public static class DbUtils
    {
        /// <summary>
        /// Looks for SQL Server instances running on the machine and returns a list of their names.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSqlServers()
        {
            var result = new List<string>();

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);

                if (instanceKey == null) return result;

                result
                    .AddRange(instanceKey.GetValueNames()
                    .Select(instanceName => (Environment.MachineName + @"\" + instanceName)
                            .Replace("\\MSSQLSERVER", "")));
            }

            return result;
        }

        /// <summary>
        /// Gets a SQL command with an open connection to the chosen server and database.
        /// </summary>
        /// <param name="serverName"></param>
        /// <param name="dataBaseName"></param>
        /// <returns></returns>
        public static SqlCommand GetCommand(string serverName, string dataBaseName)
        {
            var conn = new SqlConnection("Data Source=" + serverName + "; Initial Catalog=" + dataBaseName + "; Integrated Security=true;");
            conn.Open();
            return new SqlCommand(string.Empty, conn);
        }

        /// <summary>
        /// Gets a DbObject representation of the given selection.
        /// </summary>
        /// <param name="serverChoice"></param>
        /// <param name="dbChoice"></param>
        /// <param name="schemaChoice"></param>
        /// <param name="objectChoice"></param>
        /// <returns></returns>
        public static DbObject GetDbObject(string serverChoice, string dbChoice, string schemaChoice, string objectChoice)
        {
            using (SqlCommand cmd = GetCommand(serverChoice, dbChoice))
            {
                cmd.CommandText = @"
                    SELECT ao.[name] ObjectName
                         , ao.[object_id] ObjectId
                    	 , ao.[type_desc] ObjectType
                    	 , s.[name] SchemaName
                    FROM sys.all_objects ao
                    LEFT JOIN sys.schemas s
                      ON s.schema_id = ao.schema_id
                    WHERE ao.[name] = @objectName
                      AND s.[name] = @schemaName
                ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@objectName", SqlDbType.VarChar)
                {
                    Value = objectChoice
                });
                cmd.Parameters.Add(new SqlParameter("@schemaName", SqlDbType.VarChar)
                {
                    Value = schemaChoice
                });

                DataTable dtData = cmd.ToDataTable();

                if (dtData.Rows.Count <= 0) return null;

                DataRow dr = dtData.Rows[0];

                return new DbObject
                {
                    ObjectType = dr["ObjectType"].ToString(),
                    ObjectId = Convert.ToInt32(dr["ObjectId"]),
                    ObjectName = dr["ObjectName"].ToString(),
                    SchemaName = dr["SchemaName"].ToString()
                };

            }
        }

        public static string[] GetAllDbObjectNames(string serverChoice, string dbChoice, string input)
        {
            using (SqlCommand cmd = GetCommand(serverChoice, dbChoice))
            {
                cmd.CommandText = @"
                    SELECT s.[name] + '.' + ao.[name] ObjName
                    FROM sys.all_objects ao
                    LEFT JOIN sys.schemas s
                    ON s.schema_id = ao.schema_id
                    WHERE s.[name] + '.' + ao.[name] LIKE @input + '%';
                ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@input", SqlDbType.VarChar) { Value = input });

                DataTable dtData = cmd.ToDataTable();

                if (dtData.Rows.Count <= 0) return new string[] { };

                var schemaObjects = new List<string>();

                foreach (DataRow dr in dtData.Rows)
                {
                    schemaObjects.Add(dr["ObjName"].ToString());
                }

                return schemaObjects.ToArray();
            }
        }

        public static HashSet<string> GetAllDbObjectHashSet(string serverChoice, string dbChoice, string input)
        {
            using (SqlCommand cmd = GetCommand(serverChoice, dbChoice))
            {
                var results = new HashSet<string>();

                cmd.CommandText = @"
                    SELECT s.[name] + '.' + ao.[name] ObjName
                    FROM sys.all_objects ao
                    LEFT JOIN sys.schemas s
                    ON s.schema_id = ao.schema_id;
                ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(new SqlParameter("@input", SqlDbType.VarChar) { Value = input });

                DataTable dtData = cmd.ToDataTable();

                if (dtData.Rows.Count <= 0) return results;

                foreach (DataRow dr in dtData.Rows)
                {
                    results.Add(dr["ObjName"].ToString());
                }

                return results;
            }
        }
    }
}
