using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class StaticVar
    {
        public static string Get(Variable var)
        {
            return $"getStatic1 {var.FrameId} //{var.Name}";
        }

        public static string Set(Variable var)
        {
            return $"setStatic1 {var.FrameId} //{var.Name}";
        }

        public static string GetPointer(Variable var)
        {
            return $"pStatic1 {var.FrameId} //&{var.Name}";
        }
    }
}
