using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Function
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<string> Code { get; set; }
        public List<Variable> LocalVariables = new List<Variable>();
        public bool HasReturnValue { get; set; }
        public int frameCount = 2;
        public List<Conditional> Conditionals = new List<Conditional>();
        public List<ControlLoop> Loops = new List<ControlLoop>();

        public Function()
        {
            this.Code = new List<string>();
        }

        public bool AreThereAnyUnclosedLogicBlocks()
        {
            return Conditionals.Any(a => a.CodeEndLine == null);
        }

        public int GetIndexOfLastUnclosedLogicBlock()
        {
            return Conditionals.FindLastIndex(a => a.CodeEndLine == null);
        }

        public bool AreThereAnyUnclosedLoopBlocks()
        {
            return Loops.Any(a => a.CodeEndLine == null);
        }

        public int GetIndexOfLastUnclosedLoopBlock()
        {
            return Loops.FindLastIndex(a => a.CodeEndLine == null);
        }
    }
}
