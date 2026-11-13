using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Removal;

/// <summary>
/// Refuses a rename (<see cref="PromotedNames"/>) whose new name joins another member's in a pair or family that a rule
/// reports. These rules read members' names as they are written: SME6280-SME6283 pair an x with a y, a width with a
/// height; SME6351-SME6353 a whole value with its fraction; SME6415-SME6417 number a family. Losing its underscore can
/// make a renamed <c>xOffset</c> the partner of a <c>yOffset</c> already there. The check runs those rules' own analyzers,
/// loaded from the repository analyzer assemblies each project's build runs, on each owner's file as the relocation will
/// write it, and refuses the renames declared in a file where they report an error they do not report on it now. Whether
/// a rule is an error in a file is read from the file's configuration as it is before the relocation. A project whose
/// build runs none of these analyzers cannot break their rules.
/// </summary>
internal static class RenamePairing
{
    /// <summary>The rules that read members' names as pairs or families.</summary>
    private static readonly HashSet<string> PairingRules =
        ["SME6280", "SME6281", "SME6282", "SME6283", "SME6351", "SME6352", "SME6353", "SME6415", "SME6416", "SME6417"];

    /// <summary>The prefix of the repository's own assemblies, whose analyzers carry the SME rules.</summary>
    private const string RepositoryAssemblyPrefix = "SuperMetroid.";

    /// <summary>Why each renamed member whose new name would join a reported pair or family cannot be renamed.</summary>
    /// <param name="renames">The names the renamed members take.</param>
    /// <param name="renamed">The renamed members.</param>
    /// <param name="collected">Every edit the relocation will apply, the declarations' renames among them.</param>
    /// <param name="moves">The declarations that move, whose removal is part of each file as it will be written.</param>
    /// <param name="solution">The loaded solution, whose projects name their analyzers.</param>
    /// <param name="identity">Says which trees are repository source.</param>
    /// <exception cref="InvalidOperationException">A project's repository analyzer is not built, or a tree belongs to no project.</exception>
    public static async Task<Dictionary<ISymbol, string>> RefusalsAsync(RenameMap renames, IReadOnlyCollection<ISymbol> renamed,
        ReferenceEdits collected, RelocationMoves moves, LoadedSolution solution, SymbolIdentity identity)
    {
        var refusals = new Dictionary<ISymbol, string>(SymbolEqualityComparer.Default);
        var analyzersByPath = new Dictionary<string, ImmutableArray<DiagnosticAnalyzer>>(StringComparer.OrdinalIgnoreCase);
        var declaringTrees = renamed
            .SelectMany(symbol => symbol.ContainingType.DeclaringSyntaxReferences.Select(reference => reference.SyntaxTree)
                .Where(identity.IsRepositorySource).Select(tree => new RenamedIn(tree, symbol)))
            .GroupBy(entry => entry.Tree, entry => entry.Symbol);
        foreach (var declared in declaringTrees)
        {
            var tree = declared.Key;
            var compiling = solution.Projects.Where(candidate => candidate.Compilation.ContainsSyntaxTree(tree)).ToList();
            if (compiling.Count == 0)
                throw new InvalidOperationException($"No loaded project compiles {tree.FilePath}.");
            var project = compiling[0];
            var analyzers = PairingAnalyzers(project.Project, analyzersByPath);
            if (analyzers.IsEmpty)
                continue;
            var edits = (collected.ByTree.TryGetValue(tree, out var list) ? list : [])
                .Concat(moves.All.Where(move => move.Tree == tree).Select(move => new TextEdit(move.Node.FullSpan, "")));
            var written = tree.WithChangedText(SourceText.From(
                RelocationOutput.InPlace(tree, edits, moves.MovedSpans.TryGetValue(tree, out var spans) ? spans : []), tree.GetText().Encoding));
            var before = (await ErrorsAsync(project.Compilation, tree, tree, analyzers)).Select(Key).ToHashSet(StringComparer.Ordinal);
            var added = (await ErrorsAsync(project.Compilation.ReplaceSyntaxTree(tree, written), written, tree, analyzers))
                .FirstOrDefault(diagnostic => !before.Contains(Key(diagnostic)));
            if (added is null)
                continue;
            foreach (var symbol in declared)
                refusals.TryAdd(symbol, $"{symbol.ToDisplayString()}, which must lose its underscore to become internal (SME6354), but " +
                    $"as '{renames.NewName(symbol)}' it would make {added.Id} report: {added.GetMessage()}");
        }
        return refusals;
    }

    /// <summary>A renamed member and a tree its owner is declared in.</summary>
    /// <param name="Tree">The tree.</param>
    /// <param name="Symbol">The renamed member.</param>
    private sealed record RenamedIn(SyntaxTree Tree, ISymbol Symbol);

