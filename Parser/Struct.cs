using System.Collections.Generic;

namespace RAGE.Parser
{
	public class Struct
	{
		public string Name { get; set; }

		public List<IVariable> Members { get; set; }

		public Struct(string name)
		{
			Name = name;
			Members = new List<IVariable>();
		}
	}
}