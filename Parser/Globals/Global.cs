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
				IGlobal currentIdentifier = null;
				if (expr[currentIndex] == '.')
				{
					currentIdentifier = new Globals.Immediate();
				}
				else if (expr[currentIndex] == '[')
				{
					currentIdentifier = new Globals.Array();
				}

				if (currentIdentifier != null)
				{
					currentIndex = ParseImmediate(expr, ++currentIndex, out int id);
					currentIdentifier.Id = global.Identifiers.Count + 1;
					currentIdentifier.Index = id;
					global.Identifiers.Add(currentIdentifier);
					currentIdentifier = null;
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

		public static List<string> Parse(Global global, bool isGetting)
		{
			var code = new List<string>();

			//No items so it's just a simple global val
			if (global.Identifiers.Count == 0)
			{
				code.Add(isGetting ? Opcodes.Global.Get(global.Index) : Opcodes.Global.Set(global.Index));
			}
			else
			{
				//Pop the last item from the list because this is what is getting set or retrieved
				var lastIdentifier = global.Identifiers[global.Identifiers.Count - 1];
				global.Identifiers.Remove(lastIdentifier);

				//Sort any array identifiers because they get pushed first
				var arrayIdentifiers = global.Identifiers.Where(a => a is Globals.Array).OrderByDescending(a => a.Id);

				foreach (var identifier in arrayIdentifiers)
				{
					code.Add(Opcodes.Push.Int(identifier.Index.ToString()));
				}

				//Push the pointer for the global index
				code.Add(Opcodes.Global.GetPointer(global.Index));

				//Get all the immediates
				var immediateIdentifiers = global.Identifiers.Where(a => a is Globals.Immediate).OrderBy(a => a.Id);

				foreach (var identifier in immediateIdentifiers)
				{
					code.Add(Opcodes.Immediate.GetPointer(identifier.Index));
				}

				//Now go back to the arrays and push the pointers for them (needs to be in reverse order)
				foreach (var arr in arrayIdentifiers.OrderBy(a => a.Id))
				{
					code.Add(Opcodes.Array.GetPointer());
				}

				if (lastIdentifier is Globals.Immediate imm)
				{
					code.Add(isGetting ? Opcodes.Immediate.Get(imm.Index) : Opcodes.Immediate.Set(imm.Index));
				}
				else if (lastIdentifier is Globals.Array arr)
				{
					code.Add(isGetting ? Opcodes.Array.Get() : Opcodes.Array.Set());
				}
			}

			return code;

		}
	}
}
