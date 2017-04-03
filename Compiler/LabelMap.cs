namespace RAGE.Compiler
{
    public class LabelMap
    {
        public string Label { get; set; }
        public int ByteLocation { get; set; }
        public LabelType Type { get; set; }

        public LabelMap(string label, int location, LabelType type)
        {
            Label = label;
            ByteLocation = location;
            Type = type;

        }
    }
}
