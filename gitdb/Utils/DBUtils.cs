using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace gitdb.Utils
{
    public static class DbUtils
    {
        public static SqlCommand GetCommand(string serverName)
        {
            SqlConnection conn = new SqlConnection("Data Source=" + serverName + "; Integrated Security=true;");
            return new SqlCommand(string.Empty, conn);
        }

        public static SqlCommand GetCommand(string serverName, string dataBaseName)
        {
            SqlConnection conn = new SqlConnection("Data Source=" + serverName + "; Initial Catalog=" + dataBaseName + "; Integrated Security=true;");
            return new SqlCommand(string.Empty, conn);
        }

        public static List<string> GetServers()
        {
            List<string> result = new List<string>();

            RegistryView registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
            {
                RegistryKey instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);
                if (instanceKey != null)
                {
                    foreach (var instanceName in instanceKey.GetValueNames())
                    {
                        result.Add((Environment.MachineName + @"\" + instanceName).Replace("\\MSSQLSERVER", ""));
                    }
                }
            }

            return result;
        }

        public static List<string> GetDataBases(string serverName)
        {
            List<string> result = new List<string>();
            DataTable dtData = new DataTable();

            using (SqlCommand cmd = GetCommand(serverName))
            {
                cmd.CommandText = "SELECT name from sys.Databases";
                new SqlDataAdapter(cmd).Fill(dtData);
            }

            foreach (DataRow dr in dtData.Rows)
            {
                result.Add(dr["name"].ToString());
            }

            return result;
        }

        public static List<string> GetSchemas(string serverName, string dbName)
        {
            List<string> result = new List<string>();
            DataTable dtData = new DataTable();

            using (SqlCommand cmd = GetCommand(serverName, dbName))
            {
                cmd.CommandText = "SELECT name from sys.Schemas";
                new SqlDataAdapter(cmd).Fill(dtData);
            }

            foreach (DataRow dr in dtData.Rows)
            {
                result.Add(dr["name"].ToString());
            }

            return result;
        }
    }
}
