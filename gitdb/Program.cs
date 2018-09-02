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
            Console.WriteLine("Welcome to gitdb BETA");

            ServerChoice = GetUserSelection<string>("Select a server:", DbUtils.GetServers());
            DbChoice = GetUserSelection<string>("Select a database:", DbUtils.GetDataBases(ServerChoice));
            SchemaChoice = GetUserSelection<string>("Select a schema", DbUtils.GetSchemas(ServerChoice, DbChoice));

            var server = new Server(ServerChoice);
            Database db = server.Databases[DbChoice];

            TableCollection tables = db.Tables;
            StoredProcedureCollection procs = db.StoredProcedures;
            UserDefinedFunctionCollection funcs = db.UserDefinedFunctions;
            ViewCollection views = db.Views;

            string desiredObject = GetObjectChoice();
            Console.WriteLine(Environment.NewLine + "Working...");

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

        /// <summary>
        /// Enumerates each option in a list and allows the user to select an object from the list
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static T GetUserSelection<T>(string prompt, IReadOnlyList<object> options)
        {
            if (options.Count == 0) throw new Exception("Empty list provided to GetUserSelection");
            if (options.Count == 1) return (T)options.First();

            Console.WriteLine(Environment.NewLine + prompt + Environment.NewLine);

            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine(i + ": " + options[i]);
            }

            Console.WriteLine();

            int selectionIndex = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine();
            Console.WriteLine(options[selectionIndex] + " selected.");

            return (T)options[selectionIndex];
        }

        private static string GetObjectChoice()
        {
            Console.WriteLine();
            Console.WriteLine("Specify a database object:");
            Console.WriteLine();

            return Console.ReadLine();
        }

        //private static string GetRepoChoice()
        //{

        //}
    }
}