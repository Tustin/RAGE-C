using System;
using System.Diagnostics;

namespace RAGE.Main
{
    public static class Logger
    {
        private static Stopwatch timer = Stopwatch.StartNew();
        public static bool Verbose = false;

        private static void Time()
        {
            Console.Write($"[{timer.Elapsed.ToString("mm\\:ss\\:ff")}] ");

        }
        public static void Log(string message)
        {
            Time();
            Console.Write(message);
            Console.Write("\r\n");
        }

        public static void LogVerbose(string message)
        {
            if (Verbose)
                Log(message);
        }

        public static void Error(string message)
        {
            Time();
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
            Time();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARN: ");
            Console.ResetColor();
            Console.Write(message);
            Console.Write("\r\n");
        }
    }
}
