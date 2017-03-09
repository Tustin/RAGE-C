using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Function
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public List<string> Code { get; set; }
        public List<Variable> LocalVariables = new List<Variable>();
        public List<Argument> Arguments { get; set; }
        public bool HasReturnValue { get; set; }
        public int frameCount = 2;
        public List<Conditional> Conditionals = new List<Conditional>();
        public List<ControlLoop> Loops = new List<ControlLoop>();

        public Function()
        {
            Code = new List<string>();
        }

        public int GetIndexOfLastUnclosedBlock<T>()
        {
            Type type = typeof(T);
            if (type == typeof(Conditional))
            {
                return Conditionals.FindLastIndex(a => a.CodeEndLine == null);
            }
            else
            {
                return Loops.FindLastIndex(a => a.CodeEndLine == null);
            }
        }

        public bool AreThereAnyUnclosedBlocks<T>()
        {
            Type type = typeof(T);
            if (type == typeof(Conditional))
            {
                return Conditionals.Any(a => a.CodeEndLine == null);
            }
            else
            {
                return Loops.Any(a => a.CodeEndLine == null);
            }
        }


        public T GetLast<T>()
        {
            Type type = typeof(T);
            if (type == typeof(Conditional))
            {
                return (T)Convert.ChangeType(Conditionals.Last(), typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(Loops.Last(), typeof(T));
            }
        }
        public T GetLastParent<T>()
        {
            Type type = typeof(T);
            if (type == typeof(Conditional))
            {
                return (T)Convert.ChangeType(Conditionals.Where(a => a.Parent == null).FirstOrDefault(), typeof(T));
            }
            else
            {
                return (T)Convert.ChangeType(Loops.Where(a => a.LoopParent == null).FirstOrDefault(), typeof(T));
            }
        }


    }
}
