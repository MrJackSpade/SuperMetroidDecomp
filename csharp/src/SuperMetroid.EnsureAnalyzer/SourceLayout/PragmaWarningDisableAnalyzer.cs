using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SuperMetroid.EnsureAnalyzer.SourceLayout;

/// <summary>
/// #1396: <c>#pragma warning disable</c> turns a diagnostic off for a span of source, which hides a finding instead of
/// resolving it. Every disable is reported, whatever diagnostic it names and whatever reason follows it. The finding is
/// fixed in the code; the one sanctioned per-site exception is a <c>[SuppressMessage]</c> attribute with a real
/// justification, which names its rule and reason where the report can find it. <c>#pragma warning restore</c> on its
/// own changes nothing and is not reported. Generated sources in the repository are analyzed; files the .NET SDK or a
/// third-party generator writes are not repository source and are skipped.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PragmaWarningDisableAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A <c>#pragma warning disable</c> directive.</summary>
    public const string Id = "SME6414";

    /// <summary>Reports a warning-disable directive.</summary>
    private static readonly DiagnosticDescriptor Rule = new(
        Id,
        "Pragma warning disable",
        "'#pragma warning disable' hides a finding instead of resolving it; fix the code, or justify a false positive with [SuppressMessage] at that one site",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Diagnostics are resolved, never disabled in source; [SuppressMessage] with a justification is the one per-site form (#1396).");

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    /// <summary>Reports every warning-disable directive in the tree.</summary>
    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        if (OneTypePerFileAnalyzer.IsThirdPartyGenerated(context.Tree.FilePath) ||
            LooseCommentAnalyzer.IsSdkBuildOutput(context.Tree, context.CancellationToken))
            return;
        SyntaxNode root = context.Tree.GetRoot(context.CancellationToken);
        foreach (SyntaxTrivia trivia in root.DescendantTrivia(descendIntoTrivia: false))
        {
            if (trivia.GetStructure() is PragmaWarningDirectiveTriviaSyntax pragma && pragma.DisableOrRestoreKeyword.IsKind(SyntaxKind.DisableKeyword))
                context.ReportDiagnostic(Diagnostic.Create(Rule, trivia.GetLocation()));
        }
    }
}
