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
/// --relocate moves shipped symbols that only tools reach into a development-only project.
/// --prune-keeping-verification-used deletes dead code, first moving what tests still use into test-support.
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
                case ["--solution", var solutionPath, "--relocate", var category, "--to", var targetDirectory]:
                {
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    SymbolRelocator.Relocate(solution, ReachabilityAnalysis.Run(solution), category,
                        Path.GetFullPath(targetDirectory), ReachabilityFindings.ProductionAssemblies);
                    return 0;
                }
                case ["--solution", var solutionPath, "--prune-keeping-verification-used", var categoryList, "--path-prefix", var prefix,
                    "--to", var targetDirectory, "--retired-fields", var retiredFields]:
                {
                    // Deletes the dead code tests do not use, then moves what they still use, with everything it uses,
                    // into test-support. Removing first leaves no dead shipped code naming members the move takes away.
                    var categories = categoryList.Split(',').ToHashSet();
                    string[] prefixes = prefix.Split(',');
                    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    var result = ReachabilityAnalysis.Run(solution);
                    var unused = VerificationReferences.DeadFindings(result, categories, prefixes);
                    unused.ExceptWith(VerificationReferences.SelectUsedDeadCode(result, VerificationReferences.Collect(solution), categories, prefixes));
                    SymbolRemover.Remove(solution, unused, Path.GetFullPath(retiredFields));
                    solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
                    result = ReachabilityAnalysis.Run(solution);
                    SymbolRelocator.Relocate(solution,
                        VerificationReferences.SelectUsedDeadCode(result, VerificationReferences.Collect(solution), categories, prefixes),
                        Path.GetFullPath(targetDirectory), ReachabilityFindings.ProductionAssemblies, RelocationStyle.VerificationAccess);
                    return 0;
                }
                default:
                    throw new ArgumentException("Usage: SuperMetroid.ReachabilityAudit --solution <SuperMetroid.Full.slnx> "
                        + "(--output <directory> | --remove <category,...> --retired-fields <file> [--path-prefix <repo-relative path>] "
                        + "| --relocate <category> --to <project directory> | --why <symbol text> "
                        + "| --prune-keeping-verification-used <category,...> --path-prefix <repo-relative path,...> --to <test-support directory> --retired-fields <file> "
                        + "| --remove-keys <file> --retired-fields <file> | --check --output <directory>)");
            }
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
