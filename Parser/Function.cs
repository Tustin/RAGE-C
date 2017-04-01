using System.Collections.Generic;

namespace RAGE.Parser
{
    public class Function
    {
        public string Name { get; set; }

        public DataType Type { get; set; }

        public int FrameVars
        {
            get
            {
                return Variables.Count + 2;
            }
        }

        public List<Variable> Variables { get; set; }

        public Function(string name, string returnType)
        {
            Name = name;
            Type = Utilities.GetTypeFromDeclaration(returnType);
            Variables = new List<Variable>();
        }

        public Function(string name, DataType returnType)
        {
            Name = name;
            Type = returnType;
            Variables = new List<Variable>();
        }

        //Because a func shouldnt return a local var or native (duh)
        public static bool IsValidType(DataType type)
        {
            return (type == DataType.Void 
                || type == DataType.Int 
                || type == DataType.Bool 
                || type == DataType.Float 
                || type == DataType.String);
        }
    }
}
