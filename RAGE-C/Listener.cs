using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;

namespace RAGE
{
    public class MyListener : CBaseListener
    {
        //Stuff that gets populated as the walker goes through the tree
        Function currentFunction;
        Variable currentVariable;

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
            currentFunction = new Function(name, type);
            Core.Functions.Add(currentFunction);
            Logger.Log($"Entering function '{name}'...");
        }

        //End of a function
        public override void ExitFunctionDefinition([NotNull] CParser.FunctionDefinitionContext context)
        {
            base.ExitFunctionDefinition(context);
        }


        //New variables
        public override void EnterDeclarationSpecifier([NotNull] CParser.DeclarationSpecifierContext context)
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
        public override void EnterInitializer([NotNull] CParser.InitializerContext context)
        {
            string value = context.GetText();
            value = value.Replace(";", "");

            //Do type checking
            if (value == "true" || value == "false")
            {
                currentVariable.ValueType = VariableValueType.Bool;
                currentVariable.Value.Value = value;
            }
            else if (Regex.IsMatch(value, "^[0-9]+$"))
            {
                currentVariable.ValueType = VariableValueType.Int;
                currentVariable.Value.Value = value;
            }
            else if (Regex.IsMatch(value, "^[0-9.]+$"))
            {
                currentVariable.ValueType = VariableValueType.Float;
                currentVariable.Value.Value = value;
            }
            else if (Regex.IsMatch(value, "^\".+[^\"\']\"$"))
            {
                currentVariable.ValueType = VariableValueType.String;
                currentVariable.Value.Value = value;
            }
            else if (Regex.IsMatch(value, "^\\w+\\("))
            {
                string stripped = Regex.Replace(value, "\\(.*\\)", "");
                if (Native.IsFunctionANative(stripped))
                {
                    currentVariable.ValueType = VariableValueType.NativeCall;
                    currentVariable.Value.Value = stripped;
                }
                else if (Core.Functions.ContainFunction(stripped))
                {
                    currentVariable.ValueType = VariableValueType.LocalCall;
                    currentVariable.Value.Value = stripped;
                }
            }
            else if (Regex.IsMatch(value, "^\\w+"))
            {
                if (currentFunction.Variables.ContainVariable(value))
                {
                    currentVariable.ValueType = VariableValueType.Variable;
                    currentVariable.Value.Value = value;
                }
            }
            else
            {
                throw new Exception($"Unable to parse value for variable '{currentVariable.Name}'");
            }
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

        public override void EnterAssignmentOperator([NotNull] CParser.AssignmentOperatorContext context)
        {
            string gg = context.GetText();
            base.EnterAssignmentOperator(context);
        }
    }
}
