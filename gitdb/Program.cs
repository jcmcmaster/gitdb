using gitdb.Utils;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace gitdb
{
    internal class Program
    {
        public static Server ServerChoice { get; set; }
        public static Database DbChoice { get; set; }
        public static Schema SchemaChoice { get; set; }
        public static string ObjectChoice { get; set; }
        public static string ObjectScript { get; set; }

        public static ScriptingOptions ScriptOptions = new ScriptingOptions
        {
            ScriptDrops = false,
            WithDependencies = false,
            AnsiPadding = true,
            Indexes = true,
            AllowSystemObjects = false,
            ScriptBatchTerminator = true,
            FullTextIndexes = false
        };

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine("Welcome to gitdb BETA v0.1");

            ServerChoice = new Server(CliUtils.GetUserSelection<string>("Select a server:", DbUtils.GetSqlServers()));

            DbChoice = ServerChoice.Databases[CliUtils.GetUserSelection<string>("Select a database:",
                ServerChoice.Databases.Cast<Database>().Where(x => x.IsSystemObject == false)
                    .Select(x => x.Name).ToList())];

            SchemaChoice = DbChoice.Schemas[CliUtils.GetUserSelection<string>("Select a schema:", 
                DbChoice.Schemas.Cast<Schema>().Where(x => x.IsSystemObject == false || x.Name.Contains("dbo"))
                    .Select(x => x.Name).ToList())];

            TableCollection tables = DbChoice.Tables;
            StoredProcedureCollection procs = DbChoice.StoredProcedures;
            UserDefinedFunctionCollection funcs = DbChoice.UserDefinedFunctions;
            ViewCollection views = DbChoice.Views;

            bool found = false;
            ObjectScript = string.Empty;

            while (!found)
            {
                ObjectChoice = CliUtils.GetUserInput("Specify a database object in the current directory:");

                Console.WriteLine("Working...");

                StringCollection scriptParts;
                if (tables.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;

                    scriptParts = tables[ObjectChoice, SchemaChoice.Name].Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Tables/" + SchemaChoice + ObjectChoice + ".sql", ObjectScript);
                }
                else if (procs.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;

                    scriptParts = procs[ObjectChoice, SchemaChoice.Name].Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Tables/" + SchemaChoice + ObjectChoice + ".sql", ObjectScript);
                }
                else if (funcs.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;

                    scriptParts = funcs[ObjectChoice, SchemaChoice.Name].Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Tables/" + SchemaChoice + ObjectChoice + ".sql", ObjectScript);
                }
                else if (views.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;

                    scriptParts = views[ObjectChoice, SchemaChoice.Name].Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Tables/" + SchemaChoice + ObjectChoice + ".sql", ObjectScript);
                }
            }

            Application.Exit();
        }
    }
}