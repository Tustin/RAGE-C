using System;
using System.Collections.Generic;

namespace RAGE.Parser
{
    public class Variable : IVariable
    {
        public string Name { get; set; }

        public int FrameId { get; set; }

        public DataType Type { get; set; }

        public VariableValue Value { get; set; } 

        public bool IsIterator { get; set; }

		public Array ForeachReference { get; set; }

        public List<string> ValueAssembly { get; set; }

        public Specifier Specifier { get; set; }

		public StoredContext Scope { get; set; }

        public Variable(string name, int id, string type, StoredContext scope)
        {
            Name = name;
            FrameId = id;
            Type = Utilities.GetTypeFromDeclaration(type);
            Value = new VariableValue();
            ValueAssembly = new List<string>();
            Specifier = Specifier.None;
        }

        public Variable(string name, int id, DataType type, StoredContext scope = null)
        {
            Name = name;
            FrameId = id;
            Type = type;
            Value = new VariableValue();
            ValueAssembly = new List<string>();
            Specifier = Specifier.None;

        }
    }
}
