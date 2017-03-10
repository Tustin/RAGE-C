using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;

namespace RAGE
{
    public class MyListener : CBaseListener
    {
        public static List<string> identifiers = new List<string>();

        public override void EnterDirectDeclarator([NotNull] CParser.DirectDeclaratorContext ctx)
        {
            if (ctx.Identifier() != null)
            {
                string gg = ctx.GetText();
                string identifier = ctx.Identifier().GetText();
                identifiers.Add(ctx.Identifier().GetText());
            }
        }

        //New functions
        public override void EnterFunctionDefinition(CParser.FunctionDefinitionContext ctx)
        {
            if (Core.AssemblyCode.Count == 0)
            {
                Core.AssemblyCode.Add("__script__entry", new List<string>()
                {
                    "Function 0 2 0",
                    "Call @main",
                    "Return 0 0",
                    ""
                });
            }
            string name = Regex.Replace(ctx.declarator().GetText(), "\\(.*\\)", "");
            string type = ctx.GetChild(0).GetText();
            if (!Core.IsDataType(type))
            {
                throw new Exception($"Data type for function '{name}' is not supported.");
            }
            Core.AssemblyCode.Add(name, new List<string>());
            Core.FunctionNames.Add(name);
        }

        //If, else, switch
        public override void EnterSelectionStatement([NotNull] CParser.SelectionStatementContext context)
        {
            string gg = context.GetText();
            base.EnterSelectionStatement(context);
        }

        //For, while
        public override void EnterIterationStatement([NotNull] CParser.IterationStatementContext context)
        {
            string test = context.GetText();
            base.EnterIterationStatement(context);
        }

        public override void EnterAssignmentExpression([NotNull] CParser.AssignmentExpressionContext ctx)
        {
            string gg = ctx.GetText();
        }

        public override void EnterInitializer([NotNull] CParser.InitializerContext context)
        {
            string v = context.GetText();
            base.EnterInitializer(context);
        }

    }
}
