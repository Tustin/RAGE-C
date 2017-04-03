using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Case
    {
        public int Condition { get; set; }
        public string Label { get; set; }
        public bool Generated { get; set; }

        public Case(int condition, string label, bool generated = false)
        {
            Condition = condition;
            Label = label;
            Generated = generated;
        }
    }
}
