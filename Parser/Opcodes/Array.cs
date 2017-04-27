using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser.Opcodes
{
	public class Array
	{
		public static string Get(int size = 1)
		{
			return $"ArrayGet1 {size}";
		}
		public static string Set(int size = 1)
		{
			return $"ArraySet1 {size}";
		}

		public static string GetPointer(int size = 1)
		{
			return $"ArrayGetP1 {size}";
		}
	}
}
