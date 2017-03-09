using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public enum JumpType
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual,
        False,
        Unconditional
    }

    public class Jump
    {
        public static string Generate(JumpType type, string label)
        {
            switch (type)
            {
                case JumpType.Equal:
                    return $"JumpEQ {label}";
                case JumpType.False:
                    return $"JumpFalse {label}";
                case JumpType.GreaterThan:
                    return $"JumpGT {label}";
                case JumpType.GreaterThanEqual:
                    return $"JumpGE {label}";
                case JumpType.LessThan:
                    return $"JumpLT {label}";
                case JumpType.LessThanEqual:
                    return $"JumpLE {label}";
                case JumpType.NotEqual:
                    return $"JumpNE {label}";
                case JumpType.Unconditional:
                    return $"Jump {label}";
                default:
                    throw new Exception("Invalid jump type");
            }
        }
    }
}
