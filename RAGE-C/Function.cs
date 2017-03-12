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

        public string ReturnType { get; set; }

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
            ReturnType = returnType;
            Variables = new List<Variable>();
        }
    }
}
