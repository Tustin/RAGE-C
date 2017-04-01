using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RAGE.Parser
{
    //C keywords which cannot be used in regular code
    public class Keyword
    {
        static List<string> Keywords = new List<string>()
        {
           "if",
           "while",
           "do",
           "else",
           "for",
           "return",
           "switch",
        };
        public static bool IsMatch(string check, out string result)
        {
            result = null;
            foreach (string keyword in Keywords)
            {
                if (Regex.IsMatch(check, $@"^{keyword}\s?\(?"))
                {
                    result = keyword;
                    return true;
                }
                continue;
            }
            return false;
        }
    }
}