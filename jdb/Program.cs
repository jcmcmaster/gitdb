using Microsoft.SqlServer.Management.Smo;
using System;
using System.IO;
using System.Linq;
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
        public const string SynonymDir = "Synonyms";
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
            
            CliUtils.WriteLineInColor("Welcome to jdb v0.4", ConsoleColor.Cyan);            

            Settings = SettingsUtils.InitSettings();

            SetServer(args);
            SetDb(args);

            while (true)
            {
                bool found = false;

                while (!found)
                {
                    Console.WriteLine();
                    Console.Write("Specify an object in the selected database (");
                    CliUtils.WriteInColor("schema", ConsoleColor.DarkBlue);
                    Console.Write(".");
                    CliUtils.WriteInColor("object", ConsoleColor.Blue);
                    Console.WriteLine(")");

                    ObjectChoice = Console.ReadLine() ?? "";

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
                        CliUtils.WriteLineInColor("\tSchema '" + objectChoiceParts[0] + "' not found in " + DbChoice.Name, ConsoleColor.Red);
                        continue;
                    }

                    ObjectChoice = objectChoiceParts[1];

                    SettingsUtils.WriteSettings(Settings);

                    DbObjectModel =
                        DbUtils.GetDbObject(ServerChoice.Name, DbChoice.Name, SchemaChoice.Name, ObjectChoice);

                    if (DbObjectModel != null)
                        found = true;
                    else
                        CliUtils.WriteLineInColor("\tObject '" + ObjectChoice + "' not found in " + DbChoice.Name + "." + SchemaChoice.Name, ConsoleColor.Red);
                }

                ObjectScript = string.Empty;
                string subDir = "", objName = "", filePath = "";

                switch (DbObjectModel.ObjectType.Trim().ToUpper())
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
                    case "SEQUENCE_OBJECT":
                        SequenceCollection sequences = DbChoice.Sequences;
                        Sequence specifiedSequence = sequences[ObjectChoice, SchemaChoice.Name];

                        subDir = SequenceDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedSequence.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedSequence.Script(ScriptOptions);
                        break;
                    case "SQL_TRIGGER":
                        DatabaseDdlTriggerCollection triggers = DbChoice.Triggers;
                        DatabaseDdlTrigger specifiedTrigger = triggers[ObjectChoice];

                        subDir = TriggerDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedTrigger.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedTrigger.Script(ScriptOptions);
                        break;
                    case "SYNONYM":
                        SynonymCollection synonyms = DbChoice.Synonyms;
                        Synonym specifiedSynonym = synonyms[ObjectChoice];

                        subDir = SynonymDir;

                        Directory.CreateDirectory(subDir);

                        objName = specifiedSynonym.Name;
                        filePath = Environment.CurrentDirectory + "\\" + subDir + "\\" + SchemaChoice.Name + "." +
                                   objName + ".sql";
                        ScriptOptions.FileName = filePath;

                        specifiedSynonym.Script(ScriptOptions);
                        break;
                    case "SERVICE_QUEUE":

                    
                    case "DEFAULT_CONSTRAINT":
                    case "FOREIGN_KEY_CONSTRAINT":
                    case "PRIMARY_KEY_CONSTRAINT":
                    case "UNIQUE_CONSTRAINT":

                    default:
                        throw new NotImplementedException("Unsupported object type: " + DbObjectModel.ObjectType);
                }

                CliUtils.WriteLineInColor("\t" + subDir.Substring(0, subDir.Length - 1) + " \"" + ObjectChoice +
                                  "\" successfully scripted to " + filePath, ConsoleColor.Green);

                CliUtils.PressEscapeToQuit();
            }
        }

        private static void SetDb(string[] args)
        {
            if (Settings["db_" + Environment.CurrentDirectory] == null || args.Has("-o"))
            {
                DbChoice = ServerChoice.Databases[CliUtils.GetUserSelection<string>("Select a database:",
                    ServerChoice.Databases.Cast<Database>().Where(x => x.IsSystemObject == false)
                        .Select(x => x.Name).ToList())];

                Settings["db_" + Environment.CurrentDirectory] = DbChoice.Name;
                Console.WriteLine("DB choice saved. Use 'jdb -o' to override saved settings.");
            }
            else
            {
                DbChoice = (Database)ServerChoice.Databases[Settings["db_" + Environment.CurrentDirectory].ToString()];
            }

            CliUtils.WriteInColor("SELECTED DATABASE: ", ConsoleColor.DarkCyan);
            Console.WriteLine(DbChoice.Name);
        }

        protected static void SetServer(string[] args)
        {
            if (Settings["server_" + Environment.CurrentDirectory] == null || args.Has("-o"))
            {
                ServerChoice =
                    new Server(CliUtils.GetUserSelection<string>("Select a server:", DbUtils.GetSqlServers()));

                Settings["server_" + Environment.CurrentDirectory] = ServerChoice.Name;
                Console.WriteLine("Server choice saved. Use 'jdb -o' to override saved settings.");
            }
            else
            {
                ServerChoice = new Server(Settings["server_" + Environment.CurrentDirectory].ToString());
            }

            CliUtils.WriteInColor("SELECTED SERVER: ", ConsoleColor.DarkCyan);
            Console.WriteLine(ServerChoice.Name);
        }
    }
}