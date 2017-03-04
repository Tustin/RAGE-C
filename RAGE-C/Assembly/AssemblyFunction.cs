using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class AssemblyFunction
    {
        public string Label { get; set; }
        public Dictionary<string, List<string>> LabelBlocks = new Dictionary<string, List<string>>();

        public AssemblyFunction(string label)
        {
            Label = label;
            //make a root block to write main code into
            LabelBlocks.Add(label, new List<string>());
        }

        public void PopulateBlocks(List<string> labels)
        {
            labels.ForEach(a => LabelBlocks.Add(a, new List<string>()));
        }
    }
}
