using System.Collections.Generic;

namespace RAGE.Parser
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
