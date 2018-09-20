using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jdb.Utils
{
    /// <summary>
    /// Assorted extension utilities.
    /// </summary>
    public static class ExtensionUtils
    {
        /// <summary>
        /// Extracts a DataTable from a SqlCommand.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this SqlCommand cmd)
        {
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                var tb = new DataTable();
                tb.Load(dr);
                return tb;
            }
        }

        /// <summary>
        /// Searches a string array for a given value and returns true if found, else false.
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static bool Has(this string[] arr, string val)
        {
            return Array.FindIndex(arr, x => x == val) > -1;
        }
    }
}
