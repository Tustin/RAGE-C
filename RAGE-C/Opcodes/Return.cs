using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Return
    {
        //@Update: Give this some options for returning values
        public static string Generate(int argumentCount = 0, bool isReturning = false)
        {
            return $"Return {argumentCount} {Convert.ToInt32(isReturning)}";
        }
    }
}
