using static SuperMetroid.Verification.Harness.SuiteSchedulingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
namespace SuperMetroid.Verification.Harness;

/// <summary>Runs verification suites and commands behind a no-dialog console error boundary.</summary>
internal static class Program
{
/// <summary>Installs the Windows process policy and reports suite or command failures through stderr and exit status.</summary>
private static int Main(string[] args)
{
// This is deliberately a plain console executable rather than an xUnit/MSTest project.
// It keeps the reverse-engineering workspace dependency-free and makes every check easy
// to step through in Visual Studio. A failed check throws immediately with concrete state;
// its outermost suite records it and the run continues with the next suite.
// Windows otherwise turns an unhandled CLR assertion into a modal "unknown software
// exception" dialog. That is actively hostile to an automated verifier: the useful stack
// trace belongs in this console and a dialog must never steal focus or stall the process.
if (OperatingSystem.IsWindows())
    _ = NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
    int exit = RunFlags(args);
    if (failedSuites.Count == 0)
        return exit;
    Console.Error.WriteLine($"{failedSuites.Count} suite(s) failed: {string.Join(", ", failedSuites)}");
    return 1;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception);
    return 1;
}
}

/// <summary>Dispatches one verification command, several argument-free commands, or the full suite, and prints elapsed time and peak memory.</summary>
private static int RunFlags(string[] args)
{
var flagWatch = System.Diagnostics.Stopwatch.StartNew();
try
{
if (args.Length == 0)
    return RunFullSuite();
if (VerificationCommands.TryDispatch(args, out int exitCode))
    return exitCode;
// Several commands, each a flag and its values (such as --suite VerifyName), run in one process so focused
// checks share one startup.
if (args.Length > 1 && args[0].StartsWith("--", StringComparison.Ordinal))
{
    var commands = new List<List<string>>();
    foreach (string argument in args)
    {
        if (argument.StartsWith("--", StringComparison.Ordinal))
            commands.Add([argument]);
        else
            commands[^1].Add(argument);
    }
    int combined = 0;
    foreach (List<string> command in commands)
    {
        if (!VerificationCommands.TryDispatch([.. command], out int commandExit))
            throw new ArgumentException(VerificationCommands.UsageFor([.. command]), nameof(args));
        combined = Math.Max(combined, commandExit);
    }
    return combined;
}
throw new ArgumentException(VerificationCommands.UsageFor(args), nameof(args));
}
catch (Exception exception)
{
    // A failure outside any suite ends the flag here. Windows never receives an unhandled CLR exception that it can
    // turn into a focus-stealing dialog. ToString() retains the type, message, inner exception,
    // and complete stack trace in the terminal where the failure is actually actionable.
    Console.Error.WriteLine(exception);
    return 1;
}
finally
{
    flagWatch.Stop();
    Console.WriteLine($"TIME flag {(args.Length == 0 ? "(all)" : string.Join(' ', args))}: {flagWatch.Elapsed.TotalSeconds:0.00} s, {PeakMemoryReport()}");
}

}

}
