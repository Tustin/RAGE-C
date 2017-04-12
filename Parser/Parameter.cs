using System;

namespace RAGE.Parser
{
    public class Parameter : IVariable
    {
        public DataType Type { get; set; }

        public string Name { get; set; }

        public int FrameId { get; set; }

        //Parms wont ever have a specifier
        public Specifier Specifier
        {
            get
            {
                return Specifier.None;
            }
            set
            {

            }
        }

        public Parameter(DataType type, string name, int frameId)
        {
            Type = type;
            Name = name;
            FrameId = frameId;
        }
    }
}