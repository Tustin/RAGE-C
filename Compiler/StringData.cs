using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    internal class StringData
    {
        internal List<byte> StringStorage { get; set; }
        internal string StringLiteral { get; set; }
        internal int StringOffset { get; set; }
    }
}
