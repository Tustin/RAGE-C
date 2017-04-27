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
			if (imm > byte.MaxValue)
			{
				return $"GetImmediate2 {imm} //imm {imm}";
			}
			return $"GetImmediate1 {imm} //imm {imm}";
        }

        public static string Set(int imm)
        {
			if (imm > byte.MaxValue)
			{
				return $"SetImmediate2 {imm} //imm {imm}";
			}
			return $"SetImmediate1 {imm} //imm {imm}";
		}

        public static string GetPointer(int imm)
        {
			if (imm > byte.MaxValue)
			{
				return $"GetImmediateP2 {imm} //imm {imm}";
			}
			return $"GetImmediateP1 {imm} //imm {imm}";
		}

        public static string GetStackPointer()
        {
            return $"getImmP";
        }
    }
}
