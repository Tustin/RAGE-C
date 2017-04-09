namespace RAGE.Parser
{
    public interface IVariable
    {
        string Name { get; set; }

        int FrameId { get; set; }

        DataType Type { get; set; }

        Specifier Specifier { get; set; }


    }
}