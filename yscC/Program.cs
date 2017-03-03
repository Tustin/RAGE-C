using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace RAGE
{
    class Program
    {
        const bool DEV = true;

        static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();
            Native.PopulateNativeTable();
            using (Stream fs = new FileStream("script.c", FileMode.Open))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    Parser p = new Parser(sr);
                    List<Function> functions = p.GetAllFunctions();
                    //Function main = p.GetFunctionContents("main");
                    Debug.Assert(functions != null);
                    List<string> asmCode = new List<string>();
                    asmCode.Add("//compiled using RAGE-C by Tustin");
                    if (functions.Where(a => a.Name == "main").FirstOrDefault() == null)
                    {
                        Console.WriteLine("FAILED!! Script must have an entry point function \"main\"");
                        Console.ReadKey();
                        return;
                    }
                    Console.WriteLine("Found main script function. Generating script entry point...");
                    asmCode.AddRange(p.GenerateEntryPoint());
                    foreach (Function function in functions)
                    {
                        Console.WriteLine($"[Function] {function.Name} with return type { function.ReturnType} and {function.Code.Count} lines of code");
                        List<string> code = p.GenerateASMFunction(function);
                        asmCode.AddRange(code);
                    }

                    File.WriteAllLines("script.csa", asmCode.ToArray());

                    //TimeSpan executionTime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
                    Console.WriteLine($"Finished compiling in {sw.Elapsed.TotalSeconds} seconds");
                    if (!DEV)
                    {
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
