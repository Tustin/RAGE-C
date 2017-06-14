using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using System.Text.RegularExpressions;

using static RAGEParser;
using static RAGE.Main.Logger;
using RAGE.Parser.Opcodes;
using System.Globalization;

namespace RAGE.Parser
{
	public class RAGEVisitor : RAGEBaseVisitor<Value>
	{
		//The current context of the visitor (will be null if this isn't an expression)
		public StoredContext CurrentContext;

		public override Value VisitDeclarationSpecifiers([NotNull] DeclarationSpecifiersContext context)
		{
			var res = new DeclarationResponse();

			var specifiers = context.declarationSpecifier();

			foreach (var specifier in specifiers)
			{
				if (specifier.storageClassSpecifier() != null && res.Specifier == Specifier.None)
				{
					res.Specifier = Utilities.GetSpecifierFromDeclaration(specifier.GetText());
				}
				else if (specifier.typeSpecifier() != null && res.Type == DataType.Void)
				{
					res.Type = Utilities.GetTypeFromDeclaration(specifier.GetText());
				}
				else if (specifier.customTypeSpecifier() != null && res.Type == DataType.Void)
				{
					var custom = specifier.GetText().Replace("@", "");
					res.Type = DataType.CustomType;
					res.CustomType = custom;
				}
				else
				{
					Error($"Invalid specifier given to variable declaration ({specifier.GetText()}) - is this a custom type? Prefix it with an @ if so (e.g. @myStruct)| line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
			}

			return new Value(DataType.None, res, null);

			//At most, there should only be two specifiers, like so:
			//static int myStatic = 69;
			//Globals wont be referenced by a specifier, so doing the following is invalid:
			//global int someGlobal = 69;

			/*
			if (specifiers.Length > 2)
			{
				Error($"Too many specifiers | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}
			switch (specifiers.Length)
			{
				//Assumes theres only a type set, parse it and return
				case 1:
				var type = Utilities.GetTypeFromDeclaration(specifiers[0].GetText());
				return new Value(type, Specifier.None, null);
				//both a type and a specifier, parse them and return both
				case 2:
				var spec = Utilities.GetSpecifierFromDeclaration(specifiers[0].GetText());
				type = Utilities.GetTypeFromDeclaration(specifiers[1].GetText());
				return new Value(type, spec, null);
				default:
				Error($"Invalid specifier count | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
				return null;
			}
			*/
		}

		public override Value VisitArrayDeclarationList([NotNull] ArrayDeclarationListContext context)
		{
			return base.VisitArrayDeclarationList(context);
		}

		public override Value VisitDeclaration([NotNull] DeclarationContext context)
		{
			var specifiers = (VisitDeclarationSpecifiers(context.declarationSpecifiers())).Data as DeclarationResponse;

			DataType declType = specifiers.Type; //Should always have a type
			Specifier declSpec = specifiers.Specifier; //Will be None if there is no specifier

			var declarator = context.initDeclaratorList().initDeclarator();
			var varName = declarator.declarator().GetText();

			if (declType == DataType.Void)
			{
				Error($"No valid type given to variable declaration '{varName}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}

			//Will be null if no value is being set
			var initializer = declarator.initializer();

			//Handle statics and frame vars the same minus a few differences
			if (RAGEListener.CurrentFunction == null)
			{
				if (Script.StaticVariables.ContainsVariable(varName))
				{
					Error($"Static variable '{varName}' has already been declared | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
			}
			else
			{
				if (RAGEListener.CurrentFunction.AlreadyDeclared(varName, true))
				{
					//Check to see if the already declared variable is an iterator
					//This can happen due to the IterationStatement generating the for loop iterator before the
					//Declaration does
					var iterator = RAGEListener.CurrentFunction.GetVariable(varName) as Variable;
					if (iterator.IsIterator)
					{
						return new Value(DataType.Iterator, null, null);
					}
					Error($"'{varName}' has already been declared in this scope | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
			}

			Variable variable;

			//Specified as static
			if (declSpec == Specifier.Static)
			{
				variable = new Variable(varName, Script.StaticVariables.Count + 1, declType);
				variable.Specifier = declSpec;
			}
			else
			{
				if (RAGEListener.CurrentFunction == null)
				{
					Error($"Non-static variable used outside of function scope | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
				}
				variable = new Variable(varName, RAGEListener.CurrentFunction.FrameVars + 1, declType);
				variable.Specifier = declSpec;
			}

			if (variable.Type == DataType.CustomType)
			{
				variable.CustomType = specifiers.CustomType;
			}

			//See if this variable is being initialized
			//If not, then we'll give it a default value
			if (initializer != null)
			{
				var resp = VisitInitDeclarator(declarator);

				if (resp.Data != null)
				{
					variable.Value.Value = resp.Data.ToString();
				}
				if (resp.Type == DataType.Array)
				{
					resp.Assembly.Add(Opcodes.Array.Get());
				}
				variable.ValueAssembly = resp.Assembly;
				variable.Value.Type = resp.Type;
				variable.Value.IsDefault = false;
			}
			else
			{
				if (variable.Type == DataType.CustomType) goto end;
				variable.Value.Value = Utilities.GetDefaultValue(variable.Type);
				variable.ValueAssembly.Add(Push.Generate(variable.Value.Value, variable.Type));
				variable.Value.Type = variable.Type;
				variable.Value.IsDefault = true;
			}

			end:
			return new Value(DataType.Variable, variable, null);
		}

		public override Value VisitStructItemDeclarator([NotNull] StructItemDeclaratorContext context)
		{
			var currentStruct = Script.Structs.Last();
			var memberName = context.Identifier().GetText();
			var memberType = Utilities.GetTypeFromDeclaration(context.typeSpecifier().GetText());

			if (memberType == DataType.Void)
			{
				Error($"Invalid type for member '{memberName}' ({memberType}) in struct '{currentStruct.Name}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}

			if (currentStruct.Members.ContainsVariable(memberName))
			{
				Error($"Struct '{currentStruct.Name}' already contains a member '{memberName}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}

			Variable member = new Variable(memberName, currentStruct.Members.Count, memberType);

			//typeSpecifier Identifier
			//e.g. int myMember
			//Needs a default value
			if (context.ChildCount == 2)
			{
				member.Value.Value = Utilities.GetDefaultValue(memberType);
				member.Value.Type = memberType;
				member.Value.IsDefault = true;
				member.ValueAssembly.Add(Push.Generate(member.Value.Value.ToString(), memberType));
			}
			//typeSpecifier Identifier = constantExpression
			//e.g. int myMember = 5
			else if (context.ChildCount == 4)
			{
				var memberExpr = context.constantExpression().GetText();
				var exprType = Utilities.GetType(null, memberExpr);
				if (exprType != memberType)
				{
					Error($"Member expression '{memberExpr}' ({exprType}) doesn't match member's defined type ({memberType}) | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
				}

				member.Value.Value = memberExpr;
				member.Value.Type = memberType;
				member.Value.IsDefault = false;
				member.ValueAssembly.Add(Push.Generate(memberExpr, memberType));
			}
			else
			{
				Error($"Invalid member declaration (child count: {context.ChildCount}) | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}

			//Script.StaticVariables.Add(member);
			currentStruct.Members.Add(member);

			return base.VisitStructItemDeclarator(context);

		}

		public override Value VisitEnumerator([NotNull] EnumeratorContext context)
		{
			var currentEnum = Script.Enums.Last();
			var lastEnumerator = currentEnum.Enumerators.LastOrDefault();
			var enumName = context.enumerationConstant().GetText();
			if (currentEnum.Enumerators.ContainsEnumerator(enumName))
			{
				Error($"Enum '{currentEnum.Name}' already contains enumerator '{enumName}' | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}
			Variable enumVar = new Variable($"{currentEnum.Name}_{enumName}", Script.GetNextStaticIndex(), DataType.Int);
			enumVar.Specifier = Specifier.Static;
			//ENUMERATOR    
			if (context.ChildCount == 1)
			{
				//Since it doesn't have a value, get the last enums val and add 1
				if (lastEnumerator == null)
				{
					enumVar.Value.Value = Utilities.GetDefaultValue(DataType.Int);
					enumVar.Value.Type = DataType.Int;
					enumVar.Value.IsDefault = true;
					enumVar.ValueAssembly.Add(Push.Int(enumVar.Value.Value.ToString()));
				}
				else
				{
					var lastEnumVar = lastEnumerator.Variable as Variable;
					int newValue = Convert.ToInt32(lastEnumVar.Value.Value) + 1;
					enumVar.Value.Value = newValue.ToString();
					enumVar.Value.Type = DataType.Int;
					enumVar.Value.IsDefault = true;
					enumVar.ValueAssembly.Add(Push.Int(newValue.ToString()));
				}
				Script.StaticVariables.Add(enumVar);
				currentEnum.Enumerators.Add(new Enumerator(enumName, enumVar));
			}
			//ENUMERATOR = CONSTANT
			else if (context.ChildCount == 3)
			{
				var enumValue = context.constantExpression().GetText();
				enumValue = enumValue.Replace("0x", "");
				int val;
				if (!int.TryParse(enumValue, out val) && !int.TryParse(enumValue, NumberStyles.HexNumber & NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out val))
				{
					Error($"Unable to parse enumeration '{enumName}' as int | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
				}
				enumVar.Value.Value = enumValue;
				enumVar.Value.Type = DataType.Int;
				enumVar.Value.IsDefault = false;
				enumVar.ValueAssembly.Add(Push.Int(enumValue));

				Script.StaticVariables.Add(enumVar);
				currentEnum.Enumerators.Add(new Enumerator(enumName, enumVar));
			}
			else
			{
				Error($"Invalid use of enumeration (child count: {context.ChildCount}) | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
			}
			return base.VisitEnumerator(context);
		}
		public override Value VisitSelectionStatement([NotNull] SelectionStatementContext context)
		{
			return base.VisitSelectionStatement(context);
		}
		public override Value VisitConstantExpression([NotNull] ConstantExpressionContext context)
		{
			if (context.conditionalExpression() != null)
			{
				return VisitConditionalExpression(context.conditionalExpression());
			}
			return base.VisitConstantExpression(context);
		}
		public override Value VisitLabeledStatement(LabeledStatementContext context)
		{
			var label = context.GetChild(0).GetText();
			var ret = new Value();
			if (label == "case")
			{
				var expr = VisitConstantExpression(context.constantExpression());

				if (expr == null && expr.Type != DataType.Int && expr.Type != DataType.Enum)
				{
					Error($"Case could not be evaluted | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
				}

				int value = Utilities.GetCaseExpression(expr);

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

			var resp = VisitAssignmentExpression(context.initializer().assignmentExpression());

			if (resp.Type == DataType.Global)
			{
				var output = Globals.Global.Parse(resp.Data as Globals.Global, true);
				resp.Assembly.AddRange(output);
			}

			return resp;
		}

		public override Value VisitExpression(ExpressionContext context)
		{
			Value val = new Value();

			string expression = context.GetText();

			if (expression == "true")
			{
				if (CurrentContext?.Context is IterationStatementContext && CurrentContext?.Type != ScopeTypes.While)
				{
					val.Assembly.Add(Opcodes.Jump.Generate(Opcodes.JumpType.Unconditional, CurrentContext.Label));
					return val;
				}
			}

			if (expression == "false")
			{
				if (CurrentContext?.Context is IterationStatementContext)
				{
					val.Assembly.Add(Jump.Generate(JumpType.Unconditional, CurrentContext.Label));
					return val;
				}
			}

			Value output = VisitAssignmentExpression(context.assignmentExpression());

			//If the if expression doesnt have ==, then the result will come back as a type other than bool
			if (output.Type != DataType.Bool && CurrentContext != null && CurrentContext?.Type != ScopeTypes.While)
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

			//!someVar
			if (output.Type == DataType.Not)
			{
				val.Assembly.AddRange(output.Assembly);
				if (CurrentContext.Type == ScopeTypes.While)
				{
					val.Assembly.Add(Jump.Generate(JumpType.False, CurrentContext.EndLabel));

				}
				else
				{
					val.Assembly.Add(Jump.Generate(JumpType.False, CurrentContext.Label));
				}
				return val;
			}

			if (output.Type == DataType.Array)
			{
				string ff = "";
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

			IVariable variable = null;

			if (left.Type != DataType.Global && left.Type != DataType.Struct)
			{
				variable = Utilities.GetAnyVariable(RAGEListener.CurrentFunction.Variables, left.Data.ToString());

				if (variable == null)
				{
					Error($"Unable to find variable '{left.Data.ToString()}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
			}

			var code = new List<string>();
			var leftCode = new List<string>();
			var rightCode = new List<string>();

			var op = context.GetChild(1).GetText();
			switch (op)
			{
				case "+=":
				//This will always be a variable
				if (variable.Specifier == Specifier.Static)
				{
					code.Add(StaticVar.Get(variable));
					code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, Convert.ToInt32(right.Data.ToString())));
					code.Add(StaticVar.Set(variable));
				}
				else
				{
					code.Add(FrameVar.Get(variable));
					code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, Convert.ToInt32(right.Data.ToString())));
					code.Add(FrameVar.Set(variable));
				}

				return new Value(DataType.Int, null, code);

				case "=":
				//Foreach iterators are read-only so throw an error if it's being assigned a value
				if (left.Type == DataType.ForeachVariable)
				{
					Error($"Foreach iterator '{left.Data.ToString()}' cannot be assigned a new value | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}

				if (left.Type == DataType.Array)
				{
					//Array is on the left so we need to set the value
					left.Assembly.Add(Opcodes.Array.Set());
					leftCode.AddRange(left.Assembly);
				}

				if (right.Type == DataType.Array)
				{
					//Array is on the right so we need to get the value
					right.Assembly.Add(Opcodes.Array.Get());
					rightCode.AddRange(right.Assembly);
				}

				//Use global opcodes
				if (left.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(left.Data as Globals.Global, false);
					leftCode.AddRange(output);
				}

				if (right.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(right.Data as Globals.Global, true);
					rightCode.AddRange(output);
				}

				if (left.Type == DataType.Static)
				{
					leftCode.Add(StaticVar.Set(Script.StaticVariables.GetVariable(left.Data as string)));
				}

				if (right.Type == DataType.Static)
				{
					rightCode.Add(StaticVar.Get(Script.StaticVariables.GetVariable(right.Data as string)));
				}

				if (left.Type == DataType.Struct)
				{
					var frame = (left.Data as Variable).FrameId;
					//Hack: Fix me!
					if (frame > 0)
					{
						leftCode.Add(Immediate.Set(frame));
					}
					else
					{
						left.Assembly[0] = left.Assembly[0].Replace("Get", "Set");
					}
					leftCode.AddRange(left.Assembly);

				}

				if (right.Type == DataType.Struct)
				{
					rightCode.AddRange(right.Assembly);
					var frame = (right.Data as Variable).FrameId;
					//Hack: Fix me!
					if (frame > 0)
					{
						rightCode.Add(Immediate.Set(frame));
					}
					else
					{
						right.Assembly[0] = right.Assembly[0].Replace("Get", "Set");
					}
				}

				if (right.Assembly.Count > 0 && rightCode.Count == 0)
				{
					rightCode.AddRange(right.Assembly);
				}
				else if (right.Data != null)
				{
					//var potentialVariable = Utilities.GetAnyVariable(RAGEListener.CurrentFunction.Variables, right.Data as string) as Variable;
					//if (variable != null)
					//{
					//	rightCode.Add(Push.Generate(potentialVariable.Value.Value, variable.Type));

					//}
					//else
					//{
					//	rightCode.Add(Push.Generate(right.Data.ToString(), variable.Type));
					//}
				}
				else
				{
					Error($"Unable to parse right hand side of expression | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}

				if (leftCode.Count == 0)
				{
					leftCode.Add(FrameVar.Set(variable));
				}

				code.AddRange(rightCode);
				code.AddRange(leftCode);

				return new Value(DataType.Int, null, code);
			}

			Error($"Unsupported operator {op} | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
			return null;
		}

		public override Value VisitConditionalExpression(ConditionalExpressionContext context)
		{
			if (context.conditionalExpression() == null)
			{
				return VisitLogicalOrExpression(context.logicalOrExpression());
			}

			return null;
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

			code.AddRange(left.Assembly);
			code.AddRange(right.Assembly);

			code.Add(Bitwise.Generate(BitwiseType.Or));
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

			code.AddRange(left.Assembly);
			code.AddRange(right.Assembly);

			code.Add(Bitwise.Generate(BitwiseType.And));
			code.Add(Jump.Generate(JumpType.False, CurrentContext.Label));
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

				if (left.Type == DataType.Array)
				{
					left.Assembly.Add(Opcodes.Array.Get());
				}

				if (right.Type == DataType.Array)
				{
					right.Assembly.Add(Opcodes.Array.Get());
				}

				//Use global opcodes
				if (left.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(left.Data as Globals.Global, true);
					left.Assembly.AddRange(output);
				}

				if (right.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(right.Data as Globals.Global, true);
					right.Assembly.AddRange(output);
				}

				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
				code.Add(Jump.Generate(JumpType.NotEqual, CurrentContext.Label));
				return new Value(DataType.Bool, null, code);

				case "!=":

				if (left.Type == DataType.Array)
				{
					left.Assembly.Add(Opcodes.Array.Get());
				}
				if (right.Type == DataType.Array)
				{
					right.Assembly.Add(Opcodes.Array.Get());
				}

				//Use global opcodes
				if (left.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(left.Data as Globals.Global, true);
					left.Assembly.AddRange(output);
				}

				if (right.Type == DataType.Global)
				{
					var output = Globals.Global.Parse(right.Data as Globals.Global, true);
					right.Assembly.AddRange(output);
				}
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
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

			string op = context.GetChild(1).ToString();

			switch (op)
			{
				case "<":
				//If it's not an iterator context, then it's free to return the two values (if possible)
				//if (!isIterator)
				//{
				//	if (left.Data != null && right.Data != null)
				//	{
				//		return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());
				//	}
				//}
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);

				if (CurrentContext.Type == ScopeTypes.For)
				{
					code.Add(Jump.Generate(JumpType.LessThan, CurrentContext.Label));
				}
				else
				{
					code.Add(Jump.Generate(JumpType.GreaterThanEqual, CurrentContext.Label));
				}
				return new Value(DataType.Bool, null, code);

				case "<=":
				//if (!isIterator)
				//{
				//	if (left.Data != null && right.Data != null)
				//	{
				//		return new Value(DataType.Bool, (int)left.Data < (int)right.Data, new List<string>());
				//	}
				//}

				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);

				if (CurrentContext.Type == ScopeTypes.For)
				{
					code.Add(Jump.Generate(JumpType.LessThanEqual, CurrentContext.Label));
				}
				else
				{
					code.Add(Jump.Generate(JumpType.GreaterThan, CurrentContext.Label));
				}
				return new Value(DataType.Bool, null, code);

				case ">":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);

				if (CurrentContext.Type == ScopeTypes.For)
				{
					code.Add(Jump.Generate(JumpType.GreaterThan, CurrentContext.Label));
				}
				else
				{
					code.Add(Jump.Generate(JumpType.LessThanEqual, CurrentContext.Label));
				}
				return new Value(DataType.Bool, null, code);

				case ">=":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);

				if (CurrentContext.Type == ScopeTypes.For)
				{
					code.Add(Jump.Generate(JumpType.GreaterThanEqual, CurrentContext.Label));
				}
				else
				{
					code.Add(Jump.Generate(JumpType.LessThan, CurrentContext.Label));
				}
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
				//Addition
				case "+":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);

				code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Addition));
				return new Value(DataType.Int, null, code);

				//Subtraction
				case "-":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
				code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
				return new Value(DataType.Int, null, code);

				//String concatentation (not supported yet)
				case ".":
				throw new NotImplementedException();
				//Make sure both sides are strings
				//if (left.Type != DataType.Variable && right.Type != DataType.String)
				//{
				//	Error($"String concatenation can only be performed on two strings | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				//}
				//code.AddRange(right.Assembly);
				//code.Add(FrameVar.GetPointer(RAGEListener.CurrentFunction.GetVariable(left.Data.ToString())));
				//code.Add(Opcodes.String.Strcat());
				//return new Value(DataType.Int, null, code);

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
				//Multiplication
				case "*":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
				code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Multiplication));
				return new Value(DataType.Int, null, code);

				//Division
				case "/":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
				code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Division));
				return new Value(DataType.Int, null, code);

				//Modulus
				case "%":
				code.AddRange(left.Assembly);
				code.AddRange(right.Assembly);
				code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Modulus));
				return new Value(DataType.Int, null, code);

			}
			Error($"Unsupported operator '{op}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
			return null;
		}

		public override Value VisitCastExpression(CastExpressionContext context)
		{
			if (context.castExpression() == null)
			{
				return VisitUnaryExpression(context.unaryExpression());
			}
			var code = new List<string>();
			var castType = Utilities.GetTypeFromDeclaration(context.typeName().GetText());
			if (castType != DataType.Int && castType != DataType.Float)
			{
				Error($"Casting only supports int to float and vice-versa | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
			}
			var expr = VisitCastExpression(context.castExpression());

			if (expr.Type == DataType.Static || expr.Type == DataType.Argument || expr.Type == DataType.Variable)
			{
				switch (expr.Type)
				{
					case DataType.Static:
					var @static = Script.StaticVariables.GetVariable(expr.Data as string);
					if (@static == null)
					{
						Error($"Cast expression assumed static, but got null | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
					expr.Type = @static.Type;
					break;

					case DataType.Variable:
					var var = RAGEListener.CurrentFunction.Variables.GetVariable(expr.Data as string);
					if (var == null)
					{
						Error($"Cast expression assumed frame variable, but got null | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
					expr.Type = var.Type;
					break;

					case DataType.Argument:
					var arg = RAGEListener.CurrentFunction.GetParameter(expr.Data as string);
					if (arg == null)
					{
						Error($"Cast expression assumed function param, but got null | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
					expr.Type = arg.Type;
					break;
				}
			}

			if (expr.Type != DataType.Int && expr.Type != DataType.Float && expr.Type != DataType.Variable && expr.Type != DataType.Static && expr.Type != DataType.Argument)
			{
				Error($"Casting only supports int to float and vice-versa | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
			}
			code.AddRange(expr.Assembly);
			if (castType == DataType.Float && expr.Type == DataType.Int)
			{
				code.Add(Conversion.IntToFloat());
			}
			else if (castType == DataType.Int && expr.Type == DataType.Float)
			{
				code.Add(Conversion.FloatToInt());
			}
			else
			{
				Error("Conversion error!!1");
			}
			return new Value(DataType.Cast, null, code);

		}

		public override Value VisitUnaryExpression(UnaryExpressionContext context)
		{
			if (context.unaryExpression() == null)
			{
				if (context.unaryOperator() != null)
				{
					Value op = VisitUnaryOperator(context.unaryOperator());

					var expr = VisitCastExpression(context.castExpression());

					string var = context.GetChild(1).GetText();

					if (!RAGEListener.CurrentFunction.Variables.ContainsVariable(var) && op.Type == DataType.Address)
					{
						Error($"Unary expression {context.unaryOperator().GetText()} on {var} is not possible | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
						return null;
					}

					var v = RAGEListener.CurrentFunction.Variables.GetVariable(var);
					List<string> code = new List<string>();
					switch (op.Type)
					{
						//&someVar (address-of)
						case DataType.Address:
						code.Add(FrameVar.GetPointer(v));
						return new Value(DataType.Address, null, code);

						//!someExpression (not)
						case DataType.Not:
						code.AddRange(expr.Assembly);
						code.Add(Bitwise.Generate(BitwiseType.Not));
						return new Value(DataType.Not, null, code);

						//$"somestring" (hash)
						case DataType.Hash:
						code.AddRange(expr.Assembly);
						code.Add("GetHash");
						return new Value(DataType.Hash, null, code);
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
			string op = context.GetText();

			switch (op)
			{
				//Address of
				case "&":
				return new Value(DataType.Address, null, null);

				//Not
				case "!":
				return new Value(DataType.Not, null, null);

				//Joaat hash of string
				case "$":
				return new Value(DataType.Hash, null, null);

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
			IVariable variable = null;

			variable = Utilities.GetAnyVariable(RAGEListener.CurrentFunction.Variables, expression);

			switch (symbol)
			{
				//Inline addition
				case "++":
				if (variable == null)
				{
					Error($"Postfix operator '{symbol}' can only be used on variables | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					return null;
				}

				if (variable.Specifier == Specifier.Static)
				{
					code.Add(StaticVar.Get(variable));
					code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, 1));
					code.Add(StaticVar.Set(variable));
				}
				else
				{
					code.Add(FrameVar.Get(variable));
					code.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, 1));
					code.Add(FrameVar.Set(variable));
				}

				return new Value(DataType.Int, null, code);

				//Inline subtraction
				case "--":
				if (variable == null)
				{
					Error($"Postfix operator '{symbol}' can only be used on variables | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					return null;
				}

				if (variable.Specifier == Specifier.Static)
				{
					code.Add(StaticVar.Get(variable));
					code.Add(Push.Int(1));
					code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
					code.Add(StaticVar.Set(variable));

				}
				else
				{
					code.Add(FrameVar.Get(variable));
					code.Add(Push.Int(1));
					code.Add(Arithmetic.Generate(Arithmetic.ArithmeticType.Subtraction));
					code.Add(FrameVar.Set(variable));
				}

				return new Value(DataType.Int, null, code);

				//Function call
				case "(":
				if (Script.Functions.ContainsFunction(expression))
				{
					var args = VisitArgumentExpressionList(context.argumentExpressionList());
					var func = Script.Functions.GetFunction(expression);
					//No args
					if (args == null)
					{
						if (func.Parameters.Count > 0)
						{
							Error($"Function '{expression}' requires {func.Parameters.Count} arguments, none given | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
						}
						code.Add(Call.Local(expression));
						return new Value(DataType.LocalCall, null, code);
					}
					else
					{
						var argData = args.Data as List<Value>;
						argData.Reverse();
						if (argData.Count != func.Parameters.Count)
						{
							Error($"Function '{expression}' requires {func.Parameters.Count} arguments, {argData.Count} given | line {RAGEListener.lineNumber},{RAGEListener.linePosition}");
						}
						foreach (var v in argData)
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
						code.Add(Call.Local(expression));
						return new Value(DataType.LocalCall, null, code);

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
				else if (Core.BuiltInFunctions.ContainsFunction(expression))
				{
					var args = VisitArgumentExpressionList(context.argumentExpressionList());
					var func = Core.BuiltInFunctions.GetFunction(expression);
					var argsList = args.Data as List<Value>;
					if (argsList.Count != func.Parameters.Count)
					{
						Error($"Built-in function '{expression}' requires '{func.Parameters.Count}' arguments, but only '{argsList.Count}' given | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
				}
				Error($"Found open parens, but expression is not a function | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				return null;

				//Array
				case "[":
				string arrayName = context.GetText().Split('[')[0];
				if (arrayName.StartsWith("Global_"))
				{
					var global = Globals.Global.Parse(context.GetText());
					return new Value(DataType.Global, global, code);
				}
				else
				{
					//Find array
					var array = Utilities.GetAnyVariable<Array>(RAGEListener.CurrentFunction.Variables, arrayName) as Array;
					if (array == null)
					{
						Error($"No array '{arrayName}' exists  | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
						return null;
					}
					string index = context.expression().GetText();
					//Make sure this is a variable or an int
					var indexType = Utilities.GetType(RAGEListener.CurrentFunction, index);
					if (indexType != DataType.Int && indexType != DataType.Variable && indexType != DataType.Static)
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
						if (array.Specifier == Specifier.Static)
						{
							code.Add(StaticVar.Pointer(array));
						}
						else
						{
							code.Add(FrameVar.GetPointer(array));
						}
					}
					else if (indexType == DataType.Variable || indexType == DataType.Static)
					{
						var vVar = Utilities.GetAnyVariable(RAGEListener.CurrentFunction.Variables, index);
						if (vVar == null)
						{
							Error($"Assumed variable '{index}' used for indexer, but got null | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
						}
						var expr = VisitExpression(context.expression());
						//Since its a var, just generate the code and hope the dev knows what theyre doing
						code.AddRange(expr.Assembly);
						if (array.Specifier == Specifier.Static)
						{
							code.Add(StaticVar.Pointer(array));
						}
						else
						{
							code.Add(FrameVar.GetPointer(array));
						}
					}
				}
				return new Value(DataType.Array, arrayName, code);

				//Enums (and maybe something else in the future)
				case ".":
				string enumName = context.postfixExpression().GetText();
				if (enumName.StartsWith("Global_"))
				{
					var global = Globals.Global.Parse(context.GetText());
					return new Value(DataType.Global, global, code);
				}
				else
				{
					if (!Script.Enums.ContainsEnum(enumName))
					{
						Error($"Enum '{enumName}' was used but never declared | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
					var thisEnum = Script.Enums.GetEnum(enumName);
					string enumeratorName = context.Identifier().GetText();
					if (!thisEnum.Enumerators.ContainsEnumerator(enumeratorName))
					{
						Error($"Enumerator '{enumeratorName}' does not exist in enum '{enumName}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
					}
					var thisEnumerator = thisEnum.Enumerators.GetEnumerator(enumeratorName);
					code.Add(StaticVar.Get(thisEnumerator.Variable as Variable));
					return new Value(DataType.Enum, thisEnumerator, code);
				}

				//Struct members
				case "->":
				var structName = context.postfixExpression().GetText();
				var @struct = Utilities.GetAnyVariable(RAGEListener.CurrentFunction.Variables, structName) as Variable;
				if (@struct == null)
				{
					Error($"Struct '{structName}' was used but never declared | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
				if (@struct.Type != DataType.CustomType)
				{
					Error($"Struct '{structName}' was never declared as a custom type (is '{@struct.Type}') | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}
				var structDecl = Script.Structs.GetStruct(@struct.CustomType);
				var immediate = context.Identifier().GetText();
				var immediateMember = structDecl.Members.GetVariable(immediate);

				if (immediateMember == null)
				{
					Error($"Member variable '{immediate}' does not exist in struct '{@struct.CustomType}' | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				}

				if (immediateMember.FrameId == 0)
				{
					if (@struct.Specifier == Specifier.Static)
					{
						code.Add(StaticVar.Get(@struct));
					}
					else
					{
						code.Add(FrameVar.Get(@struct));
					}
				}
				else
				{
					if (@struct.Specifier == Specifier.Static)
					{
						code.Add(StaticVar.Pointer(@struct));
					}
					else
					{
						code.Add(FrameVar.GetPointer(@struct));
					}
				}

				return new Value(DataType.Struct, immediateMember, code);

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
				var assignment = VisitAssignmentExpression(context.assignmentExpression());
				if (assignment.Type == DataType.Array)
				{
					assignment.Assembly.Add(Opcodes.Array.Get());
				}
				args.Add(assignment);
				context = context.argumentExpressionList();
			}

			return new Value(DataType.ArgListing, args, null);
		}

		private Value ParseType(PrimaryExpressionContext context)
		{
			string value = context.GetText();

			DataType type = Utilities.GetType(RAGEListener.CurrentFunction, value);
			List<string> code = new List<string>();

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
					ival = int.Parse(value);
				}
				code.Add(Push.Generate(value, type));
				return new Value(DataType.Int, ival, code);

				case DataType.Bool:
				code.Add(Push.Generate(value, type));
				return new Value(DataType.Bool, Convert.ToBoolean(value), code);

				case DataType.Float:
				code.Add(Push.Generate(value, type));
				return new Value(DataType.Float, Convert.ToSingle(value), code);

				case DataType.String:
				code.Add(Push.Generate(value, type));
				return new Value(DataType.String, value, code);

				case DataType.Variable:
				var var = RAGEListener.CurrentFunction.Variables.GetVariable(value);
				code.Add(FrameVar.Get(var));
				return new Value(DataType.Variable, value, code);

				case DataType.NativeCall:
				return new Value(DataType.NativeCall, value, new List<string>());

				case DataType.LocalCall:
				return new Value(DataType.LocalCall, value, new List<string>());

				case DataType.Global:
				var global = Globals.Global.Parse(value);
				return new Value(DataType.Global, global, new List<string>());

				case DataType.Static:
				var = Script.StaticVariables.GetVariable(value);
				code.Add(StaticVar.Get(var));
				return new Value(DataType.Static, value, code);

				case DataType.Argument:
				var = RAGEListener.CurrentFunction.GetParameter(value);
				code.Add(FrameVar.Get(var));
				return new Value(DataType.Argument, value, code);

				case DataType.ForeachVariable:
				var tVar = RAGEListener.CurrentFunction.Variables.GetAnyVariable(value) as Variable;
				var foreachVar = tVar.ForeachReference;
				if (tVar.Specifier == Specifier.Static)
				{
					code.Add(StaticVar.Get(tVar));
				}
				else
				{
					code.Add(FrameVar.Get(tVar));

				}
				if (foreachVar.Specifier == Specifier.Static)
				{
					code.Add(StaticVar.Pointer(foreachVar));
				}
				else
				{
					code.Add(FrameVar.GetPointer(foreachVar));
				}
				code.Add(Opcodes.Array.Get());
				return new Value(DataType.ForeachVariable, value, code);

				default:
				Error($"Type {type} is unsupported | line {RAGEListener.lineNumber}, {RAGEListener.linePosition}");
				return null;
			}
		}
	}
}
