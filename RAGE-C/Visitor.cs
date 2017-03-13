using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime;

namespace RAGE
{
    public class ExpressionResponse
    {
        internal VariableType Type { get; set; }
        internal object Data { get; set; }
        internal List<string> Assembly { get; set; }

        public ExpressionResponse(VariableType type, object data, List<string> asm)
        {
            Type = type;
            Data = data;
            Assembly = asm;
        }
        public ExpressionResponse() { }
    }
    public class RAGEVisitor : CBaseVisitor<Object>
    {
        public override object VisitExpression([NotNull] CParser.ExpressionContext context)
        {
            List<string> code = new List<string>();
            string expression = context.GetText();

            //Get the context for the selection statement
            var selectionContext = RAGEListener.conditionalContexts.Where(a => a.context == context.Parent).FirstOrDefault();
            //var test = VisitChildren(context.children[0])
            // var gg = context.children;
            //We'll do some optimizations here
            //No need to push 0 or 1 to the stack if the expression is just true or false
            //If false, we'll just always jump to the if statement end, but otherwise it'll just continue the flow
            if (expression == "true") return code;
            if (expression == "false")
            {
                code.Add(Jump.Generate(JumpType.Unconditional, selectionContext.label));
                return code;
            }
            ExpressionResponse output = (ExpressionResponse)VisitAssignmentExpression(context.assignmentExpression());
            if (output.Type == VariableType.Bool && output.Data.Equals(true)) return code;
            if (output.Type == VariableType.Bool && output.Data.Equals(false))
            {
                code.Add(Jump.Generate(JumpType.Unconditional, selectionContext.label));
                return code;
            }
            code.AddRange(output.Assembly);
            return code;
        }

        public override object VisitAssignmentExpression([NotNull] CParser.AssignmentExpressionContext context)
        {
            return VisitConditionalExpression(context.conditionalExpression());
        }

        public override object VisitConditionalExpression([NotNull] CParser.ConditionalExpressionContext context)
        {
            return VisitLogicalOrExpression(context.logicalOrExpression());
        }

