namespace RAGE.Parser
{
    public class NativeParameter
    {
        private DataType _type;
        public DataType DataType
        {
            get
            {
                return Utilities.GetTypeFromDeclaration(Type);
            }
            set
            {
                _type = Utilities.GetTypeFromDeclaration(Type);
            }
        }

        public string Type { get; set; }

        public string Name { get; set; }
    }
}
