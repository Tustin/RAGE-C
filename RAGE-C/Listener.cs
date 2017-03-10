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
            Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Return.Generate());
            Logger.Log($"Leaving function '{currentFunction.Name}'");
            currentFunction = null;
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

            VariableValueType valueType = Utilities.GetType(currentFunction, value);

            currentVariable.ValueType = valueType;
            currentVariable.Value.Value = value;

            //Try to parse the arguments
            if (valueType == VariableValueType.NativeCall || valueType == VariableValueType.LocalCall)
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
        public override void ExitInitializer([NotNull] CParser.InitializerContext context)
        {
            //Generate list of arguments
            if (currentVariable.ValueType == VariableValueType.LocalCall
                || currentVariable.ValueType == VariableValueType.NativeCall)
            {
                foreach (Argument arg in currentVariable.Value.Arguments)
                {
                    Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Push.Generate(arg.Value, arg.Type));
                }

                //Generate either local call or native call
                switch (currentVariable.ValueType)
                {
                    case VariableValueType.LocalCall:
                        Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Local(currentVariable.Value.Value));
                        break;
                    case VariableValueType.NativeCall:
                        Core.AssemblyCode.FindFunction(currentFunction.Name).Value.Add(Call.Native(currentVariable.Value.Value,
                            currentVariable.Value.Arguments.Count, true));
                        break;
                }
            }
        }

        //Entering if, else, switch
        public override void EnterSelectionStatement([NotNull] CParser.SelectionStatementContext context)
        {
            string gg = context.GetText();
            base.EnterSelectionStatement(context);
        }

        //Exiting if, else, switch
        public override void ExitSelectionStatement([NotNull] CParser.SelectionStatementContext context)
        {
            string ff = context.GetText();
            base.ExitSelectionStatement(context);
        }

        //Entering for, while
        public override void EnterIterationStatement([NotNull] CParser.IterationStatementContext context)
        {
            string test = context.GetText();
            base.EnterIterationStatement(context);
        }

        //Exiting for, while
        public override void ExitIterationStatement([NotNull] CParser.IterationStatementContext context)
        {
            string code = context.GetText();
            base.ExitIterationStatement(context);
        }

        public override void EnterAssignmentOperator([NotNull] CParser.AssignmentOperatorContext context)
        {
            string gg = context.GetText();
            base.EnterAssignmentOperator(context);
        }
    }
}
