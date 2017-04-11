using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class Enumerator
    {
        public string Name { get; set; }
        public IVariable Variable { get; set; } 

        public Enumerator(string name, Variable var)
        {
            Name = name;
            Variable = var;
        }
    }
}
