using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public static class LoopExtensions
    {
        public static ControlLoop GetNextLoop(this List<ControlLoop> list, ControlLoop currentLoop)
        {
            return list.Where(a => a.Index == currentLoop.Index + 1).FirstOrDefault();
        }

        public static bool AreThereAnyParentLoops(this List<ControlLoop> list, ControlLoop currentLoop)
        {
            return list.Any(a => a.Parent == null && a != currentLoop);
        }
        public static ControlLoop GetLastLoop(this List<ControlLoop> list, ControlLoop currentLoop)
        {
            return list.Where(a => a.Index == currentLoop.Index - 1).FirstOrDefault();
        }

        public static ControlLoop GetLastParentLoop(this List<ControlLoop> list)
        {
            return list.Where(a => a.Parent == null).FirstOrDefault();
        }

        public static ControlLoop GetLastParentLoop(this List<ControlLoop> list, ControlLoop excludedLoop)
        {
            return list.Where(a => a.Parent == null && a != excludedLoop).FirstOrDefault();
        }

        public static bool AreThereAnyParentLoopsAfterThisParent(this List<ControlLoop> list, ControlLoop currentLoop)
        {
            return list.Any(a => a.Parent == null && a != currentLoop && a.Index > currentLoop.Index);
        }

        public static ControlLoop GetNextParentLoop(this List<ControlLoop> list, ControlLoop omittedLoop)
        {
            return list.Where(a => a.Parent == null && a != omittedLoop && a.Index > omittedLoop.Index).FirstOrDefault();
        }
    }
}
