using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using System.Text.RegularExpressions;
using static CParser;

namespace RAGE.Parser
{
    public class RAGEVisitor : CBaseVisitor<Value>
    {
        //The current context of the visitor (will be null if this isn't an expression)
        public StoredContext CurrentContext;

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

            Variable variable = new Variable(varName, RAGEListener.currentFunction.FrameVars + 1, varType);

            RAGEListener.currentFunction.Variables.Add(variable);

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

            string stripped = Regex.Replace(varName, "\\(.*\\)", "");

            value.Data = variable;
            return value;
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
                    val.Assembly.Add(Jump.Generate(JumpType.Unconditional, CurrentContext.Label));
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
                switch (output.Type)
                {
                    case DataType.NativeCall:
                        output.Assembly.Add(Jump.Generate(JumpType.False, CurrentContext.Label));
                        break;
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

            Variable variable = left.OriginalVariable ?? RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString());

            if (variable == null)
            {
                throw new Exception($"Failed to find variable {left.Data.ToString()}");
            }

            List<string> code = new List<string>();

            switch (context.GetChild(1).GetText())
            {
                case "+=":
                    //This will always be a variable
                    code.Add(FrameVar.Get(variable));
                    code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, Convert.ToInt32(right.Data.ToString())));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "=":
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
            throw new Exception("Invalid operator");
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
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new Value(DataType.Bool, (bool)left.Data | (bool)right.Data, new List<string>());

