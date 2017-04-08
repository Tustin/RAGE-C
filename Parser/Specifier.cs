using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE
{
    public enum Specifier
    {
        Static,
        Global,
        Auto, //Might use eventually for type inference
        None
    }
}
