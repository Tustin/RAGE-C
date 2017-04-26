using System;
using System.Collections.Generic;

namespace RAGE.Parser
{
    public class Array : IVariable
    {
        /// <summary>
        /// The beginning of the first item in the array offset by the amount of frame variables
        /// </summary>
        public int VariableOffset { get; set; }
        /// <summary>
        /// The amount of items in the array
        /// </summary>
        /// 
        public List<Variable> Indices { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
        public int FrameId { get; set; }
        public DataType Type { get; set; }
        public Specifier Specifier { get; set; }

        public Array(string name, int offset, int length, DataType type)
        {
            Name = name;
            VariableOffset = offset;
            Length = length;
            Indices = new List<Variable>();
			Type = type;
        }
    }
}
