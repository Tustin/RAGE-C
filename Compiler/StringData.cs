using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    internal class StringData
    {
        internal byte[] StringSection { get; set; }
        internal byte[] StringStorage { get; set; }
        internal int StringOffsetStorage { get; set; }
    }
}
