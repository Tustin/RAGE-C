using System;

namespace RAGE.Parser.Opcodes
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

		public static string GenerateFloat(ArithmeticType type)
		{
			switch (type)
			{
				case ArithmeticType.Addition:
				return "FAdd";
				case ArithmeticType.Subtraction:
				return "FSub";
				case ArithmeticType.Multiplication:
				return "FMul";
				case ArithmeticType.Division:
				return "FDiv";
				case ArithmeticType.Modulus:
				return "FMod";
				default:
				throw new Exception("Invalid arithmetic operation");
			}
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
        public static string GenerateInline(ArithmeticType type, int num)
        {
            if (type == ArithmeticType.Addition)
            {
                if (num < Byte.MaxValue)
                {
                    return $"Add1 {num}";
                }
                else if(num < Int16.MaxValue)
                {
                    return $"Add2 {num}";
                }
                throw new Exception("Inline addition opcode cannot be larger than 16 bits (2 bytes)");

            }
            if (type == ArithmeticType.Multiplication)
            {
                if (num < Byte.MaxValue)
                {
                    return $"Mult1 {num}";
                }
                else if (num < Int16.MaxValue)
                {
                    return $"Mult2 {num}";
                }
                throw new Exception("Inline multiplication opcode cannot be larger than 16 bits (2 bytes)");
            }
            throw new Exception("Only addition and multiplication have inline opcodes");
        }
    }

}
