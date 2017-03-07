namespace RAGE
{
    public class IfLogic : ControlLoopLogic
    {
        public string IteratorVariable { get; set; }

        public int InitialIteratorValue { get; set; }

        public ConditionalLogic Condition { get; set; }

        public ArithmeticTypes ArithmeticOperation { get; set; }
    }
}
