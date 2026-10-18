using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;
using SuperMetroid.ReachabilityAudit.SolutionLoading;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// The dead declarations verification still names. Reachability ignores tests, so code only a test
/// uses is dead; before it is removed, what tests use moves into test-support with everything it uses.
/// </summary>
internal static class VerificationReferences
{
    /// <summary>Keys of every repository symbol that verification source (test-support included) names.</summary>
    public static HashSet<string> Collect(LoadedSolution solution)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, compilation) in solution.Projects.Where(p => ReachabilityAnalysis.VerificationProjects.Contains(p.Project.Name)))
            foreach (var tree in compilation.SyntaxTrees.Where(t => identity.IsRepositorySource(t) && visited.Add(Path.GetFullPath(t.FilePath))))
            {
                var model = compilation.GetSemanticModel(tree);
                foreach (var node in tree.GetRoot().DescendantNodes().Where(n => n is ExpressionSyntax or AttributeSyntax))
                {
                    var info = model.GetSymbolInfo(node);
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
    /// The findings of the given categories, declared under one of <paramref name="filePrefixes"/>, that verification
    /// uses directly or through other such findings. A selected type's members move with it, so
    /// what they use is selected too, to a fixed point.
    /// </summary>
    public static HashSet<string> SelectUsedDeadCode(ReachabilityResult result, HashSet<string> verificationReferenced,
        IReadOnlySet<string> categories, IReadOnlyList<string> filePrefixes)
    {
        var dead = DeadFindings(result, categories, filePrefixes);
        // The finding a key belongs to: the declaration itself, or the dead type reported for its members.
        string? DeadFinding(string key)
        {
            for (string? current = key; !string.IsNullOrEmpty(current);
                 current = result.Declarations.TryGetValue(current, out var declaration) ? declaration.ContainingTypeKey : null)
                if (dead.Contains(current))
                    return current;
            return null;
        }
        var selected = verificationReferenced.Select(DeadFinding).OfType<string>().ToHashSet(StringComparer.Ordinal);
        while (true)
        {
            var moving = result.Declarations.Keys.Where(key => DeadFinding(key) is { } finding && selected.Contains(finding)).ToList();
            int before = selected.Count;
            foreach (string key in result.Graph.Reach(moving))
                if (DeadFinding(key) is { } finding)
                    selected.Add(finding);
            if (selected.Count == before)
                return selected;
        }
    }
}
