using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class FunctionDeclaration
    {
        public string ReturnType { get; set; }
        public string Name { get; set; }
        public List<Argument> Arguments { get; set; }

        public FunctionDeclaration() { }

        public FunctionDeclaration(string returnType, string name, string arguments)
        {
            ReturnType = returnType;
            Name = name;
            Arguments = Utilities.GetListOfArguments(arguments);
        }
    }
}
