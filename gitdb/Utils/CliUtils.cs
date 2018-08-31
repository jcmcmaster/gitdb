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
            Console.WriteLine(Environment.NewLine);

            Console.WriteLine("Press ESC to quit.");

            ConsoleKey nextKey = Console.ReadKey(true).Key;
            if (nextKey == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }
        }
    }
}
