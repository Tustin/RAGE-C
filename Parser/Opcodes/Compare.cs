using System;

namespace RAGE.Parser.Opcodes
{
    public enum CompareType
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanEqual,
        LessThan,
        LessThanEqual,
    }
    public class Compare
    {
        public static string Generate(CompareType type)
        {
            switch (type)
            {
                case CompareType.Equal:
                    return "CmpEQ";
                case CompareType.NotEqual:
                    return "CmpNE";
                case CompareType.GreaterThan:
                    return "CmpGT";
                case CompareType.GreaterThanEqual:
                    return "CmpGE";
                case CompareType.LessThan:
                    return "CmpLT";
                case CompareType.LessThanEqual:
                    return "CmpLE";
                default:
                    throw new Exception("Invalid compare type");

            }
        }
    }
}
