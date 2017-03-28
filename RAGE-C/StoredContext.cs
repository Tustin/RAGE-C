using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
namespace RAGE
{
    public class StoredContext
    {
        public string Label { get; set; }

        public int Id { get; set; }

        public ParserRuleContext Context { get; set; }

        public DataType Property { get; set; }

        public StoredContext(string label, int id, ParserRuleContext context)
        {
            Label = label;
            Id = id;
            Context = context;
        }
    }
}
