using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class FunctionCall
    {
        public bool HasReturnValue { get; set; }
        public string ReturnType { get; set; }
        public string ReturnVariableName { get; set; }
        public string FunctionName { get; set; }
        public List<Argument> Arguments { get; set; }
    }
}
