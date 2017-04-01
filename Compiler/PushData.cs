using System.Collections.Generic;

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
