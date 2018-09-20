using gitdb.Utils;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using gitdb.Models;
using View = Microsoft.SqlServer.Management.Smo.View;

namespace gitdb
{
    internal class Program
    {
        public static Server ServerChoice { get; set; }
        public static Database DbChoice { get; set; }
        public static Schema SchemaChoice { get; set; }
        public static string ObjectChoice { get; set; }
        public static DbObject DbObjectModel { get; set; }
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
            IncludeDatabaseContext = false,
            NoCommandTerminator = false
        };

        [STAThread]
        private static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Welcome to gitdb v0.3");

            Settings = SettingsUtils.InitSettings();

            if (Settings["server_" + Environment.CurrentDirectory] == null || args.Contains("-o"))
            {
                ServerChoice =
                    new Server(CliUtils.GetUserSelection<string>("Select a server:", DbUtils.GetSqlServers()));

                Settings["server_" + Environment.CurrentDirectory] = ServerChoice.Name;
                Console.WriteLine("SERVER CHOICE SAVED. USE 'gitdb -o' TO OVERRIDE SAVED SETTINGS.");
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
                Console.WriteLine("DB CHOICE SAVED. USE 'gitdb -o' TO OVERRIDE SAVED SETTINGS.");
            }
            else
            {
                DbChoice = (Database)ServerChoice.Databases[Settings["db_" + Environment.CurrentDirectory].ToString()];
            }

            Console.WriteLine("SELECTED DATABASE: " + DbChoice.Name);

            bool found = false;

            while (!found)
            {
                ObjectChoice = CliUtils.GetUserInput("Specify a database object in the current directory:");

                if (!ObjectChoice.Contains('.'))
                {
                    Console.WriteLine("You must qualify your object with a schema.");
                    continue;
                }

                Console.WriteLine("Working...");

                string[] objectChoiceParts = ObjectChoice.Split('.');

                SchemaChoice = DbChoice.Schemas[objectChoiceParts[0]];
                ObjectChoice = objectChoiceParts[1];

                SettingsUtils.WriteSettings(Settings);

                DbObjectModel = DbUtils.GetDbObject(ServerChoice.Name, DbChoice.Name, SchemaChoice.Name, ObjectChoice);

                if (DbObjectModel != null)
                    found = true;
                else
                    Console.WriteLine("Object " + ObjectChoice + " not found in database.");
            }

            StringCollection scriptParts;
            ObjectScript = string.Empty;
            string subDir = "", objName = "";
            switch (DbObjectModel.ObjectType)
            {
                case "CLR_STORED_PROCEDURE":
                case "SQL_STORED_PROCEDURE":
                case "EXTENDED_STORED_PROCEDURE":
                    StoredProcedureCollection procs = DbChoice.StoredProcedures;
                    StoredProcedure specifiedProc = procs[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedProc.Script(ScriptOptions);
                    subDir = "Procedures";
                    objName = specifiedProc.Name;
                    Directory.CreateDirectory(ProcedureDir);
                    break;
                case "TYPE_TABLE":
                case "INTERNAL_TABLE":
                case "SYSTEM_TABLE":
                case "AGGREGATE_FUNCTION":
                case "USER_TABLE":
                    TableCollection tables = DbChoice.Tables;
                    Table specifiedTable = tables[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedTable.Script(ScriptOptions);
                    subDir = "Tables";
                    objName = specifiedTable.Name;
                    Directory.CreateDirectory(TableDir);
                    break;
                case "CLR_SCALAR_FUNCTION":
                case "SQL_TABLE_VALUED_FUNCTION":
                case "SQL_SCALAR_FUNCTION":
                case "SQL_INLINE_TABLE_VALUED_FUNCTION":
                    UserDefinedFunctionCollection funcs = DbChoice.UserDefinedFunctions;
                    UserDefinedFunction specifiedFunc = funcs[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedFunc.Script(ScriptOptions);
                    subDir = "Functions";
                    objName = specifiedFunc.Name;
                    Directory.CreateDirectory(FunctionDir);
                    break;
                case "SQL_TRIGGER":
                    throw new NotImplementedException();
                    break;
                case "VIEW":
                    ViewCollection views = DbChoice.Views;
                    View specifiedView = views[ObjectChoice, SchemaChoice.Name];
                    scriptParts = specifiedView.Script(ScriptOptions);
                    subDir = "Views";
                    objName = specifiedView.Name;
                    Directory.CreateDirectory(ViewDir);
                    break;
                case "SYNONYM":
                    throw new NotImplementedException();
                    break;
                case "SERVICE_QUEUE":
                    throw new NotImplementedException();
                    break;
                case "SEQUENCE_OBJECT":
                    throw new NotImplementedException();
                    break;
                case "DEFAULT_CONSTRAINT":
                case "FOREIGN_KEY_CONSTRAINT":
                case "PRIMARY_KEY_CONSTRAINT":
                case "UNIQUE_CONSTRAINT":
                    throw new NotImplementedException();
                    break;
                default: throw new NotImplementedException("Unsupported type!");
            }

            foreach (string s in scriptParts)
            {
                ObjectScript += s + Environment.NewLine + "GO" + Environment.NewLine;
            }

            string filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." + objName + ".sql";

            File.WriteAllText(filePath, ObjectScript);

            Console.WriteLine(subDir.Substring(0, subDir.Length - 1) + " \"" + ObjectChoice + "\" successfully scripted to " + filePath);

            Application.Exit();
        }
    }
}