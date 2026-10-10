using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace SuperMetroid.EnsureAnalyzer;

/// <summary>
/// Finds closed domains that circulate as primitives and closed-domain switches that ignore
/// unexpected values (#627).
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrimitiveDomainAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A primitive is switched over a set of named constants: it is a closed domain.</summary>
    public const string PrimitiveSwitchId = "SME6270";

    /// <summary>A switch over a repository-owned enum lets undefined or unhandled values pass silently.</summary>
    public const string SilentEnumSwitchId = "SME6271";

    /// <summary>A primitive local extracted by mask or shift is then treated as a closed selector.</summary>
    public const string MaskedSelectorId = "SME6272";

    private static readonly DiagnosticDescriptor PrimitiveSwitchRule = new(
        PrimitiveSwitchId,
        "Closed domain switched as a primitive",
        "'{0}' is a {1} switched over named constants of {2}; model the domain as a type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Values switched over named constants form a closed domain and must use an enum or domain type end to end.");

    private static readonly DiagnosticDescriptor SilentEnumSwitchRule = new(
        SilentEnumSwitchId,
        "Closed-domain switch ignores unexpected values",
        "Switch over {0} {1}; handle every member explicitly and fail loudly on undefined values",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A switch over a closed domain must not silently ignore unhandled or undefined values. " +
            "A [Flags] enum is exempt: its values are combinations of members, so a catch-all for the " +
            "combinations a switch does not name is part of the domain, not an ignored member.");

    private static readonly DiagnosticDescriptor MaskedSelectorRule = new(
        MaskedSelectorId,
        "Masked primitive used as a selector",
        "Local '{0}' extracts a field by {1} and is then switched over as a selector; decode it to a domain type",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "A discriminator extracted by mask or shift must be decoded to its domain type rather than carried as a primitive.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [PrimitiveSwitchRule, SilentEnumSwitchRule, MaskedSelectorRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeSwitchStatement, OperationKind.Switch);
        context.RegisterOperationAction(AnalyzeSwitchExpression, OperationKind.SwitchExpression);
    }

    private static void AnalyzeSwitchStatement(OperationAnalysisContext context)
    {
        var operation = (ISwitchOperation)context.Operation;
        IOperation value = Unwrap(operation.Value);
        IEnumerable<IOperation> labels = operation.Cases
            .SelectMany(section => section.Clauses)
            .SelectMany(LabelValues);
        AnalyzePrimitiveSwitch(context, value, labels, operation.Syntax);
        AnalyzeMaskedSelector(context, value);

        if (value.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType))
        {
            ISwitchCaseOperation? defaultSection = operation.Cases.FirstOrDefault(section =>
                section.Clauses.Any(clause => clause.CaseKind == CaseKind.Default));
            if (defaultSection is null)
            {
                if (!CoversAllMembers(enumType, labels))
                    Report(context, SilentEnumSwitchRule, operation.Syntax.GetLocation(),
                        enumType.Name, "has no default and leaves members unhandled");
            }
            else if (!AlwaysThrows(defaultSection.Body))
            {
                Report(context, SilentEnumSwitchRule, DefaultLocation(defaultSection), enumType.Name,
                    "has a default that does not fail");
            }
        }
    }

    private static void AnalyzeSwitchExpression(OperationAnalysisContext context)
    {
        var operation = (ISwitchExpressionOperation)context.Operation;
        IOperation value = Unwrap(operation.Value);
        IEnumerable<IOperation> labels = operation.Arms.SelectMany(arm => PatternValues(arm.Pattern));
        AnalyzePrimitiveSwitch(context, value, labels, operation.Syntax);
        AnalyzeMaskedSelector(context, value);

        if (value.Type is INamedTypeSymbol { TypeKind: TypeKind.Enum } enumType && IsOwned(enumType) && !IsFlags(enumType))
        {
            foreach (ISwitchExpressionArmOperation arm in operation.Arms)
            {
                if (arm.Pattern is IDiscardPatternOperation && arm.Guard is null &&
                    Unwrap(arm.Value) is not IThrowOperation)
                {
                    Report(context, SilentEnumSwitchRule, arm.Syntax.GetLocation(), enumType.Name,
                        "has a discard arm that does not fail");
                }
            }
        }
    }

    private static void AnalyzePrimitiveSwitch(OperationAnalysisContext context, IOperation value,
        IEnumerable<IOperation> labels, SyntaxNode syntax)
    {
        if (!IsIntegral(value.Type))
            return;
        // Two or more labels drawn from one catalog's named constants show a closed domain.
        var catalogs = labels
            .Select(Unwrap)
            .OfType<IFieldReferenceOperation>()
            .Where(reference => reference.Field.IsConst)
            .GroupBy(reference => reference.Field.ContainingType, SymbolEqualityComparer.Default)
            .Where(group => group.Select(reference => reference.Field.Name).Distinct().Count() >= 2)
            .Select(group => group.Key!)
            .ToArray();
        if (catalogs.Length == 0)
            return;
        Report(context, PrimitiveSwitchRule, syntax.GetLocation(), Describe(value), value.Type!.ToDisplayString(),
            string.Join(", ", catalogs.Select(catalog => catalog.Name)));
    }

    private static void AnalyzeMaskedSelector(OperationAnalysisContext context, IOperation value)
    {
        if (value is not ILocalReferenceOperation { Local: var local } || !IsIntegral(local.Type))
            return;
        foreach (SyntaxReference reference in local.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is not VariableDeclaratorSyntax { Initializer.Value: var initializer })
                continue;
            ExpressionSyntax stripped = initializer;
            while (stripped is ParenthesizedExpressionSyntax or CastExpressionSyntax or CheckedExpressionSyntax)
            {
                stripped = stripped switch
                {
                    ParenthesizedExpressionSyntax parenthesized => parenthesized.Expression,
                    CastExpressionSyntax cast => cast.Expression,
                    CheckedExpressionSyntax check => check.Expression,
                    _ => stripped,
                };
            }
            string? extraction = stripped.Kind() switch
            {
                SyntaxKind.BitwiseAndExpression => "mask",
                SyntaxKind.RightShiftExpression or SyntaxKind.UnsignedRightShiftExpression => "shift",
                _ => null,
            };
            if (extraction is not null)
                Report(context, MaskedSelectorRule, reference.GetSyntax().GetLocation(), local.Name, extraction);
        }
    }

    private static IEnumerable<IOperation> LabelValues(ICaseClauseOperation clause) => clause switch
    {
        ISingleValueCaseClauseOperation single => [single.Value],
        IPatternCaseClauseOperation pattern => PatternValues(pattern.Pattern),
        _ => [],
    };

    private static IEnumerable<IOperation> PatternValues(IPatternOperation pattern) => pattern switch
    {
        IConstantPatternOperation constant => [constant.Value],
        IBinaryPatternOperation binary => PatternValues(binary.LeftPattern).Concat(PatternValues(binary.RightPattern)),
        INegatedPatternOperation negated => PatternValues(negated.Pattern),
        _ => [],
    };

    private static bool CoversAllMembers(INamedTypeSymbol enumType, IEnumerable<IOperation> labels)
    {
        var handled = new HashSet<object?>(labels.Select(label => Unwrap(label).ConstantValue)
            .Where(constant => constant.HasValue)
            .Select(constant => constant.Value));
        return enumType.GetMembers().OfType<IFieldSymbol>()
            .Where(field => field.HasConstantValue)
            .All(field => handled.Contains(field.ConstantValue));
    }

    private static bool AlwaysThrows(ImmutableArray<IOperation> body)
    {
        IOperation? last = body.LastOrDefault();
        while (last is IBlockOperation block)
            last = block.Operations.LastOrDefault();
        return last switch
        {
            IThrowOperation => true,
            IExpressionStatementOperation { Operation: IThrowOperation } => true,
            _ => false,
        };
    }

    private static Location DefaultLocation(ISwitchCaseOperation section) =>
        section.Clauses.First(clause => clause.CaseKind == CaseKind.Default).Syntax.GetLocation();

    private static bool IsOwned(INamedTypeSymbol type) =>
        type.Locations.Any(location => location.IsInSource);

    private static bool IsFlags(INamedTypeSymbol type) =>
        type.GetAttributes().Any(attribute => attribute.AttributeClass?.ToDisplayString() == "System.FlagsAttribute");

    private static bool IsIntegral(ITypeSymbol? type) => type?.SpecialType is
        SpecialType.System_Byte or SpecialType.System_SByte or
        SpecialType.System_Int16 or SpecialType.System_UInt16 or
        SpecialType.System_Int32 or SpecialType.System_UInt32 or
        SpecialType.System_Int64 or SpecialType.System_UInt64;

    private static IOperation Unwrap(IOperation operation)
    {
        while (operation is IConversionOperation { IsImplicit: true } conversion)
            operation = conversion.Operand;
        return operation;
    }

    private static string Describe(IOperation value) => value switch
    {
        ILocalReferenceOperation local => local.Local.Name,
        IParameterReferenceOperation parameter => parameter.Parameter.Name,
        IFieldReferenceOperation field => field.Field.Name,
        IPropertyReferenceOperation property => property.Property.Name,
        _ => value.Syntax.ToString(),
    };

    private static void Report(OperationAnalysisContext context, DiagnosticDescriptor rule, Location location,
        params object[] arguments) =>
        context.ReportDiagnostic(Diagnostic.Create(rule, location, arguments));
}
