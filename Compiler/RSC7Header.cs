using System;
using System.Collections.Generic;
using System.Linq;

namespace RAGE.Compiler
{
    internal class RSC7Header
    {
        internal uint Magic { get; set; }
        internal uint Version { get; set; }
        internal uint SystemFlag { get; set; }
        internal uint GraphicFlag { get; set; }

        internal List<byte> Generate()
        {
            var ret = new List<byte>();
            ret.AddRange(BitConverter.GetBytes(Magic).Reverse());
            ret.AddRange(BitConverter.GetBytes(Version).Reverse());
            ret.AddRange(BitConverter.GetBytes(SystemFlag).Reverse());
            ret.AddRange(BitConverter.GetBytes(GraphicFlag).Reverse());

            return ret;
        }
    }
}
