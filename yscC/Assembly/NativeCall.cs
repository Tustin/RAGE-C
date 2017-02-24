using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    class NativeCall
    {
        public string Native { get; set; }
        public List<Argument> Arguments { get; set; }
        public bool ReturnsValue { get; set; }
    }
}
