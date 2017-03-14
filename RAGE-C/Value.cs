﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    /// <summary>
    /// Wrapper for values returned in the visitor.
    /// </summary>
    public class Value
    {
        internal VariableType Type { get; set; }
        internal object Data { get; set; }
        internal List<string> Assembly { get; set; }
        //This is if the value inside a variable is a const and can be equated by the compiler
        //Store the original variable here so we can reference if necessary
        internal Variable OriginalVariable { get; set; }


        public Value(VariableType type, object data, List<string> asm, Variable original = null) : base()
        {
            Type = type;
            Data = data;
            Assembly = asm;
            if (original != null)
            {
                OriginalVariable = original;
            }
        }
        public Value()
        {
            Assembly = new List<string>();
        }
    }
}
