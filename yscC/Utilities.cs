﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class Utilities
    {
        public static List<string> ExplodeAndClean(this string line, char delimiter)
        {
            List<string> pieces = line.Split(delimiter).ToList();
            pieces = pieces.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
            return pieces;
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        public static string ReplaceLast(this string text, string search)
        {
            int last = text.LastIndexOf(search);

            return text.Substring(0, last > -1 ? last : text.Count());
        }

        public static bool IsLocalVariable(this List<Variable> list, string check)
        {
            return list.Any(a => a.Value == check);
        }

        public static Variable GetLocalVariable(this List<Variable> list, string variable)
        {
            return list.Where(a => a.Value == variable).FirstOrDefault();
        }

        public static bool IsLogicCode(this Dictionary<string, List<string>> dict, string line)
        {
            return dict.Any(a => a.Value.Any(b => b == line));
        }

        public static bool AreThereAnyParentConditionals(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional);
        }

        public static Conditional GetLastParentConditional(this List<Conditional> list)
        {
            return list.Where(a => a.Parent == null).FirstOrDefault();
        }

        public static Conditional GetLastParentConditional(this List<Conditional> list, Conditional excludedConditional)
        {
            return list.Where(a => a.Parent == null && a != excludedConditional).FirstOrDefault();
        }

        public static Conditional GetLastConditional(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Where(a => a.Index == currentConditional.Index - 1).FirstOrDefault();
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

        public static bool AreThereAnyParentsAfterThisParent(this List<Conditional> list, Conditional currentConditional)
        {
            return list.Any(a => a.Parent == null && a != currentConditional && a.Index > currentConditional.Index);
        }

        public static string FindConditionalBlockForCode(this Dictionary<string, List<string>> dict, string line)
        {
            return dict.Where(a => a.Value.Any(b => b == line)).FirstOrDefault().Key;
        }

        public static int GetNestedBlockIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("nested") + 1]);
        }
        public static int GetNestedBlockNormalIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (!pieces.Contains("nested"))
            {
                throw new Exception("String doesn't contain a nested label");
            }
            return int.Parse(pieces[pieces.IndexOf("end") + 1]);
        }
        public static int GetNormalBlockIndex(this string nestedConditional)
        {
            List<string> pieces = nestedConditional.Split('_').ToList();
            if (pieces.Contains("nested"))
            {
                throw new Exception("Label contains 'nested' but is not an actual nested conditional block. Please try a different function name.");
            }
            return int.Parse(pieces[pieces.IndexOf("end") + 1]);
        }

        public static Dictionary<string, List<string>> OrderBlocks(this Dictionary<string, List<string>> dict, Function function)
        {
            Dictionary<string, List<string>> finalBlocks = new Dictionary<string, List<string>>();

            var nestedBlocks = dict.Where(a => a.Key.Contains("nested")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            nestedBlocks = nestedBlocks.OrderByDescending(a => a.Key.GetNestedBlockNormalIndex());
            nestedBlocks = nestedBlocks.Reverse();
            nestedBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            var normalBlocks = dict.Where(a => !a.Key.Contains("nested")).Select(a => new KeyValuePair<string, List<string>>(a.Key, a.Value));
            normalBlocks = normalBlocks.OrderBy(a => a.Key.GetNormalBlockIndex());
            normalBlocks.ToList().ForEach(a => finalBlocks.Add(a.Key, a.Value));

            return finalBlocks;

        }
    }
}
