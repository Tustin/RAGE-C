using System;

namespace RAGE.Parser.Opcodes
{
    public class Return
    {
        //@Update: Give this some options for returning values
        public static string Generate(int argumentCount = 0, bool isReturning = false)
        {
            return $"Return {argumentCount} {Convert.ToInt32(isReturning)}";
        }
    }
}
