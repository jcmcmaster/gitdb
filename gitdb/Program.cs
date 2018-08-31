using gitdb.Utils;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

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

        [STAThread]
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

            ScriptingOptions scriptOptions = new ScriptingOptions
            {
                ScriptDrops = true,
                WithDependencies = false,
                AnsiPadding = true,                
                Indexes = true,
                AllowSystemObjects = false,                
                ScriptBatchTerminator = true,
                FullTextIndexes = false                
            };

            Clipboard.Clear();
            if (tables.Contains(desiredObject, SchemaChoice))
            {
                var obj = tables[desiredObject, SchemaChoice].Script(scriptOptions);
                                
                foreach(string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (procs.Contains(desiredObject, SchemaChoice))
            {
                var obj = procs[desiredObject, SchemaChoice].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (funcs.Contains(desiredObject, SchemaChoice))
            {
                var obj = funcs[desiredObject, SchemaChoice].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (views.Contains(desiredObject, SchemaChoice))
            {
                var obj = views[desiredObject, SchemaChoice].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }

            Application.Exit();
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

        //private static string GetRepoChoice()
        //{

        //}
    }
}