﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;
using static CParser;
using System.Text.RegularExpressions;

namespace RAGE
{
    public class RAGEVisitor : CBaseVisitor<Value>
    {

        public override Value VisitDeclaration(DeclarationContext context)
        {
            Value val = new Value();

            string declaration = context.GetText();

            string type = context.GetChild(0).GetText();

            //Ugh, why doesn't this context have an assignment context?!?!
            //For loops will just be limited to int i = {some integer or variable}
            List<string> pieces = context.GetChild(1).GetText().ExplodeAndClean('=');
            string varName = pieces[0];
            string value = pieces[1];

            VariableType valueType = Utilities.GetType(RAGEListener.currentFunction, value);

            Variable variable = new Variable(varName, RAGEListener.currentFunction.FrameVars + 1, type);

            //Try to parse the arguments
            if (valueType == VariableType.NativeCall || valueType == VariableType.LocalCall)
            {
                //Clean up the function call
                Regex reg = new Regex(@"\(([a-zA-Z0-9,\s""']*)\)");
                List<string> matches = reg.Matches(value).GetRegexGroups();
                if (matches.Count > 1)
                {
                    string arguments = matches[0];
                    variable.Value.Arguments = Utilities.GetListOfArguments(arguments);
                }

            }
            string stripped = Regex.Replace(value, "\\(.*\\)", "");
            value = stripped;
            variable.Value.Value = value;
            variable.Value.Type = valueType;
            variable.IsIterator = true;
            val.Data = variable;
            return val;
        }

        //The current context of the visitor (will be null if this isn't an expression)
        public static (string label, int id, ParserRuleContext context) currentContext;

        public override Value VisitExpression(ExpressionContext context)
        {
            Value val = new Value();

            string expression = context.GetText();

            //Get the context for the selection statement
            currentContext = RAGEListener.storedContexts.Where(a => a.context == context.Parent).FirstOrDefault();

            //We'll do some optimizations here
            //No need to push 0 or 1 to the stack if the expression is just true or false
            //If false, we'll just always jump to the if statement end, but otherwise it'll just continue the flow
            if (expression == "true") return val;
            if (expression == "false")
            {
                val.Assembly.Add(Jump.Generate(JumpType.Unconditional, currentContext.label));
                return val;
            }
            Value output = VisitAssignmentExpression(context.assignmentExpression());

            if (output.Data != null)
            {
                if (output.Type == VariableType.Bool && output.Data.Equals(true)) return val;
                if (output.Type == VariableType.Bool && output.Data.Equals(false))
                {
                    val.Assembly.Add(Jump.Generate(JumpType.Unconditional, currentContext.label));
                    return val;
                }
            }
            val.Assembly.AddRange(output.Assembly);
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

            Variable variable = RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString());

            List<string> code = new List<string>();

            switch (context.GetChild(1).GetText())
            {
                case "+=":
                    //This will always be a variable
                    code.Add(FrameVar.Get(variable));
                    code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, Convert.ToInt32(right.Data.ToString())));
                    code.Add(FrameVar.Set(variable));
                    return new Value(VariableType.Int, null, code);
                case "=":
                    code.Add(Push.Generate(right.Data.ToString(), variable.Type));
                    code.Add(FrameVar.Set(variable));
                    return new Value(VariableType.Int, null, code);
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

            if (left.Type != VariableType.Bool || right.Type != VariableType.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new Value(VariableType.Bool, (bool)left.Data | (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(true)) || (right.Data != null && right.Data.Equals(true)))
                return new Value(VariableType.Bool, true, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.NotEqual, "nnfnfn"));
            return new Value(VariableType.Bool, null, code);
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

