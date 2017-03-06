using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Core
    {
        public static List<Function> Functions;

        public static List<string> SupportedTypes = new List<string>() { "void", "int", "bool", "string" };

        public static List<string> AssemblyCode = new List<string>();

        public static List<string> RawScriptCode = new List<string>();

        public static bool IsTypeSupported(string type)
        {
            return SupportedTypes.Any(a => a == type);
        }

    }
}
