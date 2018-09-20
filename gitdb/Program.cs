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
using jdb.Models;
using jdb.Utils;
using View = Microsoft.SqlServer.Management.Smo.View;

namespace jdb
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
            
            CliUtils.WriteLineInColor("Welcome to jdb v0.5", ConsoleColor.Cyan);            

            Settings = SettingsUtils.InitSettings();

            if (Settings["server_" + Environment.CurrentDirectory] == null || args.Has("-o"))
            {
                ServerChoice =
                    new Server(CliUtils.GetUserSelection<string>("Select a server:", DbUtils.GetSqlServers()));

                Settings["server_" + Environment.CurrentDirectory] = ServerChoice.Name;
                Console.WriteLine("SERVER CHOICE SAVED. USE 'jdb -o' TO OVERRIDE SAVED SETTINGS.");
            }
            else
            {
                ServerChoice = new Server(Settings["server_" + Environment.CurrentDirectory].ToString());                
            }
            
            CliUtils.WriteInColor("SELECTED SERVER: ", ConsoleColor.DarkCyan);
            Console.WriteLine(ServerChoice.Name);

            if (Settings["db_" + Environment.CurrentDirectory] == null || args.Has("-o"))
            {
                DbChoice = ServerChoice.Databases[CliUtils.GetUserSelection<string>("Select a database:",
                    ServerChoice.Databases.Cast<Database>().Where(x => x.IsSystemObject == false)
                        .Select(x => x.Name).ToList())];

                Settings["db_" + Environment.CurrentDirectory] = DbChoice.Name;
                Console.WriteLine("DB CHOICE SAVED. USE 'jdb -o' TO OVERRIDE SAVED SETTINGS.");
            }
            else
            {
                DbChoice = (Database)ServerChoice.Databases[Settings["db_" + Environment.CurrentDirectory].ToString()];
            }

            CliUtils.WriteInColor("SELECTED DATABASE: ", ConsoleColor.DarkCyan);
            Console.WriteLine(DbChoice.Name);

            while (true)
            {
                bool found = false;

                while (!found)
                {
                    ObjectChoice =
                        CliUtils.GetUserInput("Specify an object in the selected database (schema.objectname):");

                    if (!ObjectChoice.Contains('.'))
                    {
                        CliUtils.WriteLineInColor("You must qualify your object with a schema.", ConsoleColor.Red);
                        continue;
                    }

                    Console.WriteLine();
                    CliUtils.WriteLineInColor("Working...", ConsoleColor.DarkBlue);
                    Console.WriteLine();

                    string[] objectChoiceParts = ObjectChoice.Split('.');

                    if (DbChoice.Schemas[objectChoiceParts[0]] != null)
                        SchemaChoice = DbChoice.Schemas[objectChoiceParts[0]];
                    else
                    {
                        CliUtils.WriteLineInColor("\tSCHEMA '" + objectChoiceParts[0] + "' NOT FOUND IN " + DbChoice.Name, ConsoleColor.Red);
                        continue;
                    }

                    ObjectChoice = objectChoiceParts[1];

                    SettingsUtils.WriteSettings(Settings);

                    DbObjectModel =
                        DbUtils.GetDbObject(ServerChoice.Name, DbChoice.Name, SchemaChoice.Name, ObjectChoice);

                    if (DbObjectModel != null)
                        found = true;
                    else
                        CliUtils.WriteLineInColor("\tOBJECT '" + ObjectChoice + "' NOT FOUND IN " + DbChoice.Name + "." + SchemaChoice.Name, ConsoleColor.Red);
                }

                ObjectScript = string.Empty;
                string subDir = "", objName = "", filePath = "";

                switch (DbObjectModel.ObjectType)
                {
                    case "CLR_STORED_PROCEDURE":
                    case "SQL_STORED_PROCEDURE":
                    case "EXTENDED_STORED_PROCEDURE":
                        StoredProcedureCollection procs = DbChoice.StoredProcedures;
                        StoredProcedure specifiedProc = procs[ObjectChoice, SchemaChoice.Name];

                        subDir = ProcedureDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedProc.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedProc.Script(ScriptOptions);
                        break;
                    case "TYPE_TABLE":
                    case "INTERNAL_TABLE":
                    case "SYSTEM_TABLE":
                    case "USER_TABLE":
                        TableCollection tables = DbChoice.Tables;
                        Table specifiedTable = tables[ObjectChoice, SchemaChoice.Name];

                        subDir = TableDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedTable.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedTable.Script(ScriptOptions);
                        break;
                    case "CLR_SCALAR_FUNCTION":
                    case "SQL_TABLE_VALUED_FUNCTION":
                    case "SQL_SCALAR_FUNCTION":
                    case "SQL_INLINE_TABLE_VALUED_FUNCTION":
                    case "AGGREGATE_FUNCTION":
                        UserDefinedFunctionCollection funcs = DbChoice.UserDefinedFunctions;
                        UserDefinedFunction specifiedFunc = funcs[ObjectChoice, SchemaChoice.Name];

                        subDir = FunctionDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedFunc.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedFunc.Script(ScriptOptions);
                        break;

                    case "VIEW":
                        ViewCollection views = DbChoice.Views;
                        View specifiedView = views[ObjectChoice, SchemaChoice.Name];

                        subDir = ViewDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedView.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedView.Script(ScriptOptions);
                        break;
                    case "SQL_TRIGGER":

                    case "SYNONYM":

                    case "SERVICE_QUEUE":

                    case "SEQUENCE_OBJECT":

                    case "DEFAULT_CONSTRAINT":
                    case "FOREIGN_KEY_CONSTRAINT":
                    case "PRIMARY_KEY_CONSTRAINT":
                    case "UNIQUE_CONSTRAINT":

                    default:
                        throw new NotImplementedException("Unsupported object type: " + DbObjectModel.ObjectType);
                }

                CliUtils.WriteLineInColor(subDir.Substring(0, subDir.Length - 1) + " \"" + ObjectChoice +
                                  "\" successfully scripted to " + filePath, ConsoleColor.Green);

                CliUtils.PressEscapeToQuit();
            }
        }
    }
}