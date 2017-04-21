using System.Collections.Generic;
using System.Linq;
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
                return Variables.Count + Parameters.Count + 2;
            }
        }

        public List<IVariable> Variables { get; set; }

        //public List<Array> Arrays { get; set; }

        public List<Parameter> Parameters { get; set; }

        public Function()
        {
            Variables = new List<IVariable>();
            //Arrays = new List<Array>();
            Parameters = new List<Parameter>();
        }

        public Function(string name, string returnType) : base()
        {
            Name = name;
            Type = Utilities.GetTypeFromDeclaration(returnType);
            Variables = new List<IVariable>();
            //Arrays = new List<Array>();
            Parameters = new List<Parameter>();
        }

        public Function(string name, DataType returnType) : base()
        {
            Name = name;
            Type = returnType;
            Variables = new List<IVariable>();
            //Arrays = new List<Array>();
            Parameters = new List<Parameter>();
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

        public bool ContainsParameterName(string name)
        {
            return Parameters.Any(a => a.Name == name);
        }

        public bool ContainsVariable(string name)
        {
            return Variables.Any(a => a.Name == name);
        }

        public Parameter GetParameter(string name)
        {
            return Parameters.Where(a => a.Name == name).FirstOrDefault();
        }

        public IVariable GetVariable(string name)
        {
            return Variables.Where(a => a.Name == name).FirstOrDefault();
        }

        public bool AlreadyDeclared(string var, bool iterator = false)
        {
            if (iterator)
            {
                Variable v = GetVariable(var) as Variable;
                if (v == null)
                {
                    return (ContainsParameterName(var) || ContainsVariable(var));
                }

                return (GetParameter(var) != null) || (v.IsIterator);
            }
            return (ContainsParameterName(var) || ContainsVariable(var));
        }
    }
}
