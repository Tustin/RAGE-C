using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    class Function
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<string> Code { get; set; }
        public List<Variable> LocalVariables = new List<Variable>();
        public bool HasReturnValue { get; set; }
        public int frameCount = 2;
        public List<Conditional> Conditionals = new List<Conditional>();

        public bool AreThereAnyUnclosedLogicBlocks()
        {
            return Conditionals.Where(a => a.CodeEndLine == null).Count() > 0;
        }
        public int GetIndexOfLastUnclosedLogicBlock()
        {
            return Conditionals.FindLastIndex(a => a.CodeEndLine == null);
        }

    }
}
