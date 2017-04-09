using System.Collections.Generic;

namespace RAGE.Parser
{
    public static class Script
    {
        public static List<IVariable> StaticVariables = new List<IVariable>();

        public static List<Function> Functions { get;  set; }

        public static List<Array> StaticArrays = new List<Array>();
    }
}
