using System;

namespace RAGE.Parser.Opcodes
{
    public class Return
    {
        public static string Generate(int argumentCount = 0, bool isReturning = false)
        {
            return $"Return {argumentCount} {Convert.ToInt32(isReturning)}";
        }
        public static string Generate(Function function)
        {
            return $"Return {function.Parameters.Count} {Convert.ToInt32(function.Type != DataType.Void)}";
        }
    }
}
