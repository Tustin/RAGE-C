using System;
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
            AntlrFileStream fs = new AntlrFileStream(Core.PROJECT_ROOT + "\\Tests\\script.c");

            Core.AssemblyCode = new Dictionary<string, List<string>>();

            Logger.Log("Loaded script file");

            CLexer lexer = new CLexer(fs);

            CommonTokenStream tokens = new CommonTokenStream(lexer);

            CParser parser = new CParser(tokens);

            ParseTreeWalker walker = new ParseTreeWalker();

            MyListener listener = new MyListener();

            ParseTreeWalker.Default.Walk(listener, parser.compilationUnit());

            CBaseListener c = new CBaseListener();

            List<string> final = new List<string>();
            foreach (KeyValuePair<string, List<string>> item in Core.AssemblyCode)
            {
                final.Add($":{item.Key}");
                final.AddRange(item.Value);
            }

            File.WriteAllLines(Core.PROJECT_ROOT + "\\Tests\\script.csa",final.ToArray());


            //Core.Tokens = Lexer.Tokenize();
            //Logger.Log($"Successfully tokenized {Core.Tokens.Count} items!");
            //Logger.Log("Constructing AST...");
        }
    }
}
