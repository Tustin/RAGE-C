using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using System.Linq;
using Antlr4.Runtime;

using static CParser;
using static RAGE.Logger.Logger;
using System.Text;

namespace RAGE.Parser
{
    public class RAGEListener : CBaseListener
    {
        //Stuff that gets populated as the walker goes through the tree
        public static Function CurrentFunction;
        public static Variable CurrentVariable;
        public static Switch CurrentSwitch;
        public static int lineNumber = 0;
        public static int linePosition = 0;

        public static List<StoredContext> storedContexts;

        public static Dictionary<StoredContext, Switch> switches;

        RAGEVisitor visitor;

        List<string> conditionalLabels = new List<string>();

        public RAGEListener()
        {
            visitor = new RAGEVisitor();
            storedContexts = new List<StoredContext>();
            switches = new Dictionary<StoredContext, Switch>();
        }

        //Set line number and position for error logging
        public override void EnterEveryRule([NotNull] ParserRuleContext context)
        {
            var token = context.Start;
            lineNumber = token.Line;
            linePosition = token.Column;
            base.EnterEveryRule(context);
        }

        //New functions
        public override void EnterFunctionDefinition(FunctionDefinitionContext context)
        {
            //Generate script entry point if it doesn't already exist
            if (Core.AssemblyCode.Count == 0)
            {
                Core.AssemblyCode.Add("__script_entry__", new List<string>()
                {
                    "Function 0 2 0",
                    "Call @main",
                    "Return 0 0",
                });
            }

            string name = Regex.Replace(context.declarator().GetText(), "\\(.*\\)", "");
            string type = context.GetChild(0).GetText();

            var comp = context.declarationSpecifiers();
            var ff = comp.declarationSpecifier()[0].alignmentSpecifier();

            DataType vType = Utilities.GetTypeFromDeclaration(type);

            if (!Function.IsValidType(vType))
            {
                //Won't get thrown because GetTypeFromDeclaration will throw an exception on error
                //We'll keep it here for the future (possibly)
                Error($"Type of function {name} is not valid");
            }

            //Add the default function entry instruction
            //This will get automatically changed in ExitFunctionDefinition to have the right frame variable count
            Core.AssemblyCode.Add(name, new List<string>()
            {
                "Function 0 2 0"
            });

            CurrentFunction = new Function(name, vType);
            Core.Functions.Add(CurrentFunction);
            LogVerbose($"Entering function '{name}'...");
        }

        public override void EnterDeclarationSpecifiers([NotNull] DeclarationSpecifiersContext context)
        {
            string ff = context.GetText();
            base.EnterDeclarationSpecifiers(context);
        }

        //End of a function
        public override void ExitFunctionDefinition(FunctionDefinitionContext context)
        {
            var function = Core.AssemblyCode.FindFunction(CurrentFunction.Name);
            string funcEntry = function.Value[0];
            //@TODO: Update first 0 for param count
            funcEntry = funcEntry.Replace("Function 0 2 0", $"Function 0 {CurrentFunction.FrameVars + 1} 0");
            function.Value[0] = funcEntry;
            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(Return.Generate());
            LogVerbose($"Leaving function '{CurrentFunction.Name}'");
            CurrentFunction = null;
        }
        public override void EnterPostfixExpression([NotNull] PostfixExpressionContext context)
        {
            string aa = context.GetText();
            base.EnterPostfixExpression(context);
        }
        //New variables
        public override void EnterDeclaration(DeclarationContext context)
        {
            string gg = context.GetText();

            Value res = visitor.VisitDeclaration(context);

            if (res.Data == null)
            {
                Error($"Found variable declaration but got null | line {lineNumber}, {linePosition}");
                return;
            }

            CurrentVariable = (Variable)res.Data;

            if (CurrentVariable.Value != null && !CurrentVariable.Value.IsDefault)
            {
                var current = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
                current.AddRange(res.Assembly);
                current.Add(FrameVar.Set(CurrentVariable));
            }

            //string type = context.GetChild(0).GetText();
            //List<string> pieces = context.GetChild(1).GetText().ExplodeAndClean('=');
            //string varName = pieces[0];
            //string value = pieces[1];
            ////Was probably set by the visitor for a loop
            //if (currentFunction.Variables.ContainVariable(varName)) return;
            //currentVariable = new Variable(null, currentFunction.FrameVars + 1, type);
            //currentFunction.Variables.Add(currentVariable);
        }

        public override void EnterDesignation([NotNull] DesignationContext context)
        {
            string aa = context.GetText();
            base.EnterDesignation(context);
        }

        public override void EnterDesignator([NotNull] DesignatorContext context)
        {
            string aa = context.GetText();
            base.EnterDesignator(context);
        }

        public override void EnterInitializer([NotNull] InitializerContext context)
        {
            string aa = context.GetText();
            base.EnterInitializer(context);
        }

        //Set a variable to something
        public override void EnterExpression(ExpressionContext context)
        {
            //string ff = context.GetText();
            //var poo = visitor.VisitExpression(context);
            //string aa = context.assignmentExpression().GetText();
            //List<string> pieces = context.GetText().ExplodeAndClean('=');
            //if (pieces.Count != 2)
            //{
            //    var test = visitor.VisitExpression(context);
            //    return;
            //}
            //string variableName = pieces[0];
            //string variableValue = pieces[1];
            ////@TODO: At some point this needs to check if the var is in scope
            //if (!currentFunction.Variables.ContainVariable(variableName))
            //{
            //    throw new Exception("Variable does not exist");
            //}
            //var data = visitor.VisitAssignmentExpression(context.assignmentExpression());
            //Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(data.Assembly);
        }

