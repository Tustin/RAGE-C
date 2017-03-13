using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Script
    {
        public static Dictionary<string, Variable> GlobalVariables { get; set; }


        public static bool AddGlobalVariable(Variable var)
        {
            if (GlobalVariables.ContainsKey(var.Name)) return false;

            GlobalVariables.Add(var.Name, var);
            return true;
        }

    }
}
