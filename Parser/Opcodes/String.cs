using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser.Opcodes
{
    public static class String
    {
        public static string Strcpy(int size = 64)
        {
            return $"strcpy {size}";
        }
        public static string Strcat(int size = 64)
        {
            return $"strcat {size}";
        }
    }
}
