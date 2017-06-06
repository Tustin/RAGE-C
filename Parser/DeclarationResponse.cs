using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
	public class DeclarationResponse
	{
		public DataType Type { get; set; }

		public Specifier Specifier { get; set; }

		public string CustomType { get; set; }

		public DeclarationResponse()
		{
			Type = DataType.Void;
			Specifier = Specifier.None;
		}
	}
}
