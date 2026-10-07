using SuperMetroid.ReachabilityAudit;

// Whole-solution reachability audit. Reports every repository symbol that no entry point
// reaches, regardless of accessibility: nothing outside this repository calls this code.
// --remove deletes the declarations of the named finding categories.
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
        default:
            throw new ArgumentException("Usage: SuperMetroid.ReachabilityAudit --solution <SuperMetroid.Full.slnx> "
                + "(--output <directory> | --remove <category,...> --retired-fields <file> [--path-prefix <repo-relative path>])");
    }
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
