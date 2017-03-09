using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class ConditionalExtensions
    {
        public static bool AreThereAnyParentConditionals(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional);
        }

        public static Conditional GetLastParentConditional(this List<Conditional> list)
        {
            return list.Where(a => a.Parent == null).FirstOrDefault();
        }
        public static Conditional GetLastUnclosedParentConditional(this List<Conditional> list)
        {
            return list.Where(a => a.Parent == null && a.CodeEndLine == null).FirstOrDefault();
        }
        public static Conditional GetLastParentConditional(this List<Conditional> list, Conditional excludedConditional)
        {
            return list.Where(a => a.Parent == null && a != excludedConditional).FirstOrDefault();
        }

        public static Conditional GetLastConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index == currentConditional.Index - 1).FirstOrDefault();
        }

        public static Conditional GetLastConditional(this List<Conditional> list)
        {
            return list.Last();
        }

        public static Conditional GetNextConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index == currentConditional.Index + 1).FirstOrDefault();
        }

        public static Conditional GetNextConditionalWithSameParent(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Parent == currentConditional.Parent && a.Index > currentConditional.Index).FirstOrDefault();
        }

        public static bool DoesConditionalHaveChildren(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == currentConditional);
        }

        public static Conditional GetNextNonParentConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index > currentConditional.Index && a.Parent != null).FirstOrDefault();
        }

        public static Conditional GetNextParentConditional(this List<Conditional> list, Conditional omittedConditional)
        {
            return list.Where(a => a.Parent == null && a != omittedConditional && a.Index > omittedConditional.Index).FirstOrDefault();
        }

        public static bool AreThereAnyParentConditionalsAfterThisParent(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional && a.Index > currentConditional.Index);
        }

        public static string FindBlockForCode(this Dictionary<string, List<string>> dict, string line)
        {
            var results = dict.Where(a => a.Value.Any(b => b == line));
            if (results.Count() > 1)
            {
                int gg = 1;
            }
            return results.LastOrDefault().Key;

        }

        public static int GetNestedBlockIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("end") + 1]);
        }

        public static int GetNestedBlockParentIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("nested") + 1]);
        }

        public static int GetNormalBlockIndex(this string conditional)
        {
            List<string> pieces = conditional.Split('_').ToList();
            return int.Parse(pieces[pieces.Count - 1]);
        }
    }
}
