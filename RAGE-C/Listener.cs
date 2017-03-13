using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using System.Linq;
using Antlr4.Runtime;
using static CParser;

namespace RAGE
{
    public class RAGEListener : CBaseListener
    {
        //Stuff that gets populated as the walker goes through the tree
        public static Function currentFunction;
        public static Variable currentVariable;

        public static List<(string label, int id, CParser.SelectionStatementContext context)> conditionalContexts;
        public static List<(string label, int id, CParser.IterationStatementContext context)> iteratorContexts;
        public static List<(string label, int id, ParserRuleContext context)> storedContexts;

        RAGEVisitor visitor;

        List<string> conditionalLabels = new List<string>();

        int currentConditionalCount = 1;
        int currentIteratorCount = 1;

        public RAGEListener()
        {
            visitor = new RAGEVisitor();
            //conditionalContexts = new List<(string label, int id, CParser.SelectionStatementContext context)>();
            //iteratorContexts = new List<(string label, int id, CParser.IterationStatementContext context)>();
            storedContexts = new List<(string label, int id, ParserRuleContext context)>();
        }

        //New functions
        public override void EnterFunctionDefinition(CParser.FunctionDefinitionContext context)
        {
            //Generate script entry point if it doesn't already exist
            if (Core.AssemblyCode.Count == 0)
            {
                Core.AssemblyCode.Add("__script_entry__", new List<string>()
                {
                    "Function 0 2 0",
                    "Call @main",
                    "Return 0 0",
                    ""
                });
            }

            string name = Regex.Replace(context.declarator().GetText(), "\\(.*\\)", "");
            string type = context.GetChild(0).GetText();

            VariableType vType = Utilities.GetTypeFromDeclaration(type);

            if (!Function.IsValidType(vType))
            {
                //Won't get thrown because GetTypeFromDeclaration will throw an exception on error
                //We'll keep it here for the future (possibly)
                throw new Exception($"Type of function {name} is not valid");
            }

            //Add the default function entry instruction
            //This will get automatically changed in ExitFunctionDefinition to have the right frame variable count
            Core.AssemblyCode.Add(name, new List<string>()
            {
                "Function 0 2 0"
            });

            currentFunction = new Function(name, vType);
            Core.Functions.Add(currentFunction);
            Logger.Log($"Entering function '{name}'...");
        }

        //End of a function
        public override void ExitFunctionDefinition(CParser.FunctionDefinitionContext context)
        {
            var function = Core.AssemblyCode.FindFunction(currentFunction.Name);
            string funcEntry = function.Value[0];
            //@TODO: Update first 0 for param count
            funcEntry = funcEntry.Replace("Function 0 2 0", $"Function 0 {currentFunction.FrameVars} 0");
            function.Value[0] = funcEntry;
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Return.Generate());
            Logger.Log($"Leaving function '{currentFunction.Name}'");
            currentFunction = null;
        }

        //New variables
        public override void EnterDeclaration([NotNull] CParser.DeclarationContext context)
        {
            string type = context.GetChild(0).GetText();

            currentVariable = new Variable(null, currentFunction.FrameVars + 1, type);
            currentFunction.Variables.Add(currentVariable);
        }

        //Variable names
        public override void EnterDirectDeclarator(CParser.DirectDeclaratorContext ctx)
        {
            if (ctx.Identifier() != null)
            {
                string variableName = ctx.GetText();

                //EnterDirectDeclarator will have the function name as a declarator so we wanna ignore that
                if (Core.Functions.ContainFunction(variableName)) return;

                if (currentFunction.Variables.ContainVariable(variableName))
                {
                    //throw new Exception($"Variable {variableName} already exists in this function scope");
                    return;
                }

                currentVariable.Name = variableName;

                Logger.Log($"({currentFunction.Name}) - Parsed variable '{variableName}'");
            }
        }

