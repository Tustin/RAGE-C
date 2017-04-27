using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser.Opcodes
{
    public class Global
    {
        public static string Get(int global)
        {
			if (global > short.MaxValue)
			{
				return $"GetGlobal3 {global} //Global_{global}";
			}
			return $"GetGlobal2 {global} //Global_{global}";

		}

		public static string Set(int global)
        {
			if (global > short.MaxValue)
			{
				return $"SetGlobal3 {global} //Global_{global}";
			}
			return $"SetGlobal2 {global} //Global_{global}";

		}

        public static string GetPointer(int global)
        {
			if (global > short.MaxValue)
			{
				return $"pGlobal3 {global} //&Global_{global}";
			}
			return $"pGlobal2 {global} //&Global_{global}";
        }
    }
}
