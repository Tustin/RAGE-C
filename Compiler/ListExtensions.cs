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
    }
}
