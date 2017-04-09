using System;

namespace RAGE.Parser
{
    //VariableOffset - The beginning of the first item in the array offset by the amount of frame variables
    //Length - The amount of items in the array
    public class Array : IVariable
    {    
        public int VariableOffset { get; set; }

        public int Length { get; set; }
        public string Name { get; set; }
        public int FrameId { get; set; }
        public DataType Type { get; set; }
        public Specifier Specifier { get; set; }

        public Array(string name, int offset, int length)
        {
            Name = name;
            VariableOffset = offset;
            Length = length;
        }

        //public string Name { get; set; }

        //public List<Variable> Indices { get; set; }

        //public Array()
        //{
        //    Indices = new List<Variable>();
        //}

        //public Array(string name, int arrayLength)
        //{
        //    Name = name;
        //    Indices = new List<Variable>();
        //    for (int i = 0; i < arrayLength; i++)
        //    {
        //        var variable = new Variable($"name_{i}", i, "int");
        //        variable.Value.Value = Utilities.GetDefaultValue(DataType.Int);
        //        variable.Value.Type = DataType.Int;
        //        variable.Value.IsDefault = true;
        //        Indices.Add(variable);
        //    }
        //}
    }
}
