using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.SourceLayout;

/// <summary>
/// #621: a type is one hand-written declaration. <c>partial</c> exists only for a generated boundary: one
/// hand-written declaration plus generated declarations, whether a source generator emits them (LibraryImport,
/// GeneratedRegex, ...) or a checked-in generator does. Generated declarations are recognized structurally, by
/// the same conventions the compiler uses: an <c>&lt;auto-generated&gt;</c> header comment, or a
/// <c>.g.cs</c>, <c>.generated.cs</c> or <c>.designer.cs</c> file name. This analyzer reports a lone partial
/// declaration with nothing generated beside it (SME6361); a type split across several hand-written declarations
/// (SME6360) is <see cref="PartialSplitAnalyzer"/>'s, which the asynchronous cleanup enforces.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PartialTypeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A partial type with no generated declaration to justify the modifier.</summary>
    public const string UngeneratedId = "SME6361";

    /// <summary>Reports a lone partial declaration with no generated counterpart.</summary>
    private static readonly DiagnosticDescriptor UngeneratedRule = new(
        UngeneratedId,
        "Partial type without a generated declaration",
        "{0} is partial but no generated declaration completes it; remove partial",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "partial is reserved for a type that a source generator or a checked-in generator completes (#621).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [UngeneratedRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        // Generated declarations are part of the decision, so generated code is analyzed too; the diagnostic is only
        // reported at a hand-written declaration.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeType, SymbolKind.NamedType);
    }

    /// <summary>Reports a partial type whose only declaration is hand-written.</summary>
    private static void AnalyzeType(SymbolAnalysisContext context)
    {
        var type = (INamedTypeSymbol)context.Symbol;
        if (PartialDeclarations.Of(type, context.CancellationToken) is not { HandWritten.Count: 1 } declarations ||
            declarations.All.Count != 1)
            return;
        string name = type.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        context.ReportDiagnostic(Diagnostic.Create(UngeneratedRule, declarations.HandWritten[0].Identifier.GetLocation(), name));
    }
}
