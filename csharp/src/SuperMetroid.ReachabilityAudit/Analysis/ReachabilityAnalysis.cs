using Microsoft.CodeAnalysis;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.ReachabilityAudit.Declarations;
using SuperMetroid.ReachabilityAudit.Reporting;

namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>
/// Builds the reference graph for a loaded solution and computes reachability from its roots.
/// Verification projects are not users: a symbol only a test reaches is unused, so their code is
/// neither walked for references nor reported.
/// </summary>
internal static class ReachabilityAnalysis
{
    /// <summary>Shipped player hosts and the build-time analyzer: the roots of production reachability.</summary>
    private static readonly HashSet<string> ProductionHosts =
        ["SuperMetroid.Game", "SuperMetroid.Android", "SuperMetroid.EnsureAnalyzer"];

    /// <summary>Test executables; what only they reach is not used.</summary>
    internal static readonly HashSet<string> VerificationProjects =
        ["SuperMetroid.Verification", "SuperMetroid.IntegrationVerification", "SuperMetroid.RenderVerification",
         "SuperMetroid.DesktopVerification", "SuperMetroid.EnsureVerification"];

    /// <summary>Builds declarations and references from non-test projects, then computes all-root and production-only reachability.</summary>
    public static ReachabilityResult Run(LoadedSolution solution)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var graph = new ReachabilityGraph();
        var declarations = new DeclarationCollector(identity, graph);
        var roots = new RootCollector(identity, graph);
        var json = new JsonSerializationScan(identity);
        var reflection = new ReflectionScan(identity, graph);
        var treeOwners = new Dictionary<SyntaxTree, Compilation>();
        foreach (var (_, compilation) in solution.Projects)
            foreach (var tree in compilation.SyntaxTrees)
                treeOwners.TryAdd(tree, compilation);
        var references = new ReferenceCollector(identity, graph, roots, json, reflection, treeOwners);
        var analyzed = solution.Projects.Where(p => !VerificationProjects.Contains(p.Project.Name)).ToList();

        foreach (var (project, compilation) in analyzed)
            declarations.Collect(project, compilation);
        foreach (var (project, compilation) in analyzed)
        {
            roots.Collect(project, compilation, ProductionHosts.Contains(project.Name));
            references.Collect(project, compilation);
            Console.WriteLine($"Analyzed {project.Name}");
        }

        reflection.Resolve();
        // Shared test-support sources compiled into verification projects are test infrastructure.
        var verificationSources = solution.Projects.Where(p => VerificationProjects.Contains(p.Project.Name))
            .SelectMany(p => p.Compilation.SyntaxTrees).Where(identity.IsRepositorySource)
            .Select(tree => identity.Relative(tree.FilePath)).ToHashSet(StringComparer.Ordinal);
        return new ReachabilityResult(declarations.Declarations, graph.Reach(roots.Roots), graph.Reach(roots.ProductionRoots),
            references.Referenced, json.SerializerAccessedMembers(), reflection, verificationSources, graph, roots.ProductionRoots, roots.Roots);
    }
}
