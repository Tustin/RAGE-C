using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Arithmetic
    {
        public enum ArithmeticType
        {
            Addition,
            Subtraction,
            Multiplication,
            Division,
            Modulus,
        }
        public static string Generate(ArithmeticType type)
        {
            switch (type)
            {
                case ArithmeticType.Addition:
                    return "Add";
                case ArithmeticType.Subtraction:
                    return "Sub";
                case ArithmeticType.Multiplication:
                    return "Mult";
                case ArithmeticType.Division:
                    return "Div";
                case ArithmeticType.Modulus:
                    return "Mod";
                default:
                    throw new Exception("Invalid arithmetic operation");
            }
        }
    }

}
