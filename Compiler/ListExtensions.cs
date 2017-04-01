using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Compiler
{
    internal static class ListExtensions
    {
        internal static NativeData GetNativeSection(this List<NativeData> data, string native)
        {
            return data.Where(a => a.Native == native).FirstOrDefault();
        }

        internal static StringData GetStringSection(this List<StringData> data, string s)
        {
            return data.Where(a => a.StringLiteral == s).FirstOrDefault();
        }

        internal static StringData GetStringSection(this List<StringData> data, List<byte> storage)
        {
            return data.Where(a => a.StringStorage == storage).FirstOrDefault();
        }

        internal static LabelData GetLabelSection(this List<LabelData> data, string label)
        {
            return data.Where(a => a.Label == label).FirstOrDefault();
        }

        internal static void AddOrUpdate(this Dictionary<string, List<int>> data, string key, int val)
        {
            if (!data.TryGetValue(key, out List<int> offsets))
            {
                offsets = new List<int>();
            }
            offsets.Add(val);
            data[key] = offsets;
        }
    }
}
