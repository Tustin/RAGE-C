using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public class ConditionalLogic
    {
        public ConditionalLogicTypes LogicType { get; set; }

        public string FirstCondition { get; set; }
        public string SecondCondition { get; set; }

        public ConditionalLogic(string firstCondition, string secondCondition, ConditionalLogicTypes type)
        {
            LogicType = type;
            FirstCondition = firstCondition;
            SecondCondition = secondCondition;
        }
        public ConditionalLogic() { }
        public static ConditionalLogicTypes GetLogicType(string logic)
        {
            switch (logic)
            {
                case "==":
                return ConditionalLogicTypes.Equal;
                case "!=":
                return ConditionalLogicTypes.NotEqual;
                case ">":
                return ConditionalLogicTypes.GreaterThan;
                case ">=":
                return ConditionalLogicTypes.GreaterThanEqual;
                case "<":
                return ConditionalLogicTypes.LessThan;
                case "<=":
                return ConditionalLogicTypes.LessThanEqual;
                default:
                throw new Exception("Undefined conditional logic type");
            }
        }

        public static ConditionalLogic Parse(string line)
        {
            ConditionalLogic logic = new ConditionalLogic();
            Regex regex = new Regex(Utilities.IF_LOGIC_REGEX);
            List<string> matches;
            if (regex.IsMatch(line))
            {
                matches = regex.Matches(line).GetRegexGroups();
                logic.FirstCondition = matches[0];
                logic.LogicType = GetLogicType(matches[1]);
                logic.SecondCondition = matches[2];
                return logic;
            }
            regex = new Regex(Utilities.IF_NOT_LOGIC_REGEX);
            if (regex.IsMatch(line))
            {
                matches = regex.Matches(line).GetRegexGroups();
                logic.FirstCondition = matches[0];
                logic.LogicType = ConditionalLogicTypes.NotEqual;
                logic.SecondCondition = null;
                return logic;
            }
            regex = new Regex(Utilities.IF_TRUE_LOGIC_REGEX);
            if (regex.IsMatch(line))
            {
                matches = regex.Matches(line).GetRegexGroups();
                logic.FirstCondition = matches[0];
                logic.LogicType = ConditionalLogicTypes.Equal;
                logic.SecondCondition = null;
                return logic;
            }
            throw new Exception("Unable to parse conditional logic");
        }
    }
}
