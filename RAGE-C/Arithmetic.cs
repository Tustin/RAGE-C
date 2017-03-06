using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Arithmetic
    {
        public static ArithmeticTypes GetType(string operation)
        {
            switch (operation)
            {
                case "++":
                case "+=":
                case "+":
                return ArithmeticTypes.Addition;
                case "--":
                case "-=":
                case "-":
                return ArithmeticTypes.Subtraction;
                case "*":
                case "*=":
                return ArithmeticTypes.Multiplication;
                case "/":
                case "/=":
                return ArithmeticTypes.Division;
                default:
                throw new Exception("Undefined arithmetic operation");
            }

        }
    }
}
