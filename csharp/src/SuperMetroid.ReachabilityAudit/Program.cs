using SuperMetroid.ReachabilityAudit;

// Whole-solution reachability audit. Reports every repository symbol that no entry point
// reaches, regardless of accessibility: nothing outside this repository calls this code.
NativeConsoleErrors.DisableDialogs();
try
{
    if (args is not ["--solution", var solutionPath, "--output", var outputDirectory])
        throw new ArgumentException("Usage: SuperMetroid.ReachabilityAudit --solution <SuperMetroid.Full.slnx> --output <directory>");
    var solution = await SolutionLoader.LoadAsync(Path.GetFullPath(solutionPath));
    var result = ReachabilityAnalysis.Run(solution);
    ReachabilityReport.Write(result, Path.GetFullPath(outputDirectory));
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
