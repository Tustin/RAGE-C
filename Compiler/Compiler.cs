using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using static RAGE.Main.Logger;

namespace RAGE.Compiler
{
    public class Compiler
    {
        internal List<string> AssemblyCode { get; set; }
        internal int StaticsCount { get; set; }
        public Compiler(string filepath, int staticsCount)
        {
            AssemblyCode = File.ReadAllLines(filepath).ToList();
            StaticsCount = staticsCount;
        }

        public Compiler(List<string> code, int staticsCount)
        {
            AssemblyCode = code;
            StaticsCount = staticsCount;
        }

        public byte[] Compile()
        {
            if (AssemblyCode == null || AssemblyCode.Count == 0)
            {
                throw new Exception("Assembly code contents is empty or null");
            }

            //replace carriage returns, empty lines and comment lines
            AssemblyCode = AssemblyCode.Select(a => a.Replace("\r\n", "\n")).ToList();
            AssemblyCode.RemoveAll(string.IsNullOrWhiteSpace);
            AssemblyCode.RemoveAll(a => a.StartsWith("/"));

            byte[] result = Parse();

            return result;
        }
        public byte[] Parse()
        {
            var Header = new ScriptHeader();
            var bytes = new List<byte>();
            var StringData = new List<StringData>();
            var NativeData = new List<NativeData>();
            var LabelData = new List<LabelData>();
            var LabelsToReplace = new List<LabelMap>();
            var StaticsData = new List<byte>();

            //Sections
            var CodeSection = new List<byte>();
            var StringSection = new List<byte>();
            var NativeSection = new List<byte>();
            var StaticSection = new List<byte>();
            var HeaderSection = new List<byte>();
            var EndingSection = new List<byte>();

            var result = new List<byte>();

            var StringOffsetSize = 0;

            //Fill static data
            for (int i = 0; i < StaticsCount; i++)
            {
                StaticsData.AddRange(new List<byte>() { 0x00, 0x00, 0x00, 0x00 });
            }

            foreach (string line in AssemblyCode)
            {
                if (Regex.IsMatch(line, "^\\:(.+?)$"))
                {
                    string label = line.Replace(":", "");
                    label = label.Replace("@", "");
                    label = Regex.Replace(label, "\\s+", "");

                    //Add the label and the offset to it here
                    LabelData.Add(new LabelData()
                    {
                        Label = label,
                        LabelOffset = bytes.Count
                    });
                    continue;
                }

                var lineParts = new List<string>();
                if (line.Contains("\xa0") && line.Contains(" "))
                {
                    string temp = line.Replace(" ", "");
                    temp = Regex.Replace(temp, "\\s+", "");
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
                    case "pop"://1
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
                        joaat = Utilities.Joaat(lineParts[1]).ToString("X8");
                    }

                    var data = NativeData.GetNativeSection(joaat);
                    //Didnt find it so add it
                    if (data == null)
                    {
                        data = new NativeData()
                        {
                            Native = joaat,
                            NativeLocation = NativeData.Count,
                            NativeBytes = Utilities.StringHexToBytes(joaat).ToList()
                        };
                        NativeData.Add(data);
                    }
                    int args = int.Parse(lineParts[2]);
                    int ret = int.Parse(lineParts[3]);
                    //Dont know what the purpose of this is
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
                    case "getarrayp1"://2
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
                    case "staticset1"://2 change
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
                    case "getimmp"://2
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
                    case "getarrayp2"://3
                    bytes.Add(0x49);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "getarray2"://3
                    bytes.Add(0x4A);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "setarray2"://3
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
                    case "getstatic2"://3
                    bytes.Add(0x50);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "setstatic2"://3
                    bytes.Add(0x51);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "pglobal2"://3
                    bytes.Add(0x52);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "getglobal2"://3
                    bytes.Add(0x53);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "setglobal2"://3
                    bytes.Add(0x54);
                    bytes.AddRange(Utilities.ShortToHex(lineParts[1]));
                    break;
                    case "jump"://2 special
                    bytes.Add(0x55);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumpfalse"://2 special
                    bytes.Add(0x56);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumpne"://2 special
                    bytes.Add(0x57);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumpeq"://2 special
                    bytes.Add(0x58);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumple"://2 special
                    bytes.Add(0x59);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumplt"://2 special
                    bytes.Add(0x5A);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumpge"://2 special
                    bytes.Add(0x5B);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump)); bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "jumpgt"://2 special
                    bytes.Add(0x5C);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Jump));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "call"://3 special
                    bytes.Add(0x5D);
                    LabelsToReplace.Add(new LabelMap(lineParts[1], bytes.Count, LabelType.Call));
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    bytes.Add(0x00);
                    break;
                    case "pglobal3"://4
                    bytes.Add(0x5E);
                    bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                    break;
                    case "getglobal3"://4
                    bytes.Add(0x5F);
                    bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                    break;
                    case "setglobal3"://4
                    bytes.Add(0x60);
                    bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                    break;
                    case "pushi24"://4
                    bytes.Add(0x61);
                    bytes.AddRange(Utilities.I24ToHex(lineParts[1]));
                    break;
                    case "switch"://special
                    bytes.Add(0x62);
                    int openBrackets = lineParts[1].Count(a => a == '[');
                    int closeBrackets = lineParts[1].Count(a => a == ']');
                    if (openBrackets != closeBrackets)
                    {
                        Error($"[COMPILER] Switch opcode doesn't contain equal brackets (open: {openBrackets}, close: {closeBrackets})");
                    }
                    var cases = Utilities.ParseSwitch(lineParts[1]);
                    bytes.Add((byte)cases.Count);
                    foreach (var @case in cases)
                    {
                        bytes.AddRange(BitConverter.GetBytes(@case.Key).Reverse());
                        LabelsToReplace.Add(new LabelMap(@case.Value, bytes.Count, LabelType.Jump));
                        bytes.Add(0x00);
                        bytes.Add(0x00);
                    }
                    break;
                    case "pushstring": //1 (special)
                    string pushString = lineParts[1].Replace("\"", "");
                    List<byte> stringData = Encoding.ASCII.GetBytes(pushString).ToList();
                    //Null terminate it
                    stringData.Add(0x00);
                    var sdata = StringData.GetStringSection(pushString);
                    if (sdata == null)
                    {
                        //Doesn't exist so add the string to the table
                        sdata = new StringData()
                        {
                            StringLiteral = pushString,
                            StringStorage = stringData,
                            StringOffset = StringOffsetSize,
                        };
                        StringData.Add(sdata);
                        StringOffsetSize += stringData.Count;
                    }
                    var toPush = Utilities.CreatePushBeforePushString(sdata.StringOffset);
                    bytes.Add((byte)toPush.Opcode);
                    if (toPush.Data.Count > 0)
                        bytes.AddRange(toPush.Data);
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
                    case "catch": //not used
                    bytes.Add(0x6A);
                    break;
                    case "throw": //not used
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

            //Go through each time a label is used and replace it with the offset for the label
            foreach (var item in LabelsToReplace)
            {
                string key = item.Label.Replace("@", "");

                var labelInfo = LabelData.GetLabelSection(key);
                int offset = item.ByteLocation;
                byte oneUp = bytes[offset + 1];
                byte twoUp = bytes[offset + 2];
                //Doesnt exist, so fill it with FF and throw a warning
                if (labelInfo == null)
                {
                    if (item.Type == LabelType.Call)
                    {
                        //Call
                        bytes[offset] = 0xFF;
                        bytes[offset + 1] = 0xFF;
                        bytes[offset + 2] = 0xFF;
                    }
                    else if (item.Type == LabelType.Jump)
                    {
                        //Jump
                        bytes[offset] = 0xFF;
                        bytes[offset + 1] = 0xFF;
                    }

                    Warn($"Found label {key} without a definition");
                }
                else
                {
                    if (item.Type == LabelType.Call)
                    {
                        //Call
                        byte[] i24 = Utilities.I24ToHex(labelInfo.LabelOffset);
                        bytes[offset] = i24[0];
                        bytes[offset + 1] = i24[1];
                        bytes[offset + 2] = i24[2];
                    }
                    else if (item.Type == LabelType.Jump)
                    {
                        //Jump
                        short newOffset = (short)(labelInfo.LabelOffset - (offset + 2));
                        var data = Utilities.StringHexToBytes(newOffset.ToString("X4"));
                        bytes[offset] = data[0];
                        bytes[offset + 1] = data[1];
                    }
                }
            }


            //Generate code section
            CodeSection.AddRange(bytes);

            int codeSectionSize = CodeSection.Count;
            //Need to make sure the pages are at least 16 bytes each
            while (CodeSection.Count % 16 != 0)
            {
                CodeSection.Add(0x00);
            }

            //Generate string section
            //Add all string bytes to the section list
            foreach (var sd in StringData)
            {
                StringSection.AddRange(sd.StringStorage);
            }

            int stringSectionSize = StringSection.Count;

            //Need to make sure the pages are at least 16 bytes each
            while (StringSection.Count % 16 != 0)
            {
                StringSection.Add(0x00);
            }


            //Generate native section
            //Add all natives bytes to the section list
            foreach (var nd in NativeData)
            {
                NativeSection.AddRange(nd.NativeBytes);
            }

            //Need to make sure the pages are at least 16 bytes each
            while (NativeSection.Count % 16 != 0)
            {
                NativeSection.Add(0x00);
            }


            //Generate statics section
            StaticSection.AddRange(StaticsData);

            int staticsSectionSize = StaticSection.Count;

            while (StaticSection.Count % 16 != 0)
            {
                StaticSection.Add(0x00);
            }


            //Make header
            Header.Magic = 0xB43A4500;
            Header.GlobalsVersion = 0xFF448AC7;
            Header.CodeLength = codeSectionSize;
            Header.ParameterCount = 0;
            Header.StaticsCount = staticsSectionSize / 4;
            Header.GlobalsCount = 0;
            Header.NativesCount = NativeData.Count;
            Header.GlobalsOffset = 0;
            Header.Unk2 = 0;
            Header.Unk3 = 0;
            Header.NameHash = Utilities.Joaat("test"); //@TODO: DONT HARDCODE!!!
            Header.Unk4 = 1;
            Header.StringsSize = stringSectionSize;
            Header.Unk5 = 0;

            int fileLength = CodeSection.Count + StringSection.Count + NativeSection.Count + 80;
            int nativeSectionOffset = CodeSection.Count + StringSection.Count + 80;

            //Pad ending
            for (int i = 0; i < 16; i++)
            {
                EndingSection.Add(0x00);
            }

            EndingSection.AddRange(StaticSection);
            int staticSectionOffset = fileLength + 16;
            fileLength = fileLength + StaticSection.Count;

            int fileNamePointerLoc = fileLength + 16;

            List<byte> fileName = Utilities.StringToBytes("test").ToList(); //@TODO: ALSO DONT HARDCODE!!!

            //Make sure filename has 4 null bytes affer it
            if (fileName.Count > 12)
            {
                while (fileName.Count < 32)
                {
                    fileName.Add(0x00);
                }
            }
            else
            {
                while (fileName.Count < 16)
                {
                    fileName.Add(0x00);
                }
            }

            EndingSection.AddRange(fileName);

            int codeBlockPointerLoc = fileLength + 32;

            List<byte> CodeBlocks = new List<byte>()
            {
                0x50,
                0x00,
                0x00,
                0x50
            };

            while (CodeBlocks.Count < 16)
            {
                CodeBlocks.Add(0x00);
            }

            EndingSection.AddRange(CodeBlocks);

            int stringBlocksPointerLoc = fileLength + 48;

            List<byte> StringBlocks = new List<byte>();
            //No strings so theres no need for a pointer
            if (StringData.Count == 0)
            {
                StringBlocks.Add(0x00);
                StringBlocks.Add(0x00);
                StringBlocks.Add(0x00);
                StringBlocks.Add(0x00);
            }
            else
            {
                var thing = Utilities.DecimalToHex(CodeSection.Count + 80);
                thing[0] = 0x50;
                StringBlocks.AddRange(thing);
            }

            while (StringBlocks.Count < 16)
            {
                StringBlocks.Add(0x00);
            }

            EndingSection.AddRange(StringBlocks);

            int stupidOriginalNamePointerLoc = fileLength + 64;
            var StupidNameBlock = new List<byte>();
            for (int i = 0; i < 16; i++)
            {
                if (i == 4) StupidNameBlock.Add(0x01);
                else StupidNameBlock.Add(0x00);
            }

            EndingSection.AddRange(StupidNameBlock);
            //Fill remaining header values
            Header.Unk1 = Utilities.CreatePointerFromOffset(stupidOriginalNamePointerLoc);
            Header.CodeBlocksOffset = Utilities.CreatePointerFromOffset(codeBlockPointerLoc);
            Header.NativesOffset = Utilities.CreatePointerFromOffset(nativeSectionOffset);
            Header.StaticsOffset = Utilities.CreatePointerFromOffset(staticSectionOffset);
            Header.ScriptNameOffset = Utilities.CreatePointerFromOffset(fileNamePointerLoc);
            Header.StringsOffset = Utilities.CreatePointerFromOffset(stringBlocksPointerLoc);

            result.AddRange(Header.Generate());
            result.AddRange(CodeSection);
            result.AddRange(StringSection);
            result.AddRange(NativeSection);
            result.AddRange(EndingSection);

            RSC7Header rsc7 = new RSC7Header()
            {
                Magic = 0x52534337,
                Version = 9
            };

            int hexLength = result.Count;
            int fileBaseSize = 4096; //@TODO: DONT HARDCODE
            int roundedLength = (int)Math.Ceiling((decimal)hexLength / fileBaseSize) * fileBaseSize;

            //Add padding to the final script so that it meets the required length
            while (roundedLength > result.Count)
            {
                result.Add(0x00);
            }

            hexLength = result.Count;
            rsc7.SystemFlag = Utilities.GetFlagFromSize(hexLength, fileBaseSize);
            rsc7.GraphicFlag = 0x90000000;
            result.InsertRange(0, rsc7.Generate());

            return result.ToArray();
        }
    }
}
