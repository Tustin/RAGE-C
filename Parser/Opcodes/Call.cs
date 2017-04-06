using System;

namespace RAGE.Parser.Opcodes
{
    public static class Call
    {
        public static string Native(string name, int argumentCount, bool isReturning)
        {
            return $"CallNative {name} {argumentCount} {Convert.ToInt32(isReturning)}";
        }

        public static string Local(string name)
        {
            return $"Call @{name}";
        }
    }
}
