using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RAGE.Compiler
{
    internal static class Utilities
    {
        internal static PushData CreatePushBeforePushString(int offset)
        {
            PushData ret = new PushData();
            if (offset < 8)
            {
                switch (offset)
                {
                    case 0:
                        ret.Opcode = Opcodes.push_0;
                        break;
                    case 1:
                        ret.Opcode = Opcodes.push_1;
                        break;
                    case 2:
                        ret.Opcode = Opcodes.push_2;
                        break;
                    case 3:
                        ret.Opcode = Opcodes.push_3;
                        break;
                    case 4:
                        ret.Opcode = Opcodes.push_4;
                        break;
                    case 5:
                        ret.Opcode = Opcodes.push_5;
                        break;
                    case 6:
                        ret.Opcode = Opcodes.push_6;
                        break;
                    case 7:
                        ret.Opcode = Opcodes.push_7;
                        break;
                }
            }
            else
            {
                if (offset <= 255)
                {
                    ret.Opcode = Opcodes.push1;
                    ret.Data.Add((byte)(offset & 0xFF));
                }
                else if (offset > 255 && offset <= 65535)
                {
                    ret.Opcode = Opcodes.pushs;
                    ret.Data.Add((byte)((offset >> 8) & 0xFF));
                    ret.Data.Add((byte)(offset & 0xFF));
                }
                else
                {
                    ret.Opcode = Opcodes.push;
                    ret.Data.Add((byte)((offset >> 24) & 0xFF));
                    ret.Data.Add((byte)((offset >> 16) & 0xFF));
                    ret.Data.Add((byte)((offset >> 8) & 0xFF));
                    ret.Data.Add((byte)(offset & 0xFF));
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

        internal static byte[] I24ToHex(int i24)
        {
            var hex = i24.ToString("X6");

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

        internal static byte[] DecimalToHex(int num)
        {
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

            //var hex = "0x" + num.ToString("X2");

            return num;
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

        //internal static List<byte> CreatePointerFromOffset(int offset)
        //{
        //    var pointer = DecimalToHex(offset);
        //    pointer[0] = 0x50;

        //    return pointer.ToList();
        //}

        internal static uint CreatePointerFromOffset(int offset)
        {
            var pointer = DecimalToHex(offset);
            pointer[0] = 0x50;
            Array.Reverse(pointer);
            return BitConverter.ToUInt32(pointer, 0);
        }

        //if you can interpret this shit then you need to be doing something more important than modding GTA
        internal static int GetSizeFromFlag(int flag, int baseSize)
        {
            baseSize <<= flag & 0xf;

            //dont even fucking ask me
            int size = ((((flag >> 17) & 0x7f) +
                (((flag >> 11) & 0x3f) << 1) +
                (((flag >> 7) & 0xf) << 2) +
                (((flag >> 5) & 0x3) << 3) +
                (((flag >> 4) & 0x1) << 4))
                * baseSize);

            for (int i = 0; i < 4; i++)
            {
                size += (((flag >> (24 + i)) & 1) == 1) ? (baseSize >> (1 + i)) : 0;
            }

            return size;
        }

        internal static uint GetFlagFromSize(int size, int baseSize)
        {
            if (size % baseSize != 0)
            {
                throw new Exception("Unable to set RSC7 header size");
            }

            for (int i = 0; i < 0x7FFFFFFF; i++)
            {
                if (GetSizeFromFlag(i, baseSize) == size) return (uint)i;
            }

            throw new Exception("Did you really wait this long?");
        }

        internal static Dictionary<int, string> ParseSwitch(string @switch)
        {
            List<string> parts = @switch.Split(']').ToList();
            parts.RemoveAll(a => a == "");

            List<string> parts2 = new List<string>();

            foreach (string part in parts)
            {
                parts2.Add(part.Replace("[", ""));
            }

            Dictionary<int, string> cases = new Dictionary<int, string>();

            foreach (string part2 in parts2)
            {
                var temp = part2.Split('=');
                int key = int.Parse(temp[0]);
                cases.Add(key, temp[1]);
            }

            return cases;
        }
    }
}
