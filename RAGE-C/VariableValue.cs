using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class VariableValue
    {
        public string Value { get; set; }

        public List<Argument> Arguments { get; set; }

        public DataType Type { get; set; }

        public bool IsDefault { get; set; }

        public VariableValue()
        {
            Arguments = new List<Argument>();
        }

    }
}
