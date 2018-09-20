using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jdb.Models
{
    /// <summary>
    /// Core information of a database object.
    /// </summary>
    public class DbObject
    {
        public string ObjectName { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        public string SchemaName { get; set; }
    }
}
