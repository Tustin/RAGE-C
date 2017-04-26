using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser.Globals
{
	public class Global
	{
		public List<IGlobal> Identifiers { get; set; }

		public int Index { get; set; }

		public Global()
		{
			Identifiers = new List<IGlobal>();
		}

		public static Global Parse(string expr)
		{
			Global global = new Global();

			int globalIdIndex = expr.IndexOf('_') + 1;
			int currentIndex = globalIdIndex;
			string globalIndexString = "";

			for (; globalIdIndex < expr.Length; globalIdIndex++)
			{
				if (expr[globalIdIndex] == '.' || expr[globalIdIndex] == '[')
				{
					break;
				}
				globalIndexString += expr[globalIdIndex];
				currentIndex++;
			}

			if (!int.TryParse(globalIndexString, out int globalIndex))
			{
				throw new Exception("Global err");
			}

			global.Index = globalIndex;

			//There's nothing left, so it was prob just a simple global call (no immediates or arrays)
			if (currentIndex == expr.Length)
			{
				return global;
			}

			for (; currentIndex < expr.Length;)
			{
				//Found immediate
				if (expr[currentIndex] == '.')
				{
					var currentIdentifier = new Globals.Immediate();
					currentIndex = ParseImmediate(expr, ++currentIndex, out int id);
					currentIdentifier.Index = id;
					global.Identifiers.Add(currentIdentifier);
				}
				else if (expr[currentIndex] == '[')
				{
					var currentIdentifier = new Globals.Array();
					currentIndex = ParseImmediate(expr, ++currentIndex, out int id);
					currentIdentifier.Index = id;
					global.Identifiers.Add(currentIdentifier);
				}
				currentIndex++;

			}

			return global;

		}

		private static int ParseImmediate(string expr, int index, out int identifier)
		{
			string idString = "";

			for (; index < expr.Length; index++)
			{
				if (expr[index] == '.' || expr[index] == '[' || expr[index] == ']')
				{
					goto check;
				}
				idString += expr[index];
			}

			check:
			idString = idString.Replace("imm_", "");
			if (!int.TryParse(idString, out identifier))
			{
				throw new Exception("Error getting identifier val");
			}

			return --index;
		}
	}
}
