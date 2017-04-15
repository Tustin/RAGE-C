using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class Case
    {
        public int? Condition { get; set; }
        public string Label { get; set; }
        public bool Generated { get; set; }
        public bool IsDefault { get; set; }

        public Case(int? condition, string label, bool generated = false, bool isDefault = false)
        {
            Condition = condition;
            Label = label;
            Generated = generated;
            IsDefault = isDefault;
        }
        public Case()
        {

        }
    }
}
