﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitdb.Utils
{
    public static class ExtensionUtils
    {
        public static DataTable ToDataTable(this SqlCommand cmd)
        {
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                var tb = new DataTable();
                tb.Load(dr);
                return tb;
            }
        }
    }
}
