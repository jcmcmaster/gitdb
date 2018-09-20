using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gitdb.Utils
{
    public static class CliUtils
    {
        public static void PressEscapeToQuit()
        {
            Console.WriteLine();
            Console.WriteLine("Press ESC to quit or any other key to continue.");

            ConsoleKey nextKey = Console.ReadKey(true).Key;

            if (nextKey != ConsoleKey.Escape) return;

            WriteLineInColor("Good day!", ConsoleColor.DarkBlue);
            Environment.Exit(0);
        }

        /// <summary>
        /// Enumerates each option in a list and allows the user to select an object from the list
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static T GetUserSelection<T>(string prompt, IReadOnlyList<object> options)
        {
            if (options.Count == 0) throw new Exception("Empty list provided to GetUserSelection");
            if (options.Count == 1) return (T)options.First();

            Console.WriteLine(prompt);

            for (int i = 0; i < options.Count; i++)
            {
                Console.WriteLine(i + ": " + options[i]);
            }

            int selectionIndex = Convert.ToInt32(Console.ReadLine());            

            return (T)options[selectionIndex];
        }

        /// <summary>
        /// Prompts the user for a string input and returns it.
        /// </summary>
        /// <param name="prompt"></param>
        /// <returns></returns>
        public static string GetUserInput(string prompt)
        {
            Console.WriteLine();
            Console.WriteLine(prompt);

            return Console.ReadLine();
        }

        public static void WriteLineInColor(string message, ConsoleColor foregroundColor)
        {
            ConsoleColor curForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            Console.WriteLine(message);

            Console.ForegroundColor = curForegroundColor;
        }

        public static void WriteInColor(string message, ConsoleColor foregroundColor)
        {
            ConsoleColor curForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = foregroundColor;

            Console.Write(message);

            Console.ForegroundColor = curForegroundColor;
        }
    }
}
