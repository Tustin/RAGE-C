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
        internal VariableTypes Type { get; set; }
        internal object Data { get; set; }
        internal List<string> Assembly { get; set; }

        public ExpressionResponse(VariableTypes type, object data, List<string> asm)
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

            if (left.Type != VariableTypes.Bool || right.Type != VariableTypes.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new ExpressionResponse(VariableTypes.Bool, (bool)left.Data | (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(true)) || (right.Data != null && right.Data.Equals(true)))
                return new ExpressionResponse(VariableTypes.Bool, true, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.NotEqual, "nnfnfn"));
            return new ExpressionResponse(VariableTypes.Bool, null, code);
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

            if (left.Type != VariableTypes.Bool || right.Type != VariableTypes.Bool)
            {
                throw new Exception("Invalid types");
            }
            if (left.Data != null && right.Data != null)
                return new ExpressionResponse(VariableTypes.Bool, (bool)left.Data & (bool)right.Data, new List<string>());
            if ((left.Data != null && left.Data.Equals(false)) || (right.Data != null && right.Data.Equals(false)))
                return new ExpressionResponse(VariableTypes.Bool, false, new List<string>());

            if (left.Data != null)
            {
                code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
            }
            if (right.Data != null)
            {
                code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
            }
            code.Add(Jump.Generate(JumpType.Equal, "nnfnfn"));
            return new ExpressionResponse(VariableTypes.Bool, null, code);
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
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.Equal, "dfsdf"));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);
                case "!=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, !left.Data.Equals(right.Data), new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Jump.Generate(JumpType.NotEqual, "dfsdf"));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);
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

            if (left.Type != VariableTypes.Int || right.Type != VariableTypes.Int && (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable))
            {
                throw new Exception("Cannot use relational operands on non-integer values");
            }

            switch (context.GetChild(1).ToString())
            {
                case "<":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThan));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);

                case "<=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.LessThanEqual));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);

                case ">":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);

                case ">=":
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Bool, (int)left.Data < (int)right.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Compare.Generate(CompareType.GreaterThan));
                    return new ExpressionResponse(VariableTypes.Bool, null, code);
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
                    if (left.Type != right.Type && (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable)) throw new Exception("Cannot use operand '+' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Int, (int)left.Data + (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new ExpressionResponse(VariableTypes.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new ExpressionResponse(VariableTypes.Int, (int)left.Data, new List<string>());

                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Addition));
                    return new ExpressionResponse(VariableTypes.Int, null, code);
                case "-":
                    if (left.Type != right.Type && (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable)) throw new Exception("Cannot use operand '-' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Int, (int)left.Data - (int)right.Data, new List<string>());
                    if (left.Data != null && left.Data.Equals(0)) return new ExpressionResponse(VariableTypes.Int, (int)right.Data, new List<string>());
                    if (right.Data != null && right.Data.Equals(0)) return new ExpressionResponse(VariableTypes.Int, (int)left.Data, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
                    return new ExpressionResponse(VariableTypes.Int, null, code);

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
                    if (left.Type != right.Type && (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable)) throw new Exception("Cannot use operand '*' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Int, (int)left.Data * (int)right.Data, new List<string>());
                    if ((left.Data != null && left.Data.Equals(0)) || (right.Data != null && right.Data.Equals(0))) return new ExpressionResponse(VariableTypes.Int, 0, new List<string>());
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Multiplication));
                    return new ExpressionResponse(VariableTypes.Int, null, code);
                case "/":
                    if (left.Type != right.Type && (left.Type != VariableTypes.Variable || right.Type != VariableTypes.Variable)) throw new Exception("Cannot use operand '/' on two different types.");
                    if (left.Data != null && right.Data != null) return new ExpressionResponse(VariableTypes.Int, (int)left.Data / (int)right.Data, new List<string>());
                    if (right.Data.Equals(0)) throw new Exception("Divide by zero?! IMPOSSIBRU!!!!!");
                    if (left.Data != null)
                    {
                        code.Add(Push.Generate(left.Data.ToString(), Utilities.GetType(null, left.Data.ToString())));
                    }
                    if (right.Data != null)
                    {
                        code.Add(Push.Generate(right.Data.ToString(), Utilities.GetType(null, right.Data.ToString())));
                    }
                    code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
                    return new ExpressionResponse(VariableTypes.Int, null, code);

                    //@Incomplete: Add modulus

            }
            throw new Exception("Unsupported operator");
        }

        private object ParseType(CParser.CastExpressionContext context)
        {
            string gg = context.GetText();

            VariableTypes type = Utilities.GetType(null, gg);

            switch (type)
            {
                case VariableTypes.Int:
                    return new ExpressionResponse(VariableTypes.Int, Convert.ToInt32(gg), new List<string>());
                case VariableTypes.Bool:
                    return new ExpressionResponse(VariableTypes.Bool, Convert.ToBoolean(gg), new List<string>());
                case VariableTypes.Float:
                    return new ExpressionResponse(VariableTypes.Float, Convert.ToSingle(gg), new List<string>());
                case VariableTypes.String:
                    return new ExpressionResponse(VariableTypes.String, gg, new List<string>());
                case VariableTypes.Variable:
                    return new ExpressionResponse(VariableTypes.Variable, gg, new List<string>());
                default:
                    throw new Exception("Not implemented yet");

            }
        }
    }
}
