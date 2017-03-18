﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RAGE
{
    //Singleton
    public static class Core
    {
        public readonly static string PROJECT_ROOT = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

        public static List<string> SourceCode { get; internal set; }

        public static NativeJson Natives = new NativeJson();

        public static Dictionary<string, List<string>> AssemblyCode { get; internal set; }

        public static List<Function> Functions { get; internal set; }

        public static List<string> SupportedTypes = new List<string>() { "void", "int", "bool", "string", "float" };

    }
}
