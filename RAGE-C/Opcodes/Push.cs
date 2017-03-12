using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Push
    {
        public static string Float(string value)
        {
            if (float.TryParse(value, out float val))
            {
                if (val >= -1.0f && val <= 7.0f)
                {
                    return $"fpush_{val}";
                }
                else
                {
                    return $"fpush {val}";
                }
            }
            else
            {
                throw new Exception("Assumed float, but unable to parse");
            }
        }

        public static string Bool(string value)
        {
            if (value == "true" || value == "false")
            {
                return value == "true" ? "push_1" : "push_0";
            }
            else
            {
                throw new Exception("Assumed bool, but unable to parse");
            }
        }

        public static string String(string value)
        {
            Regex regex = new Regex(@"^""[^""]*""$");
            if (!regex.IsMatch(value))
            {
                throw new Exception("Assumed string, but unable to parse");
            }
            return $"PushString {value}";
        }

        public static string Int(string value)
        {
            if (int.TryParse(value, out int ival))
            {
                if (ival >= -1 && ival <= 7)
                {
                    return $"push_{ival}";
                }
                else if (ival <= 255 && ival >= -255)
                {
                    return $"push1 {ival}";
                } //short (16 bits)
                else if (ival > 255 && ival <= Int16.MaxValue)
                {
                    return $"pushS {ival}";
                } //24 bits (why rockstar?)
                else if (ival > Int16.MaxValue && ival <= 16777215)
                {
                    return $"pushI24 {ival}";
                }
                else
                {
                    return $"push {ival}";
                }
            }
            else
            {
                throw new Exception("Assumed int, but unable to parse");
            }
        }

        public static string Generate(string value, VariableTypes valueType)
        {
            switch (valueType)
            {
                case VariableTypes.Bool:
                    return Bool(value);
                case VariableTypes.Float:
                    return Float(value);
                case VariableTypes.String:
                    return String(value);
                case VariableTypes.Int:
                    return Int(value);
                default:
                    return null;
            }
        }
    }
}