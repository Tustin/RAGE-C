//#define DEBUG

using System.Collections.Generic;
using System.IO;

namespace RAGE.Parser
{
	public static class Core
	{
		public readonly static string PROJECT_ROOT = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

		public static List<string> SourceCode { get; internal set; }

		public static NativeJson Natives = new NativeJson();

		public static Dictionary<string, List<string>> AssemblyCode { get; internal set; }

		public static string FilePath { get; set; }
		public static string FileName { get; set; }
		public static string FileDirectory { get; set; }

		private static Function strcopy = new Function("strcopy", DataType.Void, new List<Parameter>()
		{
			new Parameter(DataType.Variable, "buffer", 0),
			new Parameter(DataType.String, "string", 1),
			new Parameter(DataType.Int, "size", 2)
		});

		private static Function strcat = new Function("strcat", DataType.Void, new List<Parameter>()
		{
			new Parameter(DataType.Variable, "buffer", 0),
			new Parameter(DataType.String, "string", 1),
			new Parameter(DataType.Int, "size", 2)
		});

		public static List<Function> BuiltInFunctions = new List<Function>()
		{
			strcopy,
			strcat
		};

	}
}
