using System.Diagnostics;

namespace SuperMetroid.TestSupport.RunTimeLimit;

/// <summary>
/// Fails a verification process whose run exceeds the owner's limit (2026-11-02): thirty minutes. Every verification
/// executable starts it first thing in <c>Main</c>. When the limit passes, the process writes what was running and how
/// long it ran to standard error and exits with a failing code. It never leaves an unhandled exception to Windows, and
/// never waits for the run to finish.
/// </summary>
internal static class VerificationRunTimeLimit
{
    /// <summary>The longest a verification run may take, set by the owner.</summary>
    internal static readonly TimeSpan Limit = TimeSpan.FromMinutes(30);

    /// <summary>The exit code of a run the limit ended.</summary>
    internal const int ExceededExitCode = 124;

    /// <summary>The run's clock, started with the limit.</summary>
    private static readonly Stopwatch elapsed = new();

    /// <summary>The timer that ends the run, kept alive for the process's lifetime.</summary>
    private static System.Threading.Timer? timer;

    /// <summary>What the run is doing now, named in the failure: a suite, mode or command.</summary>
    internal static string Activity { get; set; } = "startup";

    /// <summary>Starts the limit for this process; a second call is an error.</summary>
    /// <param name="run">The executable and its arguments, named in the failure.</param>
    /// <exception cref="InvalidOperationException">The limit was already started.</exception>
    internal static void Start(string run)
    {
        if (timer is not null)
            throw new InvalidOperationException("The verification run limit is already running.");
        elapsed.Start();
        timer = new System.Threading.Timer(_ => Exceed(run), null, Limit, Timeout.InfiniteTimeSpan);
    }

    /// <summary>Reports the run that passed the limit and ends the process with a failing code.</summary>
    private static void Exceed(string run)
    {
        Console.Error.WriteLine(
            $"FAIL run time limit: '{run}' ran {elapsed.Elapsed.TotalMinutes:0.0} min, past the {Limit.TotalMinutes:0}-minute limit, " +
            $"while running {Activity}. The run is ended; reduce the work the check does.");
        Console.Error.Flush();
        Console.Out.Flush();
        Environment.Exit(ExceededExitCode);
    }
}
