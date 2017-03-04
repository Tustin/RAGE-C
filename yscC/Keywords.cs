using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
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
            Regex regex;
            result = null;
            foreach (string keyword in Keywords)
            {
                regex = new Regex($@"^{keyword}\s?\(?");
                if (regex.IsMatch(check))
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
