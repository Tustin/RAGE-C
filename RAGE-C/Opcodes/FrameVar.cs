namespace RAGE
{
    public static class FrameVar
    {
        public static string Get(Variable var)
        {
            return $"getF1 {var.FrameId}";
        }

        public static string Set(Variable var)
        {
            return $"setF1 {var.FrameId}";
        }

    }
}