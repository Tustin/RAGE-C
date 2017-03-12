﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Utilities
    {
        public static List<string> ExplodeAndClean(this string line, char delimiter)
        {
            line = line.Trim();
            List<string> pieces = line.Split(delimiter).ToList();
            return pieces;
        }

        public static bool ContainFunction(this List<Function> functions, string name)
        {
            return functions.Any(a => a.Name == name);
        }

        public static Function GetFunction(this List<Function> functions, string name)
        {
            return functions.Where(a => a.Name == name).FirstOrDefault();
        }

        public static bool ContainVariable(this List<Variable> variables, string name)
        {
            return variables.Any(a => a.Name == name);
        }

        public static Variable GetVariable(this List<Variable> variables, string name)
        {
            return variables.Where(a => a.Name == name).FirstOrDefault();
        }

        public static KeyValuePair<string, List<string>> FindFunction(this Dictionary<string, List<string>> dictionary, string name)
        {
            return dictionary.Where(a => a.Key == name).FirstOrDefault();
        }

        public static VariableTypes GetType(Function function, string value)
        {
            //Do type checking
            if (value == "true" || value == "false")
            {
                return VariableTypes.Bool;
            }
            else if (Regex.IsMatch(value, "^[0-9]+$"))
            {
                return VariableTypes.Int;
            }
            else if (Regex.IsMatch(value, "^[0-9.]+$"))
            {
                return VariableTypes.Float;
            }
            else if (Regex.IsMatch(value, "^\".+[^\"\']\"$"))
            {
                return VariableTypes.String;
            }
            else if (Regex.IsMatch(value, "^\\w+\\("))
            {
                string stripped = Regex.Replace(value, "\\(.*\\)", "");
                if (Native.IsFunctionANative(stripped))
                {
                    return VariableTypes.NativeCall;
                }
                else if (Core.Functions.ContainFunction(stripped))
                {
                    return VariableTypes.LocalCall;
                }

            }
            else if (Regex.IsMatch(value, "^\\w+"))
            {
                if (function == null) return VariableTypes.Variable;

                if (function.Variables.ContainVariable(value))
                {
                    return VariableTypes.Variable;
                }
            }
            throw new Exception($"Unable to parse value '{value}'");
        }

        public static List<Argument> GetListOfArguments(string args)
        {
            var result = new List<Argument>();

            List<string> argsList = args.Split(',').ToList();

            if (string.IsNullOrEmpty(argsList[0])) return result;

            foreach (string arg in argsList)
            {
                Argument finalArg = new Argument()
                {
                    Type = GetType(null, arg),
                    Value = arg.Replace(" ", "")
                };
                result.Add(finalArg);
            }

            return result;
        }
        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

    }
}
