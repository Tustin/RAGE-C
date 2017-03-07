using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Variable : Argument
    {
        public int FrameId { get; set; }

        public string VariableValue { get; set; }

        public Variable() { }

        public Variable(string variableType, string variableName, string variableValue, int frameId)
        {
            Value = variableName;
            ValueType = variableType;
            FrameId = frameId;
            VariableValue = variableValue;
        }
    }
}
