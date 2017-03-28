using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    public class Compiler
    {
        internal List<string> AssemblyCode { get; set; }

        public Compiler(string filepath)
        {
            AssemblyCode = File.ReadAllLines(filepath).ToList();
        }

        public Compiler(List<string> code)
        {
            AssemblyCode = code;
        }

        private byte[] StringHexToBytes(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                     .ToArray();
        }

        private byte[] DecimalToHex(string dec)
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

        private byte[] FloatToHex(string dec)
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

        private byte ByteToHex(string b)
        {
            if (!byte.TryParse(b, out byte num))
            {
                throw new Exception("Unable to parse byte. Are you using a value higher than 255 for push1-3?");
            }
            var hex = num.ToString("X2");

            return Convert.ToByte(hex);
        }
        private uint Joaat(string input)
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
        public byte[] Compile()
        {
            if (AssemblyCode == null || AssemblyCode.Count == 0)
            {
                throw new Exception("Assembly code contents is empty or null");
            }
            byte[] statics = new byte[60];

            for (int i = 0; i < statics.Length; i++)
            {
                statics[i] = 0x00;
            }

            //replace carriage returns, empty lines and comment lines
            AssemblyCode = AssemblyCode.Select(a => a.Replace("\r\n", "\n")).ToList();
            AssemblyCode.RemoveAll(string.IsNullOrWhiteSpace);
            AssemblyCode.RemoveAll(a => a.StartsWith("/"));

            byte[] result = Parse();

            return result;
        }

        internal byte[] Parse()
        {
            var bytes = new List<byte>();
            var StringData = new List<StringData>();
            var NativeData = new List<NativeData>();
            var LabelData = new List<LabelData>();

            foreach (string line in AssemblyCode)
            {
                if (Regex.IsMatch(line, "/^\\:(.+?)$/"))
                {
                    string label = line.Replace(":", "");
                    label = line.Replace("@", "");
                    label = Regex.Replace(label, "/\\s+/", "");

                    LabelData.Add(new RAGE.LabelData()
                    {
                        Label = line,
                        LabelOffset = 0 //TODO figure this out
                    });
                    continue;
                }

                var lineParts = new List<string>();
                if (line.Contains("\xa0") && line.Contains(" "))
                {
                    string temp = line.Replace(" ", "");
                    temp = Regex.Replace(temp, "/\\s+/", "");
                    lineParts.Add(temp);
                }
                else
                {
                    string temp = line.Replace("\xa0", "");
                    //Split on space but ignore spaces inside quotes
                    var parts = Regex.Matches(temp, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value)
                    .ToList();
                    lineParts.AddRange(parts);
                }

                string opcode = lineParts[0];
                var arguments = new List<byte>();
                switch (opcode.ToLower())
                {
                    case "nop"://1
                        bytes.Add(0x00);
                        break;
                    case "add"://1
                        bytes.Add(0x01);
                        break;
                    case "sub"://1
                        bytes.Add(0x02);
                        break;
                    case "mult"://1
                        bytes.Add(0x03);
                        break;
                    case "div"://1
                        bytes.Add(0x04);
                        break;
                    case "mod"://1
                        bytes.Add(0x05);
                        break;
                    case "not"://1
                        bytes.Add(0x06);
                        break;
                    case "neg"://1
                        bytes.Add(0x07);
                        break;
                    case "cmpeq"://1
                        bytes.Add(0x08);
                        break;
                    case "cmpne"://1
                        bytes.Add(0x09);
                        break;
                    case "cmpgt"://1
                        bytes.Add(0x0a);
                        break;
                    case "cmpge"://1
                        bytes.Add(0x0b);
                        break;
                    case "cmplt"://1
                        bytes.Add(0x0c);
                        break;
                    case "cmple":
                        bytes.Add(0x0d);
                        break;
                    case "fadd"://1
                        bytes.Add(0x0e);
                        break;
                    case "fsub"://1
                        bytes.Add(0x0f);
                        break;
                    case "fmul"://1
                        bytes.Add(0x10);
                        break;
                    case "fdiv"://1
                        bytes.Add(0x11);
                        break;
                    case "fmod"://1
                        bytes.Add(0x12);
                        break;
                    case "fneg"://1
                        bytes.Add(0x13);
                        break;
                    case "fcmpeq"://1
                        bytes.Add(0x14);
                        break;
                    case "fcmpne"://1
                        bytes.Add(0x15);
                        break;
                    case "fcmpgt"://1
                        bytes.Add(0x16);
                        break;
                    case "fcmpge"://1
                        bytes.Add(0x17);
                        break;
                    case "fcmplt"://1
                        bytes.Add(0x18);
                        break;
                    case "fcmple"://1
                        bytes.Add(0x19);
                        break;
                    case "vadd"://1
                        bytes.Add(0x1a);
                        break;
                    case "vsub"://1
                        bytes.Add(0x1b);
                        break;
                    case "vmul"://1
                        bytes.Add(0x1c);
                        break;
                    case "vdiv"://1
                        bytes.Add(0x1d);
                        break;
                    case "vneg"://1
                        bytes.Add(0x1e);
                        break;
                    case "and"://1
                        bytes.Add(0x1f);
                        break;
                    case "or"://1
                        bytes.Add(0x20);
                        break;
                    case "xor"://1
                        bytes.Add(0x21);
                        break;
                    case "itof"://1
                        bytes.Add(0x22);
                        break;
                    case "ftoi"://1
                        bytes.Add(0x23);
                        break;
                    case "dup2"://1
                        bytes.Add(0x24);
                        break;
                    case "push1":
                        bytes.Add(0x25);
                        bytes.Add(ByteToHex(lineParts[1]));
                        break;
                    case "push2":
                        bytes.Add(0x26);
                        bytes.Add(ByteToHex(lineParts[1]));
                        bytes.Add(ByteToHex(lineParts[2]));
                        break;
                    case "push3":
                        bytes.Add(0x27);
                        bytes.Add(ByteToHex(lineParts[1]));
                        bytes.Add(ByteToHex(lineParts[2]));
                        bytes.Add(ByteToHex(lineParts[3]));
                        break;
                    case "push":
                        bytes.Add(0x28);
                        bytes.AddRange(DecimalToHex(lineParts[1]));
                        break;
                    case "fpush":
                        bytes.Add(0x29);
                        bytes.AddRange(FloatToHex(lineParts[1]));
                        break;
                    case "dup"://1
                        bytes.Add(0x2A);
                        break;
                    case "drop"://1
                        bytes.Add(0x2B);
                        break;
                    case "callnative":
                        bytes.Add(0x2C);
                        string joaat;
                        if (lineParts[1].StartsWith("UNK_"))
                        {
                            joaat = lineParts[1].Replace("\"", "").Replace("unk_0x", "").Replace("UNK_", "");
                        }
                        else
                        {
                            joaat = Joaat(lineParts[1]).ToString();
                        }

                        var data = NativeData.GetNativeSection(joaat);
                        //Didnt find it so add it
                        if (data == null)
                        {
                            data = new RAGE.Compiler.NativeData()
                            {
                                Native = joaat,
                                NativeLocation = NativeData.Count
                            };
                            NativeData.Add(data);
                        }
                        int args = int.Parse(lineParts[2]);
                        int ret = int.Parse(lineParts[3]);
                        byte aa = (byte)((args << 2) | ret);
                        bytes.Add(aa);
                        var loc = data.NativeLocation.ToString("X4");
                        bytes.AddRange(StringHexToBytes(loc));
                        break;

                }
            }


            return null;
        }
    }
}
