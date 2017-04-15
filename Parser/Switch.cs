using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class Switch
    {
        // public List<string> Labels { get; set; }
        //Contains the case condition (int) and the label to jump to (string)
        public List<Case> Cases { get; set; }

        public Switch()
        {
            Cases = new List<Case>();
        }
    }
}
