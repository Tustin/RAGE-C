using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAGE.Parser
{
    public class LabelData
    {
        public List<string> Code { get; set; }
        public Value Result { get; set; }
        public Case Data { get; set; }

        public LabelData(List<string> code, Value res, Case @case)
        {
            Code = code;
            Result = res;
            Data = @case;
        } 
    }
}