        public override object VisitLogicalOrExpression([NotNull] CParser.LogicalOrExpressionContext context)
        {
            if (context.logicalOrExpression() == null)
            {
                return VisitLogicalAndExpression(context.logicalAndExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitLogicalOrExpression(context.logicalOrExpression());
            ExpressionResponse right = (ExpressionResponse)VisitLogicalAndExpression(context.logicalAndExpression());

            List<string> code = new List<string>();

            if (left.Type != VariableType.Bool || right.Type != VariableType.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new ExpressionResponse(VariableType.Bool, (bool)left.Data | (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(true)) || (right.Data != null && right.Data.Equals(true)))
                return new ExpressionResponse(VariableType.Bool, true, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.NotEqual, "nnfnfn"));
            return new ExpressionResponse(VariableType.Bool, null, code);
        }
        public override object VisitLogicalAndExpression([NotNull] CParser.LogicalAndExpressionContext context)
        {
            if (context.logicalAndExpression() == null)
            {
                return VisitEqualityExpression(context.inclusiveOrExpression().exclusiveOrExpression().andExpression().equalityExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitLogicalAndExpression(context.logicalAndExpression());
            ExpressionResponse right = (ExpressionResponse)VisitEqualityExpression(context.inclusiveOrExpression().exclusiveOrExpression().andExpression().equalityExpression());

            List<string> code = new List<string>();

            if (left.Type != VariableType.Bool || right.Type != VariableType.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new ExpressionResponse(VariableType.Bool, (bool)left.Data & (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(false)) || (right.Data != null && right.Data.Equals(false)))
                return new ExpressionResponse(VariableType.Bool, false, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.Equal, "nnfnfn"));
            return new ExpressionResponse(VariableType.Bool, null, code);
        }
        public override object VisitEqualityExpression([NotNull] CParser.EqualityExpressionContext context)
        {
            if (context.equalityExpression() == null)
            {
                return VisitRelationalExpression(context.relationalExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitEqualityExpression(context.equalityExpression());
            ExpressionResponse right = (ExpressionResponse)VisitRelationalExpression(context.relationalExpression());

            List<string> code = new List<string>();

            switch (context.GetChild(1).ToString())
            {
                case "==":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.Equal, "dfsdf"));
                    return new ExpressionResponse(VariableType.Bool, null, code);
                case "!=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, !left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.NotEqual, "dfsdf"));
                    return new ExpressionResponse(VariableType.Bool, null, code);
            }
            throw new Exception("Unsupported operator");
        }
        public override object VisitRelationalExpression([NotNull] CParser.RelationalExpressionContext context)
        {
            if (context.relationalExpression() == null)
            {
                return VisitAdditiveExpression(context.shiftExpression().additiveExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitRelationalExpression(context.relationalExpression());
            ExpressionResponse right = (ExpressionResponse)VisitAdditiveExpression(context.shiftExpression().additiveExpression());

            List<string> code = new List<string>();

            if (left.Type != VariableType.Int || right.Type != VariableType.Int && (left.Type != VariableType.Variable || right.Type != VariableType.Variable))
            {
                throw new Exception("Cannot use relational operands on non-integer values");
            }

            switch (context.GetChild(1).ToString())
            {
                case "<":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThan));
                    return new ExpressionResponse(VariableType.Bool, null, code);

                case "<=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThanEqual));
                    return new ExpressionResponse(VariableType.Bool, null, code);

                case ">":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new ExpressionResponse(VariableType.Bool, null, code);

                case ">=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new ExpressionResponse(VariableType.Bool, null, code);
            }
            throw new Exception("Unsupported operator");
        }
        public override object VisitAdditiveExpression([NotNull] CParser.AdditiveExpressionContext context)
        {
            if (context.additiveExpression() == null)
            {
                return VisitMultiplicativeExpression(context.multiplicativeExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitAdditiveExpression(context.additiveExpression());
            ExpressionResponse right = (ExpressionResponse)VisitMultiplicativeExpression(context.multiplicativeExpression());

            List<string> code = new List<string>();

            switch (context.GetChild(1).ToString())
            {
                case "+":
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '+' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Int, (int)left.Data + (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new ExpressionResponse(VariableType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new ExpressionResponse(VariableType.Int, (int)left.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Addition));
                    return new ExpressionResponse(VariableType.Int, null, code);
                case "-":
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '-' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Int, (int)left.Data - (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new ExpressionResponse(VariableType.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new ExpressionResponse(VariableType.Int, (int)left.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    return new ExpressionResponse(VariableType.Int, null, code);

            }
            throw new Exception("Unsupported operator");
        }
        public override object VisitMultiplicativeExpression([NotNull] CParser.MultiplicativeExpressionContext context)
        {
            if (context.multiplicativeExpression() == null)
            {
                return ParseType(context.castExpression());
            }

            ExpressionResponse left = (ExpressionResponse)VisitMultiplicativeExpression(context.multiplicativeExpression());
            ExpressionResponse right = (ExpressionResponse)ParseType(context.castExpression());

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
                        //@TODO: Make something to get variables that hold variables, etc... (to determine it's type)
                        //while (RAGEListener.currentFunction.Variables.GetVariable(left.Data.ToString()).ValueType != VariableTypes.Variable)
                        //{

                        //}

                        //If the two sides aren't the same type and they're not a function call or variable, it's not allowed
                        //if (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable
                        //    || left.Type != VariableTypes.NativeCall || right.Type != VariableTypes.NativeCall
                        //    || left.Type != VariableTypes.LocalCall || right.Type != VariableTypes.LocalCall)
                        //{
                        //    throw new Exception("Cannot use operand '*' on two different types.");
                        //}
                    }
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Int, (int)left.Data * (int)right.Data, new List<string>());
                    if ((left.Data != null && left.Data.Equals(0)) || (right.Data != null && right.Data.Equals(0))) return new ExpressionResponse(VariableType.Int, 0, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Multiplication));
                    return new ExpressionResponse(VariableType.Int, null, code);
                case "/":
                    if (left.Type != right.Type && (left.Type != VariableType.Variable || right.Type != VariableType.Variable)) throw new Exception("Cannot use operand '/' on two different types.");
                    if (right.Data.Equals(0)) throw new Exception("Divide by zero?! IMPOSSIBRU!!!!!");

                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableType.Int, (int)left.Data / (int)right.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                    return new ExpressionResponse(VariableType.Int, null, code);

                    //@Incomplete: Add modulus

            }
            throw new Exception("Unsupported operator");
        }

        private object ParseType(CParser.CastExpressionContext context)
        {
            string value = context.GetText();

            VariableType type = Utilities.GetType(RAGEListener.currentFunction, value);

            if (type == VariableType.Variable)
            {
                value = GetValueFromVariable(value);
                type = Utilities.GetType(RAGEListener.currentFunction, value);
            }

            switch (type)
            {
                case VariableType.Int:
                    return new ExpressionResponse(VariableType.Int, Convert.ToInt32(value), new List<string>());
                case VariableType.Bool:
                    return new ExpressionResponse(VariableType.Bool, Convert.ToBoolean(value), new List<string>());
                case VariableType.Float:
                    return new ExpressionResponse(VariableType.Float, Convert.ToSingle(value), new List<string>());
                case VariableType.String:
                    return new ExpressionResponse(VariableType.String, value, new List<string>());
                case VariableType.Variable:
                    //This will only be returned if the variable is set from a native or local function
                    return new ExpressionResponse(VariableType.Variable, value, new List<string>());
                case VariableType.NativeCall:
                    return new ExpressionResponse(VariableType.NativeCall, value, new List<string>());
                case VariableType.LocalCall:
                    return new ExpressionResponse(VariableType.LocalCall, value, new List<string>());
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
