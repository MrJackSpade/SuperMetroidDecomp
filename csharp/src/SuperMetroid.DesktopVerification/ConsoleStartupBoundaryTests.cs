using System.Diagnostics;

/// <summary>
/// Runs this executable's failing child fixture and confirms the console boundary reports it: a
/// nonzero exit with the exception on standard error, after verifying dialogs are disabled.
/// </summary>
internal static class ConsoleStartupBoundaryTests
{
    public const string FailingChildFlag = "--console-startup-failure-child";
    public const string ExpectedFailure = "Expected console startup-boundary fixture failure.";

    public static void Run()
    {
        var start = new ProcessStartInfo("dotnet")
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        start.ArgumentList.Add(typeof(ConsoleStartupBoundaryTests).Assembly.Location);
        start.ArgumentList.Add(FailingChildFlag);
        using Process child = Process.Start(start) ?? throw new InvalidOperationException("Could not start the console-boundary child.");
        Task<string> output = child.StandardOutput.ReadToEndAsync();
        string error = child.StandardError.ReadToEnd();
        child.WaitForExit();
        _ = output.Result;
        if (child.ExitCode == 0 || !error.Contains(ExpectedFailure, StringComparison.Ordinal))
            throw new InvalidOperationException(
                $"Console startup failure must exit nonzero and report its exception; exit {child.ExitCode}, stderr: {error}");
        Console.WriteLine($"Console startup boundary: child failure exited {child.ExitCode} with its exception on standard error.");
    }
}
