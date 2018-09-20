using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace jdb.Models
{
    public class DbObject
    {
        public string ObjectName { get; set; }
        public int ObjectId { get; set; }
        public string ObjectType { get; set; }
        public string SchemaName { get; set; }
    }
}
