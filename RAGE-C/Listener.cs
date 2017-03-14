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

        public static List<(string label, int id, ParserRuleContext context)> storedContexts;

        RAGEVisitor visitor;

        List<string> conditionalLabels = new List<string>();

        public RAGEListener()
        {
            visitor = new RAGEVisitor();
            storedContexts = new List<(string label, int id, ParserRuleContext context)>();
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

        //Set a variable to something
        public override void EnterExpression([NotNull] ExpressionContext context)
        {
            List<string> pieces = context.GetText().ExplodeAndClean('=');
            if (pieces.Count != 2)
            {
                base.EnterExpression(context);
                return;
            }
            string variableName = pieces[0];
            string variableValue = pieces[1];
            //@TODO: At some point this needs to check if the var is in scope
            if (!currentFunction.Variables.ContainVariable(variableName)) {
                return;
            }
            var data = visitor.VisitAssignmentExpression(context.assignmentExpression());
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(data.Assembly);

        }
        //End of a function
        public override void ExitFunctionDefinition(FunctionDefinitionContext context)
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
        public override void EnterDeclaration([NotNull] DeclarationContext context)
        {
            string type = context.GetChild(0).GetText();
            List<string> pieces = context.GetChild(1).GetText().ExplodeAndClean('=');
            string varName = pieces[0];
            string value = pieces[1];
            //Was probably set by the visitor for a loop
            if (currentFunction.Variables.ContainVariable(varName)) return;
            currentVariable = new Variable(null, currentFunction.FrameVars + 1, type);
            currentFunction.Variables.Add(currentVariable);
        }

        ////Variable names
        public override void EnterDirectDeclarator(DirectDeclaratorContext context)
        {
            if (context.Identifier() != null)
            {
                string variableName = context.GetText();

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
        public override void EnterInitializer(InitializerContext context)
        {
            string value = context.GetText();
            value = value.Replace(";", "");
            //Do any arithmetic that this variable might have
            var resp = visitor.VisitAssignmentExpression(context.assignmentExpression());

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
        public override void ExitInitializer(InitializerContext context)
        {
            if (currentVariable.IsIterator) return;
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
        public override void EnterSelectionStatement(SelectionStatementContext context)
        {
            string statement = context.GetText();
            if (statement.StartsWith("if"))
            {
                int count = storedContexts.Count(a => a.context is SelectionStatementContext);

                storedContexts.Add(($"if_block_end_{count}", count, context));

                var output = visitor.VisitExpression(context.expression());

                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(output.Assembly);
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

        //Exiting if, else, switch
        public override void ExitSelectionStatement(SelectionStatementContext context)
        {
            //Find the scope with the context (for the end label)
            var contextScope = storedContexts.Where(a => a.context == context).FirstOrDefault();
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":{contextScope.label}");
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
                    throw new Exception("Expected a Variable object from VisitDeclaration");
                }
                currentFunction.Variables.Add(v);
                currentVariable = v;

                int count = storedContexts.Count(a => a.context is IterationStatementContext);
                storedContexts.Add(($"loop_{count}", count, context));
                //Push the value of the iterator into the variable before the for loop label
                //Otherwise we would constantly have infinite loops
                var titties = Core.AssemblyCode.FindFunction(currentFunction.Name);
                titties.Value.Add(Push.Generate(v.Value.Value, v.Value.Type));
                titties.Value.Add(FrameVar.Set(v));
                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":loop_{count}");
            }
            //While loops
            else if (loop.StartsWith("while"))
            {
                int count = storedContexts.Count(a => a.context is IterationStatementContext);
                storedContexts.Add(($"loop_{count}", count, context));
                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":loop_{count}");
            }
        }

        //Exiting for, while
        public override void ExitIterationStatement(IterationStatementContext context)
        {
            var storedContext = storedContexts.Where(a => a.context == context).First();

            string code = context.GetText();


            //Reverse it so it evaluates the incrementing first before doing the comparison
            foreach (ExpressionContext expression in context.expression().Reverse())
            {
                var test = visitor.VisitExpression(expression);
                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.AddRange(test.Assembly);

            }
            //Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($"Jump {storedContext.label}");

            base.ExitIterationStatement(context);
        }

        public override void EnterAssignmentOperator(AssignmentOperatorContext context)
        {
            string gg = context.GetText();
            base.EnterAssignmentOperator(context);
        }
    }
}
