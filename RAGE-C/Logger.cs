using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Logger
    {
        private static Stopwatch timer = Stopwatch.StartNew();

        public static void Log(string message)
        {
            Console.WriteLine($"[{timer.Elapsed}] {message}");
        }
    }
}
