using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RAGE
{
    public static class Core
    {
        public readonly static string PROJECT_ROOT = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

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
