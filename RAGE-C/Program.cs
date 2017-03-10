﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
namespace RAGE
{
    class Program
    {
        static void Main(string[] args)
        {

            Native.PopulateNativeTable();

            AntlrFileStream fs = new AntlrFileStream(Core.PROJECT_ROOT + "\\Tests\\script.c");

            Core.AssemblyCode = new Dictionary<string, List<string>>();

            Core.Functions = new List<Function>();

            Logger.Log("Loaded script file");

            CLexer lexer = new CLexer(fs);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            Logger.Log($"Successfully lexed tokens");

            CParser parser = new CParser(tokens);

            ParseTreeWalker walker = new ParseTreeWalker();

            MyListener listener = new MyListener();

            Logger.Log("Starting to walk parse tree...");

            ParseTreeWalker.Default.Walk(listener, parser.compilationUnit());

            //CBaseListener c = new CBaseListener();

            List<string> final = new List<string>();

            foreach (KeyValuePair<string, List<string>> item in Core.AssemblyCode)
            {
                final.Add($":{item.Key}");
                final.AddRange(item.Value);
            }

            File.WriteAllLines(Core.PROJECT_ROOT + "\\Tests\\script.csa",final.ToArray());
        }
    }
}
