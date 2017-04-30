//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ANTLR Version: 4.6
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generated from RAGE.g4 by ANTLR 4.6

// Unreachable code detected
#pragma warning disable 0162
// The variable '...' is assigned but its value is never used
#pragma warning disable 0219
// Missing XML comment for publicly visible type or member '...'
#pragma warning disable 1591
// Ambiguous reference in cref attribute
#pragma warning disable 419

using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using IToken = Antlr4.Runtime.IToken;

/// <summary>
/// This interface defines a complete generic visitor for a parse tree produced
/// by <see cref="RAGEParser"/>.
/// </summary>
/// <typeparam name="Result">The return type of the visit operation.</typeparam>
[System.CodeDom.Compiler.GeneratedCode("ANTLR", "4.6")]
[System.CLSCompliant(false)]
public interface IRAGEVisitor<Result> : IParseTreeVisitor<Result> {
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.primaryExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPrimaryExpression([NotNull] RAGEParser.PrimaryExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.postfixExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPostfixExpression([NotNull] RAGEParser.PostfixExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.argumentExpressionList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArgumentExpressionList([NotNull] RAGEParser.ArgumentExpressionListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.unaryExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryExpression([NotNull] RAGEParser.UnaryExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.unaryOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitUnaryOperator([NotNull] RAGEParser.UnaryOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.castExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCastExpression([NotNull] RAGEParser.CastExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.multiplicativeExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitMultiplicativeExpression([NotNull] RAGEParser.MultiplicativeExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.additiveExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAdditiveExpression([NotNull] RAGEParser.AdditiveExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.shiftExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitShiftExpression([NotNull] RAGEParser.ShiftExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.relationalExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitRelationalExpression([NotNull] RAGEParser.RelationalExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.equalityExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEqualityExpression([NotNull] RAGEParser.EqualityExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.andExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAndExpression([NotNull] RAGEParser.AndExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.exclusiveOrExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExclusiveOrExpression([NotNull] RAGEParser.ExclusiveOrExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.inclusiveOrExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInclusiveOrExpression([NotNull] RAGEParser.InclusiveOrExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.logicalAndExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalAndExpression([NotNull] RAGEParser.LogicalAndExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.logicalOrExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLogicalOrExpression([NotNull] RAGEParser.LogicalOrExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.conditionalExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConditionalExpression([NotNull] RAGEParser.ConditionalExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.assignmentExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentExpression([NotNull] RAGEParser.AssignmentExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.assignmentOperator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAssignmentOperator([NotNull] RAGEParser.AssignmentOperatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.expression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpression([NotNull] RAGEParser.ExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.constantExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitConstantExpression([NotNull] RAGEParser.ConstantExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.globalExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitGlobalExpression([NotNull] RAGEParser.GlobalExpressionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclaration([NotNull] RAGEParser.DeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declarationSpecifiers"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationSpecifiers([NotNull] RAGEParser.DeclarationSpecifiersContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declarationSpecifiers2"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationSpecifiers2([NotNull] RAGEParser.DeclarationSpecifiers2Context context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declarationSpecifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationSpecifier([NotNull] RAGEParser.DeclarationSpecifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.initDeclaratorList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInitDeclaratorList([NotNull] RAGEParser.InitDeclaratorListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.initDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInitDeclarator([NotNull] RAGEParser.InitDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.storageClassSpecifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStorageClassSpecifier([NotNull] RAGEParser.StorageClassSpecifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.typeSpecifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeSpecifier([NotNull] RAGEParser.TypeSpecifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structOrUnionSpecifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructOrUnionSpecifier([NotNull] RAGEParser.StructOrUnionSpecifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structOrUnion"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructOrUnion([NotNull] RAGEParser.StructOrUnionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.arrayDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayDeclarator([NotNull] RAGEParser.ArrayDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.arrayDeclarationList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayDeclarationList([NotNull] RAGEParser.ArrayDeclarationListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.arrayDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitArrayDeclaration([NotNull] RAGEParser.ArrayDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structDeclarationList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructDeclarationList([NotNull] RAGEParser.StructDeclarationListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructDeclaration([NotNull] RAGEParser.StructDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.specifierQualifierList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSpecifierQualifierList([NotNull] RAGEParser.SpecifierQualifierListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structDeclaratorList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructDeclaratorList([NotNull] RAGEParser.StructDeclaratorListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.structDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStructDeclarator([NotNull] RAGEParser.StructDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.enumDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnumDeclarator([NotNull] RAGEParser.EnumDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.enumSpecifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnumSpecifier([NotNull] RAGEParser.EnumSpecifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.enumeratorList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnumeratorList([NotNull] RAGEParser.EnumeratorListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.enumerator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnumerator([NotNull] RAGEParser.EnumeratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.enumerationConstant"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitEnumerationConstant([NotNull] RAGEParser.EnumerationConstantContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.typeQualifier"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeQualifier([NotNull] RAGEParser.TypeQualifierContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarator([NotNull] RAGEParser.DeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.directDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDirectDeclarator([NotNull] RAGEParser.DirectDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.nestedParenthesesBlock"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitNestedParenthesesBlock([NotNull] RAGEParser.NestedParenthesesBlockContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.pointer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitPointer([NotNull] RAGEParser.PointerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.typeQualifierList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeQualifierList([NotNull] RAGEParser.TypeQualifierListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.parameterTypeList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterTypeList([NotNull] RAGEParser.ParameterTypeListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.parameterList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterList([NotNull] RAGEParser.ParameterListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.parameterDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitParameterDeclaration([NotNull] RAGEParser.ParameterDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.identifierList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIdentifierList([NotNull] RAGEParser.IdentifierListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.typeName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypeName([NotNull] RAGEParser.TypeNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.abstractDeclarator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitAbstractDeclarator([NotNull] RAGEParser.AbstractDeclaratorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.typedefName"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTypedefName([NotNull] RAGEParser.TypedefNameContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.initializer"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInitializer([NotNull] RAGEParser.InitializerContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.initializerList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitInitializerList([NotNull] RAGEParser.InitializerListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.designation"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDesignation([NotNull] RAGEParser.DesignationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.designatorList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDesignatorList([NotNull] RAGEParser.DesignatorListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.designator"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDesignator([NotNull] RAGEParser.DesignatorContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.selectionStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelectionStatement([NotNull] RAGEParser.SelectionStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.selectionElseStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitSelectionElseStatement([NotNull] RAGEParser.SelectionElseStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.statement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitStatement([NotNull] RAGEParser.StatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.labeledStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitLabeledStatement([NotNull] RAGEParser.LabeledStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.compoundStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompoundStatement([NotNull] RAGEParser.CompoundStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.blockItemList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockItemList([NotNull] RAGEParser.BlockItemListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.blockItem"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitBlockItem([NotNull] RAGEParser.BlockItemContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.expressionStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExpressionStatement([NotNull] RAGEParser.ExpressionStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.iterationStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIterationStatement([NotNull] RAGEParser.IterationStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.jumpStatement"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitJumpStatement([NotNull] RAGEParser.JumpStatementContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.compilationUnit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitCompilationUnit([NotNull] RAGEParser.CompilationUnitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.translationUnit"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitTranslationUnit([NotNull] RAGEParser.TranslationUnitContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.externalDeclaration"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitExternalDeclaration([NotNull] RAGEParser.ExternalDeclarationContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.functionDefinition"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitFunctionDefinition([NotNull] RAGEParser.FunctionDefinitionContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.declarationList"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitDeclarationList([NotNull] RAGEParser.DeclarationListContext context);
	/// <summary>
	/// Visit a parse tree produced by <see cref="RAGEParser.includeExpression"/>.
	/// </summary>
	/// <param name="context">The parse tree.</param>
	/// <return>The visitor result.</return>
	Result VisitIncludeExpression([NotNull] RAGEParser.IncludeExpressionContext context);
}
