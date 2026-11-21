using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// A closed choice is one typed value, never a bool standing for it (SME6485, #2560): in <c>SuperMetroid.Core</c> and
/// <c>SuperMetroid.AssetExtraction</c>, no conditional on a bool parameter, field or stored property chooses between the
/// two members of a two-member enum (<c>left ? Side.Left : Side.Right</c>), and no comparison with a member of a two-member
/// enum (<c>side == Side.Left</c>) is stored into a bool field or property or passed to a bool parameter. Either way a bool
/// carries the choice the enum names, so the bool and the enum spell one choice twice; the code holds and passes the enum.
/// A comparison that only answers a question (a query's result, a condition, a local) is not a stored choice, and neither
/// is a computed property, a question asked of a value (<c>speed.IsNegative</c>), whose answer a conditional may turn into a
/// direction.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ClosedChoiceAsBoolAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A bool stands for a two-member enum's choice.</summary>
    public const string ClosedChoiceAsBoolId = "SME6485";

    /// <summary>The import pipeline's assembly, which the rule covers with Core.</summary>
    private const string ExtractionAssembly = "SuperMetroid.AssetExtraction";

    /// <summary>The members an enum must have for a bool to stand for its whole choice.</summary>
    private const int TwoMembers = 2;

    private static readonly DiagnosticDescriptor Rule = new(
        ClosedChoiceAsBoolId,
        "A bool stands for a closed choice",
        "'{0}' carries the choice of {1} as a bool; hold and pass the {1} value itself",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "One choice is one typed value. A conditional on a bool parameter, field or property whose arms are the " +
            "two members of a two-member enum, and a comparison with such an enum's member stored into a bool field or " +
            "property or passed to a bool parameter, spell one choice as both a bool and an enum. Hold the enum, pass the " +
            "enum, and convert at a boundary through one declared map (#2560).");

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
            start.RegisterOperationAction(AnalyzeConditional, OperationKind.Conditional);
            start.RegisterOperationAction(AnalyzeComparison, OperationKind.Binary);
        });
    }

    /// <summary>Reports a conditional on a bool member or parameter that chooses between a two-member enum's members.</summary>
    private static void AnalyzeConditional(OperationAnalysisContext context)
    {
        var conditional = (IConditionalOperation)context.Operation;
        if (conditional.WhenFalse is null || conditional.Type is null || !IsStoredBool(Unwrapped(conditional.Condition)))
            return;
        if (EnumMember(conditional.WhenTrue) is not { } whenTrue || EnumMember(conditional.WhenFalse) is not { } whenFalse)
            return;
        if (!SymbolEqualityComparer.Default.Equals(whenTrue.ContainingType, whenFalse.ContainingType) ||
            SymbolEqualityComparer.Default.Equals(whenTrue, whenFalse) || !IsTwoMemberChoice(whenTrue.ContainingType))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, conditional.Syntax.GetLocation(), conditional.Syntax.ToString(),
            whenTrue.ContainingType.Name));
    }

    /// <summary>Reports a comparison with a two-member enum's member stored into a bool member or passed to a bool parameter.</summary>
    private static void AnalyzeComparison(OperationAnalysisContext context)
    {
        var comparison = (IBinaryOperation)context.Operation;
        if (comparison.OperatorKind is not (BinaryOperatorKind.Equals or BinaryOperatorKind.NotEquals))
            return;
        IFieldSymbol? member = EnumMember(comparison.RightOperand) ?? EnumMember(comparison.LeftOperand);
        if (member is null || !IsTwoMemberChoice(member.ContainingType) || !IsStoredOrPassed(comparison))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, comparison.Syntax.GetLocation(), comparison.Syntax.ToString(),
            member.ContainingType.Name));
    }

    /// <summary>The operation with parentheses, conversions and a logical not taken off.</summary>
    private static IOperation Unwrapped(IOperation operation)
    {
        while (true)
        {
            switch (operation)
            {
                case IParenthesizedOperation parenthesized:
                    operation = parenthesized.Operand;
                    continue;
                case IConversionOperation conversion:
                    operation = conversion.Operand;
                    continue;
                case IUnaryOperation { OperatorKind: UnaryOperatorKind.Not } not:
                    operation = not.Operand;
                    continue;
                default:
                    return operation;
            }
        }
    }

    /// <summary>Whether the condition is a bool parameter, field or stored property read as it is.</summary>
    private static bool IsStoredBool(IOperation condition) =>
        condition.Type?.SpecialType == SpecialType.System_Boolean && condition switch
        {
            IParameterReferenceOperation or IFieldReferenceOperation => true,
            IPropertyReferenceOperation reference => IsStored(reference.Property),
            _ => false,
        };

    /// <summary>Whether the property holds its value in a field of its own, as an automatic property does, rather than computing it.</summary>
    private static bool IsStored(IPropertySymbol property) =>
        property.ContainingType.GetMembers().OfType<IFieldSymbol>()
            .Any(field => SymbolEqualityComparer.Default.Equals(field.AssociatedSymbol, property));

    /// <summary>The enum member the operation names, or null when it names none.</summary>
    private static IFieldSymbol? EnumMember(IOperation operation)
    {
        IOperation value = operation;
        while (value is IParenthesizedOperation or IConversionOperation)
            value = value is IParenthesizedOperation parenthesized ? parenthesized.Operand : ((IConversionOperation)value).Operand;
        return value is IFieldReferenceOperation { Field: { IsConst: true, ContainingType.TypeKind: TypeKind.Enum } field } ? field : null;
    }

    /// <summary>Whether the enum is a closed choice of exactly two members, not composable flags.</summary>
    private static bool IsTwoMemberChoice(INamedTypeSymbol type) =>
        type.TypeKind == TypeKind.Enum &&
        !type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == nameof(FlagsAttribute)) &&
        type.GetMembers().OfType<IFieldSymbol>().Count(field => field.IsConst) == TwoMembers;

    /// <summary>Whether the comparison's result is stored into a bool field or property, or passed to a bool parameter.</summary>
    private static bool IsStoredOrPassed(IOperation comparison)
    {
        IOperation result = comparison;
        while (result.Parent is IParenthesizedOperation or IConversionOperation)
            result = result.Parent;
        return result.Parent switch
        {
            IArgumentOperation { Parameter.Type.SpecialType: SpecialType.System_Boolean } => true,
            IAssignmentOperation assignment when assignment.Value == result =>
                assignment.Target is IFieldReferenceOperation or IPropertyReferenceOperation &&
                assignment.Target.Type?.SpecialType == SpecialType.System_Boolean,
            IFieldInitializerOperation initializer => initializer.InitializedFields.Any(field => field.Type.SpecialType == SpecialType.System_Boolean),
            IPropertyInitializerOperation initializer => initializer.InitializedProperties.Any(property => property.Type.SpecialType == SpecialType.System_Boolean),
            _ => false,
        };
    }
}