        public override void EnterStatement(StatementContext context)
        {
            if (context.expressionStatement() == null)
            {
                base.EnterStatement(context);
                return;
            }

            var res = visitor.VisitExpression(context.expressionStatement().expression());

            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.AddRange(res.Assembly);
        }

        //Entering if, else, switch
        public override void EnterSelectionStatement(SelectionStatementContext context)
        {
            string statement = context.GetText();

            if (statement.StartsWith("if"))
            {
                int count = storedContexts.Count(a => a.Context is SelectionStatementContext);

                StoredContext sc = new StoredContext($"selection_end_{count}", count, context);

                storedContexts.Add(sc);

                visitor.CurrentContext = sc;

                var output = visitor.VisitExpression(context.expression());

                var code = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;

                code.AddRange(output.Assembly);
            }
            else if (statement.StartsWith("else"))
            {
                throw new Exception("Else not supported");
            }
            else if (statement.StartsWith("switch"))
            {
                var cases = context.statement()[0].compoundStatement().blockItemList();

                if (cases == null)
                {
                    Error($"Unable to parse switch statement | line {lineNumber},{linePosition}");
                }

                //Create the switch so we can add the items to it
                int count = storedContexts.Count(a => a.Context is SelectionStatementContext);

                StoredContext sc = new StoredContext($"selection_end_{count}", count, context);

                storedContexts.Add(sc);

                visitor.CurrentContext = sc;

                Switch currentSwitch = new Switch();

                CurrentSwitch = currentSwitch;

                //loop through each case
                while (cases != null)
                {
                    var currentCase = visitor.VisitLabeledStatement(cases.blockItem().statement().labeledStatement());
                    KeyValuePair<int, string> caseData = (KeyValuePair<int, string>)currentCase.Data;
                    currentSwitch.Labels.Add(caseData.Key, caseData.Value);
                    cases = cases.blockItemList();
                }
                var cf = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
                switches.Add(sc, currentSwitch);
                StringBuilder sb = new StringBuilder();
                sb.Append("Switch ");
                foreach (var @case in currentSwitch.Labels.Reverse())
                {
                    sb.Append($"[{@case.Key}=@{@case.Value}]");
                }
                cf.Add(sb.ToString());
            }
        }

        //Switch cases
        public override void EnterLabeledStatement([NotNull] LabeledStatementContext context)
        {
            string aa = context.GetText();
            //var aa = context.GetText();
            //var label = context.GetChild(0).GetText();
            //if (label == "case")
            //{
            //    var condition = context.GetChild(1).GetText();

            //    if (!int.TryParse(condition, out int value))
            //    {
            //        Error($"Switch cases can only contain integers | line {lineNumber},{linePosition}");
            //    }
            //    var currentSwitch = switches.LastOrDefault();

            //    if (currentSwitch.Value.Labels.ContainsKey(value))
            //    {
            //        Error($"Switch already contains case for '{value}' | line {lineNumber},{linePosition}");
            //    }

            //    string label = $":selection_{currentSwitch.Key.Id}_case_{value}";

            //}
            base.EnterLabeledStatement(context);
        }

        public override void ExitLabeledStatement([NotNull] LabeledStatementContext context)
        {
            string gff = context.GetText();
            base.ExitLabeledStatement(context);
        }

        //Exiting if, else, switch
        public override void ExitSelectionStatement(SelectionStatementContext context)
        {
            //Find the scope with the context (for the end label)
            var contextScope = storedContexts.Where(a => a.Context == context).FirstOrDefault();
            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{contextScope.Label}");
        }

        //Entering for, while
        public override void EnterIterationStatement(IterationStatementContext context)
        {
            string loop = context.GetText();

            //For loops
            if (loop.StartsWith("for"))
            {
                //Generate the variable for the for loop
                var variable = visitor.VisitDeclaration(context.declaration());
                if (!(variable.Data is Variable v))
                {
                    Error($"Expected a Variable object from VisitDeclaration, got {variable.Data.GetType()} | line {lineNumber},{linePosition}");
                    return;
                }
                CurrentFunction.Variables.Add(v);
                CurrentVariable = v;

                int count = storedContexts.Count(a => a.Context is IterationStatementContext);
                StoredContext sc = new StoredContext($"loop_{count}", count, context);
                storedContexts.Add(sc);
                visitor.CurrentContext = sc;

                //Push the value of the iterator into the variable before the for loop label
                //Otherwise we would constantly have infinite loops
                var func = Core.AssemblyCode.FindFunction(CurrentFunction.Name);
                func.Value.Add(Push.Generate(v.Value.Value, v.Value.Type));
                func.Value.Add(FrameVar.Set(v));
                //Add label
                func.Value.Add($":loop_{count}");
            }
            //While loops
            else if (loop.StartsWith("while"))
            {
                int count = storedContexts.Count(a => a.Context is IterationStatementContext);
                StoredContext sc = new StoredContext($"loop_{count}", count, context);
                storedContexts.Add(sc);
                visitor.CurrentContext = sc;
                Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":loop_{count}");
            }
        }

        //Exiting for, while
        public override void ExitIterationStatement(IterationStatementContext context)
        {
            var storedContext = storedContexts.Where(a => a.Context == context).First();

            string code = context.GetText();

            //Reverse it so it evaluates the incrementing first before doing the comparison
            foreach (ExpressionContext expression in context.expression().Reverse())
            {
                var test = visitor.VisitExpression(expression);
                Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.AddRange(test.Assembly);

            }
            //Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($"Jump {storedContext.label}");

            base.ExitIterationStatement(context);
        }

    }
}
