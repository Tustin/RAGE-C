﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using System.Linq;

namespace RAGE
{
    public class RAGEListener : CBaseListener
    {
        //Stuff that gets populated as the walker goes through the tree
        Function currentFunction;
        Variable currentVariable;

        public static List<(string label, int id, CParser.SelectionStatementContext context)> conditionalContexts;

        RAGEVisitor visitor;

        List<string> conditionalLabels = new List<string>();

        int currentConditionalCount = 1;

        public RAGEListener()
        {
            visitor = new RAGEVisitor();
            conditionalContexts = new List<(string label, int id, CParser.SelectionStatementContext context)>();
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

            if (!Core.IsDataType(type))
            {
                throw new Exception($"Data type for function '{name}' is not supported.");
            }

            //Add the default function entry instruction
            //This will get automatically changed in ExitFunctionDefinition to have the right frame variable count
            Core.AssemblyCode.Add(name, new List<string>()
            {
                "Function 0 2 0"
            });

            currentFunction = new Function(name, type);
            Core.Functions.Add(currentFunction);
            Logger.Log($"Entering function '{name}'...");
        }

        //End of a function
        public override void ExitFunctionDefinition(CParser.FunctionDefinitionContext context)
        {
            var function = Core.AssemblyCode.FindFunction(currentFunction.Name);
            //Update the frame variable count for the function entry
            string funcEntry = function.Value[0];
            funcEntry = funcEntry.Replace("Function 0 2 0", $"Function 0 {currentFunction.FrameVars} 0");
            function.Value[0] = funcEntry;
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Return.Generate());
            Logger.Log($"Leaving function '{currentFunction.Name}'");
            currentFunction = null;
        }

        //New variables
        public override void EnterDeclarationSpecifier(CParser.DeclarationSpecifierContext context)
        {
            string type = context.GetText();

            //This function will get the data type for the function as well
            //Just do this check to see if the current node is for the function or not
            if (currentFunction.ReturnType == null)
            {
                currentFunction.ReturnType = type;
                return;
            }
            currentVariable = new Variable(null, currentFunction.FrameVars, type);
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

                currentVariable.Name = variableName;

                Logger.Log($"({currentFunction.Name}) - Parsed variable '{variableName}'");
            }
        }

        //Variable values
        public override void EnterInitializer(CParser.InitializerContext context)
        {
            string value = context.GetText();
            value = value.Replace(";", "");

            VariableTypes valueType = Utilities.GetType(currentFunction, value);

            currentVariable.ValueType = valueType;
            currentVariable.Value.Value = value;

            //Try to parse the arguments
            if (valueType == VariableTypes.NativeCall || valueType == VariableTypes.LocalCall)
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
            if (currentVariable.ValueType == VariableTypes.LocalCall
                || currentVariable.ValueType == VariableTypes.NativeCall)
            {
                foreach (Argument arg in currentVariable.Value.Arguments)
                {
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Push.Generate(arg.Value, arg.Type));
                }

            }
            //Generate either function calls or just push args
            switch (currentVariable.ValueType)
            {
                case VariableTypes.LocalCall:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Local(currentVariable.Value.Value));
                    break;
                case VariableTypes.NativeCall:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Native(currentVariable.Value.Value,
                        currentVariable.Value.Arguments.Count, true));
                    break;
                case VariableTypes.Variable:
                    Variable variable = currentFunction.Variables.GetVariable(currentVariable.Value.Value);
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(FrameVar.Get(variable));
                    break;
                default:
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Push.Generate(currentVariable.Value.Value, currentVariable.ValueType));
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
                conditionalContexts.Add(($"if_block_end_{currentConditionalCount}", currentConditionalCount, context));
                visitor.VisitExpression(context.expression());

                Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add($":if_block_begin_{currentConditionalCount}"); //@Cleanup: Remove this because its not necessary for ifs
                currentConditionalCount++;
            }
            else if (statement.StartsWith("else"))
            {
                throw new Exception("Else not supported");
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
            var contextScope = conditionalContexts.Where(a => a.context == context).FirstOrDefault();
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(contextScope.label);
        }

        //Entering for, while
        public override void EnterIterationStatement(CParser.IterationStatementContext context)
        {
            string test = context.GetText();
            base.EnterIterationStatement(context);
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
