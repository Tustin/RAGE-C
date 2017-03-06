using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace RAGE
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.RawScriptCode = File.ReadAllLines("script.c").ToList();

            Native.PopulateNativeTable();

            Core.Functions = Parser.GetAllFunctions();

            Debug.Assert(Core.Functions != null);

            Core.AssemblyCode.Add("//Compiled using RAGE-C by Tustin");

            //Each scripts needs an entry point function called 'main' to be executed
            if (!Core.Functions.ContainsFunction("main"))
            {
                Logger.Log("ERROR - Script must have an entry point function called \"main\"");
                Console.ReadKey();
                return;
            }

            Logger.Log("Found main script function. Generating script entry point...");

            Core.AssemblyCode.AddRange(Parser.GenerateEntryPoint());

            foreach (Function function in Core.Functions)
            {
                Logger.Log($"Parsing and generating code for function {function.Name}...");
                List<string> code = Parser.GenerateASMFunction(function);
                Core.AssemblyCode.AddRange(code);
            }

            File.WriteAllLines("script.csa", Core.AssemblyCode.ToArray());

            Logger.Log("Compilation finished!");
        }
    }
}
