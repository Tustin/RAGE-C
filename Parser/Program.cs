using System;
using System.Collections.Generic;
using System.IO;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using NDesk.Options;
using static RAGE.Main.Logger;
namespace RAGE.Parser
{
    class Program
    {
        static void Main(string[] args)
        {

            Logo();
            string filePath = null;
            bool showingHelp = false;
            new OptionSet() {
                { "s=|script=", a => filePath = a },
                { "v|verbose", a => Verbose = true},
                { "h|help", a => showingHelp = true }
            }.Parse(args);
#if DEBUG
            filePath = Core.PROJECT_ROOT + "\\Tests\\test.c";         
#endif
            if (showingHelp)
            {
                Help();
                return;
            }

            if (filePath == null)
            {
                Error("No script file path set. Please use -h for help");
                return;
            }

            LogVerbose("Populating native table...");

            Native.PopulateNativeTable();

            LogVerbose($"Added {Core.Natives.Native.Count} natives to native table");

            AntlrFileStream fs = new AntlrFileStream(Core.PROJECT_ROOT + "\\Tests\\test.c");

            Core.AssemblyCode = new Dictionary<string, List<string>>();

            Script.Functions = new List<Function>();

            LogVerbose("Loaded script file");

            RAGELexer lexer = new RAGELexer(fs);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            LogVerbose($"Successfully lexed tokens");

            RAGEParser parser = new RAGEParser(tokens);

            ParseTreeWalker walker = new ParseTreeWalker();

            RAGEListener listener = new RAGEListener();

            LogVerbose("Starting to walk parse tree...");
            parser.RemoveErrorListeners();
            ParseTreeWalker.Default.Walk(listener, parser.compilationUnit());

            LogVerbose("Finished walking parse tree");

            LogVerbose("Writing assembly to output file...");

            List<string> final = new List<string>
            {
                $"//Compiled using RAGE-C by Tustin {DateTime.Now.ToShortDateString()}"
            };

            foreach (var item in Core.AssemblyCode)
            {
                final.Add($":{item.Key}");
                final.AddRange(item.Value);
                final.Add("");
            }

            File.WriteAllLines(Core.PROJECT_ROOT + "\\Tests\\test.csa", final.ToArray());

            Compiler.Compiler compiler = new Compiler.Compiler(final);

            LogVerbose("Compiling script file...");

            var res = compiler.Compile();

            File.WriteAllBytes(Core.PROJECT_ROOT + "\\Tests\\test.csc", res);

            Log("Successfully saved assembly!");
        }

        static void Help()
        {
            Console.Clear();
            Logo();
            Console.WriteLine("-s|-script   --   C script to be compiled");
            Console.WriteLine("-v|-verbose  --   Output additional compilation info");
            Console.WriteLine("-h|-help     --   You're already here ya dingus");
            Console.ReadKey();
        }
        static void Logo()
        {
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@"  _____            _____ ______       _____ ");
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@" |  __ \     /\   / ____|  ____|     / ____|");
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@" | |__) |   /  \ | |  __| |__ ______| |     ");
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@" |  _  /   / /\ \| | |_ |  __|______| |     ");
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@" | | \ \  / ____ \ |__| | |____     | |____ ");
            Console.SetCursorPosition((Console.WindowWidth - 44) / 2, Console.CursorTop);
            Console.WriteLine(@" |_|  \_\/_/    \_\_____|______|     \_____|");
            Console.WriteLine();
        }
    }
}
