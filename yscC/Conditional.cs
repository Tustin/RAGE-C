using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public enum ConditionalTypes
    {
        JustIf,
        IfElse,
        //todo if elseif elseif else, etc..
        Switch,
    }
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
            return this.Parent != null;
        }
    }
}
