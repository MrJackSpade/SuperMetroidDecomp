using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.SourceLayout;

/// <summary>
/// #621: a type whose hand-written code is split across several partial declarations (SME6360), reported at each of
/// them; generated declarations do not count (<see cref="PartialDeclarations"/>). The asynchronous cleanup enforces
/// this rule with its code fix, which merges the parts (tools/cleanup/README.md), so it has an analyzer of its own: the
/// build skips an analyzer only when every rule it reports is off.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PartialSplitAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A type whose hand-written code is split across several partial declarations.</summary>
    public const string SplitId = "SME6360";

    /// <summary>Reports each hand-written declaration when a partial type has multiple authored halves.</summary>
    private static readonly DiagnosticDescriptor SplitRule = new(
        SplitId,
        "Hand-written type split across partial declarations",
        "{0} has {1} hand-written partial declarations; a type is one declaration, with partial only for a generated half",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Split responsibilities into owned types instead of spreading one type over several files (#621).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [SplitRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        // Generated declarations are part of the decision, so generated code is analyzed too; diagnostics are only
        // reported at hand-written declarations.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <summary>Reports every hand-written declaration of a type that has more than one.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (PartialDeclarations.Of(type, context.CancellationToken) is not { HandWritten.Count: > 1 } declarations)
            return;
        string name = type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        foreach (var declaration in declarations.HandWritten)
            context.ReportDiagnostic(Diagnostic.Create(SplitRule, declaration.Identifier.GetLocation(), name, declarations.HandWritten.Count));
    }
}
