using System.Collections.Generic;

namespace RAGE.Compiler
{
    internal class NativeData
    {
        internal string Native { get; set; }
        internal int NativeLocation { get; set; }
        internal List<byte> NativeBytes { get; set; }
    }
}
