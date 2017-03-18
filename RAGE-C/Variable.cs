using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Variable
    {
        public string Name { get; set; }

        public int FrameId { get; set; }

        public DataType Type { get; set; }

        public VariableValue Value { get; set; } 

        public bool IsIterator { get; set; }

        public Variable(string name, int id, string type)
        {
            Name = name;
            FrameId = id;
            Type = Utilities.GetTypeFromDeclaration(type);
            Value = new VariableValue();
        }
    }
}
