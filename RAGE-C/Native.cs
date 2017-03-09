using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGE
{
    public class Native
    {
        public static Dictionary<string, string> Natives = new Dictionary<string, string>();

        public static void PopulateNativeTable()
        {
            List<string> natives = File.ReadAllLines(Core.PROJECT_ROOT + "\\Resources\\natives.dat").ToList();

            foreach (string line in natives)
            {
                List<string> items = line.Split(':').ToList();
                Natives.Add(items[0], items[2]);
            }
        }

        public static bool IsFunctionANative(string functionName)
        {
            return Natives.Any(n => n.Value == functionName || n.Key == functionName);
        }
    }
}
