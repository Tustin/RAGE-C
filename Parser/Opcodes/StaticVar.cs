namespace RAGE.Parser.Opcodes
{
    public class StaticVar
    {
        public static string Get(IVariable var)
        {
            return $"StaticGet1 {var.FrameId} //{var.Name}"; //change
        }

        public static string Set(IVariable var)
        {
            return $"StaticSet1 {var.FrameId} //{var.Name}"; //change
        }

        public static string GetPointer(IVariable var)
        {
            return $"pStatic1 {var.FrameId} //&{var.Name}";
        }
    }
}
