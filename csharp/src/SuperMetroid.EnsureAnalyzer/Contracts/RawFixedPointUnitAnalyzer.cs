using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Speeds, accelerations, distances and turns are written in named units, never raw fixed-point parts (SME6465, #2555): in
/// <c>SuperMetroid.Core</c>, outside the type that declares the factory and outside an <c>[EncodingOwner]</c> type, a
/// constant passed to a raw-unit factory (any <c>FromTwoHundredFiftySixths…</c>, <c>FromSixteenths…</c>,
/// <c>FromParts</c>, <c>FromFractionsOfStep</c>, <c>FromEighthsOfStep</c>, <c>FromHalfSteps</c> or <c>FromFraction</c>) or
/// to a parameter that counts a raw fraction (<c>fraction</c>, <c>twoHundredFiftySixths</c>, <c>sixteenths</c>,
/// <c>fractionsOfStep</c>, <c>subposition</c>) is reported; so is a power-of-two fraction factory
/// (<c>FromPixelFraction</c>, <c>FromStepFraction</c>, <c>FromMarkFraction</c>, <c>FromEnergyFraction</c>,
/// <c>FromPlaceFraction</c>, <c>FromFloorPixelsPerScreenPixel</c>) given a constant fraction not in its lowest terms, and
/// a shift of a computed value by a literal count. Such an amount is written as zero, whole units or a fraction in its
/// lowest terms through the measure's domain factory, and a scale is a named size; the encoding owner alone counts the
/// measure's finest parts.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RawFixedPointUnitAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core writes a measure in raw fixed-point parts or shifts a value by a literal count.</summary>
    public const string RawFixedPointUnitId = "SME6465";

    /// <summary>The attribute that marks the single owner of an encoding.</summary>
    private const string EncodingOwnerAttribute = "SuperMetroid.Core.EncodingOwnerAttribute";

    /// <summary>The factories that count a measure in its raw parts, whatever their parameters are named.</summary>
    private static readonly ImmutableHashSet<string> RawFactories =
        ["FromParts", "FromFractionsOfStep", "FromEighthsOfStep", "FromHalfSteps", "FromFraction"];

    /// <summary>The name prefixes of the factories that count a measure in its raw parts.</summary>
    private static readonly ImmutableArray<string> RawFactoryPrefixes = ["FromTwoHundredFiftySixths", "FromSixteenths"];

    /// <summary>The parameter names that count a raw fraction of a unit.</summary>
    private static readonly ImmutableHashSet<string> RawParameters =
        ["fraction", "twoHundredFiftySixths", "twoHundredFiftySixthsOfPixel", "twoHundredFiftySixthsOfMark", "sixteenths",
            "fractionsOfStep", "subposition"];

    /// <summary>The power-of-two fraction factories, whose constant fractions are written in lowest terms.</summary>
    private static readonly ImmutableHashSet<string> FractionFactories =
        ["FromPixelFraction", "FromStepFraction", "FromMarkFraction", "FromEnergyFraction", "FromPlaceFraction",
            "FromFloorPixelsPerScreenPixel"];

    private static readonly DiagnosticDescriptor Rule = new(
        RawFixedPointUnitId,
        "Measure written in raw fixed-point parts",
        "'{0}' {1}",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A speed, acceleration, distance, turn or zoom is written as zero, whole units or a power-of-two fraction " +
            "in its lowest terms through its measure's domain factory, never as a raw count of 256ths, 65536ths, sixteenths " +
            "or raw parts, and a scale is a named size rather than a literal shift; the encoding owner alone counts the " +
            "measure's finest parts (#2555).");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(start =>
        {
            if (start.Compilation.AssemblyName != CoreLayerAnalyzer.CoreAssembly)
                return;
            INamedTypeSymbol? owner = start.Compilation.GetTypeByMetadataName(EncodingOwnerAttribute);
            start.RegisterOperationAction(operation => AnalyzeCall(operation, owner), OperationKind.Invocation, OperationKind.ObjectCreation);
            start.RegisterOperationAction(operation => AnalyzeShift(operation, owner), OperationKind.Binary, OperationKind.CompoundAssignment);
        });
    }

    /// <summary>Reports a raw-unit factory or parameter given a constant, and a fraction factory given one not in lowest terms.</summary>
    private static void AnalyzeCall(OperationAnalysisContext context, INamedTypeSymbol? owner)
    {
        (IMethodSymbol? method, ImmutableArray<IArgumentOperation> arguments) = context.Operation switch
        {
            IInvocationOperation invocation => (invocation.TargetMethod, invocation.Arguments),
            IObjectCreationOperation creation => (creation.Constructor, creation.Arguments),
            _ => (null, ImmutableArray<IArgumentOperation>.Empty),
        };
        if (method is null || IsExempt(context.ContainingSymbol, method.ContainingType, owner))
            return;

        bool rawFactory = RawFactories.Contains(method.Name) || RawFactoryPrefixes.Any(prefix => method.Name.StartsWith(prefix, StringComparison.Ordinal));
        foreach (IArgumentOperation argument in arguments)
        {
            if (argument.ArgumentKind != ArgumentKind.Explicit || argument.Parameter is not { } parameter ||
                !argument.Value.ConstantValue.HasValue || !IsIntegral(parameter.Type))
                continue;
            if (rawFactory || RawParameters.Contains(parameter.Name))
            {
                context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), context.Operation.Syntax.ToString(),
                    "writes a constant in a measure's raw parts; write it as zero, whole units or a power-of-two fraction in its lowest terms through the measure's domain factory"));
                return;
            }
        }

        if (FractionFactories.Contains(method.Name) && arguments.Length == 2 &&
            arguments[0].Value.ConstantValue is { HasValue: true, Value: int numerator } &&
            arguments[1].Value.ConstantValue is { HasValue: true, Value: int denominator } &&
            (denominator == 1 || (denominator > 1 && numerator % 2 == 0)))
        {
            context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), context.Operation.Syntax.ToString(),
                "writes a fraction not in its lowest terms; write whole units through the whole-unit factory and any other fraction in its lowest terms"));
        }
    }

    /// <summary>Reports a shift of a computed value by a constant count.</summary>
    private static void AnalyzeShift(OperationAnalysisContext context, INamedTypeSymbol? owner)
    {
        (BinaryOperatorKind kind, IOperation? left, IOperation? right) = context.Operation switch
        {
            IBinaryOperation binary => (binary.OperatorKind, binary.LeftOperand, binary.RightOperand),
            ICompoundAssignmentOperation compound => (compound.OperatorKind, compound.Target, compound.Value),
            _ => (BinaryOperatorKind.None, null, null),
        };
        if (kind is not (BinaryOperatorKind.LeftShift or BinaryOperatorKind.RightShift or BinaryOperatorKind.UnsignedRightShift) ||
            left is null || right is null || left.ConstantValue.HasValue || !IsLiteral(right) ||
            IsExempt(context.ContainingSymbol, null, owner))
            return;
        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), context.Operation.Syntax.ToString(),
            "shifts a value by a literal count; name the size it multiplies or divides by, or let the measure's encoding owner count its parts"));
    }

    /// <summary>
    /// Whether code in <paramref name="symbol"/> may count a measure's raw parts: it lies in the type that declares the
    /// factory called, or in an encoding owner, the single owner of a stored form.
    /// </summary>
    private static bool IsExempt(ISymbol symbol, INamedTypeSymbol? declaring, INamedTypeSymbol? owner)
    {
        for (INamedTypeSymbol? type = symbol as INamedTypeSymbol ?? symbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (declaring is not null && SymbolEqualityComparer.Default.Equals(type.OriginalDefinition, declaring.OriginalDefinition))
                return true;
            if (owner is not null && type.GetAttributes().Any(attribute => SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, owner)))
                return true;
        }
        return false;
    }

    /// <summary>Whether an operand is a numeric literal written in the source, beneath any conversions and parentheses.</summary>
    private static bool IsLiteral(IOperation operand)
    {
        while (operand is IConversionOperation conversion)
            operand = conversion.Operand;
        SyntaxNode syntax = operand.Syntax;
        while (syntax is ParenthesizedExpressionSyntax parenthesized)
            syntax = parenthesized.Expression;
        return syntax.IsKind(SyntaxKind.NumericLiteralExpression);
    }

    /// <summary>Whether a type is a built-in integral type.</summary>
    private static bool IsIntegral(ITypeSymbol type) => type.SpecialType is SpecialType.System_Byte or SpecialType.System_SByte
        or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32
        or SpecialType.System_Int64 or SpecialType.System_UInt64;
}
