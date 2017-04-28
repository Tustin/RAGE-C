using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RAGE.Parser
{
	[JsonObject]
	public class Native
	{

		public string Name { get; set; }
		private DataType _results;
		public DataType ResultsType
		{
			get
			{
				return Utilities.GetTypeFromDeclaration(Results);
			}
			set
			{
				_results = Utilities.GetTypeFromDeclaration(Results);
			}
		}

		public string Results { get; set; }

		public string JHash { get; set; }

		public string x64 { get; set; }

		public List<NativeParameter> Params { get; set; }

		public static void PopulateNativeTable()
		{
			using (StreamReader sr = File.OpenText(Core.PROJECT_ROOT + "\\Resources\\natives.json"))
			using (JsonTextReader reader = new JsonTextReader(sr))
			{
				reader.SupportMultipleContent = true;

				var serializer = new JsonSerializer();
				while (reader.Read())
				{
					Core.Natives = serializer.Deserialize<NativeJson>(reader);
				}
			}
		}

		public static bool IsFunctionANative(string functionName)
		{
			functionName = functionName.ToUpper();
			return Core.Natives.Native.Any(n => n.Name == functionName || n.JHash == functionName || n.x64 == functionName);
		}

		public static int GetNativeArgumentCount(string native)
		{
			native = native.ToUpper();
			return Core.Natives.Native.Where(n => n.Name == native || n.JHash == native || n.x64 == native).Select(b => b.Params.Count).First();
		}

		public static List<NativeParameter> GetNativeArguments(string native)
		{
			native = native.ToUpper();
			return Core.Natives.Native.Where(n => n.Name == native || n.JHash == native || n.x64 == native).Select(b => b.Params).First();
		}

		public static bool HasReturnValue(string native)
		{
			native = native.ToUpper();
			return Core.Natives.Native.Any(n => (n.Name == native || n.JHash == native || n.x64 == native) && n.ResultsType != DataType.Void);
		}

		public static DataType GetReturnValue(string native)
		{
			native = native.ToUpper();
			return Core.Natives.Native.Where(n => n.Name == native || n.JHash == native || n.x64 == native).Select(b => b.ResultsType).First();
		}

		public static Native GetNative(string native)
		{
			native = native.ToUpper();
			return Core.Natives.Native.Where(n => n.Name == native || n.JHash == native || n.x64 == native).First();

		}
	}
}
