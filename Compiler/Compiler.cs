using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RAGE.Compiler;
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
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "push2":
                        bytes.Add(0x26);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        bytes.Add(Utilities.ByteToHex(lineParts[2]));
                        break;
                    case "push3":
                        bytes.Add(0x27);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        bytes.Add(Utilities.ByteToHex(lineParts[2]));
                        bytes.Add(Utilities.ByteToHex(lineParts[3]));
                        break;
                    case "push":
                        bytes.Add(0x28);
                        bytes.AddRange(Utilities.DecimalToHex(lineParts[1]));
                        break;
                    case "fpush":
                        bytes.Add(0x29);
                        bytes.AddRange(Utilities.FloatToHex(lineParts[1]));
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
                            joaat = Utilities.Joaat(lineParts[1]).ToString();
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
                        bytes.AddRange(Utilities.StringHexToBytes(loc));
                        break;
                    case "function":
                        bytes.Add(0x2D);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        bytes.AddRange(Utilities.ShortToHex(lineParts[2]));
                        bytes.Add(Utilities.ByteToHex(lineParts[3]));
                        break;
                    case "return":
                        bytes.Add(0x2E);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        bytes.Add(Utilities.ByteToHex(lineParts[2]));
                        break;
                    case "pget"://1
                        bytes.Add(0x2F);
                        break;
                    case "pset"://1
                        bytes.Add(0x30);
                        break;
                    case "ppeekset"://1
                        bytes.Add(0x31);
                        break;
                    case "tostack"://1
                        bytes.Add(0x32);
                        break;
                    case "fromstack"://1
                        bytes.Add(0x33);
                        break;
                    case "arraygetp1"://2
                        bytes.Add(0x34);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "arrayget1"://2
                        bytes.Add(0x35);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "arrayset1"://2
                        bytes.Add(0x36);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "pframe1"://2
                        bytes.Add(0x37);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "getf1"://2
                        bytes.Add(0x38);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "setf1"://2
                        bytes.Add(0x39);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "pstatic1"://2
                        bytes.Add(0x3A);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "staticget1"://2
                        bytes.Add(0x3B);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "staticset1"://2
                        bytes.Add(0x3C);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "add1"://2
                        bytes.Add(0x3D);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "mult1"://2
                        bytes.Add(0x3E);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "getstackimmediatep"://2
                        bytes.Add(0x3F);
                        break;
                    case "getimmediatep1"://2
                        bytes.Add(0x40);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "getimmediate1"://2
                        bytes.Add(0x41);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "setimmediate1"://2
                        bytes.Add(0x42);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "pushs"://2
                        bytes.Add(0x43);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "add2"://3
                        bytes.Add(0x44);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "mult2"://3
                        bytes.Add(0x45);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "getimmediatep2"://3
                        bytes.Add(0x46);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "getimmediate2"://3
                        bytes.Add(0x47);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "setimmediate2"://3
                        bytes.Add(0x48);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "arraygetp2"://3
                        bytes.Add(0x49);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "arrayget2"://3
                        bytes.Add(0x4A);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "arrayset2"://3
                        bytes.Add(0x4B);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "pframe2"://3
                        bytes.Add(0x4C);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "getf2"://3
                        bytes.Add(0x4D);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "setf2"://3
                        bytes.Add(0x4E);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "pstatic2"://3
                        bytes.Add(0x4F);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "staticget2"://3
                        bytes.Add(0x50);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "staticset2"://3
                        bytes.Add(0x51);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "pglobal2"://3
                        bytes.Add(0x52);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "globalget2"://3
                        bytes.Add(0x53);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "globalset2"://3
                        bytes.Add(0x54);
                        bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                        break;
                    case "jump"://2 special
                        bytes.Add(0x55);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumpfalse"://2 special
                        bytes.Add(0x56);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumpne"://2 special
                        bytes.Add(0x57);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumpeq"://2 special
                        bytes.Add(0x58);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumple"://2 special
                        bytes.Add(0x59);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumplt"://2 special
                        bytes.Add(0x5A);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumpge"://2 special
                        bytes.Add(0x5B);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "jumpgt"://2 special
                        bytes.Add(0x5C);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        break;
                    case "call"://3 special
                        bytes.Add(0x5D);
                        bytes.AddRange(Utilities.StringToBytes(lineParts[1]));
                        bytes.Add(0x00);
                        bytes.Add(0x00);
                        break;
                    case "pglobal3"://4
                        bytes.Add(0x5E);
                        bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                        break;
                    case "globalget3"://4
                        bytes.Add(0x5F);
                        bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                        break;
                    case "globalset3"://4
                        bytes.Add(0x60);
                        bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                        break;
                    case "pushi24"://4
                        bytes.Add(0x61);
                        bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                        break;
                    case "switch"://special
                        bytes.Add(0x62);
                        throw new NotImplementedException();
                    case "pushstring": //1 (special)
                        byte[] stringData = Encoding.ASCII.GetBytes(lineParts[1]);
                        var sdata = StringData.GetStringSection(stringData);
                        if (sdata == null)
                        {
                            //Doesn't exist so add the string to the table
                            sdata = new RAGE.Compiler.StringData()
                            {
                                StringLiteral = lineParts[1],
                                StringStorage = stringData,
                                StringOffset = StringData.Count,
                            };
                            StringData.Add(sdata);
                        }
                        var toPush = Utilities.CreatePushBeforePushString(sdata.StringOffset);
                        bytes.Add((byte)toPush.opcode);
                        bytes.AddRange(toPush.data);
                        bytes.Add(0x63);
                        break;
                    case "gethash":
                        bytes.Add(0x64);
                        break;
                    case "strcopy":
                        bytes.Add(0x65);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "itos":
                        bytes.Add(0x66);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "strcat":
                        bytes.Add(0x67);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "strcatint":
                        bytes.Add(0x68);
                        bytes.Add(Utilities.ByteToHex(lineParts[1]));
                        break;
                    case "memcpy":
                        bytes.Add(0x69); //giggity
                        break;
                    case "catch":
                        bytes.Add(0x6A);
                        break;
                    case "throw":
                        bytes.Add(0x6B);
                        break;
                    case "pcall":
                        bytes.Add(0x6C);
                        break;
                    case "push_-1":
                        bytes.Add(0x6D);
                        break;
                    case "push_0":
                        bytes.Add(0x6E);
                        break;
                    case "push_1":
                        bytes.Add(0x6F);
                        break;
                    case "push_2":
                        bytes.Add(0x70);
                        break;
                    case "push_3":
                        bytes.Add(0x71);
                        break;
                    case "push_4":
                        bytes.Add(0x72);
                        break;
                    case "push_5":
                        bytes.Add(0x73);
                        break;
                    case "push_6":
                        bytes.Add(0x74);
                        break;
                    case "push_7":
                        bytes.Add(0x75);
                        break;
                    case "fpush_-1":
                        bytes.Add(0x76);
                        break;
                    case "fpush_0":
                        bytes.Add(0x77);
                        break;
                    case "fpush_1":
                        bytes.Add(0x78);
                        break;
                    case "fpush_2":
                        bytes.Add(0x79);
                        break;
                    case "fpush_3":
                        bytes.Add(0x7A);
                        break;
                    case "fpush_4":
                        bytes.Add(0x7B);
                        break;
                    case "fpush_5":
                        bytes.Add(0x7C);
                        break;
                    case "fpush_6":
                        bytes.Add(0x7D);
                        break;
                    case "fpush_7":
                        bytes.Add(0x7E);
                        break;
                    default:
                        throw new Exception($"Unknown opcode {opcode}");
                }
            }
            return null;
        }
    }
}
