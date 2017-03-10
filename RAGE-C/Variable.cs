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

        public string Type { get; set; }

        public VariableValueType ValueType { get; set; }

        public VariableValue Value { get; set; } 

        public Variable(string name, int id, string type)
        {
            Name = name;
            FrameId = id;
            Type = type;
            Value = new VariableValue();
        }
    }
}
