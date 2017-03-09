using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
