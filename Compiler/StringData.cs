using System.Collections.Generic;

namespace RAGE.Compiler
{
    internal class StringData
    {
        internal List<byte> StringStorage { get; set; }
        internal string StringLiteral { get; set; }
        internal int StringOffset { get; set; }
    }
}
