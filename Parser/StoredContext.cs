using Antlr4.Runtime;

namespace RAGE.Parser
{
    public class StoredContext
    {
        public string Label { get; set; }

		public string EndLabel { get; set; }

        public int Id { get; set; }

        public ParserRuleContext Context { get; set; }

        public DataType Property { get; set; }

        public ScopeTypes Type { get; set; }

		public StoredContext(string label, int id, ParserRuleContext context, ScopeTypes type)
		{
			Label = label;
			Id = id;
			Context = context;
			Type = type;
		}

		public StoredContext(string label, string endLabel, int id, ParserRuleContext context, ScopeTypes type)
		{
			Label = label;
			EndLabel = endLabel;
			Id = id;
			Context = context;
			Type = type;
		}
	}
}
