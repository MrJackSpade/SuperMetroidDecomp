using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Gameplay names its random choices, never the random number's places (SME6486, #2563): in <c>SuperMetroid.Core</c>, a
/// <c>RandomSampleField</c> is built by its constructor (by name or target-typed, or as a <c>with</c> copy), which states the
/// places of the random number a choice reads by position and width, only inside a <c>[CompatibilityService]</c> type, read
/// on the type and every type containing it. Everywhere else a choice reads a named roll: the field's own rolls stated as
/// outcomes (<c>OneOf</c>, <c>SecondOf</c>, <c>RangePart</c>, <c>Whole</c>, <c>Nothing</c>) or a compatibility service's
/// member named for the choice whose places the game read otherwise. Counter phases are rhythms
/// (<c>UpdateAlternation</c>, <c>UpdateCycle</c>, <c>UpdateCadence</c>) and hold no places of a counter at all.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RandomPlaceSelectionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>Core states the places of the random number a choice reads outside a compatibility service.</summary>
    public const string RandomPlaceSelectionId = "SME6486";

    /// <summary>The random sample field type whose construction is tracked.</summary>
    private const string RandomSampleFieldType = "SuperMetroid.Core.Compatibility.RandomSampleField";

    /// <summary>The attribute that marks a compatibility service.</summary>
    private const string CompatibilityServiceAttribute = "CompatibilityServiceAttribute";

    private static readonly DiagnosticDescriptor Rule = new(
        RandomPlaceSelectionId,
        "Random number places selected outside a compatibility service",
        "'{0}' selects places of the random number by position and width; name the roll (RandomSampleField.OneOf, SecondOf or RangePart) or a compatibility service's member for the choice",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A random choice in gameplay code names its roll by its outcomes: the random number's ordinary roll " +
            "(OneOf), its second roll (SecondOf), a part of its range (RangePart), the whole number or nothing. A choice the " +
            "game took from other places of the number reads a member of a [CompatibilityService] type named for that choice, " +
            "the one place a field is built from a position and a width (#2563).");

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
            start.RegisterOperationAction(operation => Analyze(operation, field), OperationKind.ObjectCreation, OperationKind.With);
        });
    }

    /// <summary>Reports a field built from places outside a compatibility service.</summary>
    private static void Analyze(OperationAnalysisContext context, INamedTypeSymbol field)
    {
        ITypeSymbol? built = context.Operation switch
        {
            IObjectCreationOperation creation => creation.Type,
            IWithOperation with => with.Type,
            _ => null,
        };
        if (!SymbolEqualityComparer.Default.Equals(built, field))
            return;
        for (INamedTypeSymbol? type = context.ContainingSymbol.ContainingType; type is not null; type = type.ContainingType)
        {
            if (type.GetAttributes().Any(attribute => attribute.AttributeClass?.Name == CompatibilityServiceAttribute))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, context.Operation.Syntax.GetLocation(), context.Operation.Syntax.ToString()));
    }
}
