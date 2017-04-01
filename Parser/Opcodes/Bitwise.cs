using System;

namespace RAGE.Parser
{
    public enum BitwiseType
    {
        Xor,
        Not,
        Or,
        Negate,
        And

    }
    public class Bitwise
    {
        public static string Generate(BitwiseType type)
        {
            switch (type)
            {
                case BitwiseType.Xor:
                    return "Xor";
                case BitwiseType.And:
                    return "And";
                case BitwiseType.Or:
                    return "Or";
                case BitwiseType.Not:
                    return "Not";
                case BitwiseType.Negate:
                    return "Neg";
                default:
                    throw new Exception("Invalid bitwise operation");
            }
        }
    }
}