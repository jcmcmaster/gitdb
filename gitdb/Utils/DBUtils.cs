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
    }
}
