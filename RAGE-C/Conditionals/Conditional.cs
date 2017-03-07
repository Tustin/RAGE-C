using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Conditional
    {
        //Type of conditional
        public ConditionalTypes Type { get; set; }
        //Line inside the function where the block begins
        public int CodeStartLine { get; set; }
        //Line inside the function where the block ends
        public int? CodeEndLine { get; set; }

        public int Index { get; set; }

        public Conditional Parent { get; set; }

        public ConditionalLogic Logic { get; set; }

        public Function Function { get; set; }

        public Conditional(ConditionalTypes type, int start, int? end, Conditional parent = null)
        {
            Type = type;
            CodeStartLine = start;
            CodeEndLine = end;
            Logic = new ConditionalLogic();
            Parent = parent;
        }

        public bool IsNested()
        {
            return Parent != null;
        }

        public string ParseConditionalLogic(string condition)
        {
            if (Function.LocalVariables.IsLocalVariable(condition))
            {
                Variable localVar = this.Function.LocalVariables.GetLocalVariable(condition);
                return FrameVar.Get(localVar);
            }
            else
            {
                //turn any hashes into int format
                if (condition.Contains("0x"))
                {
                    condition = condition.Replace("0x", "");
                    condition = int.Parse(condition, System.Globalization.NumberStyles.HexNumber).ToString();
                }

                return Push.Generate(condition);
            }
        }
    }
}
