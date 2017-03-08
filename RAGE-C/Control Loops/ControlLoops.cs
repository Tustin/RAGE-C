using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public class ControlLoop
    {
        public ControlLoopTypes Type { get; set; }

        public int CodeStartLine { get; set; }

        public int? CodeEndLine { get; set; }

        public int Index { get; set; }

        public ControlLoopLogic Logic { get; set; }

        public Function Function { get; set; }

        public ControlLoop LoopParent { get; set; }
        
        public Conditional ConditionalParent { get; set; }

        public static ControlLoopTypes GetType(string line)
        {
            if (Regex.IsMatch(line, Utilities.FOR_LOOP_REGEX))
            {
                return ControlLoopTypes.For;
            }
            if (Regex.IsMatch(line, Utilities.WHILE_LOOP_REGEX))
            {
                return ControlLoopTypes.While;
            }
            throw new Exception("Unable to parse control loop type");
        }

        public ControlLoopLogic ParseLogic(string line)
        {
            switch (GetType(line))
            {
                case ControlLoopTypes.For:
                ForLogic ifLogic = new ForLogic();
                Regex regex = new Regex(Utilities.FOR_LOOP_REGEX);
                List<string> matches = Utilities.GetRegexGroups(regex.Matches(line));
                ifLogic.IteratorVariable = matches[0];
                ifLogic.InitialIteratorValue = int.Parse(matches[1]);
                ifLogic.Condition = new ConditionalLogic(matches[2], matches[4], ConditionalLogic.GetLogicType(matches[3]));
                ifLogic.ArithmeticOperation = Arithmetic.GetType(matches[6]);
                return ifLogic;
                case ControlLoopTypes.While: //todo
                default:
                throw new NotImplementedException();
            }
        }
    }
}
