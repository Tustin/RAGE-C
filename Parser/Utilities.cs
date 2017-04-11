using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using static RAGE.Logger.Logger;

namespace RAGE.Parser
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

        public static bool ContainVariable(this List<IVariable> variables, string name)
        {
            return variables.Any(a => a.Name == name);
        }

        public static IVariable GetVariable(this List<IVariable> variables, string name)
        {
            return variables.Where(a => a.Name == name).FirstOrDefault();
        }

        public static IVariable GetArray(this List<IVariable> variables, string name)
        {
            return variables.Where(a => a.Name == name && a is Array).FirstOrDefault();
        }

        public static KeyValuePair<string, List<string>> FindFunction(this Dictionary<string, List<string>> dictionary, string name)
        {
            return dictionary.Where(a => a.Key == name).FirstOrDefault();
        }

        public static DataType GetType(Function function, string value)
        {
            //Do type checking
            if (value.ToLower() == "true" || value.ToLower() == "false")
            {
                return DataType.Bool;
            }
            else if (Regex.IsMatch(value, "^(-)?[0-9]+$") || Regex.IsMatch(value, "^0x[0-9a-zA-Z]+$"))
            {
                return DataType.Int;
            }
            else if (Regex.IsMatch(value, "^[0-9.]+$"))
            {
                return DataType.Float;
            }
            else if (Regex.IsMatch(value, "^\".+[^\"\']\"$"))
            {
                return DataType.String;
            }
            else if (Regex.IsMatch(value, "^\\w+\\("))
            {
                string stripped = Regex.Replace(value, "\\(.*\\)", "");
                if (Native.IsFunctionANative(stripped))
                {
                    return DataType.NativeCall;
                }
                else if (Script.Functions.ContainFunction(stripped))
                {
                    if (Script.Functions.GetFunction(stripped).Type == DataType.Void)
                    {
                        Error($"Function {stripped} is void and does not return a value | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    }
                    return DataType.LocalCall;
                }

            }
            else if (Regex.IsMatch(value, "^\\w+"))
            {
                if (function == null) return DataType.Variable;

                if (function.Variables.ContainVariable(value))
                {
                    return DataType.Variable;
                }
                else if (value.StartsWith("Global_"))
                {
                    return DataType.Global;
                }
            }
            Error($"Unable to parse value '{value}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return DataType.Void;
        }

        public static DataType GetTypeFromDeclaration(string type)
        {
            switch (type)
            {
                case "bool":
                case "BOOL":
                return DataType.Bool;
                case "string":
                case "char*":
                return DataType.String;
                case "float":
                case "double":
                return DataType.Float;
                case "int":
                case "Entity":
                case "Player":
                case "FireId":
                case "Ped":
                case "Vehicle":
                case "Cam":
                case "CarGenerator":
                case "Group":
                case "Train":
                case "Pickup":
                case "Object":
                case "Weapon":
                case "Interior":
                case "Blip":
                case "Texture":
                case "TextureDict":
                case "CoverPoint":
                case "Camera":
                case "TaskSequence":
                case "ColourIndex":
                case "Sphere":
                case "ScrHandle":
                case "Any":
                case "uint":
                case "Hash":
                return DataType.Int;
                case "void":
                case "Void":
                return DataType.Void;
                default:
                if (Regex.IsMatch(type, "^\\w+\\("))
                {
                    string stripped = Regex.Replace(type, "\\(.*\\)", "");
                    if (Native.IsFunctionANative(stripped))
                    {
                        return DataType.NativeCall;
                    }
                    else if (Script.Functions.ContainFunction(stripped))
                    {
                        if (Script.Functions.GetFunction(stripped).Type == DataType.Void)
                        {
                            Error($"Function {stripped} is void and does not return a value | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        }
                        return DataType.LocalCall;
                    }
                }
                else if (Regex.IsMatch(type, "^\\w+"))
                {
                    return DataType.Variable;
                }
                Error($"Unsupported type '{type}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return DataType.Void;
            }
        }

        public static Specifier GetSpecifierFromDeclaration(string spec)
        {
            switch (spec)
            {
                case "global":
                return Specifier.Global;
                case "static":
                return Specifier.Static;
                case "auto":
                return Specifier.Auto;
                default:
                Error($"Invalid specifier '{spec}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return Specifier.None;
            }
        }

        public static string GetDefaultValue(DataType type)
        {
            switch (type)
            {
                case DataType.Int:
                return "0";
                case DataType.Float:
                return "0.0";
                case DataType.String:
                return "";
                case DataType.Bool:
                return "false";
                default:
                Error($"No default value is defined for type '{type}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return null;
            }
        }
        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

        public static bool ContainsEnum(this List<Enum> enums, string name)
        {
            return enums.Any(a => a.Name == name);
        }

        public static Enum GetEnum(this List<Enum> enums, string name)
        {
            return enums.Where(a => a.Name == name).FirstOrDefault();
        }

        public static bool ContainsEnumerator(this List<Enumerator> enumerators, string name)
        {
            return enumerators.Any(a => a.Name == name);
        }

    }
}
