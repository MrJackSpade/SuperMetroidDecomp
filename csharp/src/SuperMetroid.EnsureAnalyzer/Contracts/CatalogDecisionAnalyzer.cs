using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SuperMetroid.EnsureAnalyzer.CartridgeEncoding;
using SuperMetroid.EnsureAnalyzer.Modules;

namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>
/// Catalog files hold data only (SME6451, #1452): in <c>SuperMetroid.Core</c>, a file the encoding scanner names as a
/// catalog (<see cref="CartridgeEncodingScanner.IsCatalogFile"/>) declares no decision and no procedure. Every
/// <c>if</c>, <c>switch</c> statement or expression, conditional <c>?:</c>, null-coalescing or null-conditional
/// operator, loop, <c>goto</c>, pattern or type test, non-constant comparison, query <c>where</c> clause and LINQ
/// predicate is reported, and so is every body (a method, constructor, accessor, operator, local function or lambda)
/// with statements beyond a single <c>return</c>. Data stays: initializers of literals, record and collection
/// construction, dictionary initializers, <c>nameof</c> and constant arithmetic. A catalog-named file generated from
/// data is authored data too and is analyzed; only build output is skipped.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CatalogDecisionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>A Core catalog file holds a decision or a procedure.</summary>
    public const string CatalogDecisionId = "SME6451";

    private static readonly DiagnosticDescriptor Rule = new(
        CatalogDecisionId,
        "Core catalog holds a decision",
        "The catalog '{0}' holds {1}; a catalog holds data only: move the decision to the domain type that owns the behaviour, or to the import pipeline when it decodes cartridge bytes",
        "Design",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Definition data lives in domain-named catalog types, and catalog files hold data only: no if, switch, " +
            "loops or decisions (AGENTS.md). A decision in a catalog hides behaviour behind a data name and exempts it from " +
            "the rules that check functional code. The data stays in the catalog as records, tables and dictionaries; the " +
            "decision moves to the domain type that owns the behaviour, named for the game, or into the import pipeline " +
            "when it is an encoding of cartridge bytes (#1452).");

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
            start.RegisterSyntaxNodeAction(Analyze, CatalogDecisions.Kinds);
        });
    }

    /// <summary>Whether <paramref name="tree"/> is an authored catalog file.</summary>
    internal static bool IsCatalog(SyntaxTree tree) =>
        CartridgeEncodingScanner.IsCatalogFile(tree.FilePath) && !CartridgeEncodingScanner.IsBuildGenerated(tree.FilePath);

    /// <summary>Reports a decision or procedure in a catalog file.</summary>
    private static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (!IsCatalog(context.Node.SyntaxTree) ||
            CatalogDecisions.Describe(context.Node, context.SemanticModel, context.CancellationToken) is not { } decision)
        {
            return;
        }
        Report(context, decision.Location, decision.What);
    }

    /// <summary>Reports one finding against the type that declares it, or against the file outside any type.</summary>
    private static void Report(SyntaxNodeAnalysisContext context, Location location, string what)
    {
        string owner = context.Node.FirstAncestorOrSelf<BaseTypeDeclarationSyntax>()?.Identifier.Text ??
            Path.GetFileName(context.Node.SyntaxTree.FilePath);
        context.ReportDiagnostic(Diagnostic.Create(Rule, location, owner, what));
    }
}
