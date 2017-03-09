using System;
using System.Diagnostics;

namespace RAGE
{
    public static class Logger
    {
        private static Stopwatch timer = Stopwatch.StartNew();

        public static void Log(string message)
        {
            Console.WriteLine($"[{timer.Elapsed.TotalSeconds}] {message}");
        }
        public static void LogError(string message)
        {
            Console.Write($"[{timer.Elapsed.TotalSeconds}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR: ");
            Console.ResetColor();
            Console.Write(message);
        }
    }
}