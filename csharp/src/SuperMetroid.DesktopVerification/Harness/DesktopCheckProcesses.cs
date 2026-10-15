using System.Diagnostics;

namespace SuperMetroid.DesktopVerification.Harness;

/// <summary>Runs named desktop checks at the same time, each in its own child verifier process.</summary>
internal static class DesktopCheckProcesses
{
    /// <summary>Child-process flag that runs one named check.</summary>
    internal const string CheckFlag = "--check";

    /// <summary>
    /// Starts one <c>--check NAME</c> child per name, waits for every child, and prints each child's
    /// output as one block in the given order. Throws naming every check whose process failed.
    /// </summary>
    internal static void RunAll(IEnumerable<string> names)
    {
        var children = names.Select(Start).ToArray();
        var failed = new List<string>();
        foreach (var child in children)
        {
            string output = child.Output.GetAwaiter().GetResult();
            string error = child.Error.GetAwaiter().GetResult();
            child.Process.WaitForExit();
            Console.Out.Write(output);
            Console.Error.Write(error);
            int exitCode = child.Process.ExitCode;
            Console.WriteLine($"TIME check {child.Name}: {(child.Process.ExitTime - child.Process.StartTime).TotalSeconds:0.00} s, exit {exitCode}");
            if (exitCode != 0) failed.Add(child.Name);
            child.Process.Dispose();
        }
        if (failed.Count != 0)
            throw new InvalidOperationException($"{failed.Count} desktop check(s) failed: {string.Join(", ", failed)}.");
    }

    /// <summary>Launches this verifier with <c>--check NAME</c> and begins reading both of its streams.</summary>
    private static (string Name, Process Process, Task<string> Output, Task<string> Error) Start(string name)
    {
        string host = Environment.ProcessPath ?? throw new InvalidOperationException("The desktop verifier's host executable path is unknown.");
        var start = new ProcessStartInfo(host)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = Environment.CurrentDirectory,
        };
        // Run through dotnet: pass the verifier assembly; through the apphost: the executable is the verifier.
        if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
            start.ArgumentList.Add(typeof(DesktopCheckProcesses).Assembly.Location);
        start.ArgumentList.Add(CheckFlag);
        start.ArgumentList.Add(name);
        Process process = Process.Start(start)
            ?? throw new InvalidOperationException($"Could not start desktop check {name}.");
        return (name, process, process.StandardOutput.ReadToEndAsync(), process.StandardError.ReadToEndAsync());
    }
}
