using System.Diagnostics;

/// <summary>
/// Runs this executable's failing child fixture and confirms the console boundary reports it: a
/// nonzero exit with the exception on standard error, after verifying dialogs are disabled.
/// </summary>
internal static class ConsoleStartupBoundaryTests
{
    /// <summary>Command-line argument that makes the child process throw after checking the no-dialog policy.</summary>
    public const string FailingChildFlag = "--console-startup-failure-child";
    /// <summary>Exception text required on the child's standard error to prove the process boundary reported the failure.</summary>
    public const string ExpectedFailure = "Expected console startup-boundary fixture failure.";

    /// <summary>Starts the failing child fixture and requires a nonzero exit with its exception reported on standard error.</summary>
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