        //Variable values
        public override void EnterInitializer(CParser.InitializerContext context)
        {
            string value = context.GetText();
            value = value.Replace(";", "");
            //Do any arithmetic that this variable might have
            var resp = (ExpressionResponse)visitor.VisitAssignmentExpression(context.assignmentExpression());

            VariableType valueType = Utilities.GetType(currentFunction, resp.Data.ToString());

            if (valueType != currentVariable.Type && valueType != VariableType.LocalCall && valueType != VariableType.NativeCall)
            {
                throw new Exception($"Value of '{currentVariable.Name}' does not match it's type");
            }

            currentVariable.Value.Value = resp.Data.ToString();
            currentVariable.Value.Type = valueType;

            //Try to parse the arguments
            if (valueType == VariableType.NativeCall || valueType == VariableType.LocalCall)
            {
                //Clean up the function call
                string stripped = Regex.Replace(value, "\\(.*\\)", "");
                currentVariable.Value.Value = stripped;
                Regex reg = new Regex(@"\(([a-zA-Z0-9,\s""']*)\)");
                List<string> matches = reg.Matches(value).GetRegexGroups();
                if (matches.Count == 1 && matches[0] == "") return;
                string arguments = matches[0];
                currentVariable.Value.Arguments = Utilities.GetListOfArguments(arguments);
            }
        }

        //Parse variable function call arguments and generate push instructions
        public override void ExitInitializer(CParser.InitializerContext context)
        {
            //Generate list of arguments
            if (currentVariable.Value.Type == VariableType.LocalCall
                || currentVariable.Value.Type == VariableType.NativeCall)
            {
                foreach (Argument arg in currentVariable.Value.Arguments)
                {
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Push.Generate(arg.Value, arg.Type));
                }

            }
            //Generate either function calls or just push args
            switch (currentVariable.Value.Type)
            {
                case VariableType.LocalCall:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Local(currentVariable.Value.Value));
                    break;
                case VariableType.NativeCall:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Native(currentVariable.Value.Value,
                        currentVariable.Value.Arguments.Count, true));
                    break;
                case VariableType.Variable:
                    Variable variable = currentFunction.Variables.GetVariable(currentVariable.Value.Value);
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(FrameVar.Get(variable));
                    break;
                default:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Push.Generate(currentVariable.Value.Value, currentVariable.Value.Type));
                    break;
            }

            //Generate the variable to store the value into
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(FrameVar.Set(currentVariable));
        }

        //Entering if, else, switch
        public override void EnterSelectionStatement(CParser.SelectionStatementContext context)
        {
            string statement = context.GetText();
            if (statement.StartsWith("if"))
            {
                int count = storedContexts.Count(a => a.context is SelectionStatementContext);
                storedContexts.Add(($"if_block_end_{count}", count, context));
                //conditionalContexts.Add(($"if_block_end_{currentConditionalCount}", currentConditionalCount, context));
                var output = (List<string>)visitor.VisitExpression(context.expression());

                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(output);

                //currentConditionalCount++;
            }
            else if (statement.StartsWith("else"))
            {
                throw new Exception("Else not supported");
            }
            else if (statement.StartsWith("switch"))
            {
                throw new Exception("Switches not supported yet");
            }
        }

        public override void EnterExpression([NotNull] CParser.ExpressionContext context)
        {
            string ff = context.GetText();
            base.EnterExpression(context);
        }

        //Exiting if, else, switch
        public override void ExitSelectionStatement(CParser.SelectionStatementContext context)
        {
            //Find the scope with the context (for the end label)
            var contextScope = storedContexts.Where(a => a.context == context).FirstOrDefault();
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":{contextScope.label}");
        }

        //Entering for, while
        public override void EnterIterationStatement(IterationStatementContext context)
        {
            string loop = context.GetText();

            //Generate the variable for the for loop
            var variable = (Variable)visitor.VisitDeclaration(context.declaration());
            currentFunction.Variables.Add(variable);

            if (loop.StartsWith("for"))
            {
                int count = storedContexts.Count(a => a.context is IterationStatementContext);
                storedContexts.Add(($"for_loop_{count}", count, context));

                //iteratorContexts.Add(($"for_loop_{currentIteratorCount}", currentIteratorCount, context));
                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":for_loop_{count}");

                foreach (CParser.ExpressionContext expression in context.expression())
                {
                    var test = (List<string>)visitor.VisitExpression(expression);
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(test);

                }

                //currentIteratorCount++;
            }
        }

        //Exiting for, while
        public override void ExitIterationStatement(CParser.IterationStatementContext context)
        {
            string code = context.GetText();
            base.ExitIterationStatement(context);
        }

        public override void EnterAssignmentOperator(CParser.AssignmentOperatorContext context)
        {
            string gg = context.GetText();
            base.EnterAssignmentOperator(context);
        }
    }
}
