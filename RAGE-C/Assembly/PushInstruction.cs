using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public static class PushInstruction
    {
        public static string Float(string value)
        {
            float val;
            if (float.TryParse(value, out val))
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
            int ival;
            if (int.TryParse(value, out ival))
            {
                if (ival >= -1 && ival <= 7)
                {
                    return $"push_{ival}";
                }
                else if (ival <= 255 && ival >= -255)
                {
                    return $"push1 {ival}";
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
    }
}
