namespace RAGE
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

    }
}