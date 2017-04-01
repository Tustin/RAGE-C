using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
                else if (Core.Functions.ContainFunction(stripped))
                {
                    if (Core.Functions.GetFunction(stripped).Type == DataType.Void)
                    {
                        throw new Exception($"Function {stripped} is void and does not return a value");
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
            }
            throw new Exception($"Unable to parse value '{value}'");
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
                //@TODO: Add GTA related types (Ped, Entity, Hash, etc) (would just be ints)
                default:
                    throw new Exception("Unsupported variable type");
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
                    throw new Exception($"No default value is defined for type {type}");
            }
        }
        public static List<string> GetRegexGroups(this MatchCollection collection)
        {
            return collection[0].Groups.Cast<Group>().Skip(1).Select(a => a.Value).ToList();
        }

    }
}
