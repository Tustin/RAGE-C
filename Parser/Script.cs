using System.Collections.Generic;

namespace RAGE.Parser
{
    public static class Script
    {
        public static List<IVariable> StaticVariables = new List<IVariable>();

        public static List<Function> Functions { get; set; }

        public static List<Array> StaticArrays = new List<Array>();

        public static List<Enum> Enums = new List<Enum>();

        public static int GetNextStaticIndex()
        {
            int next = 0;
            foreach (var var in StaticVariables)
            {
                if (var is Array arr)
                {
                    next += arr.Indices.Count;
                }
                else if (var is Variable)
                {
                    next++;
                }
            }
            return next;
        }
    }
}
