using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Function
    {
        public string Name { get; set; }

        public VariableType Type { get; set; }

        public int FrameVars
        {
            get
            {
                return Variables.Count + 1;
            }
        }

        public List<Variable> Variables { get; set; }

        public Function(string name, string returnType)
        {
            Name = name;
            Type = Utilities.GetTypeFromDeclaration(returnType);
            Variables = new List<Variable>();
        }

        public Function(string name, VariableType returnType)
        {
            Name = name;
            Type = returnType;
            Variables = new List<Variable>();
        }

        //Because a func shouldnt return a local var or native (duh)
        public static bool IsValidType(VariableType type)
        {
            return (type == VariableType.Void 
                || type == VariableType.Int 
                || type == VariableType.Bool 
                || type == VariableType.Float 
                || type == VariableType.String);
        }
    }
}
