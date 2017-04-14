using System;
using System.Diagnostics;

namespace RAGE.Logger
{
    public static class Logger
    {
        private static Stopwatch timer = Stopwatch.StartNew();
        public static bool Verbose = false;

        public static void Log(string message)
        {
            Console.WriteLine($"[{timer.Elapsed.ToString("mm\\:ss\\.ff")}] {message}");
        }

        public static void LogVerbose(string message)
        {
            if (Verbose)
                Log(message);
        }

        public static void Error(string message)
        {
            Console.Write($"[{timer.Elapsed.TotalSeconds}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR: ");
            Console.ResetColor();
            Console.Write(message);
            Console.Write("\r\n");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void Warn(string message)
        {
            Console.Write($"[{timer.Elapsed.TotalSeconds}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARN: ");
            Console.ResetColor();
            Console.Write(message);
            Console.Write("\r\n");
        }
    }
}
