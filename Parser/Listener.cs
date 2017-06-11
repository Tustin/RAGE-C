﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using System.Linq;
using Antlr4.Runtime;
using RAGE.Parser.Opcodes;

using static RAGEParser;
using static RAGE.Main.Logger;
using System.Text;
using System.IO;
using Antlr4.Runtime.Tree;

namespace RAGE.Parser
{
	public class RAGEListener : RAGEBaseListener
	{
		//Stuff that gets populated as the walker goes through the tree
		public static Function CurrentFunction;
		public static Variable CurrentVariable;

		public static Switch CurrentSwitch;

		public static LabelData CurrentLabel;

		public static int lineNumber = 0;
		public static int linePosition = 0;

		public static List<StoredContext> storedContexts;

		public static ParserRuleContext Scope;

		public static Dictionary<StoredContext, Switch> switches;

		RAGEVisitor visitor;

		List<string> conditionalLabels = new List<string>();

		public RAGEListener()
		{
			visitor = new RAGEVisitor();
			storedContexts = new List<StoredContext>();
			switches = new Dictionary<StoredContext, Switch>();
		}

		public override void EnterBlockItem([NotNull] BlockItemContext context)
		{
			var ff = context.GetText();
			base.EnterBlockItem(context);
		}

		public override void EnterIncludeExpression([NotNull] IncludeExpressionContext context)
		{
			string ff = context.GetText();
			var fileName = context.StringLiteral().ToString().Replace("\"", "");
			if (!fileName.EndsWith(".c")) fileName += ".c";
			var filePath = Core.FileDirectory + "\\" + fileName;
			if (!File.Exists(filePath))
			{
				Error($"Unable to find include file '{fileName}' | line {lineNumber}, {linePosition}");
			}
			AntlrFileStream fs = new AntlrFileStream(filePath);

			RAGELexer lexer = new RAGELexer(fs);

			CommonTokenStream tokens = new CommonTokenStream(lexer);

			RAGEParser parser = new RAGEParser(tokens);

			ParseTreeWalker walker = new ParseTreeWalker();

			RAGEListener listener = new RAGEListener();
			parser.RemoveErrorListeners();
			ParseTreeWalker.Default.Walk(listener, parser.compilationUnit());
		}

		//Set line number and position for error logging
		public override void EnterEveryRule([NotNull] ParserRuleContext context)
		{
			var token = context.Start;
			lineNumber = token.Line;
			linePosition = token.Column;
			Scope = context;
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
				Error($"Enum '{enumName}' contains no enumerators | line {lineNumber}, {linePosition}");
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

		//Struct
		public override void EnterStructDeclarator([NotNull] StructDeclaratorContext context)
		{
			string structName = context.Identifier().GetText();

			if (Script.Structs.ContainsStruct(structName))
			{
				Error($"Script already contains a struct called '{structName}' | line {lineNumber}, {linePosition}");
			}

			//Do this hack so the struct is parsed from top down
			var structMembers = new List<StructItemDeclaratorContext>();
			var structItemsList = context.structDeclaratorList();

			if (structItemsList == null)
			{
				Error($"Struct '{structName}' contains no members | line {lineNumber}, {linePosition}");
			}

			while (structItemsList != null)
			{
				structMembers.Insert(0, structItemsList.structItemDeclarator());
				structItemsList = structItemsList.structDeclaratorList();
			}

			var currentStruct = new Struct(structName);

			Script.Structs.Add(currentStruct);

			foreach (var structMember in structMembers)
			{
				visitor.VisitStructItemDeclarator(structMember);
			}

			base.EnterStructDeclarator(context);
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
						//Fill out each item in the array also
						if (variable is Array arr)
						{
							entryContents.Add(Push.Int(arr.Indices.Count.ToString()));
							entryContents.Add(StaticVar.Set(arr));
							foreach (var var in arr.Indices)
							{
								entryContents.AddRange(var.ValueAssembly);
								entryContents.Add(Push.Int(var.FrameId.ToString()));
								entryContents.Add(StaticVar.Pointer(arr));
								entryContents.Add(Opcodes.Array.Set());
							}
						}
						else if (variable is Variable var)
						{
							if (var.Type == DataType.CustomType)
							{
								var @struct = Script.Structs.GetStruct(var.CustomType);
								foreach (Variable member in @struct.Members)
								{
									entryContents.AddRange(member.ValueAssembly);
									if (member.FrameId == 0)
									{
										entryContents.Add(StaticVar.Set(variable));
									}
									else
									{
										entryContents.Add(StaticVar.Pointer(variable));
										entryContents.Add(Immediate.Set(member.FrameId));
									}
								}
							}
							else
							{
								entryContents.AddRange(var.ValueAssembly);
								entryContents.Add(StaticVar.Set(var));
							}
						}

					}
				}
				entryContents.Add("Call @main");
				entryContents.Add("Return 0 0");
				Core.AssemblyCode.Add("__script_entry__", entryContents);
			}
			var specifier = (visitor.VisitDeclarationSpecifiers(context.declarationSpecifiers())).Data as DeclarationResponse;

