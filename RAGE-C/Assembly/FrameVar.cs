using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class FrameVar
    {
        /// <summary>
        /// Generates the get frame var instruction.
        /// </summary>
        /// <param name="var">Instance of the variable</param>
        /// <returns></returns>
        public static string Get(Variable var)
        {
            return $"getF1 {var.FrameId}";
        }

        /// <summary>
        /// Generates the get frame var instruction.
        /// </summary>
        /// <param name="function">Function where variable is used</param>
        /// <param name="var">Name of the variable</param>
        /// <returns></returns>
        public static string Get(Function function, string var)
        {
            return Get(function.LocalVariables.GetLocalVariable(var));
        }


        public static string Set(Variable var)
        {
            return $"setF1 {var.FrameId}";
        }

        public static string Set(Function function, string var)
        {
            return Set(function.LocalVariables.GetLocalVariable(var));
        }

    }
}
