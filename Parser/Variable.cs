using System.Collections.Generic;

namespace RAGE.Parser
{
    public class Variable
    {
        public string Name { get; set; }

        public int FrameId { get; set; }

        public DataType Type { get; set; }

        public VariableValue Value { get; set; } 

        public bool IsIterator { get; set; }

        public List<string> ValueAssembly { get; set; }

        public Variable(string name, int id, string type)
        {
            Name = name;
            FrameId = id;
            Type = Utilities.GetTypeFromDeclaration(type);
            Value = new VariableValue();
            ValueAssembly = new List<string>();
        }
    }
}
