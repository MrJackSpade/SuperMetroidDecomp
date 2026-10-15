using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>
/// Reports little-endian words rebuilt from adjacent byte reads of WRAM, SRAM or the cartridge (#223):
/// the canonical <c>Read…Word</c>/<c>Read…WordFixedBank</c> operations own the byte order and the bank
/// progression. Their implementations are exempt.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class WordReadAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A word rebuilt from two byte reads.</summary>
    public const string ReconstructedWordId = "SME6350";

    /// <summary>Warns only for supported source reads outside canonical word-read implementations.</summary>
    private static readonly DiagnosticDescriptor ReconstructedWordRule = new(
        ReconstructedWordId,
        "Read the word through its canonical operation",
        "{0}",
        "Design",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Little-endian words are read with SnesWordReads/CartridgeWordReads, which own the byte order and " +
            "state whether the high byte's address wraps within the bank.");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ReconstructedWordRule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.BitwiseOrExpression, SyntaxKind.AddExpression);
    }

    /// <summary>Reports the canonical method to use, or asks for an explicit bank-progression choice.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        for (INamedTypeSymbol? owner = context.ContainingSymbol?.ContainingType; owner is not null; owner = owner.ContainingType)
        {
            if (WordReadMatcher.CanonicalTypes.Contains(owner.ToDisplayString()))
                return;
        }
        if (WordReadMatcher.Match((BinaryExpressionSyntax)context.Node, context.SemanticModel) is not { } match)
            return;
        string message = match.CanonicalMethod is { } method
            ? $"Use {match.CanonicalReceiver}.{method}({match.Address}) instead of composing two {match.ByteMethod} calls"
            : match.Progression != WordReadProgression.Ambiguous
            ? $"Add the {match.Progression} {(match.Long ? "long" : "word")} operation for {match.ByteMethod} to the canonical " +
              "word reads and use it instead of composing bytes"
            : $"Use a canonical {match.ByteMethod.Replace("Byte", "Word")} operation; '{match.Address}' OR'd with an unwrapped " +
              "offset + 1 does not say whether the high byte stays in the bank, so choose Linear or FixedBank deliberately";
        context.ReportDiagnostic(Diagnostic.Create(ReconstructedWordRule, match.Expression.GetLocation(), message));
    }
}