            if (left.Type != VariableType.Bool || right.Type != VariableType.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new Value(VariableType.Bool, (bool)left.Data & (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(false)) || (right.Data != null && right.Data.Equals(false)))
                return new Value(VariableType.Bool, false, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.Equal, "nnfnfn"));
            return new Value(VariableType.Bool, null, code);
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
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Bool, left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.Equal, "dfsdf"));
                    return new Value(VariableType.Bool, null, code);
                case "!=":
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Bool, !left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.NotEqual, "dfsdf"));
                    return new Value(VariableType.Bool, null, code);
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
            if (left.Type != VariableType.Int || right.Type != VariableType.Int
            && (left.Type != VariableType.Variable || right.Type != VariableType.Variable))
            {
                throw new Exception("Cannot use relational operators on non-integer values");
            }

            //If we're in an iterator context, we want to return the assembly code either way...
            //The assembly code will do the comparison on the two values
            bool isIterator = (currentContext.context is IterationStatementContext);

            switch (context.GetChild(1).ToString())
            {
                case "<":
                    //If it's not an iterator context, then it's free to return the two values (if possible)
                    if (!isIterator)
                    {
                        if (left.Data != null && right.Data != null)
                        {
                            return new Value(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());
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
                    code.Add(Jump.Generate(JumpType.LessThan, currentContext.label));
                    return new Value(VariableType.Bool, null, code);

                case "<=":
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThanEqual));
                    return new Value(VariableType.Bool, null, code);

                case ">":
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new Value(VariableType.Bool, null, code);

                case ">=":
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new Value(VariableType.Bool, null, code);
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
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '+' on two different types.");
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Int, (int)left.Data + (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new Value(VariableType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new Value(VariableType.Int, (int)left.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Addition));
                    return new Value(VariableType.Int, null, code);
                case "-":
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '-' on two different types.");
                    if (left.Data != null && right.Data != null) return new Value(VariableType.Int, (int)left.Data - (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new Value(VariableType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new Value(VariableType.Int, (int)left.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    return new Value(VariableType.Int, null, code);

            }
            throw new Exception("Unsupported operator");
        }

        public override Value VisitMultiplicativeExpression(MultiplicativeExpressionContext context)
        {
            if (context.multiplicativeExpression() == null)
            {
                return ParseType(context.castExpression());
            }

            Value left = VisitMultiplicativeExpression(context.multiplicativeExpression());
            Value right = ParseType(context.castExpression());

            List<string> code = new List<string>();

            switch (context.GetChild(1).ToString())
            {
                case "*":
                    if (left.Type != right.Type)
                    {
                        if (left.Type == VariableType.Variable)
                        {
                            if (RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString()).Type != right.Type)
                            {
                                throw new Exception("Left operand variable type is not equal to the right operand type");
                            }
                        }
                        else if (right.Type == VariableType.Variable)
                        {
                            if (RAGEListener.currentFunction.Variables.GetVariable(right.Data.ToString()).Type != left.Type)
                            {
                                throw new Exception("Right operand variable type is not equal to the left operand type");
                            }
                        }
                    }
                    if (left.Data != null && right.Data != null && left.Type != VariableType.Variable && right.Type != VariableType.Variable) return new Value(VariableType.Int, (int)left.Data * (int)right.Data, new List<string>());
                    if ((left.Data != null && left.Data.Equals(0)) || (right.Data != null && right.Data.Equals(0))) return new Value(VariableType.Int, 0, new List<string>());
                    if (left.Data != null)
                    {
                        if (left.Type == VariableType.Variable)
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
                        if (right.Type == VariableType.Variable)
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
                    return new Value(VariableType.Int, null, code);
                case "/":
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '/' on two different types.");
                    if (right.Data.Equals(0)) throw new Exception("Divide by zero?! IMPOSSIBRU!!!!!");

                    if (left.Data != null && right.Data != null) return new Value(VariableType.Int, (int)left.Data / (int)right.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                    return new Value(VariableType.Int, null, code);

                    //@Incomplete: Add modulus

            }
            throw new Exception("Unsupported operator");
        }

        public override Value VisitUnaryExpression(UnaryExpressionContext context)
        {
            if (context == null)
            {
                throw new Exception();
            }
            string item = context.GetText();
            if (!RAGEListener.currentFunction.Variables.ContainVariable(item))
            {
                throw new Exception($"Unary expression {context.unaryOperator().GetText()} on {item} is not possible");
            }
            return new Value(VariableType.Variable, item, new List<string>());
        }

        private Value ParseType(CParser.CastExpressionContext context)
        {
            string value = context.GetText();

            VariableType type = Utilities.GetType(RAGEListener.currentFunction, value);
            Variable var = null;
            if (type == VariableType.Variable)
            {
                var = RAGEListener.currentFunction.Variables.GetVariable(value);
                value = GetValueFromVariable(value);
                type = Utilities.GetType(RAGEListener.currentFunction, value);
            }

            switch (type)
            {
                case VariableType.Int:
                    return new Value(VariableType.Int, Convert.ToInt32(value), new List<string>(), var);
                case VariableType.Bool:
                    return new Value(VariableType.Bool, Convert.ToBoolean(value), new List<string>(), var);
                case VariableType.Float:
                    return new Value(VariableType.Float, Convert.ToSingle(value), new List<string>(), var);
                case VariableType.String:
                    return new Value(VariableType.String, value, new List<string>(), var);
                case VariableType.Variable:
                    //This will only be returned if the variable is set from a native or local function
                    return new Value(VariableType.Variable, value, new List<string>(), var);
                case VariableType.NativeCall:
                    return new Value(VariableType.NativeCall, value, new List<string>(), var);
                case VariableType.LocalCall:
                    return new Value(VariableType.LocalCall, value, new List<string>(), var);
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
                if (var.Value.Type == VariableType.LocalCall || var.Value.Type == VariableType.NativeCall) break;
                tempValue = var.Value.Value;
            }
            while (var.Type == VariableType.Variable);
            Logger.Log($"Parsed variable '{variable}' and got value {tempValue}");
            return tempValue;
        }

    }
}
