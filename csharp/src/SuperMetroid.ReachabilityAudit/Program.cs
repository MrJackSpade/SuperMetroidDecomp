using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using SuperMetroid.ReachabilityAudit.SolutionLoading;
using SuperMetroid.TestSupport.WindowsConsole;
using SuperMetroid.ReachabilityAudit.Analysis;
using SuperMetroid.ReachabilityAudit.Removal;
using SuperMetroid.ReachabilityAudit.Reporting;

namespace SuperMetroid.ReachabilityAudit;

/// <summary>
/// Whole-solution reachability audit. Reports every repository symbol that no entry point
/// reaches, regardless of accessibility: nothing outside this repository calls this code.
/// </summary>
/// <remarks>
/// --remove deletes the declarations of the named finding categories.
/// --prune-keeping-verification-used deletes dead code, moving the dead top-level types tests still use into test-support.
/// --check is the CI gate: it fails on any finding.
/// </remarks>
internal static class Program
{
    /// <summary>Dispatches whole-solution reachability reports, checks, and
    /// explicit source cleanup commands through a guarded console entry point.</summary>
    private static async Task<int> Main(string[] args)
    {
        NativeConsoleErrors.DisableDialogs();
        try
        {
            switch (args)
            {
                case ["--solution", var solutionPath, "--output", var outputDirectory]:
                {
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    ReachabilityReport.Write(ReachabilityAnalysis.Run(solution), Path.GetFullPath(outputDirectory));
                    return 0;
                }
                case ["--solution", var solutionPath, "--remove", var categories, "--retired-fields", var retiredFields, .. var rest]
                    when rest is [] or ["--path-prefix", _]:
                {
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    SymbolRemover.Remove(solution, ReachabilityAnalysis.Run(solution), categories.Split(',').ToHashSet(),
                        rest is [_, var prefix] ? prefix : null, Path.GetFullPath(retiredFields));
                    return 0;
                }
                case ["--solution", var solutionPath, "--check", "--output", var outputDirectory]:
                {
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    return ReachabilityReport.Check(ReachabilityAnalysis.Run(solution), Path.GetFullPath(outputDirectory));
                }
                case ["--solution", var solutionPath, "--core-import-only", var outputFile]:
                {
                    // Lists the Core declarations the game never reads: reached only through the import pipeline or tools.
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    return CoreImportOnlyReport.Write(ReachabilityAnalysis.Run(solution), Path.GetFullPath(outputFile));
                }
                case ["--solution", var solutionPath, "--remove-keys", var keysFile, "--retired-fields", var retiredFields]:
                {
                    // Deletes the declarations whose symbol keys (one per line, as reports print them) are listed.
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    SymbolRemover.Remove(solution, File.ReadAllLines(keysFile).Where(l => l.Length > 0).ToHashSet(StringComparer.Ordinal),
                        Path.GetFullPath(retiredFields));
                    return 0;
                }
                case ["--solution", var solutionPath, "--why", var symbol]:
                {
                    // Prints the shortest production reference chain to each declaration whose key contains the text.
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    var result = ReachabilityAnalysis.Run(solution);
                    string[] patterns = symbol.Split('|');
                    foreach (string key in result.Declarations.Keys.Where(k => patterns.Any(p => k.Contains(p, StringComparison.Ordinal))).Order(StringComparer.Ordinal))
                    {
                        Console.WriteLine(key);
                        foreach (string step in result.Graph.PathTo(result.ProductionRoots, key) ?? ["(not reachable from production)"])
                            Console.WriteLine("  " + step);
                        if (!result.ProductionReachable.Contains(key))
                            foreach (string step in result.Graph.PathTo(result.Roots, key) ?? ["(not reachable from any root)"])
                                Console.WriteLine("  tool: " + step);
                    }
                    return 0;
                }
                case ["--solution", var solutionPath, "--prune-keeping-verification-used", var categoryList, "--path-prefix", var prefix,
                    "--to", var targetDirectory, "--retired-fields", var retiredFields]:
                {
                    // Deletes the dead code, then moves the dead top-level types tests still use into test-support.
                    // The solution is opened from the files again each time the prune asks for it.
                    var workspaces = new List<MSBuildWorkspace>();
                    try
                    {
                        await PruneKeepingVerificationUsedAsync(async () =>
                            {
                                var workspace = await SolutionLoader.OpenAsync(Path.GetFullPath(solutionPath));
                                workspaces.Add(workspace);
                                return workspace.CurrentSolution;
                            }, SolutionLoader.RepositoryRoot(solutionPath), categoryList, prefix, targetDirectory, retiredFields);
                    }
                    finally
                    {
                        foreach (var workspace in workspaces)
                            workspace.Dispose();
                    }
                    return 0;
                }
                default:
                    throw new ArgumentException("Usage: SuperMetroid.ReachabilityAudit --solution <SuperMetroid.Full.slnx> "
                        + "(--output <directory> | --remove <category,...> --retired-fields <file> [--path-prefix <repo-relative path>] "
                        + "| --why <symbol text> "
                        + "| --prune-keeping-verification-used <category,...> --path-prefix <repo-relative path,...> --to <test-support directory> --retired-fields <file> "
                        + "| --remove-keys <file> --retired-fields <file> | --core-import-only <file> | --check --output <directory>)");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    /// <summary>
    /// The dead-code prune of the asynchronous cleanup (tools/cleanup/README.md): <paramref name="current"/> returns the
    /// solution as its files are, and is asked again after the removal has written its files. The command line opens the
    /// solution from the files each time; the cleanup, which loads the solution once for all its steps, hands over the
    /// solution with every file its steps wrote. Every load accepts compile errors, which the prune reads below.
    /// </summary>
    internal static Task PruneKeepingVerificationUsedAsync(Func<Task<Solution>> current, string repositoryRoot, string categoryList,
        string prefix, string targetDirectory, string retiredFields) =>
        PruneKeepingVerificationUsedAsync(async () => await SolutionLoader.CompileAsync(await current(), repositoryRoot, mayFail: _ => true),
            categoryList, prefix, targetDirectory, retiredFields);

    /// <summary>
    /// Deletes the dead code, every dead member of a production type that only tests use among it, then moves the dead
    /// top-level types tests still use into test-support (<see cref="VerificationReferences.SelectRelocatedTypes"/>). A check
    /// or a moved type that read a removed member no longer builds, and is rewritten to drive real game code (owner ruling,
    /// #2599). Removing first leaves no dead shipped code naming the types the move takes away.
    /// <para>
    /// What is dead is read from code that compiles: a reference that does not bind reaches nothing, so on a tree with
    /// compile errors live code would read as dead and be deleted. When a project does not compile, which a fix or an
    /// earlier prune of the same cleanup run leaves where the developer has a choice to make (#2599), the prune says so and
    /// changes nothing; the next run prunes once the tree builds again. The move after the removal reads the tree the
    /// removal left, whose checks may no longer compile: it binds only the moved types' declarations, documentation and
    /// base lists, and a reference among them that does not bind is left as written, which fails the build there.
    /// </para>
    /// </summary>
    private static async Task PruneKeepingVerificationUsedAsync(Func<Task<LoadedSolution>> load, string categoryList, string prefix,
        string targetDirectory, string retiredFields)
    {
        var categories = categoryList.Split(',').ToHashSet();
        string[] prefixes = prefix.Split(',');
        var watch = System.Diagnostics.Stopwatch.StartNew();
        void Timed(string phase)
        {
            Console.WriteLine($"Prune {phase} took {watch.Elapsed.TotalSeconds:F0} s.");
            watch.Restart();
        }
        var solution = await load();
        Timed("compilation");
        if (SolutionLoader.ProjectsWithErrors(solution) is [_, ..] broken)
        {
            File.WriteAllText(Path.GetFullPath(retiredFields), "");
            Console.WriteLine($"Pruned nothing: {string.Join(", ", broken)} do not compile, and what is dead cannot be read from " +
                "code that does not compile; the next run prunes once they build.");
            return;
        }
        // What verification names and what reaches what are read from the same compilations, independently: the
        // verification projects' references are collected while the reachability analysis runs.
        var referenced = Task.Run(() => VerificationReferences.Collect(solution));
        var result = ReachabilityAnalysis.Run(solution);
        var dead = VerificationReferences.DeadFindings(result, categories, prefixes);
        var relocated = VerificationReferences.SelectRelocatedTypes(result, await referenced, categories, prefixes);
        // What a moved type declares moves with it.
        dead.ExceptWith([.. dead.Where(key => VerificationReferences.DeadTopLevelType(result, dead, key) is { } type && relocated.Contains(type))]);
        Timed("reachability analysis and verification references");
        SymbolRemover.Remove(solution, dead, Path.GetFullPath(retiredFields));
        Timed("removal");
        if (relocated.Count == 0)
            return;
        solution = await load();
        Timed("compilation after the removal");
        SymbolRelocator.Relocate(solution, relocated, Path.GetFullPath(targetDirectory), ReachabilityFindings.ProductionAssemblies);
        Timed("relocation");
    }
}
