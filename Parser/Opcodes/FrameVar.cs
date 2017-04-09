namespace RAGE.Parser.Opcodes
{
    public static class FrameVar
    {
        public static string Get(IVariable var)
        {
            return $"getF1 {var.FrameId} //{var.Name}";
        }

        public static string Set(IVariable var)
        {
            return $"setF1 {var.FrameId} //{var.Name}";
        }

        public static string GetPointer(IVariable var)
        {
            return $"pFrame1 {var.FrameId} //&{var.Name}";
        }

    }
}