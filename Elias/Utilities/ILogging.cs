using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elias.Utilities
{
    public interface ILogging
    {
        void Log(ConsoleColor consoleColor, string input);
    }
}
