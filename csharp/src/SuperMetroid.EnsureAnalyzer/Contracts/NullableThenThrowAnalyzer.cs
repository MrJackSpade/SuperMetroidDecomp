using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A required value is never held nullable and thrown on (SME6475, #2554): in <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c>, a value the repository's own source declares nullable is not read through a throw
/// on its absence. A field, property, parameter, local, source method result, downcast or collection element tested for
/// null only to throw (<c>value ?? throw</c>, <c>if (value is null) throw</c>, <c>value is { } v ? v : throw</c>,
/// <c>case null: throw</c>) says the value is always present while its type says it may be absent: a value that is
/// always present is non-nullable from construction, and an absent case is a named member of the choice its owner
/// holds. An installed document's required member is <c>required</c> and non-nullable, so the deserializer rejects its
/// absence where the file is read.
/// <para>
/// The nullability of a member declared outside the repository is that library's contract, not a choice of this source:
/// a deserializer that reads the literal <c>null</c>, a search that finds nothing or a path with no parent is checked
/// where it is called, and the rule leaves that read alone.
/// </para>
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullableThenThrowAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A value the source declares nullable is thrown on when absent.</summary>
    public const string NullableThenThrowId = "SME6475";

    /// <summary>The import pipeline, held to the rule alongside Core.</summary>
    public const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    private static readonly DiagnosticDescriptor Rule = new(
        NullableThenThrowId,
        "A required value is held nullable and thrown on",
        "'{0}' is held nullable and thrown on when absent; make it non-nullable where it is built, or name the absent case in the choice that holds it",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A value that is always present is non-nullable from construction, and an absent case is a named member " +
            "of the choice its owner holds. Reading a nullable field, property, parameter, local, method result, downcast or " +
            "element through a throw on null keeps the value's type and its meaning apart and reconciles them at run time " +
            "(#2554). A member declared outside the repository keeps its library's nullability and is not reported.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName is not (CoreLayerAnalyzer.CoreAssembly or ExtractionAssembly))
                return;
            IAssemblySymbol source = start.Compilation.Assembly;
            start.RegisterOperationAction(operation => AnalyzeCoalesce(operation, source), OperationKind.Coalesce);
            start.RegisterOperationAction(operation => AnalyzeIf(operation, source), OperationKind.Conditional);
            start.RegisterOperationAction(operation => AnalyzeSwitch(operation, source), OperationKind.Switch);
            start.RegisterOperationAction(operation => AnalyzeSwitchExpression(operation, source), OperationKind.SwitchExpression);
        });
    }

    /// <summary>Reports <c>value ?? throw</c>.</summary>
    private static void AnalyzeCoalesce(OperationAnalysisContext context, IAssemblySymbol source)
    {
        var coalesce = (ICoalesceOperation)context.Operation;
        if (IsThrow(coalesce.WhenNull))
            Report(context, coalesce.Value, source, coalesce.Syntax);
    }

    /// <summary>
    /// Reports <c>if (value is null) throw</c>, <c>value is null ? throw : other</c> and
    /// <c>value is { } v ? v : throw</c>, in statement or expression form.
    /// </summary>
    private static void AnalyzeIf(OperationAnalysisContext context, IAssemblySymbol source)
    {
        var conditional = (IConditionalOperation)context.Operation;
        if (NullTest(conditional.Condition, out bool trueWhenNull) is not { } tested)
            return;
        IOperation? throwing = trueWhenNull ? conditional.WhenTrue : conditional.WhenFalse;
        if (throwing is not null && IsThrow(throwing))
            Report(context, tested, source, conditional.Condition.Syntax);
    }

    /// <summary>Reports a <c>case null:</c> section whose only statement throws.</summary>
    private static void AnalyzeSwitch(OperationAnalysisContext context, IAssemblySymbol source)
    {
        var @switch = (ISwitchOperation)context.Operation;
        foreach (ISwitchCaseOperation section in @switch.Cases)
        {
            bool matchesNull = section.Clauses.Any(clause =>
                (clause is IPatternCaseClauseOperation { Pattern: var pattern, Guard: null } && IsNullPattern(pattern)) ||
                (clause is ISingleValueCaseClauseOperation { Value: var value } && IsNullConstant(value)));
            if (matchesNull && section.Body.Length == 1 && IsThrow(section.Body[0]))
                Report(context, @switch.Value, source, section.Syntax);
        }
    }

    /// <summary>Reports a <c>null =&gt; throw</c> arm.</summary>
    private static void AnalyzeSwitchExpression(OperationAnalysisContext context, IAssemblySymbol source)
    {
        var @switch = (ISwitchExpressionOperation)context.Operation;
        foreach (ISwitchExpressionArmOperation arm in @switch.Arms)
        {
            if (arm.Guard is null && IsNullPattern(arm.Pattern) && IsThrow(arm.Value))
                Report(context, @switch.Value, source, arm.Syntax);
        }
    }

    /// <summary>Reports the tested value when the source declares its nullability.</summary>
    private static void Report(OperationAnalysisContext context, IOperation tested, IAssemblySymbol source, SyntaxNode at)
    {
        if (!DeclaredBySource(tested, source))
            return;
        string text = Unconverted(tested).Syntax.ToString();
        if (text.Length > 60)
            text = text.Substring(0, 57) + "...";
        context.ReportDiagnostic(Diagnostic.Create(Rule, at.GetLocation(), text));
    }

    /// <summary>
    /// The value a condition tests for null: <c>value is null</c>, <c>value == null</c>, <c>value is not { } v</c> and
    /// <c>value is not null</c> are true when it is absent; <c>value is { } v</c>, <c>value is not null</c> negated and
    /// <c>value != null</c> are true when it is present.
    /// </summary>
    private static IOperation? NullTest(IOperation condition, out bool trueWhenNull)
    {
        trueWhenNull = true;
        switch (condition)
        {
            case IUnaryOperation { OperatorKind: UnaryOperatorKind.Not, Operand: var operand }:
                IOperation? negated = NullTest(operand, out bool inner);
                trueWhenNull = !inner;
                return negated;
            case IIsPatternOperation { Pattern: var pattern, Value: var value }:
                if (IsNullPattern(pattern))
                    return value;
                if (IsNotNullPattern(pattern))
                {
                    trueWhenNull = false;
                    return value;
                }
                return null;
            case IBinaryOperation { OperatorKind: BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals } binary:
                trueWhenNull = binary.OperatorKind == BinaryOperatorKind.Equals;
                if (IsNullConstant(binary.RightOperand))
                    return binary.LeftOperand;
                if (IsNullConstant(binary.LeftOperand))
                    return binary.RightOperand;
                return null;
            default:
                return null;
        }
    }

    /// <summary>A pattern that matches only null: <c>null</c> or <c>not { }</c> or <c>not not null</c>.</summary>
    private static bool IsNullPattern(IPatternOperation pattern) => pattern switch
    {
        IConstantPatternOperation { Value: var value } => IsNullConstant(value),
        INegatedPatternOperation { Pattern: var inner } => IsNotNullPattern(inner),
        _ => false,
    };

    /// <summary>A pattern that matches every present value: <c>{ }</c>, <c>{ } v</c>, the value's own type or <c>not null</c>.</summary>
    private static bool IsNotNullPattern(IPatternOperation pattern) => pattern switch
    {
        IRecursivePatternOperation { DeconstructionSubpatterns.Length: 0, PropertySubpatterns.Length: 0, MatchedType: var matched } =>
            matched is null || IsInputType(matched, pattern.InputType),
        IDeclarationPatternOperation { MatchedType: var matched } => matched is null || IsInputType(matched, pattern.InputType),
        ITypePatternOperation { MatchedType: var matched } => IsInputType(matched, pattern.InputType),
        INegatedPatternOperation { Pattern: var inner } => IsNullPattern(inner),
        _ => false,
    };

    /// <summary>Whether a pattern's type is the tested value's own type, so matching it only tests for presence.</summary>
    private static bool IsInputType(ITypeSymbol matched, ITypeSymbol input) =>
        SymbolEqualityComparer.Default.Equals(matched, input) ||
        (input is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments: [var underlying] } &&
            SymbolEqualityComparer.Default.Equals(matched, underlying));

    /// <summary>The constant <c>null</c>, through any conversion.</summary>
    private static bool IsNullConstant(IOperation operation) =>
        Unconverted(operation) is { ConstantValue: { HasValue: true, Value: null } };

    /// <summary>A throw expression or statement, through conversions, a lone expression statement or a one-statement block.</summary>
    private static bool IsThrow(IOperation operation) => operation switch
    {
        IThrowOperation => true,
        IConversionOperation { Operand: var operand } => IsThrow(operand),
        IExpressionStatementOperation { Operation: var inner } => IsThrow(inner),
        IBlockOperation { Operations: [var only] } => IsThrow(only),
        _ => false,
    };

    /// <summary>
    /// Whether the repository's source declares the tested value's nullability: its own fields, properties, parameters,
    /// locals, method results, downcasts and the elements of its own collections. A method or property declared outside
    /// the repository keeps that library's contract.
    /// </summary>
    private static bool DeclaredBySource(IOperation operation, IAssemblySymbol source)
    {
        switch (operation)
        {
            case IConversionOperation { IsTryCast: true }:
                return true;
            case IConversionOperation { Operand: var operand }:
                return DeclaredBySource(operand, source);
            case IInvocationOperation { TargetMethod: var method }:
                return method.MethodKind == MethodKind.DelegateInvoke || IsSource(method, source);
            case IPropertyReferenceOperation { Property: { IsIndexer: true } indexer, Instance: var collection }:
                return IsSource(indexer, source) || collection is not null && DeclaredBySource(collection, source);
            case IPropertyReferenceOperation { Property: var property }:
                return IsSource(property, source);
            case IFieldReferenceOperation { Field: var field }:
                return IsSource(field, source);
            case IArrayElementReferenceOperation { ArrayReference: var array }:
                return DeclaredBySource(array, source);
            case IConditionalAccessOperation { Operation: var receiver, WhenNotNull: var read }:
                return DeclaredBySource(receiver, source) || DeclaredBySource(read, source);
            case IConditionalAccessInstanceOperation:
                return false;
            case IAwaitOperation { Operation: var awaited }:
                return DeclaredBySource(awaited, source);
            default:
                return true;
        }
    }

    /// <summary>Whether a member is declared in the analyzed assembly.</summary>
    private static bool IsSource(ISymbol member, IAssemblySymbol source) =>
        SymbolEqualityComparer.Default.Equals(member.ContainingAssembly, source);

    /// <summary>The operand beneath any implicit conversions.</summary>
    private static IOperation Unconverted(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true, Operand: var operand })
            operation = operand;
        return operation;
    }
}
