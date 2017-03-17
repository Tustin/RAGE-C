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

        public static VariableType GetType(Function function, string value)
        {
            //Do type checking
            if (value.ToLower() == "true" || value.ToLower() == "false")
            {
                return VariableType.Bool;
            }
            else if (Regex.IsMatch(value, "^(-)?[0-9]+$") || Regex.IsMatch(value, "^0x[0-9a-zA-Z]+$"))
            {
                return VariableType.Int;
            }
            else if (Regex.IsMatch(value, "^[0-9.]+$"))
            {
                return VariableType.Float;
            }
            else if (Regex.IsMatch(value, "^\".+[^\"\']\"$"))
            {
                return VariableType.String;
            }
            else if (Regex.IsMatch(value, "^\\w+\\("))
            {
                string stripped = Regex.Replace(value, "\\(.*\\)", "");
                if (Native.IsFunctionANative(stripped))
                {
                    return VariableType.NativeCall;
                }
                else if (Core.Functions.ContainFunction(stripped))
                {
                    if (Core.Functions.GetFunction(stripped).Type == VariableType.Void)
                    {
                        throw new Exception($"Function {stripped} is void and does not return a value");
                    }
                    return VariableType.LocalCall;
                }

            }
            else if (Regex.IsMatch(value, "^\\w+"))
            {
                if (function == null) return VariableType.Variable;

                if (function.Variables.ContainVariable(value))
                {
                    return VariableType.Variable;
                }
            }
            throw new Exception($"Unable to parse value '{value}'");
        }

        public static VariableType GetTypeFromDeclaration(string type)
        {
            switch (type)
            {
                case "bool":
                    return VariableType.Bool;
                case "string":
                    return VariableType.String;
                case "float":
                    return VariableType.Float;
                case "int":
                    return VariableType.Int;
                case "void":
                    return VariableType.Void;
                //@TODO: Add GTA related types (Ped, Entity, Hash, etc) (would just be ints)
                default:
                    throw new Exception("Unsupported variable type");
            }
        }

        public static string GetDefaultValue(VariableType type)
        {
            switch (type)
            {
                case VariableType.Int:
                    return "0";
                case VariableType.Float:
                    return "0.0";
                case VariableType.String:
                    return "";
                case VariableType.Bool:
                    return "false";
                default:
                    throw new Exception($"No default value is defined for type {type}");
            }
        }
        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

    }
}
