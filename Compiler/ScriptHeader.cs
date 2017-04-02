﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RAGE.Compiler
{
    //Credits to Zorg
    internal class ScriptHeader
    {
        //Header Start
        public uint Magic { get; set; }
        public uint Unk1 { get; set; } //wtf?
        public uint CodeBlocksOffset { get; set; }
        public uint GlobalsVersion { get; set; } //Not sure if this is the globals version
        public Int32 CodeLength { get; set; } //Total length of code
        public Int32 ParameterCount { get; set; } //Count of paremeters to the script
        public Int32 StaticsCount { get; set; }
        public Int32 GlobalsCount { get; set; }
        public Int32 NativesCount { get; set; } //Native count * 4 = total block length
        public uint StaticsOffset { get; set; }
        public uint GlobalsOffset { get; set; }
        public uint NativesOffset { get; set; }
        public Int32 Unk2 { get; set; } //unknown
        public Int32 Unk3 { get; set; } //Unknown
        public uint NameHash { get; set; } //Hash of the script name at ScriptNameOffset
        public Int32 Unk4 { get; set; }
        public uint ScriptNameOffset { get; set; }
        public uint StringsOffset { get; set; } //Offset of the string table
        public Int32 StringsSize { get; set; } //Total length of the string block
        public Int32 Unk5 { get; set; }


        internal List<byte> Generate()
        {
            var result = new List<byte>();

            result.AddRange(BitConverter.GetBytes(Magic).Reverse());
            result.AddRange(BitConverter.GetBytes(Unk1).Reverse());
            result.AddRange(BitConverter.GetBytes(CodeBlocksOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(GlobalsVersion).Reverse());
            result.AddRange(BitConverter.GetBytes(CodeLength).Reverse());
            result.AddRange(BitConverter.GetBytes(ParameterCount));
            result.AddRange(BitConverter.GetBytes(StaticsCount).Reverse());
            result.AddRange(BitConverter.GetBytes(GlobalsCount));
            result.AddRange(BitConverter.GetBytes(NativesCount).Reverse());
            result.AddRange(BitConverter.GetBytes(StaticsOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(GlobalsOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(NativesOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(Unk2));
            result.AddRange(BitConverter.GetBytes(Unk3));
            result.AddRange(BitConverter.GetBytes(NameHash).Reverse());
            result.AddRange(BitConverter.GetBytes(Unk4).Reverse());
            result.AddRange(BitConverter.GetBytes(ScriptNameOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(StringsOffset).Reverse());
            result.AddRange(BitConverter.GetBytes(StringsSize).Reverse());
            result.AddRange(BitConverter.GetBytes(Unk5));

            return result;
        }

    }
}