    /// <summary>
    /// The pairing rules' errors in <paramref name="tree"/> of <paramref name="compilation"/>, each an error when the file's
    /// configuration, read from <paramref name="configured"/>, makes it one.
    /// </summary>
    private static async Task<IReadOnlyList<Diagnostic>> ErrorsAsync(Compilation compilation, SyntaxTree tree, SyntaxTree configured,
        ImmutableArray<DiagnosticAnalyzer> analyzers)
    {
        var withAnalyzers = compilation.WithAnalyzers(analyzers, new AnalyzerOptions([]));
        var reported = (await withAnalyzers.GetAnalyzerSyntaxDiagnosticsAsync(tree, CancellationToken.None))
            .Concat(await withAnalyzers.GetAnalyzerSemanticDiagnosticsAsync(compilation.GetSemanticModel(tree), null, CancellationToken.None));
        var options = compilation.Options.SyntaxTreeOptionsProvider;
        return [.. reported.Where(diagnostic => PairingRules.Contains(diagnostic.Id) && Severity(options, configured, diagnostic.Id) == ReportDiagnostic.Error)];
    }

    /// <summary>A rule's severity in a file, as its configuration sets it, or <see cref="ReportDiagnostic.Default"/>.</summary>
    private static ReportDiagnostic Severity(SyntaxTreeOptionsProvider? options, SyntaxTree tree, string id) =>
        options is null ? ReportDiagnostic.Default
            : options.TryGetDiagnosticValue(tree, id, CancellationToken.None, out var severity) ? severity
            : options.TryGetGlobalDiagnosticValue(id, CancellationToken.None, out var global) ? global
            : ReportDiagnostic.Default;

    /// <summary>A diagnostic as it is compared before and after: its rule and message.</summary>
    private static string Key(Diagnostic diagnostic) => $"{diagnostic.Id}: {diagnostic.GetMessage()}";

    /// <summary>The analyzers of the pairing rules among the repository analyzers a project's build runs.</summary>
    /// <exception cref="InvalidOperationException">A repository analyzer the project names is not built or does not resolve.</exception>
    private static ImmutableArray<DiagnosticAnalyzer> PairingAnalyzers(Project project, Dictionary<string, ImmutableArray<DiagnosticAnalyzer>> loaded)
    {
        var analyzers = ImmutableArray.CreateBuilder<DiagnosticAnalyzer>();
        foreach (var reference in project.AnalyzerReferences)
        {
            if (reference is not AnalyzerFileReference file)
                throw new InvalidOperationException($"{project.Name} names the analyzer {reference.Display}, which does not resolve to a file.");
            if (!Path.GetFileName(file.FullPath).StartsWith(RepositoryAssemblyPrefix, StringComparison.Ordinal))
                continue;
            if (!File.Exists(file.FullPath))
                throw new InvalidOperationException($"{project.Name}'s analyzer {file.FullPath} is not built, so the renames cannot be checked against its rules.");
            if (!loaded.TryGetValue(file.FullPath, out var fromFile))
                loaded[file.FullPath] = fromFile = Load(file.FullPath);
            analyzers.AddRange(fromFile);
        }
        return analyzers.ToImmutable();
    }

    /// <summary>The pairing rules' analyzers in one analyzer assembly.</summary>
    /// <exception cref="InvalidOperationException">The assembly or one of its analyzers does not load.</exception>
    private static ImmutableArray<DiagnosticAnalyzer> Load(string path)
    {
        var reference = new AnalyzerFileReference(path, AssemblyLoader.Instance);
        var failures = new List<string>();
        reference.AnalyzerLoadFailed += (_, failure) => failures.Add(failure.Message);
        var analyzers = reference.GetAnalyzers(LanguageNames.CSharp);
        if (failures.Count > 0)
            throw new InvalidOperationException($"The analyzers of {path} do not load:\n{string.Join('\n', failures)}");
        return [.. analyzers.Where(analyzer => analyzer.SupportedDiagnostics.Any(rule => PairingRules.Contains(rule.Id)))];
    }

    /// <summary>Loads analyzer assemblies into this process, beside the compiler it already holds.</summary>
    private sealed class AssemblyLoader : IAnalyzerAssemblyLoader
    {
        /// <summary>The one loader.</summary>
        public static AssemblyLoader Instance { get; } = new();

        /// <inheritdoc/>
        /// <remarks>
        /// Nothing to record: a repository analyzer depends only on the compiler and the shared framework, which this
        /// process resolves already.
        /// </remarks>
        public void AddDependencyLocation(string fullPath)
        {
        }

        /// <inheritdoc/>
        public Assembly LoadFromPath(string fullPath) => Assembly.LoadFrom(fullPath);
    }
}
