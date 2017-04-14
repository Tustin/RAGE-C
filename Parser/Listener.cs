using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using System.Linq;
using Antlr4.Runtime;
using RAGE.Parser.Opcodes;

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

        //Enums
        public override void EnterEnumDeclarator([NotNull] EnumDeclaratorContext context)
        {
            string enumName = context.GetChild(1).GetText();

            if (Script.Enums.ContainsEnum(enumName))
            {
                Error($"Script already contains an enum called '{enumName}' | line {lineNumber}, {linePosition}");
            }

            //Do this hack so the enum is parsed from top down
            var enumItems = new List<EnumeratorContext>();
            var enumList = context.enumeratorList();

            if (enumList == null)
            {
                Error($"Enum '{enumName}' contains no enumerators | | line {lineNumber}, {linePosition}");
            }

            while (enumList != null)
            {
                enumItems.Insert(0, enumList.enumerator());
                enumList = enumList.enumeratorList();
            }

            var currentEnum = new Enum(enumName);

            Script.Enums.Add(currentEnum);

            foreach (var enumContext in enumItems)
            {
                visitor.VisitEnumerator(enumContext);
            }

            base.EnterEnumDeclarator(context);
        }

        //New functions
        public override void EnterFunctionDefinition(FunctionDefinitionContext context)
        {
            //Generate script entry point if it doesn't already exist
            //@Cleanup: Make this not so dumb
            if (Core.AssemblyCode.Count == 0)
            {
                var entryContents = new List<string>();
                entryContents.Add("Function 0 2 0");
                if (Script.StaticVariables.Count > 0)
                {
                    entryContents.Add($"//Auto assigning {Script.StaticVariables.Count} statics");
                    foreach (var variable in Script.StaticVariables)
                    {
                        if (variable is Array)
                            continue;
                        Variable var = variable as Variable;
                        entryContents.AddRange(var.ValueAssembly);
                        entryContents.Add(StaticVar.Set(var));
                    }
                }
                entryContents.Add("Call @main");
                entryContents.Add("Return 0 0");
                Core.AssemblyCode.Add("__script_entry__", entryContents);
            }
            var specifier = visitor.VisitDeclarationSpecifiers(context.declarationSpecifiers());

            string name = Regex.Replace(context.declarator().GetText(), "\\(.*\\)", "");

            var comp = context.declarationSpecifiers();

            //Add the default function entry instruction
            //This will get automatically changed in ExitFunctionDefinition to have the right frame variable count
            Core.AssemblyCode.Add(name, new List<string>()
            {
                "Function 0 2 0"
            });

            CurrentFunction = new Function(name, specifier.Type);
            Script.Functions.Add(CurrentFunction);
            LogVerbose($"Entering function '{name}'...");
        }

        //Parse function params
        public override void EnterParameterList([NotNull] ParameterListContext context)
        {
            //For some reason, this context gets entered twice
            //Might be doing it seperately for each arg?
            if (CurrentFunction.Parameters.Count > 0)
            {
                return;
            }
            var contextsList = new List<ParameterDeclarationContext>();

            //Another hack to reverse the argument list
            while (context != null)
            {
                var decl = context.parameterDeclaration();
                var paramName = decl.declarator().GetText();
                if (CurrentFunction.ContainsParameterName(paramName))
                {
                    Error($"Function '{CurrentFunction.Name}' already contains a parameter named '{paramName}' | line {lineNumber}, {linePosition}");
                }

                contextsList.Insert(0, decl);
                context = context.parameterList();
            }
            //Loop through each param

            foreach (var declContext in contextsList)
            {
                var paramName = declContext.declarator().GetText();

                var specifier = visitor.VisitDeclarationSpecifiers(declContext.declarationSpecifiers());

                CurrentFunction.Parameters.Add(new Parameter(specifier.Type, paramName, CurrentFunction.Parameters.Count));
            }
        }

        //End of a function
        public override void ExitFunctionDefinition(FunctionDefinitionContext context)
        {
            var function = Core.AssemblyCode.FindFunction(CurrentFunction.Name);
            string funcEntry = function.Value[0];
            //@TODO: Update first 0 for param count
            funcEntry = funcEntry.Replace("Function 0 2 0", $"Function {CurrentFunction.Parameters.Count} {CurrentFunction.FrameVars + CurrentFunction.Parameters.Count} 0");
            function.Value[0] = funcEntry;
            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(Opcodes.Return.Generate(CurrentFunction.Parameters.Count));
            LogVerbose($"Leaving function '{CurrentFunction.Name}'");
            CurrentFunction = null;
        }

        //New variables
        public override void EnterDeclaration(DeclarationContext context)
        {
            var variable = visitor.VisitDeclaration(context);

            if (variable.Type == DataType.Array)
            {
                Array arr = variable.Data as Array;
                if (arr.Specifier == Specifier.Static)
                {
                    Script.StaticVariables.Add(arr);
                }
                else
                {
                    CurrentFunction.Variables.Add(arr);
                }
            }
            else if (variable.Type == DataType.Variable)
            {
                Variable var = variable.Data as Variable;
                if (var.Specifier == Specifier.Static)
                {
                    Script.StaticVariables.Add(var);
                }
                else
                {
                    CurrentFunction.Variables.Add(var);
                    var cf = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
                    cf.AddRange(var.ValueAssembly);
                    cf.Add(FrameVar.Set(var));
                }
            }
        }

        //Statements
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
   
        //Entering if, switch
        public override void EnterSelectionStatement(SelectionStatementContext context)
        {
            string selectionType = context.GetText();
            var code = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;

            if (selectionType.StartsWith("if"))
            {
                int count = storedContexts.Count(a => a.Context is SelectionStatementContext);
                var hasElse = context.selectionElseStatement() != null;
                StoredContext sc;
                //Set specific label if this selection has an else statement
                sc = new StoredContext($"selection_end_{count}", count, context);
                storedContexts.Add(sc);
                if (hasElse)
                {
                    sc = new StoredContext($"selection_else_{count}", count, context.selectionElseStatement());
                    storedContexts.Add(sc);
                }

                visitor.CurrentContext = sc;

                var output = visitor.VisitExpression(context.expression());

                code.AddRange(output.Assembly);
            }
            else if (selectionType.StartsWith("switch"))
            {
                var cases = context.statement().compoundStatement().blockItemList();

                var conditionVariable = context.expression().GetText();
                DataType conditionType = Utilities.GetTypeFromDeclaration(conditionVariable);

                if (conditionType != DataType.NativeCall && conditionType != DataType.LocalCall && conditionType != DataType.Variable)
                {
                    Error($"Undefined type '{conditionType}' used in switch expression | line {lineNumber},{linePosition}");
                }

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
                    var shit = cases.blockItem().statement().labeledStatement();
                    if (shit == null)
                    {
                        cases = cases.blockItemList();
                        continue;
                    }
                    var currentCase = visitor.VisitLabeledStatement(shit);
                    Case caseData = currentCase.Data as Case;
                    currentSwitch.Cases.Add(caseData);
                    cases = cases.blockItemList();
                }

                currentSwitch.Cases.Reverse();
                var cf = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
                switches.Add(sc, currentSwitch);
                if (conditionType == DataType.NativeCall || conditionType == DataType.LocalCall)
                {
                    var output = visitor.VisitExpression(context.expression());
                    cf.AddRange(output.Assembly);
                }
                else if (conditionType == DataType.Variable)
                {
                    if (CurrentFunction.Variables.ContainVariable(conditionVariable))
                    {
                        cf.Add(FrameVar.Get(CurrentFunction.Variables.GetVariable(conditionVariable)));
                    }
                    else if (Script.StaticVariables.ContainVariable(conditionVariable))
                    {
                        cf.Add(StaticVar.Get(Script.StaticVariables.GetVariable(conditionVariable)));
                    }
                    else
                    {
                        Error($"Unable to find variable '{conditionVariable}' used in switch expression | line {lineNumber},{linePosition}");
                    }
                }
                StringBuilder sb = new StringBuilder();
                sb.Append("Switch ");
                foreach (var @case in currentSwitch.Cases)
                {
                    sb.Append($"[{@case.Condition}=@{@case.Label}]");
                }
                cf.Add(sb.ToString());
            }
        }
        
        //Entering else statement
        public override void EnterSelectionElseStatement([NotNull] SelectionElseStatementContext context)
        {
            var contextScope = storedContexts.Where(a => a.Context == context).LastOrDefault();
            if (contextScope == null)
            {
                Error($"Failed parsing else statement | line {lineNumber},{linePosition}");
            }
            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{contextScope.Label}");
            base.EnterSelectionElseStatement(context);
        }

        //Exiting if, switch
        public override void ExitSelectionStatement(SelectionStatementContext context)
        {
            var code = new List<string>();
            string selectionType = context.GetText();

            var contextScope = storedContexts.Where(a => a.Context == context).FirstOrDefault();
            Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{contextScope.Label}");  
        }

        //Switch cases
        public override void EnterLabeledStatement([NotNull] LabeledStatementContext context)
        {
            if (context.GetChild(0).GetText() == "case")
            {
                if (CurrentSwitch == null)
                {
                    Error($"Found case label, but no switch was found | line {lineNumber},{linePosition}");
                }

                Case nextCase = CurrentSwitch.Cases.Where(a => a.Generated == false).FirstOrDefault();

                if (nextCase == null)
                {
                    Error($"Found case that wasn't defined | line {lineNumber},{linePosition}");
                }

                Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{nextCase.Label}");
                nextCase.Generated = true;

            }
            base.EnterLabeledStatement(context);
        }

        //Break, continue, return
        public override void EnterJumpStatement([NotNull] JumpStatementContext context)
        {
            string jumpType = context.GetText().Replace(";", "");
            switch (jumpType)
            {
                case "break":
                //Get the last stored context to jump to it
                var jumpLoc = storedContexts.LastOrDefault();
                if (jumpLoc == null)
                {
                    Error($"Tried to use a jump statement without a context to jump out of | line {lineNumber},{linePosition}");
                }
                Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(Jump.Generate(JumpType.Unconditional, jumpLoc.Label));
                break;
                case "continue":
                //@TODO
                break;
            }
            base.EnterJumpStatement(context);
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
                v.IsIterator = true;
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

            base.ExitIterationStatement(context);
        }

    }
}
