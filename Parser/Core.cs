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

	}
}
