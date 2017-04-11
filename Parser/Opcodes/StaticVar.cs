namespace RAGE.Parser.Opcodes
{
    public class StaticVar
    {
        public static string Get(IVariable var)
        {
            return $"getStatic1 {var.FrameId} //{var.Name}";
        }

        public static string Set(IVariable var)
        {
            return $"setStatic1 {var.FrameId} //{var.Name}";
        }

        public static string GetPointer(IVariable var)
        {
            return $"pStatic1 {var.FrameId} //&{var.Name}";
        }
    }
}
