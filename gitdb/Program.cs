using gitdb.Utils;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace gitdb
{
    class Program
    {
        public static string ServerChoice { get; set; }
        public static string DbChoice { get; set; }
        public static string SchemaChoice { get; set; }

        public static readonly List<string> Repos = new List<string>()
        {
            "connectqa",
            "coordinateqa"
        };        

        static void Main(string[] args)
        {
            List<string> serverList = DbUtils.GetServers();
            if (serverList.Count > 1)
            {
                ServerChoice = GetChoice("Server", serverList);
            }
            else
            {
                ServerChoice = serverList.First();
            }

            List<string> dbList = DbUtils.GetDataBases(ServerChoice);
            if (dbList.Count > 1)
            {
                DbChoice = GetChoice("Database", dbList);
            }
            else
            {
                DbChoice = dbList.First();
            }

            List<string> schemaList = DbUtils.GetSchemas(ServerChoice, DbChoice);
            if (schemaList.Count > 1)
            {
                SchemaChoice = GetChoice("Schema", schemaList);
            }
            else
            {
                SchemaChoice = schemaList.First();
            }



            var server = new Server(ServerChoice);
            var db = server.Databases[DbChoice];

            var tables = db.Tables;
            var procs = db.StoredProcedures;
            var funcs = db.UserDefinedFunctions;
            var views = db.Views;



            string desiredObject = GetObjectChoice();

            ScriptingOptions options = new ScriptingOptions
            {
                ScriptDrops = false,
                WithDependencies = false,
                IncludeHeaders = true,
                Indexes = true,                
                AllowSystemObjects = false,
                IncludeIfNotExists = true
            };

            if (tables.Contains(desiredObject, SchemaChoice))
            {
                var obj = tables[desiredObject, SchemaChoice].Script(options);
                
                foreach(string s in obj)
                {
                    Console.WriteLine(s);                    
                }
            }
            else if (procs.Contains(desiredObject, SchemaChoice))
            {
                var obj = procs[desiredObject, SchemaChoice].Script(options);

                foreach (string s in obj)
                {
                    Console.WriteLine(s);
                }
            }
            else if (funcs.Contains(desiredObject, SchemaChoice))
            {
                var obj = funcs[desiredObject, SchemaChoice].Script(options);

                foreach (string s in obj)
                {
                    Console.WriteLine(s);
                }
            }
            else if (views.Contains(desiredObject, SchemaChoice))
            {
                var obj = views[desiredObject, SchemaChoice].Script(options);

                foreach (string s in obj)
                {
                    Console.WriteLine(s);
                }
            }

            CliUtils.PressEscapeToQuit();
        }

        private static string GetChoice(string choiceDomain, List<string> options)
        {
            Console.WriteLine("Select a " + choiceDomain + ": ");

            var serverOptions = new Dictionary<int, string>();

            for (int i = 0; i < options.Count; i++)
            {
                serverOptions.Add(i, options[i]);
                Console.WriteLine(i + ": " + options[i]);
            }

            int serverChoice = Convert.ToInt32(Console.ReadLine());

            return options[serverChoice];
        }

        private static string GetObjectChoice()
        {
            Console.WriteLine("Object name: ");

            return Console.ReadLine();            
        }

        private static string GetRepoChoice()
        {

        }
    }
}