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
            return $"getGlobal2 {global} //Global_{global}";
        }

        public static string Set(int global)
        {
            return $"setGlobal2 {global} //Global_{global}";

        }

        public static string GetPointer(int global)
        {
            return $"pGlobal2 {global} //&Global_{global}";
        }
    }
}
