using Elias.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliasLibrary
{
    public class EliasLogging : ILogging
    {
        public void Log(ConsoleColor consoleColor, string input)
        {
            Console.ResetColor();
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(DateTime.Now.ToString());
            Console.ResetColor();
            Console.Write("] ");
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(input);
            Console.ResetColor();
        }
    }
}
