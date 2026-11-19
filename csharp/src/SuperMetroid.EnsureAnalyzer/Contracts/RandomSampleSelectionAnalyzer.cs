using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A random choice is a named choice, never a field read mapped by hand (SME6461, #2510): in <c>SuperMetroid.Core</c>,
/// the value a call reads from a random sample through a <c>RandomSampleField</c> (its own <c>Of</c> and
/// <c>OfAdvancedBy</c>, or any call that takes a field and returns a number, as the enemies' <c>Next</c> draw does) is
/// not used to index a list or an <c>Ensure.Element</c> lookup, compared with a constant, matched against a constant
/// pattern or switched on, whether read directly or through a local it was stored in. Those are choices among outcomes
/// and chances taken by some of the field's values; <c>WeightedRandomChoice</c> (by shares, or <c>InTurn</c>) and
/// <c>RandomChance</c> are the one place that maps a sample to them. A field read used as a number (an offset, a delay,
/// a speed, an angle) is not a choice and is not reported.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RandomSampleSelectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core maps a random sample field's value to an outcome by hand.</summary>
    public const string HandMappedRandomFieldId = "SME6461";

    /// <summary>The field type whose reads are tracked.</summary>
    private const string RandomSampleFieldType = "SuperMetroid.Core.Compatibility.RandomSampleField";

    private static readonly DiagnosticDescriptor Rule = new(
        HandMappedRandomFieldId,
        "Random sample field mapped to an outcome by hand",
        "'{0}' {1} a value read from a random sample field; make the choice a named WeightedRandomChoice or RandomChance",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A random sample field's value that indexes a list, is compared with a constant, matched against a " +
            "constant pattern or switched on is a choice among outcomes or a chance taken by some of its values. A catalog " +
            "names it as a WeightedRandomChoice (by shares or InTurn) or a RandomChance, the one place that maps a sample " +
            "to an outcome; the draw, its count and its order stay as they were (#2510).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly ||
                start.Compilation.GetTypeByMetadataName(RandomSampleFieldType) is not { } field)
                return;
            start.RegisterOperationBlockAction(block => AnalyzeBlock(block, field));
        });
    }

    /// <summary>
    /// Finds the locals a field read is stored in, then reports every index, constant comparison, constant pattern and
    /// switch that reads a field value directly or through one of them.
    /// </summary>
    private static void AnalyzeBlock(OperationBlockAnalysisContext context, INamedTypeSymbol field)
    {
        IOperation[] operations = [.. context.OperationBlocks.SelectMany(block => block.DescendantsAndSelf())];
        var fieldLocals = new HashSet<ILocalSymbol>(SymbolEqualityComparer.Default);
        foreach (IOperation operation in operations)
        {
            ILocalSymbol? stored = operation switch
            {
                IVariableDeclaratorOperation { Initializer.Value: var value, Symbol: var local } when IsFieldRead(value, field) => local,
                ISimpleAssignmentOperation { Target: ILocalReferenceOperation { Local: var local }, Value: var value }
                    when IsFieldRead(value, field) => local,
                _ => null,
            };
            if (stored is not null)
                fieldLocals.Add(stored);
        }

        bool IsFieldValue(IOperation? candidate) => candidate is not null && Unwrapped(candidate) switch
        {
            ILocalReferenceOperation { Local: var local } => fieldLocals.Contains(local),
            var unwrapped => IsFieldRead(unwrapped, field),
        };

        foreach (IOperation operation in operations)
        {
            string? use = operation switch
            {
                IArrayElementReferenceOperation element when element.Indices.Any(IsFieldValue) => "indexes a list with",
                IPropertyReferenceOperation { Property.IsIndexer: true } indexer
                    when indexer.Arguments.Any(argument => IsFieldValue(argument.Value)) => "indexes a list with",
                IImplicitIndexerReferenceOperation implicitIndexer when IsFieldValue(implicitIndexer.Argument) => "indexes a list with",
                IInvocationOperation invocation when IsElementLookup(invocation.TargetMethod) &&
                    invocation.Arguments.Any(argument => IsFieldValue(argument.Value)) => "looks up a list element by",
                IBinaryOperation binary when IsComparison(binary.OperatorKind) &&
                    (IsFieldValue(binary.LeftOperand) && binary.RightOperand.ConstantValue.HasValue ||
                        IsFieldValue(binary.RightOperand) && binary.LeftOperand.ConstantValue.HasValue) => "compares with a constant",
                IIsPatternOperation pattern when IsFieldValue(pattern.Value) && pattern.Pattern is not IDeclarationPatternOperation
                    and not IDiscardPatternOperation => "matches a constant pattern against",
                ISwitchOperation switchStatement when IsFieldValue(switchStatement.Value) => "switches on",
                ISwitchExpressionOperation switchExpression when IsFieldValue(switchExpression.Value) => "switches on",
                _ => null,
            };
            if (use is not null)
                context.ReportDiagnostic(Diagnostic.Create(Rule, operation.Syntax.GetLocation(), operation.Syntax.ToString(), use));
        }
    }

    /// <summary>
    /// Whether an operation is a call that reads a number from a random sample field: a call on a field, or a call taking
    /// one, that returns an integral number.
    /// </summary>
    private static bool IsFieldRead(IOperation operation, INamedTypeSymbol field) =>
        Unwrapped(operation) is IInvocationOperation invocation && IsIntegral(invocation.Type) &&
        (IsField(invocation.Instance?.Type, field) || invocation.Arguments.Any(argument => IsField(argument.Value.Type, field)));

    /// <summary>Whether a type is the random sample field type.</summary>
    private static bool IsField(ITypeSymbol? type, INamedTypeSymbol field) => SymbolEqualityComparer.Default.Equals(type, field);

    /// <summary>Whether a method looks a list element up by position: <c>Ensure.Element</c> or LINQ's <c>ElementAt</c>.</summary>
    private static bool IsElementLookup(IMethodSymbol method) =>
        method.Name == "Element" && method.ContainingType.Name == "Ensure" ||
        method.Name is "ElementAt" or "ElementAtOrDefault";

    /// <summary>Whether an operator compares its operands.</summary>
    private static bool IsComparison(BinaryOperatorKind kind) => kind is BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals
        or BinaryOperatorKind.LessThan or BinaryOperatorKind.LessThanOrEqual or BinaryOperatorKind.GreaterThan
        or BinaryOperatorKind.GreaterThanOrEqual;

    /// <summary>The operation beneath any conversions and parentheses.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (true)
        {
            if (operation is IConversionOperation conversion)
                operation = conversion.Operand;
            else if (operation is IParenthesizedOperation parenthesized)
                operation = parenthesized.Operand;
            else
                return operation;
        }
    }

    /// <summary>Whether a type is a built-in integral type.</summary>
    private static bool IsIntegral(ITypeSymbol? type) => type?.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
        or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32
        or SpecialType.System_Int64 or SpecialType.System_UInt64;
}
