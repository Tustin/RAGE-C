using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
//Tustin
namespace RAGE
{
    class Program
    {

        static void Main(string[] args)
        {
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
                    asmCode.AddRange(p.GenerateEntryPoint());
                    foreach (Function main in functions)
                    {
                        Console.WriteLine($"Function {main.Name} with return type { main.ReturnType} and {main.Code.Count} lines of code");
                        if (main.Conditionals.Count > 0)
                        {
                            for (int i = main.Conditionals[0].CodeStartLine; i <= main.Conditionals[0].CodeEndLine; i++)
                            {
                                Console.WriteLine(main.Code[i]);
                            }
                        }
                        List<string> code = p.GenerateASMFunction(main);
                        asmCode.AddRange(code);
                    }

                    File.WriteAllLines("script.csa", asmCode.ToArray());
                }
            }
        }
    }
}
