using gitdb.Utils;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        public static Server ServerChoice { get; set; }
        public static Database DbChoice { get; set; }
        public static Schema SchemaChoice { get; set; }

        [STAThread]
        static void Main(string[] args)
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

            string desiredObject = CliUtils.GetUserInput("Specify a database object:");
            Console.WriteLine("Working...");

            ScriptingOptions scriptOptions = new ScriptingOptions
            {
                ScriptDrops = false,
                WithDependencies = false,
                AnsiPadding = true,                
                Indexes = true,
                AllowSystemObjects = false,                
                ScriptBatchTerminator = true,
                FullTextIndexes = false                
            };

            Clipboard.Clear();
            if (tables.Contains(desiredObject, SchemaChoice.Name))
            {
                StringCollection obj = tables[desiredObject, SchemaChoice.Name].Script(scriptOptions);
                                
                foreach(string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (procs.Contains(desiredObject, SchemaChoice.Name))
            {
                StringCollection obj = procs[desiredObject, SchemaChoice.Name].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (funcs.Contains(desiredObject, SchemaChoice.Name))
            {
                StringCollection obj = funcs[desiredObject, SchemaChoice.Name].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }
            else if (views.Contains(desiredObject, SchemaChoice.Name))
            {
                StringCollection obj = views[desiredObject, SchemaChoice.Name].Script(scriptOptions);

                foreach (string s in obj)
                {
                    Clipboard.SetText(Clipboard.GetText() + s + Environment.NewLine);
                }
            }

            Application.Exit();
        }
    }
}