			string name = Regex.Replace(context.declarator().GetText(), "\\(.*\\)", "");
			if (Script.Functions.ContainsFunction(name))
			{
				Error($"Script already contains function named '{name}' | line {lineNumber},{linePosition}");
			}
			else if (Core.BuiltInFunctions.ContainsFunction(name))
			{
				Error($"Function name '{name}' is a reserved built-in function | line {lineNumber},{linePosition}");
			}
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

				var specifier = visitor.VisitDeclarationSpecifiers(declContext.declarationSpecifiers()).Data as DeclarationResponse;

				CurrentFunction.Parameters.Add(new Parameter(specifier.Type, paramName, CurrentFunction.Parameters.Count));
			}
		}

		//End of a function
		public override void ExitFunctionDefinition(FunctionDefinitionContext context)
		{
			var function = Core.AssemblyCode.FindFunction(CurrentFunction.Name);
			string funcEntry = function.Value[0];
			var funcCode = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
			//@TODO: Update first 0 for param count
			funcEntry = funcEntry.Replace("Function 0 2 0", $"Function {CurrentFunction.Parameters.Count} {CurrentFunction.FrameVars + CurrentFunction.Parameters.Count} 0");
			function.Value[0] = funcEntry;
			//@Hack: Fix me!
			if (!funcCode.Last().StartsWith("Return"))
			{
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(Opcodes.Return.Generate(CurrentFunction));
			}
			LogVerbose($"Leaving function '{CurrentFunction.Name}'");
			CurrentFunction = null;
		}

		//New variables
		public override void EnterDeclaration(DeclarationContext context)
		{
			var variable = visitor.VisitDeclaration(context);

			Variable var = variable.Data as Variable;

			if (var == null)
			{
				return;
			}

			if (var.Specifier == Specifier.Static)
			{
				var.FrameId = Script.GetNextStaticIndex();
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

		//New array
		public override void EnterArrayDeclarator([NotNull] ArrayDeclaratorContext context)
		{
			var arrType = Utilities.GetTypeFromDeclaration(context.typeSpecifier().GetText());
			var arrName = context.Identifier().GetText();
			string arrSizeText = null;
			bool isSizeAnonymous = false;

			if (context.constantExpression() == null)
			{
				if (context.arrayDeclarationList() == null)
				{
					Error($"Array '{arrName}' is missing it's size and has no items | line {lineNumber},{linePosition}");
				}
				arrSizeText = "0";
				isSizeAnonymous = true;
			}
			else
			{
				arrSizeText = context.constantExpression().GetText();
			}

			var isStatic = context.storageClassSpecifier() != null;

			//I won't force the static keyword but i'd rather it exist for clarity
			if (isStatic || CurrentFunction == null)
			{
				if (!isStatic)
				{
					Warn($"Array '{arrName}' isn't specified as a static but is assumed to be static | line {lineNumber},{linePosition}");
				}

				if (Script.StaticVariables.ContainsVariable(arrName))
				{
					Error($"Script already contains static variable '{arrName}' | line {lineNumber},{linePosition}");
				}

				//Just in case
				isStatic = true;
			}
			else if (CurrentFunction != null)
			{
				if (CurrentFunction.Variables.ContainsVariable(arrName))
				{
					Error($"Function '{arrName}' already contains variable '{arrName}' | line {lineNumber},{linePosition}");
				}

				isStatic = false;
			}
			else
			{
				Error($"Unable to infer scope for array '{arrName}' | line {lineNumber},{linePosition}");
			}

			if (!int.TryParse(arrSizeText, out int arrSize))
			{
				Error($"Array size can only be an integer | line {lineNumber},{linePosition}");
			}

			if (arrSize < 1 && !isSizeAnonymous)
			{
				Error($"Array size can only be 1 or larger, got '{arrSize}' for array {arrName} | line {lineNumber},{linePosition}");
			}

			int varOffset = CurrentFunction == null ? Script.GetNextStaticIndex() : CurrentFunction.FrameVars;

			Array arr = new Array(arrName, varOffset, arrSize, arrType);
			arr.FrameId = Script.GetNextStaticIndex();
			if (context.arrayDeclarationList() != null)
			{
				var arrayItems = new List<ArrayDeclarationContext>();
				var arrList = context.arrayDeclarationList();
				while (arrList != null)
				{
					var item = arrList.arrayDeclaration();
					var itemType = Utilities.GetType(null, item.GetText());
					if (itemType != arrType)
					{
						Error($"Value '{item.GetText()}' in array '{arrName}' was interpreted as type '{itemType}', which doesn't match the array type of '{arrType}' | line {lineNumber},{linePosition}");
					}
					arrayItems.Add(item);
					if (isSizeAnonymous)
					{
						arrSize++;
						arr.Length++;
					}
					arrList = arrList.arrayDeclarationList();
				}

				if (arrayItems.Count != arrSize)
				{
					Error($"Size of array doesn't match the count of declarators ('{arrName}' size: {arrSize}, declarators count: {arrayItems.Count} | line {lineNumber},{linePosition}");
				}


				//Sort them in the right order
				arrayItems.Reverse();

				int i = 0;
				foreach (var index in arrayItems)
				{
					Variable var = new Variable($"{arrName}.{i}", i, arrType);
					var.Value.Value = index.GetText();
					var.Value.IsDefault = false;
					var.Value.Type = arrType;
					var.ValueAssembly = visitor.VisitConstantExpression(index.constantExpression()).Assembly;
					arr.Indices.Add(var);
					i++;
				}
			}
			else
			{
				//No items were initialized with the array, so just generate default vals
				for (int i = 0; i < arrSize; i++)
				{
					Variable var = new Variable($"{arrName}.{i}", i, arrType);
					var.Value.Value = Utilities.GetDefaultValue(arrType);
					var.Value.IsDefault = true;
					var.Value.Type = arrType;
					arr.Indices.Add(var);
				}
			}

			if (isStatic)
			{
				Script.StaticVariables.Add(arr);
			}
			else
			{
				CurrentFunction.Variables.Add(arr);
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
				sc = new StoredContext($"selection_end_{count}", count, context, ScopeTypes.Conditional);
				storedContexts.Add(sc);
				if (hasElse)
				{
					sc = new StoredContext($"selection_else_{count}", count, context.selectionElseStatement(), ScopeTypes.Conditional);
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

				StoredContext sc = new StoredContext($"switch_end_{count}", count, context, ScopeTypes.Conditional);

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
					if (CurrentFunction.Variables.ContainsVariable(conditionVariable))
					{
						cf.Add(FrameVar.Get(CurrentFunction.Variables.GetVariable(conditionVariable)));
					}
					else if (Script.StaticVariables.ContainsVariable(conditionVariable))
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
				cf.Add(Jump.Generate(JumpType.Unconditional, sc.Label));
			}
		}

		//Entering else statement
		public override void EnterSelectionElseStatement(SelectionElseStatementContext context)
		{
			var contextScope = storedContexts.Where(a => a.Context == context).LastOrDefault();
			var parentContextScope = storedContexts.Where(a => a.Context == context.Parent).LastOrDefault();
			if (parentContextScope == null)
			{
				Error($"Found else statement, but it has no parent if statement | line {lineNumber},{linePosition}");
			}
			if (contextScope == null)
			{
				Error($"Found else statement, but unable to find context | line {lineNumber},{linePosition}");
			}
			Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(Jump.Generate(JumpType.Unconditional, parentContextScope.Label));
			Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{contextScope.Label}");
			base.EnterSelectionElseStatement(context);
		}

		//Exiting if, switch
		public override void ExitSelectionStatement(SelectionStatementContext context)
		{
			var code = new List<string>();
			string selectionType = context.GetText();
			var cf = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
			var contextScope = storedContexts.Where(a => a.Context == context).FirstOrDefault();
			visitor.CurrentContext = null;

			if (selectionType.StartsWith("switch"))
			{
				//LabelData.ForEach(a => cf.AddRange(a.Code));
				CurrentSwitch = null;
			}
			if (selectionType.StartsWith("if"))
			{
				cf.Add(Jump.Generate(JumpType.Unconditional, contextScope.Label));
			}

			cf.Add($":{contextScope.Label}");
		}

		public List<LabelData> LabelData = new List<LabelData>();
		//Entering switch case
		public override void EnterLabeledStatement([NotNull] LabeledStatementContext context)
		{
			var label = context.GetChild(0).GetText();
			List<string> caseCode = new List<string>();
			if (label == "case")
			{
				if (CurrentSwitch == null)
				{
					Error($"Found case label, but no switch was found | line {lineNumber},{linePosition}");
				}
				var expr = visitor.VisitConstantExpression(context.constantExpression());
				int caseCondition = Utilities.GetCaseExpression(expr);

				Case nextCase = CurrentSwitch.Cases.Where(a => a.Generated == false && a.Condition == caseCondition).FirstOrDefault();

				if (nextCase == null)
				{
					Error($"Found case that wasn't defined | line {lineNumber},{linePosition}");
				}

				caseCode.Add($":{nextCase.Label}");
				var statement = context.statement().expressionStatement();
				if (statement != null)
				{
					var statementRes = visitor.VisitExpression(statement.expression());

					caseCode.AddRange(statementRes.Assembly);
				}

				//@TODO: Put this in a visitor
				var jump = context.jumpStatement();

				if (jump != null)
				{
					var jumpLoc = storedContexts.LastOrDefault();
					string jumpType = jump.GetText().Replace(";", "");
					switch (jumpType)
					{
						case "break":
						caseCode.Add(Jump.Generate(JumpType.Unconditional, jumpLoc.Label));
						break;
					}
				}
				else
				{
					Warn($"Switch case '{nextCase.Condition}' does not contain a jump statement | line {lineNumber},{linePosition}");
				}


				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{nextCase.Label}");
				//nextCase.Generated = true;
				var ld = new LabelData(caseCode, expr, nextCase);
				//Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.AddRange(caseCode);
				LabelData.Add(ld);
				CurrentLabel = ld;

			}
			//else if (label == "default")
			//{
			//    if (CurrentSwitch == null)
			//    {
			//        Error($"Found case label, but no switch was found | line {lineNumber},{linePosition}");
			//    }

			//    Case nextCase = CurrentSwitch.Cases.Where(a => a.Generated == false && a.IsDefault).FirstOrDefault();

			//    if (nextCase == null)
			//    {
			//        Error($"Found a default label but couldn't find it's case | line {lineNumber},{linePosition}");
			//    }

			//    caseCode.Add($":{nextCase.Label}");

			//    var statement = context.statement().expressionStatement();
			//    if (statement != null)
			//    {
			//        var statementRes = visitor.VisitExpression(statement.expression());
			//        caseCode.AddRange(statementRes.Assembly);
			//    }

			//    //@TODO: Put this in a visitor
			//    var jump = context.jumpStatement();
			//    var jumpLoc = storedContexts.LastOrDefault();

			//    if (jump != null)
			//    {
			//        string jumpType = jump.GetText().Replace(";", "");
			//        switch (jumpType)
			//        {
			//            case "break":
			//            caseCode.Add(Jump.Generate(JumpType.Unconditional, jumpLoc.Label));
			//            break;
			//        }
			//    } //For some reason the jump statement will be null if theres no statement in the case... Do this hack for now
			//    else if (statement == null)
			//    {
			//        caseCode.Add(Jump.Generate(JumpType.Unconditional, jumpLoc.Label));
			//    }
			//    else
			//    {
			//        Warn($"Switch case '{nextCase.Condition}' does not contain a jump statement | line {lineNumber},{linePosition}");
			//    }

			//    var ld = new LabelData(caseCode, null, nextCase);

			//    //Insert the default case into the first slot (because unmatched switches will just execute the next opcode)
			//    LabelData.Insert(0, ld);
			//    CurrentLabel = ld;
			//    //Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{nextCase.Label}");
			//    //nextCase.Generated = true;
			//}
		}

		//Exiting switch case
		public override void ExitLabeledStatement([NotNull] LabeledStatementContext context)
		{
			CurrentLabel = null;
			base.ExitLabeledStatement(context);
		}

		//Break, continue, return
		public override void EnterJumpStatement([NotNull] JumpStatementContext context)
		{
			StoredContext jumpLoc = null;
			if (CurrentSwitch != null)
			{
				jumpLoc = switches.Where(a => a.Value == CurrentSwitch).FirstOrDefault().Key;
			}
			else
			{
				jumpLoc = storedContexts.LastOrDefault();

			}
			string jumpType = context.GetChild(0).GetText();
			var functionCode = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
			switch (jumpType)
			{
				case "break":
				//Get the last stored context to jump to it
				if (jumpLoc == null)
				{
					Error($"Tried to use a jump statement without a context to jump out of | line {lineNumber},{linePosition}");
				}

				functionCode.Add(Jump.Generate(JumpType.Unconditional, jumpLoc.Label));

				break;
				case "continue":
				//@TODO
				break;
				case "return":
				if (CurrentFunction == null)
				{
					Error($"Cannot return outside of function scope | line {lineNumber},{linePosition}");
				}
				var expr = context.expression();
				if (context.expression() == null)
				{
					if (CurrentFunction.Type != DataType.Void)
					{
						Error($"'{CurrentFunction.Name}' expects a return type of '{CurrentFunction.Type}' but no return expression was given | line {lineNumber},{linePosition}");
					}
					functionCode.Add(Opcodes.Return.Generate(CurrentFunction));
					return;
				}
				var exprRes = visitor.VisitExpression(expr);
				functionCode.AddRange(exprRes.Assembly);
				functionCode.Add(Opcodes.Return.Generate(CurrentFunction));
				break;
			}
			base.EnterJumpStatement(context);
		}

		//Entering for, while, foreach
		public override void EnterIterationStatement(IterationStatementContext context)
		{
			string loop = context.GetText();

			//For loops
			if (loop.StartsWith("for") && !loop.StartsWith("foreach"))
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
				int labelCount = storedContexts.Count(a => a.Type == ScopeTypes.For);
				var label = $"for_{labelCount}";
				var endLabel = $"for_{labelCount}_end";
				StoredContext sc = new StoredContext(label, endLabel, count, context, ScopeTypes.For);

				storedContexts.Add(sc);
				visitor.CurrentContext = sc;

				//Push the value of the iterator into the variable before the for loop label
				//Otherwise we would constantly have infinite loops
				var func = Core.AssemblyCode.FindFunction(CurrentFunction.Name);
				func.Value.Add(Push.Generate(v.Value.Value, v.Value.Type));
				func.Value.Add(FrameVar.Set(v));
				//Add label
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(default(string));
				func.Value.Add($":{label}");
			}
			//While loops
			else if (loop.StartsWith("while"))
			{
				int count = storedContexts.Count(a => a.Context is IterationStatementContext);
				int labelCount = storedContexts.Count(a => a.Type == ScopeTypes.While);
				var label = $"while_{labelCount}";
				var endLabel = $"while_{labelCount}_end";
				StoredContext sc = new StoredContext(label, endLabel, count, context, ScopeTypes.While);
				storedContexts.Add(sc);
				visitor.CurrentContext = sc;
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(default(string));
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{label}");

				//Evaluate this here
				foreach (ExpressionContext expression in context.expression().Reverse())
				{
					var test = visitor.VisitExpression(expression);
					Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.AddRange(test.Assembly);
				}
			}
			//Foreach loops
			else if (loop.StartsWith("foreach"))
			{
				var newVarName = context.Identifier()[0].GetText();
				var existingVarName = context.Identifier()[1].GetText();

				var existingVar = CurrentFunction.Variables.GetAnyVariable<Array>(existingVarName) as Array;

				if (existingVar == null)
				{
					Error($"Foreach variable must be an array or it doesn't exist | line {lineNumber},{linePosition}");

				}

				//Could do this check with assembly but this can be considered optimization
				if (existingVar.Indices.Count == 0)
				{
					Error($"Foreach array '{existingVarName}' is empty | line {lineNumber},{linePosition}");
				}

				//Make a new variable using the type of the array
				Variable newVar = new Variable(newVarName, CurrentFunction.FrameVars + 1, existingVar.Type);
				newVar.Value.Type = existingVar.Type;
				newVar.IsIterator = true;
				newVar.Value.Value = "0"; //Default it to 0 since it's the indexer for the array
				newVar.Value.IsDefault = true;
				//Use the array as a reference to get the value
				newVar.ForeachReference = existingVar;

				CurrentFunction.Variables.Add(newVar);

				var currentFunc = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;
				currentFunc.Add(Push.Int(newVar.Value.Value)); //0
				currentFunc.Add(FrameVar.Set(newVar));

				int count = storedContexts.Count(a => a.Context is IterationStatementContext);
				int labelCount = storedContexts.Count(a => a.Type == ScopeTypes.Foreach);
				var label = $"foreach_{labelCount}";
				var endLabel = $"foreach_{labelCount}_end";
				StoredContext sc = new StoredContext(label, endLabel, count, context, ScopeTypes.Foreach);
				storedContexts.Add(sc);
				visitor.CurrentContext = sc;
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(default(string));
				Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{label}");
			}
		}

		//Exiting for, while, foreach
		public override void ExitIterationStatement(IterationStatementContext context)
		{
			var storedContext = storedContexts.Where(a => a.Context == context).First();
			visitor.CurrentContext = storedContext;
			string code = context.GetText();
			var func = Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value;

			//Foreach needs custom code
			if (storedContext.Type == ScopeTypes.Foreach)
			{
				var newVarName = context.Identifier()[0].GetText();
				var existingVarName = context.Identifier()[1].GetText();

				var newVar = CurrentFunction.Variables.GetAnyVariable<Variable>(newVarName) as Variable;
				var existingVar = CurrentFunction.Variables.GetAnyVariable<Array>(existingVarName) as Array;

				//Check if the index is still in bounds, if so, update index and keep looping
				func.Add(FrameVar.Get(newVar));
				func.Add(Arithmetic.GenerateInline(Arithmetic.ArithmeticType.Addition, 1));
				func.Add(FrameVar.Set(newVar));
				func.Add(FrameVar.Get(newVar));
				func.Add(Push.Int(existingVar.Indices.Count) + $" //size of {existingVarName}");
				func.Add(Jump.Generate(JumpType.LessThan, storedContext.Label));
			}
			else if (storedContext.Type != ScopeTypes.While)
			{
				//Reverse it so it evaluates the incrementing first before doing the comparison
				foreach (ExpressionContext expression in context.expression().Reverse())
				{
					var test = visitor.VisitExpression(expression);
					Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.AddRange(test.Assembly);
				}
			}

			if (storedContext.Type == ScopeTypes.While)
			{
				func.Add(Jump.Generate(JumpType.Unconditional, storedContext.Label));
			}

			Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add(default(string));
			Core.AssemblyCode.FindFunction(CurrentFunction.Name).Value.Add($":{storedContext.EndLabel}");

			base.ExitIterationStatement(context);
		}
	}
}
