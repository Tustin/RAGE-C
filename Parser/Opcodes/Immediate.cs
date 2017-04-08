using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser.Opcodes
{
    public class Immediate
    {
        public static string Get(int imm)
        {
            return $"getImmediate1 {imm} //imm {imm}";
        }

        public static string Set(int imm)
        {
            return $"setImmediate1 {imm} //imm {imm}";
        }

        public static string GetPointer(int imm)
        {
            return $"getImmediateP1 {imm} //&imm {imm}";
        }

        public static string GetStackPointer()
        {
            return $"getImmP";
        }
    }
}
