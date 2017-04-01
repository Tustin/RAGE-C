namespace RAGE.Parser
{
    public static class FrameVar
    {
        public static string Get(Variable var)
        {
            return $"getF1 {var.FrameId} //{var.Name}";
        }

        public static string Set(Variable var)
        {
            return $"setF1 {var.FrameId} //{var.Name}";
        }

        public static string GetPointer(Variable var)
        {
            return $"pFrame1 {var.FrameId} //&{var.Name}";
        }

    }
}