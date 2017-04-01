using System;
using System.Diagnostics;

namespace RAGE
{
    public static class Logger
    {
        private static Stopwatch timer = Stopwatch.StartNew();

        public static void Log(string message)
        {
            Console.WriteLine($"[{timer.Elapsed.ToString("mm\\:ss\\.ff")}] {message}");
        }

        public static void Error(string message)
        {
            Console.Write($"[{timer.Elapsed.TotalSeconds}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR: ");
            Console.ResetColor();
            Console.Write(message);
        }

        public static void Warning(string message)
        {
            Console.Write($"[{timer.Elapsed.TotalSeconds}] ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("WARN: ");
            Console.ResetColor();
            Console.Write(message);
        }
    }
}