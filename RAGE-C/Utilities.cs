using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Utilities
    {
        public const string FUNCTION_CALL_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s?\([a-zA-Z0-9,\s]*\)\s?{?$";
        public const string FUNCTION_DECLARATION_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s?\(([a-zA-Z0-9,\s""']*)\)\s?{?$";

        public const string NEW_VAR_FUNCTION_CALL_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";
        public const string EXISTING_VAR_FUNCTION_CALL_REGEX = @"^([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";
        public const string VOID_FUNCTION_CALL_REGEX = @"^([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";

        public const string NEW_VAR_ASSIGNMENT_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+|""[^""]*"");?$";
        public const string EXISTING_VAR_ASSIGNMENT_REGEX = @"^([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+|""[^""]*"");?$";

        public const string IF_LOGIC_REGEX = @"^if\s?\(\s?([a-zA-Z0-9_]+|""[^""]*"")\s?(==|!=|<|<=|>|>=)\s?([a-zA-Z0-9_]+|""[^""]*"")\)";
        public const string IF_NOT_LOGIC_REGEX = @"^if\s?\(!(\w+)\)";
        public const string IF_TRUE_LOGIC_REGEX = @"^if\s?\((\w+)\)";

        public const string FOR_LOOP_REGEX = @"^for\s?\(int (\w+)\s?=\s?(\d+);\s?(\w+)\s?(<|<=|>|>=)\s?(\d+);\s?(\w+)(\D+)\)";
        public const string WHILE_LOOP_REGEX = @"^while\s?\(\s?(\w+)\s?(<|<=|>|>=)\s?(\d+)\s?\)";

        public static List<string> ExplodeAndClean(this string line, char delimiter)
        {
            line = line.Trim();
            List<string> pieces = line.Split(delimiter).ToList();
            return pieces;
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string text, string search)
        {
            int last = text.LastIndexOf(search);

            return text.Substring(0, last > -1 ? last : text.Count());
        }

        public static bool IsLocalVariable(this List<Variable> list, string check)
        {
            return list.Any(a => a.Value == check);
        }

        public static Variable GetLocalVariable(this List<Variable> list, string variable)
        {
            return list.Where(a => a.Value == variable).FirstOrDefault();
        }

        public static bool IsLogicCode(this Dictionary<string, List<string>> dict, string line)
        {
            return dict.Any(a => a.Value.Any(b => b == line));
        }

        public static Dictionary<string, List<string>> OrderBlocks(this Dictionary<string, List<string>> dict, Function function)
        {
            Dictionary<string, List<string>> finalBlocks = new Dictionary<string, List<string>>();

            var nestedBlocks = dict.Where(a => a.Key.Contains("nested")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            nestedBlocks = nestedBlocks.OrderByDescending(a => a.Key.GetNestedBlockParentIndex()).ThenBy(a => a.Key.GetNestedBlockIndex());
            nestedBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            var normalBlocks = dict.Where(a => !a.Key.Contains("nested") && !a.Key.Contains("for_loop")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            normalBlocks = normalBlocks.OrderBy(a => a.Key.GetNormalBlockIndex());
            normalBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            return finalBlocks;
        }

        public static bool IsFunction(this string line)
        {
            return Regex.IsMatch(line, FUNCTION_DECLARATION_REGEX);
        }

        public static FunctionDeclaration GetFunctionInfo(this string line)
        {
            List<string> matches = Regex.Matches(line, FUNCTION_DECLARATION_REGEX).GetRegexGroups();
            return new FunctionDeclaration(matches[0], matches[1], matches[2]);
        }

        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

        public static string GetDataType(this string value)
        {
            if (bool.TryParse(value, out bool btemp))
            {
                return "bool";
            }
            else if (float.TryParse(value, out float ftemp) && value.EndsWith("f"))
            {
                return "float";
            }
            else if (Regex.IsMatch(value, @"^""[^""]*""$"))
            {
                return "string";
            }
            else if (int.TryParse(value, out int itemp))
            {
                return "int";
            }
            return null;
        }

        public static List<Argument> GetListOfArguments(this string args)
        {
            var result = new List<Argument>();
            List<string> argsList = args.Split(',').ToList();
            if (string.IsNullOrEmpty(argsList[0]))
            {
                return result;
            }
            foreach (string arg in argsList)
            {
                Argument finalArg = new Argument()
                {
                    ValueType = arg.GetDataType(),
                    Value = arg.Replace(" ", "")
                };
                result.Add(finalArg);
            }

            return result;
        }

        public static AssignmentTypes IsFunctionCall(this string line)
        {
            //bool someVar = my_call();
            if (Regex.IsMatch(line, NEW_VAR_FUNCTION_CALL_REGEX))
            {
                return AssignmentTypes.NewVar;
            }
            //someExistingVar = my_call();
            if (Regex.IsMatch(line, EXISTING_VAR_FUNCTION_CALL_REGEX))
            {
                return AssignmentTypes.ExistingVar;
            }

            //my_call();
            if (Regex.IsMatch(line, VOID_FUNCTION_CALL_REGEX))
            {
                return AssignmentTypes.Void;
            }

            return AssignmentTypes.None;
        }

        public static FunctionCall GetFunctionCallInfo(this string line)
        {
            AssignmentTypes callType = line.IsFunctionCall();
            if (callType == AssignmentTypes.None)
            {
                throw new Exception("Trying to get function call info from a line of code that isn't a function call");
            }
            FunctionCall call = new FunctionCall();
            List<string> matches;
            switch (callType)
            {
                case AssignmentTypes.NewVar:
                    matches = Regex.Matches(line, NEW_VAR_FUNCTION_CALL_REGEX).GetRegexGroups();
                    call.HasReturnValue = true;
                    call.ReturnType = matches[0];
                    call.ReturnVariableName = matches[1];
                    call.FunctionName = matches[2];
                    call.Arguments = matches[3].GetListOfArguments();
                    break;
                case AssignmentTypes.ExistingVar:
                    matches = Regex.Matches(line, EXISTING_VAR_FUNCTION_CALL_REGEX).GetRegexGroups();
                    call.HasReturnValue = true;
                    call.ReturnType = null;
                    call.ReturnVariableName = matches[0];
                    call.FunctionName = matches[1];
                    call.Arguments = matches[2].GetListOfArguments();
                    break;
                case AssignmentTypes.Void:
                    matches = Regex.Matches(line, VOID_FUNCTION_CALL_REGEX).GetRegexGroups();
                    call.HasReturnValue = false;
                    call.ReturnType = null;
                    call.ReturnVariableName = null;
                    call.FunctionName = matches[0];
                    call.Arguments = matches[1].GetListOfArguments();
                    break;
            }
            return call;
        }

        public static AssignmentTypes IsAssignment(this string line)
        {
            //bool someVar = value;
            if (Regex.IsMatch(line, NEW_VAR_ASSIGNMENT_REGEX))
            {
                return AssignmentTypes.NewVar;
            }

            //someExistingVar = value;
            if (Regex.IsMatch(line, EXISTING_VAR_ASSIGNMENT_REGEX))
            {
                return AssignmentTypes.ExistingVar;
            }

            return AssignmentTypes.None;
        }

        public static Assignment GetAssignmentInfo(this string line)
        {
            AssignmentTypes callType = line.IsAssignment();
            if (callType == AssignmentTypes.None)
            {
                throw new Exception("Line was interpreted as an assignment, but assignment check failed");
            }
            Assignment assignment = new Assignment();
            List<string> matches;
            switch (callType)
            {
                case AssignmentTypes.NewVar:
                    matches = Regex.Matches(line, NEW_VAR_ASSIGNMENT_REGEX).GetRegexGroups();
                    assignment.AssignedVariable = matches[1];
                    assignment.AssignedValue = matches[2];
                    assignment.AssignedValueType = matches[2].GetDataType();
                    break;
                case AssignmentTypes.ExistingVar:
                    matches = Regex.Matches(line, EXISTING_VAR_ASSIGNMENT_REGEX).GetRegexGroups();
                    assignment.AssignedVariable = matches[0];
                    assignment.AssignedValue = matches[1];
                    assignment.AssignedValueType = matches[1].GetDataType();
                    break;
                default:
                    throw new Exception("Assignment type doesnt match one of the defined types");
            }
            return assignment;
        }

    }
}
