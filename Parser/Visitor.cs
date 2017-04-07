using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using System.Text.RegularExpressions;

using static CParser;
using static RAGE.Logger.Logger;
using RAGE.Parser.Opcodes;

namespace RAGE.Parser
{
    public class RAGEVisitor : CBaseVisitor<Value>
    {
        //The current context of the visitor (will be null if this isn't an expression)
        public StoredContext CurrentContext;

        public override Value VisitDeclarationSpecifiers([NotNull] DeclarationSpecifiersContext context)
        {
            var specifier = context.declarationSpecifier();

            for (int i = 0; i < specifier.Length; i++)
            {
                var test = specifier[i].GetType();
            }
            return base.VisitDeclarationSpecifiers(context);
        }
        public override Value VisitDeclaration(DeclarationContext context)
        {
            Value value = new Value();

            string varName = null;
            string varType = null;

            //Fucking dumb
            if (context.children.Count == 2)
            {
                varName = context.GetChild(0).GetChild(1).GetText();
                varType = context.GetChild(0).GetChild(0).GetText();
            }
            else if (context.children.Count == 3)
            {
                //Even dumber
                varName = context.GetChild(1).GetText().Split('=')[0];
                varType = context.GetChild(0).GetText();
            }

            string stripped = Regex.Replace(varName, "\\(.*\\)", "");

            //Array
            if (varName.Contains("["))
            {
                //Get the count
                int openBracket = varName.IndexOf('[');
                int closeBracket = varName.IndexOf(']');
                string arrName = varName.Split('[')[0];
                if (!int.TryParse(varName.Substring(openBracket + 1, closeBracket - openBracket - 1), out int arrayIndexCount))
                {
                    Error($"Failed parsing length for array {arrName} | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                }
                Array arr = new Array(arrName, RAGEListener.CurrentFunction.FrameVars, arrayIndexCount);
                RAGEListener.CurrentFunction.Arrays.Add(arr);
                //Add the array as a single variable
                Variable variable = new Variable(arrName, RAGEListener.CurrentFunction.FrameVars + 1, varType);
                variable.Value.Value = Utilities.GetDefaultValue(variable.Type);
                variable.Value.Type = variable.Type;
                variable.Value.IsDefault = true;
                RAGEListener.CurrentFunction.Variables.Add(variable);
                arrayIndexCount--;
                for (int i = 0; i < arrayIndexCount; i++)
                {
                    variable = new Variable($"{arrName}_{i + 1}", RAGEListener.CurrentFunction.FrameVars + 1, varType);
                    variable.Value.Value = Utilities.GetDefaultValue(variable.Type);
                    variable.Value.Type = variable.Type;
                    variable.Value.IsDefault = true;
                    RAGEListener.CurrentFunction.Variables.Add(variable);
                }
                value.Data = arr;
                return value;
            }
            else
            {
                Variable variable;
                //Currently not in a function, so this is a static var
                if (RAGEListener.CurrentFunction == null)
                {
                    variable = new Variable(varName, Script.StaticVariables.Count + 1, varType);
                    Script.StaticVariables.Add(variable);
                }
                else
                {
                    variable = new Variable(varName, RAGEListener.CurrentFunction.FrameVars + 1, varType);
                    RAGEListener.CurrentFunction.Variables.Add(variable);
                }
                //See if this variable is being initialized
                //If not, then we'll give it a default value
                if (context.initDeclaratorList() != null)
                {
                    var resp = VisitInitDeclarator(context.initDeclaratorList().initDeclarator());
                    value.Assembly = resp.Assembly;
                    if (resp.Data != null)
                    {
                        variable.Value.Value = resp.Data.ToString();
                    }
                    variable.Value.Type = resp.Type;
                    variable.Value.IsDefault = false;
                }
                else
                {
                    variable.Value.Value = Utilities.GetDefaultValue(variable.Type);
                    variable.Value.Type = variable.Type;
                    variable.Value.IsDefault = true;
                }


                value.Data = variable;
                return value;
            }

        }
        public override Value VisitSelectionStatement([NotNull] SelectionStatementContext context)
        {
            return base.VisitSelectionStatement(context);
        }
        public override Value VisitLabeledStatement(LabeledStatementContext context)
        {
            var label = context.GetChild(0).GetText();
            var ret = new Value();
            if (label == "case")
            {
                var condition = context.GetChild(1).GetText();

                if (!int.TryParse(condition, out int value))
                {
                    Error($"Switch cases can only contain integers | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
                }


                if (RAGEListener.CurrentSwitch.Cases.Any(a => a.Condition == value))
                {
                    Error($"Switch already contains case for '{value}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
                }

                string caseLabel = $"selection_{CurrentContext.Id}_case_{value}";
                ret.Data = new Case(value, caseLabel);

                return ret;
            }

            Error($"Unsupported label '{label}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
            return null;
        }
        public override Value VisitInitDeclarator(InitDeclaratorContext context)
        {
            Value value = new Value();

            string varName = context.GetChild(0).GetText();
            string varValue = context.GetChild(0).GetText();

            var resp = VisitAssignmentExpression(context.initializer().assignmentExpression());

            return resp;
        }

        public override Value VisitExpression(ExpressionContext context)
        {
            Value val = new Value();

            string expression = context.GetText();

            //Get the context for the selection statement
            CurrentContext = RAGEListener.storedContexts.Where(a => a.Context == context.Parent).FirstOrDefault();

            //We'll do some optimizations here
            //No need to push 0 or 1 to the stack if the expression is just true or false
            //If false, we'll just always jump to the if statement end, but otherwise it'll just continue the flow
            if (expression == "true")
            {
                if (CurrentContext.Context is IterationStatementContext)
                {
                    val.Assembly.Add(Opcodes.Jump.Generate(Opcodes.JumpType.Unconditional, CurrentContext.Label));
                    return val;
                }
            }
            if (expression == "false")
            {
                val.Assembly.Add(Jump.Generate(JumpType.Unconditional, CurrentContext.Label));
                return val;
            }

            Value output = VisitAssignmentExpression(context.assignmentExpression());

            if (output.Data != null)
            {
                if (output.Type == DataType.Bool && output.Data.Equals(true)) return val;
                if (output.Type == DataType.Bool && output.Data.Equals(false))
                {
                    val.Assembly.Add(Jump.Generate(JumpType.Unconditional, CurrentContext.Label));
                    return val;
                }
            }
            //If the if expression doesnt have ==, then the result will come back as a type other than bool
            if (output.Type != DataType.Bool && CurrentContext != null)
            {
                if (!RAGEListener.switches.ContainsKey(CurrentContext))
                {
                    switch (output.Type)
                    {
                        case DataType.NativeCall:
                            output.Assembly.Add(Jump.Generate(JumpType.False, CurrentContext.Label));
                            break;
                    }
                }
            }
            val.Assembly.AddRange(output.Assembly);
            val.Type = output.Type;
            return val;
        }

        public override Value VisitAssignmentExpression([NotNull] AssignmentExpressionContext context)
        {
            if (context.assignmentExpression() == null)
            {
                return VisitConditionalExpression(context.conditionalExpression());
            }

            Value left = VisitUnaryExpression(context.unaryExpression());
            Value right = VisitAssignmentExpression(context.assignmentExpression());

            Variable variable = null;
            if (left.Type != DataType.Array)
            {
                variable = left.OriginalVariable ?? RAGEListener.CurrentFunction.Variables.GetVariable(left.Data.ToString());
                if (variable == null)
                {
                    Error($"Unable to find variable {left.Data.ToString()} | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    return null;
                }
            }

            List<string> code = new List<string>();
            string op = context.GetChild(1).GetText();
            switch (op)
            {
                case "+=":
                    //This will always be a variable
                    code.Add(FrameVar.Get(variable));
                    code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, Convert.ToInt32(right.Data.ToString())));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "=":
                    //Since its an array, we need to push the value before the array indexing
                    if (left.Type == DataType.Array)
                    {
                        if (right.Type == DataType.Variable)
                        {
                            var rightVar = RAGEListener.CurrentFunction.Variables.GetVariable(right.Data.ToString());
                            code.Add(FrameVar.Get(rightVar));
                        }
                        else if (right.Type == DataType.Int)
                        {
                            code.Add(Push.Int(right.Data.ToString()));
                        }
                        else
                        {
                            throw new Exception("Non int types not done for arrays");
                        }
                        code.AddRange(left.Assembly);
                        return new Value(DataType.Int, null, code);
                    }

                    if (right.Data == null)
                    {
                        code.AddRange(right.Assembly);
                    }
                    else
                    {                 
                        code.Add(Push.Generate(right.Data.ToString(), variable.Type));
                    }

                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
            }

            Error($"Unsupported operator {op} | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return null;
        }

        public override Value VisitConditionalExpression(ConditionalExpressionContext context)
        {
            return VisitLogicalOrExpression(context.logicalOrExpression());
        }

        public override Value VisitLogicalOrExpression(LogicalOrExpressionContext context)
        {
            if (context.logicalOrExpression() == null)
            {
                return VisitLogicalAndExpression(context.logicalAndExpression());
            }

            Value left = VisitLogicalOrExpression(context.logicalOrExpression());
            Value right = VisitLogicalAndExpression(context.logicalAndExpression());

            List<string> code = new List<string>();

            if (left.Type != DataType.Bool || right.Type != DataType.Bool)
            {
                Error($"Invalid types for logical OR | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return null;
            }
            if (left.Data != null && right.Data != null)
                return new Value(DataType.Bool, (bool)left.Data | (bool)right.Data, new List<string>());

            if ((left.Data != null && left.Data.Equals(true)) || (right.Data != null && right.Data.Equals(true)))
                return new Value(DataType.Bool, true, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
            }
            code.Add(Compare.Generate(CompareType.Equal));
            return new Value(DataType.Bool, null, code);
        }

        public override Value VisitLogicalAndExpression(LogicalAndExpressionContext context)
        {
            if (context.logicalAndExpression() == null)
            {
                return VisitEqualityExpression(context.inclusiveOrExpression().exclusiveOrExpression().andExpression().equalityExpression());
            }
            Value left = VisitLogicalAndExpression(context.logicalAndExpression());
            Value right = VisitEqualityExpression(context.inclusiveOrExpression().exclusiveOrExpression().andExpression().equalityExpression());

            List<string> code = new List<string>();

            if (left.Type != DataType.Bool || right.Type != DataType.Bool)
            {
                Error($"Invalid types for logical AND | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return null;
            }
            if (left.Data != null && right.Data != null)
                return new Value(DataType.Bool, (bool)left.Data & (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(false)) || (right.Data != null && right.Data.Equals(false)))
                return new Value(DataType.Bool, false, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.Equal, "nnfnfn"));
            return new Value(DataType.Bool, null, code);
        }

        public override Value VisitEqualityExpression(EqualityExpressionContext context)
        {
            if (context.equalityExpression() == null)
            {
                return VisitRelationalExpression(context.relationalExpression());
            }

            Value left = VisitEqualityExpression(context.equalityExpression());
            Value right = VisitRelationalExpression(context.relationalExpression());

            List<string> code = new List<string>();
            string op = context.GetChild(1).ToString();
            switch (op)
            {
                case "==":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Jump.Generate(JumpType.NotEqual, CurrentContext.Label));
                    return new Value(DataType.Bool, null, code);
                case "!=":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, !left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Jump.Generate(JumpType.Equal, CurrentContext.Label));
                    return new Value(DataType.Bool, null, code);
            }
            Error($"Unsupported operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return null;
        }

        public override Value VisitRelationalExpression(RelationalExpressionContext context)
        {
            if (context.relationalExpression() == null)
            {
                return VisitAdditiveExpression(context.shiftExpression().additiveExpression());
            }

            Value left = VisitRelationalExpression(context.relationalExpression());
            Value right = VisitAdditiveExpression(context.shiftExpression().additiveExpression());

            List<string> code = new List<string>();

            //@TODO Clean this
            if (left.Type != DataType.Int && left.Type != DataType.Variable)
            {
                Error($"Cannot use relational operators on non-integer values | line {RAGEListener.lineNumber}:{RAGEListener.linePosition}");
                return null;
            }

            if (right.Type != DataType.Int && right.Type != DataType.Variable)
            {
                Error($"Cannot use relational operators on non-integer values | line {RAGEListener.lineNumber}:{RAGEListener.linePosition}");
                return null;
            }


            //Lets just output the variables here because fuck optimization
            //Saves some headache with the compiler parsing logic on variables that might be changed
            bool isIterator = (CurrentContext.Context is IterationStatementContext) | (CurrentContext.Context is SelectionStatementContext);
            string op = context.GetChild(1).ToString();
            switch (op)
            {
                case "<":
                    //If it's not an iterator context, then it's free to return the two values (if possible)
                    if (!isIterator)
                    {
                        if (left.Data != null && right.Data != null)
                        {
                            return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());
                        }
                    }
                    if (left.Data != null)
                    {
                        if (left.OriginalVariable != null)
                        {
                            code.Add(FrameVar.Get(left.OriginalVariable));
                        }
                        else
                        {
                            code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                        }
                    }
                    if (right.Data != null)
                    {
                        if (right.OriginalVariable != null)
                        {
                            code.Add(FrameVar.Get(right.OriginalVariable));
                        }
                        else
                        {
                            code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                        }
                    }
                    code.Add(Jump.Generate(JumpType.LessThan, CurrentContext.Label));
                    return new Value(DataType.Bool, null, code);

                case "<=":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThanEqual));
                    return new Value(DataType.Bool, null, code);

                case ">":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new Value(DataType.Bool, null, code);

                case ">=":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new Value(DataType.Bool, null, code);
            }
            Error($"Unsupported operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return null;
        }

        public override Value VisitAdditiveExpression(AdditiveExpressionContext context)
        {
            if (context.additiveExpression() == null)
            {
                return VisitMultiplicativeExpression(context.multiplicativeExpression());
            }

            Value left = VisitAdditiveExpression(context.additiveExpression());
            Value right = VisitMultiplicativeExpression(context.multiplicativeExpression());

            List<string> code = new List<string>();
            string op = context.GetChild(1).ToString();
            switch (op)
            {
                case "+":
                    if (left.Type != right.Type && (left.Type != DataType.Variable || right.Type != DataType.Variable)) throw new Exception("Cannot use operand '+' on two different types.");
                    if (left.Data != null && right.Data != null) return new Value(DataType.Int, (int)left.Data + (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new Value(DataType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new Value(DataType.Int, (int)left.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Addition));
                    return new Value(DataType.Int, null, code);
                case "-":
                    if (left.Type != right.Type && (left.Type != DataType.Variable || right.Type != DataType.Variable)) throw new Exception("Cannot use operand '-' on two different types.");
                    if (left.Data != null && right.Data != null) return new Value(DataType.Int, (int)left.Data - (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new Value(DataType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new Value(DataType.Int, (int)left.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    return new Value(DataType.Int, null, code);

            }
            Error($"Unsupported operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return null;
        }

        public override Value VisitMultiplicativeExpression(MultiplicativeExpressionContext context)
        {
            if (context.multiplicativeExpression() == null)
            {
                return VisitCastExpression(context.castExpression());
            }

            Value left = VisitMultiplicativeExpression(context.multiplicativeExpression());
            Value right = VisitCastExpression(context.castExpression());

            List<string> code = new List<string>();
            string op = context.GetChild(1).ToString();
            switch (op)
            {
                case "*":
                    if (left.Type != right.Type)
                    {
                        if (left.Type == DataType.Variable)
                        {
                            if (RAGEListener.CurrentFunction.Variables.GetVariable(left.Data.ToString()).Type != right.Type)
                            {
                                Error($"Left operand variable type is not equal to the right operand type | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                                return null;
                            }
                        }
                        else if (right.Type == DataType.Variable)
                        {
                            if (RAGEListener.CurrentFunction.Variables.GetVariable(right.Data.ToString()).Type != left.Type)
                            {
                                Error($"Right operand variable type is not equal to the left operand type | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                                return null;
                            }
                        }
                    }
                    if (left.Data != null && right.Data != null && left.Type != DataType.Variable && right.Type != DataType.Variable) return new Value(DataType.Int, (int)left.Data * (int)right.Data, new List<string>());
                    if ((left.Data != null && left.Data.Equals(0)) || (right.Data != null && right.Data.Equals(0))) return new Value(DataType.Int, 0, new List<string>());
                    if (left.Data != null)
                    {
                        if (left.Type == DataType.Variable)
                        {
                            Variable var = RAGEListener.CurrentFunction.Variables.GetVariable(left.Data.ToString());
                            code.Add(FrameVar.Get(var));
                        }
                        else
                        {
                            code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                        }
                    }
                    if (right.Data != null)
                    {
                        if (right.Type == DataType.Variable)
                        {
                            Variable var = RAGEListener.CurrentFunction.Variables.GetVariable(right.Data.ToString());
                            code.Add(FrameVar.Get(var));
                        }
                        else
                        {
                            code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                        }
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Multiplication));
                    return new Value(DataType.Int, null, code);
                case "/":
                    if (left.Type != right.Type && (left.Type != DataType.Variable || right.Type != DataType.Variable)) throw new Exception("Cannot use operand '/' on two different types.");
                    if (right.Data.Equals(0)) throw new Exception("Divide by zero?! IMPOSSIBRU!!!!!");

                    if (left.Data != null && right.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), DataType.Int));
                        code.Add(Push.Generate(right.Data.ToString(), DataType.Int));
                        code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                        return new Value(DataType.Int, (int)left.Data / (int)right.Data, code);
                    }
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.CurrentFunction, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                    return new Value(DataType.Int, null, code);

                    //@Incomplete: Add modulus
            }
            Error($"Unsupported operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
            return null;
        }

        public override Value VisitCastExpression(CastExpressionContext context)
        {
            if (context.castExpression() != null)
            {
                Error($"Casting is not supported | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                return null;
            }

            return VisitUnaryExpression(context.unaryExpression());
        }

        public override Value VisitUnaryExpression(UnaryExpressionContext context)
        {
            if (context.unaryExpression() == null)
            {
                if (context.unaryOperator() != null)
                {
                    Value op = VisitUnaryOperator(context.unaryOperator());

                    string var = context.GetChild(1).GetText();

                    if (!RAGEListener.CurrentFunction.Variables.ContainVariable(var) && op.Type == DataType.Address)
                    {
                        Error($"Unary expression {context.unaryOperator().GetText()} on {var} is not possible | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        return null;
                    }
                    Variable v = RAGEListener.CurrentFunction.Variables.GetVariable(var);
                    List<string> code = new List<string>();
                    switch (op.Type)
                    {
                        case DataType.Address:
                            code.Add(FrameVar.GetPointer(v));
                            return new Value(DataType.Address, null, code);
                        case DataType.Not:
                            CurrentContext.Property = DataType.Not;
                            return new Value(DataType.Not, null, code);
                    }
                }
                else
                {
                    return VisitPostfixExpression(context.postfixExpression());
                }
            }
            return null;
        }

        public override Value VisitUnaryOperator(UnaryOperatorContext context)
        {
            if (context == null)
            {
                throw new Exception();
            }
            string op = context.GetText();

            switch (op)
            {
                //Address of
                case "&":
                    return new Value(DataType.Address, null, null);
                case "!":
                    return new Value(DataType.Not, null, null);
                default:
                    Error($"Unsupported unary operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    return null;
            }
        }

        public override Value VisitPostfixExpression(PostfixExpressionContext context)
        {
            if (context.postfixExpression() == null)
            {
                return ParseType(context.primaryExpression());
            }
            string expression = context.GetChild(0).GetText();
            string symbol = context.GetChild(1).GetText();

            List<string> code = new List<string>();
            Variable variable;
            if (RAGEListener.CurrentFunction == null)
            {
                variable = Script.StaticVariables.GetVariable(expression);
            }
            else
            {
                variable = RAGEListener.CurrentFunction.Variables.GetVariable(expression);
            }

            switch (symbol)
            {
                case "++":
                    if (!RAGEListener.CurrentFunction.Variables.ContainVariable(expression))
                    {
                        Error($"Postfix operators ({symbol}) can only be used on variables | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        return null;
                    }
                    code.Add(FrameVar.Get(variable));
                    code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, 1));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "--":
                    if (!RAGEListener.CurrentFunction.Variables.ContainVariable(expression))
                    {
                        Error($"Postfix operators ({symbol}) can only be used on variables | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        return null;
                    }
                    code.Add(FrameVar.Get(variable));
                    code.Add(Push.Generate("1", DataType.Int));
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "(":
                    if (Script.Functions.ContainFunction(expression))
                    {
                        var args = VisitArgumentExpressionList(context.argumentExpressionList());
                        //No args
                        if (args == null)
                        {
                            return new Value(DataType.LocalCall, null, null);
                        }
                        //@TODO: Argument checking
                    }
                    else if (Native.IsFunctionANative(expression))
                    {
                        Native native = Native.GetNative(expression);
                        Value args = VisitArgumentExpressionList(context.argumentExpressionList());
                        var ff = CurrentContext;
                        if (args == null && native.Params.Count == 0)
                        {
                            code.Add(Call.Native(expression, 0, native.ResultsType != DataType.Void));
                            if (CurrentContext?.Property == DataType.Not)
                            {
                                code.Add(Bitwise.Generate(BitwiseType.Not));
                                CurrentContext.Property = DataType.Void;
                            }
                            return new Value(DataType.NativeCall, null, code);
                        }
                        else if (args == null && native.Params.Count != 0)
                        {
                            Error($"{expression} takes {native.Params.Count} arguments, none given | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                            return null;
                        }

                        List<Value> argsList = (List<Value>)args.Data;

                        argsList.Reverse();

                        if (argsList.Count != native.Params.Count)
                        {
                            Error($"{expression} takes {native.Params.Count} arguments,  {argsList.Count} given | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                            return null;
                        }
                        //Generate the code
                        foreach (Value v in argsList)
                        {
                            if (v.Assembly.Count == 0)
                            {
                                code.Add(Push.Generate(v.Data.ToString(), v.Type));
                            }
                            else
                            {
                                code.AddRange(v.Assembly);
                            }
                        }
                        code.Add(Call.Native(expression, argsList.Count, native.ResultsType != DataType.Void));
                        if (CurrentContext?.Property == DataType.Not)
                        {
                            code.Add(Bitwise.Generate(BitwiseType.Not));
                            CurrentContext.Property = DataType.Void;
                        }
                        return new Value(DataType.NativeCall, null, code);
                    }
                    Error($"Found open parens, but expression is not a function | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    return null;

                //Array
                case "[":
                    string arrayName = context.GetText().Split('[')[0];
                    //Find array
                    Array array = RAGEListener.CurrentFunction.Arrays.GetArray(arrayName);
                    Variable arrayVar = RAGEListener.CurrentFunction.Variables.GetVariable(array.Name);
                    if (array == null)
                    {
                        Error($"No array '{arrayName}' exists  | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        return null;
                    }
                    string index = context.expression().GetText();
                    //Make sure this is a variable or an int
                    var indexType = Utilities.GetType(RAGEListener.CurrentFunction, index);
                    if (indexType != DataType.Int && indexType != DataType.Variable)
                    {
                        Error($"Index used for array is not a valid indexer | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        return null;
                    }
                    //If it's a static int, make sure it's inside the bounds of the array
                    if (indexType == DataType.Int)
                    {
                        int val = int.Parse(index);
                        if (val >= array.Length)
                        {
                            Error($"Index '{val}' exceeds the length of array '{arrayName}' (size={array.Length}) | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        }

                        //Build stack
                        code.Add(Push.Int(index));
                        code.Add(FrameVar.GetPointer(arrayVar));
                        code.Add(Opcodes.Array.Set());
                    }
                    else if (indexType == DataType.Variable)
                    {
                        Variable vVar = RAGEListener.CurrentFunction.Variables.GetVariable(index);
                        if (vVar == null)
                        {
                            Error($"Assumed variable '{index}' used for indexer, but got null | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                        }
                        var expr = VisitExpression(context.expression());
                        //Since its a var, just generate the code and hope the dev knows what theyre doing
                        code.AddRange(expr.Assembly);
                        code.Add(FrameVar.GetPointer(arrayVar));
                        code.Add(Opcodes.Array.Set());
                    }
                    return new Value(DataType.Array, null, code);
                default:
                    Error($"Unknown postfix type '{symbol}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    return null;
            }
        }

        public override Value VisitArgumentExpressionList(ArgumentExpressionListContext context)
        {
            //Means theres no args being passed
            if (context == null)
            {
                return null;
            }

            List<Value> args = new List<Value>();

            //Loop through each arg and evaluate it
            while (context != null)
            {
                args.Add(VisitAssignmentExpression(context.assignmentExpression()));
                context = context.argumentExpressionList();
            }

            return new Value(DataType.ArgListing, args, null);
        }

        private Value ParseType(PrimaryExpressionContext context)
        {
            string value = context.GetText();

            DataType type = Utilities.GetType(RAGEListener.CurrentFunction, value);
            Variable var = null;
            List<string> code = new List<string>();
            //if (type == VariableType.Variable)
            //{
            //    var = RAGEListener.currentFunction.Variables.GetVariable(value);
            //    value = GetValueFromVariable(value);
            //    type = Utilities.GetType(RAGEListener.currentFunction, value);
            //}

            switch (type)
            {
                case DataType.Int:
                    int ival;
                    if (value.StartsWith("0x"))
                    {
                        value = value.Replace("0x", "");
                        ival = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
                        value = ival.ToString();
                    }
                    else
                    {
                        ival = int.Parse(value, System.Globalization.NumberStyles.HexNumber);
                    }
                    code.Add(Push.Generate(value, type));
                    return new Value(DataType.Int, ival, code, var);
                case DataType.Bool:
                    code.Add(Push.Generate(value, type));
                    return new Value(DataType.Bool, Convert.ToBoolean(value), code, var);
                case DataType.Float:
                    code.Add(Push.Generate(value, type));
                    return new Value(DataType.Float, Convert.ToSingle(value), code, var);
                case DataType.String:
                    code.Add(Push.Generate(value, type));
                    return new Value(DataType.String, value, code, var);
                case DataType.Variable:
                    var = RAGEListener.CurrentFunction.Variables.GetVariable(value);
                    code.Add(FrameVar.Get(var));
                    return new Value(DataType.Variable, value, code, var);
                case DataType.NativeCall:
                    return new Value(DataType.NativeCall, value, new List<string>(), var);
                case DataType.LocalCall:
                    return new Value(DataType.LocalCall, value, new List<string>(), var);
                default:
                    Error($"Type {type} is unsupported | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
                    return null;
            }
        }

        private string GetValueFromVariable(string variable)
        {
            string tempValue = variable;
            Variable var = null;
            do
            {
                var = RAGEListener.CurrentFunction.Variables.GetVariable(tempValue);
                if (var.Value.Type == DataType.LocalCall || var.Value.Type == DataType.NativeCall) break;
                tempValue = var.Value.Value;
            }
            while (var.Type == DataType.Variable);

            LogVerbose($"Parsed variable '{variable}' and got value {tempValue}");
            return tempValue;
        }

    }
}
