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

        public const string NEW_VAR_FUNCTION_CALL_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";
        public const string EXISTING_VAR_FUNCTION_CALL_REGEX = @"^([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";
        public const string VOID_FUNCTION_CALL_REGEX = @"^([a-zA-Z0-9_]+)\(([a-zA-Z0-9,\s""']*)\);?$";

        public const string NEW_VAR_ASSIGNMENT_REGEX = @"^([a-z]+)\s([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+|""[^""]*"");?$";
        public const string EXISTING_VAR_ASSIGNMENT_REGEX = @"^([a-zA-Z0-9_]+)\s=\s([a-zA-Z0-9_]+|""[^""]*"");?$";

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

        public static bool AreThereAnyParentConditionals(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional);
        }

        public static Conditional GetLastParentConditional(this List<Conditional> list)
        {
            return list.Where(a => a.Parent == null).FirstOrDefault();
        }

        public static Conditional GetLastParentConditional(this List<Conditional> list, Conditional excludedConditional)
        {
            return list.Where(a => a.Parent == null && a != excludedConditional).FirstOrDefault();
        }

        public static Conditional GetLastConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index == currentConditional.Index - 1).FirstOrDefault();
        }

        public static Conditional GetNextConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index == currentConditional.Index + 1).FirstOrDefault();
        }

        public static Conditional GetNextConditionalWithSameParent(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Parent == currentConditional.Parent && a.Index > currentConditional.Index).FirstOrDefault();
        }

        public static bool DoesConditionalHaveChildren(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == currentConditional);
        }

        public static Conditional GetNextNonParentConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index > currentConditional.Index && a.Parent != null).FirstOrDefault();
        }

        public static Conditional GetNextParentConditional(this List<Conditional> list, Conditional omittedConditional)
        {
            return list.Where(a => a.Parent == null && a != omittedConditional && a.Index > omittedConditional.Index).FirstOrDefault();
        }

        public static bool AreThereAnyParentsAfterThisParent(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional && a.Index > currentConditional.Index);
        }

        public static string FindConditionalBlockForCode(this Dictionary<string, List<string>> dict, string line)
        {
            return dict.Where(a => a.Value.Any(b => b == line)).FirstOrDefault().Key;
        }

        public static int GetNestedBlockIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("end") + 1]);
        }

        public static int GetNestedBlockParentIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("nested") + 1]);
        }

        public static int GetNormalBlockIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (pieces.Contains("nested"))
            {
                throw new Exception("Label contains 'nested' but is not an actual nested conditional block. Please try a different function name.");
            }
            return int.Parse(pieces[pieces.IndexOf("end") + 1]);
        }

        public static Dictionary<string, List<string>> OrderBlocks(this Dictionary<string, List<string>> dict, Function function)
        {
            Dictionary<string, List<string>> finalBlocks = new Dictionary<string, List<string>>();

            var nestedBlocks = dict.Where(a => a.Key.Contains("nested")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            nestedBlocks = nestedBlocks.OrderByDescending(a => a.Key.GetNestedBlockParentIndex()).ThenBy(a => a.Key.GetNestedBlockIndex());
            nestedBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            var normalBlocks = dict.Where(a => !a.Key.Contains("nested")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            normalBlocks = normalBlocks.OrderBy(a => a.Key.GetNormalBlockIndex());
            normalBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            return finalBlocks;
        }

        public static bool IsFunction(this string line)
        {
            Regex r = new Regex(FUNCTION_CALL_REGEX);
            return r.IsMatch(line);
        }

        public static List<string> GetFunctionInfo(this string line)
        {
            Regex r = new Regex(FUNCTION_CALL_REGEX);
            return r.Matches(line).GetRegexGroups();
        }

        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

        public static string GetDataType(this string value)
        {
            int itemp;
            bool btemp;
            float ftemp;
            Regex regex = new Regex(@"^""[^""]*""$");
            if (bool.TryParse(value, out btemp))
            {
                return "bool";
            }
            else if (float.TryParse(value, out ftemp))
            {
                return "float";
            }
            else if (regex.IsMatch(value))
            {
               return "string";
            }
            else if (int.TryParse(value, out itemp))
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
                Argument finalArg = new Argument();
                finalArg.ValueType = arg.GetDataType();
                finalArg.Value = arg.Replace(" ", "");
                result.Add(finalArg);
            }

            return result;     
        }

        public static AssignmentTypes IsFunctionCall(this string line)
        {
            //bool someVar = my_call();
            Regex functionCallRegex = new Regex(NEW_VAR_FUNCTION_CALL_REGEX);
            if (functionCallRegex.IsMatch(line))
            {
                return AssignmentTypes.NewVar;
            }
            //someExistingVar = my_call();
            functionCallRegex = new Regex(EXISTING_VAR_FUNCTION_CALL_REGEX);
            if (functionCallRegex.IsMatch(line))
            {
                return AssignmentTypes.ExistingVar;
            }

            //my_call();
            functionCallRegex = new Regex(VOID_FUNCTION_CALL_REGEX);
            if (functionCallRegex.IsMatch(line))
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
            Regex regex;
            List<string> matches;
            switch (callType)
            {
                case AssignmentTypes.NewVar:
                regex = new Regex(NEW_VAR_FUNCTION_CALL_REGEX);
                matches = regex.Matches(line).GetRegexGroups();
                call.HasReturnValue = true;
                call.ReturnType = matches[0];
                call.ReturnVariableName = matches[1];
                call.FunctionName = matches[2];
                call.Arguments = matches[3].GetListOfArguments();
                break;
                case AssignmentTypes.ExistingVar:
                regex = new Regex(EXISTING_VAR_FUNCTION_CALL_REGEX);
                matches = regex.Matches(line).GetRegexGroups();
                call.HasReturnValue = true;
                call.ReturnType = null;
                call.ReturnVariableName = matches[0];
                call.FunctionName = matches[1];
                call.Arguments = matches[2].GetListOfArguments();
                break;
                case AssignmentTypes.Void:
                regex = new Regex(VOID_FUNCTION_CALL_REGEX);
                matches = regex.Matches(line).GetRegexGroups();
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
            Regex functionCallRegex = new Regex(NEW_VAR_ASSIGNMENT_REGEX);
            if (functionCallRegex.IsMatch(line))
            {
                return AssignmentTypes.NewVar;
            }

            //someExistingVar = value;
            functionCallRegex = new Regex(EXISTING_VAR_ASSIGNMENT_REGEX);
            if (functionCallRegex.IsMatch(line))
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
            Regex regex;
            List<string> matches;
            switch (callType)
            {
                case AssignmentTypes.NewVar:
                regex = new Regex(NEW_VAR_ASSIGNMENT_REGEX);
                matches = regex.Matches(line).GetRegexGroups();
                assignment.AssignedVariable = matches[1];
                assignment.AssignedValue = matches[2];
                assignment.AssignedValueType = matches[2].GetDataType();
                break;
                case AssignmentTypes.ExistingVar:
                regex = new Regex(EXISTING_VAR_ASSIGNMENT_REGEX);
                matches = regex.Matches(line).GetRegexGroups();
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
