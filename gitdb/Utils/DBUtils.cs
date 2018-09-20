using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using gitdb.Models;
using Microsoft.Win32;

namespace gitdb.Utils
{
    public static class DbUtils
    {
        public static List<string> GetSqlServers()
        {
            List<string> result = new List<string>();

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (string instanceName in instanceKey.GetValueNames())
                    {
                        result.Add((Environment.MachineName + @"\" + instanceName).Replace("\\MSSQLSERVER", ""));
                    }
                }
            }

            return result;
        }

        public static SqlCommand GetCommand(string serverName, string dataBaseName)
        {
            var conn = new SqlConnection("Data Source=" + serverName + "; Initial Catalog=" + dataBaseName + "; Integrated Security=true;");
            conn.Open();
            return new SqlCommand(string.Empty, conn);
        }

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
    }
}
