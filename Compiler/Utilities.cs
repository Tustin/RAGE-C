using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    internal static class Utilities
    {
        internal static (Opcodes opcode, List<byte> data) CreatePushBeforePushString(int offset)
        {
            (Opcodes opcode, List<byte> data) ret = (Opcodes.push, new List<byte>());
            if (offset < 8)
            {
                switch (offset)
                {
                    case 0:
                        ret.opcode = Opcodes.push_0;
                        break;
                    case 1:
                        ret.opcode = Opcodes.push_1;
                        break;
                    case 2:
                        ret.opcode = Opcodes.push_2;
                        break;
                    case 3:
                        ret.opcode = Opcodes.push_3;
                        break;
                    case 4:
                        ret.opcode = Opcodes.push_4;
                        break;
                    case 5:
                        ret.opcode = Opcodes.push_5;
                        break;
                    case 6:
                        ret.opcode = Opcodes.push_6;
                        break;
                    case 7:
                        ret.opcode = Opcodes.push_7;
                        break;
                }
            }
            else
            {
                if (offset <= 255)
                {
                    ret.opcode = Opcodes.push1;
                    ret.data.Add((byte)(offset & 0xFF));
                }
                else if (offset > 255 && offset <= 65535)
                {
                    ret.opcode = Opcodes.pushs;
                    ret.data.Add((byte)((offset >> 8) & 0xFF));
                    ret.data.Add((byte)(offset & 0xFF));
                }
                else
                {
                    ret.opcode = Opcodes.push;
                    ret.data.Add((byte)((offset >> 24) & 0xFF));
                    ret.data.Add((byte)((offset >> 16) & 0xFF));
                    ret.data.Add((byte)((offset >> 8) & 0xFF));
                    ret.data.Add((byte)(offset & 0xFF));
                }
            }
            return ret;
        }

        internal static byte[] StringHexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        internal static byte[] StringToBytes(string s)
        {
            return Encoding.ASCII.GetBytes(s);
        }

        internal static byte[] ShortToHex(string s)
        {
            if (!short.TryParse(s, out short num))
            {
                throw new Exception("Unable to parse int value");
            }
            var hex = num.ToString("X4");

            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        internal static byte[] I24ToHex(string i24)
        {
            int int3 = i24[0] + (i24[1] << 8) + (i24[2] << 16);

            var hex = int3.ToString("X6");

            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        internal static byte[] DecimalToHex(string dec)
        {
            if (!int.TryParse(dec, out int num))
            {
                throw new Exception("Unable to parse int value");
            }
            var hex = num.ToString("X8");

            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        internal static byte[] FloatToHex(string dec)
        {
            if (!float.TryParse(dec, out float num))
            {
                throw new Exception("Unable to parse float value");
            }
            var hex = num.ToString("X8");

            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        internal static byte ByteToHex(string b)
        {
            if (!byte.TryParse(b, out byte num))
            {
                throw new Exception("Unable to parse byte. Are you using a value higher than 255 for push1-3?");
            }
            var hex = num.ToString("X2");

            return Convert.ToByte(hex);
        }

        internal static uint Joaat(string input)
        {
            byte[] stingbytes = Encoding.UTF8.GetBytes(input.ToLower());
            uint num1 = 0U;
            for (int i = 0; i < stingbytes.Length; i++)
            {
                uint num2 = num1 + (uint)stingbytes[i];
                uint num3 = num2 + (num2 << 10);
                num1 = num3 ^ num3 >> 6;
            }
            uint num4 = num1 + (num1 << 3);
            uint num5 = num4 ^ num4 >> 11;
            return num5 + (num5 << 15);
        }
    }
}
