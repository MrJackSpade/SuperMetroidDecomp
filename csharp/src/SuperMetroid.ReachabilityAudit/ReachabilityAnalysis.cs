namespace SuperMetroid.ReachabilityAudit;

/// <summary>Everything the report needs: declarations, both reachability sets and the scans.</summary>
/// <param name="Declarations">All repository declarations compiled outside verification projects.</param>
/// <param name="Reachable">Symbols reachable from any executable or tool root.</param>
/// <param name="ProductionReachable">Symbols reachable from shipped hosts or the build-time analyzer.</param>
/// <param name="Referenced">Repository symbols named by a source reference, independently of reachability.</param>
/// <param name="SerializerAccessed">Members read by supported serializer calls.</param>
/// <param name="Reflection">Reflection sites and names collected for the report.</param>
/// <param name="VerificationSourceFiles">Repository files compiled into verification projects.</param>
/// <param name="Graph">Reference and implicit-dispatch edges used to compute reachability.</param>
/// <param name="ProductionRoots">Entry points and marked roots for production assemblies.</param>
/// <param name="Roots">Entry points and marked roots for all analyzed assemblies.</param>
internal sealed record ReachabilityResult(
    IReadOnlyDictionary<string, Declaration> Declarations,
    HashSet<string> Reachable,
    HashSet<string> ProductionReachable,
    HashSet<string> Referenced,
    HashSet<string> SerializerAccessed,
    ReflectionScan Reflection,
    HashSet<string> VerificationSourceFiles,
    ReachabilityGraph Graph,
    IReadOnlySet<string> ProductionRoots,
    IReadOnlySet<string> Roots);

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
    private static readonly HashSet<string> VerificationProjects =
        ["SuperMetroid.Verification", "SuperMetroid.IntegrationVerification", "SuperMetroid.RenderVerification",
         "SuperMetroid.DesktopVerification", "SuperMetroid.EnsureVerification"];

    /// <summary>Builds declaration and reference data, then computes production and all-tool reachability.</summary>
    public static ReachabilityResult Run(LoadedSolution solution)
    {
        var identity = new SymbolIdentity(solution.RepositoryRoot);
        var graph = new ReachabilityGraph();
        var declarations = new DeclarationCollector(identity, graph);
        var roots = new RootCollector(identity, graph);
        var json = new JsonSerializationScan(identity);
        var reflection = new ReflectionScan(identity, graph);
        var references = new ReferenceCollector(identity, graph, roots, json, reflection);
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
