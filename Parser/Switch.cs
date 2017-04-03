using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public class Switch
    {
        // public List<string> Labels { get; set; }
        //Contains the case condition (int) and the label to jump to (string)
        public Dictionary<int, string> Labels { get; set; }

        public Switch()
        {
            Labels = new Dictionary<int, string>();
        }
    }
}
