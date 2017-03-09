using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    class Program
    {
        static void Main(string[] args)
        {
            Core.SourceCode = File.ReadAllLines(Core.PROJECT_ROOT + "\\Tests\\script.c").ToList();
            Logger.Log("Loaded script file. Let's tokenize!");
            List<Token> tokens = Lexer.Tokenize();
        }
    }
}
