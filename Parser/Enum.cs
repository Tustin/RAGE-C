using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class Enum
    {
        public string Name { get; set; }

        public List<Enumerator> Enumerators { get; set; }

        public Enum(string name)
        {
            Name = name;
            Enumerators = new List<Enumerator>();
        }
    }
}
