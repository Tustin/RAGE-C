using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    internal class PushData
    {
        internal Opcodes Opcode { get; set; }
        internal List<byte> Data { get; set; }

        public PushData()
        {
            Data = new List<byte>();
        }
    }
}
