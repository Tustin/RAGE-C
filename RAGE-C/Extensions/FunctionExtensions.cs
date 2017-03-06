using System.Collections.Generic;
using System.Linq;
namespace RAGE
{
    public static class FunctionExtensions
    {
        public static bool ContainsFunction(this List<Function> functions, string name)
        {
            return functions.Any(a => a.Name == name);
        }
    }
}
