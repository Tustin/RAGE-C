namespace RAGE.Parser
{
    public class Parameter
    {
        public DataType Type { get; set; }

        public string Name { get; set; }

        public Parameter(DataType type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}