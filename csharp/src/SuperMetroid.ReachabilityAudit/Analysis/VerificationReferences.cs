using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// The dead declarations verification still names. Reachability ignores tests, so code only a test
/// uses is dead; the dead top-level types tests use move into test-support with every such type they use, and every
/// other dead declaration is removed, a member of a production type that only tests use among them.
/// </summary>
internal static class VerificationReferences
{
    /// <summary>Keys of every repository symbol that verification source (test-support included) names.</summary>
    public static HashSet<string> Collect(LoadedSolution solution)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var verification = solution.Projects.Where(p => ReachabilityAnalysis.VerificationProjects.Contains(p.Project.Name)).ToList();
        // The walk keeps the first project's view of a shared file; binding those trees on every processor first leaves
        // it only the bound answers to read.
        var bindingSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        SemanticModels.Bind(verification.SelectMany(p => p.Compilation.SyntaxTrees
            .Where(t => identity.IsRepositorySource(t) && bindingSeen.Add(Path.GetFullPath(t.FilePath)))
            .Select(tree => (p.Compilation, tree))));
        foreach (var (_, compilation) in verification)
            foreach (var tree in compilation.SyntaxTrees.Where(t => identity.IsRepositorySource(t) && visited.Add(Path.GetFullPath(t.FilePath))))
            {
                foreach (var node in tree.GetRoot().DescendantNodes().Where(n => n is ExpressionSyntax or AttributeSyntax))
                {
                    var info = SemanticModels.At(compilation, tree, node).GetSymbolInfo(node);
                    foreach (var symbol in info.CandidateSymbols.Prepend(info.Symbol))
                        if (identity.Key(SymbolIdentity.Normalize(symbol)) is { } key)
                            keys.Add(key);
                }
            }
        return keys;
    }

    /// <summary>Keys of the findings of the given categories declared under one of <paramref name="filePrefixes"/>.</summary>
    public static HashSet<string> DeadFindings(ReachabilityResult result, IReadOnlySet<string> categories, IReadOnlyList<string> filePrefixes) =>
        ReachabilityFindings.Classify(result)
            .Where(f => categories.Contains(f.Category) &&
                filePrefixes.Any(prefix => f.Declaration.File.StartsWith(prefix, StringComparison.Ordinal)))
            .Select(f => f.Declaration.Key).ToHashSet(StringComparer.Ordinal);

    /// <summary>
    /// The dead top-level types of the given categories, declared under one of <paramref name="filePrefixes"/>, that
    /// verification uses directly or through other such types: check-only helper code, which moves whole into test-support.
    /// A type's members move with it, so what they use is selected too, to a fixed point. A dead member of a production type
    /// is never selected, whatever uses it: nothing in production exists for checks alone, so it is removed, and the checks
    /// and moved code reading it fail to build until they drive real game code instead (owner ruling, #2599).
    /// </summary>
    public static HashSet<string> SelectRelocatedTypes(ReachabilityResult result, HashSet<string> verificationReferenced,
        IReadOnlySet<string> categories, IReadOnlyList<string> filePrefixes)
    {
        var dead = DeadFindings(result, categories, filePrefixes);
        var selected = verificationReferenced.Select(key => DeadTopLevelType(result, dead, key)).OfType<string>().ToHashSet(StringComparer.Ordinal);
        while (true)
        {
            var moving = result.Declarations.Keys.Where(key => DeadTopLevelType(result, dead, key) is { } type && selected.Contains(type)).ToList();
            int before = selected.Count;
            foreach (string key in result.Graph.Reach(moving))
                if (DeadTopLevelType(result, dead, key) is { } type)
                    selected.Add(type);
            if (selected.Count == before)
                return selected;
        }
    }

    /// <summary>
    /// The dead top-level type a key belongs to: the outermost type declaring it, when that type is itself a dead finding;
    /// or null for a key of a staying type, a member of one among them.
    /// </summary>
    public static string? DeadTopLevelType(ReachabilityResult result, IReadOnlySet<string> dead, string key)
    {
        string current = key;
        while (result.Declarations.TryGetValue(current, out var declaration) && declaration.ContainingTypeKey.Length > 0)
            current = declaration.ContainingTypeKey;
        return dead.Contains(current) && result.Declarations.TryGetValue(current, out var outermost) && outermost.Shape == DeclarationShape.Type
            ? current : null;
    }
}
