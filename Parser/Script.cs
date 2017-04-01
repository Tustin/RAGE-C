using System.Collections.Generic;

namespace RAGE.Parser
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
