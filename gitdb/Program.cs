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
using View = Microsoft.SqlServer.Management.Smo.View;

namespace gitdb
{
    internal class Program
    {
        public static Server ServerChoice { get; set; }
        public static Database DbChoice { get; set; }
        public static Schema SchemaChoice { get; set; }
        public static string ObjectChoice { get; set; }
        public static string ObjectScript { get; set; }

        public static dynamic Settings { get; set; }

        public const string AssemblyDir = "Assemblies";
        public const string CredentialDir = "Credentials";
        public const string CustomScriptDir = "CustomScripts";
        public const string FunctionDir = "Functions";
        public const string ProcedureDir = "Procedures";
        public const string SchemaDir = "Schemas";
        public const string SequenceDir = "Sequences";
        public const string TableDir = "Tables";
        public const string TriggerDir = "Triggers";
        public const string ViewDir = "Views";

        public static ScriptingOptions ScriptOptions = new ScriptingOptions
        {
            AnsiPadding = true,
            Indexes = true,
            AllowSystemObjects = false,
            ScriptBatchTerminator = true,
            DriAll = true,
            FullTextIndexes = true,
            IncludeDatabaseContext = false
        };

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to gitdb BETA v0.3");

            Settings = SettingsUtils.InitSettings();

            if (Settings["server_" + Environment.CurrentDirectory] == null || args.Contains("-o"))
            {
                ServerChoice =
                    new Server(CliUtils.GetUserSelection<string>("Select a server:", DbUtils.GetSqlServers()));

                Settings["server_" + Environment.CurrentDirectory] = ServerChoice.Name;
            }
            else
            {
                ServerChoice = new Server(Settings["server_" + Environment.CurrentDirectory].ToString());
            }

            Console.WriteLine("SELECTED SERVER: " + ServerChoice.Name);

            if (Settings["db_" + Environment.CurrentDirectory] == null || args.Contains("-o"))
            {
                DbChoice = ServerChoice.Databases[CliUtils.GetUserSelection<string>("Select a database:",
                    ServerChoice.Databases.Cast<Database>().Where(x => x.IsSystemObject == false)
                        .Select(x => x.Name).ToList())];

                Settings["db_" + Environment.CurrentDirectory] = DbChoice.Name;

            }
            else
            {
                DbChoice = (Database) ServerChoice.Databases[Settings["db_" + Environment.CurrentDirectory].ToString()];
            }

            Console.WriteLine("SELECTED DATABASE: " + DbChoice.Name);
                
            SchemaChoice = DbChoice.Schemas[CliUtils.GetUserSelection<string>("Select a schema:", 
                DbChoice.Schemas.Cast<Schema>().Where(x => x.IsSystemObject == false || x.Name.Contains("dbo"))
                    .Select(x => x.Name).ToList())];

            SettingsUtils.WriteSettings(Settings);

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
                    Table specifiedTable = tables[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedTable.Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    Directory.CreateDirectory(TableDir);
                    File.WriteAllText(Environment.CurrentDirectory + "/Tables/" + SchemaChoice.Name + "." + specifiedTable.Name + ".sql", ObjectScript);
                }
                else if (procs.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;
                    StoredProcedure specifiedProc = procs[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedProc.Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Procedures/" + SchemaChoice.Name + "." + specifiedProc.Name + ".sql", ObjectScript);
                }
                else if (funcs.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;
                    UserDefinedFunction specifiedFunc = funcs[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedFunc.Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Functions/" + SchemaChoice.Name + "." + specifiedFunc.Name + ".sql", ObjectScript);
                }
                else if (views.Contains(ObjectChoice, SchemaChoice.Name))
                {
                    found = true;
                    View specifiedView = views[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedView.Script(ScriptOptions);

                    foreach (string s in scriptParts)
                    {
                        ObjectScript += s + Environment.NewLine;
                    }

                    File.WriteAllText(Environment.CurrentDirectory + "/Views/" + SchemaChoice.Name + "." + specifiedView.Name + ".sql", ObjectScript);
                }
                else
                {
                    Console.WriteLine("Object " + ObjectChoice + " not found in database.");
                }
            }

            Application.Exit();
        }
    }
}