            if ((left.Data != null && left.Data.Equals(true)) || (right.Data != null && right.Data.Equals(true)))
                return new Value(DataType.Bool, true, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new Value(DataType.Bool, (bool)left.Data & (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(false)) || (right.Data != null && right.Data.Equals(false)))
                return new Value(DataType.Bool, false, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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

            switch (context.GetChild(1).ToString())
            {
                case "==":
                    if (left.Data != null && right.Data != null) return new Value(DataType.Bool, left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Jump.Generate(JumpType.Equal, CurrentContext.Label));
                    return new Value(DataType.Bool, null, code);
            }
            throw new Exception("Unsupported operator");
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

            //Right now, values being compared by relational operators must be integers
            //They can also be variables which are integer values
            if (left.Type != DataType.Int || right.Type != DataType.Int
            && (left.Type != DataType.Variable || right.Type != DataType.Variable))
            {
                throw new Exception("Cannot use relational operators on non-integer values");
            }


            //Lets just output the variables here because fuck optimization
            //Saves some headache with the compiler parsing logic on variables that might be changed
            bool isIterator = (CurrentContext.Context is IterationStatementContext) | (CurrentContext.Context is SelectionStatementContext);

            switch (context.GetChild(1).ToString())
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
                            code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
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
                            code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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
            throw new Exception("Unsupported operator");
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

            switch (context.GetChild(1).ToString())
            {
                case "+":
                    if (left.Type != right.Type && (left.Type != DataType.Variable || right.Type != DataType.Variable)) throw new Exception("Cannot use operand '+' on two different types.");
                    if (left.Data != null && right.Data != null) return new Value(DataType.Int, (int)left.Data + (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new Value(DataType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new Value(DataType.Int, (int)left.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(left.Assembly);
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
                    }
                    else
                    {
                        code.AddRange(right.Assembly);
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    return new Value(DataType.Int, null, code);

            }
            throw new Exception("Unsupported operator");
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

            switch (context.GetChild(1).ToString())
            {
                case "*":
                    if (left.Type != right.Type)
                    {
                        if (left.Type == DataType.Variable)
                        {
                            if (RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString()).Type != right.Type)
                            {
                                throw new Exception("Left operand variable type is not equal to the right operand type");
                            }
                        }
                        else if (right.Type == DataType.Variable)
                        {
                            if (RAGEListener.currentFunction.Variables.GetVariable(right.Data.ToString()).Type != left.Type)
                            {
                                throw new Exception("Right operand variable type is not equal to the left operand type");
                            }
                        }
                    }
                    if (left.Data != null && right.Data != null && left.Type != DataType.Variable && right.Type != DataType.Variable) return new Value(DataType.Int, (int)left.Data * (int)right.Data, new List<string>());
                    if ((left.Data != null && left.Data.Equals(0)) || (right.Data != null && right.Data.Equals(0))) return new Value(DataType.Int, 0, new List<string>());
                    if (left.Data != null)
                    {
                        if (left.Type == DataType.Variable)
                        {
                            Variable var = RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString());
                            code.Add(FrameVar.Get(var));
                        }
                        else
                        {
                            code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                        }
                    }
                    if (right.Data != null)
                    {
                        if (right.Type == DataType.Variable)
                        {
                            Variable var = RAGEListener.currentFunction.Variables.GetVariable(right.Data.ToString());
                            code.Add(FrameVar.Get(var));
                        }
                        else
                        {
                            code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
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
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(RAGEListener.currentFunction, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                    return new Value(DataType.Int, null, code);

                    //@Incomplete: Add modulus
            }
            throw new Exception("Unsupported operator");
        }

        public override Value VisitCastExpression(CastExpressionContext context)
        {
            if (context.castExpression() != null)
            {
                throw new Exception("Casts are not supported");
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

                    if (!RAGEListener.currentFunction.Variables.ContainVariable(var) && op.Type == DataType.Address)
                    {
                        throw new Exception($"Unary expression {context.unaryOperator().GetText()} on {var} is not possible");
                    }
                    Variable v = RAGEListener.currentFunction.Variables.GetVariable(var);
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
                    throw new Exception($"Invalid unary operator {op}");
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
            Variable variable = RAGEListener.currentFunction.Variables.GetVariable(expression);

            switch (symbol)
            {
                case "++":
                    if (!RAGEListener.currentFunction.Variables.ContainVariable(expression))
                    {
                        throw new Exception($"Postfix operators ({symbol}) can only be used on variables");
                    }
                    code.Add(FrameVar.Get(variable));
                    code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, 1));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "--":
                    if (!RAGEListener.currentFunction.Variables.ContainVariable(expression))
                    {
                        throw new Exception($"Postfix operators ({symbol}) can only be used on variables");
                    }
                    code.Add(FrameVar.Get(variable));
                    code.Add(Push.Generate("1", DataType.Int));
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    code.Add(FrameVar.Set(variable));
                    return new Value(DataType.Int, null, code);
                case "(":
                    if (Core.Functions.ContainFunction(expression))
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
                            throw new Exception($"{expression} takes {native.Params.Count} arguments, none given");
                        }

                        List<Value> argsList = (List<Value>)args.Data;

                        argsList.Reverse();

                        if (argsList.Count != native.Params.Count)
                        {
                            throw new Exception($"{expression} expects {native.Params.Count} arguments, {argsList.Count} given");
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
                    throw new Exception("Found open parens, but expression is not a function");

                default:
                    throw new Exception("Unknown postfix type");

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

            DataType type = Utilities.GetType(RAGEListener.currentFunction, value);
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
                    var = RAGEListener.currentFunction.Variables.GetVariable(value);
                    code.Add(FrameVar.Get(var));
                    return new Value(DataType.Variable, value, code, var);
                case DataType.NativeCall:
                    return new Value(DataType.NativeCall, value, new List<string>(), var);
                case DataType.LocalCall:
                    return new Value(DataType.LocalCall, value, new List<string>(), var);
                default:
                    throw new Exception("Not implemented yet");

            }
        }

        private string GetValueFromVariable(string variable)
        {
            string tempValue = variable;
            Variable var = null;
            do
            {
                var = RAGEListener.currentFunction.Variables.GetVariable(tempValue);
                if (var.Value.Type == DataType.LocalCall || var.Value.Type == DataType.NativeCall) break;
                tempValue = var.Value.Value;
            }
            while (var.Type == DataType.Variable);

            Logger.Logger.Log($"Parsed variable '{variable}' and got value {tempValue}");
            return tempValue;
        }

    }
}
